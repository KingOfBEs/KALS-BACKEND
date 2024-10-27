using KALS.Domain.Entities;
using KALS.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace KALS.Repository.Implement;

public class WarrantyRequestImageRepository: GenericRepository<WarrantyRequestImage>, IWarrantyRequestImageRepository
{
    public WarrantyRequestImageRepository(DbContext context) : base(context)
    {
    }
}