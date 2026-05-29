using Bookano.Domain.Common;

namespace Bookano.Infrastructure.Persistence.Configurations.Common
{
    public static class BaseEntityConfiguration
    {
        public static void ConfigureBase<TEntity>(EntityTypeBuilder<TEntity> builder)
            where TEntity : BaseEntity
        {
            builder.Property(x => x.IsDeleted).HasDefaultValue(false).ValueGeneratedOnAdd();

            builder
                .Property(x => x.CreatedOnUtc)
                .HasDefaultValueSql("SYSUTCDATETIME()")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.LastUpdatedOnUtc);

            builder.Property(x => x.CreatedById).HasMaxLength(450);
            builder.Property(x => x.LastUpdatedById).HasMaxLength(450);

            builder
                .HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => x.CreatedById)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne(x => x.LastUpdatedBy)
                .WithMany()
                .HasForeignKey(x => x.LastUpdatedById)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
