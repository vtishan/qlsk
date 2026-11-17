using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class HoanTienConfig : IEntityTypeConfiguration<HoanTien>
{
    public void Configure(EntityTypeBuilder<HoanTien> entity)
    {
        entity.ToTable("HoanTien", table => table.HasCheckConstraint("CK_HoanTien_SoTien", "[SoTien] > 0"));
        entity.HasKey(x => x.HoanTienId);
        entity.Property(x => x.MaHoanTien).HasMaxLength(120).IsRequired();
        entity.Property(x => x.SoTien).HasPrecision(18, 2);
        entity.HasIndex(x => x.MaHoanTien).IsUnique();
        entity.HasOne<ThanhToan>().WithMany().HasForeignKey(x => x.ThanhToanId).OnDelete(DeleteBehavior.Restrict);
    }
}
