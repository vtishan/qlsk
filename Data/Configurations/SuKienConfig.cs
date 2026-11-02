using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class SuKienConfig : IEntityTypeConfiguration<SuKien>
{
    public void Configure(EntityTypeBuilder<SuKien> cauHinhSuKien)
    {
        cauHinhSuKien.ToTable("SuKien", bangSuKien =>
        {
            bangSuKien.HasCheckConstraint(
                "CK_SuKien_ThoiGian",
                "[KetThuc] > [BatDau]"
            );

            bangSuKien.HasCheckConstraint(
                "CK_SuKien_SoNguoi",
                "[SoNguoi] > 0"
            );
        });

        cauHinhSuKien.HasKey(suKienDb => suKienDb.SuKienId);

        cauHinhSuKien.Property(suKienDb => suKienDb.TenSuKien)
            .HasMaxLength(200)
            .IsRequired();

        cauHinhSuKien.Property(suKienDb => suKienDb.MoTaSuKien)
            .HasMaxLength(4000);

        cauHinhSuKien.Property(suKienDb => suKienDb.HinhAnh)
            .HasMaxLength(500);

        cauHinhSuKien.Property(suKienDb => suKienDb.PhienBanDong)
            .IsRowVersion();

        cauHinhSuKien.HasIndex(suKienDb => new
        {
            suKienDb.TrangThai,
            BatDau = suKienDb.ThoiGianBatDau
        });

        cauHinhSuKien.HasOne(suKienDb => suKienDb.BanToChuc)
            .WithMany(banToChucDb => banToChucDb.DanhSachSuKien)
            .HasForeignKey(suKienDb => suKienDb.BanToChucId)
            .OnDelete(DeleteBehavior.Restrict);

        cauHinhSuKien.HasOne(suKienDb => suKienDb.LoaiSuKien)
            .WithMany(loaiSuKienDb => loaiSuKienDb.DanhSachSuKien)
            .HasForeignKey(suKienDb => suKienDb.LoaiSuKienId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
