namespace Authentication.Infrastructure.Persistence.Entity;

public sealed class HubIdentityRole : IdentityRole<Guid>
{
    public HubIdentityRole()
        : base() {}

    public HubIdentityRole(string roleName)
        : base(roleName) {}
    
    public HubIdentityRole(string roleName, string roleDescription)
        : base(roleName)
    {
        Description = roleDescription;
    }
    
    public string Description { get; init; } = string.Empty;

    public ICollection<IdentityRoleClaim<string>> RoleClaims { get; init; } = [];
}
