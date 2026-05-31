using Bookano.Application.Attributes;

namespace Bookano.Web.ViewModels.Reports
{
    public class DelayedRentalItemViewModel
    {
        [ReportColumn("Subscriber Id", 1)]
        public int SubscriberId { get; set; }

        [ReportColumn("Subscriber Name", 2)]
        public string? SubscriberName { get; set; }

        [ReportColumn("Subscriber Mobile", 3)]
        public string? SubscriberMobile { get; set; }

        [ReportColumn("Book Title", 4)]
        public string? BookTitle { get; set; }

        [ReportColumn("Book Serial", 5)]
        public int BookSerialNumber { get; set; }

        [ReportColumn("Rental Date", 6)]
        public DateTime RentalDate { get; set; }

        [ReportColumn("End Date", 7)]
        public DateTime EndDate { get; set; }

        [ReportColumn("Extended On", 8)]
        public DateTime? ExtendedOn { get; set; }

        [ReportColumn("Delay In Days", 9)]
        public int DelayInDays => (int)(DateTime.Now.Subtract(EndDate)).TotalDays;
    }
}
