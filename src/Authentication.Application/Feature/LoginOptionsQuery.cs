namespace Authentication.Application.Feature;

public sealed class LoginOptionsQuery : IRequiresHost
{
    public required string Email { get; init; }
    public required string Origin { get; init; }
    
}

internal sealed class LoginOptionsQueryHandler(IUserIdentityManager userIdentityManager)
{

    public async Task<ITypedResponse<LoginOptionsResponse>> HandleAsync(
        LoginOptionsQuery query,
        CancellationToken cancellationToken)
    {
        
        var flows = await userIdentityManager.GetAvailableLoginFlowsAsync(
            query.Email,
            cancellationToken);

        return TypedResponse<LoginOptionsResponse>.Success(
            "Login options retrieved.",
            new LoginOptionsResponse { AvailableFlows = flows });

    }
}