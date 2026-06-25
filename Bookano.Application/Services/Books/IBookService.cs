using Bookano.Application.DTOs.Books;
using Bookano.Application.DTOs.Categories;
using Bookano.Application.DTOs.Authors;

namespace Bookano.Application.Services.Books;

public interface IBookService
{
    Task<IEnumerable<BookDto>> GetRecentBooksAsync(int count, CancellationToken ct = default);
    Task<IEnumerable<BookDto>> GetTopRentedBooksAsync(int count, CancellationToken ct = default);
    Task<IEnumerable<BookDto>> GetFilteredBooksAsync(string query, CancellationToken ct = default);
    Task<DataGridResult<TOut>> GetPagedFilteredAsync<TOut>(PaginationFilterQuery request, CancellationToken ct = default);
    Task<BookDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<BookDetailsDto?> GetDetailsAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<CategoryDto>> GetBookCategoriesAsync(int bookId, CancellationToken ct = default);
    Task<IEnumerable<AuthorDto>> GetBookAuthorsAsync(int bookId, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(BookSaveDto dto, CancellationToken ct = default);

    Task<Result<int>> UpdateAsync(int id, BookSaveDto dto,CancellationToken ct = default);
        
    Task<Result<ToggleStatusResult>> ToggleStatusAsync(int id, CancellationToken ct = default);

    Task<bool> IsIsbnAvailableAsync(
        string isbn,
        int excludeId = 0,
        CancellationToken ct = default);
}
