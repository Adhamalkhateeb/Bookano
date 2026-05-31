using Bookano.Web.ViewModels.Books;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookano.Web.ViewModels.Reports
{
    public class BooksReportViewModel
    {
        public IEnumerable<BookViewModel>? Books { get; set; } 

        public PaginatedViewModel? PaginatedViewModel { get; set; } 

        public IEnumerable<SelectListItem> Authors { get; set; } = [];

        [Display(Name = "Authors")]
        public IList<int>? SelectedAuthors { get; set; } = [];
        public IEnumerable<SelectListItem> Categories { get; set; } = [];

        [Display(Name = "Categories")]
        public IList<int>? SelectedCategories { get; set; } = [];
    }
}
