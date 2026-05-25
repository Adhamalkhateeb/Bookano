namespace Bookano.Infrastructure.Persistence.Configurations
{
    internal class PublisherConfiguration : IEntityTypeConfiguration<Publisher>
    {
        public void Configure(EntityTypeBuilder<Publisher> builder)
        {
            builder.HasKey(x => x.Id);

            BaseEntityConfiguration.ConfigureBase(builder);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);

            builder.HasIndex(x => x.Name).IsUnique();
        }
    }
}
