
namespace Bookano.Web.Core.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public sealed class Author
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = null!;
        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public DateTimeOffset? LastUpdatedOnUtc { get; set; }
    }
}
