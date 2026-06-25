using Bookano.Web.ViewModels.Books;

namespace Bookano.Web.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public int NumberOfCopies { get; set; }
        public int NumberOfSubscribers { get; set; }

        public IEnumerable<BookViewModel> RecentlyAddedBooks { get; set; } = [];
        public IEnumerable<BookViewModel> TopRentedBooks { get; set; } = [];

    }
}
