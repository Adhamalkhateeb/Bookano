namespace Bookano.Application.DTOs.Rentals;

public class RentalReturnDto
{
    public int Id { get; set; }
    public bool PenalityPaid { get; set; }
    public IList<RentalCopyReturnDto> RentalCopies { get; set; } = [];
}
