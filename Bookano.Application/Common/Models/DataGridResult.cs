namespace Bookano.Application.Common.Models;

public class DataGridResult<T>
{
    public IEnumerable<T> Data { get; set; } = [];
    public int TotalCount { get; set; }
    public int FilteredCount { get; set; }
}
