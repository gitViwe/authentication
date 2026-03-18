namespace Authentication.Infrastructure.Persistence.Entity;

public sealed class RefreshToken
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Token { get; init; } = string.Empty;
    public string JwtId { get; init; } = string.Empty;
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime AddedDate { get; init; }
    public DateTime ExpiryDate { get; init; }
    public HubIdentityUser User { get; init; } = new();
}
