namespace Bookano.Application.DTOs.Categories;

public sealed record CategoryDto(
    int Id,
    string Name,
    bool IsDeleted,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? LastUpdatedOnUtc
);
