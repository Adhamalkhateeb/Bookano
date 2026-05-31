using Bookano.Application.DTOs.Rentals;
using Bookano.Web.ViewModels.BookCopies;
using Bookano.Web.ViewModels.Rentals;

namespace Bookano.Web.Mapping
{
    public class RentalProfile : Profile
    {
        public RentalProfile()
        {
            CreateMap<RentalBookCopyDto, BookCopyViewModel>().ReverseMap();
            CreateMap<RentalCopyDto, RentalCopyViewModel>().ReverseMap();
            CreateMap<RentalDto, RentalViewModel>().ReverseMap();
            CreateMap<RentalFormDto, RentalFormViewModel>().ReverseMap();
            CreateMap<RentalReturnDto, RentalReturnFormViewModel>().ReverseMap();
        }
        
    }
}
