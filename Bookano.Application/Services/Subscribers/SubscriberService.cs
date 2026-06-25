using Bookano.Application.Common.Interfaces;
using Bookano.Application.DTOs.Dashboard;
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
    IValidator<SubscriberSaveDto> validator,
    ILogger<SubscriberService> logger
) : ISubscriberService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IImageService _imageService = imageService;
    private readonly ISubscriberNotificationService _subscriberNotificationService =
        subscriberNotificationService;
    private readonly IValidator<SubscriberSaveDto> _validator = validator;
    private readonly ILogger<SubscriberService> _logger = logger;


    public async Task<int> GetActiveSubscribersCountAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _unitOfWork.Subscribers.CountAsync(
            s => !s.IsDeleted && !s.IsBlackListed && s.Subscriptions.Any(sub => sub.EndDate >= today), ct);
    }

    public async Task<SubscriberDto?> SearchAsync(string value,CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return await _unitOfWork
            .Subscribers.GetQueryable(withTracking:false)
            .Where(s => s.MobileNumber == value || s.NationalId == value || s.Email == value)
            .ProjectTo<SubscriberDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(ct);
    }

    public async Task<SubscriberDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _unitOfWork
            .Subscribers.GetQueryable(withTracking: false)
            .Where(s => s.Id == id)
            .ProjectTo<SubscriberDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(ct);
    }

    public async Task<Result<int>> CreateAsync(SubscriberSaveDto dto,CancellationToken ct = default)
    {
        var validationError = await ValidateSubscriberSaveDtoAsync(dto, ct);
        if (validationError is not null) return Result<int>.Failure(validationError);

        var subscriber = _mapper.Map<Subscriber>(dto);

        if (dto.Image is null)
            return Result<int>.Failure(new List<ValidationError> { new(nameof(dto.Image), Error.RequiredField) });

        var (uploadResult, oldImagePublicId) = await ProcessImageUploadAsync(subscriber, dto, ct);
        if (uploadResult?.IsSuccess == false)
            return Result<int>.Failure(new List<ValidationError> { new(nameof(dto.Image), uploadResult.ErrorMessage!) });

        subscriber.Subscriptions.Add(new Subscription
        {
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays((int)SubscriptionType.Yearly)),
        });

        return await PersistCreateAsync(subscriber, uploadResult?.PublicId, ct);
    }

    public async Task<Result<int>> UpdateAsync(SubscriberSaveDto dto,CancellationToken ct = default)
    {
        var validationError = await ValidateSubscriberSaveDtoAsync(dto, ct);
        if (validationError is not null) return Result<int>.Failure(validationError);

        var subscriber = await _unitOfWork.Subscribers.GetByIdAsync(dto.Id, ct);
        if (subscriber is null) return Result<int>.Failure("Subscriber not found.");

        _mapper.Map(dto, subscriber);

        var (uploadResult, oldImagePublicId) = await ProcessImageUploadAsync(subscriber, dto, ct);
        if (uploadResult?.IsSuccess == false)
            return Result<int>.Failure(new List<ValidationError> { new(nameof(dto.Image), uploadResult.ErrorMessage!) });

        return await PersistUpdateAsync(subscriber, uploadResult?.PublicId, oldImagePublicId, ct);
    }

    public async Task<bool> CanRentAsync(int subscriberId, CancellationToken ct = default)
    {
        var subscriberInfo = await _unitOfWork.Subscribers.GetQueryable(withTracking: false)
            .Where(s => s.Id == subscriberId && !s.IsDeleted)
            .Select(s => new {
                s.IsBlackListed,
                LatestSubscriptionEndDate = s.Subscriptions.Max(sb => sb.EndDate),
                UnreturnedCopiesCount = s.Rentals.SelectMany(r => r.RentalCopies).Count(rc => rc.ReturnDate == null)
            })
            .SingleOrDefaultAsync(ct);

        if (subscriberInfo == null) return false;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var eligibility = Subscriber.ValidateRentalEligibility(
            subscriberInfo.IsBlackListed,
            subscriberInfo.LatestSubscriptionEndDate,
            subscriberInfo.UnreturnedCopiesCount,
            today
        );

        return eligibility == RentalEligibility.Eligible;
    }

    private async Task<IEnumerable<ValidationError>?> ValidateSubscriberSaveDtoAsync(SubscriberSaveDto dto, CancellationToken ct)
    {
        var validationResult = await _validator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid) return validationResult.ToValidationErrors();

        var uniquenessErrors = await ValidateUniquenessAsync(dto.Id, dto, ct);
        if (uniquenessErrors.Count > 0) return uniquenessErrors;

        return null;
    }

    private async Task<(ImageUploadResult? UploadResult, string? OldImagePublicId)> ProcessImageUploadAsync(
        Subscriber subscriber, SubscriberSaveDto dto, CancellationToken ct)
    {
        if (dto.Image is null) return (null, null);

        var oldImagePublicId = subscriber.ImagePublicId;

        await using var stream = dto.Image.Stream;
        var uploadResult = await _imageService.UploadAsync(stream, dto.Image.FileName, "subscribers", null, ct);

        if (uploadResult.IsSuccess)
        {
            subscriber.ImageUrl = uploadResult.Url!;
            subscriber.ImageThumbnailUrl = _imageService.GetThumbnail(uploadResult.PublicId!);
            subscriber.ImagePublicId = uploadResult.PublicId!;
        }

        return (uploadResult, oldImagePublicId);
    }

    private async Task<Result<int>> PersistCreateAsync(Subscriber subscriber, string? uploadedImageId, CancellationToken ct)
    {
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

        return subscriber.Id;
    }

    private async Task<Result<int>> PersistUpdateAsync(Subscriber subscriber, string? newUploadedPublicId, string? oldImagePublicId, CancellationToken ct)
    {
        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            if (!string.IsNullOrEmpty(newUploadedPublicId))
                await _imageService.DeleteAsync(newUploadedPublicId, ct);

            return Result<int>.Failure("Could not update subscriber.");
        }

        if (!string.IsNullOrEmpty(newUploadedPublicId) && !string.IsNullOrEmpty(oldImagePublicId))
            await _imageService.DeleteAsync(oldImagePublicId, ct);

        return subscriber.Id;
    }

    public async Task<bool> IsEmailAvailableAsync(string email,int excludId,CancellationToken ct = default)
    {
        return !await _unitOfWork.Subscribers.IsExistsAsync(x => x.Email == email && x.Id != excludId, ct);
    }

    public async Task<bool> IsMobileNumberAvailableAsync(string mobileNumber, int excludId, CancellationToken ct = default)
    {
        return !await _unitOfWork.Subscribers.IsExistsAsync(x => x.MobileNumber == mobileNumber && x.Id != excludId, ct);
    }

    public async Task<bool> IsNationalIdAvailableAsync( string nationalId,int excludId, CancellationToken ct = default)
    {
        return !await _unitOfWork.Subscribers.IsExistsAsync(x => x.NationalId == nationalId && x.Id != excludId,ct);
    }

    private async Task<List<ValidationError>> ValidateUniquenessAsync(
        int id,
        SubscriberSaveDto dto,
        CancellationToken ct
    )
    {
        var errors = new List<ValidationError>();

        if (!await IsEmailAvailableAsync(dto.Email,id, ct))
            errors.Add(new ValidationError(nameof(dto.Email), string.Format(Error.Duplicated, "Email")));

        if (!await IsMobileNumberAvailableAsync(dto.MobileNumber,id, ct))
            errors.Add(
                new ValidationError(
                    nameof(dto.MobileNumber),
                    string.Format(Error.Duplicated, "Mobile Number")
                )
            );

        if (!await IsNationalIdAvailableAsync(dto.NationalId,id, ct))
            errors.Add(
                new ValidationError(
                    nameof(dto.NationalId),
                    string.Format(Error.Duplicated, "National ID")
                )
            );

        return errors;
    }

    public async Task<IEnumerable<SubscriberDto>> GetSubscribersWithRentalsAsync(CancellationToken ct = default)
    {
        return await _unitOfWork.Subscribers.GetQueryable()
            .Where(s => !s.IsDeleted && s.Rentals.Any(r => r.RentalCopies.Any(rc => !rc.ReturnDate.HasValue)))
            .ProjectTo<SubscriberDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }



    public async Task<IEnumerable<SubscriberDto>> GetSubscribersWithOverdueRentalsAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _unitOfWork.Subscribers.GetQueryable(withTracking: false)
            .Where(s => !s.IsDeleted && s.Rentals.Any(r => r.RentalCopies.Any(rc => !rc.ReturnDate.HasValue && rc.EndDate < today)))
            .ProjectTo<SubscriberDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ChartItemDto>> GetSubscribersPerGovernorateAsync(CancellationToken ct = default)
    {
        return await _unitOfWork
            .Subscribers.GetQueryable()
            .Where(s => !s.IsDeleted)
            .GroupBy(s => new { GovernorateName = s.Area!.Governorate!.Name })
            .Select(g => new ChartItemDto
            {
                Label = g.Key.GovernorateName,
                Value = g.Count().ToString(),
            })
            .ToListAsync(ct);
    }

}

