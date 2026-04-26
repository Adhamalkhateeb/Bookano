namespace Bookano.Web.Core.Models
{
    public class BaseModel
    {
        public bool IsDeleted { get; set; }
        public string? CreatedById { get; set; }
        public ApplicationUser? CreatedBy { get; set; }
        public DateTimeOffset CreatedOnUtc { get; set; } = DateTimeOffset.UtcNow;
        public string? LastUpdatedById { get; set; }
        public ApplicationUser? LastUpdatedBy { get; set; }
        public DateTimeOffset? LastUpdatedOnUtc { get; set; }
    }
}
