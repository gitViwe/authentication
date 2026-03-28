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
            return TypedResponse<TokenResponse>.Fail(
                "Failed to Login User.",
                StatusCodes.Status401Unauthorized);
        }
        
        var requiresTwoFactor = claimsPrincipal.HasClaim(c => 
            c.Type == "Permission" && 
            c.Value == HubPermissions.Authentication.VerifiedTotp);

        if (requiresTwoFactor)
        {
            // Halt login and return a specific status code for the frontend to catch
            return TypedResponse<TokenResponse>.Fail(
                "Two-factor authentication required.", 
                StatusCodes.Status428PreconditionRequired);
        }

        var tokenResponse = await tokenManager.CreateTokenAsync(claimsPrincipal, command.Origin, cancellationToken);

        return TypedResponse<TokenResponse>.Success("User Logged in.", tokenResponse);
    }
}