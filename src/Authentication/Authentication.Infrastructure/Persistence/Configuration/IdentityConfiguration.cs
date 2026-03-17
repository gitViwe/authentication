namespace Authentication.Infrastructure.Persistence.Configuration
{
    internal class IdentityConfiguration :
        IEntityTypeConfiguration<IdentityUserLogin<Guid>>,
        IEntityTypeConfiguration<IdentityUserRole<Guid>>,
        IEntityTypeConfiguration<IdentityUserToken<Guid>>,
        IEntityTypeConfiguration<HubIdentityRole>,
        IEntityTypeConfiguration<HubIdentityUser>,
        IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<IdentityUserLogin<Guid>> builder)
        {
            builder.HasKey(entity => entity.UserId);
        }

        public void Configure(EntityTypeBuilder<IdentityUserRole<Guid>> builder)
        {
            builder.HasKey(entity => new { entity.UserId, entity.RoleId });
        }

        public void Configure(EntityTypeBuilder<IdentityUserToken<Guid>> builder)
        {
            builder.HasKey(entity => entity.UserId);
        }

        public void Configure(EntityTypeBuilder<HubIdentityRole> builder)
        {
            builder.HasKey(entity => entity.Id);
            
            builder.Property(entity => entity.Description)
                .HasMaxLength(128);
        }

        public void Configure(EntityTypeBuilder<HubIdentityUser> builder)
        {
            builder.HasKey(entity => entity.Id);
            
            builder.Property(entity => entity.FirstName)
                .HasMaxLength(128);
            builder.Property(entity => entity.LastName)
                .HasMaxLength(128);
            builder.Property(entity => entity.TimeBasedOneTimePinKey)
                .HasMaxLength(50);
            builder.Property(entity => entity.ProfileImageUri)
                .HasMaxLength(128);
        }

        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(entity => entity.Id);
            
            builder.HasOne(entity => entity.User)
                .WithMany(entity => entity.RefreshTokens)
                .HasForeignKey(entity => entity.UserId);
            
            builder.Property(entity => entity.Token)
                .HasMaxLength(500);
            builder.Property(entity => entity.JwtId)
                .HasMaxLength(128);
        }
    }
}
