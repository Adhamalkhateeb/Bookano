using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookano.Web.Core.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        //Categories
        CreateMap<Category, CategoryViewModel>();
        CreateMap<CategoryFormViewModel, Category>().ReverseMap();
        CreateMap<Category, SelectListItem>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(c => c.Id))
            .ForMember(dest => dest.Text, opt => opt.MapFrom(c => c.Name));

        //Authors
        CreateMap<Author, AuthorViewModel>();
        CreateMap<AuthorFormViewModel, Author>().ReverseMap();
        CreateMap<Author, SelectListItem>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(c => c.Id))
            .ForMember(dest => dest.Text, opt => opt.MapFrom(c => c.Name));

        //Books
        CreateMap<BookFormViewModel, Book>()
            .ReverseMap()
            .ForMember(dest => dest.Categories, opt => opt.Ignore());
    }
}
