using KALS.API.Constant;
using KALS.API.Models.User;
using KALS.API.Services.Interface;
using KALS.API.Validator;
using KALS.Domain.Enums;
using KALS.Domain.Filter.FilterModel;
using KALS.Domain.Paginate;
using Microsoft.AspNetCore.Mvc;

namespace KALS.API.Controller;

[ApiController]
[Route(ApiEndPointConstant.Staff.StaffEndpoint)]
public class StaffController: BaseController<StaffController>
{
    private readonly IUserService _userService;
    public StaffController(ILogger<StaffController> logger, IUserService userService) : base(logger)
    {
        _userService = userService;
    }
    
    [HttpGet(ApiEndPointConstant.Staff.StaffEndpoint)]
    [ProducesResponseType(typeof(IPaginate<StaffResponse>), statusCode: StatusCodes.Status200OK)]
    [CustomAuthorize(RoleEnum.Manager)]
    public async Task<IActionResult> GetStaffsAsync([FromQuery] int page = 1, [FromQuery] int size = 30, 
        [FromQuery] StaffFilter? filter = null, [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true)
    {
        var staffs = await _userService.GetStaffsAsync(page, size, filter, sortBy, isAsc);
        return Ok(staffs);
    }
    [HttpPatch(ApiEndPointConstant.Staff.StaffById)]
    [ProducesResponseType(typeof(StaffResponse), statusCode: StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    [CustomAuthorize(RoleEnum.Manager, RoleEnum.Staff)]
    public async Task<IActionResult> UpdateStaffAsync(Guid id, [FromBody] UpdateStaffRequest request)
    {
        var staff = await _userService.UpdateStaffAsync(id, request);
        if (staff == null)
        {
            _logger.LogError($"Create new staff failed with {request.Username}");
            return Problem($"{MessageConstant.User.UpdateStaffFail}: {request.Username}");
        }
        _logger.LogInformation($"Create new staff successful with {request.Username}");
        return Ok(staff);
    }
    [HttpPost(ApiEndPointConstant.Staff.StaffEndpoint)]
    [ProducesResponseType(typeof(StaffResponse), statusCode: StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    [CustomAuthorize(RoleEnum.Manager)]
    public async Task<IActionResult> CreateStaff([FromBody] CreateStaffRequest request)
    {
        var staff = await _userService.CreateStaffAsync(request);
        if (staff == null)
        {
            _logger.LogError($"Create new staff failed with {request.Username}");
            return Problem($"{MessageConstant.User.CreateStaffFail}: {request.Username}");
        }
        _logger.LogInformation($"Create new staff successful with {request.Username}");
        return CreatedAtAction(nameof(CreateStaff), staff);
    }
    [HttpGet(ApiEndPointConstant.Staff.StaffById)]
    [ProducesResponseType(typeof(StaffResponse), statusCode: StatusCodes.Status200OK)]
    [CustomAuthorize(RoleEnum.Manager)]
    public async Task<IActionResult> GetStaffById(Guid id)
    {
        var staff = await _userService.GetStaffById(id);
        return Ok(staff);
    }
    
}