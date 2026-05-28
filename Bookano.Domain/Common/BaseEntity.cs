using Bookano.Domain.Entities;

namespace Bookano.Domain.Common
{
    public abstract class BaseEntity : IAuditable
    {
        public bool IsDeleted { get; set; }
        public string? CreatedById { get; set; }
        public ApplicationUser? CreatedBy { get; set; }
        public DateTimeOffset CreatedOnUtc { get; set; }
        public string? LastUpdatedById { get; set; }
        public ApplicationUser? LastUpdatedBy { get; set; }
        public DateTimeOffset? LastUpdatedOnUtc { get; set; }
    }
}
