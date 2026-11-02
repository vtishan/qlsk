using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class ThanhToanConfig : IEntityTypeConfiguration<ThanhToan>
{
    public void Configure(EntityTypeBuilder<ThanhToan> cauHinhThanhToan)
    {
        cauHinhThanhToan.ToTable("ThanhToan", bangThanhToan => bangThanhToan
            .HasCheckConstraint("CK_ThanhToan_SoTien", "[SoTien] > 0 AND [SoTienHoan] >= 0 AND [SoTienHoan] <= [SoTien]"));
        cauHinhThanhToan.HasKey(thanhToanDb => thanhToanDb.ThanhToanId);
        cauHinhThanhToan.Property(thanhToanDb => thanhToanDb.SoTien).HasPrecision(18, 2);
        cauHinhThanhToan.Property(thanhToanDb => thanhToanDb.SoTienHoan).HasPrecision(18, 2);
        cauHinhThanhToan.Property(thanhToanDb => thanhToanDb.MaGiaoDich).HasMaxLength(120);
        cauHinhThanhToan.Property(thanhToanDb => thanhToanDb.MaLienKet).HasMaxLength(120);
        cauHinhThanhToan.Property(thanhToanDb => thanhToanDb.MaHoanTien).HasMaxLength(120);
        cauHinhThanhToan.Property(thanhToanDb => thanhToanDb.PhienBanDong).IsRowVersion();
        cauHinhThanhToan.HasIndex(thanhToanDb => thanhToanDb.MaGiaoDich).IsUnique().HasFilter("[MaGiaoDich] IS NOT NULL");
        cauHinhThanhToan.HasIndex(thanhToanDb => thanhToanDb.MaLienKet).IsUnique().HasFilter("[MaLienKet] IS NOT NULL");
        cauHinhThanhToan.HasIndex(thanhToanDb => new { thanhToanDb.DonVeId, thanhToanDb.TrangThai });
        cauHinhThanhToan.HasOne(thanhToanDb => thanhToanDb.DonVe).WithMany(donVeDb => donVeDb.DanhSachThanhToan).HasForeignKey(thanhToanDb => thanhToanDb.DonVeId).OnDelete(DeleteBehavior.Restrict);
    }
}
