using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class BanToChucConfig : IEntityTypeConfiguration<BanToChuc>
{
    public void Configure(EntityTypeBuilder<BanToChuc> cauHinhBanToChuc)
    {
        cauHinhBanToChuc.ToTable("BanToChuc");
        cauHinhBanToChuc.HasKey(banToChucDb => banToChucDb.BanToChucId);
        cauHinhBanToChuc.Property(banToChucDb => banToChucDb.TenBanToChuc).HasMaxLength(200).IsRequired();
        cauHinhBanToChuc.Property(banToChucDb => banToChucDb.MoTaBanToChuc).HasMaxLength(1000);
        cauHinhBanToChuc.HasIndex(banToChucDb => banToChucDb.TaiKhoanId).IsUnique();
        cauHinhBanToChuc.HasOne(banToChucDb => banToChucDb.TaiKhoan).WithOne(taiKhoanDb => taiKhoanDb.BanToChuc).HasForeignKey<BanToChuc>(banToChucDb => banToChucDb.TaiKhoanId).OnDelete(DeleteBehavior.Restrict);
    }
}
