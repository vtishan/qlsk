using Microsoft.EntityFrameworkCore;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configs;

internal static class CauHinhDiaDiem
{
    public static void CauHinh(ModelBuilder mo_hinh_bang)
    {
        var bang_dia_diem = mo_hinh_bang.Entity<DiaDiem>();
        bang_dia_diem.ToTable(nameof(DiaDiem), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_DiaDiem_TrangThai",
                "[TrangThai] IN (N'HoatDong', N'TamDung', N'DaAn')");
            cau_hinh_sql.HasCheckConstraint("CK_DiaDiem_DieuKien1",
                "[SucChua] > 0 AND [GiaThue] >= 0");
        });
        bang_dia_diem.Property(dong_du_lieu => dong_du_lieu.TenDiaDiem).HasMaxLength(200);
        bang_dia_diem.Property(dong_du_lieu => dong_du_lieu.DiaChi).HasMaxLength(300);
        bang_dia_diem.Property(dong_du_lieu => dong_du_lieu.TrangThai).HasConversion<string>().HasMaxLength(32);

        var bang_khu_vuc = mo_hinh_bang.Entity<KhuVuc>();
        bang_khu_vuc.ToTable(nameof(KhuVuc), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_KhuVuc_DieuKien1",
                "[SucChua] > 0");
        });
        bang_khu_vuc.HasOne<DiaDiem>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.DiaDiemId).OnDelete(DeleteBehavior.NoAction);
        bang_khu_vuc.Property(dong_du_lieu => dong_du_lieu.TenKhuVuc).HasMaxLength(120);
        bang_khu_vuc.HasIndex(dong_du_lieu => new { dong_du_lieu.DiaDiemId, dong_du_lieu.TenKhuVuc }, "IX_KhuVuc_TenTheoDiaDiem").IsUnique();

        var bang_ghe_ngoi = mo_hinh_bang.Entity<Ghe>();
        bang_ghe_ngoi.ToTable(nameof(Ghe), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_Ghe_TrangThai",
                "[TrangThai] IN (N'SuDung', N'BaoTri', N'NgungSuDung')");
            cau_hinh_sql.HasCheckConstraint("CK_Ghe_LoaiGhe",
                "[LoaiGhe] IN (N'GheDon', N'GheDoi', N'GheCum')");
        });
        bang_ghe_ngoi.HasOne<KhuVuc>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.KhuVucId).OnDelete(DeleteBehavior.NoAction);
        bang_ghe_ngoi.HasOne<CumGhe>().WithMany()
            .HasForeignKey(dong_du_lieu => new { dong_du_lieu.CumGheId, dong_du_lieu.KhuVucId })
            .HasPrincipalKey(dong_tham_chieu => new { dong_tham_chieu.Id, dong_tham_chieu.KhuVucId }).OnDelete(DeleteBehavior.NoAction);
        bang_ghe_ngoi.Property(dong_du_lieu => dong_du_lieu.MaGhe).HasMaxLength(32);
        bang_ghe_ngoi.Property(dong_du_lieu => dong_du_lieu.LoaiGhe).HasConversion<string>().HasMaxLength(32);
        bang_ghe_ngoi.Property(dong_du_lieu => dong_du_lieu.TrangThai).HasConversion<string>().HasMaxLength(32);
        bang_ghe_ngoi.HasIndex(dong_du_lieu => new { dong_du_lieu.KhuVucId, dong_du_lieu.MaGhe }, "IX_Ghe_MaTheoKhuVuc").IsUnique();

        var bang_cum_ghe = mo_hinh_bang.Entity<CumGhe>();
        bang_cum_ghe.ToTable(nameof(CumGhe), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_CumGhe_LoaiCum",
                "[LoaiCum] IN (N'GheDoi', N'GheCum')");
            cau_hinh_sql.HasCheckConstraint("CK_CumGhe_DieuKien1",
                "[SoCho] >= 2");
        });
        bang_cum_ghe.HasOne<KhuVuc>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.KhuVucId).OnDelete(DeleteBehavior.NoAction);
        bang_cum_ghe.Property(dong_du_lieu => dong_du_lieu.TenCum).HasMaxLength(120);
        bang_cum_ghe.Property(dong_du_lieu => dong_du_lieu.LoaiCum).HasConversion<string>().HasMaxLength(32);

        var bang_yeu_cau = mo_hinh_bang.Entity<YeuCauThue>();
        bang_yeu_cau.ToTable(nameof(YeuCauThue), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_YeuCauThue_TrangThai",
                "[TrangThai] IN (N'ChoDuyet', N'ChoCoc', N'DaXacNhan', N'TuChoi', N'HetHan', N'DaHuy')");
            cau_hinh_sql.HasCheckConstraint("CK_YeuCauThue_TienCoc",
                "[TienCoc] = ROUND([TongTien] * 0.30, 0)");
            cau_hinh_sql.HasCheckConstraint("CK_YeuCauThue_HanDatCoc",
                "[TrangThai] <> N'ChoCoc' OR [HanDatCoc] IS NOT NULL");
            cau_hinh_sql.HasCheckConstraint("CK_YeuCauThue_DieuKien1",
                "[ThoiGianBatDau] < [ThoiGianKetThuc]");
            cau_hinh_sql.HasCheckConstraint("CK_YeuCauThue_DieuKien2",
                "[TongTien] >= 0 AND [TienCoc] BETWEEN 0 AND [TongTien]");
        });
        bang_yeu_cau.HasOne<TaiKhoan>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.TaiKhoanId).OnDelete(DeleteBehavior.NoAction);
        bang_yeu_cau.HasOne<DiaDiem>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.DiaDiemId).OnDelete(DeleteBehavior.NoAction);
        bang_yeu_cau.HasOne<ChinhSach>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.ChinhSachId).OnDelete(DeleteBehavior.NoAction);
        bang_yeu_cau.Property(dong_du_lieu => dong_du_lieu.TrangThai).HasConversion<string>().HasMaxLength(32);
        bang_yeu_cau.HasOne<TaiKhoan>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.NguoiDuyetId).OnDelete(DeleteBehavior.NoAction);

        var bang_lich_dat = mo_hinh_bang.Entity<LichDatDiaDiem>();
        bang_lich_dat.ToTable(nameof(LichDatDiaDiem), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_LichDatDiaDiem_TrangThai",
                "[TrangThai] IN (N'DangGiu', N'DaXacNhan', N'HetHan', N'DaHuy')");
            cau_hinh_sql.HasCheckConstraint("CK_LichDatDiaDiem_LoaiLich",
                "[LoaiLich] IN (N'Thue', N'SuatDien', N'BaoTri')");
            cau_hinh_sql.HasCheckConstraint("CK_LichDatDiaDiem_DieuKien1",
                "[BatDau] < [KetThuc]");
        });
        bang_lich_dat.HasOne<DiaDiem>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.DiaDiemId).OnDelete(DeleteBehavior.NoAction);
        bang_lich_dat.HasOne<YeuCauThue>().WithMany()
            .HasForeignKey(dong_du_lieu => new { dong_du_lieu.YeuCauThueId, dong_du_lieu.DiaDiemId })
            .HasPrincipalKey(dong_tham_chieu => new { dong_tham_chieu.Id, dong_tham_chieu.DiaDiemId }).OnDelete(DeleteBehavior.NoAction);
        bang_lich_dat.HasOne<SuatDien>().WithMany()
            .HasForeignKey(dong_du_lieu => new { dong_du_lieu.SuatDienId, dong_du_lieu.DiaDiemId })
            .HasPrincipalKey(dong_tham_chieu => new { dong_tham_chieu.Id, dong_tham_chieu.DiaDiemId }).OnDelete(DeleteBehavior.NoAction);
        bang_lich_dat.Property(dong_du_lieu => dong_du_lieu.LoaiLich).HasConversion<string>().HasMaxLength(32);
        bang_lich_dat.Property(dong_du_lieu => dong_du_lieu.TrangThai).HasConversion<string>().HasMaxLength(32);
        bang_lich_dat.HasIndex(dong_du_lieu => new { dong_du_lieu.DiaDiemId, dong_du_lieu.TrangThai, dong_du_lieu.BatDau, dong_du_lieu.KetThuc }, "IX_LichDatDiaDiem_TraLich");
    }
}
