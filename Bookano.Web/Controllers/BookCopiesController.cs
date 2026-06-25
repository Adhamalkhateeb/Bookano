using Bookano.Application.DTOs.BookCopies;
using Bookano.Application.Services.BookCopies;
using Bookano.Application.Services.Books;
using Bookano.Application.Services.Rentals;
using Bookano.Web.ViewModels.BookCopies;

namespace Bookano.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BookCopiesController(
        IBookCopiesService bookCopiesService,
        IBookService bookService,
        IRentalService rentalService,
        IMapper mapper
    ) : Controller
    {
        private readonly IBookCopiesService _bookCopiesService = bookCopiesService;
        private readonly IBookService _bookService = bookService;
        private readonly IRentalService _rentalService = rentalService;
        private readonly IMapper _mapper = mapper;

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Create(int bookId)
        {
            var book = await _bookService.GetByIdAsync(bookId);

            if (book is null)
                return NotFound();

            var viewModel = new BookCopyFormViewModel
            {
                BookId = bookId,
                ShowRentalInput = book.IsAvailableForRental,
            };

            return PartialView("_Form", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(BookCopyFormViewModel model,CancellationToken ct)
        {
            var dto = _mapper.Map<BookCopySaveDto>(model);
            
            var result = await _bookCopiesService.AddAsync(dto,ct);
            if (result.IsFailure)
            {
                result.AddToModelState(ModelState);
                return PartialView("_Form", model);
            }

            var vm = _mapper.Map<BookCopyRowViewModel>(result.Value);

            return PartialView("_BookCopyRow", vm);
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Edit(int id,CancellationToken ct)
        {

            var copy = await _bookCopiesService.GetByIdAsync(id, ct);

            if(copy is null)
                return NotFound();
            
            var viewModel = _mapper.Map<BookCopyFormViewModel>(copy);
            viewModel.ShowRentalInput = !copy.BookIsAvailableForRental;

            return PartialView("_Form", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BookCopyFormViewModel model,CancellationToken ct)
        {
            var dto = _mapper.Map<BookCopySaveDto>(model);

            var result = await _bookCopiesService.UpdateAsync(dto,ct);
            if (result.IsFailure)
            {
                result.AddToModelState(ModelState);
                return PartialView("_Form", model);
            }

            var vm = _mapper.Map<BookCopyRowViewModel>(result.Value);

            return PartialView("_BookCopyRow", vm);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var result = await _bookCopiesService.ToggleStatusAsync(id);
            if (result.IsFailure)
                return NotFound();

            return Ok(result.Value!.LastUpdatedOnUtc.ToString());
        }

        public async Task<IActionResult> RentalHistory(int id, CancellationToken ct)
        {
            var history = await _rentalService.GetCopyRentalHistoryAsync(id, ct);

            if (history is null)
                return NotFound();

            var viewModel = _mapper.Map<IEnumerable<CopyHistoyViewModel>>(history);

            return View(viewModel);

        }
    }
}