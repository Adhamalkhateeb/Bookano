namespace Bookano.Application.DTOs.Areas;

public sealed class AreaDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsDeleted { get; set; }
    public int GovernorateId { get; set; }
    public string Governorate { get; set; } = null!;
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? LastUpdatedOnUtc { get; set; }
}
