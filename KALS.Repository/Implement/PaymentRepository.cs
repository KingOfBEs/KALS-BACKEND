using KALS.Domain.DataAccess;
using KALS.Domain.Entities;
using KALS.Domain.Enums;
using KALS.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace KALS.Repository.Implement;

public class PaymentRepository: GenericRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(DbContext context) : base(context)
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

    public async Task<ICollection<Payment>> GetPaymentExpiredList()
    {
        var paymentExpires = await GetListAsync(
            predicate: p => p.Status == PaymentStatus.Processing && p.CreatedAt.AddMinutes(10) < DateTime.Now,
            include: p => p.Include(p => p.Order)
            );
        return paymentExpires;
    }
}