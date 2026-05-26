using Microsoft.AspNetCore.Mvc.Rendering;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookano.Web.Core.ViewModels
{
    public class BookFormViewModel
    {
        public int Id { get; set; }

        [Remote("AllowItem", null!, AdditionalFields = nameof(Id), ErrorMessage = Error.Duplicated)]
        [Display(Name = "ISBN")]
        public string? Isbn { get; set; }
        public string Title { get; set; } = null!;

        [Display(Name = "Publisher")]
        public int PublisherId { get; set; }
        public IEnumerable<SelectListItem>? Publishers { get; set; }

        [Display(Name = "Publishing Date")]
        [AssertThat("PublishingDate <= Today()", ErrorMessage = Error.NotAllowFutureDates)]
        public DateTime PublishingDate { get; set; } = DateTime.Today;
        public IFormFile? Image { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }
        public string? ExistingImagePublicId { get; set; }
        public bool RemoveImage { get; set; }
        public string IdempotencyKey { get; set; } = Guid.NewGuid().ToString();
        public byte[]? RowVersion { get; set; }

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
