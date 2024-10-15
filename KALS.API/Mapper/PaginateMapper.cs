using AutoMapper;
using KALS.Domain.Paginate;

namespace KALS.API.Mapper;

public class PaginateMapper: Profile
{
    public PaginateMapper()
    {
        CreateMap(typeof(IPaginate<>), typeof(IPaginate<>)).ConvertUsing(typeof(PaginateConverter<,>));
    }
}