using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class LichSuYeuCauXacThucToChucConfig : IEntityTypeConfiguration<LichSuYeuCauXacThucToChuc>
{
    public void Configure(EntityTypeBuilder<LichSuYeuCauXacThucToChuc> e)
    {
        e.ToTable("LichSuYeuCauXacThucToChuc");
        e.HasKey(x => x.Id);
        e.Property(x => x.TenToChuc).HasMaxLength(200).IsRequired();
        e.Property(x => x.GiayTo).HasMaxLength(500).IsRequired();
        e.Property(x => x.LyDo).HasMaxLength(500);
        e.HasIndex(x => new { x.TaiKhoanId, x.NgayXuLy });
        e.HasOne(x => x.TaiKhoan).WithMany(x => x.LichSuYeuCaus).HasForeignKey(x => x.TaiKhoanId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.NguoiXuLy).WithMany().HasForeignKey(x => x.NguoiXuLyId).OnDelete(DeleteBehavior.Restrict);
    }
}
