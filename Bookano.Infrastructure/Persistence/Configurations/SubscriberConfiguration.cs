namespace Bookano.Infrastructure.Persistence.Configurations
{
    internal class SubscriberConfiguration : IEntityTypeConfiguration<Subscriber>
    {
        public void Configure(EntityTypeBuilder<Subscriber> builder)
        {
            builder.HasKey(x => x.Id);

            BaseEntityConfiguration.ConfigureBase(builder);

            builder.Property(x => x.FirstName).IsRequired().HasMaxLength(50);

            builder.Property(x => x.LastName).IsRequired().HasMaxLength(50);

            builder.Property(x => x.DateOfBirth).IsRequired();

            builder.Property(x => x.NationalId).IsRequired().HasMaxLength(20);

            builder.Property(x => x.MobileNumber).IsRequired().HasMaxLength(15);

            builder.Property(x => x.HasWhatsApp).IsRequired().HasDefaultValue(false);

            builder.Property(x => x.Email).IsRequired().HasMaxLength(150);

            builder.Property(x => x.ImageUrl).IsRequired().HasMaxLength(1000);

            builder.Property(x => x.ImageThumbnailUrl).IsRequired().HasMaxLength(1000);

            builder.Property(x => x.ImagePublicId).IsRequired();

            builder.Property(x => x.Address).IsRequired().HasMaxLength(500);

            builder.Property(x => x.IsBlackListed).IsRequired().HasDefaultValue(false);

            builder
                .HasOne(x => x.Area)
                .WithMany()
                .HasForeignKey(x => x.AreaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.Governorate)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict)
                .HasForeignKey(x => x.GovernorateId);

            builder.HasIndex(x => x.NationalId).IsUnique();
            builder.HasIndex(x => x.MobileNumber).IsUnique();
            builder.HasIndex(x => x.Email).IsUnique();
        }
    }
}
