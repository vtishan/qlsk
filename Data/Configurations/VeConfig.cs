using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class VeConfig : IEntityTypeConfiguration<Ve>
{
    public void Configure(EntityTypeBuilder<Ve> cauHinhVe)
    {
        cauHinhVe.ToTable("Ve");
        cauHinhVe.HasKey(veDaPhatHanhDb => veDaPhatHanhDb.VeId);
        cauHinhVe.Property(veDaPhatHanhDb => veDaPhatHanhDb.MaVe).HasMaxLength(40).IsRequired();
        cauHinhVe.Property(veDaPhatHanhDb => veDaPhatHanhDb.NoiDungMaQr).HasMaxLength(120).IsRequired();
        cauHinhVe.Property(veDaPhatHanhDb => veDaPhatHanhDb.PhienBanDong).IsRowVersion();
        cauHinhVe.HasIndex(veDaPhatHanhDb => veDaPhatHanhDb.MaVe).IsUnique();
        cauHinhVe.HasIndex(veDaPhatHanhDb => veDaPhatHanhDb.NoiDungMaQr).IsUnique();
        cauHinhVe.HasOne(veDaPhatHanhDb => veDaPhatHanhDb.DonVe).WithMany(donVeDb => donVeDb.DanhSachVe).HasForeignKey(veDaPhatHanhDb => veDaPhatHanhDb.DonVeId).OnDelete(DeleteBehavior.Restrict);
        cauHinhVe.HasOne(veDaPhatHanhDb => veDaPhatHanhDb.LoaiVe).WithMany(loaiVeDb => loaiVeDb.DanhSachVe).HasForeignKey(veDaPhatHanhDb => veDaPhatHanhDb.LoaiVeId).OnDelete(DeleteBehavior.Restrict);
    }
}
