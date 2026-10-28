using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class BanToChucConfig : IEntityTypeConfiguration<BanToChuc>
{
    public void Configure(EntityTypeBuilder<BanToChuc> e)
    {
        e.ToTable("BanToChuc");
        e.HasKey(x => x.Id);
        e.Property(x => x.Ten).HasMaxLength(200).IsRequired();
        e.Property(x => x.MoTa).HasMaxLength(1000);
        e.HasIndex(x => x.TaiKhoanId).IsUnique();
        e.HasOne(x => x.TaiKhoan).WithOne(x => x.BanToChuc).HasForeignKey<BanToChuc>(x => x.TaiKhoanId).OnDelete(DeleteBehavior.Restrict);
    }
}
