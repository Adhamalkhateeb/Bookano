using Microsoft.AspNetCore.Mvc.Rendering;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookano.Web.Core.ViewModels
{
    public class BookFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(255, ErrorMessage = Error.MaxLength)]
        [Remote(
            "AllowItem",
            null!,
            AdditionalFields = $"{nameof(Id)},{nameof(AuthorId)}",
            ErrorMessage = Error.DuplicatedBook
        )]
        public string Title { get; set; } = null!;

        [Display(Name = "Author")]
        [Remote(
            "AllowItem",
            null!,
            AdditionalFields = $"{nameof(Id)},{nameof(Title)}",
            ErrorMessage = Error.DuplicatedBook
        )]
        public int AuthorId { get; set; }
        public IEnumerable<SelectListItem>? Authors { get; set; }

        [Display(Name = "Publisher")]
        public int PublisherId { get; set; }
        public IEnumerable<SelectListItem>? Publishers { get; set; }

        [Display(Name = "Publishing Date")]
        [AssertThat("PublishingDate <= Today()", ErrorMessage = Error.NotAllowFutureDates)]
        public DateTime PublishingDate { get; set; } = DateTime.Now;
        public IFormFile? Image { get; set; }
        public string? ImageUrl { get; set; }

        [MaxLength(100, ErrorMessage = Error.MaxLength)]
        public string Hall { get; set; } = null!;

        [Display(Name = "Is available for rental?")]
        public bool IsAvailableForRental { get; set; }
        public string Description { get; set; } = null!;

        [Display(Name = "Categories")]
        public IList<int> SelectedCategories { get; set; } = [];
        public IEnumerable<SelectListItem>? Categories { get; set; }
    }
}
