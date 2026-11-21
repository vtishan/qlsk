using Microsoft.EntityFrameworkCore;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configs;

internal static class CauHinhTaiChinh
{
    public static void CauHinh(ModelBuilder mo_hinh_bang)
    {
        var bang_dung_ma = mo_hinh_bang.Entity<SuDungVoucher>();
        bang_dung_ma.ToTable(nameof(SuDungVoucher), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_SuDungVoucher_DieuKien1",
                "[SoTienGiam] >= 0");
            cau_hinh_sql.HasCheckConstraint("CK_SuDungVoucher_DieuKien2",
                "([TrangThai] = N'DaSuDung' AND [DaChotLuc] IS NOT NULL) OR ([TrangThai] <> N'DaSuDung' AND [DaChotLuc] IS NULL)");
            cau_hinh_sql.HasCheckConstraint("CK_SuDungVoucher_TrangThai",
                "[TrangThai] IN (N'DangGiu', N'DaSuDung', N'DaNha')");
        });
        bang_dung_ma.HasOne<Voucher>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.VoucherId).OnDelete(DeleteBehavior.NoAction);
        bang_dung_ma.HasOne<DonHang>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.DonHangId).OnDelete(DeleteBehavior.NoAction);
        bang_dung_ma.Property(dong_du_lieu => dong_du_lieu.TrangThai).HasConversion<string>().HasMaxLength(32);
        bang_dung_ma.HasIndex(dong_du_lieu => dong_du_lieu.DonHangId, "IX_SuDungVoucher_DonHang").IsUnique();
        bang_dung_ma.HasIndex(dong_du_lieu => new { dong_du_lieu.VoucherId, dong_du_lieu.TrangThai, dong_du_lieu.HetHanGiuLuot }, "IX_SuDungVoucher_DemLuot");

        var bang_thanh_toan = mo_hinh_bang.Entity<GiaoDichThanhToan>();
        bang_thanh_toan.ToTable(nameof(GiaoDichThanhToan), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_GiaoDichThanhToan_TrangThai",
                "[TrangThai] IN (N'ChoThanhToan', N'ThanhCong', N'ThatBai', N'HetHan', N'NghiVan', N'ThuTrungCanHoan')");
            cau_hinh_sql.HasCheckConstraint("CK_GiaoDichThanhToan_PhuongThuc",
                "[PhuongThuc] IN (N'PayOS', N'ChuyenKhoan', N'MienPhi')");
            cau_hinh_sql.HasCheckConstraint("CK_GiaoDichThanhToan_ThucThu",
                "[TrangThai] <> N'ThanhCong' OR ([SoTienThucThu] IS NOT NULL AND [SoTienThucThu] = [SoTienYeuCau] AND [NgayThanhToan] IS NOT NULL)");
            cau_hinh_sql.HasCheckConstraint("CK_GiaoDichThanhToan_DieuKien1",
                "([DonHangId] IS NULL AND [YeuCauThueId] IS NOT NULL) OR ([DonHangId] IS NOT NULL AND [YeuCauThueId] IS NULL)");
            cau_hinh_sql.HasCheckConstraint("CK_GiaoDichThanhToan_DieuKien2",
                "[SoTienYeuCau] >= 0 AND ([SoTienThucThu] IS NULL OR [SoTienThucThu] >= 0)");
        });
        bang_thanh_toan.HasOne<DonHang>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.DonHangId).OnDelete(DeleteBehavior.NoAction);
        bang_thanh_toan.HasOne<YeuCauThue>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.YeuCauThueId).OnDelete(DeleteBehavior.NoAction);
        bang_thanh_toan.Property(dong_du_lieu => dong_du_lieu.MaDonNhaCungCap).HasMaxLength(128);
        bang_thanh_toan.Property(dong_du_lieu => dong_du_lieu.MaGiaoDich).HasMaxLength(128);
        bang_thanh_toan.Property(dong_du_lieu => dong_du_lieu.PhuongThuc).HasConversion<string>().HasMaxLength(32);
        bang_thanh_toan.Property(dong_du_lieu => dong_du_lieu.TrangThai).HasConversion<string>().HasMaxLength(32);
        bang_thanh_toan.Property(dong_du_lieu => dong_du_lieu.ChungTu).HasMaxLength(500);
        bang_thanh_toan.HasIndex(dong_du_lieu => dong_du_lieu.MaDonNhaCungCap, "IX_GiaoDichThanhToan_MaDon").IsUnique()
            .HasFilter("[MaDonNhaCungCap] IS NOT NULL");
        bang_thanh_toan.HasIndex(dong_du_lieu => dong_du_lieu.MaGiaoDich, "IX_GiaoDichThanhToan_MaGiaoDich").IsUnique()
            .HasFilter("[MaGiaoDich] IS NOT NULL");
        bang_thanh_toan.HasIndex(dong_du_lieu => dong_du_lieu.DonHangId, "IX_GiaoDichThanhToan_ChotDon").IsUnique()
            .HasFilter("[DonHangId] IS NOT NULL AND [TrangThai] = N'ThanhCong'");
        bang_thanh_toan.HasIndex(dong_du_lieu => dong_du_lieu.YeuCauThueId, "IX_GiaoDichThanhToan_ChotCoc").IsUnique()
            .HasFilter("[YeuCauThueId] IS NOT NULL AND [TrangThai] = N'ThanhCong'");

        var bang_thong_diep = mo_hinh_bang.Entity<WebhookThanhToan>();
        bang_thong_diep.ToTable(nameof(WebhookThanhToan), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_WebhookThanhToan_TrangThaiXuLy",
                "[TrangThaiXuLy] IN (N'ChoXuLy', N'DaXuLy', N'NghiVan', N'Loi')");
            cau_hinh_sql.HasCheckConstraint("CK_WebhookThanhToan_NoiDungDaLoc",
                "ISJSON([NoiDungDaLoc], OBJECT) = 1");
        });
        bang_thong_diep.HasOne<GiaoDichThanhToan>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.GiaoDichId).OnDelete(DeleteBehavior.NoAction);
        bang_thong_diep.Property(dong_du_lieu => dong_du_lieu.MaSuKienWebhook).HasMaxLength(128);
        bang_thong_diep.Property(dong_du_lieu => dong_du_lieu.TrangThaiXuLy).HasConversion<string>().HasMaxLength(32);
        bang_thong_diep.HasIndex(dong_du_lieu => dong_du_lieu.MaSuKienWebhook, "IX_WebhookThanhToan_MaSuKien").IsUnique();

        var bang_yeu_cau = mo_hinh_bang.Entity<YeuCauHoanTien>();
        bang_yeu_cau.ToTable(nameof(YeuCauHoanTien), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_YeuCauHoanTien_DieuKien1",
                "[SoTienYeuCau] > 0 AND ([SoTienDuyet] IS NULL OR [SoTienDuyet] BETWEEN 1 AND [SoTienYeuCau])");
            cau_hinh_sql.HasCheckConstraint("CK_YeuCauHoanTien_TrangThai",
                "[TrangThai] IN (N'ChoDuyet', N'DaDuyet', N'TuChoi', N'DangChuyen', N'HoanTat', N'ThatBai')");
        });
        bang_yeu_cau.HasOne<TaiKhoan>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.TaiKhoanId).OnDelete(DeleteBehavior.NoAction);
        bang_yeu_cau.HasOne<GiaoDichThanhToan>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.GiaoDichId).OnDelete(DeleteBehavior.NoAction);
        bang_yeu_cau.HasOne<Ve>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.VeId).OnDelete(DeleteBehavior.NoAction);
        bang_yeu_cau.Property(dong_du_lieu => dong_du_lieu.KhoaChongLap).HasMaxLength(128);
        bang_yeu_cau.Property(dong_du_lieu => dong_du_lieu.LoaiYeuCau).HasMaxLength(32);
        bang_yeu_cau.Property(dong_du_lieu => dong_du_lieu.TrangThai).HasConversion<string>().HasMaxLength(32);
        bang_yeu_cau.HasOne<TaiKhoan>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.NguoiDuyetId).OnDelete(DeleteBehavior.NoAction);
        bang_yeu_cau.HasIndex(dong_du_lieu => dong_du_lieu.KhoaChongLap, "IX_YeuCauHoanTien_ChongLap").IsUnique();

        var bang_hoan_tien = mo_hinh_bang.Entity<GiaoDichHoanTien>();
        bang_hoan_tien.ToTable(nameof(GiaoDichHoanTien), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_GiaoDichHoanTien_TrangThai",
                "[TrangThai] IN (N'ChoChuyen', N'DangChuyen', N'ThanhCong', N'ThatBai')");
            cau_hinh_sql.HasCheckConstraint("CK_GiaoDichHoanTien_DieuKien1",
                "[SoTien] > 0");
            cau_hinh_sql.HasCheckConstraint("CK_GiaoDichHoanTien_DieuKien2",
                "[TrangThai] <> N'ThanhCong' OR ([NgayXuLy] IS NOT NULL AND ([MaGiaoDichHoan] IS NOT NULL OR [ChungTu] IS NOT NULL))");
        });
        bang_hoan_tien.HasOne<YeuCauHoanTien>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.YeuCauHoanTienId).OnDelete(DeleteBehavior.NoAction);
        bang_hoan_tien.Property(dong_du_lieu => dong_du_lieu.KhoaChongLap).HasMaxLength(128);
        bang_hoan_tien.Property(dong_du_lieu => dong_du_lieu.MaGiaoDichHoan).HasMaxLength(128);
        bang_hoan_tien.Property(dong_du_lieu => dong_du_lieu.TrangThai).HasConversion<string>().HasMaxLength(32);
        bang_hoan_tien.Property(dong_du_lieu => dong_du_lieu.ChungTu).HasMaxLength(500);
        bang_hoan_tien.HasIndex(dong_du_lieu => dong_du_lieu.KhoaChongLap, "IX_GiaoDichHoanTien_ChongLap").IsUnique();
        bang_hoan_tien.HasIndex(dong_du_lieu => dong_du_lieu.MaGiaoDichHoan, "IX_GiaoDichHoanTien_MaGiaoDich").IsUnique()
            .HasFilter("[MaGiaoDichHoan] IS NOT NULL");

        var bang_hop_dong = mo_hinh_bang.Entity<HopDong>();
        bang_hop_dong.ToTable(nameof(HopDong), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_HopDong_TrangThai",
                "[TrangThai] IN (N'Nhap', N'CoHieuLuc', N'HetHan', N'DaHuy')");
            cau_hinh_sql.HasCheckConstraint("CK_HopDong_DieuKien1",
                "[HieuLucDen] IS NULL OR [HieuLucTu] < [HieuLucDen]");
            cau_hinh_sql.HasCheckConstraint("CK_HopDong_DieuKien2",
                "([LoaiPhi] = N'PhanTram' AND [TyLePhi] IS NOT NULL AND [TyLePhi] BETWEEN 0 AND 100 AND [PhiMoiVe] IS NULL) OR ([LoaiPhi] = N'MoiVe' AND [TyLePhi] IS NULL AND [PhiMoiVe] IS NOT NULL AND [PhiMoiVe] >= 0)");
        });
        bang_hop_dong.HasOne<HoSoBTC>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.BTCId).OnDelete(DeleteBehavior.NoAction);
        bang_hop_dong.HasOne<SuKien>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.SuKienId).OnDelete(DeleteBehavior.NoAction);
        bang_hop_dong.Property(dong_du_lieu => dong_du_lieu.LoaiPhi).HasConversion<string>().HasMaxLength(32);
        bang_hop_dong.Property(dong_du_lieu => dong_du_lieu.TyLePhi).HasPrecision(5, 2);
        bang_hop_dong.Property(dong_du_lieu => dong_du_lieu.TrangThai).HasConversion<string>().HasMaxLength(32);
        bang_hop_dong.HasIndex(dong_du_lieu => new { dong_du_lieu.BTCId, dong_du_lieu.SuKienId, dong_du_lieu.HieuLucTu, dong_du_lieu.HieuLucDen }, "IX_HopDong_HieuLuc");

        var bang_doi_soat = mo_hinh_bang.Entity<DoiSoat>();
        bang_doi_soat.ToTable(nameof(DoiSoat), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_DoiSoat_ChiTietDoiSoat",
                "ISJSON([ChiTietDoiSoat], OBJECT) = 1");
            cau_hinh_sql.HasCheckConstraint("CK_DoiSoat_DieuKien1",
                "[TuNgay] <= [DenNgay] AND [PhienBan] > 0");
            cau_hinh_sql.HasCheckConstraint("CK_DoiSoat_DieuKien2",
                "[TongDoanhThu] >= 0 AND [TongTienHoan] >= 0 AND [PhiNenTang] >= 0 AND [TienBTCNhan] = [TongDoanhThu] - [TongTienHoan] - [PhiNenTang] - [TienDieuChinh]");
            cau_hinh_sql.HasCheckConstraint("CK_DoiSoat_TrangThai",
                "[TrangThai] IN (N'ChoXacNhan', N'DaXacNhan', N'TranhChap', N'ChoGiaiNgan', N'HoanTat')");
        });
        bang_doi_soat.HasOne<SuKien>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.SuKienId).OnDelete(DeleteBehavior.NoAction);
        bang_doi_soat.HasOne<TaiKhoan>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.NguoiDuyetId).OnDelete(DeleteBehavior.NoAction);
        bang_doi_soat.Property(dong_du_lieu => dong_du_lieu.TrangThai).HasConversion<string>().HasMaxLength(32);
        bang_doi_soat.HasIndex(dong_du_lieu => new { dong_du_lieu.SuKienId, dong_du_lieu.TuNgay, dong_du_lieu.DenNgay, dong_du_lieu.PhienBan }, "IX_DoiSoat_PhienBan").IsUnique();
        bang_doi_soat.HasIndex(dong_du_lieu => new { dong_du_lieu.SuKienId, dong_du_lieu.TuNgay, dong_du_lieu.DenNgay }, "IX_DoiSoat_KyChiTra").IsUnique()
            .HasFilter("[TrangThai] IN (N'ChoGiaiNgan', N'HoanTat')");

        var bang_giai_ngan = mo_hinh_bang.Entity<GiaiNgan>();
        bang_giai_ngan.ToTable(nameof(GiaiNgan), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_GiaiNgan_DieuKien1",
                "[SoTien] > 0");
            cau_hinh_sql.HasCheckConstraint("CK_GiaiNgan_DieuKien2",
                "[TrangThai] <> N'ThanhCong' OR ([NgayGiaiNgan] IS NOT NULL AND ([MaGiaoDich] IS NOT NULL OR [ChungTu] IS NOT NULL))");
            cau_hinh_sql.HasCheckConstraint("CK_GiaiNgan_TrangThai",
                "[TrangThai] IN (N'ChoChuyen', N'DangChuyen', N'ThanhCong', N'ThatBai')");
        });
        bang_giai_ngan.HasOne<DoiSoat>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.DoiSoatId).OnDelete(DeleteBehavior.NoAction);
        bang_giai_ngan.Property(dong_du_lieu => dong_du_lieu.KhoaChongLap).HasMaxLength(128);
        bang_giai_ngan.Property(dong_du_lieu => dong_du_lieu.MaGiaoDich).HasMaxLength(128);
        bang_giai_ngan.Property(dong_du_lieu => dong_du_lieu.TrangThai).HasConversion<string>().HasMaxLength(32);
        bang_giai_ngan.Property(dong_du_lieu => dong_du_lieu.ChungTu).HasMaxLength(500);
        bang_giai_ngan.HasIndex(dong_du_lieu => dong_du_lieu.DoiSoatId, "IX_GiaiNgan_DoiSoat").IsUnique();
        bang_giai_ngan.HasIndex(dong_du_lieu => dong_du_lieu.KhoaChongLap, "IX_GiaiNgan_ChongLap").IsUnique();
        bang_giai_ngan.HasIndex(dong_du_lieu => dong_du_lieu.MaGiaoDich, "IX_GiaiNgan_MaGiaoDich").IsUnique()
            .HasFilter("[MaGiaoDich] IS NOT NULL");
    }
}
