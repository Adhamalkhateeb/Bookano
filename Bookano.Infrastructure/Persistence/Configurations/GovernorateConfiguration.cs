namespace Bookano.Infrastructure.Persistence.Configurations
{
    internal class GovernorateConfiguration : IEntityTypeConfiguration<Governorate>
    {
        public void Configure(EntityTypeBuilder<Governorate> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(255);

            builder.HasIndex(x => x.Name).IsUnique();
        }
    }
}
