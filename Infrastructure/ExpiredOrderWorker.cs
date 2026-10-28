using QLSK.Features.DonVe;

namespace QLSK.Infrastructure;

public class ExpiredOrderWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<ExpiredOrderWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await XuLyDonHetHan();
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Expire order worker failed"
                );
            }

            await Task.Delay(
                TimeSpan.FromSeconds(30),
                stoppingToken
            );
        }
    }

    private async Task XuLyDonHetHan()
    {
        using var scope = scopeFactory.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<DonVeService>();

        var count = await service.HetHan();

        if (count > 0)
        {
            logger.LogInformation(
                "Expired {Count} orders",
                count
            );
        }
    }
}