// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Bookano.Web.Core.Models;
using Bookano.Web.Services.Image;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing.Matching;
using NuGet.Common;

namespace Bookano.Web.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IImageService _imageService;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            [FromKeyedServices("local")] IImageService imageService
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _imageService = imageService;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

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
            ///
            [
                Required,
                MaxLength(100, ErrorMessage = Error.MaxLength),
                Display(Name = "Full Name"),
                RegularExpression(
                    RegexPatterns.CharactersOnly_Eng,
                    ErrorMessage = Error.OnlyEnglishLetters
                )
            ]
            public string FullName { get; set; } = null!;

            [Phone]
            [
                Display(Name = "Phone number"),
                MaxLength(11, ErrorMessage = Error.MaxLength),
                RegularExpression(
                    RegexPatterns.MobileNumber,
                    ErrorMessage = Error.InvalidMobileNumber
                )
            ]
            public string PhoneNumber { get; set; }

            public IFormFile? Avatar { get; set; }

            public bool RemoveImage { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel { PhoneNumber = phoneNumber, FullName = user.FullName };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            if (Input.Avatar is not null)
            {
                var imageValidationError = _imageService.ValidateImage(Input.Avatar);
                if (imageValidationError is not null)
                {
                    ModelState.AddModelError("Input.Avatar", imageValidationError);
                    await LoadAsync(user);
                    return Page();
                }

                await _imageService.DeleteAsync($"users/{user.Id}.png");
                var imageUploadResult = await _imageService.UploadAsync(
                    Input.Avatar,
                    "users",
                    $"{user.Id}.png"
                );

                if (!imageUploadResult.IsSuccess)
                {
                    ModelState.AddModelError("Input.Avatar", imageUploadResult.ErrorMessage);
                    await LoadAsync(user);
                    return Page();
                }
            }
            else if (Input.RemoveImage)
            {
                var deleteImageResult = await _imageService.DeleteAsync($"users/{user.Id}.png");
                if (!deleteImageResult.IsSuccess)
                {
                    ModelState.AddModelError("Input.Avatar", deleteImageResult.ErrorMessage);
                    await LoadAsync(user);
                    return Page();
                }
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(
                    user,
                    Input.PhoneNumber
                );
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            if (Input.FullName != user.FullName)
            {
                user.FullName = Input.FullName;
                var setfullNameResult = await _userManager.UpdateAsync(user);
                if (!setfullNameResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set full name.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
