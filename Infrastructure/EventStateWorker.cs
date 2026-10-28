using Microsoft.EntityFrameworkCore;
using QLSK.Data;
using QLSK.Domain.Enums;

namespace QLSK.Infrastructure;

public class EventStateWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<EventStateWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CapNhatTrangThai(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Event state worker failed"
                );
            }

            await Task.Delay(
                TimeSpan.FromMinutes(1),
                stoppingToken
            );
        }
    }

    private async Task CapNhatTrangThai(
        CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var now = DateTime.UtcNow;

        var dangBatDau = await db.SuKiens
            .Where(x =>
                x.TrangThai == TrangThaiSuKien.DaDuyet &&
                x.BatDau <= now &&
                x.KetThuc > now)
            .ToListAsync(stoppingToken);

        foreach (var suKien in dangBatDau)
        {
            suKien.TrangThai =
                TrangThaiSuKien.DangDienRa;
        }

        var daKetThuc = await db.SuKiens
            .Where(x =>
                (
                    x.TrangThai == TrangThaiSuKien.DaDuyet ||
                    x.TrangThai == TrangThaiSuKien.DangDienRa
                )
                && x.KetThuc <= now)
            .ToListAsync(stoppingToken);

        foreach (var suKien in daKetThuc)
        {
            suKien.TrangThai =
                TrangThaiSuKien.DaKetThuc;
        }

        if (dangBatDau.Count + daKetThuc.Count > 0)
        {
            await db.SaveChangesAsync(stoppingToken);
        }
    }
}