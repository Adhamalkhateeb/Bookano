namespace Bookano.Web.Core.ViewModels
{
    public class CopyHistoyViewModel
    {
        public string? SubscriberName { get; set; }
        public string? SubscriberMobile { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public DateTime? ReturnDate { get; set; }
        public DateTime? ExtendedOn { get; set; }

        public int DelayInDays
        {
            get
            {
                if (ReturnDate.HasValue && ReturnDate.Value > EndDate)
                    return ReturnDate.Value.Date.Subtract(EndDate).Days;

                if (!ReturnDate.HasValue && DateTime.Today > EndDate)
                    return DateTime.Today.Subtract(EndDate).Days;

                return 0;
            }
        }
    }
}
