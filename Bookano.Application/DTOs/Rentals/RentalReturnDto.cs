namespace Bookano.Application.DTOs.Rentals;

public class RentalReturnDto
{
    public int Id { get; set; }
    public bool PenalityPaid { get; set; }
    public IList<RentalCopyDto> RentalCopies { get; set; } = [];
    public bool AllowExtend { get; set; }
    public int TotalDelayInDays { get; set; }
}
