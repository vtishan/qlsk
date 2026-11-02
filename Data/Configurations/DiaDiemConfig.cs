using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class DiaDiemConfig : IEntityTypeConfiguration<DiaDiem>
{
    public void Configure(EntityTypeBuilder<DiaDiem> cauHinhDiaDiem)
    {
        cauHinhDiaDiem.ToTable("DiaDiem", bangDiaDiem =>
        {
            bangDiaDiem.HasCheckConstraint(
                "CK_DiaDiem_SucChua",
                "[SucChua] > 0"
            );

            bangDiaDiem.HasCheckConstraint(
                "CK_DiaDiem_GiaThue",
                "[GiaThue] >= 0"
            );
        });

        cauHinhDiaDiem.HasKey(diaDiemDb => diaDiemDb.DiaDiemId);

        cauHinhDiaDiem.Property(diaDiemDb => diaDiemDb.TenDiaDiem)
            .HasMaxLength(200)
            .IsRequired();

        cauHinhDiaDiem.Property(diaDiemDb => diaDiemDb.DiaChi)
            .HasMaxLength(300)
            .IsRequired();

        cauHinhDiaDiem.Property(diaDiemDb => diaDiemDb.GiaThue)
            .HasPrecision(18, 2);
        cauHinhDiaDiem.Property(diaDiemDb => diaDiemDb.HinhAnh).HasMaxLength(500);

        cauHinhDiaDiem.Property(diaDiemDb => diaDiemDb.PhienBanDong)
            .IsRowVersion();

        cauHinhDiaDiem.HasIndex(diaDiemDb => new
        {
            Ten = diaDiemDb.TenDiaDiem,
            diaDiemDb.DiaChi
        })
        .IsUnique();
    }
}
