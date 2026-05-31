// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Bookano.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResendEmailConfirmationModel(
        UserManager<ApplicationUser> userManager,
        IUserNotificationService userNotificationService
    ) : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IUserNotificationService _userNotificationService = userNotificationService;

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public string Username { get; set; }
        }

        public void OnGet(string username)
        {
            Input = new InputModel { Username = username };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var normalizedUserName = Input.Username.ToUpper();
            var user = await _userManager.Users.SingleOrDefaultAsync(u =>
                (
                    u.NormalizedUserName == normalizedUserName
                    || u.NormalizedEmail == normalizedUserName
                ) && !u.IsDeleted
            );

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Verification email sent. Please check your email."
                );
                return Page();
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new
                {
                    area = "Identity",
                    userId = user.Id,
                    code,
                },
                protocol: Request.Scheme
            );

            await _userNotificationService.SendEmailConfirmationAsync(
                user.Email,
                user.FullName,
                HtmlEncoder.Default.Encode(callbackUrl!)
            );

            ModelState.AddModelError(
                string.Empty,
                "Verification email sent. Please check your email."
            );
            return Page();
        }
    }
}
