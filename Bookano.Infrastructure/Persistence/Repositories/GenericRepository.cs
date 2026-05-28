using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Bookano.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<T>(ApplicationDbContext context) : IGenericRepository<T>
        where T : class
    {
        private readonly ApplicationDbContext _context = context;

        public T Add(T entity)
        {
            _context.Add(entity);
            return entity;
        }

        public IEnumerable<T> AddRange(IEnumerable<T> entities)
        {
            _context.AddRange(entities);
            return entities;
        }

        public async Task<int> CountAsync() => await _context.Set<T>().CountAsync();

        public async Task<int> CountAsync(Expression<Func<T, bool>> expression)
        {
            return await _context.Set<T>().CountAsync(expression);
        }

        public async Task DeleteBulkAsync(Expression<Func<T, bool>> expression) =>
            await _context.Set<T>().Where(expression).ExecuteDeleteAsync();

        public async Task<IEnumerable<T>> FindAllAsync(
            Expression<Func<T, bool>> expression,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>>? orderBy = null,
            bool orderByDescending = false,
            bool isTracking = false,
            CancellationToken cancellationToken = default
        )
        {
            IQueryable<T> query = _context.Set<T>().Where(expression);

            if (!isTracking)
                query = query.AsNoTracking();

            if (orderBy is not null)
            {
                if (orderByDescending)
                    query = query.OrderByDescending(orderBy);
                else
                    query = query.OrderBy(orderBy);
            }

            if (skip.HasValue)
                query = query.Skip(skip.Value);

            if (take.HasValue)
                query = query.Take(take.Value);

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<T>> FindAllAsync(
            Expression<Func<T, bool>> expression,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? includes = null,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>>? orderBy = null,
            bool orderByDescending = false,
            bool isTracking = false,
            CancellationToken cancellationToken = default
        )
        {
            IQueryable<T> query = _context.Set<T>();

            if (includes is not null)
                query = includes(query);

            if (!isTracking)
                query = query.AsNoTracking();

            query = query.Where(expression);

            if (orderBy is not null)
            {
                if (orderByDescending)
                    query = query.OrderByDescending(orderBy);
                else
                    query = query.OrderBy(orderBy);
            }

            if (skip.HasValue)
                query = query.Skip(skip.Value);

            if (take.HasValue)
                query = query.Take(take.Value);

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<T>> FindAllAsync(
            Expression<Func<T, bool>> expression,
            bool isTracking = false,
            CancellationToken cancellationToken = default
        )
        {
            IQueryable<T> query = _context.Set<T>().Where(expression);

            if (!isTracking)
                query = query.AsNoTracking();

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<T?> FindAsync(
            Expression<Func<T, bool>> expression,
            bool isTracking = false,
            CancellationToken cancellationToken = default
        )
        {
            IQueryable<T> query = _context.Set<T>();

            if (!isTracking)
                query = query.AsNoTracking();

            return await query.SingleOrDefaultAsync(expression, cancellationToken);
        }

        public Task<T?> FindAsync(
            Expression<Func<T, bool>> expression,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? includes = null,
            bool isTracking = false,
            CancellationToken cancellationToken = default
        )
        {
            IQueryable<T> query = _context.Set<T>();

            if (includes is not null)
                query = includes(query);

            if (!isTracking)
                query = query.AsNoTracking();

            return query.SingleOrDefaultAsync(expression, cancellationToken);
        }

        public async Task<IEnumerable<T>> GetAllAsync(
            bool isTracked = false,
            CancellationToken cancellationToken = default
        )
        {
            IQueryable<T> query = _context.Set<T>();

            if (!isTracked)
                query = query.AsNoTracking();

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().FindAsync(id, cancellationToken);
        }

        public IQueryable<T> GetQueryable(bool isTracked = false)
        {
            IQueryable<T> query = _context.Set<T>();

            if (!isTracked)
                query = query.AsNoTracking();

            return query;
        }

        public async Task<bool> IsExistsAsync(Expression<Func<T, bool>> expression) =>
            await _context.Set<T>().AnyAsync(expression);

        public void Remove(T entity) => _context.Remove(entity);

        public void Update(T entity) => _context.Update(entity);
    }
}
