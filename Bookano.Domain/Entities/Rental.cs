using Bookano.Domain.Common.Constants;
using Bookano.Domain.Enums;

namespace Bookano.Domain.Entities;

public sealed class Rental : BaseEntity
{
    public int Id { get; set; }

    public int SubscriberId { get; set; }
    public Subscriber? Subscriber { get; set; }

    public DateOnly StartDate { get; set; }

    public bool PenaltyPaid { get; set; }

    public ICollection<RentalCopy> RentalCopies { get; set; } = [];


    public static ExtensionEligibility ValidateExtensionEligibility(
        bool isBlackListed,
        DateOnly? latestSubscriptionEndDate,
        DateOnly startDate,
        DateOnly today
    )
    {
        if (isBlackListed)
            return ExtensionEligibility.SubscriberBlackListed;

        var extendDeadline = startDate.AddDays(RentalConstants.MaxRentalDuration);
        if (latestSubscriptionEndDate == null || latestSubscriptionEndDate < extendDeadline)
            return ExtensionEligibility.SubscriberInactive;

        if (today > startDate.AddDays(RentalConstants.RentalDuration))
            return ExtensionEligibility.NotAllowed;

        return ExtensionEligibility.Eligible;
    }
}
