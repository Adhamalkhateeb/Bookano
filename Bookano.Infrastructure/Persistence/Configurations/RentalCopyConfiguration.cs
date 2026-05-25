using Bookano.Domain.Common.Enums;

namespace Bookano.Infrastructure.Persistence.Configurations
{
    internal class RentalCopyConfiguration : IEntityTypeConfiguration<RentalCopy>
    {
        public void Configure(EntityTypeBuilder<RentalCopy> builder)
        {
            builder.HasKey(x => new { x.RentalId, x.BookCopyId });

            builder
                .HasOne(x => x.BookCopy)
                .WithMany(x => x.Rentals)
                .HasForeignKey(x => x.BookCopyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .Property(x => x.RentalDate)
                .IsRequired()
                .HasColumnType("date")
                .HasDefaultValueSql("CAST(GETUTCDATE() AS date)")
                .ValueGeneratedOnAdd();
            ;

            builder.Property(x => x.EndDate).IsRequired().HasColumnType("date");

            builder.Property(x => x.ReturnDate).HasColumnType("date");
            builder.Property(x => x.ExtendedOn).HasColumnType("date");

            builder.HasQueryFilter(e => !e.Rental!.IsDeleted);

            builder.HasIndex(x => x.RentalId);
            builder.HasIndex(x => x.BookCopyId);
            builder.HasIndex(x => x.ReturnDate);
        }
    }
}
