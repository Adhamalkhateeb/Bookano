using Bookano.Application.Common.Models;
using Bookano.Application.DTOs.Reports;

namespace Bookano.Application.Services.Reports;

public interface IReportsService
{
    Task<PaginatedList<BookReportDto>> GetBooksReportAsync(
        IEnumerable<int> selectedAuthors,
        IEnumerable<int> selectedCategories,
        int pageNumber = 1,
        int pageSize = ReportsConfigurations.DefaultPageSize,
        CancellationToken ct = default);

    Task<IEnumerable<BookReportDto>> GetBooksReportAsync(
        IEnumerable<int> selectedAuthors,
        IEnumerable<int> selectedCategories,
        CancellationToken ct = default);

    Task<Result<PaginatedList<RentalsReportDto>>> GetRentalsReportAsync(
        string duration,
        int pageNumber = 1,
        int pageSize = ReportsConfigurations.DefaultPageSize,
        CancellationToken ct = default);

    Task<Result<IEnumerable<RentalsReportDto>>> GetRentalsReportAsync(string duration, CancellationToken ct = default);
    Task<IEnumerable<DelayedRentalDto>> GetDelayedRentalsReportAsync(CancellationToken ct = default);
}
