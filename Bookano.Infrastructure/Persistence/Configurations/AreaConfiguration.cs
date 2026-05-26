namespace Bookano.Infrastructure.Persistence.Configurations
{
    internal class AreaConfiguration : IEntityTypeConfiguration<Area>
    {
        public void Configure(EntityTypeBuilder<Area> builder)
        {
            builder.HasKey(x => x.Id);

            BaseEntityConfiguration.ConfigureBase(builder);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);

            builder.HasIndex(x => new { x.Name, x.GovernorateId }).IsUnique();

            builder
                .HasOne(x => x.Governorate)
                .WithMany()
                .HasForeignKey(x => x.GovernorateId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
