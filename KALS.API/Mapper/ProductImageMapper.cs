using AutoMapper;
using KALS.API.Models.Product;
using KALS.Domain.Entities;

namespace KALS.API.Mapper;

public class ProductImageMapper: Profile
{
    public ProductImageMapper()
    {
        CreateMap<ProductImage, ProductImageResponse>();
        CreateMap<AddImageProductRequest, ProductImage>();
    }
}