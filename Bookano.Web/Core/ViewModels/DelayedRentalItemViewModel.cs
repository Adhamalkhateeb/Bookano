namespace Bookano.Web.Core.ViewModels
{
    public class DelayedRentalItemViewModel
    {
        public int SubscriberId { get; set; }
        public string? SubscriberName { get; set; }
        public string? SubscriberMobile { get; set; }
        public string? BookTitle { get; set; }

        public int BookSerialNumber { get; set; }

        public DateTime RentalDate { get; set; }
        public DateTime EndDate { get; set; }

        public DateTime? ExtendedOn { get; set; }

        public int DelayInDays => (int)(DateTime.Now - EndDate).TotalDays;
    }
}
