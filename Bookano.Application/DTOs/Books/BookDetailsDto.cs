using Bookano.Application.DTOs.BookCopies;

namespace Bookano.Application.DTOs.Books;

public class BookDetailsDto
{
    public int Id { get; set; }
    public string? Isbn { get; set; }
    public string Title { get; set; } = null!;
    public string Publisher { get; set; } = null!;
    public DateOnly PublishingDate { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsAvailableForRental { get; set; }
    public string Description { get; set; } = null!;
    public IEnumerable<string> Categories { get; set; } = [];
    public IEnumerable<string> Authors { get; set; } = [];
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }

    public IEnumerable<BookCopyDto> Copies { get; set; } = [];
}
