namespace Bookano.Application.DTOs.Rentals;

public class RentalSaveDto
{
    public int? Id { get; set; }
    public int SubscriberId { get; set; }
    public IList<int> SelectedCopies { get; set; } = [];
}
