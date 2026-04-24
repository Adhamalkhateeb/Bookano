using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Identity;

namespace Bookano.Web.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public UsersController(UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> GetUsers()
        {
            int skip = int.TryParse(Request.Form["start"], out var parsedSkip) ? parsedSkip : 0;
            int pageSize =
                int.TryParse(Request.Form["length"], out var parsedPageSize) && parsedPageSize > 0
                    ? parsedPageSize
                    : 10;
            var searchValue = Request.Form["search[value]"].ToString();

            var sortColumnIndex = int.TryParse(
                Request.Form["order[0][column]"],
                out var parsedSortColumnIndex
            )
                ? parsedSortColumnIndex
                : 0;

            var allowedSortColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Id",
                "FullName",
                "UserName",
                "Email",
                "IsDeleted",
                "CreatedOn",
                "LastUpdatedOn",
            };

            var requestedColumn = Request.Form[$"columns[{sortColumnIndex}][name]"].ToString();
            var sortColumn = allowedSortColumns.Contains(requestedColumn) ? requestedColumn : "Id";

            var isDescending = string.Equals(
                Request.Form["order[0][dir]"].ToString(),
                "desc",
                StringComparison.OrdinalIgnoreCase
            );
            var sortDirection = isDescending ? "desc" : "asc";

            var usersQuery = _userManager.Users.AsNoTracking().AsQueryable();

            var totalRecords = await usersQuery.CountAsync();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                usersQuery = usersQuery.Where(b => b.UserName!.Contains(searchValue));
            }

            usersQuery = usersQuery.OrderBy($"{sortColumn} {sortDirection}");

            var filteredRecords = await usersQuery.CountAsync();

            var result = await usersQuery.Skip(skip).Take(pageSize).ToListAsync();

            var mappedData = _mapper.Map<IEnumerable<UserViewModel>>(result);

            return Ok(
                new
                {
                    recordsTotal = totalRecords,
                    recordsFiltered = filteredRecords,
                    data = mappedData,
                }
            );
        }
    }
}
