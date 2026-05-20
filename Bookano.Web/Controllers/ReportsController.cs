using System.Globalization;
using System.Net.Mime;
using Bookano.Web.Extensions;
using Bookano.Web.Services.PDF;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc.Rendering;
using OpenHtmlToPdf;

namespace Bookano.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMapper _mapper;
        private readonly IViewRendererService _viewRenderer;
        private readonly int excelDataStartRow = 10;

        public ReportsController(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            IMapper mapper,
            IViewRendererService viewRenderer
        )
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _mapper = mapper;
            _viewRenderer = viewRenderer;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region Books

        public async Task<IActionResult> Books(
            IList<int> selectedAuthors,
            IList<int> selectedCategories,
            int? pageNumber
        )
        {
            var authors = await _context.Authors.OrderBy(a => a.Name).AsNoTracking().ToListAsync();
            var categories = await _context
                .Categories.OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();

            var booksQuery = GetBooksQuery(selectedAuthors, selectedCategories);

            var viewModel = new BooksReportViewModel
            {
                Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors),
                Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories),
            };

            var page = pageNumber ?? 1;
            viewModel.Books = await PaginatedList<BookViewModel>.CreateAsync(
                booksQuery,
                page,
                (int)ReportsConfigurations.PageSize
            );

            return View(viewModel);
        }

        public async Task<IActionResult> ExportBooksToExcel(string authors, string categories)
        {
            var (selectAuthors, selectCategories) = GetBooksSelectedFilters(authors, categories);

            var books = await GetBooksQuery(selectAuthors, selectCategories).ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Books");

            var headers = new string[]
            {
                "ISBN",
                "Title",
                "Authors",
                "Categories",
                "Publisher",
                "Publishing Date",
                "Hall",
                "Available For Rental",
                "Status",
            };

            ws.SetHeader(_webHostEnvironment, headers);

            for (int i = 0; i < books.Count; i++)
            {
                ws.Cell(i + excelDataStartRow, 1).SetValue(books[i].Isbn);
                ws.Cell(i + excelDataStartRow, 2).SetValue(books[i].Title);
                ws.Cell(i + excelDataStartRow, 3).SetValue(string.Join(", ", books[i].Authors));
                ws.Cell(i + excelDataStartRow, 4).SetValue(string.Join(", ", books[i].Categories));
                ws.Cell(i + excelDataStartRow, 5).SetValue(books[i].Publisher);
                ws.Cell(i + excelDataStartRow, 6)
                    .SetValue(books[i].PublishingDate.ToString("d MMM, yyyy"));
                ws.Cell(i + excelDataStartRow, 7).SetValue(books[i].Hall);
                ws.Cell(i + excelDataStartRow, 8)
                    .SetValue(books[i].IsAvailableForRental ? "Yes" : "No");
                ws.Cell(i + excelDataStartRow, 9)
                    .SetValue(books[i].IsDeleted ? "Deleted" : "Active");
            }

            ws.Format();
            ws.AddTable(books.Count, headers.Length);

            await using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return File(
                stream.ToArray(),
                MediaTypeNames.Application.Octet,
                $"Books_{Guid.NewGuid()}.xlsx"
            );
        }

        public async Task<IActionResult> ExportBooksToPdf(string authors, string categories)
        {
            var (selectAuthors, selectCategories) = GetBooksSelectedFilters(authors, categories);

            var books = await GetBooksQuery(selectAuthors, selectCategories).ToListAsync();

            var templatePath = "~/Views/Reports/BooksReport.cshtml";
            var html = await _viewRenderer.RenderViewToStringAsync(
                ControllerContext,
                templatePath,
                books
            );

            var pdf = Pdf.From(html)
                .EncodedWith("Utf-8")
                .WithMargins(1.Centimeters())
                .Landscape()
                .Content();

            return File(pdf, MediaTypeNames.Application.Octet, $"Books_{Guid.NewGuid()}.pdf");
        }

        private IQueryable<BookViewModel> GetBooksQuery(
            IEnumerable<int> selectedAuthors,
            IEnumerable<int> selectedCategories
        )
        {
            var booksQuery = _context.Books.AsNoTracking();

            if (selectedAuthors.Any())
            {
                booksQuery = booksQuery.Where(b =>
                    b.Authors.Any(a => selectedAuthors.Contains(a.AuthorId))
                );
            }

            if (selectedCategories.Any())
            {
                booksQuery = booksQuery.Where(b =>
                    b.Categories.Any(c => selectedCategories.Contains(c.CategoryId))
                );
            }

            var books = booksQuery.Select(b => new BookViewModel
            {
                Isbn = b.Isbn ?? "N/A",
                Title = b.Title,
                Authors = b.Authors.Select(a => a.Author!.Name),
                Categories = b.Categories.Select(c => c.Category!.Name),
                Publisher = b.Publisher!.Name,
                PublishingDate = b.PublishingDate,
                Hall = b.Hall,
                IsAvailableForRental = b.IsAvailableForRental,
                IsDeleted = b.IsDeleted,
            });

            return books;
        }

        private (IEnumerable<int> authors, IEnumerable<int> categories) GetBooksSelectedFilters(
            string authors,
            string categories
        )
        {
            IEnumerable<int> selectedAuthors = string.IsNullOrEmpty(authors)
                ? Enumerable.Empty<int>()
                : authors
                    .Split(',')
                    .Select(s => int.TryParse(s.Trim(), out var id) ? (int?)id : null)
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .ToList();

            IEnumerable<int> selectedCategories = string.IsNullOrEmpty(categories)
                ? Enumerable.Empty<int>()
                : categories
                    .Split(',')
                    .Select(s => int.TryParse(s.Trim(), out var id) ? (int?)id : null)
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .ToList();

            return (selectedAuthors, selectedCategories);
        }

        #endregion

        #region Rentals

        public async Task<IActionResult> Rentals(string duration, int? pageNumber)
        {
            var viewModel = new RentalsReportViewModel { Duration = duration };

            var (error, query) = GetRentalsQuery(duration);

            if (error is not null)
            {
                ModelState.AddModelError("Duration", error);
                return View(viewModel);
            }

            if (pageNumber.HasValue)
                viewModel.Rentals = await PaginatedList<RentalsReportItemViewModel>.CreateAsync(
                    query,
                    pageNumber.Value,
                    (int)ReportsConfigurations.PageSize
                );

            ModelState.Clear();
            return View(viewModel);
        }

        public async Task<IActionResult> ExportRentalsToExcel(string duration)
        {
            var (error, rentalsQuery) = GetRentalsQuery(duration);

            if (error is not null)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Rentals), new { duration });
            }

            var rentals = await rentalsQuery.ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Rentals");

            var headers = new string[]
            {
                "Subscriber Id",
                "Subscriber Name",
                "Subscriber Mobile",
                "Book Title",
                "Book Authors",
                "Book Serial",
                "Rental Date",
                "End Date",
                "Return Date",
                "Extended On",
            };

            ws.SetHeader(_webHostEnvironment, headers);

            for (int i = 0; i < rentals.Count; i++)
            {
                ws.Cell(i + excelDataStartRow, 1).SetValue(rentals[i].SubscriberId);
                ws.Cell(i + excelDataStartRow, 2).SetValue(rentals[i].SubscriberName);
                ws.Cell(i + excelDataStartRow, 3).SetValue(rentals[i].SubscriberMobile);
                ws.Cell(i + excelDataStartRow, 4).SetValue(rentals[i].BookTitle);
                ws.Cell(i + excelDataStartRow, 5)
                    .SetValue(string.Join(", ", rentals[i].BookAuthors!));
                ws.Cell(i + excelDataStartRow, 6).SetValue(rentals[i].BookSerialNumber);
                ws.Cell(i + excelDataStartRow, 7)
                    .SetValue(rentals[i].RentalDate.ToString("d MMM, yyyy"));
                ws.Cell(i + excelDataStartRow, 8)
                    .SetValue(rentals[i].EndDate.ToString("d MMM, yyyy"));
                ws.Cell(i + excelDataStartRow, 9)
                    .SetValue(
                        rentals[i].ReturnDate.HasValue
                            ? rentals[i].ReturnDate!.Value.ToString("d MMM, yyyy")
                            : "-"
                    );
                ws.Cell(i + excelDataStartRow, 10)
                    .SetValue(
                        rentals[i].ExtendedOn.HasValue
                            ? rentals[i].ExtendedOn!.Value.ToString("d MMM, yyyy")
                            : "-"
                    );
            }

            ws.Format();
            ws.AddTable(rentals.Count, headers.Length);

            await using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return File(
                stream.ToArray(),
                MediaTypeNames.Application.Octet,
                $"Rentals_{Guid.NewGuid()}.xlsx"
            );
        }

        public async Task<IActionResult> ExportRentalsToPdf(string duration)
        {
            var (error, rentalsQuery) = GetRentalsQuery(duration);

            if (error is not null)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Rentals), new { duration });
            }

            var rentals = await rentalsQuery.ToListAsync();

            var templatePath = "~/Views/Reports/RentalsReport.cshtml";
            var html = await _viewRenderer.RenderViewToStringAsync(
                ControllerContext,
                templatePath,
                rentals
            );

            var pdf = Pdf.From(html)
                .EncodedWith("Utf-8")
                .WithMargins(1.Centimeters())
                .Landscape()
                .Content();

            return File(pdf, MediaTypeNames.Application.Octet, $"Rentals_{Guid.NewGuid()}.pdf");
        }

        private (string? error, IQueryable<RentalsReportItemViewModel>) GetRentalsQuery(
            string duration
        )
        {
            var query = Enumerable.Empty<RentalsReportItemViewModel>().AsQueryable();

            if (string.IsNullOrEmpty(duration))
                return (null, query);

            var dateRange = duration.Split(" - ");

            if (dateRange.Length != 2)
                return (error: Error.InvalidDuration, query);

            if (
                !DateOnly.TryParse(
                    dateRange[0].Trim(),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var fromDate
                )
            )
                return (error: Error.InvalidStartDate, query);

            if (
                !DateOnly.TryParse(
                    dateRange[1].Trim(),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var toDate
                )
            )
                return (error: Error.InvalidEndDate, query);

            query = _context
                .RentalCopies.Where(rc => rc.RentalDate >= fromDate && rc.RentalDate <= toDate)
                .OrderByDescending(rc => rc.RentalDate)
                .AsNoTracking()
                .Select(rc => new RentalsReportItemViewModel
                {
                    SubscriberId = rc.Rental!.SubscriberId,
                    SubscriberName =
                        $"{rc.Rental.Subscriber!.FirstName} {rc.Rental.Subscriber.LastName}",
                    SubscriberMobile = rc.Rental.Subscriber.MobileNumber,
                    BookTitle = rc.BookCopy!.Book!.Title,
                    BookSerialNumber = rc.BookCopy.SerialNumber,
                    BookAuthors = rc.BookCopy.Book.Authors.Select(a => a.Author!.Name),
                    RentalDate = rc.RentalDate.ToDateTime(TimeOnly.MinValue),
                    EndDate = rc.EndDate.ToDateTime(TimeOnly.MinValue),
                    ReturnDate = rc.ReturnDate.HasValue
                        ? rc.ReturnDate.Value.ToDateTime(TimeOnly.MinValue)
                        : null,
                    ExtendedOn = rc.ExtendedOn.HasValue
                        ? rc.ExtendedOn.Value.ToDateTime(TimeOnly.MinValue)
                        : null,
                });

            return (error: null, query);
        }

        #endregion

        #region Delayed Rentals

        public async Task<IActionResult> DelayedRentals()
        {
            var viewModel = new DelayedRentalsViewModel { Rentals = await GetDelayedRentals() };

            return View(viewModel);
        }

        public async Task<IActionResult> ExportDelayedRentalsToExcel()
        {
            var delayedRentals = await GetDelayedRentals();

            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Delayed Rentals");

            var headers = new string[]
            {
                "Subscriber Id",
                "Subscriber Name",
                "Subscriber Mobile",
                "Book Title",
                "Book Serial",
                "Rental Date",
                "End Date",
                "Extended On",
                "Delay In Days",
            };

            ws.SetHeader(_webHostEnvironment, headers);

            for (int i = 0; i < delayedRentals.Count; i++)
            {
                ws.Cell(i + excelDataStartRow, 1).SetValue(delayedRentals[i].SubscriberId);
                ws.Cell(i + excelDataStartRow, 2).SetValue(delayedRentals[i].SubscriberName);
                ws.Cell(i + excelDataStartRow, 3).SetValue(delayedRentals[i].SubscriberMobile);
                ws.Cell(i + excelDataStartRow, 4).SetValue(delayedRentals[i].BookTitle);
                ws.Cell(i + excelDataStartRow, 5).SetValue(delayedRentals[i].BookSerialNumber);
                ws.Cell(i + excelDataStartRow, 6)
                    .SetValue(delayedRentals[i].RentalDate.ToString("d MMM, yyyy"));
                ws.Cell(i + excelDataStartRow, 7)
                    .SetValue(delayedRentals[i].EndDate.ToString("d MMM, yyyy"));
                ws.Cell(i + excelDataStartRow, 8)
                    .SetValue(
                        delayedRentals[i].ExtendedOn.HasValue
                            ? delayedRentals[i].ExtendedOn!.Value.ToString("d MMM, yyyy")
                            : "-"
                    );
                ws.Cell(i + excelDataStartRow, 9).SetValue(delayedRentals[i].DelayInDays);
            }

            ws.Format();
            ws.AddTable(delayedRentals.Count, headers.Length);

            await using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return File(
                stream.ToArray(),
                MediaTypeNames.Application.Octet,
                $"Dalyed_Rentals_{Guid.NewGuid()}.xlsx"
            );
        }

        public async Task<IActionResult> ExportDelayedRentalsToPdf(string duration)
        {
            var delayedRentals = await GetDelayedRentals();

            var templatePath = "~/Views/Reports/DelayedRentalsReport.cshtml";
            var html = await _viewRenderer.RenderViewToStringAsync(
                ControllerContext,
                templatePath,
                delayedRentals
            );

            var pdf = Pdf.From(html)
                .EncodedWith("Utf-8")
                .WithMargins(1.Centimeters())
                .Landscape()
                .Content();

            return File(pdf, MediaTypeNames.Application.Octet, $"Rentals_{Guid.NewGuid()}.pdf");
        }

        private async Task<List<DelayedRentalItemViewModel>> GetDelayedRentals()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var delayedRentals = await _context
                .RentalCopies.AsNoTracking()
                .Where(rc => !rc.ReturnDate.HasValue && rc.EndDate < today)
                .Select(rc => new DelayedRentalItemViewModel
                {
                    SubscriberId = rc.Rental!.SubscriberId,
                    SubscriberMobile = rc.Rental.Subscriber!.MobileNumber,
                    SubscriberName =
                        $"{rc.Rental.Subscriber.FirstName} {rc.Rental.Subscriber.LastName}",
                    BookTitle = rc.BookCopy!.Book!.Title,
                    BookSerialNumber = rc.BookCopy.SerialNumber,
                    RentalDate = rc.RentalDate.ToDateTime(TimeOnly.MinValue),
                    EndDate = rc.EndDate.ToDateTime(TimeOnly.MinValue),
                    ExtendedOn = rc.ExtendedOn.HasValue
                        ? rc.ExtendedOn.Value.ToDateTime(TimeOnly.MinValue)
                        : null,
                })
                .ToListAsync();

            return delayedRentals;
        }

        #endregion
    }
}
