namespace Authentication.Application.Feature;

public sealed class TotpLoginCommand : TimeBasedOneTimePinLoginRequest, IRequiresHost
{
    public required string Origin { get; init; }
}

internal sealed class TotpLoginCommandHandler(ITokenManager tokenManager, IUserIdentityManager userIdentityManager)
{
    public async Task<ITypedResponse<TokenResponse>> HandleAsync(TotpLoginCommand command, CancellationToken cancellationToken)
    {
        var claimsPrincipal = await userIdentityManager.LoginUserAsync(command, cancellationToken);
        
        if (claimsPrincipal is null)
        {
            return TypedResponse<TokenResponse>.Fail("Invalid login attempt or 2FA token.", StatusCodes.Status401Unauthorized);
        }

        var tokenResponse = await tokenManager.CreateTokenAsync(claimsPrincipal, command.Origin, cancellationToken);
        return TypedResponse<TokenResponse>.Success("User Logged in.", tokenResponse);
    }
}