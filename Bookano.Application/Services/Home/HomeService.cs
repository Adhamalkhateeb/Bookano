using Bookano.Application.Common.Interfaces;
using Bookano.Application.DTOs.Home;

namespace Bookano.Application.Services.Home;

public sealed class HomeService(IUnitOfWork unitOfWork) : IHomeService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<IEnumerable<HomeBookDto>> GetRecentlyAddedBooksAsync(CancellationToken ct = default)
    {
        return await _unitOfWork
            .Books.GetQueryable()
            .Where(b => !b.IsDeleted)
            .OrderByDescending(b => b.CreatedOnUtc)
            .Take(10)
            .Select(b => new HomeBookDto
            {
                Id = b.Id,
                Title = b.Title,
                ImageUrl = b.ImageUrl,
                Authors = b.Authors.Select(a => a.Author!.Name).ToList(),
            })
            .ToListAsync(ct);
    }
}
