namespace Authentication.Application.Feature;

public sealed class PasskeyVerifyAssertionCommand: IRequiresHost
{
    public required string Email { get; init; }
    public required string Origin { get; init; }
    public required AuthenticatorAssertionRawResponse AssertionRawResponse { get; init; }
}

internal sealed partial class PasskeyVerifyAssertionCommandHandler(
    IMemoryCache memoryCache,
    ITokenManager tokenManager,
    IPasskeyManager passkeyManager,
    IUserIdentityManager userIdentityManager,
    ILogger<PasskeyVerifyAssertionCommandHandler> logger)
{
    public async Task<ITypedResponse<TokenResponse>> HandleAsync(
        PasskeyVerifyAssertionCommand command,
        CancellationToken cancellationToken)
    {
        var originalOptions = memoryCache.Get<AssertionOptions>(HubCacheKeys.Passkey.AssertionOptions + command.Email);

        if (originalOptions is null)
        {
            LogFailedToRetrieveAssertionOptions(command.Email);
            return TypedResponse<TokenResponse>.Fail("Invalid login attempt or passkey.", StatusCodes.Status401Unauthorized);
        }
        
        var userId = await passkeyManager.VerifyAssertionAsync(
            originalOptions,
            command.AssertionRawResponse,
            cancellationToken);

        if (userId is null)
        {
            return TypedResponse<TokenResponse>.Fail("Invalid login attempt or passkey.", StatusCodes.Status401Unauthorized);
        }

        var claimsPrincipal = await userIdentityManager.CreateClaimsPrincipalAsync(userId.ToString()!, cancellationToken);
        
        if (claimsPrincipal is null)
        {
            return TypedResponse<TokenResponse>.Fail("Invalid login attempt or passkey.", StatusCodes.Status401Unauthorized);
        }

        var tokenResponse = await tokenManager.CreateTokenAsync(claimsPrincipal, command.Origin, cancellationToken);
        return TypedResponse<TokenResponse>.Success("User Logged in.", tokenResponse);
    }

    [LoggerMessage(LogLevel.Warning, "Failed to retrieve assertion options for user with email {Email}.")]
    partial void LogFailedToRetrieveAssertionOptions(string email);
}