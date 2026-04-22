using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookano.Web.Core.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        //Authors
        CreateMap<Author, AuthorViewModel>();
        CreateMap<AuthorFormViewModel, Author>().ReverseMap();
        CreateMap<Author, SelectListItem>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(c => c.Id))
            .ForMember(dest => dest.Text, opt => opt.MapFrom(c => c.Name));

        //Books
        CreateMap<BookFormViewModel, Book>()
            .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
            .ForMember(dest => dest.ImageThumbnailUrl, opt => opt.Ignore())
            .ForMember(dest => dest.Categories, opt => opt.Ignore())
            .ForMember(dest => dest.Authors, opt => opt.Ignore());

        CreateMap<Book, BookFormViewModel>()
            .ForMember(dest => dest.Categories, opt => opt.Ignore())
            .ForMember(dest => dest.Authors, opt => opt.Ignore());

        CreateMap<Book, BookViewModel>()
            .ForMember(
                dest => dest.Authors,
                opt => opt.MapFrom(b => b.Authors.Select(a => a.Author!.Name).ToList())
            )
            .ForMember(
                dest => dest.Categories,
                opt => opt.MapFrom(b => b.Categories.Select(a => a.Category!.Name).ToList())
            )
            .ForMember(dest => dest.Publisher, opt => opt.MapFrom(b => b.Publisher!.Name));
        ;

        //Categories
        CreateMap<Category, CategoryViewModel>();
        CreateMap<CategoryFormViewModel, Category>().ReverseMap();
        CreateMap<Category, SelectListItem>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(c => c.Id))
            .ForMember(dest => dest.Text, opt => opt.MapFrom(c => c.Name));

        //Publishers
        CreateMap<Publisher, PublisherViewModel>();
        CreateMap<PublisherFormViewModel, Publisher>().ReverseMap();
        CreateMap<Publisher, SelectListItem>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(c => c.Id))
            .ForMember(dest => dest.Text, opt => opt.MapFrom(c => c.Name));
    }
}
