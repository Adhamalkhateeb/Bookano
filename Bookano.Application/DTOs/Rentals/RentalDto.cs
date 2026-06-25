namespace Bookano.Application.DTOs.Rentals;

public class RentalDto
{
    public int Id { get; set; }
    public int SubscriberId { get; set; }
    public DateOnly StartDate { get; set; }
    public bool PenaltyPaid { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public IList<RentalCopyDto> RentalCopies { get; set; } = [];
}
