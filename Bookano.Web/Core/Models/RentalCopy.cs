namespace Bookano.Web.Core.Models
{
    public class RentalCopy
    {
        public int RentalId { get; set; }
        public Rental? Rental { get; set; }
        public int BookCopyId { get; set; }
        public BookCopy? BookCopy { get; set; }

        public DateOnly RentalDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        public DateOnly EndDate { get; set; } =
            DateOnly.FromDateTime(
                DateTime.Today.AddDays((int)RentalsConfigurations.RentalDuration)
            );

        public DateOnly? ReturnDate { get; set; }

        public DateOnly? ExtendedOn { get; set; }
    }
}
