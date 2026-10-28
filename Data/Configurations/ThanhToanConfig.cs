using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class ThanhToanConfig : IEntityTypeConfiguration<ThanhToan>
{
    public void Configure(EntityTypeBuilder<ThanhToan> e)
    {
        e.ToTable("ThanhToan", t => t
            .HasCheckConstraint("CK_ThanhToan_SoTien", "[SoTien] > 0 AND [SoTienHoan] >= 0 AND [SoTienHoan] <= [SoTien]"));
        e.HasKey(x => x.Id);
        e.Property(x => x.SoTien).HasPrecision(18, 2);
        e.Property(x => x.SoTienHoan).HasPrecision(18, 2);
        e.Property(x => x.MaGiaoDich).HasMaxLength(120);
        e.Property(x => x.MaLienKet).HasMaxLength(120);
        e.Property(x => x.MaHoanTien).HasMaxLength(120);
        e.Property(x => x.RowVersion).IsRowVersion();
        e.HasIndex(x => x.MaGiaoDich).IsUnique().HasFilter("[MaGiaoDich] IS NOT NULL");
        e.HasIndex(x => x.MaLienKet).IsUnique().HasFilter("[MaLienKet] IS NOT NULL");
        e.HasIndex(x => new { x.DonVeId, x.TrangThai });
        e.HasOne(x => x.DonVe).WithMany(x => x.ThanhToans).HasForeignKey(x => x.DonVeId).OnDelete(DeleteBehavior.Restrict);
    }
}
