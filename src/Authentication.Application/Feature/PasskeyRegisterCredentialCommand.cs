namespace Authentication.Application.Feature;

public sealed class PasskeyRegisterCredentialCommand
{
    public required string UserId { get; init; }
    public required AuthenticatorAttestationRawResponse AttestationRawResponse { get; init; }
}

internal sealed partial class PasskeyRegisterCredentialCommandHandler(
    IMemoryCache memoryCache,
    IPasskeyManager passkeyManager,
    ILogger<PasskeyRegisterCredentialCommandHandler> logger)
{
    public async Task<IResponse> HandleAsync(
        PasskeyRegisterCredentialCommand command,
        CancellationToken cancellationToken)
    {
        var originalOptions = memoryCache.Get<CredentialCreateOptions>(HubCacheKeys.Passkey.CredentialCreateOptions + command.UserId);

        if (originalOptions is null)
        {
            LogFailedToRetrieveRegistrationOptions(command.UserId);
            return Response.Fail("Registration failed");
        }
        
        var isRegistrationSuccess = await passkeyManager.RegisterCredentialAsync(
            command.UserId,
            originalOptions,
            command.AttestationRawResponse,
            cancellationToken);

        return isRegistrationSuccess
            ? Response.Success("Registration successful")
            : Response.Fail("Registration failed");
    }

    [LoggerMessage(LogLevel.Warning, "Failed to retrieve registration options for user with id {UserId}.")]
    partial void LogFailedToRetrieveRegistrationOptions(string userId);
}