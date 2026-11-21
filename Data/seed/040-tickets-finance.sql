-- Test-only ticket and financial history. Applied by DemoSeed inside its transaction.
-- Deliberately offline: DEMO-* provider references never contact a payment provider.
DECLARE @tfNow datetime2 = SYSUTCDATETIME();
DECLARE @tfCustomer int = (SELECT Id FROM #SeedAccount WHERE Code='customer');
DECLARE @tfStaff int = (SELECT Id FROM #SeedAccount WHERE Code='staff');
DECLARE @tfOrganizer int = (SELECT Id FROM #SeedAccount WHERE Code='organizer');
DECLARE @tfManager int = (SELECT Id FROM #SeedAccount WHERE Code='manager');
DECLARE @tfPolicy int = (SELECT Id FROM #SeedPolicy WHERE Code='ticketing');
IF @tfPolicy IS NULL SELECT TOP (1) @tfPolicy=Id FROM dbo.ChinhSach ORDER BY Id;
IF @tfPolicy IS NULL THROW 51040, 'Demo tickets require a seeded policy.', 1;

CREATE TABLE #SeedTicketType(Code varchar(60) PRIMARY KEY, Id int NOT NULL);
DECLARE @tfEvent varchar(30), @tfSession int, @tfArea int, @tfType int;
DECLARE tfTypes CURSOR LOCAL FAST_FORWARD FOR
 SELECT s.Code,s.Id,a.Id FROM #SeedSession s CROSS JOIN #SeedArea a
 WHERE a.Code=CASE s.Code WHEN 'hanoi' THEN 'studio-standard' WHEN 'other' THEN 'garden-standard' ELSE 'main-standard' END;
OPEN tfTypes;
FETCH NEXT FROM tfTypes INTO @tfEvent,@tfSession,@tfArea;
WHILE @@FETCH_STATUS=0
BEGIN
 INSERT dbo.LoaiVe(SuatDienId,KhuVucId,TenLoaiVe,NhomVe,GiaVe,SoLuong,QuyenLoi,BatDauBan,KetThucBan)
 SELECT @tfSession,@tfArea,N'Vé tiêu chuẩn',N'TieuChuan',350000,(SELECT SucChua FROM dbo.KhuVuc WHERE Id=@tfArea),N'Ghế ngồi đánh số · nước uống',ThoiGianMoBan,ThoiGianDongBan FROM dbo.SuatDien WHERE Id=@tfSession;
 SET @tfType=CONVERT(int,SCOPE_IDENTITY());
 INSERT #SeedTicketType VALUES(@tfEvent+'-standard',@tfType);
 IF @tfEvent NOT IN ('other','hanoi')
 BEGIN
 INSERT dbo.LoaiVe(SuatDienId,KhuVucId,TenLoaiVe,NhomVe,GiaVe,SoLuong,QuyenLoi,BatDauBan,KetThucBan)
 SELECT @tfSession,a.Id,N'VIP',N'VIP',650000,24,N'Khu vực gần sân khấu · quà lưu niệm',s.ThoiGianMoBan,s.ThoiGianDongBan FROM dbo.SuatDien s CROSS JOIN #SeedArea a WHERE s.Id=@tfSession AND a.Code='main-vip';
 INSERT #SeedTicketType VALUES(@tfEvent+'-vip',CONVERT(int,SCOPE_IDENTITY()));
 INSERT dbo.LoaiVe(SuatDienId,KhuVucId,TenLoaiVe,NhomVe,GiaVe,SoLuong,QuyenLoi,BatDauBan,KetThucBan)
 SELECT @tfSession,a.Id,N'Khách mời đối tác',N'VeMoi',0,24,N'Vé mời miễn phí, không chuyển nhượng',s.ThoiGianMoBan,s.ThoiGianDongBan FROM dbo.SuatDien s CROSS JOIN #SeedArea a WHERE s.Id=@tfSession AND a.Code='main-balcony';
 INSERT #SeedTicketType VALUES(@tfEvent+'-guest',CONVERT(int,SCOPE_IDENTITY()));
 END;
 FETCH NEXT FROM tfTypes INTO @tfEvent,@tfSession,@tfArea;
END;
CLOSE tfTypes; DEALLOCATE tfTypes;

-- Each order contains two seats. Pending/expired/cancelled orders have no issued tickets.
CREATE TABLE #SeedOrder(Code varchar(40) PRIMARY KEY,Id int NOT NULL,LineId int NULL,PaymentId int NULL);
DECLARE @tfOrders TABLE (N int PRIMARY KEY, Code varchar(40), EventCode varchar(30), State nvarchar(32), PaymentState nvarchar(32), AgeDays int);
INSERT @tfOrders VALUES
 (1,'live-paid','live',N'DaThanhToan',N'ThanhCong',2),
 (2,'live-pending','live',N'ChoThanhToan',N'ChoThanhToan',0),
 (3,'live-expired','live',N'HetHan',N'HetHan',1),
 (4,'live-cancelled','live',N'DaHuy',N'ThatBai',1),
 (5,'late-payment','upcoming',N'DaThuTienHetCho',N'ThuTrungCanHoan',1),
 (6,'refund-requested','upcoming',N'ChoHoanTien',N'ThanhCong',3),
 (7,'refund-partial','cancelled',N'HoanMotPhan',N'ThanhCong',5),
 (8,'refund-complete','cancelled',N'DaHoanTien',N'ThanhCong',6),
 (9,'completed-paid','completed',N'DaThanhToan',N'ThanhCong',20),
 (10,'upcoming-paid','upcoming',N'DaThanhToan',N'ThanhCong',1),
 (11,'postponed-paid','postponed',N'DaThanhToan',N'ThanhCong',5),
 (12,'payment-review','live',N'ChoThanhToan',N'NghiVan',1),
 (13,'refund-rejected','upcoming',N'DaThanhToan',N'ThanhCong',4),
 (14,'refund-approved','upcoming',N'ChoHoanTien',N'ThanhCong',4),
 (15,'refund-processing','upcoming',N'ChoHoanTien',N'ThanhCong',4),
 (16,'refund-failed','upcoming',N'ChoHoanTien',N'ThanhCong',4),
 (17,'completed-payout-pending','completed',N'DaThanhToan',N'ThanhCong',22),
 (18,'completed-history-24','completed',N'DaThanhToan',N'ThanhCong',24),
 (19,'completed-history-26','completed',N'DaThanhToan',N'ThanhCong',26),
 (20,'completed-history-28','completed',N'DaThanhToan',N'ThanhCong',28),
 (21,'completed-history-30','completed',N'DaThanhToan',N'ThanhCong',30);
DECLARE @tfBuyer int;
DECLARE @tfN int,@tfCode varchar(40),@tfState nvarchar(32),@tfPayState nvarchar(32),@tfAge int,@tfOrder int,@tfLine int,@tfPayment int,@tfCreated datetime2,@tfHold int,@tfSeat int,@tfI int,@tfTicketState nvarchar(32);
DECLARE tfOrders CURSOR LOCAL FAST_FORWARD FOR SELECT N,Code,EventCode,State,PaymentState,AgeDays FROM @tfOrders ORDER BY N;
OPEN tfOrders; FETCH NEXT FROM tfOrders INTO @tfN,@tfCode,@tfEvent,@tfState,@tfPayState,@tfAge;
WHILE @@FETCH_STATUS=0
BEGIN
 -- Spread historical cases across two consumer accounts; preserve purchase headroom per session.
 SET @tfBuyer=CASE WHEN @tfCode IN ('refund-approved','refund-processing','refund-failed','completed-history-26','completed-history-28','completed-history-30') THEN @tfStaff ELSE @tfCustomer END;
 SELECT @tfSession=Id FROM #SeedSession WHERE Code=@tfEvent;
 SELECT @tfType=Id FROM #SeedTicketType WHERE Code=@tfEvent+'-standard';
 SET @tfCreated=CASE WHEN @tfCode='live-pending' THEN DATEADD(minute,-2,@tfNow) ELSE DATEADD(day,-@tfAge,@tfNow) END;
 INSERT dbo.DonHang(TaiKhoanId,SuatDienId,HopDongId,ChinhSachId,TienVe,TienGiam,PhiDichVu,PhiNenTang,TongTien,TrangThai,NgayTao,HanThanhToan,KhoaTaoDon,DauVanTayTaoDon)
 SELECT @tfBuyer,@tfSession,c.Id,@tfPolicy,700000,0,10000,35000,710000,@tfState,@tfCreated,DATEADD(minute,15,@tfCreated),'DEMO-ORDER-'+@tfCode,NULL FROM #SeedContract c WHERE c.Code=@tfEvent;
 SET @tfOrder=CONVERT(int,SCOPE_IDENTITY());
 INSERT dbo.ChiTietDonHang(DonHangId,LoaiVeId,SoLuong,DonGia,ThanhTien) VALUES(@tfOrder,@tfType,2,350000,700000);
 SET @tfLine=CONVERT(int,SCOPE_IDENTITY());
 INSERT dbo.GiaoDichThanhToan(DonHangId,YeuCauThueId,MaDonNhaCungCap,MaGiaoDich,SoTienYeuCau,SoTienThucThu,PhuongThuc,TrangThai,NgayThanhToan,ChungTu)
 VALUES(@tfOrder,NULL,'DEMO-ORDER-'+@tfCode,CASE WHEN @tfPayState IN (N'ThanhCong',N'ThuTrungCanHoan',N'NghiVan') THEN 'DEMO-PAY-'+@tfCode END,710000,
 CASE WHEN @tfPayState IN (N'ThanhCong',N'ThuTrungCanHoan') THEN 710000 WHEN @tfPayState=N'NghiVan' THEN 700000 END,N'ChuyenKhoan',@tfPayState,
 CASE WHEN @tfPayState IN (N'ThanhCong',N'ThuTrungCanHoan',N'NghiVan') THEN DATEADD(minute,1,@tfCreated) END,N'Dữ liệu kiểm thử nội bộ; không có giao dịch ngân hàng thật.');
 SET @tfPayment=CONVERT(int,SCOPE_IDENTITY());
 INSERT #SeedOrder VALUES(@tfCode,@tfOrder,@tfLine,@tfPayment);
 SET @tfI=1;
 WHILE @tfI<=2
 BEGIN
  SELECT @tfSeat=Id FROM #SeedSeat WHERE Code='main-standard-'+RIGHT('0'+CONVERT(varchar(2),(@tfN-1)*2+@tfI),2);
  INSERT dbo.GiuGhe(SuatDienId,GheId,ChiTietDonHangId,HetHan,TrangThai) VALUES(@tfSession,@tfSeat,@tfLine,DATEADD(minute,15,@tfCreated),
   CASE WHEN @tfPayState=N'ThanhCong' THEN N'DaChot' WHEN @tfCode='live-pending' THEN N'DangGiu' WHEN @tfState=N'HetHan' THEN N'HetHan' ELSE N'DaNha' END);
  SET @tfHold=CONVERT(int,SCOPE_IDENTITY());
  IF @tfPayState=N'ThanhCong'
  BEGIN
   SET @tfTicketState=CASE WHEN @tfCode='refund-complete' OR (@tfCode='refund-partial' AND @tfI=1) THEN N'DaHuy'
    WHEN @tfEvent='completed' OR (@tfCode='live-paid' AND @tfI=1) THEN N'DaCheckIn' ELSE N'DaPhatHanh' END;
   INSERT dbo.Ve(ChiTietDonHangId,GiuGheId,LoaiVeId,SuatDienId,GheId,MaVe,MaQR,TrangThai,NgayPhatHanh,DaCheckInLuc)
   VALUES(@tfLine,@tfHold,@tfType,@tfSession,@tfSeat,'DEMO-'+@tfCode+'-'+CONVERT(varchar(1),@tfI),'DEMO-QR-'+@tfCode+'-'+CONVERT(varchar(1),@tfI),@tfTicketState,DATEADD(minute,1,@tfCreated),
    CASE WHEN @tfTicketState=N'DaCheckIn' THEN CASE WHEN @tfEvent='completed' THEN DATEADD(day,-14,@tfNow) ELSE DATEADD(minute,-10,@tfNow) END END);
  END;
  SET @tfI+=1;
 END;
 FETCH NEXT FROM tfOrders INTO @tfN,@tfCode,@tfEvent,@tfState,@tfPayState,@tfAge;
END;
CLOSE tfOrders; DEALLOCATE tfOrders;

INSERT dbo.NhatKyCheckIn(VeId,NhanVienId,ThoiGianQuet,KetQua,HinhThuc)
SELECT v.Id,@tfStaff,v.DaCheckInLuc,N'ThanhCong',CASE WHEN v.MaVe LIKE '%-2' THEN N'ThuCong' ELSE N'QuetQR' END FROM dbo.Ve v JOIN dbo.ChiTietDonHang l ON l.Id=v.ChiTietDonHangId JOIN #SeedOrder o ON o.Id=l.DonHangId WHERE v.TrangThai=N'DaCheckIn';

-- Order refunds always point to an order payment; rental-deposit refunds are seeded separately.
DECLARE @tfRefunds TABLE(Code varchar(40),State nvarchar(32),Amount decimal(19,2),TransferState nvarchar(32));
INSERT @tfRefunds VALUES ('refund-requested',N'ChoDuyet',350000,NULL),('refund-rejected',N'TuChoi',350000,NULL),('refund-approved',N'DaDuyet',350000,N'ChoChuyen'),('refund-processing',N'DangChuyen',350000,N'DangChuyen'),('refund-failed',N'ThatBai',350000,N'ThatBai'),('refund-partial',N'HoanTat',350000,N'ThanhCong'),('refund-complete',N'HoanTat',700000,N'ThanhCong'),('late-payment',N'ChoDuyet',710000,NULL);
DECLARE @tfAmount decimal(19,2),@tfTransfer nvarchar(32),@tfRefund int;
DECLARE tfRefunds CURSOR LOCAL FAST_FORWARD FOR SELECT Code,State,Amount,TransferState FROM @tfRefunds;
OPEN tfRefunds; FETCH NEXT FROM tfRefunds INTO @tfCode,@tfState,@tfAmount,@tfTransfer;
WHILE @@FETCH_STATUS=0
BEGIN
 SELECT @tfOrder=Id,@tfLine=LineId,@tfPayment=PaymentId FROM #SeedOrder WHERE Code=@tfCode;
 SELECT @tfBuyer=TaiKhoanId FROM dbo.DonHang WHERE Id=@tfOrder;
 INSERT dbo.YeuCauHoanTien(TaiKhoanId,GiaoDichId,VeId,KhoaChongLap,SoTienYeuCau,SoTienDuyet,LoaiYeuCau,LyDo,TrangThai,NgayYeuCau,NguoiDuyetId,NgayDuyet)
 SELECT @tfBuyer,@tfPayment,CASE WHEN @tfCode NOT IN ('late-payment','refund-complete') THEN (SELECT MIN(Id) FROM dbo.Ve WHERE ChiTietDonHangId=@tfLine) END,'DEMO-REFUND-'+@tfCode,@tfAmount,
 CASE WHEN @tfState NOT IN (N'ChoDuyet',N'TuChoi') THEN @tfAmount END,CASE WHEN @tfCode='late-payment' THEN N'Thu tiền sau giữ ghế' ELSE N'Tự hủy vé · xét điều khoản' END,
 N'Yêu cầu kiểm thử: thay đổi kế hoạch tham dự / sự kiện hủy; chờ đối chiếu điều khoản.',@tfState,DATEADD(hour,-4,@tfNow),CASE WHEN @tfState<>N'ChoDuyet' THEN @tfManager END,CASE WHEN @tfState<>N'ChoDuyet' THEN DATEADD(hour,-3,@tfNow) END;
 SET @tfRefund=CONVERT(int,SCOPE_IDENTITY());
 IF @tfTransfer IS NOT NULL
  INSERT dbo.GiaoDichHoanTien(YeuCauHoanTienId,KhoaChongLap,MaGiaoDichHoan,SoTien,TrangThai,NgayXuLy,ChungTu)
  VALUES(@tfRefund,'DEMO-TRANSFER-'+@tfCode,CASE WHEN @tfTransfer=N'ThanhCong' THEN 'DEMO-REFUND-PAY-'+@tfCode END,@tfAmount,@tfTransfer,CASE WHEN @tfTransfer IN (N'ThanhCong',N'ThatBai') THEN DATEADD(hour,-2,@tfNow) END,N'Chứng từ mô phỏng, không chuyển tiền thật.');
 INSERT dbo.NhatKyHeThong(TaiKhoanId,HanhDong,DoiTuong,DoiTuongId,ThoiGian,DuLieuThayDoi) VALUES(@tfBuyer,N'Gửi yêu cầu hoàn vé',N'YeuCauHoanTien',@tfRefund,DATEADD(hour,-4,@tfNow),N'{"note":"Đã tiếp nhận yêu cầu hoàn vé thử nghiệm."}');
 FETCH NEXT FROM tfRefunds INTO @tfCode,@tfState,@tfAmount,@tfTransfer;
END;
CLOSE tfRefunds; DEALLOCATE tfRefunds;

-- Three effective permissions give the staff account the complete admission workflow.
INSERT dbo.PhanCongSuKien(SuKienId,SuatDienId,TaiKhoanId,EmailMoi,NguoiPhanCongId,LoaiQuyen,NgayPhanCong,NgayThuHoi,TrangThai)
SELECT e.Id,s.Id,@tfStaff,NULL,@tfOrganizer,p.Permission,DATEADD(day,-3,@tfNow),NULL,N'DangHieuLuc'
FROM #SeedEvent e JOIN #SeedSession s ON s.Code=e.Code CROSS JOIN (VALUES(N'checkin.scan'),(N'checkin.manual'),(N'attendees.view')) p(Permission) WHERE e.Code IN ('live','completed','upcoming');
INSERT dbo.PhanCongSuKien(SuKienId,SuatDienId,TaiKhoanId,EmailMoi,NguoiPhanCongId,LoaiQuyen,NgayPhanCong,NgayThuHoi,TrangThai)
SELECT e.Id,NULL,NULL,'invited-staff@example.test',@tfOrganizer,N'attendees.view',DATEADD(day,-1,@tfNow),NULL,N'ChoNhan' FROM #SeedEvent e WHERE e.Code='live';
INSERT dbo.PhanCongSuKien(SuKienId,SuatDienId,TaiKhoanId,EmailMoi,NguoiPhanCongId,LoaiQuyen,NgayPhanCong,NgayThuHoi,TrangThai)
SELECT e.Id,NULL,@tfStaff,NULL,@tfOrganizer,N'attendees.view',DATEADD(day,-5,@tfNow),DATEADD(day,-4,@tfNow),N'DaThuHoi' FROM #SeedEvent e WHERE e.Code='live';

-- First invitation is claimable by the demo customer; links are generated on demand in the app.
DECLARE @tfGuest int=1,@tfVe int;
SELECT @tfSession=Id FROM #SeedSession WHERE Code='live';
SELECT @tfType=Id FROM #SeedTicketType WHERE Code='live-guest';
WHILE @tfGuest<=5
BEGIN
 SELECT @tfSeat=Id FROM #SeedSeat WHERE Code='main-balcony-'+RIGHT('0'+CONVERT(varchar(2),@tfGuest+2),2);
 INSERT dbo.Ve(ChiTietDonHangId,GiuGheId,LoaiVeId,SuatDienId,GheId,MaVe,MaQR,TrangThai,NgayPhatHanh,DaCheckInLuc)
 VALUES(NULL,NULL,@tfType,@tfSession,@tfSeat,'DEMO-GUEST-'+CONVERT(varchar(1),@tfGuest),'DEMO-QR-GUEST-'+CONVERT(varchar(1),@tfGuest),
 CASE WHEN @tfGuest=3 THEN N'DaCheckIn' WHEN @tfGuest IN (4,5) THEN N'DaThuHoi' ELSE N'DaPhatHanh' END,DATEADD(day,-1,@tfNow),CASE WHEN @tfGuest=3 THEN DATEADD(minute,-5,@tfNow) END);
 SET @tfVe=CONVERT(int,SCOPE_IDENTITY());
 INSERT dbo.VeMoi(VeId,HoTenKhachMoi,Email,SoDienThoai,TaiKhoanNhanId,MaBamNhanVe,HanNhanVe,DaGuiLuc,DaNhanLuc,TrangThai)
 VALUES(@tfVe,CASE @tfGuest WHEN 1 THEN N'Nguyễn Minh An · đối tác' WHEN 2 THEN N'Trần Ngọc Mai · báo chí' WHEN 3 THEN N'Lê Hải Nam · diễn giả' WHEN 4 THEN N'Phạm Thu Hà · đã thu hồi' ELSE N'Võ Thanh Bình · hết hạn' END,
 CASE WHEN @tfGuest IN (1,2) THEN 'customer@qlsk.test' ELSE 'guest-'+CONVERT(varchar(1),@tfGuest)+'@example.test' END,NULL,CASE WHEN @tfGuest=2 THEN @tfCustomer END,NULL,CASE WHEN @tfGuest=5 THEN DATEADD(hour,-1,@tfNow) ELSE DATEADD(hour,2,@tfNow) END,
 CASE WHEN @tfGuest<>1 THEN DATEADD(hour,-12,@tfNow) END,CASE WHEN @tfGuest IN (2,3) THEN DATEADD(hour,-6,@tfNow) END,
 CASE @tfGuest WHEN 1 THEN N'ChoGui' WHEN 2 THEN N'DaNhan' WHEN 3 THEN N'DaNhan' WHEN 4 THEN N'DaThuHoi' ELSE N'HetHan' END);
 IF @tfGuest=3 INSERT dbo.NhatKyCheckIn(VeId,NhanVienId,ThoiGianQuet,KetQua,HinhThuc) VALUES(@tfVe,@tfStaff,DATEADD(minute,-5,@tfNow),N'ThanhCong',N'ThuCong');
 SET @tfGuest+=1;
END;

INSERT dbo.Voucher(MaVoucher,SuKienId,SuatDienId,KhuVucId,LoaiGiam,GiaTri,MucGiamToiDa,GiaTriDonToiThieu,SoLuong,LuotMoiTaiKhoan,NguonTaiTro,NgayBatDau,NgayKetThuc,TrangThai)
SELECT 'DEMO-'+v.Code,e.Id,NULL,NULL,v.DiscountType,v.Value,v.Maximum,300000,50,2,N'BTC',DATEADD(day,-10,@tfNow),DATEADD(day,CASE WHEN v.State=N'HetHan' THEN -1 ELSE 30 END,@tfNow),v.State
FROM #SeedEvent e CROSS JOIN (VALUES
 ('WELCOME10',N'PhanTram',CONVERT(decimal(19,2),10),CONVERT(decimal(19,2),100000),N'HoatDong'),
 ('SAVE50',N'SoTien',50000,NULL,N'HoatDong'),('DRAFT',N'SoTien',30000,NULL,N'Nhap'),('REVIEW',N'SoTien',30000,NULL,N'ChoDuyet'),
 ('PAUSED',N'SoTien',30000,NULL,N'TamDung'),('REJECTED',N'SoTien',30000,NULL,N'TuChoi'),('EXPIRED',N'SoTien',30000,NULL,N'HetHan')) v(Code,DiscountType,Value,Maximum,State)
WHERE e.Code='upcoming';

-- Six historical receipts underpin six distinct settlement receipt days.
-- No receipt day is reused by another settlement.
DECLARE @tfCompleted int=(SELECT Id FROM #SeedEvent WHERE Code='completed');
DECLARE @tfSettlement int,@tfVersion int=1;
WHILE @tfVersion<=6
BEGIN
 SET @tfState=CASE @tfVersion WHEN 1 THEN N'ChoXacNhan' WHEN 2 THEN N'DaXacNhan' WHEN 3 THEN N'TranhChap' WHEN 4 THEN N'ChoGiaiNgan' WHEN 5 THEN N'ChoGiaiNgan' ELSE N'HoanTat' END;
 -- Non-overlapping periods for payout-approved examples honor the unique payout-period index.
 INSERT dbo.DoiSoat(SuKienId,PhienBan,TuNgay,DenNgay,TongDoanhThu,TongTienHoan,PhiNenTang,TienDieuChinh,TienBTCNhan,ChiTietDoiSoat,HanXacNhan,NguoiDuyetId,NgayDuyet,TrangThai)
 VALUES(@tfCompleted,@tfVersion,CONVERT(date,DATEADD(day,-20-2*(@tfVersion-1),DATEADD(hour,7,@tfNow))),CONVERT(date,DATEADD(day,-20-2*(@tfVersion-1),DATEADD(hour,7,@tfNow))),
 700000,0,35000,0,665000,(N'{"demo":true,"lockedAt":"'+CONVERT(nvarchar(33),@tfNow,127)+N'Z","closeDays":3,"responseHours":72,"basis":"Ticket revenue excludes service fees"}'),DATEADD(day,3,@tfNow),
 CASE WHEN @tfVersion>=4 THEN @tfManager END,CASE WHEN @tfVersion>=4 THEN DATEADD(hour,-2,@tfNow) END,@tfState);
 SET @tfSettlement=CONVERT(int,SCOPE_IDENTITY());
 IF @tfVersion>=5
 INSERT dbo.GiaiNgan(DoiSoatId,KhoaChongLap,MaGiaoDich,SoTien,TrangThai,NgayGiaiNgan,ChungTu)
 VALUES(@tfSettlement,'DEMO-PAYOUT-'+CONVERT(varchar(1),@tfVersion),CASE WHEN @tfVersion=6 THEN 'DEMO-PAYOUT-COMPLETED' END,665000,
 CASE WHEN @tfVersion=6 THEN N'ThanhCong' ELSE N'ChoChuyen' END,CASE WHEN @tfVersion=6 THEN DATEADD(hour,-1,@tfNow) END,N'Chứng từ mô phỏng; không chuyển tiền thật.');
 SET @tfVersion+=1;
END;


-- A consumed voucher is reflected in the order and payment totals.
UPDATE d SET TienGiam=50000,TongTien=660000 FROM dbo.DonHang d JOIN #SeedOrder o ON o.Id=d.Id WHERE o.Code='upcoming-paid';
UPDATE p SET SoTienYeuCau=660000,SoTienThucThu=660000 FROM dbo.GiaoDichThanhToan p JOIN #SeedOrder o ON o.PaymentId=p.Id WHERE o.Code='upcoming-paid';
INSERT dbo.SuDungVoucher(VoucherId,DonHangId,SoTienGiam,HetHanGiuLuot,DaChotLuc,TrangThai)
SELECT v.Id,o.Id,50000,DATEADD(day,-1,@tfNow),DATEADD(day,-1,@tfNow),N'DaSuDung' FROM dbo.Voucher v CROSS JOIN #SeedOrder o WHERE v.MaVoucher='DEMO-SAVE50' AND o.Code='upcoming-paid';

-- Filtered webhook bodies are local audit fixtures, never provider callbacks.
INSERT dbo.WebhookThanhToan(GiaoDichId,MaSuKienWebhook,NoiDungDaLoc,ThoiGianNhan,TrangThaiXuLy)
SELECT o.PaymentId,'DEMO-WEBHOOK-'+o.Code,N'{"demo":true,"source":"offline-fixture"}',DATEADD(hour,-1,@tfNow),
 CASE WHEN o.Code='payment-review' THEN N'NghiVan' ELSE N'DaXuLy' END FROM #SeedOrder o WHERE o.Code IN ('live-paid','payment-review');







