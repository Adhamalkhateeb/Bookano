using AutoMapper;
using Bookano.Web.Core.Models;

namespace Bookano.Web.Core.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Category, CategoryViewModel>();
            CreateMap<CategoryFormViewModel, Category>().ReverseMap();
            CreateMap<Author, AuthorViewModel>();
            CreateMap<AuthorFormViewModel, Author>().ReverseMap();
        }
    }

}
