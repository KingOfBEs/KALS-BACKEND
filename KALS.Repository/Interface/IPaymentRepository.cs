using KALS.Domain.Entities;

namespace KALS.Repository.Interface;

public interface IPaymentRepository: IGenericRepository<Payment>
{
    Task<Payment> GetPaymentByOrderCode(int orderCode);
    
    Task<ICollection<Payment>> GetPaymentExpiredList();
}