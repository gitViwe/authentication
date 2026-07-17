# Authentication API

A containerized **.NET 10 Web API** for authentication and account management, backed by **ASP.NET Core Identity**, **Entity Framework Core**, and **PostgreSQL**.

The application supports password-based authentication, JWT access and refresh tokens, TOTP, passkeys/WebAuthn, API-key authentication, account detail management, OpenAPI/Scalar documentation, health checks, and OpenTelemetry-based observability.

## Current stack

- **Runtime:** .NET 10 / ASP.NET Core minimal APIs
- **Persistence:** PostgreSQL 16.3 with Entity Framework Core migrations
- **Authentication:** ASP.NET Core Identity, JWT bearer tokens, refresh tokens, TOTP, passkeys, and API keys
- **API documentation:** OpenAPI with Scalar
- **Telemetry:** OpenTelemetry Collector
- **Logs:** Loki
- **Traces:** Tempo
- **Metrics:** Prometheus
- **Dashboards:** Grafana with a provisioned ASP.NET Core endpoint dashboard
- **Testing:** xUnit integration tests, `WebApplicationFactory`, and Testcontainers for PostgreSQL
- **Delivery:** GitHub Actions, GitHub Container Registry, and Azure Web App deployment

## Repository structure

```text
.
├── .github/workflows/              # Build, image publishing, and Azure deployment
├── config/
│   ├── grafana/                    # Provisioned datasources and dashboards
│   ├── loki/                       # Log storage configuration
│   ├── otel-collector/             # OTLP receivers and telemetry pipelines
│   ├── prometheus/                 # Metrics scraping configuration
│   └── tempo/                      # Trace storage and derived metrics
├── scripts/
│   └── telemetry-smoke-test.js     # k6 authentication and telemetry smoke test
├── src/
│   ├── Authentication/             # API host, configuration, OpenAPI, and Dockerfile
│   ├── Authentication.Application/ # Endpoints and application features
│   ├── Authentication.Infrastructure/ # Identity, persistence, auth, and telemetry
│   ├── Authentication.Shared/      # Contracts, constants, and compliance types
│   └── Authentication.Test/        # Integration tests and PostgreSQL Testcontainer
├── docker-compose.yml
└── README.md
```

## Authentication capabilities

The codebase currently contains support for:

- User registration and password login
- JWT access tokens and refresh-token validation/revocation
- Configurable issuer and audience validation
- Login-flow discovery (`password`, `totp`, and `passkey`)
- TOTP setup, verification, and login
- Passkey registration and assertion verification
- Passkey metadata such as friendly name, registration time, and last-used time
- Authenticated user detail retrieval and update
- API-key authentication using configurable API-key and origin-service headers
- CORS and passkey-origin allowlists
- Sensitive-data classification and erasing redaction

> Endpoint contracts and request/response schemas are exposed through Scalar after the API starts.

## Prerequisites

For the full local stack:

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) with Docker Compose

For local development outside containers:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A reachable PostgreSQL instance

Optional:

