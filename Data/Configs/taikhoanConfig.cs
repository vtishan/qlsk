using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;

namespace QLSK.Data.Configs;

public class TaiKhoanConfig : IEntityTypeConfiguration<TaiKhoan>
{
    public void Configure(EntityTypeBuilder<TaiKhoan> cau_hinh_bang)
    {
        cau_hinh_bang.ToTable(nameof(TaiKhoan), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_TaiKhoan_VaiTro", "[VaiTro] BETWEEN 1 AND 5");
            cau_hinh_sql.HasCheckConstraint("CK_TaiKhoan_TrangThai",
                "[TrangThai] IN (N'HoatDong', N'DaKhoa', N'ChoXacMinh') AND [PhienBanDangNhap] >= 0");
            cau_hinh_sql.HasCheckConstraint("CK_TaiKhoan_LienHe",
                "([Email] IS NOT NULL OR [SoDienThoai] IS NOT NULL) AND ([Email] IS NULL OR TRIM([Email]) <> N'') AND ([SoDienThoai] IS NULL OR TRIM([SoDienThoai]) <> N'')");
            cau_hinh_sql.HasCheckConstraint("CK_TaiKhoan_QuyenQuanLy", "[QuyenQuanLy] IS NULL OR ISJSON([QuyenQuanLy], OBJECT) = 1");
            cau_hinh_sql.HasCheckConstraint("CK_TaiKhoan_KhoiPhuc",
                "([MaBamKhoiPhuc] IS NULL AND [HanKhoiPhuc] IS NULL) OR ([MaBamKhoiPhuc] IS NOT NULL AND [HanKhoiPhuc] IS NOT NULL)");
            cau_hinh_sql.HasCheckConstraint("CK_TaiKhoan_LienHeXacMinh",
                "([EmailXacMinhLuc] IS NULL OR NULLIF(TRIM([Email]), N'') IS NOT NULL) AND ([SoDienThoaiXacMinhLuc] IS NULL OR NULLIF(TRIM([SoDienThoai]), N'') IS NOT NULL)");
        });
        cau_hinh_bang.HasKey(tai_khoan_bang => tai_khoan_bang.Id);

        cau_hinh_bang.Property(tai_khoan_bang => tai_khoan_bang.Email).HasMaxLength(255);
        cau_hinh_bang.Property(tai_khoan_bang => tai_khoan_bang.SoDienThoai).HasMaxLength(15);
        cau_hinh_bang.Property(tai_khoan_bang => tai_khoan_bang.HoTen).HasMaxLength(100).IsRequired();
        cau_hinh_bang.Property(tai_khoan_bang => tai_khoan_bang.TenDangNhap).HasMaxLength(50).IsRequired();
        cau_hinh_bang.Property(tai_khoan_bang => tai_khoan_bang.MatKhauBam).HasMaxLength(500).IsRequired();
        cau_hinh_bang.Property(tai_khoan_bang => tai_khoan_bang.VaiTro).HasConversion<int>().HasDefaultValue(VaiTroTaiKhoan.NguoiDung);
        cau_hinh_bang.Property(tai_khoan_bang => tai_khoan_bang.TrangThai).HasConversion<string>().HasMaxLength(32).HasDefaultValue(TrangThaiTaiKhoan.HoatDong);
        cau_hinh_bang.Property(tai_khoan_bang => tai_khoan_bang.NgayTao).HasDefaultValueSql("SYSUTCDATETIME()");
        cau_hinh_bang.Property(tai_khoan_bang => tai_khoan_bang.MaBamKhoiPhuc).HasMaxLength(128);

        cau_hinh_bang.HasIndex(tai_khoan_bang => tai_khoan_bang.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
        cau_hinh_bang.HasIndex(tai_khoan_bang => tai_khoan_bang.SoDienThoai).IsUnique().HasFilter("[SoDienThoai] IS NOT NULL");
        cau_hinh_bang.HasIndex(tai_khoan_bang => tai_khoan_bang.TenDangNhap).IsUnique();
    }
}
