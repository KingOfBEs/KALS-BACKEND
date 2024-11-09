namespace KALS.API.Models.Report;

public class DayReportResponse
{
    public DateTime Date { get; set; }
    public PaymentReport PaymentReport { get; set; }
    public OrderReport OrderReport { get; set; }
    
}