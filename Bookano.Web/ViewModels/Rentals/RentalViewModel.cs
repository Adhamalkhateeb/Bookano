namespace Bookano.Web.ViewModels.Rentals
{
    public class RentalViewModel
    {
        public int Id { get; set; }

        public DateOnly StartDate { get; set; }

        public bool PenaltyPaid { get; set; }

        public DateTimeOffset CreatedOnUtc { get; set; }

        public IEnumerable<RentalCopyViewModel> RentalCopies { get; set; } = [];

        public int TotalDelayInDays => RentalCopies.Sum(c => c.DelayInDays);
        public int NumberOfCopies => RentalCopies.Count();
        public int ActiveCopies => RentalCopies.Count(c => c.ReturnDate == null);
    }
}
