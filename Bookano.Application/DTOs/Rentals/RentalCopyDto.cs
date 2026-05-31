namespace Bookano.Application.DTOs.Rentals;

public class RentalCopyDto
{
    public RentalBookCopyDto? BookCopy { get; set; }

    public DateOnly RentalDate { get; set; }

    public DateOnly EndDate { get; set; }

    public DateOnly? ReturnDate { get; set; }

    public DateOnly? ExtendedOn { get; set; }

    public bool? IsReturned { get; set; }

    public int DelayInDays
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            if (ReturnDate.HasValue && ReturnDate.Value > EndDate)
                return ReturnDate.Value.DayNumber - EndDate.DayNumber;

            if (!ReturnDate.HasValue && today > EndDate)
                return today.DayNumber - EndDate.DayNumber;

            return 0;
        }
    }
}
