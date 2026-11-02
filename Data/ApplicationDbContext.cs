// Đây là "cổng trung gian" giữa code C# và database.
// Mỗi DbSet bên dưới tương ứng với một bảng, dùng để đọc/thêm/sửa/xóa dữ liệu.
// Khi thao tác với DbSet, Entity Framework Core sẽ tự tạo câu lệnh SQL phù hợp để làm việc với database.

using Microsoft.EntityFrameworkCore;
using QLSK.Domain.Entities;

namespace QLSK.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> tuyChonCoSoDuLieu)
    : DbContext(tuyChonCoSoDuLieu)
{
    // Cho phép code truy cập và thao tác với bảng VaiTro.
    public DbSet<VaiTro> DanhSachVaiTro => Set<VaiTro>();

    public DbSet<TaiKhoan> DanhSachTaiKhoan => Set<TaiKhoan>();
    public DbSet<YeuCauXacThucToChuc> YeuCauXacThucToChuc => Set<YeuCauXacThucToChuc>();
    public DbSet<LichSuYeuCauXacThucToChuc> LichSuYeuCauXacThucToChuc => Set<LichSuYeuCauXacThucToChuc>();
    public DbSet<BanToChuc> DanhSachBanToChuc => Set<BanToChuc>();
    public DbSet<LoaiSuKien> DanhSachLoaiSuKien => Set<LoaiSuKien>();
    public DbSet<SuKien> DanhSachSuKien => Set<SuKien>();
    public DbSet<DiaDiem> DanhSachDiaDiem => Set<DiaDiem>();
    public DbSet<DatDiaDiem> DanhSachDatDiaDiem => Set<DatDiaDiem>();
    public DbSet<LoaiVe> DanhSachLoaiVe => Set<LoaiVe>();
    public DbSet<DonVe> DanhSachDonVe => Set<DonVe>();
    public DbSet<ChiTietDonVe> ChiTietDonVe => Set<ChiTietDonVe>();
    public DbSet<ThanhToan> DanhSachThanhToan => Set<ThanhToan>();
    public DbSet<Ve> DanhSachVe => Set<Ve>();
    public DbSet<XuatVe> LichSuXuatVe => Set<XuatVe>();

    // Khi EF Core khởi tạo database model, tự tìm các file Config
    // để biết khóa chính, khóa ngoại, độ dài, quan hệ và các ràng buộc của từng bảng.
    protected override void OnModelCreating(ModelBuilder boDungModelDuLieu)
    {
        base.OnModelCreating(boDungModelDuLieu);

        boDungModelDuLieu.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly
        );
    }
}