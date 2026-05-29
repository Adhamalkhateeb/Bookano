namespace Bookano.Application.Common.Models;

public class DataTableResult<T>
{
    public IEnumerable<T> Data { get; set; } = [];
    public int RecordsTotal { get; set; }
    public int RecordsFiltered { get; set; }
}
