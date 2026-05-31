using Bookano.Domain.Common.Constants;

namespace Bookano.Domain.Entities;

public sealed class RentalCopy
{
    public int RentalId { get; set; }
    public Rental? Rental { get; set; }
    public int BookCopyId { get; set; }
    public BookCopy? BookCopy { get; set; }

    public DateOnly RentalDate { get; set; }
    public DateOnly EndDate { get; set; } =
        DateOnly.FromDateTime(DateTime.Now).AddDays(RentalConstants.RentalDuration);

    public DateOnly? ReturnDate { get; set; }

    public DateOnly? ExtendedOn { get; set; }

    public int GetDelayInDays(DateOnly today)
    {
        if (ReturnDate.HasValue)
        {
            return ReturnDate.Value > EndDate ? ReturnDate.Value.DayNumber - EndDate.DayNumber : 0;
        }
        return today > EndDate ? today.DayNumber - EndDate.DayNumber : 0;
    }
}
