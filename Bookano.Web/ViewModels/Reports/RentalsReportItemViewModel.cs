using Bookano.Application.Attributes;

namespace Bookano.Web.ViewModels.Reports
{
    public class RentalsReportItemViewModel
    {
        [ReportColumn("Book Title", 4)]
        public string? BookTitle { get; set; }

        [ReportColumn("Book Authors", 5)]
        public IEnumerable<string>? BookAuthors { get; set; }

        [ReportColumn("Book Serial", 6)]
        public int BookSerialNumber { get; set; }

        [ReportColumn("Subscriber Id", 1)]
        public int SubscriberId { get; set; }

        [ReportColumn("Subscriber Name", 2)]
        public string? SubscriberName { get; set; }

        [ReportColumn("Subscriber Mobile", 3)]
        public string? SubscriberMobile { get; set; }

        [ReportColumn("Rental Date", 7)]
        public DateTime RentalDate { get; set; }

        [ReportColumn("End Date", 8)]
        public DateTime EndDate { get; set; }

        [ReportColumn("Return Date", 9)]
        public DateTime? ReturnDate { get; set; }

        [ReportColumn("Extended On", 10)]
        public DateTime? ExtendedOn { get; set; }
    }
}
