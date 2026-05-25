using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookano.Web.Core.ViewModels
{
    public class BooksReportViewModel
    {
        public PaginatedList<BookViewModel>? Books { get; set; }
        public IEnumerable<SelectListItem> Authors { get; set; } = [];

        [Display(Name = "Authors")]
        public IList<int>? SelectedAuthors { get; set; } = [];
        public IEnumerable<SelectListItem> Categories { get; set; } = [];

        [Display(Name = "Categories")]
        public IList<int>? SelectedCategories { get; set; } = [];
    }
}
