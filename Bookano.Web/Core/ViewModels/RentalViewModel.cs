namespace Bookano.Web.Core.ViewModels
{
    public class RentalViewModel
    {
        public int Id { get; set; }

        public SubscriberViewModel? Subscriber { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Today;

        public bool PenaltyPaid { get; set; }

        public DateTimeOffset CreatedOnUtc { get; set; }

        public IEnumerable<RentalCopyViewModel> RentalCopies = [];

        public int TotalDelayInDays => RentalCopies.Sum(c => c.DelayInDays);
        public int NumberOfCopies => RentalCopies.Count();
        public int ActiveCopies => RentalCopies.Count(c => !c.ReturnDate.HasValue);
    }
}
