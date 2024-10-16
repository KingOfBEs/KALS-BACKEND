using KALS.Domain.DataAccess;
using KALS.Domain.Entities;
using KALS.Repository.Interface;

namespace KALS.Repository.Implement;

public class SupportMessageImageRepository: GenericRepository<SupportMessageImage>, ISupportMessageImageRepository 
{
    public SupportMessageImageRepository(KitAndLabDbContext context) : base(context)
    {
    }
}