using AutoMapper;
using KALS.API.Models.Order;
using KALS.Domain.Entities;
using KALS.Domain.Paginate;

namespace KALS.API.Mapper;

public class OrderMapper: Profile
{
    public OrderMapper()
    {
        CreateMap<Order, OrderResponse>();
    }
}