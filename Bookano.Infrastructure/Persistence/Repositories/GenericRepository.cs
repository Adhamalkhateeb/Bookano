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

        public void Attach(T entity)
        {
            _context.Attach(entity);
        }

        public async Task<int> CountAsync(
            Expression<Func<T, bool>> expression,
            CancellationToken cancellationToken = default
        )
        {

            return await _context.Set<T>().CountAsync(expression,cancellationToken);
        }


        public async Task<IEnumerable<T>> FindAllAsync(
            Expression<Func<T, bool>> expression,
            bool withTracking = true,
            CancellationToken cancellationToken = default
        )
        {
            IQueryable<T> query = _context.Set<T>().Where(expression);

            if (!withTracking)
                query = query.AsNoTracking();

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<T?> FindAsync(
            Expression<Func<T, bool>> expression,
            bool withTracking = true,
            CancellationToken cancellationToken = default
        )
        {
            IQueryable<T> query = _context.Set<T>();

            if (!withTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(expression, cancellationToken);
        }



        public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().FindAsync(id, cancellationToken);
        }

        public IQueryable<T> GetQueryable(bool withTracking = true)
        {
            IQueryable<T> query = _context.Set<T>();

            if (!withTracking)
                query = query.AsNoTracking();

            return query;
        }

        public async Task<bool> IsExistsAsync(
            Expression<Func<T, bool>> expression,
            CancellationToken cancellationToken = default
        ) => await _context.Set<T>().AnyAsync(expression,cancellationToken);

        public void Remove(T entity) => _context.Remove(entity);

    }
}
