using Bookano.Domain.Common;
using Bookano.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Bookano.Infrastructure.Persistence.Interceptors
{
    public sealed class AuditableInterceptor(ICurrentUserService currentUserService)
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
                if (entry.Entity is IAuditable auditable)
                {
                    if (entry.State == EntityState.Added)
                    {
                        auditable.CreatedById = userId;
                        auditable.CreatedOnUtc = now;
                    }
                    if (entry.State == EntityState.Modified)
                    {
                        auditable.LastUpdatedById = userId;
                        auditable.LastUpdatedOnUtc = now;
                    }
                }

            }
        }
    }
}
