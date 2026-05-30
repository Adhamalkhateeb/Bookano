namespace Bookano.Application.DTOs.Subscribers;

public sealed class SubscriberSearchResultDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string ImageThumbnailUrl { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
}
