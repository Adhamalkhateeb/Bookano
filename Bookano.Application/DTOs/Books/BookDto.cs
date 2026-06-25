namespace Bookano.Application.DTOs.Books;

public sealed record BookDto(
    int Id,
    string Title,
    int PublisherId,
    string Publisher,
    string? ImageUrl,
    string? ImageThumbnailUrl,
    string? Isbn,
    DateOnly PublishingDate,
    string Hall,
    bool IsAvailableForRental,
    string Description,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? LastUpdatedOnUtc,
    bool IsDeleted,
    byte[]? RowVersion,
    string? ImagePublicId
);
