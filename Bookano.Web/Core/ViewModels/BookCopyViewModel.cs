using NuGet.Protocol.Plugins;

namespace Bookano.Web.Core.ViewModels
{
    public class BookCopyViewModel
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = null!;

        public string? BookImageUrl { get; set; } = null!;
        public string? BookThumbnailUrl { get; set; } = null!;
        public int EditionNumber { get; set; }
        public int SerialNumber { get; set; }
        public bool IsAvailableForRental { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedOnUtc { get; set; }
    }
}
