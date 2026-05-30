namespace Bookano.Web.Core.ViewModels
{
    public class RentalViewModel
    {
        public int Id { get; set; }

        public DateOnly StartDate { get; set; }

        public bool PenaltyPaid { get; set; }

        public DateTimeOffset CreatedOnUtc { get; set; }

        public IEnumerable<RentalCopyViewModel> RentalCopies { get; set; } = [];

        public int TotalDelayInDays { get; set; }
        public int NumberOfCopies { get; set; }
        public int ActiveCopies { get; set; }
    }
}
