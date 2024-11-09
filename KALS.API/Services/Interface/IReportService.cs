using KALS.API.Models.Order;
using KALS.API.Models.Report;

namespace KALS.API.Services.Interface;

public interface IReportService
{
    Task<ReportResponse> GetReport();
}