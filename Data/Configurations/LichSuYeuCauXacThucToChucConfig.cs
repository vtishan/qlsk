using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class LichSuYeuCauXacThucToChucConfig : IEntityTypeConfiguration<LichSuYeuCauXacThucToChuc>
{
    public void Configure(EntityTypeBuilder<LichSuYeuCauXacThucToChuc> cauHinhLichSuYeuCauXacThucToChuc)
    {
        cauHinhLichSuYeuCauXacThucToChuc.ToTable("LichSuYeuCauXacThucToChuc");
        cauHinhLichSuYeuCauXacThucToChuc.HasKey(lichSuXacThucDb => lichSuXacThucDb.LichSuYeuCauXacThucToChucId);
        cauHinhLichSuYeuCauXacThucToChuc.Property(lichSuXacThucDb => lichSuXacThucDb.TenToChuc).HasMaxLength(200).IsRequired();
        cauHinhLichSuYeuCauXacThucToChuc.Property(lichSuXacThucDb => lichSuXacThucDb.GiayTo).HasMaxLength(500).IsRequired();
        cauHinhLichSuYeuCauXacThucToChuc.Property(lichSuXacThucDb => lichSuXacThucDb.LyDo).HasMaxLength(500);
        cauHinhLichSuYeuCauXacThucToChuc.HasIndex(lichSuXacThucDb => new { lichSuXacThucDb.TaiKhoanId, lichSuXacThucDb.NgayXuLy });
        cauHinhLichSuYeuCauXacThucToChuc.HasOne(lichSuXacThucDb => lichSuXacThucDb.TaiKhoan).WithMany(taiKhoanDb => taiKhoanDb.LichSuYeuCauXacThuc).HasForeignKey(lichSuXacThucDb => lichSuXacThucDb.TaiKhoanId).OnDelete(DeleteBehavior.Restrict);
        cauHinhLichSuYeuCauXacThucToChuc.HasOne(lichSuXacThucDb => lichSuXacThucDb.NguoiXuLy).WithMany().HasForeignKey(lichSuXacThucDb => lichSuXacThucDb.NguoiXuLyId).OnDelete(DeleteBehavior.Restrict);
    }
}
