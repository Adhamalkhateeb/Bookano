using Bookano.Application.DTOs.Rentals;
using Bookano.Web.ViewModels.BookCopies;
using Bookano.Web.ViewModels.Rentals;

namespace Bookano.Web.Mapping
{
    public class RentalProfile : Profile
    {
        public RentalProfile()
        {
            CreateMap<Bookano.Application.DTOs.BookCopies.BookCopyDto, BookCopyViewModel>().ReverseMap();
            CreateMap<RentalCopyDto, RentalCopyViewModel>().ReverseMap();
            CreateMap<RentalDto, RentalViewModel>().ReverseMap();
            CreateMap<RentalSaveDto, RentalFormViewModel>().ReverseMap();
            
            CreateMap<RentalCopyReturnDto, RentalCopyViewModel>()
                .ForMember(dest => dest.BookCopy, opt => opt.MapFrom(src => new BookCopyViewModel { Id = src.BookCopyId }));
            
            CreateMap<RentalCopyViewModel, RentalCopyReturnDto>()
                .ForMember(dest => dest.BookCopyId, opt => opt.MapFrom(src => src.BookCopy!.Id));

            CreateMap<RentalReturnDto, RentalReturnFormViewModel>().ReverseMap();
        }
    }
}
