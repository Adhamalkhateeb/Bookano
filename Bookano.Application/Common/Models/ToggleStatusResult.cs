
namespace Bookano.Application.Common.Models;

public sealed record ToggleStatusResult(
    bool IsDeleted,
    DateTimeOffset? LastUpdatedOnUtc
);