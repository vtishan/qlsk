using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class ChiTietDonVeConfig : IEntityTypeConfiguration<ChiTietDonVe>
{
    public void Configure(EntityTypeBuilder<ChiTietDonVe> cauHinhChiTietDonVe)
    {
        cauHinhChiTietDonVe.ToTable("ChiTietDonVe", bangChiTietDonVe =>
        {
            bangChiTietDonVe.HasCheckConstraint(
                "CK_ChiTietDonVe_SoLuong",
                "[SoLuong] > 0"
            );

            bangChiTietDonVe.HasCheckConstraint(
                "CK_ChiTietDonVe_Tien",
                "[DonGia] >= 0 AND [ThanhTien] >= 0"
            );
        });

        cauHinhChiTietDonVe.HasKey(chiTietDonVeDb => chiTietDonVeDb.ChiTietDonVeId);

        cauHinhChiTietDonVe.Property(chiTietDonVeDb => chiTietDonVeDb.DonGia)
            .HasPrecision(18, 2);

        cauHinhChiTietDonVe.Property(chiTietDonVeDb => chiTietDonVeDb.ThanhTien)
            .HasPrecision(18, 2);

        cauHinhChiTietDonVe.HasIndex(chiTietDonVeDb => new
        {
            chiTietDonVeDb.DonVeId,
            chiTietDonVeDb.LoaiVeId
        })
        .IsUnique();

        cauHinhChiTietDonVe.HasOne(chiTietDonVeDb => chiTietDonVeDb.DonVe)
            .WithMany(donVeDb => donVeDb.ChiTietDonVe)
            .HasForeignKey(chiTietDonVeDb => chiTietDonVeDb.DonVeId)
            .OnDelete(DeleteBehavior.Cascade);

        cauHinhChiTietDonVe.HasOne(chiTietDonVeDb => chiTietDonVeDb.LoaiVe)
            .WithMany(loaiVeDb => loaiVeDb.ChiTietDonVe)
            .HasForeignKey(chiTietDonVeDb => chiTietDonVeDb.LoaiVeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}