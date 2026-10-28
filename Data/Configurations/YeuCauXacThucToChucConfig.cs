using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class YeuCauXacThucToChucConfig : IEntityTypeConfiguration<YeuCauXacThucToChuc>
{
    public void Configure(EntityTypeBuilder<YeuCauXacThucToChuc> e)
    {
        e.ToTable("YeuCauXacThucToChuc");
        e.HasKey(x => x.Id);
        e.Property(x => x.TenToChuc).HasMaxLength(200).IsRequired();
        e.Property(x => x.GiayTo).HasMaxLength(500).IsRequired();
        e.Property(x => x.LyDo).HasMaxLength(500);
        e.HasIndex(x => x.TaiKhoanId).IsUnique();
        e.HasOne(x => x.TaiKhoan).WithOne(x => x.YeuCauXacThuc).HasForeignKey<YeuCauXacThucToChuc>(x => x.TaiKhoanId).OnDelete(DeleteBehavior.Cascade);
        e.HasOne(x => x.NguoiXuLy).WithMany().HasForeignKey(x => x.NguoiXuLyId).OnDelete(DeleteBehavior.Restrict);
    }
}
