using Microsoft.EntityFrameworkCore;
using QLSK.Domain.Entities;

namespace QLSK.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<VaiTro> VaiTros => Set<VaiTro>();
    public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
    public DbSet<YeuCauXacThucToChuc> YeuCauXacThucToChucs => Set<YeuCauXacThucToChuc>();
    public DbSet<LichSuYeuCauXacThucToChuc> LichSuYeuCauXacThucToChucs => Set<LichSuYeuCauXacThucToChuc>();
    public DbSet<BanToChuc> BanToChucs => Set<BanToChuc>();
    public DbSet<LoaiSuKien> LoaiSuKiens => Set<LoaiSuKien>();
    public DbSet<SuKien> SuKiens => Set<SuKien>();
    public DbSet<DiaDiem> DiaDiems => Set<DiaDiem>();
    public DbSet<DatDiaDiem> DatDiaDiems => Set<DatDiaDiem>();
    public DbSet<LoaiVe> LoaiVes => Set<LoaiVe>();
    public DbSet<DonVe> DonVes => Set<DonVe>();
    public DbSet<ChiTietDonVe> ChiTietDonVes => Set<ChiTietDonVe>();
    public DbSet<ThanhToan> ThanhToans => Set<ThanhToan>();
    public DbSet<Ve> Ves => Set<Ve>();
    public DbSet<XuatVe> XuatVes => Set<XuatVe>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);
        model.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
