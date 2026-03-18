namespace Authentication.Application.Feature;

public sealed class UserDetailQuery
{
    public required string UserId { get; init; }
}

internal sealed class UserDetailQueryHandler(IUserIdentityManager userIdentityManager)
{
    public async Task<ITypedResponse<UserDetailResponse>> HandleAsync(UserDetailQuery query, CancellationToken cancellationToken)
    {
        var response = await userIdentityManager.GetUserDetailAsync(query.UserId);
        return TypedResponse<UserDetailResponse>.Success("User Details found.", response);
    }
}