
using Bookano.Application.Common.Models;

namespace Bookano.Web.Binders
{
    public class DataTableRequestBinder
    {
        public static PaginationFilterQuery Bind(IFormCollection form)
        {
            var sortIndex = int.Parse(form["order[0][column]"]!);

            return new PaginationFilterQuery
            {
                Skip = int.Parse(form["start"]!),
                PageSize = int.Parse(form["length"]!),
                Search = form["search[value]"],
                SortColumn = form[$"columns[{sortIndex}][name]"]!,
                SortDirection = form["order[0][dir]"]!
            };
        }
    }
}
