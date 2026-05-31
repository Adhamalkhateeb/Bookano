using Bookano.Application.DTOs.Home;

namespace Bookano.Application.Services.Home;

public interface IHomeService
{
    Task<IEnumerable<HomeBookDto>> GetRecentlyAddedBooksAsync(CancellationToken ct = default);
}
