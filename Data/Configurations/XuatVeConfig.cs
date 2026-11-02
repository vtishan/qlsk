using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class XuatVeConfig : IEntityTypeConfiguration<XuatVe>
{
    public void Configure(EntityTypeBuilder<XuatVe> cauHinhXuatVe)
    {
        cauHinhXuatVe.ToTable("XuatVe");
        cauHinhXuatVe.HasKey(xuatVeDb => xuatVeDb.XuatVeId);
        cauHinhXuatVe.Property(xuatVeDb => xuatVeDb.HinhThucXuat).HasMaxLength(30).IsRequired();
        cauHinhXuatVe.Property(xuatVeDb => xuatVeDb.EmailNhan).HasMaxLength(250);
        cauHinhXuatVe.HasIndex(xuatVeDb => new { xuatVeDb.VeId, xuatVeDb.NgayTao });
        cauHinhXuatVe.HasOne(xuatVeDb => xuatVeDb.VeDuocXuat).WithMany(veDaPhatHanhDb => veDaPhatHanhDb.LichSuXuatVe).HasForeignKey(xuatVeDb => xuatVeDb.VeId).OnDelete(DeleteBehavior.Restrict);
    }
}
