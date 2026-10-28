using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class DonVeConfig : IEntityTypeConfiguration<DonVe>
{
    public void Configure(EntityTypeBuilder<DonVe> e)
    {
        e.ToTable("DonVe", t =>
        {
            t.HasCheckConstraint(
                "CK_DonVe_SoLuong",
                "[SoLuong] > 0"
            );

            t.HasCheckConstraint(
                "CK_DonVe_TongTien",
                "[TongTien] >= 0"
            );
        });

        e.HasKey(x => x.Id);

        e.Property(x => x.TongTien)
            .HasPrecision(18, 2);

        e.Property(x => x.RowVersion)
            .IsRowVersion();

        e.HasIndex(x => new
        {
            x.TaiKhoanId,
            x.NgayTao
        });

        e.HasIndex(x => new
        {
            x.TrangThai,
            x.HetHan
        });

        e.HasOne(x => x.TaiKhoan)
            .WithMany(x => x.DonVes)
            .HasForeignKey(x => x.TaiKhoanId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.SuKien)
            .WithMany(x => x.DonVes)
            .HasForeignKey(x => x.SuKienId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}