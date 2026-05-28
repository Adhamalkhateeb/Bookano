using Bookano.Application.Interfaces;

namespace Bookano.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BookCopiesController(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<BookCopyFormViewModel> validator
    ) : Controller
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly IValidator<BookCopyFormViewModel> _validator = validator;

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Create(int bookId)
        {
            var book = await _unitOfWork
                .Books.GetQueryable()
                .SingleOrDefaultAsync(b => b.Id == bookId);

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
        public async Task<IActionResult> Create(BookCopyFormViewModel model)
        {
            var validationResult = _validator.Validate(model);
            validationResult.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return PartialView("_Form", model);

            var book = await _unitOfWork
                .Books.GetQueryable()
                .SingleOrDefaultAsync(b => b.Id == model.BookId);

            if (book is null)
                return NotFound();

            var copy = new BookCopy
            {
                EditionNumber = model.EditionNumber,
                IsAvailableForRental = book.IsAvailableForRental && model.IsAvailableForRental,
            };

            book.Copies.Add(copy);
            await _unitOfWork.SaveChangesAsync();

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);
            return PartialView("_BookCopyRow", viewModel);
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Edit(int id)
        {
            var copy = await _unitOfWork
                .BookCopies.GetQueryable()
                .Include(c => c.Book)
                .SingleOrDefaultAsync(c => c.Id == id);

            if (copy is null)
                return NotFound();

            var viewModel = _mapper.Map<BookCopyFormViewModel>(copy);
            viewModel.ShowRentalInput = copy.Book!.IsAvailableForRental;

            return PartialView("_Form", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BookCopyFormViewModel model)
        {
            var validationResult = _validator.Validate(model);
            validationResult.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return PartialView("_Form", model);

            var copy = await _unitOfWork
                .BookCopies.GetQueryable()
                .Include(c => c.Book)
                .SingleOrDefaultAsync(c => c.Id == model.Id);

            if (copy is null)
                return NotFound();

            copy.EditionNumber = model.EditionNumber;
            copy.IsAvailableForRental =
                copy.Book!.IsAvailableForRental && model.IsAvailableForRental;

            await _unitOfWork.SaveChangesAsync();

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_BookCopyRow", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var copy = await _unitOfWork
                .BookCopies.GetQueryable()
                .SingleOrDefaultAsync(c => c.Id == id);

            if (copy is null)
                return NotFound();

            copy.IsDeleted = !copy.IsDeleted;

            await _unitOfWork.SaveChangesAsync();

            return Ok(copy.LastUpdatedOnUtc.ToString());
        }

        public async Task<IActionResult> RentalHistory(int id)
        {
            var viewModel = await _unitOfWork
                .RentalCopies.GetQueryable()
                .Where(rc => rc.BookCopy!.Id == id)
                .Select(c => new CopyHistoyViewModel
                {
                    SubscriberName =
                        $"{c.Rental!.Subscriber!.FirstName} {c.Rental.Subscriber.LastName}",
                    SubscriberMobile = c.Rental.Subscriber.MobileNumber,

                    StartDate = c.RentalDate.ToDateTime(TimeOnly.MinValue),
                    EndDate = c.EndDate.ToDateTime(TimeOnly.MinValue),

                    ReturnDate = c.ReturnDate.HasValue
                        ? c.ReturnDate.Value.ToDateTime(TimeOnly.MinValue)
                        : null,

                    ExtendedOn = c.ExtendedOn.HasValue
                        ? c.ExtendedOn.Value.ToDateTime(TimeOnly.MinValue)
                        : null,
                })
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();

            if (
                viewModel.Count == 0
                && !await _unitOfWork.BookCopies.GetQueryable().AnyAsync(c => c.Id == id)
            )
                return NotFound();

            return View(viewModel);
        }
    }
}
