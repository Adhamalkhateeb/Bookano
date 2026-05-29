using System.Linq.Dynamic.Core;

namespace Bookano.Application.Common;


public class DataTableQueryBuilder<T>
    where T : class
{
    private IQueryable<T> _baseQuery = null!;
    private IQueryable<T> _filteredQuery = null!;
    private readonly IMapper _mapper;

    private string _search = "";
    private string _sortColumn = "Id";
    private string _sortDirection = "asc";
    private int _skip;
    private int _take;


    private readonly List<string> _allowedSortColumns = new();

    public DataTableQueryBuilder(IMapper mapper)
    {
        _mapper = mapper;
    }

    public DataTableQueryBuilder<T> For(IQueryable<T> query)
    {
        _baseQuery = query;
        _filteredQuery = query;
        return this;
    }

    public DataTableQueryBuilder<T> AllowSorting(params string[] columns)
    {
        _allowedSortColumns.AddRange(columns);
        return this;
    }

    public DataTableQueryBuilder<T> WithRequest(DataTableRequest request)
    {
        _search = request.Search ?? "";
        _skip = request.Skip;
        _take = request.PageSize;

        _sortColumn = request.SortColumn;
        _sortDirection = request.SortDirection;

        return this;
    }

    public DataTableQueryBuilder<T> Search(Func<IQueryable<T>, string, IQueryable<T>> searchFunc)
    {
        if (!string.IsNullOrWhiteSpace(_search))
        {
            _filteredQuery = searchFunc(_filteredQuery, _search);
        }

        return this;
    }

    public DataTableQueryBuilder<T> Sort()
    {
        var column = _allowedSortColumns.Contains(_sortColumn,StringComparer.OrdinalIgnoreCase)
            ? _sortColumn
            : "Id";

        var direction = _sortDirection.ToLower() == "desc" ? "desc" : "asc";

        _filteredQuery = _filteredQuery.OrderBy($"{column} {direction}");

        return this;
    }


    public async Task<DataTableResult<TDto>> ExecuteAsync<TDto>(
        CancellationToken ct = default)
    {
        var total = await _baseQuery.CountAsync(ct);

        var filtered = await _filteredQuery.CountAsync(ct);

        var data = await _filteredQuery
            .Skip(_skip)
            .Take(_take)
            .ProjectTo<TDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return new DataTableResult<TDto>
        {
            RecordsTotal = total,
            RecordsFiltered = filtered,
            Data = data
        };
    }

    public async Task<(int total, int filtered, List<T>)> ExecuteRawAsync(
        CancellationToken ct = default)
    {
        var total = await _filteredQuery.CountAsync(ct);

        var data = await _filteredQuery.ToListAsync(ct);

        return (total, total, data);
    }
}
