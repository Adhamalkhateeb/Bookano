namespace Bookano.Application.DTOs.Dashboard;

public sealed class DashboardDto
{
    public int NumberOfCopies { get; set; }
    public int NumberOfSubscribers { get; set; }
    public IEnumerable<DashboardBookDto> RecentlyAddedBooks { get; set; } = [];
    public IEnumerable<DashboardBookDto> TopRentedBooks { get; set; } = [];
}
