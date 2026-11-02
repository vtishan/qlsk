using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class DonVeConfig : IEntityTypeConfiguration<DonVe>
{
    public void Configure(EntityTypeBuilder<DonVe> cauHinhDonVe)
    {
        cauHinhDonVe.ToTable("DonVe", bangDonVe =>
        {
            bangDonVe.HasCheckConstraint(
                "CK_DonVe_SoLuong",
                "[SoLuong] > 0"
            );

            bangDonVe.HasCheckConstraint(
                "CK_DonVe_TongTien",
                "[TongTien] >= 0"
            );
        });

        cauHinhDonVe.HasKey(donVeDb => donVeDb.DonVeId);

        cauHinhDonVe.Property(donVeDb => donVeDb.TongTien)
            .HasPrecision(18, 2);

        cauHinhDonVe.Property(donVeDb => donVeDb.PhienBanDong)
            .IsRowVersion();

        cauHinhDonVe.HasIndex(donVeDb => new
        {
            donVeDb.TaiKhoanId,
            donVeDb.NgayTao
        });

        cauHinhDonVe.HasIndex(donVeDb => new
        {
            donVeDb.TrangThai,
            HetHan = donVeDb.ThoiGianHetHan
        });

        cauHinhDonVe.HasOne(donVeDb => donVeDb.TaiKhoan)
            .WithMany(taiKhoanDb => taiKhoanDb.DanhSachDonVe)
            .HasForeignKey(donVeDb => donVeDb.TaiKhoanId)
            .OnDelete(DeleteBehavior.Restrict);

        cauHinhDonVe.HasOne(donVeDb => donVeDb.SuKien)
            .WithMany(suKienDb => suKienDb.DanhSachDonVe)
            .HasForeignKey(donVeDb => donVeDb.SuKienId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}