
namespace Bookano.Web.Core.Models
{
    public sealed class Category
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = null!;
        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public DateTimeOffset? UpdatedOnUtc { get; set; }
    }
}
