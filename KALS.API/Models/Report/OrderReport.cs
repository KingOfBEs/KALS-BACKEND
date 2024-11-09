namespace KALS.API.Models.Report;

public class OrderReport
{
    public int TotalOrder { get; set; }
    public int TotalOrderPending { get; set; }
    public int TotalOrderPrepare { get; set; }
    public int TotalOrderShipping { get; set; }
    public int TotalOrderCompleted { get; set; }
    public int TotalOrderCancelled { get; set; }
    public int TotalOrderRefuseReceive { get; set; }
}