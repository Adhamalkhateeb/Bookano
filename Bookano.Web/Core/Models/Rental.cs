namespace Bookano.Web.Core.Models
{
    public class Rental : BaseModel
    {
        public int Id { get; set; }

        public int SubscriberId { get; set; }
        public Subscriber? Subscriber { get; set; }

        public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        public bool PenaltyPaid { get; set; }

        public ICollection<RentalCopy> RentalCopies { get; set; } = [];
    }
}
