namespace Bookano.Application.DTOs.Areas;

public sealed class AreaFormDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int GovernorateId { get; set; }
}
