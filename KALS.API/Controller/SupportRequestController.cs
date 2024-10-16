using KALS.API.Constant;
using KALS.API.Models.SupportRequest;
using KALS.API.Services.Interface;
using KALS.Domain.Filter.FilterModel;
using KALS.Domain.Paginate;
using Microsoft.AspNetCore.Mvc;

namespace KALS.API.Controller;

[ApiController]
[Route(ApiEndPointConstant.SupportRequest.SupportRequestEndPoint)]
public class SupportRequestController: BaseController<SupportRequestController>
{
    private readonly ISupportRequestService _supportRequestService;
    public SupportRequestController(ILogger<SupportRequestController> logger, ISupportRequestService supportRequestService) : base(logger)
    {
        _supportRequestService = supportRequestService;
    }
    [HttpPost(ApiEndPointConstant.SupportRequest.SupportRequestEndPoint)]
    [ProducesResponseType(typeof(SupportRequestResponse), statusCode: StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateSupportRequest([FromForm] SupportRequest request)
    {
        var response = await _supportRequestService.CreateSupportRequest(request);
        if (response == null)
        {
            _logger.LogError($"Create support request failed");
            return Problem($"{MessageConstant.SupportRequest.CreateSupportRequestFail}");
        }
        _logger.LogInformation($"Create support request successful with {response.Id}");
        return Ok(response);
    }
    [HttpPost(ApiEndPointConstant.SupportRequest.SupportMessage)]
    [ProducesResponseType(typeof(SupportMessageResponse), statusCode: StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateSupportMessage(Guid id , [FromForm] ResponseSupportRequest request)
    {
        var response = await _supportRequestService.ResponseSupportMessage(id, request);
        if (response == null)
        {
            _logger.LogError($"Response support message failed");
            return Problem($"{MessageConstant.SupportRequest.ResponseSupportRequestFail}");
        }
        _logger.LogInformation($"Response support message successful with {response.Id}");
        return Ok(response);
    }
    [HttpGet(ApiEndPointConstant.SupportRequest.SupportRequestEndPoint)]
    [ProducesResponseType(typeof(IPaginate<SupportRequestResponse>), statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSupportRequestPagingAsync(int page = 1, int size = 30, [FromQuery] SupportRequestFilter? filter = null, string? sortBy = null, bool isAsc = true)
    {
        var response = await _supportRequestService.GetSupportRequestPagingAsync(page, size, filter, sortBy, isAsc);
        return Ok(response);
    }
}