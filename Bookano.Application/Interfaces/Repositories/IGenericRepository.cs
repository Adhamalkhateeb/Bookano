using System.Linq.Expressions;

public interface IGenericRepository<T>
    where T : class
{
    IQueryable<T> GetQueryable(bool withTracking = true);

    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<T?> FindAsync(
        Expression<Func<T, bool>> expression,
        bool withTracking = true,
        CancellationToken cancellationToken = default
    );

    Task<IEnumerable<T>> FindAllAsync(
        Expression<Func<T, bool>> expression,
        bool withTracking = true,
        CancellationToken cancellationToken = default
    );

    T Add(T entity);
    IEnumerable<T> AddRange(IEnumerable<T> entities);
    void Remove(T entity);

    Task<bool> IsExistsAsync(
        Expression<Func<T, bool>> expression,
        CancellationToken cancellationToken = default
    );
    Task<int> CountAsync(
        Expression<Func<T, bool>> expression,
        CancellationToken cancellationToken = default
    );

    void Attach(T entity);

}
