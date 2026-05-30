using Bookano.Application.DTOs.BookCopies;


namespace Bookano.Application.Services.BookCopies;

public class BookCopiesService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<BookCopyFormDto> validator) : IBookCopiesService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IValidator<BookCopyFormDto> _validator = validator;


    public async Task<BookCopyDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var copy = await _unitOfWork.BookCopies
            .GetQueryable()
            .ProjectTo<BookCopyDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(c => c.Id == id, ct);


        return copy;

    }

    public async Task<BookDetailsForCopy?> GetBook(int bookId, CancellationToken ct = default)
    {
        var book = await _unitOfWork.Books.GetQueryable()
            .Where(b => b.Id == bookId)
            .Select(b => new BookDetailsForCopy
            {
                BookId = b.Id,
                IsAvailableForRental = b.IsAvailableForRental,
            }).SingleOrDefaultAsync(ct); ;


        return book;

    }

    public async Task<Result<BookCopyDto?>> AddAsync(BookCopyFormDto dto, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
            return Result<BookCopyDto?>.Failure(validationResult.ToValidationErrors());

        var book = await _unitOfWork
            .Books.GetByIdAsync(dto.BookId, ct);

        if (book is null)
            return Result<BookCopyDto?>.Failure("Book not found.");

        var copy = new BookCopy
        {
            EditionNumber = dto.EditionNumber,
            IsAvailableForRental = book.IsAvailableForRental && dto.IsAvailableForRental,
        };

        book.Copies.Add(copy);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<BookCopyDto?>.Success(_mapper.Map<BookCopyDto>(copy));
    }

    public async Task<Result<BookCopyDto?>> UpdateAsync(BookCopyFormDto dto, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
            return Result<BookCopyDto?>.Failure(validationResult.ToValidationErrors());

        var copy = await _unitOfWork
            .BookCopies.GetQueryable()
            .Include(c => c.Book)
            .SingleOrDefaultAsync(c => c.Id == dto.Id, ct);

        if (copy is null)
            return Result<BookCopyDto?>.Failure("Book copy not found.");

        copy.EditionNumber = dto.EditionNumber;
        copy.IsAvailableForRental = copy.Book!.IsAvailableForRental && dto.IsAvailableForRental;

        await _unitOfWork.SaveChangesAsync(ct);

        return Result<BookCopyDto?>.Success(_mapper.Map<BookCopyDto>(copy));
    }


    public async Task<DateTimeOffset?> ToggleAsync(int id, CancellationToken ct = default)
    {
        var copy = await _unitOfWork.BookCopies.GetByIdAsync(id, ct);

        if (copy is null)
            return null;

        copy.IsDeleted = !copy.IsDeleted;
        await _unitOfWork.SaveChangesAsync(ct);

        return copy.LastUpdatedOnUtc;
    }
    public async Task<IEnumerable<BookCopyRentalHistoryDto>?> GetRentalHistoryAsync(int id, CancellationToken ct = default)
    {
        
        var copyExists = await _unitOfWork.BookCopies.IsExistsAsync(c => c.Id == id,ct);
        if (!copyExists)
            return null;

        var history = await _unitOfWork
            .RentalCopies.GetQueryable()
            .Where(rc => rc.BookCopy!.Id == id)
            .ProjectTo<BookCopyRentalHistoryDto>(_mapper.ConfigurationProvider)
            .OrderByDescending(c => c.StartDate)
            .ToListAsync(ct);

        return history;
    }


}