namespace Bookano.Web.Core.ViewModels
{
    public class SubscriptionViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTimeOffset CreatedOnUtc { get; set; }
        public string Status
        {
            get
            {
                var today = DateTime.Today;

                return today > EndDate ? SubscriptionStatus.Expired.ToString()
                    : today >= StartDate ? SubscriptionStatus.Active.ToString()
                    : string.Empty;
            }
        }
    }
}
