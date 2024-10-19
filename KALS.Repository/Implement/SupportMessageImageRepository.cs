using KALS.Domain.DataAccess;
using KALS.Domain.Entities;
using KALS.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace KALS.Repository.Implement;

public class SupportMessageImageRepository: GenericRepository<SupportMessageImage>, ISupportMessageImageRepository 
{
    public SupportMessageImageRepository(DbContext context) : base(context)
    {
    }
}