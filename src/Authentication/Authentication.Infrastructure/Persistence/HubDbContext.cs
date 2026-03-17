namespace Authentication.Infrastructure.Persistence;

/// <summary>
/// The entity Framework Core context class inherits from <see cref="IdentityDbContext"/>
/// </summary>
public sealed class HubDbContext : IdentityDbContext<HubIdentityUser, HubIdentityRole, Guid>
{
    public HubDbContext(DbContextOptions<HubDbContext> options)
        : base(options)
    {
        RefreshTokens = Set<RefreshToken>();
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // call this first for Identity contexts
        base.OnModelCreating(modelBuilder);
        // configure identity relations
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; }
}
