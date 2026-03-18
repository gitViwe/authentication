namespace Authentication.Application.Feature;

public sealed class UserDetailUpdateCommand : UpdateUserRequest
{
    public required string UserId { get; init; }
}

internal sealed class UserDetailUpdateCommandHandler(IUserIdentityManager userIdentityManager)
{
    public async Task<IResponse> HandleAsync(UserDetailUpdateCommand command, CancellationToken cancellationToken)
    {
        if (false == await userIdentityManager.UpdateUserAsync(command.UserId, command))
        {
            return Response.Fail("Failed to Update User details.");
        }

        return Response.Success("User details updated.");
    }
}