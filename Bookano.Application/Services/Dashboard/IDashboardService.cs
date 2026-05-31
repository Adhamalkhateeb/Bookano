using Bookano.Application.DTOs.Dashboard;

namespace Bookano.Application.Services.Dashboard;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardDataAsync(CancellationToken ct = default);
    Task<IEnumerable<ChartItemDto>> GetRentalsPerDayAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default);
    Task<IEnumerable<ChartItemDto>> GetSubscribersPerGovernorateAsync(CancellationToken ct = default);
}
