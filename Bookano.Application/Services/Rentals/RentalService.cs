using Bookano.Application.DTOs.Rentals;
using Bookano.Domain.Enums;
using Bookano.Domain.Entities;
using Bookano.Domain.Common.Constants;

namespace Bookano.Application.Services.Rentals;

public class RentalService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IValidator<RentalFormDto> rentalFormValidator,
    IValidator<RentalReturnDto> rentalReturnValidator
) : IRentalService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IValidator<RentalFormDto> _rentalFormValidator = rentalFormValidator;
    private readonly IValidator<RentalReturnDto> _rentalReturnValidator = rentalReturnValidator;


    public async Task<Result<int>> GetAvailableCopiesCountAsync(
        int subscriberId,
        int? excludeRentalId = null,
        CancellationToken ct = default
    )
    {
        var snapshot = await GetSubscriberSnapshotAsync(subscriberId, excludeRentalId, ct);

        if (snapshot is null)
            return Result<int>.Failure("Subscriber not found.");

        var (errorMessage, availableCopiesCount) = ValidateSubscriberSnapshot(snapshot);

        if (!string.IsNullOrEmpty(errorMessage))
            return Result<int>.Failure(errorMessage);

        return Result<int>.Success(availableCopiesCount ?? 0);
    }

    public async Task<Result<RentalReturnDto>> GetReturnFormAsync(
        int id,
        CancellationToken ct = default
    )
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var rental = await _unitOfWork
            .Rentals.GetQueryable()
            .Where(r => r.Id == id && r.CreatedOnUtc.Date != DateTime.UtcNow.Date)
            .Select(r => new
            {
                r.Id,
                r.SubscriberId,
                r.StartDate,
                r.PenaltyPaid,
                ActiveCopies = r.RentalCopies
                    .Where(rc => !rc.ReturnDate.HasValue)
                    .Select(rc => new RentalCopyDto
                    {
                        BookCopy = new RentalBookCopyDto
                        {
                            Id = rc.BookCopy!.Id,
                            BookId = rc.BookCopy.BookId,
                            BookTitle = rc.BookCopy.Book!.Title,
                            BookImageUrl = rc.BookCopy.Book.ImageUrl,
                            BookThumbnailUrl = rc.BookCopy.Book.ImageThumbnailUrl,
                            EditionNumber = rc.BookCopy.EditionNumber,
                            SerialNumber = rc.BookCopy.SerialNumber,
                            IsAvailableForRental = rc.BookCopy.IsAvailableForRental,
                            IsDeleted = rc.BookCopy.IsDeleted,
                            CreatedOnUtc = rc.BookCopy.CreatedOnUtc,
                        },
                        RentalDate = rc.RentalDate,
                        EndDate = rc.EndDate,
                        ReturnDate = rc.ReturnDate,
                        ExtendedOn = rc.ExtendedOn,
                        IsReturned = rc.ExtendedOn.HasValue ? false : (bool?)null,
                    })
                    .ToList(),
                TotalDelayInDays = r.RentalCopies.Sum(rc =>
                    rc.ReturnDate.HasValue
                        ? (rc.ReturnDate.Value > rc.EndDate
                            ? rc.ReturnDate.Value.DayNumber - rc.EndDate.DayNumber
                            : 0)
                        : (today > rc.EndDate
                            ? today.DayNumber - rc.EndDate.DayNumber
                            : 0)
                ),
            })
            .SingleOrDefaultAsync(ct);

        if (rental is null)
            return Result<RentalReturnDto>.Failure("Rental not found.");

        var subscriberInfo = await _unitOfWork
            .Subscribers.GetQueryable()
            .Where(s => s.Id == rental.SubscriberId)
            .Select(s => new
            {
                s.IsBlackListed,
                LatestSubscriptionEndDate = s.Subscriptions.Max(sb => (DateOnly?)sb.EndDate),
            })
            .SingleOrDefaultAsync(ct);

        if (subscriberInfo is null)
            return Result<RentalReturnDto>.Failure("Rental not found.");

        return Result<RentalReturnDto>.Success(
            new RentalReturnDto
            {
                Id = rental.Id,
                PenalityPaid = rental.PenaltyPaid,
                RentalCopies = rental.ActiveCopies,
                TotalDelayInDays = rental.TotalDelayInDays,
                AllowExtend = Rental.ValidateExtensionEligibility(
                    subscriberInfo.IsBlackListed,
                    subscriberInfo.LatestSubscriptionEndDate,
                    rental.StartDate,
                    today
                ) == ExtensionEligibility.Eligible,
            }
        );
    }

    public async Task<RentalDto?> GetDetailsAsync(int id, CancellationToken ct = default)
    {
        return await _unitOfWork
            .Rentals.GetQueryable()
            .Where(r => r.Id == id)
            .ProjectTo<RentalDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(ct);
    }

    public async Task<IList<RentalBookCopyDto>> GetCopiesForDisplayAsync(
        IEnumerable<int> serialNumbers,
        CancellationToken ct = default
    )
    {
        var selectedSerials = serialNumbers.Distinct().ToList();

        if (selectedSerials.Count == 0)
            return [];

        var copies = await _unitOfWork
            .BookCopies.GetQueryable()
            .Where(c => selectedSerials.Contains(c.SerialNumber))
            .ProjectTo<RentalBookCopyDto>(_mapper.ConfigurationProvider)
            .ToDictionaryAsync(c => c.SerialNumber, ct);

        // Preserve the order dictated by the caller.
        return selectedSerials
            .Where(copies.ContainsKey)
            .Select(s => copies[s])
            .ToList();
    }

    public async Task<Result<RentalBookCopyDto>> GetCopyDetailsAsync(
        string value,
        CancellationToken ct = default
    )
    {
        if (!int.TryParse(value, out var serialNumber))
            return Result<RentalBookCopyDto>.Failure(Error.InvalidSerialNumber);

        
        var copy = await _unitOfWork
            .BookCopies.GetQueryable()
            .Where(c =>
                c.SerialNumber == serialNumber
                && !c.IsDeleted
                && !c.Book!.IsDeleted)
            .Select(c => new
            {
                Dto = new RentalBookCopyDto
                {
                    Id = c.Id,
                    BookId = c.BookId,
                    BookTitle = c.Book!.Title,
                    BookImageUrl = c.Book.ImageUrl,
                    BookThumbnailUrl = c.Book.ImageThumbnailUrl,
                    EditionNumber = c.EditionNumber,
                    SerialNumber = c.SerialNumber,
                    IsAvailableForRental = c.IsAvailableForRental,
                    IsDeleted = c.IsDeleted,
                    CreatedOnUtc = c.CreatedOnUtc,
                },
                c.IsAvailableForRental,
                BookIsAvailableForRental = c.Book!.IsAvailableForRental,
                
                IsInActiveRental = c.Rentals.Any(r => !r.ReturnDate.HasValue),
            })
            .SingleOrDefaultAsync(ct);

        if (copy is null)
            return Result<RentalBookCopyDto>.Failure(Error.InvalidSerialNumber);

        if (!copy.IsAvailableForRental || !copy.BookIsAvailableForRental)
            return Result<RentalBookCopyDto>.Failure(Error.NotAvailableForRental);

        if (copy.IsInActiveRental)
            return Result<RentalBookCopyDto>.Failure(Error.CopyIsInRental);

        return Result<RentalBookCopyDto>.Success(copy.Dto);
    }

    public async Task<Result<int>> CreateAsync(RentalFormDto dto, CancellationToken ct = default)
    {
        var validationResult = await _rentalFormValidator.ValidateAsync(dto, ct);

        if (!validationResult.IsValid)
            return Result<int>.Failure(validationResult.ToValidationErrors());

        var snapshot = await GetSubscriberSnapshotAsync(
            dto.SubscriberId,
            excludeRentalId: null,
            ct
        );

        if (snapshot is null)
            return Result<int>.Failure("Subscriber not found.");

        var (subscriberError, _) = ValidateSubscriberSnapshot(snapshot);

        if (!string.IsNullOrEmpty(subscriberError))
            return Result<int>.Failure(subscriberError);

        var (copiesError, rentalCopies) = await ValidateCopiesAsync(
            snapshot,
            dto.SelectedCopies,
            rentalId: null,
            ct
        );

        if (!string.IsNullOrEmpty(copiesError))
            return Result<int>.Failure(copiesError);

        var rental = new Rental
        {
            SubscriberId = dto.SubscriberId,
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            RentalCopies = rentalCopies,
        };

        _unitOfWork.Rentals.Add(rental);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<int>.Success(rental.Id);
    }

    public async Task<Result<int>> UpdateAsync(RentalFormDto dto, CancellationToken ct = default)
    {
        var validationResult = await _rentalFormValidator.ValidateAsync(dto, ct);

        if (!validationResult.IsValid)
            return Result<int>.Failure(validationResult.ToValidationErrors());

        var rental = await _unitOfWork
            .Rentals.GetQueryable(true)
            .Where(r => r.Id == dto.Id && r.CreatedOnUtc.Date == DateTime.UtcNow.Date)
            .Include(r => r.RentalCopies)
            .SingleOrDefaultAsync(ct);

        if (rental is null)
            return Result<int>.Failure("Rental not found.");

        var snapshot = await GetSubscriberSnapshotAsync(
            rental.SubscriberId,
            excludeRentalId: rental.Id,
            ct
        );

        if (snapshot is null)
            return Result<int>.Failure("Rental not found.");

        var (subscriberError, _) = ValidateSubscriberSnapshot(snapshot, rental.Id);

        if (!string.IsNullOrEmpty(subscriberError))
            return Result<int>.Failure(subscriberError);

        var (copiesError, editedCopies) = await ValidateCopiesAsync(
            snapshot,
            dto.SelectedCopies,
            rental.Id,
            ct
        );

        if (!string.IsNullOrEmpty(copiesError))
            return Result<int>.Failure(copiesError);

        rental.RentalCopies = editedCopies;
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<int>.Success(rental.Id);
    }

    public async Task<Result<int>> ReturnAsync(
        RentalReturnDto dto,
        CancellationToken ct = default
    )
    {
        var validationResult = await _rentalReturnValidator.ValidateAsync(dto, ct);

        if (!validationResult.IsValid)
            return Result<int>.Failure(validationResult.ToValidationErrors());

        var rental = await _unitOfWork
            .Rentals.GetQueryable(isTracking: true)
            .Where(r => r.Id == dto.Id && r.CreatedOnUtc.Date != DateTime.UtcNow.Date)
            .Include(r => r.RentalCopies)
            .SingleOrDefaultAsync(ct);

        if (rental is null)
            return Result<int>.Failure("Rental not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (dto.RentalCopies.Any(c => c.IsReturned == false))
        {
            var subscriberInfo = await _unitOfWork
                .Subscribers.GetQueryable()
                .Where(s => s.Id == rental.SubscriberId)
                .Select(s => new
                {
                    s.IsBlackListed,
                    LatestSubscriptionEndDate =
                        s.Subscriptions.Max(sb => (DateOnly?)sb.EndDate),
                })
                .SingleOrDefaultAsync(ct);

            if (subscriberInfo is null)
                return Result<int>.Failure("Rental not found.");

            var extendDeadline = rental.StartDate.AddDays(RentalConstants.MaxRentalDuration);

            var extensionEligibility = Rental.ValidateExtensionEligibility(
                subscriberInfo.IsBlackListed,
                subscriberInfo.LatestSubscriptionEndDate,
                rental.StartDate,
                today
            );

            string? error = extensionEligibility switch
            {
                ExtensionEligibility.SubscriberBlackListed => Error.ExtendNotAllowedForBlackListed,
                ExtensionEligibility.SubscriberInactive => Error.ExtendNotAllowedForInactive,
                ExtensionEligibility.NotAllowed => Error.ExtendNotAllowed,
                _ => null
            };

            if (!string.IsNullOrEmpty(error))
                return Result<int>.Failure(error);
        }

        var isUpdated = false;

        foreach (var copy in dto.RentalCopies)
        {
            if (copy.BookCopy == null || !copy.IsReturned.HasValue)
                continue;

            var current = rental.RentalCopies.SingleOrDefault(rc => rc.BookCopyId == copy.BookCopy.Id);
            if (current is null)
                continue;

            if (copy.IsReturned.Value)
            {
                if (current.ReturnDate.HasValue)
                    continue;

                current.ReturnDate = today;
                isUpdated = true;
            }
            else
            {
                if (current.ExtendedOn.HasValue)
                    continue;

                current.ExtendedOn = today;
                current.EndDate = current.RentalDate.AddDays(RentalConstants.MaxRentalDuration);
                isUpdated = true;
            }
        }

        if (isUpdated)
        {
            rental.PenaltyPaid = dto.PenalityPaid;
            await _unitOfWork.SaveChangesAsync(ct);
        }

        return Result<int>.Success(rental.Id);
    }

    public async Task<Result<int>> CancelAsync(int id, CancellationToken ct = default)
    {
        var rental = await _unitOfWork
            .Rentals.GetQueryable(isTracking: true)
            .Where(r => r.Id == id && r.CreatedOnUtc.Date == DateTime.UtcNow.Date)
            .Select(r => new { r.Id, r.IsDeleted, CopyCount = r.RentalCopies.Count })
            .SingleOrDefaultAsync(ct);

        if (rental is null)
            return Result<int>.Failure("Rental not found.");


        var stub = new Rental { Id = rental.Id, IsDeleted = true };
        _unitOfWork.Rentals.Attach(stub);
        _unitOfWork.Entry(stub).Property(r => r.IsDeleted).IsModified = true;

        await _unitOfWork.SaveChangesAsync(ct);

        return Result<int>.Success(rental.CopyCount);
    }


    private async Task<SubscriberSnapshot?> GetSubscriberSnapshotAsync(
        int subscriberId,
        int? excludeRentalId,
        CancellationToken ct
    )
    {
        return await _unitOfWork
            .Subscribers.GetQueryable()
            .Where(s => s.Id == subscriberId)
            .Select(s => new SubscriberSnapshot
            {
                Id = s.Id,
                IsBlackListed = s.IsBlackListed,

                LatestSubscriptionEndDate =
                    s.Subscriptions.Max(sb => (DateOnly?)sb.EndDate),

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

    private static (string? errorMessage, int? maxAllowedCopies) ValidateSubscriberSnapshot(
        SubscriberSnapshot snapshot,
        int? rentalId = null
    )
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
            return (error, null);

        var availableCopiesCount = RentalConstants.MaxAllowedCopies - snapshot.UnreturnedCopiesCount;
        return (null, availableCopiesCount);
    }


    private async Task<(string? errorMessage, ICollection<RentalCopy> rentalCopies )> ValidateCopiesAsync(
        SubscriberSnapshot snapshot,
        IEnumerable<int> serialNumbers,
        int? rentalId,
        CancellationToken ct
    )
    {
        var serialList = serialNumbers.Distinct().ToList();

    
        var copyData = await _unitOfWork
            .BookCopies.GetQueryable()
            .Where(c => serialList.Contains(c.SerialNumber))
            .Select(c => new
            {
                c.Id,
                c.BookId,
                BookTitle = c.Book!.Title,
                c.IsAvailableForRental,
                BookIsAvailableForRental = c.Book.IsAvailableForRental,
                IsExistingInRental = rentalId.HasValue
                    && c.Rentals.Any(r => r.RentalId == rentalId),
                IsInOtherActiveRental =
                    c.Rentals.Any(r =>
                        !r.ReturnDate.HasValue
                        && (!rentalId.HasValue || r.RentalId != rentalId)),
            })
            .ToListAsync(ct);

        var copies = new List<RentalCopy>(copyData.Count);

        foreach (var c in copyData)
        {
            if (c.IsExistingInRental)
            {
                copies.Add(new RentalCopy { BookCopyId = c.Id });
                continue;
            }

            if (!c.IsAvailableForRental || !c.BookIsAvailableForRental)
                return (Error.NotAvailableForRental, copies);

            if (c.IsInOtherActiveRental)
                return (Error.CopyIsInRental, copies);

            if (snapshot.ActiveBookIds.Contains(c.BookId))
                return ($"This subscriber already has a copy for '{c.BookTitle}' book", []);

            copies.Add(new RentalCopy { BookCopyId = c.Id });
        }

        return (null, copies);
    }



   
}