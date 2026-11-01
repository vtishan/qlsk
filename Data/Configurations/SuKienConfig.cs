using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class SuKienConfig : IEntityTypeConfiguration<SuKien>
{
    public void Configure(EntityTypeBuilder<SuKien> e)
    {
        e.ToTable("SuKien", t =>
        {
            t.HasCheckConstraint(
                "CK_SuKien_ThoiGian",
                "[KetThuc] > [BatDau]"
            );

            t.HasCheckConstraint(
                "CK_SuKien_SoNguoi",
                "[SoNguoi] > 0"
            );
        });

        e.HasKey(x => x.Id);

        e.Property(x => x.Ten)
            .HasMaxLength(200)
            .IsRequired();

        e.Property(x => x.MoTa)
            .HasMaxLength(4000);

        e.Property(x => x.HinhAnh)
            .HasMaxLength(500);

        e.Property(x => x.RowVersion)
            .IsRowVersion();

        e.HasIndex(x => new
        {
            x.TrangThai,
            x.BatDau
        });

        e.HasOne(x => x.BanToChuc)
            .WithMany(x => x.SuKiens)
            .HasForeignKey(x => x.BanToChucId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.LoaiSuKien)
            .WithMany(x => x.SuKiens)
            .HasForeignKey(x => x.LoaiSuKienId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
