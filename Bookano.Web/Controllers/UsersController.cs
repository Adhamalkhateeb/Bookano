using Bookano.Application.DTOs.Users;
using Bookano.Application.Services.Users;
using Bookano.Web.Binders;
using Bookano.Web.ViewModels.Users;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookano.Web.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class UsersController(
        IUserService userService,
        IMapper mapper
    ) : Controller
    {
        private readonly IUserService _userService = userService;
        private readonly IMapper _mapper = mapper;

        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetUsers(CancellationToken ct)
        {
            var request = DataTableRequestBinder.Bind(Request.Form);

            var data = await _userService.GetPagedAsync(request, ct);

            var mappedData = _mapper.Map<IEnumerable<UserViewModel>>(data.Data);

            return Ok(
                new
                {
                    recordsTotal = data.RecordsTotal,
                    recordsFiltered = data.RecordsFiltered,
                    data = mappedData,
                }
            );
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var viewModel = new UserFormViewModel
            {
                Roles = await GetRolesSelectItemsAsync(ct),
            };
            return PartialView("_Form", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserFormViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var dto = _mapper.Map<UserFormDto>(model);

            var result = await _userService.CreateAsync(
                dto,
                (userId, code) => Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId, code },
                    protocol: Request.Scheme
                )!,
                ct
            );

            result.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return BadRequest();

            return Ok();
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Edit(string id, CancellationToken ct)
        {
            var userFormDto = await _userService.GetUserFormAsync(id, ct);

            if (userFormDto is null)
                return NotFound();

            var viewModel = _mapper.Map<UserFormViewModel>(userFormDto);
            viewModel.Roles = await GetRolesSelectItemsAsync(ct);

            return PartialView("_Form", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserFormViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var dto = _mapper.Map<UserFormDto>(model);

            var result = await _userService.UpdateAsync(dto, ct);

            result.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return BadRequest();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id, CancellationToken ct)
        {
            var result = await _userService.ToggleStatusAsync(id, ct);

            if (result.IsFailure)
                return NotFound();

            return Ok(result.Value);
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> ResetPassword(string id, CancellationToken ct)
        {
            var userFormDto = await _userService.GetUserFormAsync(id, ct);

            if (userFormDto is null)
                return NotFound();

            var viewModel = new ResetPasswordFormViewModel { Id = userFormDto.Id! };

            return PartialView("_ResetPassword", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordFormViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var dto = _mapper.Map<UserResetPasswordDto>(model);

            var result = await _userService.ResetPasswordAsync(dto, ct);

            result.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return BadRequest();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Unlock(string id, CancellationToken ct)
        {
            var result = await _userService.UnlockAsync(id, ct);

            if (result.IsFailure)
                return NotFound();

            return Ok();
        }

        public async Task<IActionResult> AllowUserName(UserFormViewModel model, CancellationToken ct)
        {
            var isAllowed = await _userService.IsUserNameUniqueAsync(model.UserName, model.Id, ct);

            return Json(isAllowed);
        }

        public async Task<IActionResult> AllowEmail(UserFormViewModel model, CancellationToken ct)
        {
            var isAllowed = await _userService.IsEmailUniqueAsync(model.Email, model.Id, ct);

            return Json(isAllowed);
        }

        private async Task<IEnumerable<SelectListItem>> GetRolesSelectItemsAsync(CancellationToken ct)
        {
            var roles = await _userService.GetRolesAsync(ct);
            return roles.Select(r => new SelectListItem { Text = r, Value = r });
        }
    }
}
