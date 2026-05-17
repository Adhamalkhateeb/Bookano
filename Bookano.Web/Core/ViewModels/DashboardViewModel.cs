namespace Bookano.Web.Core.ViewModels
{
    public class DashboardViewModel
    {
        public int NumberOfCopies { get; set; }
        public int NumberOfSubscribers { get; set; }

        public IEnumerable<BookViewModel> RecentlyAddedBooks { get; set; } = [];
        public IEnumerable<BookViewModel> TopRentedBooks { get; set; } = [];
    }
}
