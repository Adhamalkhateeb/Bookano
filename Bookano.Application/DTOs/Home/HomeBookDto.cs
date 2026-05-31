namespace Bookano.Application.DTOs.Home;

public sealed class HomeBookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public IEnumerable<string> Authors { get; set; } = [];
}
