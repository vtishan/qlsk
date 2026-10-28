using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class VeConfig : IEntityTypeConfiguration<Ve>
{
    public void Configure(EntityTypeBuilder<Ve> e)
    {
        e.ToTable("Ve");
        e.HasKey(x => x.Id);
        e.Property(x => x.MaVe).HasMaxLength(40).IsRequired();
        e.Property(x => x.MaQR).HasMaxLength(120).IsRequired();
        e.Property(x => x.RowVersion).IsRowVersion();
        e.HasIndex(x => x.MaVe).IsUnique();
        e.HasIndex(x => x.MaQR).IsUnique();
        e.HasOne(x => x.DonVe).WithMany(x => x.Ves).HasForeignKey(x => x.DonVeId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.LoaiVe).WithMany(x => x.Ves).HasForeignKey(x => x.LoaiVeId).OnDelete(DeleteBehavior.Restrict);
    }
}
