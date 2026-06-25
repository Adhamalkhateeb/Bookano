using Bookano.Application.DTOs.BookCopies;
using Bookano.Application.DTOs.Subscribers;

namespace Bookano.Application.DTOs.Rentals;

public class RentalCopyDto
{
    public BookCopyDto? BookCopy { get; set; }
    public SubscriberDto? Subscriber { get; set; }

    public DateOnly RentalDate { get; set; }

    public DateOnly EndDate { get; set; }

    public DateOnly? ReturnDate { get; set; }

    public DateOnly? ExtendedOn { get; set; }

    public bool? IsReturned { get; set; }
}
