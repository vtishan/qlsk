using Microsoft.EntityFrameworkCore;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configs;

internal static class CauHinhQuanTri
{
    public static void CauHinh(ModelBuilder mo_hinh_bang)
    {
        var bang_thong_bao = mo_hinh_bang.Entity<ThongBao>();
        bang_thong_bao.ToTable(nameof(ThongBao), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_ThongBao_TrangThaiGui",
                "[TrangThaiGui] IN (N'ChoGui', N'DangGui', N'DaGui', N'LoiGui')");
            cau_hinh_sql.HasCheckConstraint("CK_ThongBao_KenhGui",
                "[KenhGui] IN (N'TrongUngDung', N'Email', N'SMS')");
            cau_hinh_sql.HasCheckConstraint("CK_ThongBao_DieuKien1",
                "([TaiKhoanId] IS NULL AND [VeMoiId] IS NOT NULL) OR ([TaiKhoanId] IS NOT NULL AND [VeMoiId] IS NULL)");
        });
        bang_thong_bao.HasOne<TaiKhoan>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.TaiKhoanId).OnDelete(DeleteBehavior.NoAction);
        bang_thong_bao.HasOne<VeMoi>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.VeMoiId).OnDelete(DeleteBehavior.NoAction);
        bang_thong_bao.Property(dong_du_lieu => dong_du_lieu.TieuDe).HasMaxLength(200);
        bang_thong_bao.Property(dong_du_lieu => dong_du_lieu.LoaiThongBao).HasMaxLength(32);
        bang_thong_bao.Property(dong_du_lieu => dong_du_lieu.KenhGui).HasConversion<string>().HasMaxLength(32);
        bang_thong_bao.Property(dong_du_lieu => dong_du_lieu.TrangThaiGui).HasConversion<string>().HasMaxLength(32);
        bang_thong_bao.HasIndex(dong_du_lieu => new { dong_du_lieu.TaiKhoanId, dong_du_lieu.DaDoc, dong_du_lieu.NgayTao }, "IX_ThongBao_HopThu");

        var bang_khieu_nai = mo_hinh_bang.Entity<KhieuNai>();
        bang_khieu_nai.ToTable(nameof(KhieuNai), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_KhieuNai_TrangThai",
                "[TrangThai] IN (N'ChoXuLy', N'DangXuLy', N'DaGiaiQuyet', N'TuChoi')");
        });
        bang_khieu_nai.HasOne<TaiKhoan>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.TaiKhoanId).OnDelete(DeleteBehavior.NoAction);
        bang_khieu_nai.HasOne<SuKien>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.SuKienId).OnDelete(DeleteBehavior.NoAction);
        bang_khieu_nai.HasOne<Ve>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.VeId).OnDelete(DeleteBehavior.NoAction);
        bang_khieu_nai.HasOne<YeuCauThue>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.YeuCauThueId).OnDelete(DeleteBehavior.NoAction);
        bang_khieu_nai.Property(dong_du_lieu => dong_du_lieu.TieuDe).HasMaxLength(200);
        bang_khieu_nai.Property(dong_du_lieu => dong_du_lieu.TrangThai).HasConversion<string>().HasMaxLength(32);
        bang_khieu_nai.HasOne<TaiKhoan>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.NguoiXuLyId).OnDelete(DeleteBehavior.NoAction);

        var bang_danh_gia = mo_hinh_bang.Entity<DanhGia>();
        bang_danh_gia.ToTable(nameof(DanhGia), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_DanhGia_TrangThai",
                "[TrangThai] IN (N'ChoDuyet', N'CongKhai', N'DaAn')");
            cau_hinh_sql.HasCheckConstraint("CK_DanhGia_DieuKien1",
                "[SoSao] BETWEEN 1 AND 5");
        });
        bang_danh_gia.HasOne<TaiKhoan>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.TaiKhoanId).OnDelete(DeleteBehavior.NoAction);
        bang_danh_gia.HasOne<SuKien>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.SuKienId).OnDelete(DeleteBehavior.NoAction);
        bang_danh_gia.Property(dong_du_lieu => dong_du_lieu.TrangThai).HasConversion<string>().HasMaxLength(32);
        bang_danh_gia.HasIndex(dong_du_lieu => new { dong_du_lieu.TaiKhoanId, dong_du_lieu.SuKienId }, "IX_DanhGia_TaiKhoanSuKien").IsUnique();

        var bang_anh_bia = mo_hinh_bang.Entity<Banner>();
        bang_anh_bia.ToTable(nameof(Banner), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_Banner_TrangThai",
                "[TrangThai] IN (N'Hien', N'An')");
            cau_hinh_sql.HasCheckConstraint("CK_Banner_DieuKien1",
                "[ThuTu] >= 0");
        });
        bang_anh_bia.Property(dong_du_lieu => dong_du_lieu.TieuDe).HasMaxLength(200);
        bang_anh_bia.Property(dong_du_lieu => dong_du_lieu.HinhAnh).HasMaxLength(500);
        bang_anh_bia.Property(dong_du_lieu => dong_du_lieu.DuongDan).HasMaxLength(500);
        bang_anh_bia.Property(dong_du_lieu => dong_du_lieu.TrangThai).HasConversion<string>().HasMaxLength(32);

        var bang_cau_hinh = mo_hinh_bang.Entity<CauHinhHeThong>();
        bang_cau_hinh.Property(dong_du_lieu => dong_du_lieu.Khoa).HasMaxLength(100);
        bang_cau_hinh.HasOne<TaiKhoan>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.NguoiCapNhatId).OnDelete(DeleteBehavior.NoAction);
        bang_cau_hinh.HasIndex(dong_du_lieu => dong_du_lieu.Khoa, "IX_CauHinhHeThong_Khoa").IsUnique();

        var bang_nhat_ky = mo_hinh_bang.Entity<NhatKyHeThong>();
        bang_nhat_ky.ToTable(nameof(NhatKyHeThong), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_NhatKyHeThong_DuLieuThayDoi",
                "[DuLieuThayDoi] IS NULL OR ISJSON([DuLieuThayDoi], OBJECT) = 1");
        });
        bang_nhat_ky.HasOne<TaiKhoan>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.TaiKhoanId).OnDelete(DeleteBehavior.NoAction);
        bang_nhat_ky.Property(dong_du_lieu => dong_du_lieu.HanhDong).HasMaxLength(100);
        bang_nhat_ky.Property(dong_du_lieu => dong_du_lieu.DoiTuong).HasMaxLength(100);

        var bang_chinh_sach = mo_hinh_bang.Entity<ChinhSach>();
        bang_chinh_sach.ToTable(nameof(ChinhSach), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_ChinhSach_TrangThai",
                "[TrangThai] IN (N'Nhap', N'CoHieuLuc', N'HetHan', N'DaHuy')");
            cau_hinh_sql.HasCheckConstraint("CK_ChinhSach_DieuKien1",
                "[PhienBan] > 0");
        });
        bang_chinh_sach.Property(dong_du_lieu => dong_du_lieu.TenChinhSach).HasMaxLength(120);
        bang_chinh_sach.Property(dong_du_lieu => dong_du_lieu.TrangThai).HasConversion<string>().HasMaxLength(32);
        bang_chinh_sach.HasIndex(dong_du_lieu => new { dong_du_lieu.TenChinhSach, dong_du_lieu.PhienBan }, "IX_ChinhSach_PhienBan").IsUnique();
    }
}
