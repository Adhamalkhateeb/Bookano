using Microsoft.AspNetCore.Mvc.Rendering;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookano.Web.Core.ViewModels
{
    public class BookFormViewModel
    {
        public int Id { get; set; }

        [Remote("AllowItem", null!, AdditionalFields = nameof(Id), ErrorMessage = Error.Duplicated)]
        [MaxLength(20, ErrorMessage = Error.MaxLength), Display(Name = "ISBN")]
        public string? Isbn { get; set; }

        [MaxLength(255, ErrorMessage = Error.MaxLength)]
        public string Title { get; set; } = null!;

        [Display(Name = "Publisher")]
        public int PublisherId { get; set; }
        public IEnumerable<SelectListItem>? Publishers { get; set; }

        [Display(Name = "Publishing Date")]
        [AssertThat("PublishingDate <= Today()", ErrorMessage = Error.NotAllowFutureDates)]
        public DateTime PublishingDate { get; set; } = DateTime.Now;
        public IFormFile? Image { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }

        public bool RemoveImage { get; set; }

        [MaxLength(50, ErrorMessage = Error.MaxLength)]
        public string Hall { get; set; } = null!;

        [Display(Name = "Is available for rental?")]
        public bool IsAvailableForRental { get; set; }
        public string Description { get; set; } = null!;

        [Display(Name = "Categories")]
        public IList<int> SelectedCategories { get; set; } = [];
        public IEnumerable<SelectListItem>? Categories { get; set; }

        [Display(Name = "Authors")]
        public IList<int> SelectedAuthors { get; set; } = [];
        public IEnumerable<SelectListItem>? Authors { get; set; }
    }
}
