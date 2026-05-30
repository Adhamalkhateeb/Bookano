using Bookano.Application.Common.Interfaces;
using Bookano.Application.DTOs.Subscribers;
using Bookano.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bookano.Application.Services.Subscribers;

public sealed class SubscriberService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    [FromKeyedServices("local")] IImageService imageService,
    ISubscriberNotificationService subscriberNotificationService,
    IValidator<SubscriberFormDto> validator,
    ILogger<SubscriberService> logger
) : ISubscriberService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IImageService _imageService = imageService;
    private readonly ISubscriberNotificationService _subscriberNotificationService =
        subscriberNotificationService;
    private readonly IValidator<SubscriberFormDto> _validator = validator;
    private readonly ILogger<SubscriberService> _logger = logger;

    public async Task<SubscriberSearchResultDto?> SearchAsync(
        string value,
        CancellationToken ct = default
    )
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return await _unitOfWork
            .Subscribers.GetQueryable()
            .AsNoTracking()
            .Where(s => s.MobileNumber == value || s.NationalId == value || s.Email == value)
            .ProjectTo<SubscriberSearchResultDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(ct);
    }

    public async Task<SubscriberDto?> GetDetails(int id, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return await _unitOfWork
            .Subscribers.GetQueryable()
            .AsSplitQuery()
            .Where(s => s.Id == id)
            .Select(s => new SubscriberDto
            {
                Id = s.Id,
                FullName = $"{s.FirstName} {s.LastName}",
                DateOfBirth = s.DateOfBirth,
                NationalId = s.NationalId,
                MobileNumber = s.MobileNumber,
                Email = s.Email,
                ImageUrl = s.ImageUrl,
                ImageThumbnailUrl = s.ImageThumbnailUrl,
                Area = s.Area!.Name,
                Governorate = s.Area.Governorate!.Name,
                Address = s.Address,
                IsBlackListed = s.IsBlackListed,
                CreatedOnUtc = s.CreatedOnUtc,
                Subscriptions = s
                    .Subscriptions.OrderByDescending(subscription => subscription.EndDate)
                    .Select(subscription => new SubscriptionDto
                    {
                        Id = subscription.Id,
                        StartDate = subscription.StartDate,
                        EndDate = subscription.EndDate,
                        CreatedOnUtc = subscription.CreatedOnUtc,
                    }),
                Rentals = s
                    .Rentals.OrderByDescending(rental => rental.CreatedOnUtc)
                    .Select(rental => new SubscriberRentalDto
                    {
                        Id = rental.Id,
                        StartDate = rental.StartDate,
                        CreatedOnUtc = rental.CreatedOnUtc,
                        NumberOfCopies = rental.RentalCopies.Count(),
                        ActiveCopies = rental.RentalCopies.Count(copy => copy.ReturnDate == null),
                        TotalDelayInDays = rental.RentalCopies.Sum(copy =>

    copy.ReturnDate.HasValue

        ? (
            copy.ReturnDate.Value.DayNumber > copy.EndDate.DayNumber

                ? copy.ReturnDate.Value.DayNumber - copy.EndDate.DayNumber

                : 0
          )

        : (
            today.DayNumber > copy.EndDate.DayNumber

                ? today.DayNumber - copy.EndDate.DayNumber

                : 0
          )
),
                    }),
            })
            .SingleOrDefaultAsync(ct);
    }

    public Task<SubscriberFormDto?> GetFormAsync(int id, CancellationToken ct = default)
    {
        return _unitOfWork
            .Subscribers.GetQueryable()
            .Where(s => s.Id == id)
            .ProjectTo<SubscriberFormDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(ct);
    }

    public async Task<Result<int>> CreateAsync(
        SubscriberFormDto dto,
        CancellationToken ct = default
    )
    {
        var validationResult = await _validator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
            return Result<int>.Failure(validationResult.ToValidationErrors());

        var uniquenessErrors = await ValidateUniquenessAsync(0, dto, ct);
        if (uniquenessErrors.Count > 0)
            return Result<int>.Failure(uniquenessErrors);

        var subscriber = _mapper.Map<Subscriber>(dto);

        string? uploadedImageId = null;

        if (dto.Image is not null)
        {
            await using var stream = dto.Image.Stream;
            var uploadResult = await _imageService.UploadAsync(
                stream,
                dto.Image.FileName,
                "subscribers",
                null,
                ct
            );

            if (!uploadResult.IsSuccess)
                return Result<int>.Failure(
                    new List<ValidationError>
                    {
                        new ValidationError(nameof(dto.Image), uploadResult.ErrorMessage!),
                    }
                );

            uploadedImageId = uploadResult.PublicId;
            subscriber.ImageUrl = uploadResult.Url!;
            subscriber.ImageThumbnailUrl = _imageService.GetThumbnail(uploadResult.PublicId!);
            subscriber.ImagePublicId = uploadResult.PublicId!;
        }
        else
        {
            return Result<int>.Failure(
                new List<ValidationError> { new(nameof(dto.Image), Error.RequiredField) }
            );
        }

        subscriber.Subscriptions.Add(
            new Subscription
            {
                StartDate = DateOnly.FromDateTime(DateTime.Today),
                EndDate = DateOnly.FromDateTime(
                    DateTime.Today.AddDays((int)SubscriptionType.Yearly)
                ),
            }
        );

        _unitOfWork.Subscribers.Add(subscriber);

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            if (!string.IsNullOrEmpty(uploadedImageId))
                await _imageService.DeleteAsync(uploadedImageId, ct);

            return Result<int>.Failure("Could not create subscriber.");
        }

        try
        {
            await _subscriberNotificationService.SendWelcomeAsync(subscriber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome notification.");
        }

        return Result<int>.Success(subscriber.Id);
    }

    public async Task<Result<int>> UpdateAsync(
        SubscriberFormDto dto,
        CancellationToken ct = default
    )
    {
        var validationResult = await _validator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
            return Result<int>.Failure(validationResult.ToValidationErrors());

        var subscriber = await _unitOfWork.Subscribers.GetByIdAsync(dto.Id, ct);

        if (subscriber is null)
            return Result<int>.Failure("Subscriber not found.");

        var uniquenessErrors = await ValidateUniquenessAsync(dto.Id, dto, ct);
        if (uniquenessErrors.Count > 0)
            return Result<int>.Failure(uniquenessErrors);

        var oldImagePublicId = subscriber.ImagePublicId;
        ImageUploadResult? uploadResult = null;

        if (dto.Image is not null)
        {
            await using var stream = dto.Image.Stream;
            uploadResult = await _imageService.UploadAsync(
                stream,
                dto.Image.FileName,
                "subscribers",
                null,
                ct
            );

            if (!uploadResult.IsSuccess)
                return Result<int>.Failure(
                    new List<ValidationError>
                    {
                        new ValidationError(nameof(dto.Image), uploadResult.ErrorMessage!),
                    }
                );
        }

        _mapper.Map(dto, subscriber);

        if (uploadResult?.IsSuccess == true)
        {
            subscriber.ImageUrl = uploadResult.Url!;
            subscriber.ImageThumbnailUrl = _imageService.GetThumbnail(uploadResult.PublicId!);
            subscriber.ImagePublicId = uploadResult.PublicId!;
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            if (uploadResult?.IsSuccess == true && !string.IsNullOrEmpty(uploadResult.PublicId))
                await _imageService.DeleteAsync(uploadResult.PublicId, ct);

            return Result<int>.Failure("Could not update subscriber.");
        }

        if (uploadResult?.IsSuccess == true && !string.IsNullOrEmpty(oldImagePublicId))
            await _imageService.DeleteAsync(oldImagePublicId, ct);

        return Result<int>.Success(subscriber.Id);
    }

    public async Task<Result<SubscriptionDto>> RenewSubscriptionAsync(
        int id,
        CancellationToken ct = default
    )
    {
        var data = await _unitOfWork
            .Subscribers.GetQueryable(true)
            .Where(s => s.Id == id)
            .Select(s => new
            {
                subscriber = s,
                s.IsBlackListed,
                LastEndDate = (DateOnly?)s.Subscriptions.Max(x => x.EndDate),
            })
            .SingleOrDefaultAsync(ct);

        if (data is null)
            return Result<SubscriptionDto>.Failure("Subscriber not found.");

        if (data.IsBlackListed)
            return Result<SubscriptionDto>.Failure(Error.BlackListedSubscriber);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var startDate =
            data.LastEndDate is null || today > data.LastEndDate
                ? today
                : data.LastEndDate.Value.AddDays(1);

        var newSubscription = new Subscription
        {
            Subscriber = data.subscriber,
            StartDate = startDate,
            EndDate = startDate.AddYears(1),
        };

        _unitOfWork.Subscriptions.Add(newSubscription);
        await _unitOfWork.SaveChangesAsync(ct);

        try
        {
            await _subscriberNotificationService.SendSubscriptionRenewalAsync(newSubscription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send renewal notification.");
        }

        return Result<SubscriptionDto>.Success(_mapper.Map<SubscriptionDto>(newSubscription));
    }

    public async Task<bool> IsEmailAvailableAsync(
        int id,
        string email,
        CancellationToken ct = default
    )
    {
        return !await _unitOfWork.Subscribers.IsExistsAsync(
            x => x.Email == email && x.Id != id,
            ct
        );
    }

    public async Task<bool> IsMobileNumberAvailableAsync(
        int id,
        string mobileNumber,
        CancellationToken ct = default
    )
    {
        return !await _unitOfWork.Subscribers.IsExistsAsync(
            x => x.MobileNumber == mobileNumber && x.Id != id,
            ct
        );
    }

    public async Task<bool> IsNationalIdAvailableAsync(
        int id,
        string nationalId,
        CancellationToken ct = default
    )
    {
        return !await _unitOfWork.Subscribers.IsExistsAsync(
            x => x.NationalId == nationalId && x.Id != id,
            ct
        );
    }

    private async Task<List<ValidationError>> ValidateUniquenessAsync(
        int id,
        SubscriberFormDto dto,
        CancellationToken ct
    )
    {
        var errors = new List<ValidationError>();

        if (!await IsEmailAvailableAsync(id, dto.Email, ct))
            errors.Add(
                new ValidationError(nameof(dto.Email), string.Format(Error.Duplicated, "Email"))
            );

        if (!await IsMobileNumberAvailableAsync(id, dto.MobileNumber, ct))
            errors.Add(
                new ValidationError(
                    nameof(dto.MobileNumber),
                    string.Format(Error.Duplicated, "Mobile Number")
                )
            );

        if (!await IsNationalIdAvailableAsync(id, dto.NationalId, ct))
            errors.Add(
                new ValidationError(
                    nameof(dto.NationalId),
                    string.Format(Error.Duplicated, "National ID")
                )
            );

        return errors;
    }
}
