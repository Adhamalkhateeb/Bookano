using AutoMapper;
using Bookano.Application.DTOs.Categories;
using Bookano.Domain.Entities;

namespace Bookano.Application.Mappings;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<Category, CategoryDto>();
        CreateMap<CategoryFormDto, Category>();
    }
}