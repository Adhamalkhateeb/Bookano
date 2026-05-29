namespace Bookano.Application.DTOs.Books;

public class BookListDto
{
    public int Id { get; set; }
    public string? Isbn { get; set; }
    public string Title { get; set; } = null!;
    public string Publisher { get; set; } = null!;
    public DateOnly PublishingDate { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageThumbnailUrl { get; set; }
    public string Hall { get; set; } = null!;
    public bool IsAvailableForRental { get; set; }
    public IEnumerable<string> Authors { get; set; } = [];
    public bool IsDeleted { get; set; }
}
