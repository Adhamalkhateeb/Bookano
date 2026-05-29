using Bookano.Application.DTOs.BookCopies;

namespace Bookano.Application.Services.BookCopies;

public interface IBookCopiesService
{
    Task<BookDetailsForCopy?> GetBook(int bookId, CancellationToken ct = default);
    Task<Result<BookCopyDto?>> AddAsync(BookCopyFormDto dto, CancellationToken ct = default);
    Task<BookCopyDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<BookCopyDto?>> UpdateAsync(BookCopyFormDto dto, CancellationToken ct = default);
    Task<DateTimeOffset?> ToggleAsync(int id, CancellationToken ct = default);

    Task<IEnumerable<BookCopyRentalHistoryDto>?> GetRentalHistoryAsync(int id, CancellationToken ct = default);

}