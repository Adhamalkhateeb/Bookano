namespace Bookano.Application.DTOs.Rentals;

public class RentalFormDto
{
    public int? Id { get; set; }
    public int SubscriberId { get; set; }
    public IList<int> SelectedCopies { get; set; } = [];
    public IEnumerable<RentalBookCopyDto> CurrentCopies { get; set; } = [];
    public int? MaxAllowedCopies { get; set; }
}
