using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class TaiKhoanConfig : IEntityTypeConfiguration<TaiKhoan>
{
    public void Configure(EntityTypeBuilder<TaiKhoan> e)
    {
        e.ToTable("TaiKhoan");
        e.HasKey(x => x.Id);
        e.Property(x => x.HoTen).HasMaxLength(120).IsRequired();
        e.Property(x => x.Email).HasMaxLength(200).IsRequired();
        e.Property(x => x.MatKhau).HasMaxLength(500).IsRequired();
        e.Property(x => x.RowVersion).IsRowVersion();
        e.HasIndex(x => x.Email).IsUnique();
        e.HasOne(x => x.VaiTro).WithMany(x => x.TaiKhoans).HasForeignKey(x => x.VaiTroId).OnDelete(DeleteBehavior.Restrict);
    }
}
