
using Microsoft.EntityFrameworkCore;
using QLSK.Data.Configs;
using QLSK.Domain.Entities;

namespace QLSK.Data;

public class AppDbContext(DbContextOptions<AppDbContext> tuy_chon_luu)
    : DbContext(tuy_chon_luu)
{
    public DbSet<TaiKhoan> TaiKhoan => Set<TaiKhoan>();

    public DbSet<HoSoBTC> HoSoBTC => Set<HoSoBTC>();
    public DbSet<GiayToBTC> GiayToBTC => Set<GiayToBTC>();
    public DbSet<PhanCongSuKien> PhanCongSuKien => Set<PhanCongSuKien>();

    public DbSet<SuKien> SuKien => Set<SuKien>();
    public DbSet<PhienBanSuKien> PhienBanSuKien => Set<PhienBanSuKien>();
    public DbSet<SuatDien> SuatDien => Set<SuatDien>();
    public DbSet<DanhMucSuKien> DanhMucSuKien => Set<DanhMucSuKien>();

    public DbSet<DiaDiem> DiaDiem => Set<DiaDiem>();
    public DbSet<KhuVuc> KhuVuc => Set<KhuVuc>();
    public DbSet<Ghe> Ghe => Set<Ghe>();
    public DbSet<CumGhe> CumGhe => Set<CumGhe>();
    public DbSet<YeuCauThue> YeuCauThue => Set<YeuCauThue>();
    public DbSet<LichDatDiaDiem> LichDatDiaDiem => Set<LichDatDiaDiem>();

    public DbSet<LoaiVe> LoaiVe => Set<LoaiVe>();
    public DbSet<DonHang> DonHang => Set<DonHang>();
    public DbSet<ChiTietDonHang> ChiTietDonHang => Set<ChiTietDonHang>();
    public DbSet<GiuGhe> GiuGhe => Set<GiuGhe>();
    public DbSet<Ve> Ve => Set<Ve>();
    public DbSet<VeMoi> VeMoi => Set<VeMoi>();
    public DbSet<NhatKyCheckIn> NhatKyCheckIn => Set<NhatKyCheckIn>();
    public DbSet<Voucher> Voucher => Set<Voucher>();

    public DbSet<SuDungVoucher> SuDungVoucher => Set<SuDungVoucher>();
    public DbSet<GiaoDichThanhToan> GiaoDichThanhToan => Set<GiaoDichThanhToan>();
    public DbSet<WebhookThanhToan> WebhookThanhToan => Set<WebhookThanhToan>();
    public DbSet<YeuCauHoanTien> YeuCauHoanTien => Set<YeuCauHoanTien>();
    public DbSet<GiaoDichHoanTien> GiaoDichHoanTien => Set<GiaoDichHoanTien>();
    public DbSet<HopDong> HopDong => Set<HopDong>();
    public DbSet<DoiSoat> DoiSoat => Set<DoiSoat>();
    public DbSet<GiaiNgan> GiaiNgan => Set<GiaiNgan>();

    public DbSet<ThongBao> ThongBao => Set<ThongBao>();
    public DbSet<KhieuNai> KhieuNai => Set<KhieuNai>();
    public DbSet<DanhGia> DanhGia => Set<DanhGia>();
    public DbSet<Banner> Banner => Set<Banner>();
    public DbSet<CauHinhHeThong> CauHinhHeThong => Set<CauHinhHeThong>();
    public DbSet<NhatKyHeThong> NhatKyHeThong => Set<NhatKyHeThong>();
    public DbSet<ChinhSach> ChinhSach => Set<ChinhSach>();

    protected override void ConfigureConventions(ModelConfigurationBuilder quy_uoc_bang)
    {
        quy_uoc_bang.Properties<DateTime>().HaveColumnType("datetime2(3)");
        quy_uoc_bang.Properties<decimal>().HavePrecision(19, 0);
    }

    protected override void OnModelCreating(ModelBuilder mo_hinh_bang)
    {
        base.OnModelCreating(mo_hinh_bang);
        mo_hinh_bang.ApplyConfiguration(new TaiKhoanConfig());
        CauHinhTaiKhoan.CauHinh(mo_hinh_bang);
        CauHinhSuKien.CauHinh(mo_hinh_bang);
        CauHinhDiaDiem.CauHinh(mo_hinh_bang);
        CauHinhDatVe.CauHinh(mo_hinh_bang);
        CauHinhTaiChinh.CauHinh(mo_hinh_bang);
        CauHinhQuanTri.CauHinh(mo_hinh_bang);
    }
}
