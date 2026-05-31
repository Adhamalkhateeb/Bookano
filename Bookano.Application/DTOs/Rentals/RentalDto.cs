namespace Bookano.Application.DTOs.Rentals;

public class RentalDto
{
    public int Id { get; set; }
    public int SubscriberId { get; set; }
    public DateOnly StartDate { get; set; }
    public bool PenaltyPaid { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public IList<RentalCopyDto> RentalCopies { get; set; } = [];

    public int TotalDelayInDays => RentalCopies.Sum(c => c.DelayInDays);

    public int NumberOfCopies => RentalCopies.Count;

    public int ActiveCopies => RentalCopies.Count(c => !c.ReturnDate.HasValue);
}
