namespace Bookano.Application.DTOs.Subscribers;

public class SubscriberRentalDto
{
    public int Id { get; set; }
    public DateOnly StartDate { get; set; } 
    public DateTimeOffset CreatedOnUtc { get; set; }
    public int TotalDelayInDays { get; set; }
    public int NumberOfCopies { get; set; }
    public int ActiveCopies { get; set; }
}
