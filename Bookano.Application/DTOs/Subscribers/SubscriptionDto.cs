namespace Bookano.Application.DTOs.Subscribers;

public sealed class SubscriptionDto
{
    public int Id { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
}
