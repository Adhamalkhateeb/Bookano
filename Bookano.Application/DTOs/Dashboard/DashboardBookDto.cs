namespace Bookano.Application.DTOs.Dashboard;

public sealed class DashboardBookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public IEnumerable<string> Authors { get; set; } = [];
}
