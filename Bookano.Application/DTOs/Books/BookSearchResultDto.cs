namespace Bookano.Application.DTOs.Books;

public class BookSearchResultDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public IEnumerable<string> Authors { get; set; } = [];
}
