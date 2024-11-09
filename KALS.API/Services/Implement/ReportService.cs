using AutoMapper;
using KALS.API.Models.Report;
using KALS.API.Services.Interface;
using KALS.API.Utils;
using KALS.Domain.Entities;
using KALS.Domain.Enums;
using KALS.Repository.Interface;

namespace KALS.API.Services;

public class ReportService : BaseService<ReportService>, IReportService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly ILabRepository _labRepository;
    public ReportService(ILogger<ReportService> logger, IMapper mapper, 
        IHttpContextAccessor httpContextAccessor, IConfiguration configuration,
        IPaymentRepository paymentRepository, IOrderRepository orderRepository, IProductRepository productRepository,
        IMemberRepository memberRepository, ILabRepository labRepository) : base(logger, mapper, httpContextAccessor, configuration)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _memberRepository = memberRepository;
        _labRepository = labRepository;
    }

    private async Task<List<DayReportResponse>> GetDayReport()
    {
        var endDate = TimeUtil.GetCurrentSEATime().AddHours(24).Date;
        var startDate = endDate.AddDays(-6).Date;
        var allDates = Enumerable.Range(-1, (endDate - startDate).Days + 1)
            .Select(offset => startDate.AddDays(offset).Date)
            .ToList();
        var paymentList = await _paymentRepository.GetSuccessPaymentByDate(startDate, endDate);
        var paymentGroups = paymentList.GroupBy(p => p.Order.ModifiedAt.Date)
            .ToDictionary(g => g.Key, g => g.ToList());
        var orders = await _orderRepository.GetOrdersByDate(startDate, endDate);
        var orderList = orders.GroupBy(p => p.CreatedAt.Date)
            .ToDictionary(g => g.Key , g => g.ToList());

        var dayReportList = allDates.Select(date => new DayReportResponse()
        {
            Date = date,
            PaymentReport = paymentGroups.TryGetValue(date, out List<Payment> payments) ? new PaymentReport()
            {
                PaymentSuccessQuantity = payments.Count,
                PaymentSuccessTotal = payments.Sum(p => p.Amount)
            } : null,
            OrderReport = orderList.TryGetValue(date, out List<Order> order) ? new OrderReport()
            {
                TotalOrder = order.Count,
                TotalOrderCompleted = order.Count(o => o.Status == OrderStatus.Completed),
                TotalOrderCancelled = order.Count(o => o.Status == OrderStatus.Cancelled),
                TotalOrderPending = order.Count(o => o.Status == OrderStatus.Pending),
                TotalOrderPrepare = order.Count(o => o.Status == OrderStatus.Prepare),
                TotalOrderRefuseReceive = order.Count(o => o.Status == OrderStatus.RefuseReceive),
                TotalOrderShipping = order.Count(o => o.Status == OrderStatus.Shipping)
            } : null
        }).ToList();
        return dayReportList;
    }

    private async Task<List<WeekReportResponse>> GetWeekReport()
    {
        var reports = new List<WeekReportResponse>();
        for (int i = 4; i >= 1; i--)
        {
            var endDate = TimeUtil.GetCurrentSEATime().AddHours(24).AddDays(-7 * (i - 1)).Date;
            var startDate = endDate.AddDays(-6).Date;
            var paymentList = await _paymentRepository.GetSuccessPaymentByDate(startDate, endDate);
            var orderList = await _orderRepository.GetOrdersByDate(startDate, endDate);
            reports.Add(new WeekReportResponse()
            {
                Week = i,
                PaymentReport = new PaymentReport()
                {
                    PaymentSuccessQuantity = paymentList.Count,
                    PaymentSuccessTotal = paymentList.Sum(p => p.Amount)
                },
                OrderReport = new OrderReport()
                {
                    TotalOrder = orderList.Count,
                    TotalOrderCompleted = orderList.Count(o => o.Status == OrderStatus.Completed),
                    TotalOrderCancelled = orderList.Count(o => o.Status == OrderStatus.Cancelled),
                    TotalOrderPending = orderList.Count(o => o.Status == OrderStatus.Pending),
                    TotalOrderPrepare = orderList.Count(o => o.Status == OrderStatus.Prepare),
                    TotalOrderRefuseReceive = orderList.Count(o => o.Status == OrderStatus.RefuseReceive),
                    TotalOrderShipping = orderList.Count(o => o.Status == OrderStatus.Shipping)
                }
            });
        }

        return reports;
    }

    public async Task<ReportResponse> GetReport()
    {
        var report = new ReportResponse();
        
        report.DayReportResponses = await GetDayReport();
        report.WeekReportResponses = await GetWeekReport();
        
        var orders = await _orderRepository.GetOrderList();
        report.OrderCount = orders.Count;
        report.OrderPendingCount = orders.Count(o => o.Status == OrderStatus.Pending);
        report.OrderPrepareCount = orders.Count(o => o.Status == OrderStatus.Prepare);
        report.OrderShippingCount = orders.Count(o => o.Status == OrderStatus.Shipping);
        report.OrderCompletedCount = orders.Count(o => o.Status == OrderStatus.Completed);
        report.OrderCancelledCount = orders.Count(o => o.Status == OrderStatus.Cancelled);
        report.OrderRefuseReceiveCount = orders.Count(o => o.Status == OrderStatus.RefuseReceive);
        var products = await _productRepository.GetListAsync();
        report.ProductCount = products.Count;
        
        var paymentSuccess = await _paymentRepository.GetSuccessPaymentByDate(null, null);
        report.PaymentTotal = paymentSuccess.Sum(p=> p.Amount);
        
        var members = await _memberRepository.GetListAsync();
        report.MemberCount = members.Count;
        
        var labs = await _labRepository.GetListAsync();
        report.LabCount = labs.Count;

        return report;
    }
}