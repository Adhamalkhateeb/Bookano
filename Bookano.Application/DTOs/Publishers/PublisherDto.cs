namespace Bookano.Application.DTOs.Publishers;

public class PublisherDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? LastUpdatedOnUtc { get; set; }
}