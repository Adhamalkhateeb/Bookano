namespace Bookano.Web.Core.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DateOnly, DateTime>().ConvertUsing(d => d.ToDateTime(TimeOnly.MinValue));

        CreateMap<DateTime, DateOnly>().ConvertUsing(d => DateOnly.FromDateTime(d));




        //Rentals
        CreateMap<Rental, RentalViewModel>();
        CreateMap<RentalCopy, RentalCopyViewModel>();

        //Subscribers
        CreateMap<Subscriber, SubscriberViewModel>()
            .ForMember(dest => dest.Governorate, opt => opt.MapFrom(s => s.Area!.Governorate!.Name))
            .ForMember(dest => dest.Area, opt => opt.MapFrom(s => s.Area!.Name))
            .ForMember(
                dest => dest.FullName,
                opt => opt.MapFrom(s => $"{s.FirstName} {s.LastName}")
            );
        ;
        CreateMap<Subscriber, SubscriberFormViewModel>();
        CreateMap<SubscriberFormViewModel, Subscriber>()
            .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
            .ForMember(dest => dest.ImageThumbnailUrl, opt => opt.Ignore())
            .ForMember(dest => dest.ImagePublicId, opt => opt.Ignore());
        CreateMap<Subscriber, SubscriberSearchResultViewModel>()
            .ForMember(
                dest => dest.FullName,
                opt => opt.MapFrom(s => $"{s.FirstName} {s.LastName}")
            );

        //Subscription
        CreateMap<Subscription, SubscriptionViewModel>();

        //Users
        CreateMap<ApplicationUser, UserViewModel>()
            .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(u => u.CreatedOnUtc))
            .ForMember(dest => dest.LastUpdatedOn, opt => opt.MapFrom(u => u.LastUpdatedOnUtc));

        CreateMap<UserFormViewModel, ApplicationUser>()
            .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(u => u.Email.ToUpper()))
            .ForMember(
                dest => dest.NormalizedUserName,
                opt => opt.MapFrom(u => u.UserName.ToUpper())
            )
            .ReverseMap();
    }
}
