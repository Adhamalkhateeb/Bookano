

namespace Bookano.Application.DTOs.BookCopies;

public class BookCopyRentalHistoryDto
{
    public string? SubscriberName { get; set; }
    public string? SubscriberMobile { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public DateOnly? ReturnDate { get; set; }
    public DateOnly? ExtendedOn { get; set; }
}
