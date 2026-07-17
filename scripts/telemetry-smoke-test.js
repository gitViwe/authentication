import http from 'k6/http';
import { check, group, sleep } from 'k6';
import encoding from 'k6/encoding';
import exec from 'k6/execution';

/*
  Authentication API telemetry generator

  Purpose:
    - Generate successful, validation-failure, unauthorized, and CORS traffic.
    - Exercise logs, traces, and HTTP metrics through normal API requests.
    - Verify that issued JWTs use an expected issuer and audience.

  Run:
    k6 run telemetry-smoke-test.js

  Useful overrides:
    BASE_URL=http://localhost:5056 \
    ORIGIN=http://localhost:5109 \
    EXPECTED_ISSUERS=auth-api,http://localhost:5056 \
    EXPECTED_AUDIENCES=http://localhost:5109,http://localhost:5056 \
    VUS=3 DURATION=45s \
    k6 run telemetry-smoke-test.js

  To reuse an existing account instead of registering a generated user:
    TEST_EMAIL=user@example.test TEST_PASSWORD='your-password' \
    k6 run telemetry-smoke-test.js
*/

const BASE_URL = (__ENV.BASE_URL || 'http://localhost:5056').replace(/\/$/, '');
const ORIGIN = __ENV.ORIGIN || 'http://localhost:5109';
const EXPECTED_ISSUERS = csv(__ENV.EXPECTED_ISSUERS || 'auth-api,http://localhost:5056');
const EXPECTED_AUDIENCES = csv(
  __ENV.EXPECTED_AUDIENCES || 'http://localhost:5109,http://localhost:5056'
);
const PASSWORD = __ENV.TEST_PASSWORD || 'Telemetry!12345';
const VUS = Number.parseInt(__ENV.VUS || '2', 10);
const DURATION = __ENV.DURATION || '30s';
const PAUSE_SECONDS = Number.parseFloat(__ENV.PAUSE_SECONDS || '0.5');

export const options = {
  vus: VUS,
  duration: DURATION,
  thresholds: {
    http_req_failed: ['rate<0.45'],
    http_req_duration: ['p(95)<3000'],
    checks: ['rate>0.70'],
  },
  tags: {
    test_suite: 'authentication-telemetry',
    configured_origin: ORIGIN,
  },
};

function csv(value) {
  return value
    .split(',')
    .map((item) => item.trim())
    .filter(Boolean);
}

function requestHeaders(token) {
  const headers = {
    'Content-Type': 'application/json',
    Accept: 'application/json',
    Origin: ORIGIN,
    'x-origin-service-name': 'telemetry-generator',
  };

  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  return headers;
}

function params(name, token, extraTags = {}) {
  return {
    headers: requestHeaders(token),
    redirects: 0,
    tags: {
      name,
      telemetry_scenario: name,
      ...extraTags,
    },
  };
}

function safeJson(response) {
  try {
    return response.json();
  } catch (_) {
    return null;
  }
}

function findFirstString(value, wantedKeys) {
  if (!value || typeof value !== 'object') return null;

  for (const [key, child] of Object.entries(value)) {
    if (wantedKeys.includes(key.toLowerCase()) && typeof child === 'string' && child.length > 0) {
      return child;
    }
  }

  for (const child of Object.values(value)) {
    if (child && typeof child === 'object') {
      const found = findFirstString(child, wantedKeys);
      if (found) return found;
    }
  }

  return null;
}

function extractToken(response) {
  return findFirstString(safeJson(response), ['token', 'accesstoken', 'access_token']);
}

function decodeJwtPayload(token) {
  if (!token) return null;
  const parts = token.split('.');
  if (parts.length !== 3) return null;

  try {
    return JSON.parse(encoding.b64decode(parts[1], 'rawurl', 's'));
  } catch (_) {
    return null;
  }
}

function audienceMatches(actualAudience) {
  const actual = Array.isArray(actualAudience) ? actualAudience : [actualAudience];
  return actual.some((audience) => EXPECTED_AUDIENCES.includes(audience));
}

function verifyTokenClaims(token, source) {
  const payload = decodeJwtPayload(token);

  check(payload, {
    [`${source}: JWT can be decoded`]: (value) => value !== null,
    [`${source}: issuer is allowed`]: (value) =>
      value !== null && EXPECTED_ISSUERS.includes(value.iss),
    [`${source}: audience is allowed`]: (value) =>
      value !== null && audienceMatches(value.aud),
    [`${source}: token is not expired`]: (value) =>
      value !== null && typeof value.exp === 'number' && value.exp > Math.floor(Date.now() / 1000),
  });

  if (payload && !EXPECTED_ISSUERS.includes(payload.iss)) {
    console.warn(`${source}: unexpected issuer '${payload.iss}'`);
  }

  if (payload && !audienceMatches(payload.aud)) {
    console.warn(`${source}: unexpected audience '${JSON.stringify(payload.aud)}'`);
  }
}

function login(email, password, scenario = 'login') {
  return http.post(
    `${BASE_URL}/account/login`,
    JSON.stringify({ email, password }),
    params('POST /account/login', null, { flow: scenario })
  );
}

function register(email, userName, password) {
  return http.post(
    `${BASE_URL}/account/register`,
    JSON.stringify({
      userName,
      email,
      password,
      passwordConfirmation: password,
      firstName: 'Telemetry',
      lastName: 'Generator',
    }),
    params('POST /account/register', null, { flow: 'register' })
  );
}

