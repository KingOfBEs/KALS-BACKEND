using KALS.API.Models.Cart;
using KALS.API.Models.Payment;
using Net.payOS.Types;

namespace KALS.API.Services.Interface;

public interface IPaymentService
{
    Task<string> CheckOut(CheckOutRequest request);
    Task<PaymentWithOrderResponse> HandlePayment(UpdatePaymentOrderStatusRequest request);

    Task<bool> UpdateExpiredPayment();
}