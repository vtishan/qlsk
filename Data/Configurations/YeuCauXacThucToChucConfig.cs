using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class YeuCauXacThucToChucConfig : IEntityTypeConfiguration<YeuCauXacThucToChuc>
{
    public void Configure(EntityTypeBuilder<YeuCauXacThucToChuc> cauHinhYeuCauXacThucToChuc)
    {
        cauHinhYeuCauXacThucToChuc.ToTable("YeuCauXacThucToChuc");
        cauHinhYeuCauXacThucToChuc.HasKey(yeuCauXacThucDb => yeuCauXacThucDb.YeuCauXacThucToChucId);
        cauHinhYeuCauXacThucToChuc.Property(yeuCauXacThucDb => yeuCauXacThucDb.TenToChuc).HasMaxLength(200).IsRequired();
        cauHinhYeuCauXacThucToChuc.Property(yeuCauXacThucDb => yeuCauXacThucDb.GiayTo).HasMaxLength(500).IsRequired();
        cauHinhYeuCauXacThucToChuc.Property(yeuCauXacThucDb => yeuCauXacThucDb.LyDo).HasMaxLength(500);
        cauHinhYeuCauXacThucToChuc.HasIndex(yeuCauXacThucDb => yeuCauXacThucDb.TaiKhoanId).IsUnique();
        cauHinhYeuCauXacThucToChuc.HasOne(yeuCauXacThucDb => yeuCauXacThucDb.TaiKhoan).WithOne(taiKhoanDb => taiKhoanDb.YeuCauXacThuc).HasForeignKey<YeuCauXacThucToChuc>(yeuCauXacThucDb => yeuCauXacThucDb.TaiKhoanId).OnDelete(DeleteBehavior.Cascade);
        cauHinhYeuCauXacThucToChuc.HasOne(yeuCauXacThucDb => yeuCauXacThucDb.NguoiXuLy).WithMany().HasForeignKey(yeuCauXacThucDb => yeuCauXacThucDb.NguoiXuLyId).OnDelete(DeleteBehavior.Restrict);
    }
}
