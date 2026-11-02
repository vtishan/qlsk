using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class VaiTroConfig : IEntityTypeConfiguration<VaiTro>
{
    public void Configure(EntityTypeBuilder<VaiTro> cauHinhVaiTro)
    {
        cauHinhVaiTro.ToTable("VaiTro");
        cauHinhVaiTro.HasKey(vaiTroDb => vaiTroDb.VaiTroId);
        cauHinhVaiTro.Property(vaiTroDb => vaiTroDb.TenVaiTro).HasMaxLength(40).IsRequired();
        cauHinhVaiTro.Property(vaiTroDb => vaiTroDb.MoTaVaiTro).HasMaxLength(200);
        cauHinhVaiTro.HasIndex(vaiTroDb => vaiTroDb.TenVaiTro).IsUnique();

        cauHinhVaiTro.HasData(
            new VaiTro { VaiTroId = 1, TenVaiTro = "NguoiDung", MoTaVaiTro = "Người dùng" },
            new VaiTro { VaiTroId = 2, TenVaiTro = "BanToChuc", MoTaVaiTro = "Ban tổ chức" },
            new VaiTro { VaiTroId = 3, TenVaiTro = "QuanLySuKien", MoTaVaiTro = "Quản lý sự kiện" },
            new VaiTro { VaiTroId = 4, TenVaiTro = "QuanTriVien", MoTaVaiTro = "Quản trị viên" }
        );
    }
}
