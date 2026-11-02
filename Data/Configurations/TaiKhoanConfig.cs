using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class TaiKhoanConfig : IEntityTypeConfiguration<TaiKhoan>
{
    public void Configure(EntityTypeBuilder<TaiKhoan> cauHinhTaiKhoan)
    {
        cauHinhTaiKhoan.ToTable("TaiKhoan");
        cauHinhTaiKhoan.HasKey(taiKhoanDb => taiKhoanDb.TaiKhoanId);
        cauHinhTaiKhoan.Property(taiKhoanDb => taiKhoanDb.HoTen).HasMaxLength(120).IsRequired();
        cauHinhTaiKhoan.Property(taiKhoanDb => taiKhoanDb.Email).HasMaxLength(200).IsRequired();
        cauHinhTaiKhoan.Property(taiKhoanDb => taiKhoanDb.MatKhauBam).HasMaxLength(500).IsRequired();
        cauHinhTaiKhoan.Property(taiKhoanDb => taiKhoanDb.PhienBanDong).IsRowVersion();
        cauHinhTaiKhoan.HasIndex(taiKhoanDb => taiKhoanDb.Email).IsUnique();
        cauHinhTaiKhoan.HasOne(taiKhoanDb => taiKhoanDb.VaiTro).WithMany(vaiTroDb => vaiTroDb.DanhSachTaiKhoan).HasForeignKey(taiKhoanDb => taiKhoanDb.VaiTroId).OnDelete(DeleteBehavior.Restrict);
    }
}
