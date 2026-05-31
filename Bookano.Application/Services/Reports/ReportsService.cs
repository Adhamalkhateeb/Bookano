using System.Globalization;
using System.Net.Quic;
using Bookano.Application.DTOs.Reports;

namespace Bookano.Application.Services.Reports;

public sealed class ReportsService(IUnitOfWork unitOfWork) : IReportsService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<PaginatedList<BookReportDto>> GetBooksReportAsync(
        IEnumerable<int> selectedAuthors,
        IEnumerable<int> selectedCategories,
        int pageNumber = 1,
        int pageSize = ReportsConfigurations.DefaultPageSize,
        CancellationToken ct = default)
    {
        var query = GetBooksReportQuery(selectedAuthors, selectedCategories);
        return await PaginatedList<BookReportDto>.CreateAsync(query, pageNumber, pageSize, ct);
    }

    public async Task<IEnumerable<BookReportDto>> GetBooksReportAsync(
        IEnumerable<int> selectedAuthors,
        IEnumerable<int> selectedCategories,
        CancellationToken ct = default)
    {
        var query = GetBooksReportQuery(selectedAuthors, selectedCategories);
        return await query.ToListAsync(ct);
    }


    public async Task<Result<PaginatedList<RentalsReportDto>>> GetRentalsReportAsync(string duration, int pageNumber = 1,
        int pageSize = ReportsConfigurations.DefaultPageSize,
        CancellationToken ct = default)
    {
        var query =  GetRentalsReportQuery(duration);
        if(query.IsFailure)
            return Result<PaginatedList<RentalsReportDto>>.Failure(query.ErrorMessage!);

        var result = await PaginatedList<RentalsReportDto>.CreateAsync(query.Value!,pageNumber,pageSize,ct);

        return Result<PaginatedList<RentalsReportDto>>.Success(result);
    }

    public async Task<Result<IEnumerable<RentalsReportDto>>> GetRentalsReportAsync(
        string duration,
        CancellationToken ct = default)
    {
        var query = GetRentalsReportQuery(duration);

        if (query.IsFailure)
            return Result<IEnumerable<RentalsReportDto>>.Failure(query.ErrorMessage!);

        var result = await query.Value!.ToListAsync(ct);

        return Result<IEnumerable<RentalsReportDto>>.Success(result);
    }



    public async Task<IEnumerable<DelayedRentalDto>> GetDelayedRentalsReportAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return await _unitOfWork.RentalCopies.GetQueryable()
            .Where(rc => !rc.ReturnDate.HasValue && rc.EndDate < today)
            .Select(rc => new DelayedRentalDto
            {
                SubscriberId = rc.Rental!.SubscriberId,
                SubscriberMobile = rc.Rental.Subscriber!.MobileNumber,
                SubscriberName = $"{rc.Rental.Subscriber.FirstName} {rc.Rental.Subscriber.LastName}",
                BookTitle = rc.BookCopy!.Book!.Title,
                BookSerialNumber = rc.BookCopy.SerialNumber,
                RentalDate = rc.RentalDate,
                EndDate = rc.EndDate,
                ExtendedOn = rc.ExtendedOn.HasValue ? rc.ExtendedOn.Value : null
            })
            .ToListAsync(ct);
    }

    private IQueryable<BookReportDto> GetBooksReportQuery(
    IEnumerable<int> selectedAuthors,
    IEnumerable<int> selectedCategories)
    {
        var booksQuery = _unitOfWork.Books.GetQueryable();

        if (selectedAuthors.Any())
        {
            booksQuery = booksQuery.Where(b =>
                b.Authors.Any(a => selectedAuthors.Contains(a.AuthorId))
            );
        }

        if (selectedCategories.Any())
        {
            booksQuery = booksQuery.Where(b =>
                b.Categories.Any(c => selectedCategories.Contains(c.CategoryId))
            );
        }

        return booksQuery
            .OrderByDescending(b => b.CreatedOnUtc)
            .Select(b => new BookReportDto
            {
                Isbn = b.Isbn ?? "N/A",
                Title = b.Title,
                Authors = b.Authors.Select(a => a.Author!.Name).ToList(),
                Categories = b.Categories.Select(c => c.Category!.Name).ToList(),
                Publisher = b.Publisher!.Name,
                PublishingDate = b.PublishingDate,
                Hall = b.Hall,
                IsAvailableForRental = b.IsAvailableForRental,
                IsDeleted = b.IsDeleted,
            });
    }

    private Result<IQueryable<RentalsReportDto>> GetRentalsReportQuery(string? duration)
    {
        if (string.IsNullOrEmpty(duration))
            return Result<IQueryable<RentalsReportDto>>.Success(Enumerable.Empty<RentalsReportDto>().AsQueryable());

        var dateRange = duration.Split(" - ");

        if (dateRange.Length != 2)
            return Result<IQueryable<RentalsReportDto>>.Failure(Error.InvalidDuration);

        if (!DateOnly.TryParse(dateRange[0].Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var fromDate))
            return Result<IQueryable<RentalsReportDto>>.Failure(Error.InvalidStartDate);

        if (!DateOnly.TryParse(dateRange[1].Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var toDate))
            return Result<IQueryable<RentalsReportDto>>.Failure(Error.InvalidEndDate);

        var query = _unitOfWork.RentalCopies.GetQueryable()
            .Where(rc => rc.RentalDate >= fromDate && rc.RentalDate <= toDate)
            .OrderByDescending(rc => rc.RentalDate)
            .Select(rc => new RentalsReportDto
            {
                SubscriberId = rc.Rental!.SubscriberId,
                SubscriberName = $"{rc.Rental.Subscriber!.FirstName} {rc.Rental.Subscriber.LastName}",
                SubscriberMobile = rc.Rental.Subscriber.MobileNumber,
                BookTitle = rc.BookCopy!.Book!.Title,
                BookSerialNumber = rc.BookCopy.SerialNumber,
                BookAuthors = rc.BookCopy.Book.Authors.Select(a => a.Author!.Name).ToList(),
                RentalDate = rc.RentalDate,
                EndDate = rc.EndDate,
                ReturnDate = rc.ReturnDate.HasValue
                    ? rc.ReturnDate.Value : null,
                ExtendedOn = rc.ExtendedOn.HasValue
                    ? rc.ExtendedOn.Value : null,
            });

        return Result<IQueryable<RentalsReportDto>>.Success(query);
    }
}
