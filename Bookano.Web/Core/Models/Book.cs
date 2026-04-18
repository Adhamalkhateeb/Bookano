using Microsoft.IdentityModel.Tokens;

namespace Bookano.Web.Core.Models
{
    [Index(nameof(Isbn), IsUnique = true)]
    public class Book : BaseModel
    {
        public int Id { get; set; }

        [MaxLength(20)]
        public string? Isbn { get; set; }

        [MaxLength(255)]
        public string Title { get; set; } = null!;
        public int PublisherId { get; set; }
        public Publisher? Publisher { get; set; }
        public DateTime PublishingDate { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImagePublicId { get; set; }
        public string? ImageThumbnailUrl { get; set; }

        [MaxLength(50)]
        public string Hall { get; set; } = null!;

        public bool IsAvailableForRental { get; set; }

        public string Description { get; set; } = null!;

        public ICollection<BookCategory> Categories { get; set; } = [];
        public ICollection<BookAuthor> Authors { get; set; } = [];
    }
}
