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
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName));
    }
}