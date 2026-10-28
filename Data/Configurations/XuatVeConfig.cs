using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class XuatVeConfig : IEntityTypeConfiguration<XuatVe>
{
    public void Configure(EntityTypeBuilder<XuatVe> e)
    {
        e.ToTable("XuatVe");
        e.HasKey(x => x.Id);
        e.Property(x => x.Kieu).HasMaxLength(30).IsRequired();
        e.Property(x => x.DiaChi).HasMaxLength(250);
        e.HasIndex(x => new { x.VeId, x.NgayTao });
        e.HasOne(x => x.Ve).WithMany(x => x.XuatVes).HasForeignKey(x => x.VeId).OnDelete(DeleteBehavior.Restrict);
    }
}
