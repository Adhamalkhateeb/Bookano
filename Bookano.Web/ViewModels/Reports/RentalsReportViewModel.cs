using Bookano.Application.Common.Models;

namespace Bookano.Web.ViewModels.Reports
{
    public class RentalsReportViewModel
    {
        public string Duration { get; set; } = null!;
        public IEnumerable<RentalsReportItemViewModel>? Rentals { get; set; }
        public PaginatedViewModel? PaginatedViewModel { get; set; }
    }
}
