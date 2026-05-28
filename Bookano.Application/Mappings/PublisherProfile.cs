using Bookano.Application.DTOs.Publishers;

namespace Bookano.Application.Mappings;

public class PublisherProfile : Profile
{
    public PublisherProfile()
    {
        CreateMap<Publisher, PublisherDto>();
        CreateMap<PublisherFormDto, Publisher>();
    }
}