using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class VaiTroConfig : IEntityTypeConfiguration<VaiTro>
{
    public void Configure(EntityTypeBuilder<VaiTro> e)
    {
        e.ToTable("VaiTro");
        e.HasKey(x => x.Id);
        e.Property(x => x.Ten).HasMaxLength(40).IsRequired();
        e.Property(x => x.MoTa).HasMaxLength(200);
        e.HasIndex(x => x.Ten).IsUnique();

        e.HasData(
            new VaiTro { Id = 1, Ten = "NguoiDung", MoTa = "Người dùng" },
            new VaiTro { Id = 2, Ten = "BanToChuc", MoTa = "Ban tổ chức" },
            new VaiTro { Id = 3, Ten = "QuanLySuKien", MoTa = "Quản lý sự kiện" },
            new VaiTro { Id = 4, Ten = "QuanTriVien", MoTa = "Quản trị viên" }
        );
    }
}
