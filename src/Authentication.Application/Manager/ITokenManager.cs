namespace Authentication.Application.Manager;

public interface ITokenManager
{
    Task<TokenResponse> CreateTokenAsync(ClaimsPrincipal principal, string origin, CancellationToken cancellation);
    Task RevokeRefreshTokenAsync(string jwtId, CancellationToken cancellation);
    Task<string> ValidateRefreshTokenAsync(TokenRequest request, CancellationToken cancellation);
}