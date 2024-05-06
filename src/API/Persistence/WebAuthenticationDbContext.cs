namespace API.Persistence;

internal class WebAuthenticationDbContext : DbContext
{
    public WebAuthenticationDbContext(DbContextOptions<WebAuthenticationDbContext> options)
        : base(options)
    {
        UserDetails = Set<UserDetail>();
        KanBanSections = Set<KanBanSection>();
        KanBanTaskItems = Set<KanBanTaskItem>();
        UserDetailHandles = Set<UserDetailHandle>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserDetail>()
            .HasMany(section => section.KanBanSections)
            .WithOne(item => item.UserDetail)
            .HasForeignKey(handle => handle.UserId);

        modelBuilder.Entity<UserDetail>()
            .HasMany(section => section.UserDetailHandles)
            .WithOne(handle => handle.UserDetail)
            .HasForeignKey(handle => handle.UserId);

        modelBuilder.Entity<KanBanSection>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<KanBanSection>()
            .HasMany(section => section.KanBanTaskItems)
            .WithOne(item => item.KanBanSection);

        modelBuilder.Entity<KanBanTaskItem>()
            .HasKey(x => x.Id);
    }

    public DbSet<UserDetail> UserDetails { get; set; }
    public DbSet<KanBanSection> KanBanSections { get; set; }
    public DbSet<KanBanTaskItem> KanBanTaskItems { get; set; }
    public DbSet<UserDetailHandle> UserDetailHandles { get; set; }
}
