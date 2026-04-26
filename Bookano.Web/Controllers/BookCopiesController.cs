namespace Bookano.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BookCopiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public BookCopiesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Create(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookCopyFormViewModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("_Form", model);

            var book = await _context.Books.FindAsync(model.BookId);

            if (book is null)
                return NotFound();

            var copy = new BookCopy
            {
                EditionNumber = model.EditionNumber,
                IsAvailableForRental = book.IsAvailableForRental && model.IsAvailableForRental,
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value,
            };

            book.Copies.Add(copy);
            await _context.SaveChangesAsync();

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);
            return PartialView("_BookCopyRow", viewModel);
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Edit(int id)
        {
            var copy = await _context
                .BookCopies.AsNoTracking()
                .Include(c => c.Book)
                .SingleOrDefaultAsync(c => c.Id == id);

            if (copy is null)
                return NotFound();

            var viewModel = _mapper.Map<BookCopyFormViewModel>(copy);
            viewModel.ShowRentalInput = copy.Book!.IsAvailableForRental;

            return PartialView("_Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookCopyFormViewModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("_Form", model);

            var copy = await _context
                .BookCopies.Include(c => c.Book)
                .SingleOrDefaultAsync(c => c.Id == model.Id);

            if (copy is null)
                return NotFound();

            copy.EditionNumber = model.EditionNumber;
            copy.LastUpdatedOnUtc = DateTime.UtcNow;
            copy.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            copy.IsAvailableForRental =
                copy.Book!.IsAvailableForRental && model.IsAvailableForRental;

            await _context.SaveChangesAsync();

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_BookCopyRow", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var copy = await _context.BookCopies.FindAsync(id);

            if (copy is null)
                return NotFound();

            copy.IsDeleted = !copy.IsDeleted;
            var updatedOn = DateTimeOffset.UtcNow;
            copy.LastUpdatedOnUtc = updatedOn;
            copy.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            await _context.SaveChangesAsync();

            return Ok(updatedOn.ToString("o"));
        }
    }
}
