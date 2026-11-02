using QLSK.Features.DonVe;

namespace QLSK.Infrastructure;

public class ExpiredOrderWorker(
    IServiceScopeFactory boTaoPhamViDichVu,
    ILogger<ExpiredOrderWorker> boGhiLog)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken tokenDungWorker)
    {
        while (!tokenDungWorker.IsCancellationRequested)
        {
            try
            {
                await XuLyDonHetHan();
            }
            catch (Exception ngoaiLe)
            {
                boGhiLog.LogError(
                    ngoaiLe,
                    "Expire order worker failed"
                );
            }

            await Task.Delay(
                TimeSpan.FromSeconds(30),
                tokenDungWorker
            );
        }
    }

    private async Task XuLyDonHetHan()
    {
        using var phamViDichVu = boTaoPhamViDichVu.CreateScope();

        var dichVuDonVe = phamViDichVu.ServiceProvider
            .GetRequiredService<DonVeService>();

        var soDonHetHan = await dichVuDonVe.XuLyCacDonVeHetHan();

        if (soDonHetHan > 0)
        {
            boGhiLog.LogInformation(
                "Expired {Count} orders",
                soDonHetHan
            );
        }
    }
}