using KALS.API.Services.Interface;

namespace KALS.API.Services.Implement;

public class CancelOrderService : BackgroundService
{
    private readonly ILogger<CancelOrderService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public CancelOrderService(IServiceProvider serviceProvider, ILogger<CancelOrderService> logger)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CancelOrderService running at: {time}", DateTimeOffset.Now);
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
                    await paymentService.UpdateExpiredPayment();
                    
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogInformation("Error: " + e.Message);
            await Task.Delay(600000, stoppingToken);
            await ExecuteAsync(stoppingToken);
        }
    }
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("Worker STARTING");
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("Worker STOPPING: {time}", DateTimeOffset.Now);
        return base.StopAsync(cancellationToken);
    }
}