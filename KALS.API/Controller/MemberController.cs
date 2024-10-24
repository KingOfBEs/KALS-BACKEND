using KALS.API.Constant;
using KALS.API.Models.User;
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
[Route(ApiEndPointConstant.Member.MemberEndpoint)]
public class MemberController: BaseController<MemberController>
{
    private readonly IUserService _userService;
    public MemberController(ILogger<MemberController> logger, IUserService userService) : base(logger)
    {
        _userService = userService;
    }
    
    [HttpGet(ApiEndPointConstant.Member.MemberEndpoint)]
    [ProducesResponseType(typeof(IPaginate<MemberResponse>), statusCode: StatusCodes.Status200OK)]
    [CustomAuthorize(RoleEnum.Manager, RoleEnum.Staff)]
    public async Task<IActionResult> GetMembersAsync([FromQuery] int page = 1, [FromQuery] int size = 30, 
        [FromQuery] MemberFilter? filter = null, [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true)
    {
        var members = await _userService.GetMembersAsync(page, size, filter, sortBy, isAsc);
        return Ok(members);
    }
    [HttpPatch(ApiEndPointConstant.Member.MemberById)]
    [ProducesResponseType(typeof(MemberResponse), statusCode: StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    [CustomAuthorize(RoleEnum.Manager, RoleEnum.Staff)]
    public async Task<IActionResult> UpdateMemberAsyncByManager(Guid id, [FromBody] UpdateMemberRequest request)
    {
        var member = await _userService.UpdateMemberAsyncByManager(id, request);
        if (member == null)
        {
            _logger.LogError($"Update member failed with {request.Username}");
            return Problem($"{MessageConstant.User.UpdateMemberFail}: {request.Username}");
        }
        _logger.LogInformation($"Update member successful with {request.Username}");
        return Ok(member);
    }
    [HttpPatch(ApiEndPointConstant.Member.MemberEndpoint)]
    [ProducesResponseType(typeof(MemberResponse), statusCode: StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    [CustomAuthorize(RoleEnum.Member, RoleEnum.Manager, RoleEnum.Staff)]
    public async Task<IActionResult> UpdateMemberAsync([FromBody] UpdateMemberRequest request)
    {
        var member = await _userService.UpdateMemberAsync(request);
        if (member == null)
        {
            _logger.LogError($"Update member failed with {request.Username}");
            return Problem($"{MessageConstant.User.UpdateMemberFail}: {request.Username}");
        }
        _logger.LogInformation($"Update member successful with {request.Username}");
        return Ok(member);
    }
    [HttpGet(ApiEndPointConstant.Member.MemberInformation)]
    [ProducesResponseType(typeof(MemberResponse), statusCode: StatusCodes.Status200OK)]
    [CustomAuthorize(RoleEnum.Member, RoleEnum.Manager, RoleEnum.Staff)]
    public async Task<IActionResult> GetMemberInformationAsync()
    {
        var member = await _userService.GetMemberInformationAsync();
        return Ok(member);
    }
}