using Microsoft.EntityFrameworkCore.Query;

namespace Bookano.Application.Common.Models;

public class PaginatedList<T>
{
    public IReadOnlyList<T> Items { get; }
    public int PageNumber { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    private PaginatedList(IReadOnlyList<T> items, int count, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = count;
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
    }

    public static async Task<PaginatedList<T>> CreateAsync( IQueryable<T> source, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);
        
        var count = source.Provider is IAsyncQueryProvider ? await source.CountAsync(ct) : source.Count();

        source = source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        var items = source.Provider is IAsyncQueryProvider ? await source.ToListAsync(ct) : source.ToList();

        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}
