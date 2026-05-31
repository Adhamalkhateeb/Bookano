using Bookano.Application.DTOs.Rentals;

namespace Bookano.Application.Mappings;

public class RentalProfile : Profile
{
    public RentalProfile()
    {
        CreateMap<BookCopy, RentalBookCopyDto>()
            .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book!.Title))
            .ForMember(dest => dest.BookImageUrl, opt => opt.MapFrom(src => src.Book!.ImageUrl))
            .ForMember(
                dest => dest.BookThumbnailUrl,
                opt => opt.MapFrom(src => src.Book!.ImageThumbnailUrl)
            );

        CreateMap<RentalCopy, RentalCopyDto>()
            .ForMember(dest => dest.BookCopy, opt => opt.MapFrom(src => src.BookCopy))
            .ReverseMap();

        CreateMap<Rental, RentalDto>()
            .ForMember(dest => dest.TotalDelayInDays, opt => opt.Ignore())
            .ForMember(dest => dest.NumberOfCopies, opt => opt.Ignore())
            .ForMember(dest => dest.ActiveCopies, opt => opt.Ignore());
    }
}
