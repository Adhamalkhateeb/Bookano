namespace Bookano.Infrastructure.Persistence.Configurations
{
    internal class BookCopyConfiguration : IEntityTypeConfiguration<BookCopy>
    {
        public void Configure(EntityTypeBuilder<BookCopy> builder)
        {
            builder.HasKey(x => x.Id);

            BaseEntityConfiguration.ConfigureBase(builder);

            builder.Property(x => x.EditionNumber).IsRequired();

            builder
                .Property(x => x.SerialNumber)
                .HasDefaultValueSql("NEXT VALUE FOR Shared.SerialNumber");

            builder.Property(x => x.IsAvailableForRental).HasDefaultValue(true);

            builder.HasIndex(x => new { x.BookId, x.SerialNumber }).IsUnique();
        }
    }
}
