namespace Bookano.Application.DTOs.Rentals;
internal sealed class SubscriberSnapshot
{
    public int Id { get; init; }
    public bool IsBlackListed { get; init; }
    public DateOnly? LatestSubscriptionEndDate { get; init; }
    public int UnreturnedCopiesCount { get; init; }
    public HashSet<int> ActiveBookIds { get; init; } = [];
}