using KALS.API.Constant;
using KALS.API.Models.Lab;
using KALS.API.Services.Interface;
using KALS.Domain.Paginate;
using Microsoft.AspNetCore.Mvc;

namespace KALS.API.Controller;

[ApiController]
[Route(ApiEndPointConstant.Lab.LabEndPoint)]
public class LabController: BaseController<LabController>
{
    private readonly ILabService _labService;
    public LabController(ILogger<LabController> logger, ILabService labService) : base(logger)
    {
        _labService = labService;
    }
    
    [HttpGet(ApiEndPointConstant.Lab.LabEndPoint)]
    [ProducesResponseType(typeof(IPaginate<LabResponse>), statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllLabs(int page = 1, int size = 30, string? searchName = null)
    {
        var response = await _labService.GetLabsAsync(page, size, searchName);
        return Ok(response);
    }
    [HttpGet(ApiEndPointConstant.Lab.LabById)]
    [ProducesResponseType(typeof(IPaginate<LabResponse>), statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLabById(Guid id)
    {
        var response = await _labService.GetLabByIdAsync(id);
        return Ok(response);
    }
    [HttpPost(ApiEndPointConstant.Lab.LabEndPoint)]
    [ProducesResponseType(typeof(LabResponse), statusCode: StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateLab([FromForm] CreateLabRequest request)
    {
        var response = await _labService.CreateLabAsync(request);
        if (response == null)
        {
            _logger.LogError($"Create new lab failed with {request.Name}");
            return Problem($"{MessageConstant.Lab.CreateLabFail}: {request.Name}");
        }
        _logger.LogInformation($"Create new lab successful with {request.Name}");
        return CreatedAtAction(nameof(CreateLab), response);
    }
    
}