namespace Bookano.Infrastructure.Persistence.Configurations
{
    internal class BookConfiguration : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Isbn).HasMaxLength(20).IsUnicode(false);
            builder.HasIndex(b => b.Isbn).IsUnique().HasFilter("[Isbn] IS NOT NULL");

            builder.Property(x => x.Title).IsRequired().HasMaxLength(200);

            builder.Property(x => x.Hall).IsRequired().HasMaxLength(50);

            builder.Property(x => x.Description).IsRequired().HasColumnType("nvarchar(max)");

            builder.Property(x => x.PublishingDate).HasColumnType("date");

            builder.Property(x => x.ImageUrl).HasMaxLength(1000);
            builder.Property(x => x.ImageThumbnailUrl).HasMaxLength(1000);
            builder.Property(x => x.ImagePublicId).HasMaxLength(255);

            builder.Property(x => x.IdempotencyKey).HasMaxLength(100).IsUnicode(false);
            builder
                .HasIndex(x => x.IdempotencyKey)
                .IsUnique()
                .HasFilter("[IdempotencyKey] IS NOT NULL");

            builder.Property(x => x.IsAvailableForRental).HasDefaultValue(false);

            builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();

            builder
                .HasOne(x => x.Publisher)
                .WithMany(x => x.Books)
                .HasForeignKey(x => x.PublisherId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(x => x.Copies)
                .WithOne(x => x.Book)
                .HasForeignKey(x => x.BookId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
