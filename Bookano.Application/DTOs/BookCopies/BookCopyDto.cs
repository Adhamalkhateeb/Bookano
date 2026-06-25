
namespace Bookano.Application.DTOs.BookCopies;

public class BookCopyDto
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string? BookTitle { get; set; }
    public string? BookImageUrl { get; set; }
    public string? BookThumbnailUrl { get; set; }
    public bool BookIsAvailableForRental { get; set; }
    public int EditionNumber { get; set; }
    public int SerialNumber { get; set; }
    public bool IsAvailableForRental { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset LastUpdatedOnUtc { get; set; }
}
