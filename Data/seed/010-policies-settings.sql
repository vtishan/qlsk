-- The three titles are the lookup keys used by registration, rentals and booking.
CREATE TABLE #SeedPolicy (Code varchar(30) PRIMARY KEY, Id int NOT NULL);
DECLARE @today date = CONVERT(date, DATEADD(hour, 7, SYSUTCDATETIME()));
DECLARE @policies TABLE (Code varchar(30), Title nvarchar(120), Content nvarchar(1200));
INSERT @policies VALUES
('registration', N'Điều khoản sử dụng', N'Điều khoản của môi trường thử nghiệm. Người tham gia dùng thông tin giả lập, giữ riêng mật khẩu và chỉ truy cập sự kiện được cấp quyền.'),
('venue-rental', N'Chính sách thuê địa điểm', N'Ban tổ chức gửi thời gian, số khách và nhu cầu thiết bị. Sau khi được duyệt, người thuê đặt cọc 30% trong thời hạn ghi trên hồ sơ để xác nhận lịch.'),
('ticketing', N'Chính sách đặt vé', N'Chọn suất diễn, loại vé và chỗ ngồi trước khi xác nhận đơn. Yêu cầu hoàn trước giờ diễn ít nhất 72 giờ và vé chưa sử dụng được xét theo chính sách của sự kiện. Giao dịch trong bộ demo chỉ phục vụ kiểm thử.');
INSERT dbo.ChinhSach(TenChinhSach, NoiDung, PhienBan, NgayHieuLuc, TrangThai)
SELECT p.Title, p.Content, 1 + COALESCE((SELECT MAX(s.PhienBan) FROM dbo.ChinhSach s WHERE s.TenChinhSach=p.Title), 0),
       DATEADD(day, -120, @today), N'CoHieuLuc'
FROM @policies p WHERE NOT EXISTS (
    SELECT 1 FROM dbo.ChinhSach s WHERE s.TenChinhSach=p.Title AND s.TrangThai=N'CoHieuLuc' AND s.NgayHieuLuc<=@today);
INSERT #SeedPolicy
SELECT p.Code, active.Id FROM @policies p CROSS APPLY (
    SELECT TOP(1) Id FROM dbo.ChinhSach s WHERE s.TenChinhSach=p.Title AND s.TrangThai=N'CoHieuLuc' AND s.NgayHieuLuc<=@today
    ORDER BY s.NgayHieuLuc DESC, s.PhienBan DESC
) active;
INSERT dbo.ChinhSach(TenChinhSach, NoiDung, PhienBan, NgayHieuLuc, TrangThai)
VALUES (N'Hướng dẫn vào cửa (demo)', N'Mang mã vé đến khu vực soát vé. Nhân viên hỗ trợ tra cứu thủ công tại bàn đón khách.', 1, DATEADD(day, 14, @today), N'Nhap');

INSERT dbo.CauHinhHeThong(Khoa, GiaTri, MoTa, NgayCapNhat, NguoiCapNhatId)
SELECT v.Khoa, v.GiaTri, v.MoTa, SYSUTCDATETIME(), a.Id FROM (VALUES
    ('review.holidays', N'[]', N'Ngày nghỉ bổ sung ngoài thứ Bảy và Chủ nhật'),
    ('booking.serviceFee', N'0', N'Phí dịch vụ mỗi đơn vé (đồng)'),
    ('settlement.closeDays', N'3', N'Số ngày chờ khóa sổ'),
    ('settlement.responseHours', N'48', N'Số giờ chờ phản hồi đối soát')
) v(Khoa,GiaTri,MoTa) CROSS JOIN #SeedAccount a
WHERE a.Code='admin' AND NOT EXISTS (SELECT 1 FROM dbo.CauHinhHeThong s WHERE s.Khoa=v.Khoa);
