using System.Net.Mime;
using Bookano.Application.Services.Authors;
using Bookano.Application.Services.Categories;
using Bookano.Application.Services.Reports;
using Bookano.Web.Services.PDF;
using Bookano.Web.ViewModels.Books;
using Bookano.Web.ViewModels.Reports;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookano.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController(
        IReportsService reportsService,
        IAuthorService authorService,
        ICategoryService categoryService,
        IWebHostEnvironment webHostEnvironment,
        IMapper mapper,
        IViewRendererService viewRenderer,
        IExcelService excelService,
        IPdfService pdfService
    ) : Controller
    {
        private readonly IReportsService _reportsService = reportsService;
        private readonly IAuthorService _authorService = authorService;
        private readonly ICategoryService _categoryService = categoryService;
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
        private readonly IMapper _mapper = mapper;
        private readonly IViewRendererService _viewRenderer = viewRenderer;
        private readonly IExcelService _excelService = excelService;
        private readonly IPdfService _pdfService = pdfService;

        public IActionResult Index()
        {
            return View();
        }

        #region Books

        public async Task<IActionResult> Books(
            IList<int> selectedAuthors,
            IList<int> selectedCategories,
            int? pageNumber,
            CancellationToken ct
        )
        {
            var authors = await _authorService.GetActiveAsync(ct);
            var categories = await _categoryService.GetActiveAsync(ct);

            var viewModel = new BooksReportViewModel
            {
                Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors),
                Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories),
            };

            var page = pageNumber ?? 1;

            var booksData = await _reportsService.GetBooksReportAsync(
                selectedAuthors,
                selectedCategories,
                page,ReportsConfigurations.DefaultPageSize, ct);

            viewModel.Books = _mapper.Map<IEnumerable<BookViewModel>>(booksData.Items);
            viewModel.PaginatedViewModel = new()
            {
                PageNumber = booksData.PageNumber,
                TotalPages = booksData.TotalPages
            };

            return View(viewModel);
        }

        public async Task<IActionResult> ExportBooksToExcel(string authors, string categories, CancellationToken ct)
        {
            var (selectAuthors, selectCategories) = GetBooksSelectedFilters(authors, categories);
            var booksData = await _reportsService.GetBooksReportAsync(selectAuthors, selectCategories, ct);

            var fileBytes = _excelService.GenerateExcel(booksData, "Books");

            return File(
                fileBytes,
                MediaTypeNames.Application.Octet,
                $"Books_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx"
            );
        }

        public async Task<IActionResult> ExportBooksToPdf(string authors, string categories, CancellationToken ct)
        {
            var (selectAuthors, selectCategories) = GetBooksSelectedFilters(authors, categories);
            var booksData = await _reportsService.GetBooksReportAsync(selectAuthors, selectCategories, ct);

            var vm = _mapper.Map<IEnumerable<BookViewModel>>(booksData);

            var fileBytes = await _pdfService.GeneratePdfFromViewAsync(ControllerContext,"~/Views/Reports/BooksReport.cshtml",vm,landscape: true);

            return File(
                fileBytes,
                MediaTypeNames.Application.Octet,
                $"Books_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf"
            );
        }

        private static (
            IEnumerable<int> authors,
            IEnumerable<int> categories
        ) GetBooksSelectedFilters(string authors, string categories)
        {
            IEnumerable<int> selectedAuthors = string.IsNullOrEmpty(authors)
                ? []
                :
                [
                    .. authors
                        .Split(',')
                        .Select(s => int.TryParse(s.Trim(), out var id) ? (int?)id : null)
                        .Where(id => id.HasValue)
                        .Select(id => id!.Value),
                ];

            IEnumerable<int> selectedCategories = string.IsNullOrEmpty(categories)
                ? []
                :
                [
                    .. categories
                        .Split(',')
                        .Select(s => int.TryParse(s.Trim(), out var id) ? (int?)id : null)
                        .Where(id => id.HasValue)
                        .Select(id => id!.Value),
                ];

            return (selectedAuthors, selectedCategories);
        }

        #endregion

        #region Rentals

        public async Task<IActionResult> Rentals(string duration, int? pageNumber, CancellationToken ct)
        {
            var viewModel = new RentalsReportViewModel { Duration = duration };

            var page = pageNumber ?? 1;

            var result = await _reportsService.GetRentalsReportAsync(duration,page, ReportsConfigurations.DefaultPageSize, ct);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Duration", result.ErrorMessage!);
                return View(viewModel);
            }

            viewModel.Rentals = _mapper.Map<IEnumerable<RentalsReportItemViewModel>>(result.Value!.Items);
            viewModel.PaginatedViewModel = new()
            {
                PageNumber = result.Value.PageNumber,
                TotalPages = result.Value.TotalPages
            };

            ModelState.Clear();
            return View(viewModel);
        }

        public async Task<IActionResult> ExportRentalsToExcel(string duration, CancellationToken ct)
        {
            var result = await _reportsService.GetRentalsReportAsync(duration, ct);

            if (result.IsFailure)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction(nameof(Rentals), new { duration });
            }

            var rentals = _mapper.Map<List<RentalsReportItemViewModel>>(result.Value);
            var fileBytes = _excelService.GenerateExcel(rentals, "Rentals");

            return File(
                fileBytes,
                MediaTypeNames.Application.Octet,
                $"Rentals_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx"
            );
        }

        public async Task<IActionResult> ExportRentalsToPdf(string duration, CancellationToken ct)
        {
            var result = await _reportsService.GetRentalsReportAsync(duration, ct);

            if (result.IsFailure)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction(nameof(Rentals), new { duration });
            }

            var rentals = _mapper.Map<List<RentalsReportItemViewModel>>(result.Value);

            var pdf = await _pdfService.GeneratePdfFromViewAsync(
                ControllerContext,
                "~/Views/Reports/RentalsReport.cshtml",
                rentals,
                landscape: true
            );

            return File(pdf, MediaTypeNames.Application.Octet, $"Rentals_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf");
        }

        #endregion

        #region Delayed Rentals

        public async Task<IActionResult> DelayedRentals(CancellationToken ct)
        {
            var delayedRentalsData = await _reportsService.GetDelayedRentalsReportAsync(ct);
            var delayedRentals = _mapper.Map<List<DelayedRentalItemViewModel>>(delayedRentalsData);

            var viewModel = new DelayedRentalsViewModel { Rentals = delayedRentals };

            return View(viewModel);
        }

        public async Task<IActionResult> ExportDelayedRentalsToExcel(CancellationToken ct)
        {
            var delayedRentalsData = await _reportsService.GetDelayedRentalsReportAsync(ct);
            var delayedRentals = _mapper.Map<List<DelayedRentalItemViewModel>>(delayedRentalsData);

            var fileBytes = _excelService.GenerateExcel(delayedRentals, "Delayed Rentals");

            return File(
                fileBytes,
                MediaTypeNames.Application.Octet,
                $"Delayed_Rentals_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx"
            );
        }

        public async Task<IActionResult> ExportDelayedRentalsToPdf(CancellationToken ct)
        {
            var delayedRentalsData = await _reportsService.GetDelayedRentalsReportAsync(ct);
            var delayedRentals = _mapper.Map<List<DelayedRentalItemViewModel>>(delayedRentalsData);

            var pdf = await _pdfService.GeneratePdfFromViewAsync(
                ControllerContext,
                "~/Views/Reports/DelayedRentalsReport.cshtml",
                delayedRentals,
                landscape: true
            );

            return File(pdf, MediaTypeNames.Application.Octet, $"Rentals_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf");
        }

        #endregion
    }
}
