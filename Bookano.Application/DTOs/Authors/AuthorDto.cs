namespace Bookano.Application.DTOs.Authors;

public sealed record AuthorDto(
    int Id, 
    string Name,
    bool IsDeleted,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? LastUpdatedOnUtc
);
