using Bookano.Application.Common.Interfaces;
using Bookano.Application.DTOs.Books;
using Bookano.Application.DTOs.Categories;
using Bookano.Application.DTOs.Authors;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Dynamic.Core;
using Bookano.Application.DTOs.BookCopies;

namespace Bookano.Application.Services.Books;

public class BookService(IUnitOfWork unitOfWork, IMapper mapper, PaginationQueryBuilder<Book> builder, [FromKeyedServices("cloudinary")] IImageService imageService, IValidator<BookSaveDto> validator) : IBookService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly PaginationQueryBuilder<Book> _builder = builder;
    private readonly IImageService _imageService = imageService;
    private readonly IValidator<BookSaveDto> _validator = validator;


    private static readonly List<string> AllowedSortColumns =
        new()
        {
            "Id", "Title", "Publisher.Name", "PublishingDate",
            "Hall", "IsAvailableForRental", "IsDeleted",
        };

    public async Task<DataGridResult<TOut>> GetPagedFilteredAsync<TOut>(PaginationFilterQuery request,CancellationToken ct = default)
    {
        var query = _unitOfWork.Books.GetQueryable(withTracking: false);

        return await _builder.For(query)
            .WithRequest(request)
            .AllowSorting([.. AllowedSortColumns])
            .Search((q, s) =>
            {
                return q.Where(b =>
                    b.Title.Contains(s) ||
                    (b.Isbn != null && b.Isbn.Contains(s)) ||
                    b.Authors.Any(a => a.Author!.Name.Contains(s))
                );
            })
            .Sort()
            .ExecuteAsync<TOut>(ct);

    }

    public async Task<IEnumerable<BookDto>> GetRecentBooksAsync(int count, CancellationToken ct = default)
    {
        return await _unitOfWork
            .Books.GetQueryable()
            .Where(b => !b.IsDeleted)
            .OrderByDescending(b => b.CreatedOnUtc)
            .Take(count)
            .ProjectTo<BookDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<BookDto>> GetFilteredBooksAsync(string query, CancellationToken ct = default)
    {
        var trimmed = query.Trim();

        return await _unitOfWork
            .Books.GetQueryable()
            .Where(b =>
                !b.IsDeleted
                && (
                    b.Title.Contains(trimmed)
                    || b.Authors.Any(a => a.Author!.Name.Contains(trimmed))
                    || (b.Isbn != null && b.Isbn.Contains(trimmed))
                )
            )
            .ProjectTo<BookDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<BookDto>> GetTopRentedBooksAsync(int count, CancellationToken ct = default)
    {
        var topBookIds = await _unitOfWork
            .RentalCopies.GetQueryable(withTracking: false)
            .GroupBy(rc => rc.BookCopy!.BookId)
            .OrderByDescending(g => g.Count())
            .Take(count)
            .Select(g => g.Key)
            .ToListAsync(ct);

        var topRentedBooks = await _unitOfWork
            .Books.GetQueryable(withTracking: false)
            .Where(b => !b.IsDeleted && topBookIds.Contains(b.Id))
            .ProjectTo<BookDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return topBookIds
            .Join(topRentedBooks, id => id, book => book.Id, (id, book) => book)
            .ToList();
    }

    public async Task<BookDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _unitOfWork.Books.GetQueryable(withTracking: false)
            .Where(b => b.Id == id)
            .ProjectTo<BookDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(ct);
    }

    public async Task<BookDetailsDto?> GetDetailsAsync(int id, CancellationToken ct = default)
    {
        return await _unitOfWork.Books.GetQueryable(withTracking: false)
            .Where(b => b.Id == id)
            .ProjectTo<BookDetailsDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<CategoryDto>> GetBookCategoriesAsync(int bookId, CancellationToken ct = default)
    {
        return await _unitOfWork.Books.GetQueryable(withTracking: false)
            .Where(b => b.Id == bookId)
            .SelectMany(b => b.Categories)
            .Select(c => c.Category!)
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<AuthorDto>> GetBookAuthorsAsync(int bookId, CancellationToken ct = default)
    {
        return await _unitOfWork.Books.GetQueryable(withTracking: false)
            .Where(b => b.Id == bookId)
            .SelectMany(b => b.Authors)
            .Select(a => a.Author!)
            .ProjectTo<AuthorDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<Result<int>> CreateAsync(BookSaveDto dto, CancellationToken ct = default)
    {
        var validationError = await ValidateBookSaveDtoAsync(dto, ct);
        if (validationError is not null) return validationError;

        var existingId = await CheckIdempotencyAsync(dto.IdempotencyKey, ct);
        if (existingId.HasValue) return Result<int>.Success(existingId.Value);

        var book = MapAndSyncBookEntity(dto);

        var (newUploadedPublicId, _) = await ProcessImageUploadAsync(book, dto, ct);

        return await PersistCreationAsync(book, dto.IdempotencyKey, newUploadedPublicId, ct);
    }

    public async Task<Result<int>> UpdateAsync(int id,BookSaveDto dto, CancellationToken ct = default)
    {
        var validationError = await ValidateBookSaveDtoAsync(dto, ct);
        if (validationError is not null) return validationError;

        var book =  await _unitOfWork.Books.GetQueryable(withTracking: true)
            .Include(b => b.Categories)
            .Include(b => b.Authors)
            .SingleOrDefaultAsync(b => b.Id == id, ct);

        if (book is null) return Result<int>.Failure("Book not found.");

        if (dto.RowVersion is not null)
        {
            _unitOfWork.Entry(book).Property(b => b.RowVersion).OriginalValue = dto.RowVersion;
        }

        var availabilityChangedToFalse = book.IsAvailableForRental && !dto.IsAvailableForRental;

        MapAndSyncBookEntity(dto, book);

        var (newUploadedPublicId, oldImagePublicId) = await ProcessImageUploadAsync(book, dto, ct);

        return await PersistUpdateAsync(book, dto, newUploadedPublicId, oldImagePublicId, availabilityChangedToFalse, ct);
    }

    public async Task<Result<ToggleStatusResult>> ToggleStatusAsync(int id, CancellationToken ct = default)
    {
        var book = await _unitOfWork.Books.GetByIdAsync(id,ct);

        if (book is null) return Result<ToggleStatusResult>.Failure("Book not found.");

        book.IsDeleted = !book.IsDeleted;
        await _unitOfWork.SaveChangesAsync(ct);

        return new ToggleStatusResult(book.IsDeleted,book.LastUpdatedOnUtc);
    }

    public async Task<bool> IsIsbnAvailableAsync(string isbn, int excludeId = 0, CancellationToken ct = default)
    {
        return !await _unitOfWork.Books.IsExistsAsync(b => b.Isbn == isbn && b.Id != excludeId, ct);
    }

   
    private static void SyncCategories(Book book, IEnumerable<int> selected)
    {
        var set = selected.ToHashSet();

        foreach (var c in book.Categories.ToList())
            if (!set.Contains(c.CategoryId)) book.Categories.Remove(c);

        foreach (var id in set)
            if (!book.Categories.Any(c => c.CategoryId == id))
                book.Categories.Add(new BookCategory { CategoryId = id });
    }

    private static void SyncAuthors(Book book, IEnumerable<int> selected)
    {
        var set = selected.ToHashSet();

        foreach (var a in book.Authors.ToList())
            if (!set.Contains(a.AuthorId)) book.Authors.Remove(a);

        foreach (var id in set)
            if (!book.Authors.Any(a => a.AuthorId == id))
                book.Authors.Add(new BookAuthor { AuthorId = id });
    }

    private async Task<Result<int>?> ValidateBookSaveDtoAsync(BookSaveDto dto, CancellationToken ct)
    {
        var validationResult = await _validator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
            return Result<int>.Failure(validationResult.ToValidationErrors());

        if(dto.Isbn is not null && !await IsIsbnAvailableAsync(dto.Isbn, dto.Id, ct)) 
            return Result<int>.Failure("ISBN already exists.");

        if (dto.Image is not null)
        {
            var imageValidationError = _imageService.ValidateImage(dto.Image.FileName, dto.Image.Length);
            if (!string.IsNullOrEmpty(imageValidationError))
                return Result<int>.Failure(imageValidationError);
        }

        return null;
    }

    private async Task<int?> CheckIdempotencyAsync(string? idempotencyKey, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(idempotencyKey)) return null;

        var existing = await _unitOfWork.Books
            .GetQueryable(withTracking: false)
            .FirstOrDefaultAsync(b => b.IdempotencyKey == idempotencyKey, ct);

        return existing?.Id;
    }

    private Book MapAndSyncBookEntity(BookSaveDto dto, Book? existingBook = null)
    {
        var book = existingBook ?? _mapper.Map<Book>(dto);

        if (existingBook is not null)
        {
            _mapper.Map(dto, book);
        }

        SyncCategories(book, dto.SelectedCategories);
        SyncAuthors(book, dto.SelectedAuthors);

        return book;
    }

    private async Task<(string? newUploadedPublicId, string? oldImagePublicId)> ProcessImageUploadAsync(
        Book book, BookSaveDto dto, CancellationToken ct)
    {
        string? newUploadedPublicId = null;
        string? oldImagePublicId = null;

        if (dto.Image is not null)
        {
            await using var stream = dto.Image.Stream;
            var uploadResult = await _imageService.UploadAsync(stream, dto.Image.FileName, "books", null, ct);

            if (uploadResult.IsSuccess)
            {
                oldImagePublicId = book.ImagePublicId;
                newUploadedPublicId = uploadResult.PublicId;

                book.ImageUrl = uploadResult.Url;
                book.ImageThumbnailUrl = _imageService.GetThumbnail(uploadResult.PublicId!);
                book.ImagePublicId = uploadResult.PublicId;
            }
        }
        else if (dto.RemoveImage)
        {
            oldImagePublicId = book.ImagePublicId;

            book.ImageUrl = null;
            book.ImageThumbnailUrl = null;
            book.ImagePublicId = null;
        }

        return (newUploadedPublicId, oldImagePublicId);
    }

    private async Task<Result<int>> PersistCreationAsync(
        Book book, string? idempotencyKey, string? newUploadedPublicId, CancellationToken ct)
    {
        _unitOfWork.Books.Add(book);

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
            return Result<int>.Success(book.Id);
        }
        catch (DbUpdateException)
        {
            if (!string.IsNullOrEmpty(newUploadedPublicId))
            {
                await _imageService.DeleteAsync(newUploadedPublicId, ct);
            }

            if (!string.IsNullOrEmpty(idempotencyKey))
            {
                var fallback = await _unitOfWork.Books
                    .GetQueryable(withTracking: false)
                    .FirstOrDefaultAsync(b => b.IdempotencyKey == idempotencyKey, ct);

                if (fallback is not null)
                    return Result<int>.Success(fallback.Id);
            }

            return Result<int>.Failure("Could not create book.");
        }
    }

    private async Task<Result<int>> PersistUpdateAsync(
        Book book, BookSaveDto dto, string? newUploadedPublicId, string? oldImagePublicId, bool availabilityChangedToFalse, CancellationToken ct)
    {
        try
        {
            await _unitOfWork.SaveChangesAsync(ct);

            if (availabilityChangedToFalse)
            {
                await _unitOfWork.BookCopies.GetQueryable()
                    .Where(bc => bc.BookId == dto.Id)
                    .ExecuteUpdateAsync(p =>
                        p.SetProperty(c => c.IsAvailableForRental, false), ct);
            }
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!string.IsNullOrEmpty(newUploadedPublicId))
            {
                await _imageService.DeleteAsync(newUploadedPublicId, ct);
            }
            return Result<int>.Failure(Error.ConcurrencyError);
        }

        if (!string.IsNullOrEmpty(oldImagePublicId))
        {
            await _imageService.DeleteAsync(oldImagePublicId, ct);
        }

        return Result<int>.Success(book.Id);
    }


}