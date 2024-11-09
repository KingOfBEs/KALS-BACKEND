namespace KALS.API.Models.Report;

public class ReportResponse
{
    public List<DayReportResponse> DayReportResponses { get; set; }
    public List<WeekReportResponse> WeekReportResponses { get; set; }
    public int OrderCount { get; set; }
    public int OrderPendingCount { get; set; }
    public int OrderPrepareCount { get; set; }
    public int OrderShippingCount { get; set; }
    public int OrderCompletedCount { get; set; }
    public int OrderCancelledCount { get; set; }
    
    public int OrderRefuseReceiveCount { get; set; }
    public int ProductCount { get; set; }
    public decimal PaymentTotal { get; set; }
    public int MemberCount { get; set; }
    public int LabCount { get; set; }
}