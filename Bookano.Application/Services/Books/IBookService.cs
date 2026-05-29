using Bookano.Application.DTOs.Books;

namespace Bookano.Application.Services.Books;

public interface IBookService
{
    Task<DataTableResult<BookListDto>> GetPagedAsync( DataTableRequest request, CancellationToken ct = default);
    Task<BookDetailsDto?> GetBookDetailsAsync(int id, CancellationToken ct = default);
    Task<BookFormDto?> GetBookFormAsync(int id, CancellationToken ct = default);

    Task<Result<int>> CreateAsync(
        BookFormDto dto,
        CancellationToken ct = default);

    Task<Result<int>> UpdateAsync(
        BookFormDto dto,
        CancellationToken ct = default);
        
    Task<DateTimeOffset?> ToggleAsync(int id, CancellationToken ct = default);

    Task<bool> IsIsbnUniqueAsync(
        string isbn,
        int excludeId = 0,
        CancellationToken ct = default);
}
