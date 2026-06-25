using Bookano.Application.DTOs.BookCopies;


namespace Bookano.Application.Services.BookCopies;

public class BookCopiesService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<BookCopySaveDto> validator) : IBookCopiesService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IValidator<BookCopySaveDto> _validator = validator;


    public async Task<int> GetCountAsync(CancellationToken ct = default)
    {
        var copiesCount = await _unitOfWork.BookCopies.GetQueryable().CountAsync(c => !c.IsDeleted, ct);
        return copiesCount <= 10 ? copiesCount : copiesCount / 10 * 10;
    }

    public async Task<BookCopyDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
       return await _unitOfWork.BookCopies
            .GetQueryable(withTracking: false)
            .Where(c => c.Id == id)
            .ProjectTo<BookCopyDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(ct);
    }

    public async Task<BookCopyDto?> GetActiveCopyBySerialNumberAsync(int serialNumber, CancellationToken ct = default)
    {
        return await _unitOfWork.BookCopies
            .GetQueryable(withTracking: false)
            .Where(c => c.SerialNumber == serialNumber && !c.IsDeleted && !c.Book!.IsDeleted)
            .ProjectTo<BookCopyDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<BookCopyDto>> GetByBookAsync(int bookId,CancellationToken ct = default)
    {
        return await _unitOfWork.BookCopies
            .GetQueryable(withTracking: false)
            .Where(c => c.BookId == bookId)
            .ProjectTo<BookCopyDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<IEnumerable<BookCopyDto>> GetCopiesBySerialNumbersAsync(IEnumerable<int> serialNumbers, CancellationToken ct = default)
    {
        var selectedSerials = serialNumbers.Distinct().ToList();

        if (selectedSerials.Count == 0)
            return [];

        var copies = await _unitOfWork.BookCopies
            .GetQueryable(withTracking: false)
            .Where(c => selectedSerials.Contains(c.SerialNumber))
            .ProjectTo<BookCopyDto>(_mapper.ConfigurationProvider)
            .ToDictionaryAsync(c => c.SerialNumber, ct);

        return selectedSerials
            .Where(copies.ContainsKey)
            .Select(s => copies[s])
            .ToList();
    }

    public async Task<Result<BookCopyDto?>> AddAsync(BookCopySaveDto dto, CancellationToken ct = default)
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

        return _mapper.Map<BookCopyDto>(copy);
    }

    public async Task<Result<BookCopyDto?>> UpdateAsync(BookCopySaveDto dto, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
            return Result<BookCopyDto?>.Failure(validationResult.ToValidationErrors());

        var copy = await _unitOfWork
            .BookCopies.GetQueryable(withTracking: true)
            .Include(c => c.Book)
            .FirstOrDefaultAsync(c => c.Id == dto.Id, ct);

        if (copy is null)
            return Result<BookCopyDto?>.Failure("Book copy not found.");

        copy.EditionNumber = dto.EditionNumber;
        copy.IsAvailableForRental = copy.Book!.IsAvailableForRental && dto.IsAvailableForRental;

        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<BookCopyDto>(copy);
    }


    public async Task<Result<ToggleStatusResult>> ToggleStatusAsync(int id, CancellationToken ct = default)
    {
        var copy = await _unitOfWork.BookCopies.GetByIdAsync(id, ct);

        if (copy is null)
            return Result<ToggleStatusResult>.Failure("Book copy not found.");

        copy.IsDeleted = !copy.IsDeleted;
        await _unitOfWork.SaveChangesAsync(ct);

        return new ToggleStatusResult(copy.IsDeleted, copy.LastUpdatedOnUtc);
    }


}