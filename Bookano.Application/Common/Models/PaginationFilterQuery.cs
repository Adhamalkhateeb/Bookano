namespace Bookano.Application.Common.Models;

public sealed class PaginationFilterQuery
{
    public int Skip { get; set; }
    public int PageSize { get; set; }
    public string? Search { get; set; }
    public string SortColumn { get; set; } = "Id";
    public string SortDirection { get; set; } = "asc";
}