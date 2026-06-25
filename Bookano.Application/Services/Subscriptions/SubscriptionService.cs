using Bookano.Application.Common.Interfaces;
using Bookano.Application.DTOs.Subscribers;
using Microsoft.Extensions.Logging;

namespace Bookano.Application.Services.Subscriptions;

public class SubscriptionService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ISubscriberNotificationService subscriberNotificationService,
    ILogger<SubscriptionService> logger
) : ISubscriptionService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly ISubscriberNotificationService _subscriberNotificationService = subscriberNotificationService;
    private readonly ILogger<SubscriptionService> _logger = logger;

    public async Task<IEnumerable<SubscriptionDto>> GetBySubscriberAsync(int subscriberId, CancellationToken ct = default)
    {
        return await _unitOfWork.Subscriptions
            .GetQueryable(withTracking: false)
            .Where(s => s.SubscriberId == subscriberId)
            .OrderByDescending(s => s.EndDate)
            .ProjectTo<SubscriptionDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<Result<SubscriptionDto>> RenewAsync(int subscriberId, CancellationToken ct = default)
    {
        var data = await _unitOfWork
            .Subscribers.GetQueryable(withTracking: true)
            .Where(s => s.Id == subscriberId)
            .Select(s => new
            {
                subscriber = s,
                s.IsBlackListed,
                LastEndDate = (DateOnly?)s.Subscriptions.Max(x => x.EndDate),
            })
            .FirstOrDefaultAsync(ct);

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

        return _mapper.Map<SubscriptionDto>(newSubscription);
    }

    public async Task<IEnumerable<SubscriberDto>> GetSubscribersWithSubscriptionAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _unitOfWork.Subscribers.GetQueryable()
            .Where(s => !s.IsDeleted && s.Subscriptions.Any(sub => sub.EndDate >= today))
            .ProjectTo<SubscriberDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<SubscriberDto>> GetExpiredSubscribersAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _unitOfWork.Subscribers.GetQueryable()
            .Where(s => !s.IsDeleted && !s.Subscriptions.Any(sub => sub.EndDate >= today))
            .ProjectTo<SubscriberDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<SubscriberDto>> GetSubscribersNearingExpiryAsync(int days = 7, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var targetDate = today.AddDays(days);
        
        return await _unitOfWork.Subscribers.GetQueryable()
            .Where(s => !s.IsDeleted && 
                        s.Subscriptions.Any() && 
                        s.Subscriptions.Max(sub => sub.EndDate) >= today && 
                        s.Subscriptions.Max(sub => sub.EndDate) <= targetDate)
            .ProjectTo<SubscriberDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
}
