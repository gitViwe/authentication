namespace API;

public class WebAuthentication
{
    public WebAuthentication(
        IServiceProvider provider,
        IRegistrationCeremonyService registrationCeremonyService,
        IAuthenticationCeremonyService authenticationCeremonyService)
    {
        _registrationCeremonyService = registrationCeremonyService;
        _authenticationCeremonyService = authenticationCeremonyService;

        using var scope = provider.CreateAsyncScope();
        _cache = scope.ServiceProvider.GetRequiredService<IRedisDistributedCache>();
        _configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    }

    private readonly IRedisDistributedCache _cache;
    private readonly IConfiguration _configuration;
    private readonly IRegistrationCeremonyService _registrationCeremonyService;
    private readonly IAuthenticationCeremonyService _authenticationCeremonyService;
    private const string WEBAUTHN_REGISTRATION_HEADER = "X-WebAuthn-Registration-Id";
    private const string WEBAUTHN_AUTHENTICATION_HEADER = "X-WebAuthn-Authentication-Id";

    public async Task<PublicKeyCredentialCreationOptionsJSON> GetRegistrationOptionsAsync(HttpContext context, byte[] userIdBytes, string userName = "User Display Name")
    {
        string[] origins = [.. _configuration["AllowedOrigins"]!.Split(';'), $"http://{context.Request.Host}", $"https://{context.Request.Host}"];

        var result = await _registrationCeremonyService.BeginCeremonyAsync(
            httpContext: context,
            request: new BeginRegistrationCeremonyRequest(
                origins: new RegistrationCeremonyOriginParameters(allowedOrigins: origins),
                topOrigins: null,
                rpDisplayName: "Passkeys demonstration",
                user: new PublicKeyCredentialUserEntity(
                    name: userName,
                    id: userIdBytes,
                    displayName: $"{context.Request.Scheme}://{context.Request.Host} [{userName}]"),
                challengeSize: 32,
                pubKeyCredParams:
                [
                    CoseAlgorithm.ES256,
                    CoseAlgorithm.ES384,
                    CoseAlgorithm.ES512,
                    CoseAlgorithm.RS256,
                    CoseAlgorithm.RS384,
                    CoseAlgorithm.RS512,
                    CoseAlgorithm.PS256,
                    CoseAlgorithm.PS384,
                    CoseAlgorithm.PS512,
                    CoseAlgorithm.EdDSA
                ],
                timeout: 300_000,
                excludeCredentials: RegistrationCeremonyExcludeCredentials.AllExisting(),
                authenticatorSelection: new AuthenticatorSelectionCriteria(
                    authenticatorAttachment: null,
                    residentKey: ResidentKeyRequirement.Required,
                    requireResidentKey: true,
                    userVerification: UserVerificationRequirement.Required),
                hints: null,
                attestation: null,
                attestationFormats: null,
                extensions: null),
            cancellationToken: CancellationToken.None);

        string cacheKey = context.Request.Headers[WEBAUTHN_REGISTRATION_HEADER]!;
        ArgumentException.ThrowIfNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        _cache.Set(cacheKey, result.RegistrationCeremonyId);

        return result.Options;
    }

    public async Task<byte[]> CompleteRegistrationAsync(HttpContext context, RegistrationResponseJSON responseJSON)
    {
        string cacheKey = context.Request.Headers[WEBAUTHN_REGISTRATION_HEADER]!;
        ArgumentException.ThrowIfNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        string? registrationCeremonyId = await _cache.GetAsync(cacheKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(registrationCeremonyId, nameof(registrationCeremonyId));

        var result = await _registrationCeremonyService.CompleteCeremonyAsync(
            httpContext: context,
            request: new CompleteRegistrationCeremonyRequest(
                registrationCeremonyId: registrationCeremonyId.Replace("\"", string.Empty),
                description: "Web Authentication Demonstration",
                response: responseJSON),
            cancellationToken: CancellationToken.None);

        ArgumentNullException.ThrowIfNull(result, "CompleteRegistrationCeremonyResult");

        return result.HasError ? [] : result.Ok.UserHandle;
    }

    public async Task<PublicKeyCredentialRequestOptionsJSON> GetAuthenticationOptionsAsync(HttpContext context)
    {
        string[] origins = [.. _configuration["AllowedOrigins"]!.Split(';'), $"http://{context.Request.Host}", $"https://{context.Request.Host}"];

        var result = await _authenticationCeremonyService.BeginCeremonyAsync(
            httpContext: context,
            request: new BeginAuthenticationCeremonyRequest(
                origins: new AuthenticationCeremonyOriginParameters(allowedOrigins: origins),
                topOrigins: null,
                userHandle: null,
                challengeSize: 32,
                timeout: 300_000,
                allowCredentials: AuthenticationCeremonyIncludeCredentials.AllExisting(),
                userVerification: UserVerificationRequirement.Required,
                hints: null,
                attestation: null,
                attestationFormats: null,
                extensions: null),
            cancellationToken: CancellationToken.None);

        string cacheKey = context.Request.Headers[WEBAUTHN_AUTHENTICATION_HEADER]!;
        ArgumentException.ThrowIfNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        _cache.Set(cacheKey, result.AuthenticationCeremonyId);

        return result.Options;
    }

    public async Task<byte[]> CompleteAuthenticationAsync(HttpContext context, AuthenticationResponseJSON responseJSON)
    {
        string cacheKey = context.Request.Headers[WEBAUTHN_AUTHENTICATION_HEADER]!;
        ArgumentException.ThrowIfNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        string? authenticationCeremonyId = await _cache.GetAsync(cacheKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(authenticationCeremonyId, nameof(authenticationCeremonyId));

        var result = await _authenticationCeremonyService.CompleteCeremonyAsync(
            httpContext: context,
            request: new CompleteAuthenticationCeremonyRequest(
                authenticationCeremonyId: authenticationCeremonyId.Replace("\"", string.Empty),
                response: responseJSON),
            cancellationToken: CancellationToken.None);

        ArgumentNullException.ThrowIfNull(result, "CompleteAuthenticationCeremonyResult");

        return result.HasError ? [] : result.Ok.UserHandle;
    }
}
