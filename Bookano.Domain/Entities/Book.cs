namespace Bookano.Domain.Entities;

public sealed class Book : BaseEntity
{
    public int Id { get; set; }
    public string? Isbn { get; set; }
    public string Title { get; set; } = null!;
    public int PublisherId { get; set; }
    public Publisher? Publisher { get; set; }
    public DateOnly PublishingDate { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImagePublicId { get; set; }
    public string? ImageThumbnailUrl { get; set; }
    public string Hall { get; set; } = null!;
    public bool IsAvailableForRental { get; set; }
    public string Description { get; set; } = null!;
    public string? IdempotencyKey { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ICollection<BookCategory> Categories { get; set; } = [];
    public ICollection<BookAuthor> Authors { get; set; } = [];
    public ICollection<BookCopy> Copies { get; set; } = [];
}
