using Bookano.Application.DTOs.Authors;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookano.Web.Core.Mapping
{
    public class AuthorsProfile : Profile
    {
        public AuthorsProfile()
        {
            CreateMap<AuthorDto, AuthorViewModel>();
            CreateMap<AuthorDto, AuthorFormViewModel>().ReverseMap();
            CreateMap<AuthorFormViewModel, AuthorFormDto>().ReverseMap();

            CreateMap<AuthorDto, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(c => c.Id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(c => c.Name));
        }
    }
}
