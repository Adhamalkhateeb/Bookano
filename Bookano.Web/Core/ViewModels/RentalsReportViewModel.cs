namespace Bookano.Web.Core.ViewModels
{
    public class RentalsReportViewModel
    {
        public string Duration { get; set; } = null!;
        public PaginatedList<RentalsReportItemViewModel> Rentals { get; set; }
    }
}
