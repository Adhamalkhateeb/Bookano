namespace Bookano.Infrastructure.Persistence.Configurations
{
    internal class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FullName).IsRequired().HasMaxLength(200);

            builder.Property(x => x.IsDeleted).HasDefaultValue(false);

            builder
                .Property(x => x.CreatedOnUtc)
                .HasDefaultValueSql("SYSUTCDATETIME()")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.CreatedById).HasMaxLength(450);

            builder.Property(x => x.LastUpdatedById).HasMaxLength(450);

            builder.HasIndex(x => x.Email);

            builder.HasIndex(x => x.UserName);
        }
    }
}
