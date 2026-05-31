namespace Bookano.Application.DTOs.Reports;

public sealed class DelayedRentalDto
{
    public int SubscriberId { get; set; }
    public string SubscriberName { get; set; } = null!;
    public string SubscriberMobile { get; set; } = null!;
    public string BookTitle { get; set; } = null!;
    public int BookSerialNumber { get; set; }
    public DateOnly RentalDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateOnly? ExtendedOn { get; set; }
}
