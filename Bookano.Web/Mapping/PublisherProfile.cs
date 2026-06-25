using Bookano.Application.DTOs.Publishers;
using Bookano.Web.ViewModels.Publishers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookano.Web.Mapping;

public class PublisherProfile : Profile
{
    public PublisherProfile()
    {
        CreateMap<PublisherDto, PublisherViewModel>();
        CreateMap<PublisherDto, PublisherFormViewModel>().ReverseMap();
        CreateMap<PublisherFormViewModel, PublisherSaveDto>().ReverseMap();


        CreateMap<PublisherDto, SelectListItem>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(c => c.Id))
            .ForMember(dest => dest.Text, opt => opt.MapFrom(c => c.Name));
    }
}