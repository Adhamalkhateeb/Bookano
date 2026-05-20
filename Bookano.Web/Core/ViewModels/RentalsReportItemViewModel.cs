namespace Bookano.Web.Core.ViewModels
{
    public class RentalsReportItemViewModel
    {
        public string? BookTitle { get; set; }
        public IEnumerable<string>? BookAuthors { get; set; }
        public int BookSerialNumber { get; set; }
        public int SubscriberId { get; set; }
        public string? SubscriberName { get; set; }
        public string? SubscriberMobile { get; set; }

        public DateTime RentalDate { get; set; }
        public DateTime EndDate { get; set; }

        public DateTime? ReturnDate { get; set; }
        public DateTime? ExtendedOn { get; set; }
    }
}
