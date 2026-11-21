using Microsoft.EntityFrameworkCore;
using QLSK.Domain.Entities;

namespace QLSK.Data.Configs;

internal static class CauHinhDatVe
{
    public static void CauHinh(ModelBuilder mo_hinh_bang)
    {
        var bang_loai_ve = mo_hinh_bang.Entity<LoaiVe>();
        bang_loai_ve.ToTable(nameof(LoaiVe), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_LoaiVe_DieuKien1",
                "[GiaVe] >= 0 AND [SoLuong] >= 0");
            cau_hinh_sql.HasCheckConstraint("CK_LoaiVe_DieuKien2",
                "[BatDauBan] < [KetThucBan]");
            cau_hinh_sql.HasCheckConstraint("CK_LoaiVe_DieuKien3",
                "[NhomVe] IN (N'TieuChuan', N'VIP', N'VeMoi') AND ([NhomVe] <> N'VeMoi' OR [GiaVe] = 0)");
        });
        bang_loai_ve.HasOne<SuatDien>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.SuatDienId).OnDelete(DeleteBehavior.NoAction);
        bang_loai_ve.HasOne<KhuVuc>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.KhuVucId).OnDelete(DeleteBehavior.NoAction);
        bang_loai_ve.Property(dong_du_lieu => dong_du_lieu.TenLoaiVe).HasMaxLength(120);
        bang_loai_ve.Property(dong_du_lieu => dong_du_lieu.NhomVe).HasConversion<string>().HasMaxLength(32);

        var bang_don_hang = mo_hinh_bang.Entity<DonHang>();
        bang_don_hang.ToTable(nameof(DonHang), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_DonHang_DieuKien1",
                "[TienVe] >= 0 AND [TienGiam] BETWEEN 0 AND [TienVe] AND [PhiDichVu] >= 0 AND [PhiNenTang] >= 0 AND [TongTien] = [TienVe] - [TienGiam] + [PhiDichVu]");
            cau_hinh_sql.HasCheckConstraint("CK_DonHang_DieuKien2",
                "[HanThanhToan] > [NgayTao]");
            cau_hinh_sql.HasCheckConstraint("CK_DonHang_TrangThai",
                "[TrangThai] IN (N'ChoThanhToan', N'DaThanhToan', N'HetHan', N'DaHuy', N'DaThuTienHetCho', N'ChoHoanTien', N'HoanMotPhan', N'DaHoanTien')");
        });
        bang_don_hang.HasOne<TaiKhoan>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.TaiKhoanId).OnDelete(DeleteBehavior.NoAction);
        bang_don_hang.HasOne<SuatDien>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.SuatDienId).OnDelete(DeleteBehavior.NoAction);
        bang_don_hang.HasOne<HopDong>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.HopDongId).OnDelete(DeleteBehavior.NoAction);
        bang_don_hang.HasOne<ChinhSach>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.ChinhSachId).OnDelete(DeleteBehavior.NoAction);
        bang_don_hang.Property(dong_du_lieu => dong_du_lieu.TrangThai).HasConversion<string>().HasMaxLength(32);
        bang_don_hang.HasIndex(dong_du_lieu => new { dong_du_lieu.SuatDienId, dong_du_lieu.TaiKhoanId, dong_du_lieu.TrangThai }, "IX_DonHang_HanMuc");
        bang_don_hang.Property(row => row.KhoaTaoDon).HasMaxLength(64);
        bang_don_hang.Property(row => row.DauVanTayTaoDon).HasMaxLength(64);

        var bang_chi_tiet = mo_hinh_bang.Entity<ChiTietDonHang>();
        bang_chi_tiet.ToTable(nameof(ChiTietDonHang), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_ChiTietDonHang_DieuKien1",
                "[SoLuong] > 0 AND [DonGia] >= 0 AND [ThanhTien] = [SoLuong] * [DonGia]");
        });
        bang_chi_tiet.HasOne<DonHang>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.DonHangId).OnDelete(DeleteBehavior.NoAction);
        bang_chi_tiet.HasOne<LoaiVe>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.LoaiVeId).OnDelete(DeleteBehavior.NoAction);

        var bang_giu_ghe = mo_hinh_bang.Entity<GiuGhe>();
        bang_giu_ghe.ToTable(nameof(GiuGhe), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_GiuGhe_TrangThai",
                "[TrangThai] IN (N'DangGiu', N'DaChot', N'DaNha', N'HetHan')");
        });
        bang_giu_ghe.HasOne<SuatDien>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.SuatDienId).OnDelete(DeleteBehavior.NoAction);
        bang_giu_ghe.HasOne<Ghe>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.GheId).OnDelete(DeleteBehavior.NoAction);
        bang_giu_ghe.HasOne<ChiTietDonHang>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.ChiTietDonHangId).OnDelete(DeleteBehavior.NoAction);
        bang_giu_ghe.Property(dong_du_lieu => dong_du_lieu.TrangThai).HasConversion<string>().HasMaxLength(32);
        bang_giu_ghe.HasIndex(dong_du_lieu => new { dong_du_lieu.TrangThai, dong_du_lieu.HetHan }, "IX_GiuGhe_HetHan");
        bang_giu_ghe.HasIndex(dong_du_lieu => new { dong_du_lieu.SuatDienId, dong_du_lieu.GheId }, "IX_GiuGhe_GheDangGiu").IsUnique()
            .HasFilter("[GheId] IS NOT NULL AND [TrangThai] = N'DangGiu'");

        var bang_ve_ban = mo_hinh_bang.Entity<Ve>();
        bang_ve_ban.ToTable(nameof(Ve), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_Ve_DieuKien1",
                "([TrangThai] = N'DaCheckIn' AND [DaCheckInLuc] IS NOT NULL) OR ([TrangThai] = N'DaPhatHanh' AND [DaCheckInLuc] IS NULL) OR [TrangThai] IN (N'DaHuy', N'DaThuHoi')");
            cau_hinh_sql.HasCheckConstraint("CK_Ve_DieuKien2",
                "[GiuGheId] IS NULL OR [ChiTietDonHangId] IS NOT NULL");
            cau_hinh_sql.HasCheckConstraint("CK_Ve_TrangThai",
                "[TrangThai] IN (N'DaPhatHanh', N'DaCheckIn', N'DaHuy', N'DaThuHoi')");
        });
        bang_ve_ban.HasOne<ChiTietDonHang>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.ChiTietDonHangId).OnDelete(DeleteBehavior.NoAction);
        bang_ve_ban.HasOne<GiuGhe>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.GiuGheId).OnDelete(DeleteBehavior.NoAction);
        bang_ve_ban.HasOne<LoaiVe>().WithMany()
            .HasForeignKey(dong_du_lieu => new { dong_du_lieu.LoaiVeId, dong_du_lieu.SuatDienId })
            .HasPrincipalKey(dong_tham_chieu => new { dong_tham_chieu.Id, dong_tham_chieu.SuatDienId }).OnDelete(DeleteBehavior.NoAction);
        bang_ve_ban.HasOne<SuatDien>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.SuatDienId).OnDelete(DeleteBehavior.NoAction);
        bang_ve_ban.HasOne<Ghe>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.GheId).OnDelete(DeleteBehavior.NoAction);
        bang_ve_ban.Property(dong_du_lieu => dong_du_lieu.MaVe).HasMaxLength(64);
        bang_ve_ban.Property(dong_du_lieu => dong_du_lieu.MaQR).HasMaxLength(128);
        bang_ve_ban.Property(dong_du_lieu => dong_du_lieu.TrangThai).HasConversion<string>().HasMaxLength(32);
        bang_ve_ban.HasIndex(dong_du_lieu => dong_du_lieu.MaVe, "IX_Ve_MaVe").IsUnique();
        bang_ve_ban.HasIndex(dong_du_lieu => dong_du_lieu.MaQR, "IX_Ve_MaQR").IsUnique();
        bang_ve_ban.HasIndex(dong_du_lieu => dong_du_lieu.GiuGheId, "IX_Ve_GiuGhe").IsUnique()
            .HasFilter("[GiuGheId] IS NOT NULL");
        bang_ve_ban.HasIndex(dong_du_lieu => new { dong_du_lieu.SuatDienId, dong_du_lieu.GheId }, "IX_Ve_GheDaCap").IsUnique()
            .HasFilter("[GheId] IS NOT NULL AND [TrangThai] IN (N'DaPhatHanh', N'DaCheckIn')");
        bang_ve_ban.HasIndex(dong_du_lieu => new { dong_du_lieu.ChiTietDonHangId, dong_du_lieu.SuatDienId, dong_du_lieu.TrangThai }, "IX_Ve_HanMuc");

        var bang_ve_moi = mo_hinh_bang.Entity<VeMoi>();
        bang_ve_moi.ToTable(nameof(VeMoi), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_VeMoi_TrangThai",
                "[TrangThai] IN (N'ChoGui', N'DaGui', N'DaNhan', N'DaThuHoi', N'HetHan')");
            cau_hinh_sql.HasCheckConstraint("CK_VeMoi_DieuKien1",
                "NULLIF(TRIM([Email]), N'') IS NOT NULL OR NULLIF(TRIM([SoDienThoai]), N'') IS NOT NULL");
        });
        bang_ve_moi.HasOne<Ve>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.VeId).OnDelete(DeleteBehavior.NoAction);
        bang_ve_moi.Property(dong_du_lieu => dong_du_lieu.HoTenKhachMoi).HasMaxLength(120);
        bang_ve_moi.Property(dong_du_lieu => dong_du_lieu.Email).HasMaxLength(255);
        bang_ve_moi.Property(dong_du_lieu => dong_du_lieu.SoDienThoai).HasMaxLength(15);
        bang_ve_moi.HasOne<TaiKhoan>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.TaiKhoanNhanId).OnDelete(DeleteBehavior.NoAction);
        bang_ve_moi.Property(dong_du_lieu => dong_du_lieu.MaBamNhanVe).HasMaxLength(128);
        bang_ve_moi.Property(dong_du_lieu => dong_du_lieu.TrangThai).HasConversion<string>().HasMaxLength(32);
        bang_ve_moi.HasIndex(dong_du_lieu => dong_du_lieu.VeId, "IX_VeMoi_Ve").IsUnique();

        var bang_luot_quet = mo_hinh_bang.Entity<NhatKyCheckIn>();
        bang_luot_quet.ToTable(nameof(NhatKyCheckIn), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_NhatKyCheckIn_HinhThuc",
                "[HinhThuc] IN (N'QuetQR', N'ThuCong')");
        });
        bang_luot_quet.HasOne<Ve>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.VeId).OnDelete(DeleteBehavior.NoAction);
        bang_luot_quet.HasOne<TaiKhoan>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.NhanVienId).OnDelete(DeleteBehavior.NoAction);
        bang_luot_quet.Property(dong_du_lieu => dong_du_lieu.KetQua).HasMaxLength(32);
        bang_luot_quet.Property(dong_du_lieu => dong_du_lieu.HinhThuc).HasConversion<string>().HasMaxLength(32);
        bang_luot_quet.HasIndex(dong_du_lieu => dong_du_lieu.VeId, "IX_NhatKyCheckIn_ThanhCong").IsUnique()
            .HasFilter("[KetQua] = N'ThanhCong'");

        var bang_khuyen_mai = mo_hinh_bang.Entity<Voucher>();
        bang_khuyen_mai.ToTable(nameof(Voucher), cau_hinh_sql =>
        {
            cau_hinh_sql.HasCheckConstraint("CK_Voucher_TrangThai",
                "[TrangThai] IN (N'Nhap', N'ChoDuyet', N'HoatDong', N'TamDung', N'TuChoi', N'HetHan')");
            cau_hinh_sql.HasCheckConstraint("CK_Voucher_DieuKien1",
                "[NgayBatDau] < [NgayKetThuc]");
            cau_hinh_sql.HasCheckConstraint("CK_Voucher_DieuKien2",
                "[SoLuong] > 0 AND [LuotMoiTaiKhoan] BETWEEN 1 AND [SoLuong] AND [GiaTriDonToiThieu] >= 0 AND ([MucGiamToiDa] IS NULL OR [MucGiamToiDa] > 0)");
            cau_hinh_sql.HasCheckConstraint("CK_Voucher_DieuKien3",
                "([LoaiGiam] = N'PhanTram' AND [GiaTri] > 0 AND [GiaTri] <= 100 AND [MucGiamToiDa] IS NOT NULL) OR ([LoaiGiam] = N'SoTien' AND [GiaTri] > 0 AND [GiaTri] = FLOOR([GiaTri]))");
        });
        bang_khuyen_mai.Property(dong_du_lieu => dong_du_lieu.MaVoucher).HasMaxLength(64);
        bang_khuyen_mai.HasOne<SuKien>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.SuKienId).OnDelete(DeleteBehavior.NoAction);
        bang_khuyen_mai.HasOne<SuatDien>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.SuatDienId).OnDelete(DeleteBehavior.NoAction);
        bang_khuyen_mai.HasOne<KhuVuc>().WithMany()
            .HasForeignKey(dong_du_lieu => dong_du_lieu.KhuVucId).OnDelete(DeleteBehavior.NoAction);
        bang_khuyen_mai.Property(dong_du_lieu => dong_du_lieu.LoaiGiam).HasConversion<string>().HasMaxLength(32);
        bang_khuyen_mai.Property(dong_du_lieu => dong_du_lieu.GiaTri).HasPrecision(19, 2);
        bang_khuyen_mai.Property(dong_du_lieu => dong_du_lieu.NguonTaiTro).HasMaxLength(32);
        bang_khuyen_mai.Property(dong_du_lieu => dong_du_lieu.TrangThai).HasConversion<string>().HasMaxLength(32);
        bang_khuyen_mai.HasIndex(dong_du_lieu => dong_du_lieu.MaVoucher, "IX_Voucher_Ma").IsUnique();
    }
}
