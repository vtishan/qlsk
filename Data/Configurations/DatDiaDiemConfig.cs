using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class DatDiaDiemConfig : IEntityTypeConfiguration<DatDiaDiem>
{
    public void Configure(EntityTypeBuilder<DatDiaDiem> e)
    {
        e.ToTable("DatDiaDiem", t =>
        {
            t.HasCheckConstraint(
                "CK_DatDiaDiem_ThoiGian",
                "[KetThuc] > [BatDau]"
            );

            t.HasCheckConstraint(
                "CK_DatDiaDiem_TienCoc",
                "[TienCoc] >= 0 AND [TienCoc] <= [TongTien]"
            );

            t.HasCheckConstraint(
                "CK_DatDiaDiem_TongTien",
                "[TongTien] >= 0"
            );
        });

        e.HasKey(x => x.Id);

        e.Property(x => x.TienCoc)
            .HasPrecision(18, 2);

        e.Property(x => x.TongTien)
            .HasPrecision(18, 2);

        e.Property(x => x.RowVersion)
            .IsRowVersion();

        e.HasIndex(x => x.SuKienId)
            .IsUnique();

        e.HasIndex(x => new
        {
            x.DiaDiemId,
            x.BatDau,
            x.KetThuc
        });

        e.HasOne(x => x.DiaDiem)
            .WithMany(x => x.DatCho)
            .HasForeignKey(x => x.DiaDiemId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.SuKien)
            .WithOne(x => x.DatDiaDiem)
            .HasForeignKey<DatDiaDiem>(x => x.SuKienId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}