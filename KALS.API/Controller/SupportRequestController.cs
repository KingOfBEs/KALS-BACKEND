using KALS.API.Constant;
using KALS.API.Models.SupportRequest;
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
[Route(ApiEndPointConstant.SupportRequest.SupportRequestEndPoint)]
public class SupportRequestController: BaseController<SupportRequestController>
{
    private readonly ISupportRequestService _supportRequestService;
    public SupportRequestController(ILogger<SupportRequestController> logger, ISupportRequestService supportRequestService) : base(logger)
    {
        _supportRequestService = supportRequestService;
    }
    [HttpPost(ApiEndPointConstant.SupportRequest.SupportRequestEndPoint)]
    [ProducesResponseType(typeof(SupportRequestResponse), statusCode: StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    [CustomAuthorize(RoleEnum.Manager, RoleEnum.Member, RoleEnum.Staff)]
    public async Task<IActionResult> CreateSupportRequest([FromBody] SupportRequest request)
    {
        var response = await _supportRequestService.CreateSupportRequest(request);
        if (response == null)
        {
            _logger.LogError($"Create support request failed");
            return Problem($"{MessageConstant.SupportRequest.CreateSupportRequestFail}");
        }
        _logger.LogInformation($"Create support request successful with {response.Id}");
        return CreatedAtAction(nameof(CreateSupportRequest), response);
    }
    [HttpPost(ApiEndPointConstant.SupportRequest.SupportMessage)]
    [ProducesResponseType(typeof(SupportMessageResponse), statusCode: StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    [CustomAuthorize(RoleEnum.Staff)]
    public async Task<IActionResult> CreateSupportMessage(Guid id , [FromBody] ResponseSupportRequest request)
    {
        var response = await _supportRequestService.ResponseSupportMessage(id, request);
        if (response == null)
        {
            _logger.LogError($"Response support message failed");
            return Problem($"{MessageConstant.SupportRequest.ResponseSupportRequestFail}");
        }
        _logger.LogInformation($"Response support message successful with {response.Id}");
        return CreatedAtAction(nameof(CreateSupportMessage), response);
    }
    [HttpGet(ApiEndPointConstant.SupportRequest.SupportRequestEndPoint)]
    [ProducesResponseType(typeof(IPaginate<SupportRequestResponse>), statusCode: StatusCodes.Status200OK)]
    [CustomAuthorize(RoleEnum.Manager, RoleEnum.Member, RoleEnum.Staff)]
    public async Task<IActionResult> GetSupportRequestPagingAsync(int page = 1, int size = 30, [FromQuery] SupportRequestFilter? filter = null, string? sortBy = null, bool isAsc = true)
    {
        var response = await _supportRequestService.GetSupportRequestPagingAsync(page, size, filter, sortBy, isAsc);
        return Ok(response);
    }
}