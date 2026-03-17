using gitViwe.Shared.JsonWebToken.Option;

namespace Authentication.Infrastructure.Manager;

internal sealed class TokenManager(
    HubDbContext context,
    IJsonWebToken jsonWebToken,
    ILogger<TokenManager> logger,
    IOptionsMonitor<JsonWebTokenOption> options) : ITokenManager
{
    public async Task<TokenResponse> CreateTokenAsync(ClaimsPrincipal principal, string origin, CancellationToken cancellation)
    {
        var jwt = jsonWebToken.CreateJsonWebToken(principal.Claims, origin);

        var refreshToken = CreateRefreshToken(
            principal.FindFirstValue(JwtRegisteredClaimNames.Jti)!,
            Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!),
            options.CurrentValue.RefreshTokenExpiryInSeconds);

        var response = new TokenResponse()
        {
            RefreshToken = refreshToken.Token,
            Token = jwt,
        };

        await context.RefreshTokens.AddAsync(refreshToken, cancellation);
        await context.SaveChangesAsync(cancellation);

        return response;
    }

    public async Task RevokeRefreshTokenAsync(string jwtId, CancellationToken cancellation)
    {
        var storedToken = await context.RefreshTokens.FirstOrDefaultAsync(item => item.JwtId.Equals(jwtId), cancellation);

        if (storedToken is not null)
        {
            storedToken.IsRevoked = true;
            await context.SaveChangesAsync(cancellation);
        }
    }

    public async Task<string> ValidateRefreshTokenAsync(TokenRequest request, CancellationToken cancellation)
    {
        var claimsPrincipal = await jsonWebToken.ValidateTokenAsync(request.Token, isRefreshToken: true);

        if (claimsPrincipal is null)
        {
            logger.FailedToValidateRefreshToken(request);
            return string.Empty;
        }

        var storedToken = await context.RefreshTokens.FirstOrDefaultAsync(item => item.Token.Equals(request.RefreshToken), cancellation);

        if (storedToken is null
            || storedToken.IsUsed
            || storedToken.IsRevoked
            || storedToken.ExpiryDate < DateTime.UtcNow
            || false == storedToken.JwtId.Equals(claimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.Jti)))
        {
            logger.FailedToValidateRefreshToken(request, claimsPrincipal, storedToken);
            return string.Empty;
        }

        storedToken.IsUsed = true;
        await context.SaveChangesAsync(cancellation);

        return claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)!;
    }
    
    private static RefreshToken CreateRefreshToken(string jwtId, Guid userId, int expiryInMinutes)
    {
        return new RefreshToken()
        {
            JwtId = jwtId,
            IsUsed = false,
            IsRevoked = false,
            UserId = userId,
            AddedDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddMinutes(expiryInMinutes),
            Token = Generator.RandomString(CharacterCombination.NumberAndAlphabet, 25)
        };
    }
}