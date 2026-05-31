using Bookano.Application.DTOs.Books;
using Bookano.Application.DTOs.Search;

namespace Bookano.Application.Services.Search;

public interface ISearchService
{
    Task<IEnumerable<SearchBookDto>> FindBooksAsync(string query, CancellationToken ct = default);
    Task<BookDetailsDto?> GetBookDetailsAsync(int id, CancellationToken ct = default);
}
