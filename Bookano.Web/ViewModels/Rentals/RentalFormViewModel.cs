using Bookano.Web.ViewModels.BookCopies;

namespace Bookano.Web.ViewModels.Rentals
{
    public class RentalFormViewModel
    {
        public int? Id { get; set; }
        public string SubscriberKey { get; set; } = null!;

        public IList<int> SelectedCopies { get; set; } = [];

        public IEnumerable<BookCopyViewModel> CurrentCopies { get; set; } = [];

        public int? MaxAllowedCopies { get; set; }
    }
}
