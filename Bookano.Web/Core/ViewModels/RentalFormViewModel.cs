namespace Bookano.Web.Core.ViewModels
{
    public class RentalFormViewModel
    {
        public int? Id { get; set; }
        public string SubscriberKey { get; set; } = null!;

        public IList<int> SelectedCopies { get; set; } = [];

        public IEnumerable<BookCopyViewModel> CurrentCopies = [];

        public int? MaxAllowedCopies { get; set; }
    }
}
