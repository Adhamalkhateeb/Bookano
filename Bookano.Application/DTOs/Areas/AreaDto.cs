namespace Bookano.Application.DTOs.Areas;

public sealed record AreaDto(
    int Id,
    string Name,
    bool IsDeleted,
    int GovernorateId,
    string Governorate,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? LastUpdatedOnUtc
 );