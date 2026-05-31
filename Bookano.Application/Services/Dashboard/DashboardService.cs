using Bookano.Application.Common.Interfaces;
using Bookano.Application.DTOs.Dashboard;

namespace Bookano.Application.Services.Dashboard;

public sealed class DashboardService(IUnitOfWork unitOfWork) : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<DashboardDto> GetDashboardDataAsync(CancellationToken ct = default)
    {
        var copiesCount = await _unitOfWork
            .BookCopies.GetQueryable()
            .CountAsync(c => !c.IsDeleted, ct);
        copiesCount = copiesCount <= 10 ? copiesCount : copiesCount / 10 * 10;

        var subscribersCount = await _unitOfWork.Subscribers.CountAsync(s => !s.IsDeleted, ct);

        var recentlyAddedBooks = await _unitOfWork
            .Books.GetQueryable()
            .Where(b => !b.IsDeleted)
            .OrderByDescending(b => b.CreatedOnUtc)
            .Take(8)
            .Select(b => new DashboardBookDto
            {
                Id = b.Id,
                Title = b.Title,
                ImageUrl = b.ImageUrl,
                Authors = b.Authors.Select(a => a.Author!.Name).ToList(),
            })
            .ToListAsync(ct);

        var topBookIds = await _unitOfWork
            .RentalCopies.GetQueryable()
            .GroupBy(rc => rc.BookCopy!.BookId)
            .OrderByDescending(g => g.Count())
            .Take(6)
            .Select(g => g.Key)
            .ToListAsync(ct);

        var topRentedBooks = await _unitOfWork
            .Books.GetQueryable()
            .Where(b => topBookIds.Contains(b.Id) && !b.IsDeleted)
            .Select(b => new DashboardBookDto
            {
                Id = b.Id,
                Title = b.Title,
                ImageUrl = b.ImageUrl,
                Authors = b.Authors.Select(a => a.Author!.Name).ToList(),
            })
            .ToListAsync(ct);

        var orderedBooks = topBookIds
            .Join(topRentedBooks, id => id, book => book.Id, (id, book) => book)
            .ToList();

        return new DashboardDto
        {
            NumberOfCopies = copiesCount,
            NumberOfSubscribers = subscribersCount,
            RecentlyAddedBooks = recentlyAddedBooks,
            TopRentedBooks = orderedBooks,
        };
    }

    public async Task<IEnumerable<ChartItemDto>> GetRentalsPerDayAsync(
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        CancellationToken ct = default)
    {
        var start = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-29));
        var end = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var data = await _unitOfWork
            .RentalCopies.GetQueryable()
            .Where(rc => rc.RentalDate >= start && rc.RentalDate <= end)
            .GroupBy(rc => rc.RentalDate)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var figures = new List<ChartItemDto>();

        for (var day = start; day <= end; day = day.AddDays(1))
        {
            var count = data.FirstOrDefault(d => d.Date == day)?.Count ?? 0;
            figures.Add(
                new ChartItemDto
                {
                    Label = day.ToString("d MMM"),
                    Value = count.ToString(),
                }
            );
        }

        return figures;
    }

    public async Task<IEnumerable<ChartItemDto>> GetSubscribersPerGovernorateAsync(CancellationToken ct = default)
    {
        return await _unitOfWork
            .Subscribers.GetQueryable()
            .Where(s => !s.IsDeleted)
            .GroupBy(s => new { GovernorateName = s.Area!.Governorate!.Name })
            .Select(g => new ChartItemDto
            {
                Label = g.Key.GovernorateName,
                Value = g.Count().ToString(),
            })
            .ToListAsync(ct);
    }
}
