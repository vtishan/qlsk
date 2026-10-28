using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class ChiTietDonVeConfig : IEntityTypeConfiguration<ChiTietDonVe>
{
    public void Configure(EntityTypeBuilder<ChiTietDonVe> e)
    {
        e.ToTable("ChiTietDonVe", t =>
        {
            t.HasCheckConstraint(
                "CK_ChiTietDonVe_SoLuong",
                "[SoLuong] > 0"
            );

            t.HasCheckConstraint(
                "CK_ChiTietDonVe_Tien",
                "[DonGia] >= 0 AND [ThanhTien] >= 0"
            );
        });

        e.HasKey(x => x.Id);

        e.Property(x => x.DonGia)
            .HasPrecision(18, 2);

        e.Property(x => x.ThanhTien)
            .HasPrecision(18, 2);

        e.HasIndex(x => new
        {
            x.DonVeId,
            x.LoaiVeId
        })
        .IsUnique();

        e.HasOne(x => x.DonVe)
            .WithMany(x => x.ChiTiets)
            .HasForeignKey(x => x.DonVeId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(x => x.LoaiVe)
            .WithMany(x => x.ChiTiets)
            .HasForeignKey(x => x.LoaiVeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}