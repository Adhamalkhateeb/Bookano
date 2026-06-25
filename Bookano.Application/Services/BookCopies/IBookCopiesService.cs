using Bookano.Application.DTOs.BookCopies;

namespace Bookano.Application.Services.BookCopies;

public interface IBookCopiesService
{
    Task<int> GetCountAsync(CancellationToken ct = default);
    Task<Result<BookCopyDto?>> AddAsync(BookCopySaveDto dto, CancellationToken ct = default);
    Task<Result<BookCopyDto?>> UpdateAsync(BookCopySaveDto dto, CancellationToken ct = default);
    Task<BookCopyDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<BookCopyDto?> GetActiveCopyBySerialNumberAsync(int serialNumber,CancellationToken ct = default);
    Task<Result<ToggleStatusResult>> ToggleStatusAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<BookCopyDto>> GetByBookAsync(int bookId,CancellationToken ct = default);
    Task<IEnumerable<BookCopyDto>> GetCopiesBySerialNumbersAsync(IEnumerable<int> serialNumbers, CancellationToken ct = default);

}