namespace Bookano.Application.DTOs.Search;

public sealed class SearchBookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Authors { get; set; } = null!;
}
