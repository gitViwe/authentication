namespace Authentication.Application.Feature;

public sealed class LoginUserCommand : LoginRequest, IRequiresHost
{
    public required string Origin { get; init; }
}

internal sealed class LoginUserCommandHandler(ITokenManager tokenManager, IUserIdentityManager userIdentityManager)
{
    public async Task<ITypedResponse<TokenResponse>> HandleAsync(LoginUserCommand command, CancellationToken cancellationToken)
    {
        var claimsPrincipal = await userIdentityManager.LoginUserAsync(command, cancellationToken);

        if (claimsPrincipal is null)
        {
            return TypedResponse<TokenResponse>.Fail("Failed to Login User.", StatusCodes.Status401Unauthorized);
        }

        var tokenResponse = await tokenManager.CreateTokenAsync(claimsPrincipal, command.Origin, cancellationToken);

        return TypedResponse<TokenResponse>.Success("User Logged in.", tokenResponse);
    }
}