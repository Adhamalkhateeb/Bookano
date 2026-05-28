namespace Bookano.Domain.Interfaces;

public interface IAuditable
{
    string? CreatedById { get; set; }
    DateTimeOffset CreatedOnUtc { get; set; }
    string? LastUpdatedById { get; set; }
    DateTimeOffset? LastUpdatedOnUtc { get; set; }
}
