using Bookano.Application.DTOs.Publishers;

namespace Bookano.Application.Services.Publishers;

public interface IPublisherService
{
    Task<IEnumerable<PublisherDto>> GetAllAsync(CancellationToken ct = default);
    Task<PublisherDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PublisherDto?> AddAsync(PublisherFormDto formDto, CancellationToken ct = default);
    Task<PublisherDto?> UpdateAsync(int id, PublisherFormDto formDto, CancellationToken ct = default);
    Task<PublisherDto?> ToggleAsync(int id, CancellationToken ct = default);
    Task<bool> IsPublisherAllowedAsync(PublisherFormDto formDto, CancellationToken ct = default);
}