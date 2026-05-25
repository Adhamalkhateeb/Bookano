using Bookano.Domain.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Bookano.Infrastructure.Persistence.Interceptors
{
    public sealed class AuditInterceptor(ICurrentUserService currentUserService)
        : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result
        )
        {
            Audit(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default
        )
        {
            Audit(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void Audit(DbContext? context)
        {
            if (context is null)
                return;

            var now = DateTimeOffset.UtcNow;
            var userId = _currentUserService.UserId;

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.Entity is BaseEntity baseEntity)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            baseEntity.CreatedById = userId!;
                            baseEntity.CreatedOnUtc = now;
                            break;

                        case EntityState.Modified:
                            baseEntity.LastUpdatedById = userId;
                            baseEntity.LastUpdatedOnUtc = now;
                            break;
                    }
                }

                if (entry.Entity is ApplicationUser appUser)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            appUser.CreatedById = userId;
                            appUser.CreatedOnUtc = now;
                            break;

                        case EntityState.Modified:
                            appUser.LastUpdatedById = userId;
                            appUser.LastUpdatedOnUtc = now;
                            break;
                    }
                }

                if (entry.Entity is Subscription subscription)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            subscription.CreatedById = userId;
                            subscription.CreatedOnUtc = now;
                            break;
                    }
                }
            }
        }
    }
}
