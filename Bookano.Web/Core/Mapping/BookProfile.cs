using Bookano.Application.DTOs.Books;

namespace Bookano.Web.Core.Mapping
{
    public class BookProfile : Profile
    {
        public BookProfile()
        {
            //CreateMap<BookFormViewModel, Book>()
            //    .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
            //    .ForMember(dest => dest.ImageThumbnailUrl, opt => opt.Ignore())
            //    .ForMember(dest => dest.ImagePublicId, opt => opt.Ignore())
            //    .ForMember(dest => dest.IdempotencyKey, opt => opt.Ignore())
            //    .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            //    .ForMember(dest => dest.Categories, opt => opt.Ignore())
            //    .ForMember(dest => dest.Authors, opt => opt.Ignore());


            CreateMap<BookDetailsDto, BookViewModel>();
            CreateMap<BookFormViewModel, BookFormDto>()
                .ForMember(dest => dest.Image, opt => opt.Ignore())
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.ImageThumbnailUrl, opt => opt.Ignore());

            CreateMap<BookFormDto, BookFormViewModel>()
                .ForMember(dest => dest.Publishers, opt => opt.Ignore())
                .ForMember(dest => dest.Categories, opt => opt.Ignore())
                .ForMember(dest => dest.Authors, opt => opt.Ignore())
                .ForMember(dest => dest.Image, opt => opt.Ignore());
        }       
    }           
}
