
using Bookano.Application.DTOs.BookCopies;

namespace Bookano.Application.Mappings;

public class BookCopyProfile : Profile
{
    public BookCopyProfile()
    {
        CreateMap<BookCopy, BookCopyDto>()
            .ForMember(dest => dest.BookIsAvailableForRental, opt => opt.MapFrom(src => !src.Book!.IsAvailableForRental))
            .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book!.Title))
            .ForMember(dest => dest.BookImageUrl, opt => opt.MapFrom(src => src.Book!.ImageUrl))
            .ForMember(dest => dest.BookThumbnailUrl, opt => opt.MapFrom(src => src.Book!.ImageThumbnailUrl));
        CreateMap<BookCopySaveDto, BookCopy>();



    }
}
