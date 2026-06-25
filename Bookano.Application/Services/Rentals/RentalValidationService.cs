using Bookano.Application.DTOs.Rentals;
using Bookano.Domain.Enums;

namespace Bookano.Application.Services.Rentals;

public class RentalValidationService(IUnitOfWork unitOfWork) : IRentalValidationService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<int>> CheckSubscriberEligibilityAsync(int subscriberId, int? excludeRentalId = null, CancellationToken ct = default)
    {
        var snapshot = await GetSubscriberSnapshotAsync(subscriberId, excludeRentalId, ct);
        if (snapshot is null)
            return Result<int>.Failure("Subscriber not found.");

        return ValidateSubscriberSnapshot(snapshot);
    }

    public async Task<Result<ICollection<RentalCopy>>> ValidateCopiesForRentalAsync(
        int subscriberId, 
        int? excludeRentalId, 
        IEnumerable<int> serialNumbers, 
        int? rentalId, 
        CancellationToken ct = default)
    {
        var snapshot = await GetSubscriberSnapshotAsync(subscriberId, excludeRentalId, ct);
        if (snapshot is null)
            return Result<ICollection<RentalCopy>>.Failure("Subscriber not found.");

        var serialList = serialNumbers.Distinct().ToList();

        var copyData = await _unitOfWork
            .BookCopies.GetQueryable(withTracking: false)
            .Where(c => serialList.Contains(c.SerialNumber))
            .Select(c => new
            {
                c.Id,
                c.BookId,
                BookTitle = c.Book!.Title,
                c.IsAvailableForRental,
                BookIsAvailableForRental = c.Book.IsAvailableForRental,
                IsExistingInRental = rentalId.HasValue && c.Rentals.Any(r => r.RentalId == rentalId),
                IsInOtherActiveRental = c.Rentals.Any(r => !r.ReturnDate.HasValue && (!rentalId.HasValue || r.RentalId != rentalId)),
            })
            .ToListAsync(ct);

        var copies = new List<RentalCopy>();

        foreach (var c in copyData)
        {
            if (c.IsExistingInRental)
            {
                copies.Add(new RentalCopy { BookCopyId = c.Id });
                continue;
            }

            if (!c.IsAvailableForRental || !c.BookIsAvailableForRental)
                return Result<ICollection<RentalCopy>>.Failure(Error.NotAvailableForRental);

            if (c.IsInOtherActiveRental)
                return Result<ICollection<RentalCopy>>.Failure(Error.CopyIsInRental);

            if (snapshot.ActiveBookIds.Contains(c.BookId))
                return Result<ICollection<RentalCopy>>.Failure($"This subscriber already has a copy for '{c.BookTitle}' book");

            copies.Add(new RentalCopy { BookCopyId = c.Id });
        }

        return copies;
    }

    public async Task<Result<bool>> CanExtendRentalAsync(int rentalId, CancellationToken ct = default)
    {
        var rental = await _unitOfWork.Rentals.GetQueryable(withTracking: false)
            .Where(r => r.Id == rentalId)
            .Select(r => new { r.SubscriberId, r.StartDate })
            .SingleOrDefaultAsync(ct);

        if (rental == null) return Result<bool>.Failure("Rental not found.");

        var subscriberInfo = await _unitOfWork.Subscribers.GetQueryable(withTracking: false)
            .Where(s => s.Id == rental.SubscriberId)
            .Select(s => new {
                s.IsBlackListed,
                LatestSubscriptionEndDate = s.Subscriptions.Max(sb => (DateOnly?)sb.EndDate)
            })
            .SingleOrDefaultAsync(ct);

        if (subscriberInfo == null) return Result<bool>.Failure("Subscriber not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var eligibility = Rental.ValidateExtensionEligibility(
            subscriberInfo.IsBlackListed,
            subscriberInfo.LatestSubscriptionEndDate,
            rental.StartDate,
            today
        );

        if (eligibility == ExtensionEligibility.Eligible)
            return true;

        string? error = eligibility switch
        {
            ExtensionEligibility.SubscriberBlackListed => Error.ExtendNotAllowedForBlackListed,
            ExtensionEligibility.SubscriberInactive => Error.ExtendNotAllowedForInactive,
            ExtensionEligibility.NotAllowed => Error.ExtendNotAllowed,
            _ => "Not allowed."
        };

        return Result<bool>.Failure(error);
    }

    public async Task<Result<int>> CalculateReturnPenaltyAsync(int rentalId, CancellationToken ct = default)
    {
        var rental = await _unitOfWork.Rentals.GetQueryable(withTracking: false)
            .Include(r => r.RentalCopies)
            .SingleOrDefaultAsync(r => r.Id == rentalId, ct);

        if (rental == null) return Result<int>.Failure("Rental not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var totalDelayInDays = rental.RentalCopies.Sum(rc =>
            rc.ReturnDate.HasValue
                ? (rc.ReturnDate.Value > rc.EndDate ? rc.ReturnDate.Value.DayNumber - rc.EndDate.DayNumber : 0)
                : (today > rc.EndDate ? today.DayNumber - rc.EndDate.DayNumber : 0)
        );

        return totalDelayInDays;
    }



    private async Task<SubscriberSnapshot?> GetSubscriberSnapshotAsync(int subscriberId, int? excludeRentalId, CancellationToken ct)
    {
        return await _unitOfWork
            .Subscribers.GetQueryable(withTracking: false)
            .Where(s => s.Id == subscriberId)
            .Select(s => new SubscriberSnapshot
            {
                Id = s.Id,
                IsBlackListed = s.IsBlackListed,
                LatestSubscriptionEndDate = s.Subscriptions.Max(sb => (DateOnly?)sb.EndDate),
                UnreturnedCopiesCount = s.Rentals
                    .Where(r => excludeRentalId == null || r.Id != excludeRentalId)
                    .SelectMany(r => r.RentalCopies)
                    .Count(rc => rc.ReturnDate == null),
                ActiveBookIds = s.Rentals
                    .Where(r => excludeRentalId == null || r.Id != excludeRentalId)
                    .SelectMany(r => r.RentalCopies)
                    .Where(rc => rc.ReturnDate == null)
                    .Select(rc => rc.BookCopy!.BookId)
                    .Distinct()
                    .ToHashSet(),
            })
            .SingleOrDefaultAsync(ct);
    }

    private static Result<int> ValidateSubscriberSnapshot(SubscriberSnapshot snapshot)
    {
        var eligibility = Subscriber.ValidateRentalEligibility(
            snapshot.IsBlackListed,
            snapshot.LatestSubscriptionEndDate,
            snapshot.UnreturnedCopiesCount,
            DateOnly.FromDateTime(DateTime.UtcNow)
        );

        string? error = eligibility switch
        {
            RentalEligibility.BlackListed => Error.BlackListedSubscriber,
            RentalEligibility.Inactive => Error.InactiveSubscriber,
            RentalEligibility.MaxCopiesReached => Error.MaxAllowedCopiesReached,
            _ => null
        };

        if (error is not null)
            return Result<int>.Failure(error);

        var availableCopiesCount = RentalConstants.MaxAllowedCopies - snapshot.UnreturnedCopiesCount;
        return Result<int>.Success(availableCopiesCount);
    }
}
