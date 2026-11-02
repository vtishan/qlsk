using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class LoaiSuKienConfig : IEntityTypeConfiguration<LoaiSuKien>
{
    public void Configure(EntityTypeBuilder<LoaiSuKien> cauHinhLoaiSuKien)
    {
        cauHinhLoaiSuKien.ToTable("LoaiSuKien");
        cauHinhLoaiSuKien.HasKey(loaiSuKienDb => loaiSuKienDb.LoaiSuKienId);
        cauHinhLoaiSuKien.Property(loaiSuKienDb => loaiSuKienDb.TenLoaiSuKien).HasMaxLength(120).IsRequired();
        cauHinhLoaiSuKien.Property(loaiSuKienDb => loaiSuKienDb.MoTaLoaiSuKien).HasMaxLength(500);
        cauHinhLoaiSuKien.HasIndex(loaiSuKienDb => loaiSuKienDb.TenLoaiSuKien).IsUnique();
    }
}
