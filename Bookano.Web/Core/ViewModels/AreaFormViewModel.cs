using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookano.Web.Core.ViewModels
{
    public class AreaFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Area")]
        [Remote(
            "AllowItem",
            null!,
            AdditionalFields = $"{nameof(Id)},{nameof(GovernorateId)}",
            ErrorMessage = Error.AreaAlreadyExists
        )]
        public string Name { get; set; } = null!;

        [Display(Name = "Governorate")]
        public int GovernorateId { get; set; }
        public IEnumerable<SelectListItem>? Governorates { get; set; }
    }
}
