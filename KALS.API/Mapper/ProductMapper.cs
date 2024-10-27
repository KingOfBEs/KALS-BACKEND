using AutoMapper;
using KALS.API.Models.Cart;
using KALS.API.Models.Product;
using KALS.Domain.Entities;

namespace KALS.API.Mapper;

public class ProductMapper: Profile
{
    public ProductMapper()
    {
        CreateMap<CreateProductRequest, Product>();
        CreateMap<Product, GetProductResponse>()
            .ForMember(dest => dest.ProductImages, 
                opt => opt.MapFrom(src => src.ProductImages));
        CreateMap<Product, CartModelResponse>();
        CreateMap<Product, ProductWithLabResponse>()
            .ForMember(dest => dest.Labs, 
                opt => opt.MapFrom(src => src.Labs));
        CreateMap<Product, GetProductWithCatogoriesResponse>()
            .ForMember(dest => dest.Categories, 
                opt => opt.MapFrom(src => src.ProductCategories!.Select(pc => pc.Category)))
            .ForMember(dest => dest.ProductImages, 
                opt => opt.MapFrom(src => src.ProductImages));
    }
}