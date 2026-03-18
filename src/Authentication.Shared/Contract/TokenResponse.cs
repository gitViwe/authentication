namespace Authentication.Shared.Contract;

public sealed class TokenResponse
{
    public required string Token { get; init; }
    public required string RefreshToken { get; init; }
}