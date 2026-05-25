namespace Bookano.Infrastructure.Persistence.Configurations
{
    internal class RentalConfiguration : IEntityTypeConfiguration<Rental>
    {
        public void Configure(EntityTypeBuilder<Rental> builder)
        {
            builder.HasKey(x => x.Id);

            BaseEntityConfiguration.ConfigureBase(builder);

            builder.Property(x => x.StartDate).IsRequired().HasColumnType("date");

            builder.Property(x => x.PenaltyPaid).HasDefaultValue(false);

            builder
                .HasOne(x => x.Subscriber)
                .WithMany(x => x.Rentals)
                .HasForeignKey(x => x.SubscriberId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(x => x.RentalCopies)
                .WithOne(x => x.Rental)
                .HasForeignKey(x => x.RentalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.SubscriberId);
            builder.HasIndex(x => x.StartDate);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
