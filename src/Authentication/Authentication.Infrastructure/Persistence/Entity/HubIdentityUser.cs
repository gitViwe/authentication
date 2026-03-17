namespace Authentication.Infrastructure.Persistence.Entity;

public sealed class HubIdentityUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string TimeBasedOneTimePinKey { get; set; } = string.Empty;
    public string ProfileImageUri { get; set; } = string.Empty;
    public DateTime? ProfileImageExpiry { get; set; }

    public ICollection<HubIdentityRole> Roles { get; init; } = [];
    public ICollection<IdentityUserClaim<Guid>> Claims { get; init; } = [];
    public ICollection<IdentityUserLogin<Guid>> Logins { get; init; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; init; } = [];
}
