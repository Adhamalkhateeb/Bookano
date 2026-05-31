using Bookano.Application.DTOs.Books;
using Bookano.Application.DTOs.Search;

namespace Bookano.Application.Services.Search;

public sealed class SearchService(IUnitOfWork unitOfWork, IMapper mapper) : ISearchService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<SearchBookDto>> FindBooksAsync(string query, CancellationToken ct = default)
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
            .Select(b => new SearchBookDto
            {
                Id = b.Id,
                Title = b.Title,
                Authors = string.Join(", ", b.Authors.Select(a => a.Author!.Name)),
            })
            .ToListAsync(ct);
    }

    public async Task<BookDetailsDto?> GetBookDetailsAsync(int id, CancellationToken ct = default)
    {
        return await _unitOfWork
            .Books.GetQueryable()
            .AsSplitQuery()
            .ProjectTo<BookDetailsDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(b => !b.IsDeleted && b.Id == id, ct);
    }
}
