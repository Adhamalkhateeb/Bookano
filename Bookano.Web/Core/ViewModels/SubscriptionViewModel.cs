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
                return DateTime.Today > EndDate ? SubscriptionStatus.Expired.ToString()
                    : DateTime.Today >= StartDate ? SubscriptionStatus.Active.ToString()
                    : string.Empty;
            }
        }
    }
}
