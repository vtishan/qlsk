-- Demo catalog only: append-only, executed inside the demo migration transaction.
-- All identifiers are captured from IDENTITY; existing application rows are untouched.
DECLARE @CatalogNow datetime2(3) = SYSUTCDATETIME();
DECLARE @CatalogAdmin int = (SELECT Id FROM #SeedAccount WHERE Code = 'admin');
CREATE TABLE #SeedOrganizer(Code varchar(30) PRIMARY KEY, Id int NOT NULL);
CREATE TABLE #SeedCategory(Code varchar(30) PRIMARY KEY, Id int NOT NULL);
CREATE TABLE #SeedVenue(Code varchar(30) PRIMARY KEY, Id int NOT NULL);
CREATE TABLE #SeedArea(Code varchar(30) PRIMARY KEY, Id int NOT NULL);
CREATE TABLE #SeedSeat(Code varchar(30) PRIMARY KEY, Id int NOT NULL);
CREATE TABLE #SeedEvent(Code varchar(30) PRIMARY KEY, Id int NOT NULL);
CREATE TABLE #SeedVersion(Code varchar(30) PRIMARY KEY, Id int NOT NULL);
CREATE TABLE #SeedSession(Code varchar(30) PRIMARY KEY, Id int NOT NULL);

DECLARE @CatalogProfiles TABLE(Code varchar(30), AccountCode varchar(30), Name nvarchar(200), State nvarchar(32), Note nvarchar(500));
INSERT @CatalogProfiles VALUES
('organizer','organizer',N'Sài Gòn Live',N'DaDuyet',NULL),
('other-organizer','other-organizer',N'Không Gian Sáng Tạo Việt',N'DaDuyet',NULL),
('needs-info','pending-organizer',N'Bình Minh – hồ sơ trước khi bổ sung',N'CanBoSung',N'Vui lòng bổ sung địa chỉ trụ sở và giấy ủy quyền.'),
('pending','pending-organizer',N'Công ty Sự kiện Bình Minh',N'ChoDuyet',NULL),
('rejected','customer',N'Nhóm nghệ thuật Tháng Mười',N'TuChoi',N'Thông tin đại diện chưa thống nhất; có thể gửi hồ sơ mới.');
DECLARE @CatalogCode varchar(30), @CatalogAccount varchar(30), @CatalogName nvarchar(200), @CatalogState nvarchar(32), @CatalogNote nvarchar(500), @CatalogId int;
DECLARE CatalogProfiles CURSOR LOCAL FAST_FORWARD FOR SELECT Code,AccountCode,Name,State,Note FROM @CatalogProfiles;
OPEN CatalogProfiles;
FETCH NEXT FROM CatalogProfiles INTO @CatalogCode,@CatalogAccount,@CatalogName,@CatalogState,@CatalogNote;
WHILE @@FETCH_STATUS = 0
BEGIN
 INSERT dbo.HoSoBTC(TaiKhoanId,TenToChuc,NguoiDaiDien,MaSoThue,DiaChi,ThongTinLienHe,ThongTinNhanTienMaHoa,TrangThai,LyDoTuChoi,NgayNop,HanDuyet,NguoiDuyetId,NgayDuyet)
 SELECT Id,@CatalogName,N'Nguyễn Minh Anh',N'0312345678',N'18 Nguyễn Huệ, Quận 1, TP. Hồ Chí Minh',N'demo@qlsk.example · 0901234567',NULL,@CatalogState,@CatalogNote,DATEADD(day,-12,@CatalogNow),DATEADD(day,-2,@CatalogNow),CASE WHEN @CatalogState = N'ChoDuyet' THEN NULL ELSE @CatalogAdmin END,CASE WHEN @CatalogState = N'ChoDuyet' THEN NULL ELSE DATEADD(day,-3,@CatalogNow) END FROM #SeedAccount WHERE Code=@CatalogAccount;
 SET @CatalogId=CONVERT(int,SCOPE_IDENTITY());
 INSERT #SeedOrganizer VALUES(@CatalogCode,@CatalogId);
 FETCH NEXT FROM CatalogProfiles INTO @CatalogCode,@CatalogAccount,@CatalogName,@CatalogState,@CatalogNote;
END;
CLOSE CatalogProfiles; DEALLOCATE CatalogProfiles;

DECLARE @CatalogCategories TABLE(Code varchar(30),Name nvarchar(120),State nvarchar(32));
INSERT @CatalogCategories VALUES ('music',N'Âm nhạc',N'Hien'),('art',N'Nghệ thuật',N'Hien'),('workshop',N'Workshop',N'Hien'),('conference',N'Hội nghị',N'Hien'),('sport',N'Thể thao',N'Hien'),('hidden',N'Danh mục lưu trữ demo',N'An');
DECLARE CatalogCategories CURSOR LOCAL FAST_FORWARD FOR SELECT Code,Name,State FROM @CatalogCategories;
OPEN CatalogCategories; FETCH NEXT FROM CatalogCategories INTO @CatalogCode,@CatalogName,@CatalogState;
WHILE @@FETCH_STATUS=0
BEGIN
 SET @CatalogId=NULL;
 SELECT TOP(1) @CatalogId=Id FROM dbo.DanhMucSuKien WHERE TenDanhMuc=@CatalogName AND TrangThai=@CatalogState ORDER BY Id;
 IF @CatalogId IS NULL
 BEGIN
  INSERT dbo.DanhMucSuKien(TenDanhMuc,MoTa,TrangThai) VALUES(@CatalogName,N'Danh mục sự kiện mẫu để khám phá và quản trị.',@CatalogState);
  SET @CatalogId=CONVERT(int,SCOPE_IDENTITY());
 END;
 INSERT #SeedCategory VALUES(@CatalogCode,@CatalogId);
 FETCH NEXT FROM CatalogCategories INTO @CatalogCode,@CatalogName,@CatalogState;
END;
CLOSE CatalogCategories; DEALLOCATE CatalogCategories;

DECLARE @CatalogVenues TABLE(Code varchar(30),Name nvarchar(200),Address nvarchar(300),Capacity int,Price decimal(19,0),Facilities nvarchar(max),State nvarchar(32));
INSERT @CatalogVenues VALUES
('main',N'Nhà hát Ánh Sao – Demo',N'7 Lê Duẩn, Quận 1, TP. Hồ Chí Minh',96,12000000,N'Âm thanh, ánh sáng, sân khấu, điều hòa, bãi đỗ xe, wifi',N'HoatDong'),
('garden',N'Vườn sự kiện Bờ Sông – Demo',N'12 Bạch Đằng, Hải Châu, Đà Nẵng',120,8000000,N'Ngoài trời, sân khấu, bãi đỗ xe, wifi',N'HoatDong'),
('studio',N'Không gian sáng tạo Hồ Tây – Demo',N'45 Trích Sài, Tây Hồ, Hà Nội',40,3000000,N'Máy chiếu, điều hòa, wifi, phòng họp',N'HoatDong'),
('paused',N'Hội trường Bình Minh – Bảo trì demo',N'20 Nguyễn Văn Cừ, TP. Cần Thơ',80,4000000,N'Âm thanh, máy chiếu',N'TamDung'),
('hidden',N'Studio lưu trữ – Demo',N'5 Pasteur, Quận 3, TP. Hồ Chí Minh',30,2000000,N'Phòng họp',N'DaAn');
DECLARE @CatalogAddress nvarchar(300),@CatalogCapacity int,@CatalogPrice decimal(19,0),@CatalogFacilities nvarchar(max);
DECLARE CatalogVenues CURSOR LOCAL FAST_FORWARD FOR SELECT Code,Name,Address,Capacity,Price,Facilities,State FROM @CatalogVenues;
OPEN CatalogVenues; FETCH NEXT FROM CatalogVenues INTO @CatalogCode,@CatalogName,@CatalogAddress,@CatalogCapacity,@CatalogPrice,@CatalogFacilities,@CatalogState;
WHILE @@FETCH_STATUS=0
BEGIN
 INSERT dbo.DiaDiem(TenDiaDiem,DiaChi,SucChua,GiaThue,TienIch,TrangThai) VALUES(@CatalogName,@CatalogAddress,@CatalogCapacity,@CatalogPrice,@CatalogFacilities,@CatalogState);
 INSERT #SeedVenue VALUES(@CatalogCode,CONVERT(int,SCOPE_IDENTITY()));
 FETCH NEXT FROM CatalogVenues INTO @CatalogCode,@CatalogName,@CatalogAddress,@CatalogCapacity,@CatalogPrice,@CatalogFacilities,@CatalogState;
END;
CLOSE CatalogVenues; DEALLOCATE CatalogVenues;
DECLARE @CatalogAreas TABLE(Code varchar(30),VenueCode varchar(30),Name nvarchar(120),Capacity int);
INSERT @CatalogAreas VALUES ('main-vip','main',N'VIP',24),('main-standard','main',N'Tiêu chuẩn',48),('main-balcony','main',N'Ban công',24),('garden-standard','garden',N'Sân vườn',120),('studio-standard','studio',N'Phòng workshop',40),('paused-standard','paused',N'Hội trường',80),('hidden-standard','hidden',N'Studio',30);
DECLARE @CatalogVenueCode varchar(30),@CatalogAreaId int,@CatalogSeatNo int,@CatalogClusterId int;
DECLARE CatalogAreas CURSOR LOCAL FAST_FORWARD FOR SELECT Code,VenueCode,Name,Capacity FROM @CatalogAreas;
OPEN CatalogAreas; FETCH NEXT FROM CatalogAreas INTO @CatalogCode,@CatalogVenueCode,@CatalogName,@CatalogCapacity;
WHILE @@FETCH_STATUS=0
BEGIN
 INSERT dbo.KhuVuc(DiaDiemId,TenKhuVuc,SucChua) SELECT Id,@CatalogName,@CatalogCapacity FROM #SeedVenue WHERE Code=@CatalogVenueCode;
 SET @CatalogAreaId=CONVERT(int,SCOPE_IDENTITY()); INSERT #SeedArea VALUES(@CatalogCode,@CatalogAreaId);
 IF @CatalogVenueCode IN('main','garden','studio')
 BEGIN
  SET @CatalogClusterId=NULL;
  IF @CatalogCode='main-balcony'
  BEGIN
   INSERT dbo.CumGhe(KhuVucId,TenCum,LoaiCum,SoCho) VALUES(@CatalogAreaId,N'Cặp ghế B01',N'GheDoi',2);
   SET @CatalogClusterId=CONVERT(int,SCOPE_IDENTITY());
  END;
  SET @CatalogSeatNo=1;
  WHILE @CatalogSeatNo<=@CatalogCapacity
  BEGIN
   INSERT dbo.Ghe(KhuVucId,CumGheId,MaGhe,LoaiGhe,TrangThai) VALUES(@CatalogAreaId,CASE WHEN @CatalogSeatNo<=2 THEN @CatalogClusterId ELSE NULL END,CONCAT(N'A',RIGHT(CONCAT('00',@CatalogSeatNo),CASE WHEN @CatalogCapacity>99 THEN 3 ELSE 2 END)),CASE WHEN @CatalogClusterId IS NOT NULL AND @CatalogSeatNo<=2 THEN N'GheDoi' ELSE N'GheDon' END,N'SuDung');
   INSERT #SeedSeat VALUES(CONCAT(@CatalogCode,'-',RIGHT(CONCAT('00',@CatalogSeatNo),CASE WHEN @CatalogCapacity>99 THEN 3 ELSE 2 END)),CONVERT(int,SCOPE_IDENTITY()));
   SET @CatalogSeatNo+=1;
  END;
 END;
 FETCH NEXT FROM CatalogAreas INTO @CatalogCode,@CatalogVenueCode,@CatalogName,@CatalogCapacity;
END;
CLOSE CatalogAreas; DEALLOCATE CatalogAreas;

DECLARE @CatalogEvents TABLE(Code varchar(30),Name nvarchar(200),CategoryCode varchar(30),VenueCode varchar(30),State nvarchar(32),VersionState nvarchar(32),SessionState nvarchar(32),StartHours int,Image nvarchar(500),Published bit);
INSERT @CatalogEvents VALUES
('live',N'Đêm nhạc Sài Gòn – Những thanh âm kết nối','music','main',N'DangMoBan',N'DaDuyet',N'DangMoBan',1,N'/assets/images/music.webp',1),
('upcoming',N'Hòa nhạc Ánh Sao – Mùa của những ước mơ','music','main',N'DangMoBan',N'DaDuyet',N'DangMoBan',240,N'/assets/images/concert.webp',1),
('completed',N'Giai điệu mùa thu – Đêm nhạc đã kết thúc','music','main',N'HoanTat',N'DaDuyet',N'HoanTat',-336,N'/assets/images/theatre.webp',1),
('postponed',N'Nhịp điệu thành phố – Lịch diễn mới','art','main',N'Hoan',N'DaDuyet',N'Hoan',504,N'/assets/images/dance.webp',1),
('cancelled',N'Ngày hội âm nhạc – Thông báo hủy','music','main',N'DaHuy',N'DaDuyet',N'DaHuy',336,N'/assets/images/dj.webp',1),
('draft',N'Workshop kể chuyện bằng màu sắc – Bản nháp','workshop','main',N'Nhap',N'Nhap',N'Nhap',720,N'/assets/images/workshop.webp',0),
('pending',N'Diễn đàn sáng tạo Việt – Chờ xét duyệt','conference','main',N'ChoDuyet',N'ChoDuyet',N'Nhap',840,N'/assets/images/conference.webp',0),
('approved-private',N'Triển lãm Chạm vào ký ức – Chờ công bố','art','main',N'DaDuyet',N'DaDuyet',N'Nhap',960,N'/assets/images/art.webp',0),
('rejected',N'Chương trình nghệ thuật thử nghiệm – Cần chỉnh sửa','art','main',N'TuChoi',N'TuChoi',N'Nhap',1080,N'/assets/images/art.webp',0),
('suspended',N'Lễ hội nhịp sống trẻ – Tạm đình chỉ','music','main',N'DinhChi',N'DaDuyet',N'DongBan',1200,N'/assets/images/dj.webp',1),
('postpone-request',N'Giao lưu nghệ thuật – Đề nghị đổi lịch','art','main',N'DangMoBan',N'DaDuyet',N'DangMoBan',384,N'/assets/images/theatre.webp',1),
('cancel-request',N'Đêm nhạc ngoài trời – Đề nghị hủy','music','main',N'DangMoBan',N'DaDuyet',N'DangMoBan',432,N'/assets/images/music.webp',1),
('hanoi',N'Workshop sáng tạo bên Hồ Tây','workshop','studio',N'DangMoBan',N'DaDuyet',N'DangMoBan',288,N'/assets/images/workshop.webp',1),
('other',N'Ngày hội xanh bên sông Hàn','workshop','garden',N'DangMoBan',N'DaDuyet',N'DangMoBan',360,N'/assets/images/workshop.webp',1);
DECLARE @CatalogCategoryCode varchar(30),@CatalogVersionState nvarchar(32),@CatalogSessionState nvarchar(32),@CatalogStartHours int,@CatalogImage nvarchar(500),@CatalogPublished bit;
DECLARE @CatalogEventId int,@CatalogVersionId int,@CatalogSessionId int,@CatalogOwnerId int,@CatalogCategoryId int,@CatalogVenueId int,@CatalogContent nvarchar(max),@CatalogSnapshot nvarchar(max),@CatalogStart datetime2(3),@CatalogDescription nvarchar(max);
DECLARE CatalogEvents CURSOR LOCAL FAST_FORWARD FOR SELECT Code,Name,CategoryCode,VenueCode,State,VersionState,SessionState,StartHours,Image,Published FROM @CatalogEvents;
OPEN CatalogEvents; FETCH NEXT FROM CatalogEvents INTO @CatalogCode,@CatalogName,@CatalogCategoryCode,@CatalogVenueCode,@CatalogState,@CatalogVersionState,@CatalogSessionState,@CatalogStartHours,@CatalogImage,@CatalogPublished;
WHILE @@FETCH_STATUS=0
BEGIN
 SELECT @CatalogOwnerId=Id FROM #SeedOrganizer WHERE Code=CASE WHEN @CatalogCode='other' THEN 'other-organizer' ELSE 'organizer' END;
 SELECT @CatalogCategoryId=Id FROM #SeedCategory WHERE Code=@CatalogCategoryCode;
 SELECT @CatalogVenueId=Id FROM #SeedVenue WHERE Code=@CatalogVenueCode;
 SET @CatalogDescription=N'Cùng gặp gỡ những người yêu nghệ thuật trong không gian gần gũi, khám phá các tiết mục được chuẩn bị riêng và lưu giữ những khoảnh khắc đáng nhớ. Dữ liệu minh họa cho hệ thống quản lý sự kiện.';
 INSERT dbo.SuKien(BTCId,DanhMucId,TenSuKien,MoTa,AnhBia,TrangThai,NgayTao,PhienBanCongKhaiId,ChoPhepTuHuyVe,HanHoanKhiHoan)
 VALUES(@CatalogOwnerId,@CatalogCategoryId,@CatalogName,@CatalogDescription,@CatalogImage,@CatalogState,DATEADD(day,-45,@CatalogNow),NULL,1,CASE WHEN @CatalogCode='postponed' THEN DATEADD(day,7,@CatalogNow) ELSE NULL END);
 SET @CatalogEventId=CONVERT(int,SCOPE_IDENTITY()); INSERT #SeedEvent VALUES(@CatalogCode,@CatalogEventId);
 SET @CatalogContent=(SELECT @CatalogName AS [name],@CatalogCategoryId AS categoryId,@CatalogDescription AS [description],N'18:00 Đón khách · 19:00 Khai mạc · 19:15 Chương trình chính · 21:00 Giao lưu và kết thúc' AS program,N'Ban nhạc Mộc, ca sĩ Minh Hà và các nghệ sĩ khách mời' AS participants,N'Xuất trình mã QR khi vào cửa. Không mang chất dễ cháy và đồ uống có cồn. Trẻ em đi cùng người lớn.' AS rules,N'Vé chỉ có giá trị cho suất đã chọn. Yêu cầu hoàn vé được xử lý theo chính sách công bố.' AS policy,@CatalogImage AS [image],CAST(1 AS bit) AS allowRefund FOR JSON PATH,WITHOUT_ARRAY_WRAPPER);
 INSERT dbo.PhienBanSuKien(SuKienId,SoPhienBan,LoaiYeuCau,NoiDung,LyDo,TrangThaiDuyet,NguoiGuiId,NgayGui,NguoiDuyetId,NgayDuyet,KetQuaDuyet)
 SELECT @CatalogEventId,1,N'TaoMoi',@CatalogContent,N'Đề nghị xem xét chương trình và phương án tổ chức.',@CatalogVersionState,a.Id,CASE WHEN @CatalogVersionState=N'Nhap' THEN NULL ELSE DATEADD(day,-30,@CatalogNow) END,CASE WHEN @CatalogVersionState IN(N'DaDuyet',N'TuChoi') THEN @CatalogAdmin ELSE NULL END,CASE WHEN @CatalogVersionState IN(N'DaDuyet',N'TuChoi') THEN DATEADD(day,-28,@CatalogNow) ELSE NULL END,CASE WHEN @CatalogVersionState=N'TuChoi' THEN N'Bổ sung kế hoạch an toàn và nội dung chương trình chi tiết.' WHEN @CatalogVersionState=N'DaDuyet' THEN N'Hồ sơ đầy đủ, chấp thuận phương án tổ chức.' ELSE NULL END FROM #SeedAccount a WHERE a.Code=CASE WHEN @CatalogCode='other' THEN 'other-organizer' ELSE 'organizer' END;
 SET @CatalogVersionId=CONVERT(int,SCOPE_IDENTITY()); INSERT #SeedVersion VALUES(@CatalogCode,@CatalogVersionId);
 IF @CatalogPublished=1 UPDATE dbo.SuKien SET PhienBanCongKhaiId=@CatalogVersionId WHERE Id=@CatalogEventId;
 SET @CatalogStart=DATEADD(hour,@CatalogStartHours,@CatalogNow);
 SET @CatalogSnapshot=(SELECT CONCAT(N'Suất chính · ',@CatalogName) AS [name],CONVERT(varchar(12),@CatalogVersionId) AS versionId,JSON_QUERY((SELECT Id,TenKhuVuc AS [name],SucChua AS capacity FROM dbo.KhuVuc WHERE DiaDiemId=@CatalogVenueId FOR JSON PATH)) AS zones FOR JSON PATH,WITHOUT_ARRAY_WRAPPER);
 INSERT dbo.SuatDien(SuKienId,DiaDiemId,ThoiGianBatDau,ThoiGianKetThuc,ThoiGianMoBan,ThoiGianDongBan,SoDoChot,SucChua,TrangThai)
 SELECT @CatalogEventId,@CatalogVenueId,@CatalogStart,DATEADD(hour,3,@CatalogStart),DATEADD(day,-30,@CatalogStart),CASE WHEN @CatalogCode='live' THEN DATEADD(minute,30,@CatalogNow) ELSE DATEADD(hour,-1,@CatalogStart) END,@CatalogSnapshot,SucChua,@CatalogSessionState FROM dbo.DiaDiem WHERE Id=@CatalogVenueId;
 SET @CatalogSessionId=CONVERT(int,SCOPE_IDENTITY()); INSERT #SeedSession VALUES(@CatalogCode,@CatalogSessionId);
 IF @CatalogSessionState NOT IN(N'Nhap',N'DaHuy')
 INSERT dbo.LichDatDiaDiem(DiaDiemId,YeuCauThueId,SuatDienId,BatDau,KetThuc,LoaiLich,TrangThai) VALUES(@CatalogVenueId,NULL,@CatalogSessionId,@CatalogStart,DATEADD(hour,3,@CatalogStart),N'SuatDien',N'DaXacNhan');
 FETCH NEXT FROM CatalogEvents INTO @CatalogCode,@CatalogName,@CatalogCategoryCode,@CatalogVenueCode,@CatalogState,@CatalogVersionState,@CatalogSessionState,@CatalogStartHours,@CatalogImage,@CatalogPublished;
END;
CLOSE CatalogEvents; DEALLOCATE CatalogEvents;

-- A public event with a pending edit keeps its approved public version intact.
INSERT dbo.PhienBanSuKien(SuKienId,SoPhienBan,LoaiYeuCau,NoiDung,LyDo,TrangThaiDuyet,NguoiGuiId,NgayGui)
SELECT e.Id,2,N'CapNhat',JSON_MODIFY(v.NoiDung,'$.description',N'Bổ sung phần giao lưu với nghệ sĩ và hướng dẫn đến địa điểm.'),N'Bổ sung thông tin giao lưu sau chương trình.',N'ChoDuyet',a.Id,DATEADD(day,-1,@CatalogNow)
FROM #SeedEvent e JOIN #SeedVersion sv ON sv.Code=e.Code JOIN dbo.PhienBanSuKien v ON v.Id=sv.Id CROSS JOIN #SeedAccount a WHERE e.Code='upcoming' AND a.Code='organizer';
INSERT dbo.PhienBanSuKien(SuKienId,SoPhienBan,LoaiYeuCau,NoiDung,LyDo,TrangThaiDuyet,NguoiGuiId,NgayGui)
SELECT e.Id,2,CASE e.Code WHEN 'postpone-request' THEN N'Hoan' ELSE N'Huy' END,v.NoiDung,CASE e.Code WHEN 'postpone-request' THEN N'Đề nghị điều chỉnh lịch để bảo đảm điều kiện thời tiết.' ELSE N'Đề nghị xác nhận phương án hủy và hoàn vé.' END,N'ChoDuyet',a.Id,DATEADD(hour,-8,@CatalogNow)
FROM #SeedEvent e JOIN #SeedVersion sv ON sv.Code=e.Code JOIN dbo.PhienBanSuKien v ON v.Id=sv.Id CROSS JOIN #SeedAccount a WHERE e.Code IN('postpone-request','cancel-request') AND a.Code='organizer';

INSERT dbo.DanhGia(TaiKhoanId,SuKienId,SoSao,BinhLuan,NgayDanhGia,TrangThai)
SELECT a.Id,e.Id,5,N'Âm thanh tốt, hướng dẫn vào cửa rõ ràng. Mong được tham dự chương trình tiếp theo!',DATEADD(day,-13,@CatalogNow),N'CongKhai' FROM #SeedAccount a CROSS JOIN #SeedEvent e WHERE a.Code='customer' AND e.Code='completed';
INSERT dbo.DanhGia(TaiKhoanId,SuKienId,SoSao,BinhLuan,NgayDanhGia,TrangThai)
SELECT a.Id,e.Id,4,N'Chương trình hấp dẫn; khu vực gửi xe cần thêm biển chỉ dẫn.',DATEADD(day,-12,@CatalogNow),N'ChoDuyet' FROM #SeedAccount a CROSS JOIN #SeedEvent e WHERE a.Code='staff' AND e.Code='completed';
INSERT dbo.DanhGia(TaiKhoanId,SuKienId,SoSao,BinhLuan,NgayDanhGia,TrangThai)
SELECT a.Id,e.Id,2,N'Nội dung lặp lại, đánh giá mẫu đã được ẩn để kiểm tra kiểm duyệt.',DATEADD(day,-11,@CatalogNow),N'DaAn' FROM #SeedAccount a CROSS JOIN #SeedEvent e WHERE a.Code='other-organizer' AND e.Code='completed';




-- Application history appears in both the owner and platform review detail pages.
INSERT dbo.NhatKyHeThong(TaiKhoanId,HanhDong,DoiTuong,DoiTuongId,ThoiGian,DuLieuThayDoi)
SELECT h.TaiKhoanId,N'NopHoSo',N'HoSoBTC',h.Id,h.NgayNop,N'{"note":"Nộp hồ sơ tổ chức mẫu để kiểm tra quy trình xét duyệt."}'
FROM dbo.HoSoBTC h JOIN #SeedOrganizer s ON s.Id=h.Id;
INSERT dbo.NhatKyHeThong(TaiKhoanId,HanhDong,DoiTuong,DoiTuongId,ThoiGian,DuLieuThayDoi)
SELECT @CatalogAdmin,CASE h.TrangThai WHEN N'DaDuyet' THEN N'approve' WHEN N'CanBoSung' THEN N'needs_info' ELSE N'reject' END,N'HoSoBTC',h.Id,h.NgayDuyet,
 (SELECT COALESCE(h.LyDoTuChoi,N'Hồ sơ đầy đủ, chấp thuận quyền Ban tổ chức.') AS note FOR JSON PATH,WITHOUT_ARRAY_WRAPPER)
FROM dbo.HoSoBTC h JOIN #SeedOrganizer s ON s.Id=h.Id WHERE h.TrangThai<>N'ChoDuyet';
