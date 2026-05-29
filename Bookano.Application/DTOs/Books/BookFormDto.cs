namespace Bookano.Application.DTOs.Books;

public sealed class BookFormDto
{
    public int Id { get; set; }
    public string? IdempotencyKey { get; set; }
    public string? Isbn { get; set; }
    public string Title { get; set; } = null!;
    public int PublisherId { get; set; }
    public DateOnly PublishingDate { get; set; }
    public string Hall { get; set; } = null!;
    public bool IsAvailableForRental { get; set; }
    public string Description { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public string? ImageThumbnailUrl { get; set; }
    public string? ExistingImagePublicId { get; set; }
    public bool RemoveImage { get; set; }
    public byte[]? RowVersion { get; set; }
    public ImageUploadDto? Image { get; set; }
    public IEnumerable<int> SelectedCategories { get; set; } = [];
    public IEnumerable<int> SelectedAuthors { get; set; } = [];
}
