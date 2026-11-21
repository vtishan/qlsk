using Microsoft.EntityFrameworkCore;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configs;

internal static class CauHinhSuKien
{
    public static void CauHinh(ModelBuilder mo_hinh_bang)
    {
        var bang_su_kien = mo_hinh_bang.Entity<SuKien>();
        bang_su_kien.Property(dong_du_lieu => dong_du_lieu.Id).UseIdentityColumn();
        bang_su_kien.ToTable(nameof(SuKien), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_SuKien_TrangThai",
                "[TrangThai] IN (N'Nhap', N'ChoDuyet', N'DaDuyet', N'DangMoBan', N'Hoan', N'DinhChi', N'DaHuy', N'HoanTat', N'TuChoi')");
        });
        bang_su_kien.HasOne<HoSoBTC>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.BTCId).OnDelete(DeleteBehavior.NoAction);
        bang_su_kien.HasOne<DanhMucSuKien>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.DanhMucId).OnDelete(DeleteBehavior.NoAction);
        bang_su_kien.Property(dong_du_lieu => dong_du_lieu.TenSuKien).HasMaxLength(200);
        bang_su_kien.Property(dong_du_lieu => dong_du_lieu.AnhBia).HasMaxLength(500);
        bang_su_kien.Property(dong_du_lieu => dong_du_lieu.TrangThai).HasConversion<string>().HasMaxLength(32);
        bang_su_kien.HasOne<PhienBanSuKien>().WithMany()
            .HasForeignKey(dong_du_lieu => new { dong_du_lieu.PhienBanCongKhaiId, dong_du_lieu.Id })
            .HasPrincipalKey(dong_tham_chieu => new { dong_tham_chieu.Id, dong_tham_chieu.SuKienId }).OnDelete(DeleteBehavior.NoAction);

        var bang_phien_ban = mo_hinh_bang.Entity<PhienBanSuKien>();
        bang_phien_ban.ToTable(nameof(PhienBanSuKien), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_PhienBanSuKien_TrangThaiDuyet",
                "[TrangThaiDuyet] IN (N'Nhap', N'ChoDuyet', N'DaDuyet', N'TuChoi')");
            cau_hinh_sql.HasCheckConstraint("CK_PhienBanSuKien_LoaiYeuCau",
                "[LoaiYeuCau] IN (N'TaoMoi', N'CapNhat', N'Hoan', N'Huy')");
            cau_hinh_sql.HasCheckConstraint("CK_PhienBanSuKien_NoiDung",
                "ISJSON([NoiDung], OBJECT) = 1");
            cau_hinh_sql.HasCheckConstraint("CK_PhienBanSuKien_DieuKien1",
                "[SoPhienBan] > 0");
        });
        bang_phien_ban.HasOne<SuKien>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.SuKienId).OnDelete(DeleteBehavior.NoAction);
        bang_phien_ban.Property(dong_du_lieu => dong_du_lieu.LoaiYeuCau).HasConversion<string>().HasMaxLength(32);
        bang_phien_ban.Property(dong_du_lieu => dong_du_lieu.TrangThaiDuyet).HasConversion<string>().HasMaxLength(32);
        bang_phien_ban.HasOne<TaiKhoan>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.NguoiGuiId).OnDelete(DeleteBehavior.NoAction);
        bang_phien_ban.HasOne<TaiKhoan>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.NguoiDuyetId).OnDelete(DeleteBehavior.NoAction);
        bang_phien_ban.HasIndex(dong_du_lieu => new { dong_du_lieu.SuKienId, dong_du_lieu.SoPhienBan }, "IX_PhienBanSuKien_SoPhienBan").IsUnique();

        var bang_suat_dien = mo_hinh_bang.Entity<SuatDien>();
        bang_suat_dien.ToTable(nameof(SuatDien), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_SuatDien_TrangThai",
                "[TrangThai] IN (N'Nhap', N'DaXacNhan', N'DangMoBan', N'DongBan', N'Hoan', N'DaHuy', N'HoanTat')");
            cau_hinh_sql.HasCheckConstraint("CK_SuatDien_SoDoChot",
                "ISJSON([SoDoChot], OBJECT) = 1");
            cau_hinh_sql.HasCheckConstraint("CK_SuatDien_DieuKien1",
                "[SucChua] > 0");
            cau_hinh_sql.HasCheckConstraint("CK_SuatDien_DieuKien2",
                "[ThoiGianBatDau] < [ThoiGianKetThuc]");
            cau_hinh_sql.HasCheckConstraint("CK_SuatDien_DieuKien3",
                "[ThoiGianMoBan] < [ThoiGianDongBan] AND [ThoiGianDongBan] <= [ThoiGianBatDau]");
        });
        bang_suat_dien.HasOne<SuKien>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.SuKienId).OnDelete(DeleteBehavior.NoAction);
        bang_suat_dien.HasOne<DiaDiem>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.DiaDiemId).OnDelete(DeleteBehavior.NoAction);
        bang_suat_dien.Property(dong_du_lieu => dong_du_lieu.TrangThai).HasConversion<string>().HasMaxLength(32);
        bang_suat_dien.HasIndex(dong_du_lieu => new { dong_du_lieu.SuKienId, dong_du_lieu.ThoiGianBatDau }, "IX_SuatDien_LichSuKien");

        var bang_danh_muc = mo_hinh_bang.Entity<DanhMucSuKien>();
        bang_danh_muc.ToTable(nameof(DanhMucSuKien), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_DanhMucSuKien_TrangThai",
                "[TrangThai] IN (N'Hien', N'An')");
        });
        bang_danh_muc.Property(dong_du_lieu => dong_du_lieu.TenDanhMuc).HasMaxLength(120);
        bang_danh_muc.Property(dong_du_lieu => dong_du_lieu.TrangThai).HasConversion<string>().HasMaxLength(32);
    }
}
