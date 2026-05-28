using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Bookano.Application.Interfaces;

public interface IUnitOfWork
{
    IGenericRepository<Area> Areas { get; }
    IGenericRepository<Author> Authors { get; }
    IGenericRepository<Book> Books { get; }
    IGenericRepository<BookCopy> BookCopies { get; }
    IGenericRepository<Category> Categories { get; }
    IGenericRepository<Governorate> Governorates { get; }
    IGenericRepository<Publisher> Publishers { get; }
    IGenericRepository<Rental> Rentals { get; }
    IGenericRepository<RentalCopy> RentalCopies { get; }
    IGenericRepository<Subscriber> Subscribers { get; }
    IGenericRepository<Subscription> Subscriptions { get; }

    EntityEntry<TEntity> Entry<TEntity>(TEntity entity)
       where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
}
