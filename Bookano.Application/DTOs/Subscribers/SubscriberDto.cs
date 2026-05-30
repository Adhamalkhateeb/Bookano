namespace Bookano.Application.DTOs.Subscribers;

public sealed class SubscriberDto
{
    public int Id { get; set; }
    public string FullName { get; set;} = null!;
    public DateOnly DateOfBirth { get; set; }
    public string NationalId { get; set; } = null!;
    public string MobileNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public string ImageThumbnailUrl { get; set; } = null!;
    public string Area { get; set; } = null!;
    public string Governorate { get; set; } = null!;
    public string Address { get; set; } = null!;
    public bool IsBlackListed { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public IEnumerable<SubscriptionDto> Subscriptions { get; set; } = [];
    public IEnumerable<SubscriberRentalDto> Rentals { get; set; } = [];
}
