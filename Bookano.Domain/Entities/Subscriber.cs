using Bookano.Domain.Common.Constants;
using Bookano.Domain.Enums;

namespace Bookano.Domain.Entities;

public sealed class Subscriber : BaseEntity
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateOnly DateOfBirth { get; set; }
    public string NationalId { get; set; } = null!;
    public string MobileNumber { get; set; } = null!;
    public bool HasWhatsApp { get; set; }
    public string Email { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public string ImageThumbnailUrl { get; set; } = null!;
    public string ImagePublicId { get; set; } = null!;
    public int AreaId { get; set; }
    public Area? Area { get; set; }
    public string Address { get; set; } = null!;
    public bool IsBlackListed { get; set; } = false;

    public ICollection<Subscription> Subscriptions { get; set; } = [];
    public ICollection<Rental> Rentals { get; set; } = [];

    public SubscriberStatus GetStatus(DateOnly today)
    {
        if (IsBlackListed)
            return SubscriberStatus.Banned;

        var latestSubscriptionEndDate = Subscriptions.Any()
            ? Subscriptions.Max(s => s.EndDate)
            : (DateOnly?)null;

        if (latestSubscriptionEndDate == null || latestSubscriptionEndDate < today)
            return SubscriberStatus.Inactive;

        return SubscriberStatus.Active;
    }

    public bool CanRent(DateOnly today)
    {
        var unreturnedCopiesCount = Rentals
            .SelectMany(r => r.RentalCopies)
            .Count(rc => rc.ReturnDate == null);

        var latestSubscriptionEndDate = Subscriptions.Any()
            ? Subscriptions.Max(s => s.EndDate)
            : (DateOnly?)null;

        var eligibility = ValidateRentalEligibility(IsBlackListed, latestSubscriptionEndDate, unreturnedCopiesCount, today);
        return eligibility == RentalEligibility.Eligible;
    }

    public static RentalEligibility ValidateRentalEligibility(
        bool isBlackListed,
        DateOnly? latestSubscriptionEndDate,
        int unreturnedCopiesCount,
        DateOnly today
    )
    {
        if (isBlackListed)
            return RentalEligibility.BlackListed;

        var latestAllowedDate = today.AddDays(RentalConstants.RentalDuration);

        if (latestSubscriptionEndDate is null || latestSubscriptionEndDate < latestAllowedDate)
            return RentalEligibility.Inactive;

        var availableCopiesCount = RentalConstants.MaxAllowedCopies - unreturnedCopiesCount;

        if (availableCopiesCount <= 0)
            return RentalEligibility.MaxCopiesReached;

        return RentalEligibility.Eligible;
    }
}
