using Bookano.Application.Interfaces;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Bookano.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    private IGenericRepository<Author>? _authors;
    private IGenericRepository<Category>? _categories;
    private IGenericRepository<Area>? _areas;
    private IGenericRepository<Book>? _books;
    private IGenericRepository<BookCopy>? _bookCopies;
    private IGenericRepository<Governorate>? _governorates;
    private IGenericRepository<Publisher>? _publishers;
    private IGenericRepository<Rental>? _rentals;
    private IGenericRepository<RentalCopy>? _rentalCopies;
    private IGenericRepository<Subscriber>? _subscribers;
    private IGenericRepository<Subscription>? _subscriptions;

    public IGenericRepository<Author> Authors =>
        _authors ??= new GenericRepository<Author>(_context);

    public IGenericRepository<Category> Categories =>
        _categories ??= new GenericRepository<Category>(_context);

    public IGenericRepository<Area> Areas =>
        _areas ??= new GenericRepository<Area>(_context);

    public IGenericRepository<Book> Books =>
        _books ??= new GenericRepository<Book>(_context);

    public IGenericRepository<BookCopy> BookCopies =>
        _bookCopies ??= new GenericRepository<BookCopy>(_context);

    public IGenericRepository<Governorate> Governorates =>
        _governorates ??= new GenericRepository<Governorate>(_context);

    public IGenericRepository<Publisher> Publishers =>
        _publishers ??= new GenericRepository<Publisher>(_context);

    public IGenericRepository<Rental> Rentals =>
        _rentals ??= new GenericRepository<Rental>(_context);

    public IGenericRepository<RentalCopy> RentalCopies =>
        _rentalCopies ??= new GenericRepository<RentalCopy>(_context);

    public IGenericRepository<Subscriber> Subscribers =>
        _subscribers ??= new GenericRepository<Subscriber>(_context);

    public IGenericRepository<Subscription> Subscriptions =>
        _subscriptions ??= new GenericRepository<Subscription>(_context);

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public EntityEntry<TEntity> Entry<TEntity>(TEntity entity)
        where TEntity : class
    {
        return _context.Entry(entity);
    }

}