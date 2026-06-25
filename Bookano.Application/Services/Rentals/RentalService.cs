using Bookano.Application.DTOs.Dashboard;
using Bookano.Application.DTOs.Rentals;
using Bookano.Application.DTOs.BookCopies;

namespace Bookano.Application.Services.Rentals;

public class RentalService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IRentalValidationService validationService,
    IValidator<RentalSaveDto> rentalFormValidator,
    IValidator<RentalReturnDto> rentalReturnValidator
) : IRentalService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IRentalValidationService _validationService = validationService;
    private readonly IValidator<RentalSaveDto> _rentalFormValidator = rentalFormValidator;
    private readonly IValidator<RentalReturnDto> _rentalReturnValidator = rentalReturnValidator;


    public async Task<IEnumerable<ChartItemDto>> GetRentalsPerDayAsync(
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        CancellationToken ct = default)
    {
        var start = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-29));
        var end = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var data = await _unitOfWork
            .RentalCopies.GetQueryable()
            .Where(rc => rc.RentalDate >= start && rc.RentalDate <= end)
            .GroupBy(rc => rc.RentalDate)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var figures = new List<ChartItemDto>();

        for (var day = start; day <= end; day = day.AddDays(1))
        {
            var count = data.FirstOrDefault(d => d.Date == day)?.Count ?? 0;
            figures.Add(
                new ChartItemDto
                {
                    Label = day.ToString("d MMM"),
                    Value = count.ToString(),
                }
            );
        }

        return figures;
    }

    public async Task<IEnumerable<RentalDto>> GetBySubscriberAsync(int subscriberId, CancellationToken ct = default)
    {
        return await _unitOfWork
            .Rentals.GetQueryable(withTracking: false)
            .Where(r => r.SubscriberId == subscriberId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedOnUtc)
            .ProjectTo<RentalDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }


    public async Task<RentalDto?> GetDetailsAsync(int id, CancellationToken ct = default)
    {
        return await _unitOfWork
            .Rentals.GetQueryable(withTracking: false)
            .Where(r => r.Id == id)
            .ProjectTo<RentalDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(ct);
    }

    public async Task<Result<BookCopyDto>> GetCopyReadyForRentalAsync(string value,CancellationToken ct = default
    )
    {
        if (!int.TryParse(value, out var serialNumber))
            return Result<BookCopyDto>.Failure(Error.InvalidSerialNumber);

        
        var copy = await _unitOfWork
            .BookCopies.GetQueryable()
            .Where(c =>
                c.SerialNumber == serialNumber
                && !c.IsDeleted
                && !c.Book!.IsDeleted)
            .Select(c => new
            {
                Dto = new BookCopyDto
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
            return Result<BookCopyDto>.Failure(Error.InvalidSerialNumber);

        if (!copy.IsAvailableForRental || !copy.BookIsAvailableForRental)
            return Result<BookCopyDto>.Failure(Error.NotAvailableForRental);

        if (copy.IsInActiveRental)
            return Result<BookCopyDto>.Failure(Error.CopyIsInRental);

        return copy.Dto;
    }

    public async Task<Result<int>> CreateAsync(RentalSaveDto dto, CancellationToken ct = default)
    {
        var validationResult = await _rentalFormValidator.ValidateAsync(dto, ct);

        if (!validationResult.IsValid)
            return Result<int>.Failure(validationResult.ToValidationErrors());

        var eligibilityResult = await _validationService.CheckSubscriberEligibilityAsync(dto.SubscriberId, null, ct);
        if (!eligibilityResult.IsSuccess)
            return Result<int>.Failure(eligibilityResult.ErrorMessage!);

        var copiesResult = await _validationService.ValidateCopiesForRentalAsync(
            dto.SubscriberId,
            null,
            dto.SelectedCopies,
            null,
            ct
        );

        if (!copiesResult.IsSuccess)
            return Result<int>.Failure(copiesResult.ErrorMessage!);

        var rental = new Rental
        {
            SubscriberId = dto.SubscriberId,
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            RentalCopies = copiesResult.Value!,
        };

        _unitOfWork.Rentals.Add(rental);
        await _unitOfWork.SaveChangesAsync(ct);

        return rental.Id;
    }

    public async Task<Result<int>> UpdateAsync(RentalSaveDto dto, CancellationToken ct = default)
    {
        var validationResult = await _rentalFormValidator.ValidateAsync(dto, ct);

        if (!validationResult.IsValid)
            return Result<int>.Failure(validationResult.ToValidationErrors());

        var rental = await _unitOfWork
            .Rentals.GetQueryable(withTracking: true)
            .Where(r => r.Id == dto.Id && r.CreatedOnUtc.Date == DateTime.UtcNow.Date)
            .Include(r => r.RentalCopies)
            .SingleOrDefaultAsync(ct);

        if (rental is null)
            return Result<int>.Failure("Rental not found.");

        var eligibilityResult = await _validationService.CheckSubscriberEligibilityAsync(rental.SubscriberId, rental.Id, ct);
        if (!eligibilityResult.IsSuccess)
            return Result<int>.Failure(eligibilityResult.ErrorMessage!);

        var copiesResult = await _validationService.ValidateCopiesForRentalAsync(
            rental.SubscriberId,
            rental.Id,
            dto.SelectedCopies,
            rental.Id,
            ct
        );

        if (!copiesResult.IsSuccess)
            return Result<int>.Failure(copiesResult.ErrorMessage!);

        rental.RentalCopies = copiesResult.Value!;
        await _unitOfWork.SaveChangesAsync(ct);

        return rental.Id;
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
            .Rentals.GetQueryable(withTracking: true)
            .Where(r => r.Id == dto.Id && r.CreatedOnUtc.Date != DateTime.UtcNow.Date)
            .Include(r => r.RentalCopies)
            .SingleOrDefaultAsync(ct);

        if (rental is null)
            return Result<int>.Failure("Rental not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var delayResult = await _validationService.CalculateReturnPenaltyAsync(rental.Id, ct);
        if (delayResult.IsSuccess && delayResult.Value > 0 && !dto.PenalityPaid)
            return Result<int>.Failure(Error.PenalityShouldBePaid);

        if (dto.RentalCopies.Any(c => c.IsReturned == false))
        {
            var allowExtendResult = await _validationService.CanExtendRentalAsync(rental.Id, ct);
            if (!allowExtendResult.IsSuccess || !allowExtendResult.Value)
                return Result<int>.Failure(allowExtendResult.ErrorMessage ?? "Not allowed to extend.");
        }

        var isUpdated = false;

        foreach (var copy in dto.RentalCopies)
        {
            if (!copy.IsReturned.HasValue)
                continue;

            var current = rental.RentalCopies.SingleOrDefault(rc => rc.BookCopyId == copy.BookCopyId);
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

        return rental.Id;
    }

    public async Task<Result<int>> CancelAsync(int id, CancellationToken ct = default)
    {
        var rental = await _unitOfWork
            .Rentals.GetQueryable(withTracking: true)
            .Where(r => r.Id == id && r.CreatedOnUtc.Date == DateTime.UtcNow.Date)
            .Select(r => new { r.Id, r.IsDeleted, CopyCount = r.RentalCopies.Count })
            .SingleOrDefaultAsync(ct);

        if (rental is null)
            return Result<int>.Failure("Rental not found.");


        var stub = new Rental { Id = rental.Id, IsDeleted = true };
        _unitOfWork.Rentals.Attach(stub);
        _unitOfWork.Entry(stub).Property(r => r.IsDeleted).IsModified = true;

        await _unitOfWork.SaveChangesAsync(ct);

        return rental.CopyCount;
    }


    public async Task<int> GetTotalRentedCopiesAsync(CancellationToken ct = default)
    {
        return await _unitOfWork.RentalCopies.CountAsync(rc => !rc.ReturnDate.HasValue, ct);
    }


    public async Task<IEnumerable<RentalCopyDto>?> GetCopyRentalHistoryAsync(int copyId, CancellationToken ct = default)
    {
        var copyExists = await _unitOfWork.BookCopies.IsExistsAsync(c => c.Id == copyId, ct);
        if (!copyExists)
            return null;

        var history = await _unitOfWork
            .RentalCopies.GetQueryable(withTracking: false)
            .Where(rc => rc.BookCopy!.Id == copyId)
            .ProjectTo<RentalCopyDto>(_mapper.ConfigurationProvider)
            .OrderByDescending(c => c.RentalDate)
            .ToListAsync(ct);

        return history;
    }






}