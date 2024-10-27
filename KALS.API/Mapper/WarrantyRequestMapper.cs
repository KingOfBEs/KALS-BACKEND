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
                opt => opt.MapFrom(src => src.WarrantyRequestImages));
        CreateMap<CreateWarrantyRequestRequest, WarrantyRequest>();
    }
}