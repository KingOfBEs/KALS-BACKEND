using AutoMapper;
using KALS.API.Models.User;
using KALS.Domain.Entities;
using KALS.Domain.Paginate;

namespace KALS.API.Mapper;

public class MemberMapper: Profile
{
    public MemberMapper()
    {
        CreateMap<Member, MemberResponse>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.Province, opt => opt.MapFrom(src => src.Province))
            .ForMember(dest => dest.District, opt => opt.MapFrom(src => src.District))
            .ForMember(dest => dest.Commune, opt => opt.MapFrom(src => src.Commune))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.ProvinceCode, opt => opt.MapFrom(src => src.ProvinceCode))
            .ForMember(dest => dest.DistrictCode, opt => opt.MapFrom(src => src.DistrictCode))
            .ForMember(dest => dest.CommuneCode, opt => opt.MapFrom(src => src.CommuneCode));
    }
}