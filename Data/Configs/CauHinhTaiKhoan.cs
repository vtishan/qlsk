using Microsoft.EntityFrameworkCore;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configs;

internal static class CauHinhTaiKhoan
{
    public static void CauHinh(ModelBuilder mo_hinh_bang)
    {
        var bang_ho_so = mo_hinh_bang.Entity<HoSoBTC>();
        bang_ho_so.ToTable(nameof(HoSoBTC), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_HoSoBTC_TrangThai",
                "[TrangThai] IN (N'Nhap', N'ChoDuyet', N'CanBoSung', N'TuChoi', N'DaDuyet')");
            cau_hinh_sql.HasCheckConstraint("CK_HoSoBTC_DieuKien1",
                "[NgayNop] IS NULL OR [HanDuyet] IS NULL OR [NgayNop] <= [HanDuyet]");
        });
        bang_ho_so.HasOne<TaiKhoan>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.TaiKhoanId).OnDelete(DeleteBehavior.NoAction);
        bang_ho_so.Property(dong_du_lieu => dong_du_lieu.TenToChuc).HasMaxLength(200);
        bang_ho_so.Property(dong_du_lieu => dong_du_lieu.NguoiDaiDien).HasMaxLength(120);
        bang_ho_so.Property(dong_du_lieu => dong_du_lieu.MaSoThue).HasMaxLength(20);
        bang_ho_so.Property(dong_du_lieu => dong_du_lieu.DiaChi).HasMaxLength(300);
        bang_ho_so.Property(dong_du_lieu => dong_du_lieu.ThongTinLienHe).HasMaxLength(300);
        bang_ho_so.Property(dong_du_lieu => dong_du_lieu.TrangThai).HasConversion<string>().HasMaxLength(32);
        bang_ho_so.HasOne<TaiKhoan>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.NguoiDuyetId).OnDelete(DeleteBehavior.NoAction);

        var bang_giay_to = mo_hinh_bang.Entity<GiayToBTC>();
        bang_giay_to.HasOne<HoSoBTC>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.HoSoBTCId).OnDelete(DeleteBehavior.NoAction);
        bang_giay_to.Property(dong_du_lieu => dong_du_lieu.LoaiGiayTo).HasMaxLength(32);
        bang_giay_to.Property(dong_du_lieu => dong_du_lieu.KhoaLuuTru).HasMaxLength(500);
        bang_giay_to.Property(dong_du_lieu => dong_du_lieu.TenTep).HasMaxLength(255);
        bang_giay_to.Property(dong_du_lieu => dong_du_lieu.LoaiNoiDung).HasMaxLength(32);

        var bang_phan_cong = mo_hinh_bang.Entity<PhanCongSuKien>();
        bang_phan_cong.ToTable(nameof(PhanCongSuKien), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_PhanCongSuKien_TrangThai",
                "[TrangThai] IN (N'ChoNhan', N'DangHieuLuc', N'DaThuHoi', N'HetHan', N'TuChoi')");
            cau_hinh_sql.HasCheckConstraint("CK_PhanCongSuKien_DieuKien1",
                "[TaiKhoanId] IS NOT NULL OR NULLIF(TRIM([EmailMoi]), N'') IS NOT NULL");
            cau_hinh_sql.HasCheckConstraint("CK_PhanCongSuKien_DieuKien2",
                "[TrangThai] <> N'DangHieuLuc' OR ([TaiKhoanId] IS NOT NULL AND [NgayThuHoi] IS NULL)");
            cau_hinh_sql.HasCheckConstraint("CK_PhanCongSuKien_DieuKien3",
                "[NgayThuHoi] IS NULL OR [NgayThuHoi] >= [NgayPhanCong]");
        });
        bang_phan_cong.HasOne<SuKien>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.SuKienId).OnDelete(DeleteBehavior.NoAction);
        bang_phan_cong.HasOne<SuatDien>().WithMany()
            .HasForeignKey(dong_du_lieu => new { dong_du_lieu.SuatDienId, dong_du_lieu.SuKienId })
            .HasPrincipalKey(dong_tham_chieu => new { dong_tham_chieu.Id, dong_tham_chieu.SuKienId }).OnDelete(DeleteBehavior.NoAction);
        bang_phan_cong.HasOne<TaiKhoan>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.TaiKhoanId).OnDelete(DeleteBehavior.NoAction);
        bang_phan_cong.Property(dong_du_lieu => dong_du_lieu.EmailMoi).HasMaxLength(255);
        bang_phan_cong.Property(dong_du_lieu => dong_du_lieu.MaBamLoiMoi).HasMaxLength(128);
        bang_phan_cong.HasOne<TaiKhoan>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.NguoiPhanCongId).OnDelete(DeleteBehavior.NoAction);
        bang_phan_cong.Property(dong_du_lieu => dong_du_lieu.LoaiQuyen).HasMaxLength(64);
        bang_phan_cong.Property(dong_du_lieu => dong_du_lieu.TrangThai).HasConversion<string>().HasMaxLength(32);
        bang_phan_cong.HasIndex(dong_du_lieu => new { dong_du_lieu.TaiKhoanId, dong_du_lieu.SuKienId, dong_du_lieu.SuatDienId, dong_du_lieu.LoaiQuyen, dong_du_lieu.TrangThai }, "IX_PhanCongSuKien_TraQuyen");
        bang_phan_cong.HasIndex(dong_du_lieu => new { dong_du_lieu.TaiKhoanId, dong_du_lieu.SuKienId, dong_du_lieu.LoaiQuyen }, "IX_PhanCongSuKien_ToanSuKien").IsUnique()
            .HasFilter("[TaiKhoanId] IS NOT NULL AND [SuatDienId] IS NULL AND [TrangThai] = N'DangHieuLuc'");
        bang_phan_cong.HasIndex(dong_du_lieu => new { dong_du_lieu.TaiKhoanId, dong_du_lieu.SuKienId, dong_du_lieu.SuatDienId, dong_du_lieu.LoaiQuyen }, "IX_PhanCongSuKien_TheoSuat").IsUnique()
            .HasFilter("[TaiKhoanId] IS NOT NULL AND [SuatDienId] IS NOT NULL AND [TrangThai] = N'DangHieuLuc'");
    }
}
