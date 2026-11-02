using QLSK.Features.SuKien;

namespace QLSK.Infrastructure;

public class EventStateWorker(
    IServiceScopeFactory boTaoPhamViDichVu,
    ILogger<EventStateWorker> boGhiLog)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken tokenDungWorker)
    {
        while (!tokenDungWorker.IsCancellationRequested)
        {
            try
            {
                using var phamViDichVu = boTaoPhamViDichVu.CreateScope();
                var dichVuSuKien = phamViDichVu.ServiceProvider.GetRequiredService<SuKienService>();
                await dichVuSuKien.CapNhatVongDoiSuKienTheoThoiGian(tokenDungWorker);
            }
            catch (Exception ngoaiLe)
            {
                boGhiLog.LogError(ngoaiLe, "Event state worker failed");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), tokenDungWorker);
        }
    }
}
