using Bookano.Application.DTOs.Subscribers;
using Bookano.Web.ViewModels.Rentals;
using Bookano.Web.ViewModels.Subscribers;

namespace Bookano.Web.Mapping
{
    public class SubscriberProfile : Profile
    {
        public SubscriberProfile()
        {
            CreateMap<SubscriberDto, SubscriberSearchResultViewModel>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

            CreateMap<SubscriberDto, SubscriberViewModel>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

            CreateMap<SubscriberDto,SubscriberFormViewModel>().ReverseMap();

            CreateMap<SubscriberSaveDto, SubscriberFormViewModel>()
             .ForMember(dest => dest.Image, opt => opt.Ignore());

            CreateMap<SubscriberFormViewModel, SubscriberSaveDto>()
                .ForMember(dest => dest.Image, opt => opt.Ignore());

            CreateMap<SubscriptionDto, SubscriptionViewModel>();


        }
    }
}
