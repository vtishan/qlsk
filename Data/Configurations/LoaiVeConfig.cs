using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configurations;

public class LoaiVeConfig : IEntityTypeConfiguration<LoaiVe>
{
    public void Configure(EntityTypeBuilder<LoaiVe> e)
    {
        e.ToTable("LoaiVe", t =>
        {
            t.HasCheckConstraint(
                "CK_LoaiVe_Gia",
                "[Gia] >= 0"
            );

            t.HasCheckConstraint(
                "CK_LoaiVe_SoLuong",
                "[SoLuong] > 0"
            );

            t.HasCheckConstraint(
                "CK_LoaiVe_ThoiGian",
                "[DongBan] > [MoBan]"
            );

            t.HasCheckConstraint(
                "CK_LoaiVe_Ton",
                "[DaBan] >= 0 AND [DangGiu] >= 0 AND [DaBan] + [DangGiu] <= [SoLuong]"
            );
        });

        e.HasKey(x => x.Id);

        e.Property(x => x.Ten)
            .HasMaxLength(120)
            .IsRequired();

        e.Property(x => x.Gia)
            .HasPrecision(18, 2);

        e.Property(x => x.RowVersion)
            .IsRowVersion();

        e.HasIndex(x => new
        {
            x.SuKienId,
            x.Ten
        })
        .IsUnique();

        e.HasOne(x => x.SuKien)
            .WithMany(x => x.LoaiVes)
            .HasForeignKey(x => x.SuKienId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}