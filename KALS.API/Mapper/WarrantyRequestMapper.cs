using AutoMapper;
using KALS.API.Models.WarrantyRequest;
using KALS.Domain.Entities;

namespace KALS.API.Mapper;

public class WarrantyRequestMapper: Profile
{
    public WarrantyRequestMapper()
    {
        CreateMap<WarrantyRequest, WarrantyRequestWithImageResponse>()
            .ForMember(dest => dest.WarrantyRequestImages,
                opt => opt.MapFrom(src => src.WarrantyRequestImages))
            .ForMember(dest => dest.Member,
                opt => opt.MapFrom(src => src.OrderItem.Order.Member))
            .ForMember(dest => dest.WarrantyCode,
                opt => opt.MapFrom(src => src.OrderItem.WarrantyCode))
            .ForMember(dest => dest.WarrantyExpired,
                opt => opt.MapFrom(src => src.OrderItem.WarrantyExpired));
        CreateMap<CreateWarrantyRequestRequest, WarrantyRequest>();
    }
}