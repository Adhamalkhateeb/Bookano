using Bookano.Application.DTOs.Books;

namespace Bookano.Application.Mappings;

public class BookProfile : Profile
{
    public BookProfile()
    {
        CreateMap<Book, BookListDto>()
       .ForMember(dest => dest.Publisher, opt => opt.MapFrom(src => src.Publisher!.Name))
       .ForMember(dest => dest.Authors, opt => opt.MapFrom(src =>
           src.Authors.Select(a => a.Author!.Name)));

        CreateMap<Book, BookDetailsDto>()
            .ForMember(dest => dest.Publisher, opt => opt.MapFrom(src => src.Publisher!.Name))
            .ForMember(dest => dest.Authors, opt => opt.MapFrom(src =>
                src.Authors.Select(a => a.Author!.Name)))
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src =>
                src.Categories.Select(c => c.Category!.Name)));


        CreateMap<BookFormDto, Book>()
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Categories, opt => opt.Ignore())
                .ForMember(dest => dest.Authors, opt => opt.Ignore())
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.ImageThumbnailUrl, opt => opt.Ignore())
                .ForMember(dest => dest.ImagePublicId, opt => opt.Ignore())
                .ForMember(dest => dest.RowVersion, opt => opt.Ignore()); 

        CreateMap<Book, BookFormDto>()
            .ForMember(dest => dest.SelectedCategories,
                opt => opt.MapFrom(src => src.Categories.Select(c => c.CategoryId)))
            .ForMember(dest => dest.SelectedAuthors,
                opt => opt.MapFrom(src => src.Authors.Select(a => a.AuthorId)))
            .ForMember(dest => dest.ExistingImagePublicId,
                opt => opt.MapFrom(src => src.ImagePublicId))
            .ForMember(dest => dest.Image, opt => opt.Ignore())
            .ForMember(dest => dest.RemoveImage, opt => opt.Ignore());




    }
}
