using Bookano.Application.DTOs.Dashboard;

namespace Bookano.Application.Services.Dashboard;

public interface IDashboardService
{
    Task<IEnumerable<ChartItemDto>> GetSubscribersPerGovernorateAsync(CancellationToken ct = default);

    Task<IEnumerable<ChartItemDto>> GetSubscribersPerCityAsync(CancellationToken ct = default);
}
