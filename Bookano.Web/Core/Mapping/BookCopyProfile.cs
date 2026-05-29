using Bookano.Application.DTOs.BookCopies;

namespace Bookano.Web.Core.Mapping
{
    public class BookCopyProfile : Profile
    {
        public BookCopyProfile()
        {
            CreateMap<BookCopyDto, BookCopyRowViewModel>();

            //CreateMap<BookCopy, BookCopyViewModel>()
            //    .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(bc => bc.Book!.Title))
            //    .ForMember(dest => dest.BookImageUrl, opt => opt.MapFrom(bc => bc.Book!.ImageUrl))
            //    .ForMember(
            //        dest => dest.BookThumbnailUrl,
            //        opt => opt.MapFrom(bc => bc.Book!.ImageThumbnailUrl)
            //    )
            //    .ForMember(dest => dest.BookId, opt => opt.MapFrom(bc => bc.Book!.Id));

            CreateMap<BookCopy, BookCopyFormViewModel>();

            CreateMap<BookCopyFormViewModel, BookCopyFormDto>();

            CreateMap<BookCopyDto, BookCopyFormViewModel>();
            CreateMap<BookCopyRentalHistoryDto, CopyHistoyViewModel>();

        }
    }
}
