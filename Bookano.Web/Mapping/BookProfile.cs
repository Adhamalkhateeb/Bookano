using Bookano.Application.DTOs.Books;
using Bookano.Web.ViewModels.Books;

namespace Bookano.Web.Mapping
{
    public class BookProfile : Profile
    {
        public BookProfile()
        {

            CreateMap<BookListDto, BookViewModel>();
            CreateMap<BookDto, BookViewModel>();
            CreateMap<BookDetailsDto, BookViewModel>();

            CreateMap<BookDto, BookFormViewModel>()
                .ForMember(dest => dest.SelectedCategories, opt => opt.Ignore())
                .ForMember(dest => dest.SelectedAuthors, opt => opt.Ignore())
                .ForMember(dest => dest.ExistingImagePublicId, opt => opt.MapFrom(src => src.ImagePublicId))
                .ForMember(dest => dest.Image, opt => opt.Ignore());

            CreateMap<BookFormViewModel, BookSaveDto>()
                .ForMember(dest => dest.Image, opt => opt.Ignore())
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.ImageThumbnailUrl, opt => opt.Ignore());

        }       
    }           
}
