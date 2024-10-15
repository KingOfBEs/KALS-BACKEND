using AutoMapper;
using KALS.API.Models.Category;
using KALS.Domain.Entities;
using KALS.Domain.Paginate;

namespace KALS.API.Mapper;

public class CategoryMapper : Profile
{
    public CategoryMapper()
    {
        CreateMap<CreateCategoryRequest, Category>();
        CreateMap<Category, CategoryResponse>();
    }
}