using Bookano.Application.DTOs.BookCopies;

namespace Bookano.Application.DTOs.Books;

public class BookDetailsDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int PublisherId { get; set; }
    public string Publisher { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public string? ImageThumbnailUrl { get; set; }
    public string? Isbn { get; set; }
    public DateOnly PublishingDate { get; set; }
    public string Hall { get; set; } = null!;
    public bool IsAvailableForRental { get; set; }
    public string Description { get; set; } = null!;
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? LastUpdatedOnUtc { get; set; }
    public bool IsDeleted { get; set; }
    public byte[]? RowVersion { get; set; }
    public string? ImagePublicId { get; set; }

    public IEnumerable<string> Categories { get; set; } = [];
    public IEnumerable<string> Authors { get; set; } = [];
    public IEnumerable<BookCopyDto> Copies { get; set; } = [];
}
