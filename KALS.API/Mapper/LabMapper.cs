using AutoMapper;
using KALS.API.Models.GoogleDrive;
using KALS.API.Models.Lab;
using KALS.Domain.Entities;
using KALS.Domain.Paginate;

namespace KALS.API.Mapper;

public class LabMapper: Profile
{
    public LabMapper()
    {
        CreateMap<CreateLabRequest, Lab>();
        CreateMap<Lab, LabResponse>()
            .ForMember(dest => dest.Product, 
                opt => opt.MapFrom(src => src.Product));
        CreateMap<Lab, LabNoProductResponse>();
    }
}