- [k6](https://grafana.com/docs/k6/latest/set-up/install-k6/) for the telemetry smoke test
- `dotnet-ef` for creating migrations

## Configuration and secrets

The checked-in configuration intentionally leaves secrets and production-specific values empty. Before starting the stack, provide values for the substitutions referenced by `docker-compose.yml`, including:

- PostgreSQL user, password, and database
- JWT signing secret
- Token-validation signing secret
- API-key value

The API also supports Azure App Configuration through the `AZURE_APP_CONFIGURATION_CONNECTION_STRING` environment variable. The application-level configuration sections are:

- `ApiKeyAuthenticationOption`
- `CorsPolicyOption`
- `DatabaseConfigurationOption`
- `JsonWebTokenOption`
- `OpenTelemetryConfigurationOption`
- `PassKeyAuthenticationOption`
- `TimeBasedOneTimePasswordOption`
- `TokenValidationParameterOption`

Nested settings can be supplied as environment variables using double underscores. Arrays use indexed keys, for example:

```dotenv
JsonWebTokenOption__ValidAudiences__0=http://localhost:5109
JsonWebTokenOption__ValidAudiences__1=http://localhost:5056
TokenValidationParameterOption__ValidAudiences__0=http://localhost:5109
```

Do not commit real secrets. Use local environment variables, an untracked `.env` file, Azure App Configuration, or the hosting platform's secret settings.


### Execution Steps
1.  Open your terminal in the root directory of the repository.
2.  Spin up the infrastructure:
    ```bash
    docker-compose up -d --build
    ```
3.  **Note:** Wait for the `postgres` health check to pass. The `auth-api` is configured to restart automatically until the database becomes available.


## Service Map & Endpoints

| Service               | Public URL                     | Description                                          |
|:----------------------|:-------------------------------|:-----------------------------------------------------|
| **Auth API**          | `http://localhost:5056`        | Backend API                                          |
| **API Documentation** | `http://localhost:5056/scalar` | Scalar Docs                                          |
| **Health check**      | `http://localhost:5056/health` | API health endpoint                                  |
| **Grafana**           | `http://localhost:3000`        | Dashboards (`admin` / `admin` for local development) |

---

## 📊 Observability Flow

```text
Authentication API
       │ OTLP (gRPC/HTTP)
       ▼
OpenTelemetry Collector
       ├── logs ───► Loki ──────────┐
       ├── traces ─► Tempo ─────────┤
       └── metrics ► Prometheus ────┤
                                    ▼
                                  Grafana
```

The collector adds `deployment.environment.name=docker`, batches telemetry, and exposes application metrics for Prometheus. Tempo generates service-graph and span metrics and writes them to Prometheus. Grafana is provisioned with Prometheus, Loki, and Tempo datasources plus an ASP.NET Core endpoint-monitoring dashboard.

## Run integration tests

The integration tests start an isolated PostgreSQL 16.3 container with Testcontainers and host the API through a specialized `WebApplicationFactory`. Docker must be running.

From the repository root:

```bash
dotnet test src/Authentication.Test/Authentication.Test.csproj
```

Or from the solution directory:

```bash
cd src
dotnet test Authentication.sln
```

The current test suite covers login/registration fallback, authenticated user-detail retrieval, and user-detail updates.

## Run the telemetry smoke test

With the stack running and k6 installed:

```bash
k6 run scripts/telemetry-smoke-test.js
```

Useful overrides:

```bash
BASE_URL=http://localhost:5056 \
ORIGIN=http://localhost:5109 \
EXPECTED_ISSUERS=auth-api,http://localhost:5056 \
EXPECTED_AUDIENCES=http://localhost:5109,http://localhost:5056 \
VUS=3 \
DURATION=45s \
k6 run scripts/telemetry-smoke-test.js
```

To reuse an existing account:

```bash
TEST_EMAIL=user@example.test \
TEST_PASSWORD='your-password' \
k6 run scripts/telemetry-smoke-test.js
```

The script exercises CORS preflight, registration or login, authenticated requests, expected unauthorized traffic, and JWT issuer/audience checks. It writes `telemetry-summary.json` in the current directory.

## CI/CD pipeline

The workflows are chained in this order:

```text
Build and Test
      │ successful run on main
      ▼
Docker Publish
      │ successful run on main
      ▼
Deploy to Azure Web App
```

- **Build and Test** runs for pull requests and pushes targeting `main`, and also declares a manual trigger.
- **Docker Publish** runs only after a successful `Build and Test` workflow on `main` and publishes to `ghcr.io/<owner>/<repository>`.
- **Deploy to Azure Web App** runs only after a successful `Docker Publish` workflow on `main`.


## Development scripts

Entity framework migrations
```bash
dotnet ef migrations add <migration-name> --project Authentication.Infrastructure --startup-project Authentication
```
Combine files tracked by Git
```bash
git ls-files | while read -r file; do echo "--- $file ---"; cat "$file"; echo -e "\n"; done > combined_files.txt
```