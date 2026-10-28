using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class LoaiSuKienConfig : IEntityTypeConfiguration<LoaiSuKien>
{
    public void Configure(EntityTypeBuilder<LoaiSuKien> e)
    {
        e.ToTable("LoaiSuKien");
        e.HasKey(x => x.Id);
        e.Property(x => x.Ten).HasMaxLength(120).IsRequired();
        e.Property(x => x.MoTa).HasMaxLength(500);
        e.HasIndex(x => x.Ten).IsUnique();
    }
}
