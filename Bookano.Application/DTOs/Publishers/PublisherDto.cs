namespace Bookano.Application.DTOs.Publishers;

public sealed record PublisherDto(
    int Id,
    string Name,
    bool IsDeleted,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? LastUpdatedOnUtc
);
