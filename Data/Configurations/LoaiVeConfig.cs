using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class LoaiVeConfig : IEntityTypeConfiguration<LoaiVe>
{
    public void Configure(EntityTypeBuilder<LoaiVe> cauHinhLoaiVe)
    {
        cauHinhLoaiVe.ToTable("LoaiVe", bangLoaiVe =>
        {
            bangLoaiVe.HasCheckConstraint(
                "CK_LoaiVe_Gia",
                "[Gia] >= 0"
            );

            bangLoaiVe.HasCheckConstraint(
                "CK_LoaiVe_SoLuong",
                "[SoLuong] > 0"
            );

            bangLoaiVe.HasCheckConstraint(
                "CK_LoaiVe_ThoiGian",
                "[DongBan] > [MoBan]"
            );

            bangLoaiVe.HasCheckConstraint(
                "CK_LoaiVe_Ton",
                "[DaBan] >= 0 AND [DangGiu] >= 0 AND [DaBan] + [DangGiu] <= [SoLuong]"
            );
        });

        cauHinhLoaiVe.HasKey(loaiVeDb => loaiVeDb.LoaiVeId);

        cauHinhLoaiVe.Property(loaiVeDb => loaiVeDb.TenLoaiVe)
            .HasMaxLength(120)
            .IsRequired();

        cauHinhLoaiVe.Property(loaiVeDb => loaiVeDb.GiaVe)
            .HasPrecision(18, 2);

        cauHinhLoaiVe.Property(loaiVeDb => loaiVeDb.PhienBanDong)
            .IsRowVersion();

        cauHinhLoaiVe.HasIndex(loaiVeDb => new
        {
            loaiVeDb.SuKienId,
            Ten = loaiVeDb.TenLoaiVe
        })
        .IsUnique();

        cauHinhLoaiVe.HasOne(loaiVeDb => loaiVeDb.SuKien)
            .WithMany(suKienDb => suKienDb.DanhSachLoaiVe)
            .HasForeignKey(loaiVeDb => loaiVeDb.SuKienId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}