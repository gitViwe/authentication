namespace Authentication.Application.Feature;

public sealed class RegisterUserCommand: RegisterRequest, IRequiresHost
{
    public required string Origin { get; init; }
}

internal sealed class RegisterUserCommandHandler(ITokenManager tokenManager, IUserIdentityManager userIdentityManager)
{
    public async Task<ITypedResponse<TokenResponse>> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var claimsPrincipal = await userIdentityManager.RegisterAsync(command, cancellationToken);

        if (claimsPrincipal is null)
        {
            return TypedResponse<TokenResponse>.Fail("Failed to Register User.", StatusCodes.Status401Unauthorized);
        }

        var tokenResponse = await tokenManager.CreateTokenAsync(claimsPrincipal, command.Origin, cancellationToken);

        return TypedResponse<TokenResponse>.Success("User Registered.", tokenResponse);
    }
}