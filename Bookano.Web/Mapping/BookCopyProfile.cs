using Bookano.Application.DTOs.BookCopies;
using Bookano.Application.DTOs.Rentals;
using Bookano.Web.ViewModels.BookCopies;

namespace Bookano.Web.Mapping
{
    public class BookCopyProfile : Profile
    {
        public BookCopyProfile()
        {
            CreateMap<BookCopyDto, BookCopyRowViewModel>();
            CreateMap<BookCopyDto, BookCopyFormViewModel>();


            CreateMap<BookCopyFormViewModel, BookCopySaveDto>();

            CreateMap<RentalCopyDto, CopyHistoyViewModel>()
                .ForMember(dest => dest.SubscriberName, opt => opt.MapFrom(src => $"{src.Subscriber!.FirstName} {src.Subscriber!.LastName}"))
                .ForMember(dest => dest.SubscriberMobile, opt => opt.MapFrom(src => src.Subscriber!.MobileNumber))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.RentalDate));        }
    }
}
