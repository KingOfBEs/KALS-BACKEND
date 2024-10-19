using KALS.Domain.DataAccess;
using KALS.Domain.Entities;
using KALS.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace KALS.Repository.Implement;

public class SupportMessageRepository: GenericRepository<SupportMessage>, ISupportMessageRepository
{
    public SupportMessageRepository(DbContext context) : base(context)
    {
    }
}