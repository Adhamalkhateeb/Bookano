namespace Bookano.Domain.Entities;

public sealed class Subscription : IAuditable
{
    public int Id { get; set; }
    public int SubscriberId { get; set; }
    public Subscriber? Subscriber { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? CreatedById { get; set; }
    public ApplicationUser? CreatedBy { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public string? LastUpdatedById { get; set; }
    public DateTimeOffset? LastUpdatedOnUtc { get ; set; }
}
