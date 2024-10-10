using KALS.Domain.DataAccess;
using KALS.Domain.Entities;
using KALS.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace KALS.Repository.Implement;

public class PaymentRepository: GenericRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(KitAndLabDbContext context) : base(context)
    {
        
    }

    public async Task<Payment> GetPaymentByOrderCode(int orderCode)
    {
        var payment = await SingleOrDefaultAsync(
            predicate: p => p.OrderCode == orderCode,
            include: p => p.Include(p => p.Order)
        );
        return payment;
    }
}