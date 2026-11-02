using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class DatDiaDiemConfig : IEntityTypeConfiguration<DatDiaDiem>
{
    public void Configure(EntityTypeBuilder<DatDiaDiem> cauHinhDatDiaDiem)
    {
        cauHinhDatDiaDiem.ToTable("DatDiaDiem", bangDatDiaDiem =>
        {
            bangDatDiaDiem.HasCheckConstraint(
                "CK_DatDiaDiem_ThoiGian",
                "[KetThuc] > [BatDau]"
            );

            bangDatDiaDiem.HasCheckConstraint(
                "CK_DatDiaDiem_TienCoc",
                "[TienCoc] >= 0 AND [TienCoc] <= [TongTien]"
            );

            bangDatDiaDiem.HasCheckConstraint(
                "CK_DatDiaDiem_TongTien",
                "[TongTien] >= 0"
            );
        });

        cauHinhDatDiaDiem.HasKey(datDiaDiemDb => datDiaDiemDb.DatDiaDiemId);

        cauHinhDatDiaDiem.Property(datDiaDiemDb => datDiaDiemDb.TienCoc)
            .HasPrecision(18, 2);

        cauHinhDatDiaDiem.Property(datDiaDiemDb => datDiaDiemDb.TongTien)
            .HasPrecision(18, 2);

        cauHinhDatDiaDiem.Property(datDiaDiemDb => datDiaDiemDb.PhienBanDong)
            .IsRowVersion();

        cauHinhDatDiaDiem.HasIndex(datDiaDiemDb => datDiaDiemDb.SuKienId)
            .IsUnique();

        cauHinhDatDiaDiem.HasIndex(datDiaDiemDb => new
        {
            datDiaDiemDb.DiaDiemId,
            BatDau = datDiaDiemDb.ThoiGianBatDau,
            KetThuc = datDiaDiemDb.ThoiGianKetThuc
        });

        cauHinhDatDiaDiem.HasOne(datDiaDiemDb => datDiaDiemDb.DiaDiem)
            .WithMany(diaDiemDb => diaDiemDb.DanhSachDatDiaDiem)
            .HasForeignKey(datDiaDiemDb => datDiaDiemDb.DiaDiemId)
            .OnDelete(DeleteBehavior.Restrict);

        cauHinhDatDiaDiem.HasOne(datDiaDiemDb => datDiaDiemDb.SuKien)
            .WithOne(suKienDb => suKienDb.DatDiaDiem)
            .HasForeignKey<DatDiaDiem>(datDiaDiemDb => datDiaDiemDb.SuKienId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}