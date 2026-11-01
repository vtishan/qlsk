using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class DiaDiemConfig : IEntityTypeConfiguration<DiaDiem>
{
    public void Configure(EntityTypeBuilder<DiaDiem> e)
    {
        e.ToTable("DiaDiem", t =>
        {
            t.HasCheckConstraint(
                "CK_DiaDiem_SucChua",
                "[SucChua] > 0"
            );

            t.HasCheckConstraint(
                "CK_DiaDiem_GiaThue",
                "[GiaThue] >= 0"
            );
        });

        e.HasKey(x => x.Id);

        e.Property(x => x.Ten)
            .HasMaxLength(200)
            .IsRequired();

        e.Property(x => x.DiaChi)
            .HasMaxLength(300)
            .IsRequired();

        e.Property(x => x.GiaThue)
            .HasPrecision(18, 2);
        e.Property(x => x.HinhAnh).HasMaxLength(500);

        e.Property(x => x.RowVersion)
            .IsRowVersion();

        e.HasIndex(x => new
        {
            x.Ten,
            x.DiaChi
        })
        .IsUnique();
    }
}
