namespace Bookano.Application.DTOs.Rentals;

public class RentalBookCopyDto
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = null!;
    public string? BookImageUrl { get; set; }
    public string? BookThumbnailUrl { get; set; }
    public int EditionNumber { get; set; }
    public int SerialNumber { get; set; }
    public bool IsAvailableForRental { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
}
