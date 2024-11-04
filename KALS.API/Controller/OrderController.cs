using KALS.API.Constant;
using KALS.API.Models.Order;
using KALS.API.Models.OrderItem;
using KALS.API.Services.Implement;
using KALS.API.Services.Interface;
using KALS.API.Validator;
using KALS.Domain.Enums;
using KALS.Domain.Filter.FilterModel;
using KALS.Domain.Paginate;
using Microsoft.AspNetCore.Mvc;

namespace KALS.API.Controller;
////////////////////////////////////////////////////////////////////
//                          _ooOoo_                               //
//                         o8888888o                              //
//                         88" . "88                              //
//                         (| ^_^ |)                              //
//                         O\  =  /O                              //
//                      ____/`---'\____                           //
//                    .'  \\|     |//  `.                         //
//                   /  \\|||  :  |||//  \                        //
//                  /  _||||| -:- |||||-  \                       //
//                  |   | \\\  -  /// |   |                       //
//                  | \_|  ''\---/''  |   |                       //
//                  \  .-\__  `-`  ___/-. /                       //
//                ___`. .'  /--.--\  `. . ___                     //
//              ."" '<  `.___\_<|>_/___.'  >'"".                  //
//            | | :  `- \`.;`\ _ /`;.`/ - ` : | |                 //
//            \  \ `-.   \_ __\ /__ _/   .-` /  /                 //
//      ========`-.____`-.___\_____/___.-`____.-'========         //
//                           `=---='                              //
//      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^        //
//                                                                //
////////////////////////////////////////////////////////////////////
[ApiController]
[Route(ApiEndPointConstant.Order.OrderEndpoint)]
public class OrderController: BaseController<OrderController>
{
    private readonly IOrderService _orderService;
    public OrderController(ILogger<OrderController> logger, IOrderService orderService) : base(logger)
    {
        _orderService = orderService;
    }
    
    [HttpGet(ApiEndPointConstant.Order.OrderEndpoint)]
    [ProducesResponseType(typeof(IPaginate<OrderResponse>), statusCode: StatusCodes.Status200OK)]
    [CustomAuthorize(RoleEnum.Manager, RoleEnum.Staff, RoleEnum.Member)]
    public async Task<IActionResult> GetOrderList([FromQuery] int page = 1, [FromQuery] int size = 30, 
        [FromQuery] OrderFilter? filter = null, [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true)
    {
        var result = await _orderService.GetOrderList(page, size, filter, sortBy, isAsc);
        return Ok(result);
    }
    [HttpPatch(ApiEndPointConstant.Order.UpdateSupportOrderStatusSuccess)]
    [ProducesResponseType(typeof(OrderResponse), statusCode: StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    [CustomAuthorize(RoleEnum.Manager, RoleEnum.Staff)]
    public async Task<IActionResult> UpdateOrderStatusCompleted(Guid id)
    {
        var result = await _orderService.UpdateOrderStatusCompleted(id);
        if (result == null)
        {
            _logger.LogError($"Update order status completed failed with {id}");
            return Problem($"{MessageConstant.Order.UpdateOrderStatusFail}: {id}");
        }
        _logger.LogInformation($"Update order status completed successful with {id}");
        return Ok(result);
    }
    [HttpPatch(ApiEndPointConstant.Order.UpdateSupportOrderStatusRefuse)]
    [ProducesResponseType(typeof(OrderResponse), statusCode: StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    [CustomAuthorize(RoleEnum.Manager, RoleEnum.Staff)]
    public async Task<IActionResult> UpdateOrderStatusRefuse(Guid id)
    {
        var result = await _orderService.UpdateOrderStatusCancelled(id);
        if (result == null)
        {
            _logger.LogError($"Update order status refused failed with {id}");
            return Problem($"{MessageConstant.Order.UpdateOrderStatusFail}: {id}");
        }
        _logger.LogInformation($"Update order status refused successful with {id}");
        return Ok(result);
    }
    [HttpPatch(ApiEndPointConstant.Order.UpdateSupportOrderStatusShipping)]
    [ProducesResponseType(typeof(OrderResponse), statusCode: StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    [CustomAuthorize(RoleEnum.Manager, RoleEnum.Staff)]
    public async Task<IActionResult> UpdateOrderStatusShipping(Guid id)
    {
        var result = await _orderService.UpdateOrderStatusShipping(id);
        if (result == null)
        {
            _logger.LogError($"Update order status shipping failed with {id}");
            return Problem($"{MessageConstant.Order.UpdateOrderStatusFail}: {id}");
        }
        _logger.LogInformation($"Update order status shipping successful with {id}");
        return Ok(result);
    }
    [HttpGet(ApiEndPointConstant.Order.OrderItems)]
    [ProducesResponseType(typeof(ICollection<OrderItemResponse>), statusCode: StatusCodes.Status200OK)]
    [CustomAuthorize(RoleEnum.Manager, RoleEnum.Staff, RoleEnum.Member)]
    public async Task<IActionResult> GetOrderItemsByOrderId(Guid id)
    {
        var result = await _orderService.GetOrderItemsByOrderId(id);
        return Ok(result);
    }
}