using AutoMapper;
using KALS.API.Models.SupportRequest;
using SupportRequest = KALS.Domain.Entities.SupportRequest;

namespace KALS.API.Mapper;

public class SupportRequestMapper: Profile
{
    public SupportRequestMapper()
    {
        CreateMap<SupportRequest, SupportRequestResponse>()
            .ForMember(dest => dest.NumberOfRequest,
                opt => opt.MapFrom(src => src.LabMember.NumberOfRequest))
            .ForMember(dest => dest.SupportMessages,
                opt => opt.MapFrom(src => src.SupportMessages))
            .ForMember(dest => dest.Member,
                opt => opt.MapFrom(src => src.Member))
            .ForMember(dest => dest.Lab,
                opt => opt.MapFrom(src => src.Lab));
    }
}