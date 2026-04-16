
namespace Bookano.Web.Core.Models
{
    public class BaseModel
    {
        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public DateTimeOffset? LastUpdatedOnUtc { get; set; }
    }
}
