using KALS.API.Constant;
using KALS.API.Models.WarrantyRequest;
using KALS.API.Services.Interface;
using KALS.API.Validator;
using KALS.Domain.Enums;
using KALS.Domain.Paginate;
using Microsoft.AspNetCore.Mvc;

namespace KALS.API.Controller;

[ApiController]
[Route(ApiEndPointConstant.WarrantyRequest.WarrantyRequestEndPoint)]
public class WarrantyRequestController: BaseController<WarrantyRequestController>
{
    private readonly IWarrantyRequestService _warrantyRequestService;
    public WarrantyRequestController(ILogger<WarrantyRequestController> logger, IWarrantyRequestService warrantyRequestService) : base(logger)
    {
        _warrantyRequestService = warrantyRequestService;
    }
    [HttpGet(ApiEndPointConstant.WarrantyRequest.WarrantyRequestEndPoint)]
    [ProducesResponseType(typeof(IPaginate<WarrantyRequestWithImageResponse>), statusCode: StatusCodes.Status200OK)]
    [CustomAuthorize(RoleEnum.Manager, RoleEnum.Member, RoleEnum.Staff)]
    public async Task<IActionResult> GetWarrantyRequestsAsync(int page = 1, int size = 30, Guid? memberId = null)
    {
        var response = await _warrantyRequestService.GetWarrantyRequestsAsync(page, size, memberId);
        return Ok(response);
    }
    [HttpPost(ApiEndPointConstant.WarrantyRequest.WarrantyRequestEndPoint)]
    [ProducesResponseType(typeof(WarrantyRequestWithImageResponse), statusCode: StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    [CustomAuthorize(RoleEnum.Member)]
    public async Task<IActionResult> CreateWarrantyRequest([FromBody] CreateWarrantyRequestRequest request)
    {
        var response = await _warrantyRequestService.CreateWarrantyRequestAsync(request);
        if (response == null)
        {
            _logger.LogError($"Create new warranty request fail with order item id: {request.OrderItemId}");
            return Problem($"{MessageConstant.WarrantyRequest.CreateWarrantyRequestFail}");
        }
        _logger.LogError($"Create new warranty request success with order item id: {request.OrderItemId}");
        return CreatedAtAction(nameof(CreateWarrantyRequest), response);
    }
    [HttpPatch(ApiEndPointConstant.WarrantyRequest.WarrantyRequestById)]
    [ProducesResponseType(typeof(WarrantyRequestWithImageResponse), statusCode: StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    [CustomAuthorize(RoleEnum.Staff)]
    public async Task<IActionResult> UpdateWarrantyRequest(Guid warrantyRequestId, [FromBody] UpdateWarrantyRequestRequest request)
    {
        var response = await _warrantyRequestService.UpdateWarrantyRequestAsync(warrantyRequestId, request);
        if (response == null)
        {
            _logger.LogError($"Update warranty request fail with warranty request id: {warrantyRequestId}");
            return Problem($"{MessageConstant.WarrantyRequest.UpdateWarrantyRequestFail}");
        }
        _logger.LogError($"Update warranty request success with warranty request id: {warrantyRequestId}");
        return Ok(response);
    }
    
}