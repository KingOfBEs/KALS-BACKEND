using AutoMapper;
using KALS.API.Models.WarrantyRequest;
using KALS.Domain.Entities;

namespace KALS.API.Mapper;

public class WarrantyRequestImageMapper: Profile
{
    public WarrantyRequestImageMapper()
    {
        CreateMap<WarrantyRequestImage, WarrantyRequestImageResponse>();
    }
}