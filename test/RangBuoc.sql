SET NOCOUNT ON;
SET XACT_ABORT OFF;
SET DATEFORMAT ymd;

IF (SELECT COUNT(*) FROM sys.tables WHERE is_ms_shipped = 0) <> 37
    THROW 51000, N'Cần đúng 37 bảng nghiệp vụ.', 1;

IF (SELECT COUNT(*) FROM sys.tables AS bang_du_lieu
    JOIN sys.identity_columns AS cot_tu_tang ON cot_tu_tang.object_id = bang_du_lieu.object_id
    WHERE bang_du_lieu.is_ms_shipped = 0 AND cot_tu_tang.name = N'Id') <> 37
    THROW 51000, N'Tất cả khóa chính Id phải tự tăng.', 1;

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE delete_referential_action <> 0)
    THROW 51000, N'Khóa ngoại phải giữ lịch sử bằng NO ACTION.', 1;

BEGIN TRANSACTION;
BEGIN TRY
    DECLARE @ma_chay_thu char(32) = REPLACE(CONVERT(char(36), NEWID()), '-', '');

    INSERT INTO TaiKhoan (HoTen, Email, TenDangNhap, MatKhauBam, PhienBanDangNhap)
    VALUES (N'Mẫu 1', @ma_chay_thu + '@example.invalid', @ma_chay_thu + '1', 'mau', 0),
           (N'Mẫu 2', @ma_chay_thu + '@thu.example.invalid', @ma_chay_thu + '2', 'mau', 0);
    DECLARE @ma_tai_khoan int = CONVERT(int, SCOPE_IDENTITY());

    IF (SELECT COUNT(*) FROM TaiKhoan WHERE TenDangNhap IN (@ma_chay_thu + '1', @ma_chay_thu + '2') AND VaiTro = 1) <> 2
        THROW 51000, N'Sai vai trò mặc định.', 1;

    BEGIN TRY
        UPDATE TaiKhoan SET Email = NULL WHERE Id = @ma_tai_khoan;
        THROW 51000, N'Đã chấp nhận tài khoản thiếu liên hệ.', 1;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() <> 547 THROW;
    END CATCH;

    BEGIN TRY
        UPDATE TaiKhoan SET Email = @ma_chay_thu + '@example.invalid' WHERE Id = @ma_tai_khoan;
        THROW 51000, N'Đã chấp nhận email trùng.', 1;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() <> 2601 THROW;
    END CATCH;

    INSERT INTO HoSoBTC (TaiKhoanId, TenToChuc, NguoiDaiDien, DiaChi, ThongTinLienHe, TrangThai)
    VALUES (@ma_tai_khoan, N'Tổ chức mẫu', N'Đại diện', N'Địa chỉ', N'Liên hệ', N'DaDuyet');
    DECLARE @ma_ho_so int = CONVERT(int, SCOPE_IDENTITY());

    INSERT INTO DanhMucSuKien (TenDanhMuc, TrangThai) VALUES (N'Mẫu', N'Hien');
    DECLARE @ma_danh_muc int = CONVERT(int, SCOPE_IDENTITY());

    INSERT INTO SuKien (BTCId, DanhMucId, TenSuKien, MoTa, AnhBia, TrangThai, NgayTao, ChoPhepTuHuyVe)
    VALUES (@ma_ho_so, @ma_danh_muc, N'Sự kiện mẫu', N'Mô tả', '/mau.jpg', N'DaDuyet', '2026-11-01', 1);
    DECLARE @ma_su_kien int = CONVERT(int, SCOPE_IDENTITY());

    INSERT INTO PhienBanSuKien (SuKienId, SoPhienBan, LoaiYeuCau, NoiDung, TrangThaiDuyet, NguoiGuiId)
    VALUES (@ma_su_kien, 1, N'TaoMoi', N'{}', N'DaDuyet', @ma_tai_khoan);
    DECLARE @ma_phien_ban int = CONVERT(int, SCOPE_IDENTITY());
    UPDATE SuKien SET PhienBanCongKhaiId = @ma_phien_ban WHERE Id = @ma_su_kien;

    INSERT INTO DiaDiem (TenDiaDiem, DiaChi, SucChua, GiaThue, TrangThai)
    VALUES (N'Địa điểm mẫu', N'Địa chỉ', 10, 1000, N'HoatDong');
    DECLARE @ma_dia_diem int = CONVERT(int, SCOPE_IDENTITY());

    INSERT INTO KhuVuc (DiaDiemId, TenKhuVuc, SucChua) VALUES (@ma_dia_diem, N'Khu mẫu', 10);
    DECLARE @ma_khu_vuc int = CONVERT(int, SCOPE_IDENTITY());

    INSERT INTO Ghe (KhuVucId, MaGhe, LoaiGhe, TrangThai) VALUES (@ma_khu_vuc, N'A1', N'GheDon', N'SuDung');
    DECLARE @ma_ghe_ngoi int = CONVERT(int, SCOPE_IDENTITY());

    INSERT INTO SuatDien (SuKienId, DiaDiemId, ThoiGianBatDau, ThoiGianKetThuc, ThoiGianMoBan, ThoiGianDongBan, SoDoChot, SucChua, TrangThai)
    VALUES (@ma_su_kien, @ma_dia_diem, '2026-12-10T07:00:00', '2026-12-10T08:00:00', '2026-11-10', '2026-12-10', N'{"khuVuc":[]}', 10, N'DaXacNhan');
    DECLARE @ma_suat_dien int = CONVERT(int, SCOPE_IDENTITY());

    INSERT INTO PhanCongSuKien (SuKienId, SuatDienId, TaiKhoanId, NguoiPhanCongId, LoaiQuyen, NgayPhanCong, TrangThai)
    VALUES (@ma_su_kien, NULL, @ma_tai_khoan, @ma_tai_khoan, 'QuetQR', '2026-11-01', N'DangHieuLuc'),
           (@ma_su_kien, @ma_suat_dien, @ma_tai_khoan, @ma_tai_khoan, 'QuetQR', '2026-11-01', N'DangHieuLuc');

    BEGIN TRY
        INSERT INTO PhanCongSuKien (SuKienId, TaiKhoanId, NguoiPhanCongId, LoaiQuyen, NgayPhanCong, TrangThai)
        VALUES (@ma_su_kien, @ma_tai_khoan, @ma_tai_khoan, 'QuetQR', '2026-11-01', N'DangHieuLuc');
        THROW 51000, N'Đã chấp nhận quyền trùng toàn sự kiện.', 1;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() <> 2601 THROW;
    END CATCH;

    BEGIN TRY
        INSERT INTO PhanCongSuKien (SuKienId, SuatDienId, TaiKhoanId, NguoiPhanCongId, LoaiQuyen, NgayPhanCong, TrangThai)
        VALUES (@ma_su_kien, @ma_suat_dien, @ma_tai_khoan, @ma_tai_khoan, 'QuetQR', '2026-11-01', N'DangHieuLuc');
        THROW 51000, N'Đã chấp nhận quyền trùng theo suất.', 1;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() <> 2601 THROW;
    END CATCH;

    INSERT INTO LoaiVe (SuatDienId, KhuVucId, TenLoaiVe, NhomVe, GiaVe, SoLuong, BatDauBan, KetThucBan)
    VALUES (@ma_suat_dien, @ma_khu_vuc, N'Vé mẫu', N'TieuChuan', 1000, 10, '2026-11-10', '2026-12-10');
    DECLARE @ma_loai_ve int = CONVERT(int, SCOPE_IDENTITY());

    INSERT INTO ChinhSach (TenChinhSach, NoiDung, PhienBan, NgayHieuLuc, TrangThai)
    VALUES (@ma_chay_thu, N'Nội dung', 1, '2026-11-01', N'CoHieuLuc');
    DECLARE @ma_chinh_sach int = CONVERT(int, SCOPE_IDENTITY());

    INSERT INTO DonHang (TaiKhoanId, SuatDienId, ChinhSachId, TienVe, TienGiam, PhiDichVu, PhiNenTang, TongTien, TrangThai, NgayTao, HanThanhToan)
    VALUES (@ma_tai_khoan, @ma_suat_dien, @ma_chinh_sach, 1000, 0, 0, 0, 1000, N'ChoThanhToan', '2026-11-20T00:00:00', '2026-11-20T00:10:00');
    DECLARE @ma_don_hang int = CONVERT(int, SCOPE_IDENTITY());

    INSERT INTO ChiTietDonHang (DonHangId, LoaiVeId, SoLuong, DonGia, ThanhTien)
    VALUES (@ma_don_hang, @ma_loai_ve, 1, 1000, 1000);
    DECLARE @ma_chi_tiet int = CONVERT(int, SCOPE_IDENTITY());

    INSERT INTO GiuGhe (SuatDienId, GheId, ChiTietDonHangId, HetHan, TrangThai)
    VALUES (@ma_suat_dien, @ma_ghe_ngoi, @ma_chi_tiet, '2026-11-20T00:10:00', N'DangGiu');
    DECLARE @ma_luot_giu int = CONVERT(int, SCOPE_IDENTITY());

    BEGIN TRY
        INSERT INTO GiuGhe (SuatDienId, GheId, ChiTietDonHangId, HetHan, TrangThai)
        VALUES (@ma_suat_dien, @ma_ghe_ngoi, @ma_chi_tiet, '2026-11-20T00:10:00', N'DangGiu');
        THROW 51000, N'Đã chấp nhận giữ trùng ghế.', 1;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() <> 2601 THROW;
    END CATCH;

    UPDATE GiuGhe SET TrangThai = N'DaChot' WHERE Id = @ma_luot_giu;
    INSERT INTO Ve (ChiTietDonHangId, GiuGheId, LoaiVeId, SuatDienId, GheId, MaVe, MaQR, TrangThai, NgayPhatHanh, DaCheckInLuc)
    VALUES (@ma_chi_tiet, @ma_luot_giu, @ma_loai_ve, @ma_suat_dien, @ma_ghe_ngoi, @ma_chay_thu, @ma_chay_thu, N'DaCheckIn', '2026-11-20', '2026-12-10');
    DECLARE @ma_ve_thu int = CONVERT(int, SCOPE_IDENTITY());

    BEGIN TRY
        INSERT INTO Ve (LoaiVeId, SuatDienId, GheId, MaVe, MaQR, TrangThai, NgayPhatHanh)
        VALUES (@ma_loai_ve, @ma_suat_dien, @ma_ghe_ngoi, @ma_chay_thu + '2', @ma_chay_thu + '2', N'DaPhatHanh', '2026-11-20');
        THROW 51000, N'Đã chấp nhận hai vé cùng ghế.', 1;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() <> 2601 THROW;
    END CATCH;

    INSERT INTO NhatKyCheckIn (VeId, NhanVienId, ThoiGianQuet, KetQua, HinhThuc)
    VALUES (@ma_ve_thu, @ma_tai_khoan, '2026-12-10', N'ThanhCong', N'QuetQR');

    BEGIN TRY
        INSERT INTO NhatKyCheckIn (VeId, NhanVienId, ThoiGianQuet, KetQua, HinhThuc)
        VALUES (@ma_ve_thu, @ma_tai_khoan, '2026-12-10', N'ThanhCong', N'QuetQR');
        THROW 51000, N'Đã chấp nhận check-in thành công hai lần.', 1;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() <> 2601 THROW;
    END CATCH;

    INSERT INTO WebhookThanhToan (MaSuKienWebhook, NoiDungDaLoc, ThoiGianNhan, TrangThaiXuLy)
    VALUES (@ma_chay_thu, N'{}', '2026-11-20', N'ChoXuLy');

    BEGIN TRY
        INSERT INTO WebhookThanhToan (MaSuKienWebhook, NoiDungDaLoc, ThoiGianNhan, TrangThaiXuLy)
        VALUES (@ma_chay_thu, N'{}', '2026-11-20', N'ChoXuLy');
        THROW 51000, N'Đã chấp nhận mã webhook trùng.', 1;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() <> 2601 THROW;
    END CATCH;

    ROLLBACK TRANSACTION;
    SELECT N'Đạt kiểm thử ràng buộc 37 bảng.' AS KetQua;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
