namespace KALS.API.Models.Report;

public class WeekReportResponse
{
    public int Week { get; set; }
    public PaymentReport PaymentReport { get; set; }
    public OrderReport OrderReport { get; set; }
}