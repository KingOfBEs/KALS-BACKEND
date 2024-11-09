using KALS.API.Constant;
using KALS.API.Models.Report;
using KALS.API.Services.Interface;
using KALS.API.Validator;
using KALS.Domain.Enums;
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
//                  | \_|  ''\---/ư''  |   |                       //
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
[Route(ApiEndPointConstant.Report.ReportEndPoint)]
public class ReportController: BaseController<ReportController>
{
    private readonly IReportService _reportService;
    public ReportController(ILogger<ReportController> logger, IReportService reportService) : base(logger)
    {
        _reportService = reportService;
    }
    
    [HttpGet(ApiEndPointConstant.Report.ReportEndPoint)]
    [ProducesResponseType(typeof(ReportResponse), StatusCodes.Status200OK)]
    [CustomAuthorize(RoleEnum.Manager, RoleEnum.Staff)]
    public async Task<IActionResult> GetReport()
    {
        var report = await _reportService.GetReport();
        return Ok(report);
    }
}