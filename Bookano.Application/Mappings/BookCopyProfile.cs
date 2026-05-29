
using Bookano.Application.DTOs.BookCopies;

namespace Bookano.Application.Mappings;

public class BookCopyProfile : Profile
{
    public BookCopyProfile()
    {
        CreateMap<BookCopy, BookCopyDto>()
            .ForMember(dest => dest.BookIsAvailableForRental,opt => opt.MapFrom(src => !src.Book!.IsAvailableForRental));
        CreateMap<BookCopyFormDto, BookCopy>();

        CreateMap<RentalCopy, BookCopyRentalHistoryDto>()
            .ForMember(dest => dest.SubscriberName, opt => opt.MapFrom(src => $"{src.Rental!.Subscriber!.FirstName} {src.Rental.Subscriber.LastName}"))
            .ForMember(dest => dest.SubscriberMobile, opt => opt.MapFrom(src => src.Rental!.Subscriber!.MobileNumber))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.RentalDate));

    }
}
