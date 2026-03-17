namespace Authentication.Shared.Contract;

public sealed class UserDetailResponse
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}