export function setup() {
  const suffix = `${Date.now()}-${Math.floor(Math.random() * 1_000_000)}`;
  const email = __ENV.TEST_EMAIL || `telemetry-${suffix}@example.test`;
  const userName = __ENV.TEST_USERNAME || `telemetry-${suffix}`;

  const preflight = http.options(
    `${BASE_URL}/account/login`,
    null,
    {
      headers: {
        Origin: ORIGIN,
        'Access-Control-Request-Method': 'POST',
        'Access-Control-Request-Headers': 'content-type,authorization,x-origin-service-name',
      },
      tags: {
        name: 'OPTIONS /account/login',
        telemetry_scenario: 'cors-preflight',
      },
    }
  );

  check(preflight, {
    'CORS preflight returns a non-error status': (response) => response.status < 400,
    'CORS allows configured origin': (response) =>
      response.headers['Access-Control-Allow-Origin'] === ORIGIN,
  });

  let authResponse;
  if (__ENV.TEST_EMAIL) {
    authResponse = login(email, PASSWORD, 'setup-existing-user');
    check(authResponse, {
      'setup login succeeds': (response) => response.status === 200,
    });
  } else {
    authResponse = register(email, userName, PASSWORD);
    check(authResponse, {
      'setup registration succeeds': (response) => response.status === 200,
    });

    // Some deployments may create the account without returning the token shape
    // expected by the OpenAPI document. Log in as a safe fallback.
    if (!extractToken(authResponse)) {
      authResponse = login(email, PASSWORD, 'setup-register-fallback-login');
    }
  }

  const token = extractToken(authResponse);
  check(token, {
    'setup obtains bearer token': (value) => typeof value === 'string' && value.length > 0,
  });

  if (token) verifyTokenClaims(token, 'setup');

  return {
    email,
    userName,
    password: PASSWORD,
    token,
  };
}

export default function (data) {
  const token = data.token;
  const iteration = exec.scenario.iterationInTest;

  group('Successful authenticated traffic', () => {
    const loginResponse = login(data.email, data.password, 'iteration-login');
    check(loginResponse, {
      'login returns 200': (response) => response.status === 200,
    });

    const freshToken = extractToken(loginResponse) || token;
    if (freshToken) verifyTokenClaims(freshToken, 'login');

    const loginOptions = http.get(
      `${BASE_URL}/account/login/${encodeURIComponent(data.email)}`,
      params('GET /account/login/{email}', freshToken)
    );
    check(loginOptions, {
      'login options returns 200': (response) => response.status === 200,
    });

    const detail = http.get(
      `${BASE_URL}/account/detail`,
      params('GET /account/detail', freshToken)
    );
    check(detail, {
      'user detail returns 200': (response) => response.status === 200,
    });

    const update = http.put(
      `${BASE_URL}/account/detail`,
      JSON.stringify({
        firstName: 'Telemetry',
        lastName: `Iteration-${iteration}`,
      }),
      params('PUT /account/detail', freshToken)
    );
    check(update, {
      'user detail update returns 204': (response) => response.status === 204,
    });

    const totpSetup = http.get(
      `${BASE_URL}/account/2fa/setup`,
      params('GET /account/2fa/setup', freshToken)
    );
    check(totpSetup, {
      '2FA setup is handled': (response) => [200, 400, 401].includes(response.status),
    });

    const passkeyRegisterOptions = http.get(
      `${BASE_URL}/account/passkey/register`,
      params('GET /account/passkey/register', freshToken)
    );
    check(passkeyRegisterOptions, {
      'passkey registration options are handled': (response) =>
        [200, 400, 401].includes(response.status),
    });

    const passkeyAssertionOptions = http.get(
      `${BASE_URL}/account/passkey/login/${encodeURIComponent(data.email)}`,
      params('GET /account/passkey/login/{email}', freshToken)
    );
    check(passkeyAssertionOptions, {
      'passkey assertion options are handled': (response) =>
        [200, 400, 401].includes(response.status),
    });
  });

  group('Expected client and authentication failures', () => {
    const noToken = http.get(
      `${BASE_URL}/account/detail`,
      params('GET /account/detail unauthorized', null, { expected_status: '401' })
    );
    check(noToken, {
      'missing token returns 401': (response) => response.status === 401,
    });

    const malformedToken = http.get(
      `${BASE_URL}/account/detail`,
      params('GET /account/detail invalid-token', 'not-a-valid-jwt', {
        expected_status: '401',
      })
    );
    check(malformedToken, {
      'invalid token returns 401': (response) => response.status === 401,
    });

    const invalidLogin = http.post(
      `${BASE_URL}/account/login`,
      JSON.stringify({ email: 'not-an-email', password: 'x' }),
      params('POST /account/login validation-error', null, { expected_status: '400' })
    );
    check(invalidLogin, {
      'invalid login returns 400': (response) => response.status === 400,
    });

    const unknownUser = http.get(
      `${BASE_URL}/account/login/${encodeURIComponent(`missing-${iteration}@example.test`)}`,
      params('GET /account/login/{email} unknown-user', token, {
        expected_status: '400-or-404',
      })
    );
    check(unknownUser, {
      'unknown-user request is handled': (response) =>
        [200, 400, 404].includes(response.status),
    });
  });

  sleep(PAUSE_SECONDS);
}

export function handleSummary(data) {
  const summary = {
    generatedAt: new Date().toISOString(),
    baseUrl: BASE_URL,
    origin: ORIGIN,
    expectedIssuers: EXPECTED_ISSUERS,
    expectedAudiences: EXPECTED_AUDIENCES,
    metrics: data.metrics,
    rootGroup: data.root_group,
  };

  return {
    stdout: `\nTelemetry run complete.\n` +
      `Target: ${BASE_URL}\n` +
      `Origin: ${ORIGIN}\n` +
      `Summary: telemetry-summary.json\n`,
    'telemetry-summary.json': JSON.stringify(summary, null, 2),
  };
}
