using System.Data;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Bookano.Web.Core.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        //Areas
        CreateMap<Area, AreaViewModel>()
            .ForMember(dest => dest.Governorate, opt => opt.MapFrom(a => a.Governorate!.Name));
        CreateMap<AreaFormViewModel, Area>().ReverseMap();
        CreateMap<Area, SelectListItem>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(c => c.Id))
            .ForMember(dest => dest.Text, opt => opt.MapFrom(c => c.Name));

        //Authors
        CreateMap<Author, AuthorViewModel>();
        CreateMap<AuthorFormViewModel, Author>().ReverseMap();
        CreateMap<Author, SelectListItem>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(c => c.Id))
            .ForMember(dest => dest.Text, opt => opt.MapFrom(c => c.Name));

        //Books
        CreateMap<BookFormViewModel, Book>()
            .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
            .ForMember(dest => dest.ImageThumbnailUrl, opt => opt.Ignore())
            .ForMember(dest => dest.ImagePublicId, opt => opt.Ignore())
            .ForMember(dest => dest.IdempotencyKey, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.Categories, opt => opt.Ignore())
            .ForMember(dest => dest.Authors, opt => opt.Ignore());

        CreateMap<Book, BookFormViewModel>()
            .ForMember(
                dest => dest.ExistingImagePublicId,
                opt => opt.MapFrom(src => src.ImagePublicId)
            )
            .ForMember(dest => dest.Categories, opt => opt.Ignore())
            .ForMember(dest => dest.Authors, opt => opt.Ignore());

        CreateMap<Book, BookViewModel>()
            .ForMember(
                dest => dest.Authors,
                opt => opt.MapFrom(b => b.Authors.Select(a => a.Author!.Name).ToList())
            )
            .ForMember(
                dest => dest.Categories,
                opt => opt.MapFrom(b => b.Categories.Select(a => a.Category!.Name).ToList())
            )
            .ForMember(dest => dest.Publisher, opt => opt.MapFrom(b => b.Publisher!.Name));

        //BookCopies
        CreateMap<BookCopy, BookCopyViewModel>()
            .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(bc => bc.Book!.Title))
            .ForMember(dest => dest.BookImageUrl, opt => opt.MapFrom(bc => bc.Book!.ImageUrl))
            .ForMember(
                dest => dest.BookThumbnailUrl,
                opt => opt.MapFrom(bc => bc.Book!.ImageThumbnailUrl)
            )
            .ForMember(dest => dest.BookId, opt => opt.MapFrom(bc => bc.Book!.Id));

        CreateMap<BookCopy, BookCopyFormViewModel>();

        //Categories
        CreateMap<Category, CategoryViewModel>();
        CreateMap<CategoryFormViewModel, Category>().ReverseMap();
        CreateMap<Category, SelectListItem>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(c => c.Id))
            .ForMember(dest => dest.Text, opt => opt.MapFrom(c => c.Name));

        //Governorates

        CreateMap<Governorate, SelectListItem>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(c => c.Id))
            .ForMember(dest => dest.Text, opt => opt.MapFrom(c => c.Name));

        //Publishers
        CreateMap<Publisher, PublisherViewModel>();
        CreateMap<PublisherFormViewModel, Publisher>().ReverseMap();
        CreateMap<Publisher, SelectListItem>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(c => c.Id))
            .ForMember(dest => dest.Text, opt => opt.MapFrom(c => c.Name));

        //Rentals
        CreateMap<Rental, RentalViewModel>();
        CreateMap<RentalCopy, RentalCopyViewModel>();

        //Subscribers
        CreateMap<Subscriber, SubscriberViewModel>()
            .ForMember(dest => dest.Governorate, opt => opt.MapFrom(s => s.Governorate!.Name))
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
