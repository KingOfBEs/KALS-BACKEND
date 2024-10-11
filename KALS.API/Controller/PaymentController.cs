using KALS.API.Constant;
using KALS.API.Models.Cart;
using KALS.API.Models.Payment;
using KALS.API.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace KALS.API.Controller;
[ApiController]
[Route(ApiEndPointConstant.Payment.PaymentEndPoint)]
public class PaymentController: BaseController<PaymentController>
{
    private readonly IPaymentService _paymentService;
    public PaymentController(ILogger<PaymentController> logger, IPaymentService paymentService) : base(logger)
    {
        _paymentService = paymentService;
    }
    [HttpPost(ApiEndPointConstant.Payment.PaymentCheckOut)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutRequest request)
    {
            var result = await _paymentService.CheckOut(request);
            if (result == null)
            {
                _logger.LogError($"Check out failed");
                return Problem(MessageConstant.Payment.CheckOutFail);
            }
            _logger.LogInformation("Check out successful");
            return Ok(result);
    }
    [HttpPost(ApiEndPointConstant.Payment.PaymentEndPoint)]
    [ProducesResponseType(typeof(PaymentWithOrderResponse), statusCode: StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePaymentStatus([FromBody] UpdatePaymentOrderStatusRequest request)
    {
        var response = await _paymentService.HandlePayment(request);
        if (response == null)
        {
            _logger.LogError($"Update payment status failed with {request.OrderCode}");
            return Problem($"{MessageConstant.Payment.UpdateStatusPaymentAndOrderFail}: {request.OrderCode}");
        }
        _logger.LogInformation($"Update payment status successful with {request.OrderCode}");
        return Ok(response);
    }
    
}