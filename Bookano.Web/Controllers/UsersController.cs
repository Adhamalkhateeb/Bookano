using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookano.Web.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IMapper mapper
        )
        {
            _userManager = userManager;
            _mapper = mapper;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                "CreatedOnUtc",
                "LastUpdatedOnUtc",
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
                usersQuery = usersQuery.Where(b =>
                    b.UserName!.Contains(searchValue) || b.Email!.Contains(searchValue)
                );
            }

            usersQuery = usersQuery.OrderBy($"{sortColumn} {sortDirection}");

            var filteredRecords = await usersQuery.CountAsync();

            var result = await usersQuery
                .Skip(skip)
                .Take(pageSize)
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    UserName = u.UserName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    IsDeleted = u.IsDeleted,
                    CreatedOn = u.CreatedOnUtc,
                    LastUpdatedOn = u.LastUpdatedOnUtc,
                })
                .ToListAsync();

            return Ok(
                new
                {
                    recordsTotal = totalRecords,
                    recordsFiltered = filteredRecords,
                    data = result,
                }
            );
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Create()
        {
            var viewModel = new UserFormViewModel
            {
                Roles = await _roleManager
                    .Roles.Select(r => new SelectListItem { Text = r.Name, Value = r.Name })
                    .ToListAsync(),
            };
            return PartialView("_Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                UserName = model.UserName,
                Email = model.Email,
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value,
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                return Ok();
            }

            return BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
                return NotFound();

            var viewModel = _mapper.Map<UserFormViewModel>(user);
            viewModel.SelectedRoles = await _userManager.GetRolesAsync(user);
            viewModel.Roles = await _roleManager
                .Roles.Select(r => new SelectListItem { Text = r.Name, Value = r.Name })
                .ToListAsync();

            return PartialView("_Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.FindByIdAsync(model.Id!);

            if (user is null)
                return NotFound();

            user = _mapper.Map(model, user);
            user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            user.LastUpdatedOnUtc = DateTimeOffset.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                var rolesUpdated = !currentRoles.SequenceEqual(model.SelectedRoles);

                if (rolesUpdated)
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                }

                await _userManager.UpdateSecurityStampAsync(user);

                return Ok();
            }

            return BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
                return NotFound();

            user.IsDeleted = !user.IsDeleted;
            var updatedOn = DateTimeOffset.UtcNow;
            user.LastUpdatedOnUtc = updatedOn;
            user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            await _userManager.UpdateAsync(user);
            if (user.IsDeleted)
                await _userManager.UpdateSecurityStampAsync(user);

            return Ok(updatedOn.ToString("o"));
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
                return NotFound();

            var viewModel = new ResetPasswordFormViewModel { Id = user.Id };

            return PartialView("_ResetPassword", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.FindByIdAsync(model.Id!);

            if (user is null)
                return NotFound();

            var currentPasswordHash = user.PasswordHash;

            await _userManager.RemovePasswordAsync(user);
            var result = await _userManager.AddPasswordAsync(user, model.Password);

            if (result.Succeeded)
            {
                user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                user.LastUpdatedOnUtc = DateTimeOffset.UtcNow;

                await _userManager.UpdateAsync(user);
                return Ok();
            }

            user.PasswordHash = currentPasswordHash;
            await _userManager.UpdateAsync(user);

            return BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
                return NotFound();

            var isLockedOut = await _userManager.IsLockedOutAsync(user);

            if (isLockedOut)
                await _userManager.SetLockoutEndDateAsync(user, null);

            return Ok();
        }

        public async Task<IActionResult> AllowUserName(UserFormViewModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            var isAllowed = user is null || user.Id.Equals(model.Id);

            return Json(isAllowed);
        }

        public async Task<IActionResult> AllowEmail(UserFormViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            var isAllowed = user is null || user.Id.Equals(model.Id);

            return Json(isAllowed);
        }
    }
}
