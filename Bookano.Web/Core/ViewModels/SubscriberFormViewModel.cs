using Microsoft.AspNetCore.Mvc.Rendering;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookano.Web.Core.ViewModels
{
    public class SubscriberFormViewModel
    {
        public string? Key { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; } = null!;

        [Display(Name = "Last Name")]
        public string LastName { get; set; } = null!;

        [Display(Name = "Date Of Birth")]
        [AssertThat("DateOfBirth <= Today()", ErrorMessage = Error.NotAllowFutureDates)]
        public DateTime DateOfBirth { get; set; } = DateTime.Today;

        [Display(Name = "National ID")]
        [Remote(
            "AllowNationalId",
            null!,
            AdditionalFields = nameof(Key),
            ErrorMessage = Error.Duplicated
        )]
        public string NationalId { get; set; } = null!;

        [Remote(
            "AllowEmail",
            null!,
            AdditionalFields = nameof(Key),
            ErrorMessage = Error.Duplicated
        )]
        public string Email { get; set; } = null!;

        [Display(Name = "Mobile Number")]
        [Remote(
            "AllowMobileNumber",
            null!,
            AdditionalFields = nameof(Key),
            ErrorMessage = Error.Duplicated
        )]
        public string MobileNumber { get; set; } = null!;

        [Display(Name = "Has WhatsApp?")]
        public bool HasWhatsApp { get; set; }

        [Display(Name = "Governorate")]
        public int GovernorateId { get; set; }
        public IEnumerable<SelectListItem>? Governorates { get; set; }

        [Display(Name = "Area")]
        public int AreaId { get; set; }
        public IEnumerable<SelectListItem>? Areas { get; set; } = [];

        public string Address { get; set; } = null!;

        [RequiredIf("Key == ''", ErrorMessage = Error.EmptyImage)]
        public IFormFile? Image { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }
    }
}
