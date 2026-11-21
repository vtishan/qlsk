-- Rental, support, contract and administration examples; executed by the demo seed runner.
-- All timestamps are relative to installation, so pending actions remain actionable.
CREATE TABLE #SeedRequest (Code varchar(40) NOT NULL PRIMARY KEY, Id int NOT NULL);
CREATE TABLE #SeedContract (Code varchar(40) NOT NULL PRIMARY KEY, Id int NOT NULL);
DECLARE @RentalNow datetime2(3) = SYSUTCDATETIME();
DECLARE @RentalAdmin int = (SELECT Id FROM #SeedAccount WHERE Code = 'admin');
DECLARE @RentalManager int = (SELECT Id FROM #SeedAccount WHERE Code = 'manager');
DECLARE @RentalPolicy int = (SELECT Id FROM #SeedPolicy WHERE Code = 'venue-rental');
DECLARE @RentalRows TABLE (Code varchar(40), AccountCode varchar(30), VenueCode varchar(30), DayOffset int, Phase nvarchar(32), Amount decimal(19,0), Note nvarchar(500));
INSERT @RentalRows VALUES
('requested','organizer','studio',7,N'ChoDuyet',8000000,N'Đề nghị duyệt hội thảo khởi nghiệp; cần máy chiếu, hai micro không dây và bàn tiếp tân.'),
('customer-requested','customer','garden',8,N'ChoDuyet',12000000,N'Ngày hội gia đình, dự kiến 100 khách; đề nghị bố trí lối đi cho xe lăn.'),
('deposit-due','organizer','studio',9,N'ChoCoc',10000000,N'Đã duyệt phương án tổ chức. Vui lòng đặt cọc trong 24 giờ để xác nhận lịch.'),
('confirmed','organizer','garden',12,N'DaXacNhan',15000000,N'Đã nhận cọc và chốt phương án sân khấu; bàn giao thiết bị lúc 08:00.'),
('rejected','organizer','main',16,N'TuChoi',20000000,N'Chưa đáp ứng phương án an toàn và giới hạn âm thanh. Có thể nộp lại sau khi bổ sung hồ sơ.'),
('expired','customer','studio',18,N'ChoCoc',8000000,N'Đã quá hạn cọc; lịch giữ được giải phóng, cần gửi yêu cầu mới.'),
('cancelled','organizer','garden',20,N'DaHuy',15000000,N'Ban tổ chức chủ động hủy trước khi thanh toán do thay đổi kế hoạch.'),
('refund-review','customer','studio',22,N'DaHuy',10000000,N'Khách đã thanh toán cọc; địa điểm phải bảo trì đột xuất, đề nghị hoàn toàn bộ cọc.'),
('refund-completed','organizer','garden',-20,N'DaHuy',15000000,N'Đã hoàn cọc do thay đổi lịch bảo trì từ phía địa điểm.'),
('other-confirmed','other-organizer','garden',25,N'DaXacNhan',12000000,N'Yêu cầu của đơn vị khác để kiểm tra phân quyền dữ liệu.');
DECLARE @RentalCode varchar(40), @RentalAccountCode varchar(30), @RentalVenueCode varchar(30), @RentalDay int, @RentalPhase nvarchar(32), @RentalAmount decimal(19,0), @RentalNote nvarchar(500), @RentalId int;
DECLARE RentalCursor CURSOR LOCAL FAST_FORWARD FOR SELECT Code,AccountCode,VenueCode,DayOffset,Phase,Amount,Note FROM @RentalRows;
OPEN RentalCursor;
FETCH NEXT FROM RentalCursor INTO @RentalCode,@RentalAccountCode,@RentalVenueCode,@RentalDay,@RentalPhase,@RentalAmount,@RentalNote;
WHILE @@FETCH_STATUS = 0
BEGIN
    INSERT dbo.YeuCauThue (TaiKhoanId,DiaDiemId,ThoiGianBatDau,ThoiGianKetThuc,TongTien,TienCoc,HanDatCoc,ChinhSachId,TrangThai,NguoiDuyetId,LyDoXuLy,NgayHuy,NgayTao,NoiDungYeuCau)
    SELECT a.Id,v.Id,DATEADD(day,@RentalDay,@RentalNow),DATEADD(hour,6,DATEADD(day,@RentalDay,@RentalNow)),@RentalAmount,ROUND(@RentalAmount * 0.30,0),
        CASE WHEN @RentalPhase=N'ChoCoc' THEN DATEADD(hour,CASE WHEN @RentalCode='expired' THEN -2 ELSE 24 END,@RentalNow) END,
        @RentalPolicy,@RentalPhase,CASE WHEN @RentalPhase<>N'ChoDuyet' THEN @RentalManager END,@RentalNote,
        CASE WHEN @RentalPhase=N'DaHuy' THEN DATEADD(day,-1,@RentalNow) END,DATEADD(day,-3,@RentalNow),NULL
    FROM #SeedAccount a CROSS JOIN #SeedVenue v
    WHERE a.Code=@RentalAccountCode AND v.Code=@RentalVenueCode;
    SET @RentalId=CONVERT(int,SCOPE_IDENTITY());
    INSERT #SeedRequest VALUES (@RentalCode,@RentalId);
    IF @RentalPhase IN (N'ChoCoc',N'DaXacNhan')
        INSERT dbo.LichDatDiaDiem (DiaDiemId,YeuCauThueId,SuatDienId,BatDau,KetThuc,LoaiLich,TrangThai)
        SELECT DiaDiemId,Id,NULL,ThoiGianBatDau,ThoiGianKetThuc,N'Thue',CASE WHEN @RentalCode='expired' THEN N'HetHan' WHEN @RentalPhase=N'ChoCoc' THEN N'DangGiu' ELSE N'DaXacNhan' END
        FROM dbo.YeuCauThue WHERE Id=@RentalId;
    INSERT dbo.NhatKyHeThong (TaiKhoanId,HanhDong,DoiTuong,DoiTuongId,ThoiGian,DuLieuThayDoi)
    VALUES (@RentalManager,N'Xử lý yêu cầu thuê',N'YeuCauThue',@RentalId,DATEADD(day,-2,@RentalNow),(SELECT @RentalNote AS note FOR JSON PATH,WITHOUT_ARRAY_WRAPPER));
    FETCH NEXT FROM RentalCursor INTO @RentalCode,@RentalAccountCode,@RentalVenueCode,@RentalDay,@RentalPhase,@RentalAmount,@RentalNote;
END;
CLOSE RentalCursor;
DEALLOCATE RentalCursor;

-- A maintenance block is the existing schema's supported venue operations record.
INSERT dbo.LichDatDiaDiem (DiaDiemId,YeuCauThueId,SuatDienId,BatDau,KetThuc,LoaiLich,TrangThai)
SELECT Id,NULL,NULL,DATEADD(day,3,@RentalNow),DATEADD(hour,8,DATEADD(day,3,@RentalNow)),N'BaoTri',N'DaXacNhan' FROM #SeedVenue WHERE Code='studio';
INSERT dbo.NhatKyHeThong (TaiKhoanId,HanhDong,DoiTuong,DoiTuongId,ThoiGian,DuLieuThayDoi)
SELECT @RentalManager,N'Lập lịch bảo trì và kiểm tra thiết bị',N'DiaDiem',Id,@RentalNow,
 N'{"note":"Kiểm tra tải điện, micro và máy chiếu; vệ sinh điều hòa, thử đèn thoát hiểm trước khi mở lại phòng."}' FROM #SeedVenue WHERE Code='studio';

CREATE TABLE #SeedRentalPayment (Code varchar(40) NOT NULL PRIMARY KEY,Id int NOT NULL);
DECLARE @RentalPaymentCode varchar(40), @RentalPaymentId int;
DECLARE RentalPaymentCursor CURSOR LOCAL FAST_FORWARD FOR SELECT Code FROM #SeedRequest WHERE Code IN ('confirmed','other-confirmed','refund-review','refund-completed');
OPEN RentalPaymentCursor;
FETCH NEXT FROM RentalPaymentCursor INTO @RentalPaymentCode;
WHILE @@FETCH_STATUS=0
BEGIN
    INSERT dbo.GiaoDichThanhToan (DonHangId,YeuCauThueId,MaDonNhaCungCap,MaGiaoDich,SoTienYeuCau,SoTienThucThu,PhuongThuc,TrangThai,NgayThanhToan,ChungTu)
    SELECT NULL,r.Id,'DEMO-RENT-'+s.Code,'DEMO-RENT-PAID-'+s.Code,r.TienCoc,r.TienCoc,N'ChuyenKhoan',N'ThanhCong',DATEADD(day,-2,@RentalNow),N'Biên nhận cọc mẫu; không phải giao dịch ngân hàng thực tế.'
    FROM #SeedRequest s JOIN dbo.YeuCauThue r ON r.Id=s.Id WHERE s.Code=@RentalPaymentCode;
    INSERT #SeedRentalPayment VALUES (@RentalPaymentCode,CONVERT(int,SCOPE_IDENTITY()));
    FETCH NEXT FROM RentalPaymentCursor INTO @RentalPaymentCode;
END;
CLOSE RentalPaymentCursor;
DEALLOCATE RentalPaymentCursor;
INSERT dbo.YeuCauHoanTien (TaiKhoanId,GiaoDichId,VeId,KhoaChongLap,SoTienYeuCau,SoTienDuyet,LoaiYeuCau,LyDo,TrangThai,NgayYeuCau,NguoiDuyetId,NgayDuyet)
SELECT r.TaiKhoanId,p.Id,NULL,'DEMO-RENT-REFUND-'+s.Code,r.TienCoc,CASE WHEN s.Code='refund-completed' THEN r.TienCoc END,N'Hoàn cọc thuê',r.LyDoXuLy,
 CASE WHEN s.Code='refund-completed' THEN N'HoanTat' ELSE N'ChoDuyet' END,DATEADD(day,-1,@RentalNow),CASE WHEN s.Code='refund-completed' THEN @RentalManager END,CASE WHEN s.Code='refund-completed' THEN @RentalNow END
FROM #SeedRequest s JOIN dbo.YeuCauThue r ON r.Id=s.Id JOIN #SeedRentalPayment p ON p.Code=s.Code WHERE s.Code IN ('refund-review','refund-completed');
INSERT dbo.GiaoDichHoanTien (YeuCauHoanTienId,KhoaChongLap,MaGiaoDichHoan,SoTien,TrangThai,NgayXuLy,ChungTu)
SELECT Id,'DEMO-RENT-REFUND-TRANSFER','DEMO-RENT-REFUNDED',SoTienDuyet,N'ThanhCong',@RentalNow,N'Ủy nhiệm chi hoàn cọc mẫu.' FROM dbo.YeuCauHoanTien WHERE KhoaChongLap='DEMO-RENT-REFUND-refund-completed';

-- Event-specific fee agreements allow finance and settlement pages to calculate fees.
DECLARE @RentalEventCode varchar(40), @RentalEventId int;
DECLARE RentalContractCursor CURSOR LOCAL FAST_FORWARD FOR SELECT Code,Id FROM #SeedEvent;
OPEN RentalContractCursor;
FETCH NEXT FROM RentalContractCursor INTO @RentalEventCode,@RentalEventId;
WHILE @@FETCH_STATUS=0
BEGIN
    INSERT dbo.HopDong (BTCId,SuKienId,LoaiPhi,TyLePhi,PhiMoiVe,NoiDung,NgayKy,HieuLucTu,HieuLucDen,TrangThai)
    SELECT BTCId,Id,N'PhanTram',5.00,NULL,N'Phí nền tảng 5% trên doanh thu vé sau hoàn tiền. Đối soát sau sự kiện, BTC phản hồi trong 48 giờ; chuyển khoản sau khi hai bên xác nhận.',DATEADD(day,-60,@RentalNow),DATEADD(day,-60,@RentalNow),DATEADD(day,365,@RentalNow),N'CoHieuLuc'
    FROM dbo.SuKien WHERE Id=@RentalEventId;
    INSERT #SeedContract VALUES (@RentalEventCode,CONVERT(int,SCOPE_IDENTITY()));
    FETCH NEXT FROM RentalContractCursor INTO @RentalEventCode,@RentalEventId;
END;
CLOSE RentalContractCursor;
DEALLOCATE RentalContractCursor;

INSERT dbo.KhieuNai (TaiKhoanId,SuKienId,VeId,YeuCauThueId,TieuDe,NoiDung,TrangThai,NguoiXuLyId,NgayTao,KetQuaXuLy)
SELECT a.Id,NULL,NULL,r.Id,x.Subject,x.Body,x.Phase,CASE WHEN x.Phase<>N'ChoXuLy' THEN @RentalManager END,DATEADD(hour,-6,@RentalNow),x.Resolution
FROM (VALUES
 ('customer','refund-review',N'Đề nghị kiểm tra tiến độ hoàn cọc',N'Đã gửi yêu cầu hoàn cọc do địa điểm bảo trì. Xin xác nhận thời gian nhận tiền.',N'ChoXuLy',CAST(NULL AS nvarchar(500))),
 ('organizer','confirmed',N'Bổ sung micro cho chương trình',N'Ban tổ chức cần thêm hai micro không dây và một kỹ thuật viên trực âm thanh.',N'DangXuLy',N'Đã chuyển bộ phận kỹ thuật kiểm tra tồn thiết bị; sẽ báo giá bổ sung trong ngày.'),
 ('organizer','refund-completed',N'Xác nhận đã nhận tiền hoàn',N'Ban tổ chức cần bản xác nhận hoàn cọc để đối chiếu kế toán.',N'DaGiaiQuyet',N'Đã gửi biên nhận hoàn cọc và xác nhận không còn công nợ.'),
 ('customer','expired',N'Gia hạn yêu cầu quá hạn cọc',N'Tôi muốn tiếp tục đặt phòng theo yêu cầu trước.',N'TuChoi',N'Yêu cầu cũ đã hết hạn. Vui lòng tạo yêu cầu mới để kiểm tra lịch trống.')
) x(AccountCode,RentalCode,Subject,Body,Phase,Resolution)
JOIN #SeedAccount a ON a.Code=x.AccountCode JOIN #SeedRequest r ON r.Code=x.RentalCode;

INSERT dbo.ThongBao (TaiKhoanId,VeMoiId,TieuDe,NoiDung,LoaiThongBao,KenhGui,TrangThaiGui,DaDoc,NgayTao,NgayGui)
SELECT a.Id,NULL,x.Title,x.Body,x.Kind,N'TrongUngDung',N'DaGui',x.IsRead,DATEADD(hour,-2,@RentalNow),DATEADD(hour,-2,@RentalNow)
FROM (VALUES
 ('organizer',N'Yêu cầu thuê đã được duyệt',N'Vui lòng mở yêu cầu thuê và hoàn tất đặt cọc trong 24 giờ.',N'rental',0),
 ('organizer',N'Bộ phận kỹ thuật đang xử lý',N'Yêu cầu bổ sung micro đã được tiếp nhận.',N'support',0),
 ('customer',N'Đã tiếp nhận yêu cầu hoàn cọc',N'Bộ phận tài chính đang kiểm tra chứng từ hoàn cọc.',N'refund',0),
 ('customer',N'Yêu cầu thuê đã hết hạn cọc',N'Bạn có thể gửi lại yêu cầu thuê để chọn lịch còn trống.',N'rental',1),
 ('admin',N'Lịch bảo trì phòng Studio',N'Đã lập lịch kiểm tra điện và thiết bị. Kiểm tra lịch địa điểm trước khi duyệt thuê.',N'rental',0),
 ('manager',N'Có yêu cầu hỗ trợ mới',N'Khách hàng đề nghị kiểm tra tiến độ hoàn cọc thuê.',N'support',0)
) x(AccountCode,Title,Body,Kind,IsRead) JOIN #SeedAccount a ON a.Code=x.AccountCode;

INSERT dbo.NhatKyHeThong (TaiKhoanId,HanhDong,DoiTuong,DoiTuongId,ThoiGian,DuLieuThayDoi)
SELECT @RentalManager,N'Cập nhật hỗ trợ',N'KhieuNai',Id,DATEADD(hour,-1,@RentalNow),(SELECT COALESCE(KetQuaXuLy,NoiDung) AS note FOR JSON PATH,WITHOUT_ARRAY_WRAPPER)
FROM dbo.KhieuNai WHERE YeuCauThueId IN (SELECT Id FROM #SeedRequest);
INSERT dbo.NhatKyHeThong (TaiKhoanId,HanhDong,DoiTuong,DoiTuongId,ThoiGian,DuLieuThayDoi)
SELECT @RentalAdmin,N'Xác nhận hợp đồng phí',N'HopDong',Id,DATEADD(day,-30,@RentalNow),N'{"note":"Đã kiểm tra biểu phí 5%, thông tin BTC và kỳ đối soát."}' FROM #SeedContract;
