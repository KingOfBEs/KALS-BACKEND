using AutoMapper;
using KALS.API.Models.SupportRequest;
using KALS.Domain.Entities;

namespace KALS.API.Mapper;

public class SupportMessageImageMapper: Profile
{
    public SupportMessageImageMapper()
    {
        CreateMap<SupportMessageImage, SupportMessageImageResponse>();
    }
}