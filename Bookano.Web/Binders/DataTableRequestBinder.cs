
using Bookano.Application.Common.Models;

namespace Bookano.Web.Binders
{
    public class DataTableRequestBinder
    {
        public static DataTableRequest Bind(IFormCollection form)
        {
            int skip = int.TryParse(form["start"], out var s) ? s : 0;

            int pageSize =
                int.TryParse(form["length"], out var l) && l > 0 ? l : 10;

            var search = form["search[value]"].ToString();

            var sortIndex = int.TryParse(form["order[0][column]"], out var i) ? i : 0;

            var requestedColumn = form[$"columns[{sortIndex}][name]"].ToString();

            var isDesc = string.Equals(
                form["order[0][dir]"],
                "desc",
                StringComparison.OrdinalIgnoreCase);

            return new DataTableRequest
            {
                Skip = skip,
                PageSize = pageSize,
                Search = search,
                SortColumn = requestedColumn,
                SortDirection = isDesc ? "desc" : "asc"
            };
        }
    }
}
