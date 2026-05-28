using Bookano.Application.DTOs.Authors;

namespace Bookano.Application.Mappings;

public class AuthorProfile : Profile
{
    public AuthorProfile()
    {
        CreateMap<Author, AuthorDto>();
        CreateMap<AuthorFormDto, Author>();
    }
}
