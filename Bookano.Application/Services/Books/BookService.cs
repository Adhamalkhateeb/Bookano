using Bookano.Application.Common;
using Bookano.Application.Common.Interfaces;
using Bookano.Application.DTOs.Books;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Dynamic.Core;

namespace Bookano.Application.Services.Books;

public class BookService(IUnitOfWork unitOfWork, IMapper mapper, DataTableQueryBuilder<Book> builder, [FromKeyedServices("cloudinary")] IImageService imageService, IValidator<BookFormDto> validator) : IBookService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly DataTableQueryBuilder<Book> _builder = builder;
    private readonly IImageService _imageService = imageService;
    private readonly IValidator<BookFormDto> _validator = validator;


    private static readonly List<string> AllowedSortColumns =
        new()
        {
            "Id", "Title", "Publisher.Name", "PublishingDate",
            "Hall", "IsAvailableForRental", "IsDeleted",
        };

    public async Task<DataTableResult<BookListDto>> GetPagedAsync(
        DataTableRequest request,
        CancellationToken ct = default)
    {
        var query = _unitOfWork.Books.GetQueryable();


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
            .ExecuteAsync<BookListDto>(ct);

    }

    public async Task<BookDetailsDto?> GetBookDetailsAsync(int id, CancellationToken ct = default)
    {
        var book = await _unitOfWork.Books.GetQueryable()
            .AsSplitQuery()
            .ProjectTo<BookDetailsDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(b => b.Id == id, ct);

        return book;
    }

    public async Task<BookFormDto?> GetBookFormAsync(int id, CancellationToken ct = default)
    {
        var book = await _unitOfWork.Books.GetQueryable(true)
            .ProjectTo<BookFormDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(b => b.Id == id, ct);

        return book;
    }

    public async Task<Result<int>> CreateAsync(BookFormDto dto, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
            return Result<int>.Failure(validationResult.ToValidationErrors());

        if (dto.Image is not null)
        {
            var imageValidationError = _imageService.ValidateImage(dto.Image.FileName, dto.Image.Length);

            if (!string.IsNullOrEmpty(imageValidationError))
                return Result<int>.Failure(imageValidationError);
        }

        var existing = await _unitOfWork.Books
            .GetQueryable()
            .FirstOrDefaultAsync(b => b.IdempotencyKey == dto.IdempotencyKey, ct);

        if (existing is not null)
            return Result<int>.Success(existing.Id);

        var book = _mapper.Map<Book>(dto);

        SyncCategories(book, dto.SelectedCategories);
        SyncAuthors(book, dto.SelectedAuthors);

        string? newUploadedPublicId = null;

        if (dto.Image is not null)
        {
            await using var stream = dto.Image.Stream;
            var uploadResult = await _imageService.UploadAsync(stream, dto.Image.FileName, "books", null, ct);

            if (uploadResult.IsSuccess)
            {
                newUploadedPublicId = uploadResult.PublicId;

                book.ImageUrl = uploadResult.Url;
                book.ImageThumbnailUrl = _imageService.GetThumbnail(uploadResult.PublicId!);
                book.ImagePublicId = uploadResult.PublicId;
            }
        }

        _unitOfWork.Books.Add(book);

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            if (!string.IsNullOrEmpty(newUploadedPublicId))
            {
                await _imageService.DeleteAsync(newUploadedPublicId, ct);
            }

            var fallback = await _unitOfWork.Books
                .GetQueryable()
                .FirstOrDefaultAsync(b => b.IdempotencyKey == dto.IdempotencyKey, ct);

            if (fallback is null)
                return Result<int>.Failure("Could not create book.");

            return Result<int>.Success(fallback.Id);
        }

        return Result<int>.Success(book.Id);
    }

    public async Task<Result<int>> UpdateAsync(BookFormDto dto, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
            return Result<int>.Failure(validationResult.ToValidationErrors());

        if (dto.Image is not null)
        {
            var imageValidationError = _imageService.ValidateImage(dto.Image.FileName, dto.Image.Length);

            if (!string.IsNullOrEmpty(imageValidationError))
                return Result<int>.Failure(imageValidationError);
        }

        var book = await _unitOfWork.Books.GetQueryable(isTracking: true)
            .Include(b => b.Categories)
            .Include(b => b.Authors)
            .SingleOrDefaultAsync(b => b.Id == dto.Id, ct);

        if (book is null) return Result<int>.Failure("Book not found.");

        if (dto.RowVersion is not null)
        {
            _unitOfWork.Entry(book).Property(b => b.RowVersion).OriginalValue = dto.RowVersion;
        }

        var availabilityChangedToFalse = book.IsAvailableForRental && !dto.IsAvailableForRental;

        _mapper.Map(dto, book);

        SyncCategories(book, dto.SelectedCategories);
        SyncAuthors(book, dto.SelectedAuthors);

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

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);

            if (availabilityChangedToFalse)
                await _unitOfWork.BookCopies.GetQueryable()
                    .Where(bc => bc.BookId == dto.Id)
                    .ExecuteUpdateAsync(p =>
                        p.SetProperty(c => c.IsAvailableForRental, false), ct);
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

    public async Task<DateTimeOffset?> ToggleAsync(int id, CancellationToken ct = default)
    {
        var book = await _unitOfWork.Books.GetByIdAsync(id,ct);

        if (book is null) return null;

        book.IsDeleted = !book.IsDeleted;
        await _unitOfWork.SaveChangesAsync(ct);

        return book.LastUpdatedOnUtc;
    }

    public async Task<bool> IsIsbnUniqueAsync(
        string isbn, int excludeId = 0, CancellationToken ct = default)
    {
        return !await _unitOfWork.Books.GetQueryable()
            .AnyAsync(b => b.Isbn == isbn && b.Id != excludeId, ct);
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


}