namespace Bookano.Domain.Entities
{
    public class RentalCopy
    {
        public int RentalId { get; set; }
        public Rental? Rental { get; set; }
        public int BookCopyId { get; set; }
        public BookCopy? BookCopy { get; set; }

        public DateOnly RentalDate { get; set; }
        public DateOnly EndDate { get; set; } =
            DateOnly.FromDateTime(DateTime.Now).AddDays((int)RentalsConfigurations.RentalDuration);

        public DateOnly? ReturnDate { get; set; }

        public DateOnly? ExtendedOn { get; set; }
    }
}
