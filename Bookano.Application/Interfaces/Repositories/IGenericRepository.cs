using System.Linq.Expressions;

public interface IGenericRepository<T>
    where T : class
{
    IQueryable<T> GetQueryable(bool isTracking = false);

    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<T?> FindAsync(
        Expression<Func<T, bool>> expression,
        bool isTracking = false,
        CancellationToken cancellationToken = default
    );

    Task<IEnumerable<T>> FindAllAsync(
        Expression<Func<T, bool>> expression,
        bool isTracking = false,
        CancellationToken cancellationToken = default
    );

    T Add(T entity);
    IEnumerable<T> AddRange(IEnumerable<T> entities);
    void Update(T entity);
    void Remove(T entity);

    Task<bool> IsExistsAsync(
        Expression<Func<T, bool>> expression,
        CancellationToken cancellationToken = default
    );
    Task<int> CountAsync(
        Expression<Func<T, bool>>? expression = null,
        CancellationToken cancellationToken = default
    );
}
