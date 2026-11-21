# 04 · Kiểm kê database

**VERIFIED từ mã nguồn.** T-SQL được sinh bằng `AppDbContext.Database.GenerateCreateScript()` với provider SQL Server. Có **37 bảng, 339 cột, 37 PK, 75 FK, 92 CHECK và 95 chỉ mục** tạo qua `CREATE INDEX`, trong đó 15 chỉ mục duy nhất có lọc. Ràng buộc UNIQUE trong CREATE TABLE được liệt kê riêng. [AppDbContext](../../Data/AppDbContext.cs#L55) · [QLSK.csproj](../../QLSK.csproj)

**UNKNOWN về database đang chạy.** Host hiện tại chỉ dùng middleware static files, không có connection string dành cho workspace. Kết quả bên dưới lấy từ DDL của EF Core. [Program.cs](../../Program.cs) · [Báo cáo SQL ngày 20/11/2026](../bc_01_sql.md)

Mọi PK là `Id int NOT NULL IDENTITY`. FK bỏ qua mệnh đề ON DELETE dùng mặc định NO ACTION. Cột ghi `NULL` là tùy chọn. Default trong phần chi tiết là default SQL, độc lập với giá trị khởi tạo trong C#. Module sử dụng là mapping nghiệp vụ dự kiến vì hiện chưa có API.

| Bảng | Vai trò | Module dự kiến | Entity và cấu hình |
|---|---|---|---|
| [TaiKhoan](#taikhoan) | Hồ sơ và xác thực | Tài khoản | [TaiKhoan.cs:7](../../Domain/Entities/TaiKhoan.cs#L7) · [taikhoanConfig.cs:12](../../Data/Configs/taikhoanConfig.cs#L12) |
| [HoSoBTC](#hosobtc) | Xét duyệt ban tổ chức | Ban tổ chức | [NhomTaiKhoan.cs:7](../../Domain/Entities/NhomTaiKhoan.cs#L7) · [CauHinhTaiKhoan.cs:10](../../Data/Configs/CauHinhTaiKhoan.cs#L10) |
| [GiayToBTC](#giaytobtc) | Tệp hồ sơ | Ban tổ chức | [NhomTaiKhoan.cs:26](../../Domain/Entities/NhomTaiKhoan.cs#L26) · [CauHinhTaiKhoan.cs:29](../../Data/Configs/CauHinhTaiKhoan.cs#L29) |
| [PhanCongSuKien](#phancongsukien) | Quyền theo sự kiện hoặc suất | Vận hành | [NhomTaiKhoan.cs:37](../../Domain/Entities/NhomTaiKhoan.cs#L37) · [CauHinhTaiKhoan.cs:37](../../Data/Configs/CauHinhTaiKhoan.cs#L37) |
| [SuKien](#sukien) | Sự kiện hiện hành | Trang chủ, lịch, vận hành | [NhomSuKien.cs:5](../../Domain/Entities/NhomSuKien.cs#L5) · [CauHinhSuKien.cs:10](../../Data/Configs/CauHinhSuKien.cs#L10) |
| [PhienBanSuKien](#phienbansukien) | Bản chỉnh sửa và phê duyệt | Trang chủ, lịch, vận hành | [NhomSuKien.cs:20](../../Domain/Entities/NhomSuKien.cs#L20) · [CauHinhSuKien.cs:28](../../Data/Configs/CauHinhSuKien.cs#L28) |
| [SuatDien](#suatdien) | Lịch suất diễn và sơ đồ chốt | Trang chủ, lịch, vận hành | [NhomSuKien.cs:36](../../Domain/Entities/NhomSuKien.cs#L36) · [CauHinhSuKien.cs:50](../../Data/Configs/CauHinhSuKien.cs#L50) |
| [DanhMucSuKien](#danhmucsukien) | Danh mục công khai | Trang chủ, lịch, vận hành | [NhomSuKien.cs:50](../../Domain/Entities/NhomSuKien.cs#L50) · [CauHinhSuKien.cs:71](../../Data/Configs/CauHinhSuKien.cs#L71) |
| [DiaDiem](#diadiem) | Danh mục địa điểm | Địa điểm, lịch | [NhomDiaDiem.cs:5](../../Domain/Entities/NhomDiaDiem.cs#L5) · [CauHinhDiaDiem.cs:10](../../Data/Configs/CauHinhDiaDiem.cs#L10) |
| [KhuVuc](#khuvuc) | Khu vực của địa điểm | Địa điểm, vé | [NhomDiaDiem.cs:16](../../Domain/Entities/NhomDiaDiem.cs#L16) · [CauHinhDiaDiem.cs:22](../../Data/Configs/CauHinhDiaDiem.cs#L22) |
| [Ghe](#ghe) | Ghế vật lý | Vé | [NhomDiaDiem.cs:24](../../Domain/Entities/NhomDiaDiem.cs#L24) · [CauHinhDiaDiem.cs:33](../../Data/Configs/CauHinhDiaDiem.cs#L33) |
| [CumGhe](#cumghe) | Cụm ghế | Vé | [NhomDiaDiem.cs:34](../../Domain/Entities/NhomDiaDiem.cs#L34) · [CauHinhDiaDiem.cs:51](../../Data/Configs/CauHinhDiaDiem.cs#L51) |
| [YeuCauThue](#yeucauthue) | Hồ sơ thuê | Yêu cầu | [NhomDiaDiem.cs:43](../../Domain/Entities/NhomDiaDiem.cs#L43) · [CauHinhDiaDiem.cs:64](../../Data/Configs/CauHinhDiaDiem.cs#L64) |
| [LichDatDiaDiem](#lichdatdiadiem) | Giữ lịch, suất và bảo trì | Lịch, yêu cầu | [NhomDiaDiem.cs:60](../../Domain/Entities/NhomDiaDiem.cs#L60) · [CauHinhDiaDiem.cs:88](../../Data/Configs/CauHinhDiaDiem.cs#L88) |
| [LoaiVe](#loaive) | Hạng và suất bán vé | Vé, vận hành | [NhomDatVe.cs:7](../../Domain/Entities/NhomDatVe.cs#L7) · [CauHinhDatVe.cs:10](../../Data/Configs/CauHinhDatVe.cs#L10) |
| [DonHang](#donhang) | Đơn hàng và giá đã chốt | Vé, vận hành | [NhomDatVe.cs:21](../../Domain/Entities/NhomDatVe.cs#L21) · [CauHinhDatVe.cs:27](../../Data/Configs/CauHinhDatVe.cs#L27) |
| [ChiTietDonHang](#chitietdonhang) | Số lượng theo hạng | Vé, vận hành | [NhomDatVe.cs:38](../../Domain/Entities/NhomDatVe.cs#L38) · [CauHinhDatVe.cs:48](../../Data/Configs/CauHinhDatVe.cs#L48) |
| [GiuGhe](#giughe) | Giữ một chỗ | Vé, vận hành | [NhomDatVe.cs:48](../../Domain/Entities/NhomDatVe.cs#L48) · [CauHinhDatVe.cs:59](../../Data/Configs/CauHinhDatVe.cs#L59) |
| [Ve](#ve) | Vé phát hành, QR và ghế | Vé, vận hành | [NhomDatVe.cs:58](../../Domain/Entities/NhomDatVe.cs#L58) · [CauHinhDatVe.cs:76](../../Data/Configs/CauHinhDatVe.cs#L76) |
| [VeMoi](#vemoi) | Vé mời và liên kết người nhận | Vé, vận hành | [NhomDatVe.cs:73](../../Domain/Entities/NhomDatVe.cs#L73) · [CauHinhDatVe.cs:108](../../Data/Configs/CauHinhDatVe.cs#L108) |
| [NhatKyCheckIn](#nhatkycheckin) | Lượt quét hoặc thủ công | Vé, vận hành | [NhomDatVe.cs:89](../../Domain/Entities/NhomDatVe.cs#L89) · [CauHinhDatVe.cs:127](../../Data/Configs/CauHinhDatVe.cs#L127) |
| [Voucher](#voucher) | Điều kiện ưu đãi | Vé, vận hành | [NhomDatVe.cs:99](../../Domain/Entities/NhomDatVe.cs#L99) · [CauHinhDatVe.cs:142](../../Data/Configs/CauHinhDatVe.cs#L142) |
| [SuDungVoucher](#sudungvoucher) | Giữ và chốt lượt voucher | Vé | [NhomTaiChinh.cs:5](../../Domain/Entities/NhomTaiChinh.cs#L5) · [CauHinhTaiChinh.cs:10](../../Data/Configs/CauHinhTaiChinh.cs#L10) |
| [GiaoDichThanhToan](#giaodichthanhtoan) | Đơn hoặc tiền cọc | Vé, yêu cầu | [NhomTaiChinh.cs:16](../../Domain/Entities/NhomTaiChinh.cs#L16) · [CauHinhTaiChinh.cs:28](../../Data/Configs/CauHinhTaiChinh.cs#L28) |
| [WebhookThanhToan](#webhookthanhtoan) | Thông điệp thanh toán đã lọc | Vé, yêu cầu | [NhomTaiChinh.cs:31](../../Domain/Entities/NhomTaiChinh.cs#L31) · [CauHinhTaiChinh.cs:60](../../Data/Configs/CauHinhTaiChinh.cs#L60) |
| [YeuCauHoanTien](#yeucauhoantien) | Yêu cầu hoàn theo giao dịch | Yêu cầu | [NhomTaiChinh.cs:41](../../Domain/Entities/NhomTaiChinh.cs#L41) · [CauHinhTaiChinh.cs:74](../../Data/Configs/CauHinhTaiChinh.cs#L74) |
| [GiaoDichHoanTien](#giaodichhoantien) | Lệnh hoàn và chứng từ | Yêu cầu | [NhomTaiChinh.cs:58](../../Domain/Entities/NhomTaiChinh.cs#L58) · [CauHinhTaiChinh.cs:95](../../Data/Configs/CauHinhTaiChinh.cs#L95) |
| [HopDong](#hopdong) | Phí dịch vụ đã thỏa thuận | Vận hành, báo cáo | [NhomTaiChinh.cs:70](../../Domain/Entities/NhomTaiChinh.cs#L70) · [CauHinhTaiChinh.cs:115](../../Data/Configs/CauHinhTaiChinh.cs#L115) |
| [DoiSoat](#doisoat) | Phiên bản đối soát | Vận hành, báo cáo | [NhomTaiChinh.cs:85](../../Domain/Entities/NhomTaiChinh.cs#L85) · [CauHinhTaiChinh.cs:134](../../Data/Configs/CauHinhTaiChinh.cs#L134) |
| [GiaiNgan](#giaingan) | Lệnh giải ngân | Vận hành, báo cáo | [NhomTaiChinh.cs:104](../../Domain/Entities/NhomTaiChinh.cs#L104) · [CauHinhTaiChinh.cs:155](../../Data/Configs/CauHinhTaiChinh.cs#L155) |
| [ThongBao](#thongbao) | Hộp thư và kênh gửi | Tài khoản | [NhomQuanTri.cs:5](../../Domain/Entities/NhomQuanTri.cs#L5) · [CauHinhQuanTri.cs:10](../../Data/Configs/CauHinhQuanTri.cs#L10) |
| [KhieuNai](#khieunai) | Khiếu nại | Yêu cầu, vận hành | [NhomQuanTri.cs:20](../../Domain/Entities/NhomQuanTri.cs#L20) · [CauHinhQuanTri.cs:30](../../Data/Configs/CauHinhQuanTri.cs#L30) |
| [DanhGia](#danhgia) | Đánh giá | Trang chủ | [NhomQuanTri.cs:35](../../Domain/Entities/NhomQuanTri.cs#L35) · [CauHinhQuanTri.cs:49](../../Data/Configs/CauHinhQuanTri.cs#L49) |
| [Banner](#banner) | Banner công khai | Trang chủ | [NhomQuanTri.cs:46](../../Domain/Entities/NhomQuanTri.cs#L46) · [CauHinhQuanTri.cs:64](../../Data/Configs/CauHinhQuanTri.cs#L64) |
| [CauHinhHeThong](#cauhinhhethong) | Cấu hình nghiệp vụ | Vận hành | [NhomQuanTri.cs:56](../../Domain/Entities/NhomQuanTri.cs#L56) · [CauHinhQuanTri.cs:77](../../Data/Configs/CauHinhQuanTri.cs#L77) |
| [NhatKyHeThong](#nhatkyhethong) | Nhật ký thao tác | Vận hành, báo cáo | [NhomQuanTri.cs:66](../../Domain/Entities/NhomQuanTri.cs#L66) · [CauHinhQuanTri.cs:83](../../Data/Configs/CauHinhQuanTri.cs#L83) |
| [ChinhSach](#chinhsach) | Điều khoản theo phiên bản | Vé, yêu cầu | [NhomQuanTri.cs:77](../../Domain/Entities/NhomQuanTri.cs#L77) · [CauHinhQuanTri.cs:94](../../Data/Configs/CauHinhQuanTri.cs#L94) |

## Tài khoản và phân quyền

### TaiKhoan

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `Email` | `nvarchar(255)` | NULL | — |
| `SoDienThoai` | `nvarchar(15)` | NULL | — |
| `HoTen` | `nvarchar(100)` | NOT NULL | — |
| `TenDangNhap` | `nvarchar(50)` | NOT NULL | — |
| `MatKhauBam` | `nvarchar(500)` | NOT NULL | — |
| `VaiTro` | `int` | NOT NULL | `DEFAULT 1` |
| `TrangThai` | `nvarchar(32)` | NOT NULL | `DEFAULT N'HoatDong'` |
| `NgayTao` | `datetime2(3)` | NOT NULL | `DEFAULT (SYSUTCDATETIME())` |
| `EmailXacMinhLuc` | `datetime2(3)` | NULL | — |
| `SoDienThoaiXacMinhLuc` | `datetime2(3)` | NULL | — |
| `MaBamKhoiPhuc` | `nvarchar(128)` | NULL | — |
| `HanKhoiPhuc` | `datetime2(3)` | NULL | — |
| `PhienBanDangNhap` | `int` | NOT NULL | — |
| `QuyenQuanLy` | `nvarchar(max)` | NULL | — |

- `[PK_TaiKhoan] PRIMARY KEY ([Id])`
- `[CK_TaiKhoan_KhoiPhuc] CHECK (([MaBamKhoiPhuc] IS NULL AND [HanKhoiPhuc] IS NULL) OR ([MaBamKhoiPhuc] IS NOT NULL AND [HanKhoiPhuc] IS NOT NULL))`
- `[CK_TaiKhoan_LienHe] CHECK (([Email] IS NOT NULL OR [SoDienThoai] IS NOT NULL) AND ([Email] IS NULL OR TRIM([Email]) <> N'') AND ([SoDienThoai] IS NULL OR TRIM([SoDienThoai]) <> N''))`
- `[CK_TaiKhoan_LienHeXacMinh] CHECK (([EmailXacMinhLuc] IS NULL OR NULLIF(TRIM([Email]), N'') IS NOT NULL) AND ([SoDienThoaiXacMinhLuc] IS NULL OR NULLIF(TRIM([SoDienThoai]), N'') IS NOT NULL))`
- `[CK_TaiKhoan_QuyenQuanLy] CHECK ([QuyenQuanLy] IS NULL OR ISJSON([QuyenQuanLy], OBJECT) = 1)`
- `[CK_TaiKhoan_TrangThai] CHECK ([TrangThai] IN (N'HoatDong', N'DaKhoa', N'ChoXacMinh') AND [PhienBanDangNhap] >= 0)`
- `[CK_TaiKhoan_VaiTro] CHECK ([VaiTro] BETWEEN 1 AND 5)`
- UNIQUE `IX_TaiKhoan_Email` `([Email]) WHERE [Email] IS NOT NULL`
- UNIQUE `IX_TaiKhoan_SoDienThoai` `([SoDienThoai]) WHERE [SoDienThoai] IS NOT NULL`
- UNIQUE `IX_TaiKhoan_TenDangNhap` `([TenDangNhap])`

### HoSoBTC

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `TaiKhoanId` | `int` | NOT NULL | — |
| `TenToChuc` | `nvarchar(200)` | NOT NULL | — |
| `NguoiDaiDien` | `nvarchar(120)` | NOT NULL | — |
| `MaSoThue` | `nvarchar(20)` | NULL | — |
| `DiaChi` | `nvarchar(300)` | NOT NULL | — |
| `ThongTinLienHe` | `nvarchar(300)` | NOT NULL | — |
| `ThongTinNhanTienMaHoa` | `nvarchar(max)` | NULL | — |
| `TrangThai` | `nvarchar(32)` | NOT NULL | — |
| `LyDoTuChoi` | `nvarchar(max)` | NULL | — |
| `NgayNop` | `datetime2(3)` | NULL | — |
| `HanDuyet` | `datetime2(3)` | NULL | — |
| `NguoiDuyetId` | `int` | NULL | — |
| `NgayDuyet` | `datetime2(3)` | NULL | — |

- `[PK_HoSoBTC] PRIMARY KEY ([Id])`
- `[CK_HoSoBTC_DieuKien1] CHECK ([NgayNop] IS NULL OR [HanDuyet] IS NULL OR [NgayNop] <= [HanDuyet])`
- `[CK_HoSoBTC_TrangThai] CHECK ([TrangThai] IN (N'Nhap', N'ChoDuyet', N'CanBoSung', N'TuChoi', N'DaDuyet'))`
- `[FK_HoSoBTC_TaiKhoan_NguoiDuyetId] FOREIGN KEY ([NguoiDuyetId]) REFERENCES [TaiKhoan] ([Id])`
- `[FK_HoSoBTC_TaiKhoan_TaiKhoanId] FOREIGN KEY ([TaiKhoanId]) REFERENCES [TaiKhoan] ([Id])`
- INDEX `IX_HoSoBTC_NguoiDuyetId` `([NguoiDuyetId])`
- INDEX `IX_HoSoBTC_TaiKhoanId` `([TaiKhoanId])`

### GiayToBTC

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `HoSoBTCId` | `int` | NOT NULL | — |
| `LoaiGiayTo` | `nvarchar(32)` | NOT NULL | — |
| `KhoaLuuTru` | `nvarchar(500)` | NOT NULL | — |
| `TenTep` | `nvarchar(255)` | NOT NULL | — |
| `LoaiNoiDung` | `nvarchar(32)` | NOT NULL | — |
| `NgayTaiLen` | `datetime2(3)` | NOT NULL | — |

- `[PK_GiayToBTC] PRIMARY KEY ([Id])`
- `[FK_GiayToBTC_HoSoBTC_HoSoBTCId] FOREIGN KEY ([HoSoBTCId]) REFERENCES [HoSoBTC] ([Id])`
- INDEX `IX_GiayToBTC_HoSoBTCId` `([HoSoBTCId])`

### PhanCongSuKien

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `SuKienId` | `int` | NOT NULL | — |
| `SuatDienId` | `int` | NULL | — |
| `TaiKhoanId` | `int` | NULL | — |
| `EmailMoi` | `nvarchar(255)` | NULL | — |
| `MaBamLoiMoi` | `nvarchar(128)` | NULL | — |
| `HanLoiMoi` | `datetime2(3)` | NULL | — |
| `NguoiPhanCongId` | `int` | NOT NULL | — |
| `LoaiQuyen` | `nvarchar(64)` | NOT NULL | — |
| `NgayPhanCong` | `datetime2(3)` | NOT NULL | — |
| `NgayThuHoi` | `datetime2(3)` | NULL | — |
| `TrangThai` | `nvarchar(32)` | NOT NULL | — |

- `[PK_PhanCongSuKien] PRIMARY KEY ([Id])`
- `[CK_PhanCongSuKien_DieuKien1] CHECK ([TaiKhoanId] IS NOT NULL OR NULLIF(TRIM([EmailMoi]), N'') IS NOT NULL)`
- `[CK_PhanCongSuKien_DieuKien2] CHECK ([TrangThai] <> N'DangHieuLuc' OR ([TaiKhoanId] IS NOT NULL AND [NgayThuHoi] IS NULL))`
- `[CK_PhanCongSuKien_DieuKien3] CHECK ([NgayThuHoi] IS NULL OR [NgayThuHoi] >= [NgayPhanCong])`
- `[CK_PhanCongSuKien_TrangThai] CHECK ([TrangThai] IN (N'ChoNhan', N'DangHieuLuc', N'DaThuHoi', N'HetHan', N'TuChoi'))`
- `[FK_PhanCongSuKien_TaiKhoan_NguoiPhanCongId] FOREIGN KEY ([NguoiPhanCongId]) REFERENCES [TaiKhoan] ([Id])`
- `[FK_PhanCongSuKien_TaiKhoan_TaiKhoanId] FOREIGN KEY ([TaiKhoanId]) REFERENCES [TaiKhoan] ([Id])`
- `[FK_PhanCongSuKien_SuKien_SuKienId] FOREIGN KEY ([SuKienId]) REFERENCES [SuKien] ([Id])`
- `[FK_PhanCongSuKien_SuatDien_SuatDienId_SuKienId] FOREIGN KEY ([SuatDienId], [SuKienId]) REFERENCES [SuatDien] ([Id], [SuKienId])`
- INDEX `IX_PhanCongSuKien_NguoiPhanCongId` `([NguoiPhanCongId])`
- INDEX `IX_PhanCongSuKien_SuatDienId_SuKienId` `([SuatDienId], [SuKienId])`
- INDEX `IX_PhanCongSuKien_SuKienId` `([SuKienId])`
- INDEX `IX_PhanCongSuKien_TraQuyen` `([TaiKhoanId], [SuKienId], [SuatDienId], [LoaiQuyen], [TrangThai])`
- UNIQUE `IX_PhanCongSuKien_TheoSuat` `([TaiKhoanId], [SuKienId], [SuatDienId], [LoaiQuyen]) WHERE [TaiKhoanId] IS NOT NULL AND [SuatDienId] IS NOT NULL AND [TrangThai] = N'DangHieuLuc'`
- UNIQUE `IX_PhanCongSuKien_ToanSuKien` `([TaiKhoanId], [SuKienId], [LoaiQuyen]) WHERE [TaiKhoanId] IS NOT NULL AND [SuatDienId] IS NULL AND [TrangThai] = N'DangHieuLuc'`

## Quản lý sự kiện

### SuKien

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `BTCId` | `int` | NOT NULL | — |
| `DanhMucId` | `int` | NOT NULL | — |
| `TenSuKien` | `nvarchar(200)` | NOT NULL | — |
| `MoTa` | `nvarchar(max)` | NOT NULL | — |
| `AnhBia` | `nvarchar(500)` | NOT NULL | — |
| `TrangThai` | `nvarchar(32)` | NOT NULL | — |
| `NgayTao` | `datetime2(3)` | NOT NULL | — |
| `PhienBanCongKhaiId` | `int` | NULL | — |
| `ChoPhepTuHuyVe` | `bit` | NOT NULL | — |
| `HanHoanKhiHoan` | `datetime2(3)` | NULL | — |

- `[PK_SuKien] PRIMARY KEY ([Id])`
- `[CK_SuKien_TrangThai] CHECK ([TrangThai] IN (N'Nhap', N'ChoDuyet', N'DaDuyet', N'DangMoBan', N'Hoan', N'DinhChi', N'DaHuy', N'HoanTat', N'TuChoi'))`
- `[FK_SuKien_DanhMucSuKien_DanhMucId] FOREIGN KEY ([DanhMucId]) REFERENCES [DanhMucSuKien] ([Id])`
- `[FK_SuKien_HoSoBTC_BTCId] FOREIGN KEY ([BTCId]) REFERENCES [HoSoBTC] ([Id])`
- `[FK_SuKien_PhienBanSuKien_PhienBanCongKhaiId_Id] FOREIGN KEY ([PhienBanCongKhaiId], [Id]) REFERENCES [PhienBanSuKien] ([Id], [SuKienId])`
- INDEX `IX_SuKien_BTCId` `([BTCId])`
- INDEX `IX_SuKien_DanhMucId` `([DanhMucId])`
- INDEX `IX_SuKien_PhienBanCongKhaiId_Id` `([PhienBanCongKhaiId], [Id])`

### PhienBanSuKien

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `SuKienId` | `int` | NOT NULL | — |
| `SoPhienBan` | `int` | NOT NULL | — |
| `LoaiYeuCau` | `nvarchar(32)` | NOT NULL | — |
| `NoiDung` | `nvarchar(max)` | NOT NULL | — |
| `LyDo` | `nvarchar(max)` | NULL | — |
| `TrangThaiDuyet` | `nvarchar(32)` | NOT NULL | — |
| `NguoiGuiId` | `int` | NOT NULL | — |
| `NgayGui` | `datetime2(3)` | NULL | — |
| `NguoiDuyetId` | `int` | NULL | — |
| `NgayDuyet` | `datetime2(3)` | NULL | — |
| `KetQuaDuyet` | `nvarchar(max)` | NULL | — |

- `[PK_PhienBanSuKien] PRIMARY KEY ([Id])`
- `[AK_PhienBanSuKien_Id_SuKienId] UNIQUE ([Id], [SuKienId])`
- `[CK_PhienBanSuKien_DieuKien1] CHECK ([SoPhienBan] > 0)`
- `[CK_PhienBanSuKien_LoaiYeuCau] CHECK ([LoaiYeuCau] IN (N'TaoMoi', N'CapNhat', N'Hoan', N'Huy'))`
- `[CK_PhienBanSuKien_NoiDung] CHECK (ISJSON([NoiDung], OBJECT) = 1)`
- `[CK_PhienBanSuKien_TrangThaiDuyet] CHECK ([TrangThaiDuyet] IN (N'Nhap', N'ChoDuyet', N'DaDuyet', N'TuChoi'))`
- `[FK_PhienBanSuKien_TaiKhoan_NguoiDuyetId] FOREIGN KEY ([NguoiDuyetId]) REFERENCES [TaiKhoan] ([Id])`
- `[FK_PhienBanSuKien_TaiKhoan_NguoiGuiId] FOREIGN KEY ([NguoiGuiId]) REFERENCES [TaiKhoan] ([Id])`
- `[FK_PhienBanSuKien_SuKien_SuKienId] FOREIGN KEY ([SuKienId]) REFERENCES [SuKien] ([Id])`
- INDEX `IX_PhienBanSuKien_NguoiDuyetId` `([NguoiDuyetId])`
- INDEX `IX_PhienBanSuKien_NguoiGuiId` `([NguoiGuiId])`
- UNIQUE `IX_PhienBanSuKien_SoPhienBan` `([SuKienId], [SoPhienBan])`

### SuatDien

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `SuKienId` | `int` | NOT NULL | — |
| `DiaDiemId` | `int` | NOT NULL | — |
| `ThoiGianBatDau` | `datetime2(3)` | NOT NULL | — |
| `ThoiGianKetThuc` | `datetime2(3)` | NOT NULL | — |
| `ThoiGianMoBan` | `datetime2(3)` | NOT NULL | — |
| `ThoiGianDongBan` | `datetime2(3)` | NOT NULL | — |
| `SoDoChot` | `nvarchar(max)` | NOT NULL | — |
| `SucChua` | `int` | NOT NULL | — |
| `TrangThai` | `nvarchar(32)` | NOT NULL | — |

- `[PK_SuatDien] PRIMARY KEY ([Id])`
- `[AK_SuatDien_Id_DiaDiemId] UNIQUE ([Id], [DiaDiemId])`
- `[AK_SuatDien_Id_SuKienId] UNIQUE ([Id], [SuKienId])`
- `[CK_SuatDien_DieuKien1] CHECK ([SucChua] > 0)`
- `[CK_SuatDien_DieuKien2] CHECK ([ThoiGianBatDau] < [ThoiGianKetThuc])`
- `[CK_SuatDien_DieuKien3] CHECK ([ThoiGianMoBan] < [ThoiGianDongBan] AND [ThoiGianDongBan] <= [ThoiGianBatDau])`
- `[CK_SuatDien_SoDoChot] CHECK (ISJSON([SoDoChot], OBJECT) = 1)`
- `[CK_SuatDien_TrangThai] CHECK ([TrangThai] IN (N'Nhap', N'DaXacNhan', N'DangMoBan', N'DongBan', N'Hoan', N'DaHuy', N'HoanTat'))`
- `[FK_SuatDien_DiaDiem_DiaDiemId] FOREIGN KEY ([DiaDiemId]) REFERENCES [DiaDiem] ([Id])`
- `[FK_SuatDien_SuKien_SuKienId] FOREIGN KEY ([SuKienId]) REFERENCES [SuKien] ([Id])`
- INDEX `IX_SuatDien_DiaDiemId` `([DiaDiemId])`
- INDEX `IX_SuatDien_LichSuKien` `([SuKienId], [ThoiGianBatDau])`

### DanhMucSuKien

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `TenDanhMuc` | `nvarchar(120)` | NOT NULL | — |
| `MoTa` | `nvarchar(max)` | NULL | — |
| `TrangThai` | `nvarchar(32)` | NOT NULL | — |

- `[PK_DanhMucSuKien] PRIMARY KEY ([Id])`
- `[CK_DanhMucSuKien_TrangThai] CHECK ([TrangThai] IN (N'Hien', N'An'))`

## Địa điểm và ghế

### DiaDiem

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `TenDiaDiem` | `nvarchar(200)` | NOT NULL | — |
| `DiaChi` | `nvarchar(300)` | NOT NULL | — |
| `SucChua` | `int` | NOT NULL | — |
| `GiaThue` | `decimal(19,0)` | NOT NULL | — |
| `TienIch` | `nvarchar(max)` | NULL | — |
| `TrangThai` | `nvarchar(32)` | NOT NULL | — |

- `[PK_DiaDiem] PRIMARY KEY ([Id])`
- `[CK_DiaDiem_DieuKien1] CHECK ([SucChua] > 0 AND [GiaThue] >= 0)`
- `[CK_DiaDiem_TrangThai] CHECK ([TrangThai] IN (N'HoatDong', N'TamDung', N'DaAn'))`

### KhuVuc

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `DiaDiemId` | `int` | NOT NULL | — |
| `TenKhuVuc` | `nvarchar(120)` | NOT NULL | — |
| `SucChua` | `int` | NOT NULL | — |

- `[PK_KhuVuc] PRIMARY KEY ([Id])`
- `[CK_KhuVuc_DieuKien1] CHECK ([SucChua] > 0)`
- `[FK_KhuVuc_DiaDiem_DiaDiemId] FOREIGN KEY ([DiaDiemId]) REFERENCES [DiaDiem] ([Id])`
- UNIQUE `IX_KhuVuc_TenTheoDiaDiem` `([DiaDiemId], [TenKhuVuc])`

### Ghe

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `KhuVucId` | `int` | NOT NULL | — |
| `CumGheId` | `int` | NULL | — |
| `MaGhe` | `nvarchar(32)` | NOT NULL | — |
| `LoaiGhe` | `nvarchar(32)` | NOT NULL | — |
| `TrangThai` | `nvarchar(32)` | NOT NULL | — |

- `[PK_Ghe] PRIMARY KEY ([Id])`
- `[CK_Ghe_LoaiGhe] CHECK ([LoaiGhe] IN (N'GheDon', N'GheDoi', N'GheCum'))`
- `[CK_Ghe_TrangThai] CHECK ([TrangThai] IN (N'SuDung', N'BaoTri', N'NgungSuDung'))`
- `[FK_Ghe_CumGhe_CumGheId_KhuVucId] FOREIGN KEY ([CumGheId], [KhuVucId]) REFERENCES [CumGhe] ([Id], [KhuVucId])`
- `[FK_Ghe_KhuVuc_KhuVucId] FOREIGN KEY ([KhuVucId]) REFERENCES [KhuVuc] ([Id])`
- INDEX `IX_Ghe_CumGheId_KhuVucId` `([CumGheId], [KhuVucId])`
- UNIQUE `IX_Ghe_MaTheoKhuVuc` `([KhuVucId], [MaGhe])`

### CumGhe

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `KhuVucId` | `int` | NOT NULL | — |
| `TenCum` | `nvarchar(120)` | NOT NULL | — |
| `LoaiCum` | `nvarchar(32)` | NOT NULL | — |
| `SoCho` | `int` | NOT NULL | — |

- `[PK_CumGhe] PRIMARY KEY ([Id])`
- `[AK_CumGhe_Id_KhuVucId] UNIQUE ([Id], [KhuVucId])`
- `[CK_CumGhe_DieuKien1] CHECK ([SoCho] >= 2)`
- `[CK_CumGhe_LoaiCum] CHECK ([LoaiCum] IN (N'GheDoi', N'GheCum'))`
- `[FK_CumGhe_KhuVuc_KhuVucId] FOREIGN KEY ([KhuVucId]) REFERENCES [KhuVuc] ([Id])`
- INDEX `IX_CumGhe_KhuVucId` `([KhuVucId])`

### YeuCauThue

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `TaiKhoanId` | `int` | NOT NULL | — |
| `DiaDiemId` | `int` | NOT NULL | — |
| `ThoiGianBatDau` | `datetime2(3)` | NOT NULL | — |
| `ThoiGianKetThuc` | `datetime2(3)` | NOT NULL | — |
| `TongTien` | `decimal(19,0)` | NOT NULL | — |
| `TienCoc` | `decimal(19,0)` | NOT NULL | — |
| `HanDatCoc` | `datetime2(3)` | NULL | — |
| `ChinhSachId` | `int` | NOT NULL | — |
| `TrangThai` | `nvarchar(32)` | NOT NULL | — |
| `NguoiDuyetId` | `int` | NULL | — |
| `LyDoXuLy` | `nvarchar(max)` | NULL | — |
| `NgayHuy` | `datetime2(3)` | NULL | — |

- `[PK_YeuCauThue] PRIMARY KEY ([Id])`
- `[AK_YeuCauThue_Id_DiaDiemId] UNIQUE ([Id], [DiaDiemId])`
- `[CK_YeuCauThue_DieuKien1] CHECK ([ThoiGianBatDau] < [ThoiGianKetThuc])`
- `[CK_YeuCauThue_DieuKien2] CHECK ([TongTien] >= 0 AND [TienCoc] BETWEEN 0 AND [TongTien])`
- `[CK_YeuCauThue_HanDatCoc] CHECK ([TrangThai] <> N'ChoCoc' OR [HanDatCoc] IS NOT NULL)`
- `[CK_YeuCauThue_TienCoc] CHECK ([TienCoc] = ROUND([TongTien] * 0.30, 0))`
- `[CK_YeuCauThue_TrangThai] CHECK ([TrangThai] IN (N'ChoDuyet', N'ChoCoc', N'DaXacNhan', N'TuChoi', N'HetHan', N'DaHuy'))`
- `[FK_YeuCauThue_ChinhSach_ChinhSachId] FOREIGN KEY ([ChinhSachId]) REFERENCES [ChinhSach] ([Id])`
- `[FK_YeuCauThue_DiaDiem_DiaDiemId] FOREIGN KEY ([DiaDiemId]) REFERENCES [DiaDiem] ([Id])`
- `[FK_YeuCauThue_TaiKhoan_NguoiDuyetId] FOREIGN KEY ([NguoiDuyetId]) REFERENCES [TaiKhoan] ([Id])`
- `[FK_YeuCauThue_TaiKhoan_TaiKhoanId] FOREIGN KEY ([TaiKhoanId]) REFERENCES [TaiKhoan] ([Id])`
- INDEX `IX_YeuCauThue_ChinhSachId` `([ChinhSachId])`
- INDEX `IX_YeuCauThue_DiaDiemId` `([DiaDiemId])`
- INDEX `IX_YeuCauThue_NguoiDuyetId` `([NguoiDuyetId])`
- INDEX `IX_YeuCauThue_TaiKhoanId` `([TaiKhoanId])`

### LichDatDiaDiem

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `DiaDiemId` | `int` | NOT NULL | — |
| `YeuCauThueId` | `int` | NULL | — |
| `SuatDienId` | `int` | NULL | — |
| `BatDau` | `datetime2(3)` | NOT NULL | — |
| `KetThuc` | `datetime2(3)` | NOT NULL | — |
| `LoaiLich` | `nvarchar(32)` | NOT NULL | — |
| `TrangThai` | `nvarchar(32)` | NOT NULL | — |

- `[PK_LichDatDiaDiem] PRIMARY KEY ([Id])`
- `[CK_LichDatDiaDiem_DieuKien1] CHECK ([BatDau] < [KetThuc])`
- `[CK_LichDatDiaDiem_LoaiLich] CHECK ([LoaiLich] IN (N'Thue', N'SuatDien', N'BaoTri'))`
- `[CK_LichDatDiaDiem_TrangThai] CHECK ([TrangThai] IN (N'DangGiu', N'DaXacNhan', N'HetHan', N'DaHuy'))`
- `[FK_LichDatDiaDiem_DiaDiem_DiaDiemId] FOREIGN KEY ([DiaDiemId]) REFERENCES [DiaDiem] ([Id])`
- `[FK_LichDatDiaDiem_YeuCauThue_YeuCauThueId_DiaDiemId] FOREIGN KEY ([YeuCauThueId], [DiaDiemId]) REFERENCES [YeuCauThue] ([Id], [DiaDiemId])`
- `[FK_LichDatDiaDiem_SuatDien_SuatDienId_DiaDiemId] FOREIGN KEY ([SuatDienId], [DiaDiemId]) REFERENCES [SuatDien] ([Id], [DiaDiemId])`
- INDEX `IX_LichDatDiaDiem_SuatDienId_DiaDiemId` `([SuatDienId], [DiaDiemId])`
- INDEX `IX_LichDatDiaDiem_TraLich` `([DiaDiemId], [TrangThai], [BatDau], [KetThuc])`
- INDEX `IX_LichDatDiaDiem_YeuCauThueId_DiaDiemId` `([YeuCauThueId], [DiaDiemId])`

## Đặt vé và check-in

### LoaiVe

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `SuatDienId` | `int` | NOT NULL | — |
| `KhuVucId` | `int` | NOT NULL | — |
| `TenLoaiVe` | `nvarchar(120)` | NOT NULL | — |
| `NhomVe` | `nvarchar(32)` | NOT NULL | — |
| `GiaVe` | `decimal(19,0)` | NOT NULL | — |
| `SoLuong` | `int` | NOT NULL | — |
| `QuyenLoi` | `nvarchar(max)` | NULL | — |
| `BatDauBan` | `datetime2(3)` | NOT NULL | — |
| `KetThucBan` | `datetime2(3)` | NOT NULL | — |

- `[PK_LoaiVe] PRIMARY KEY ([Id])`
- `[AK_LoaiVe_Id_SuatDienId] UNIQUE ([Id], [SuatDienId])`
- `[CK_LoaiVe_DieuKien1] CHECK ([GiaVe] >= 0 AND [SoLuong] >= 0)`
- `[CK_LoaiVe_DieuKien2] CHECK ([BatDauBan] < [KetThucBan])`
- `[CK_LoaiVe_DieuKien3] CHECK ([NhomVe] IN (N'TieuChuan', N'VIP', N'VeMoi') AND ([NhomVe] <> N'VeMoi' OR [GiaVe] = 0))`
- `[FK_LoaiVe_KhuVuc_KhuVucId] FOREIGN KEY ([KhuVucId]) REFERENCES [KhuVuc] ([Id])`
- `[FK_LoaiVe_SuatDien_SuatDienId] FOREIGN KEY ([SuatDienId]) REFERENCES [SuatDien] ([Id])`
- INDEX `IX_LoaiVe_KhuVucId` `([KhuVucId])`
- INDEX `IX_LoaiVe_SuatDienId` `([SuatDienId])`

### DonHang

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `TaiKhoanId` | `int` | NOT NULL | — |
| `SuatDienId` | `int` | NOT NULL | — |
| `HopDongId` | `int` | NULL | — |
| `ChinhSachId` | `int` | NOT NULL | — |
| `TienVe` | `decimal(19,0)` | NOT NULL | — |
| `TienGiam` | `decimal(19,0)` | NOT NULL | — |
| `PhiDichVu` | `decimal(19,0)` | NOT NULL | — |
| `PhiNenTang` | `decimal(19,0)` | NOT NULL | — |
| `TongTien` | `decimal(19,0)` | NOT NULL | — |
| `TrangThai` | `nvarchar(32)` | NOT NULL | — |
| `NgayTao` | `datetime2(3)` | NOT NULL | — |
| `HanThanhToan` | `datetime2(3)` | NOT NULL | — |

- `[PK_DonHang] PRIMARY KEY ([Id])`
- `[CK_DonHang_DieuKien1] CHECK ([TienVe] >= 0 AND [TienGiam] BETWEEN 0 AND [TienVe] AND [PhiDichVu] >= 0 AND [PhiNenTang] >= 0 AND [TongTien] = [TienVe] - [TienGiam] + [PhiDichVu])`
- `[CK_DonHang_DieuKien2] CHECK ([HanThanhToan] > [NgayTao])`
- `[CK_DonHang_TrangThai] CHECK ([TrangThai] IN (N'ChoThanhToan', N'DaThanhToan', N'HetHan', N'DaHuy', N'DaThuTienHetCho', N'ChoHoanTien', N'HoanMotPhan', N'DaHoanTien'))`
- `[FK_DonHang_ChinhSach_ChinhSachId] FOREIGN KEY ([ChinhSachId]) REFERENCES [ChinhSach] ([Id])`
- `[FK_DonHang_TaiKhoan_TaiKhoanId] FOREIGN KEY ([TaiKhoanId]) REFERENCES [TaiKhoan] ([Id])`
- `[FK_DonHang_HopDong_HopDongId] FOREIGN KEY ([HopDongId]) REFERENCES [HopDong] ([Id])`
- `[FK_DonHang_SuatDien_SuatDienId] FOREIGN KEY ([SuatDienId]) REFERENCES [SuatDien] ([Id])`
- INDEX `IX_DonHang_ChinhSachId` `([ChinhSachId])`
- INDEX `IX_DonHang_HanMuc` `([SuatDienId], [TaiKhoanId], [TrangThai])`
- INDEX `IX_DonHang_HopDongId` `([HopDongId])`
- INDEX `IX_DonHang_TaiKhoanId` `([TaiKhoanId])`

### ChiTietDonHang

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `DonHangId` | `int` | NOT NULL | — |
| `LoaiVeId` | `int` | NOT NULL | — |
| `SoLuong` | `int` | NOT NULL | — |
| `DonGia` | `decimal(19,0)` | NOT NULL | — |
| `ThanhTien` | `decimal(19,0)` | NOT NULL | — |

- `[PK_ChiTietDonHang] PRIMARY KEY ([Id])`
- `[CK_ChiTietDonHang_DieuKien1] CHECK ([SoLuong] > 0 AND [DonGia] >= 0 AND [ThanhTien] = [SoLuong] * [DonGia])`
- `[FK_ChiTietDonHang_DonHang_DonHangId] FOREIGN KEY ([DonHangId]) REFERENCES [DonHang] ([Id])`
- `[FK_ChiTietDonHang_LoaiVe_LoaiVeId] FOREIGN KEY ([LoaiVeId]) REFERENCES [LoaiVe] ([Id])`
- INDEX `IX_ChiTietDonHang_DonHangId` `([DonHangId])`
- INDEX `IX_ChiTietDonHang_LoaiVeId` `([LoaiVeId])`

### GiuGhe

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `SuatDienId` | `int` | NOT NULL | — |
| `GheId` | `int` | NULL | — |
| `ChiTietDonHangId` | `int` | NOT NULL | — |
| `HetHan` | `datetime2(3)` | NOT NULL | — |
| `TrangThai` | `nvarchar(32)` | NOT NULL | — |

- `[PK_GiuGhe] PRIMARY KEY ([Id])`
- `[CK_GiuGhe_TrangThai] CHECK ([TrangThai] IN (N'DangGiu', N'DaChot', N'DaNha', N'HetHan'))`
- `[FK_GiuGhe_ChiTietDonHang_ChiTietDonHangId] FOREIGN KEY ([ChiTietDonHangId]) REFERENCES [ChiTietDonHang] ([Id])`
- `[FK_GiuGhe_Ghe_GheId] FOREIGN KEY ([GheId]) REFERENCES [Ghe] ([Id])`
- `[FK_GiuGhe_SuatDien_SuatDienId] FOREIGN KEY ([SuatDienId]) REFERENCES [SuatDien] ([Id])`
- INDEX `IX_GiuGhe_ChiTietDonHangId` `([ChiTietDonHangId])`
- INDEX `IX_GiuGhe_GheId` `([GheId])`
- INDEX `IX_GiuGhe_HetHan` `([TrangThai], [HetHan])`
- UNIQUE `IX_GiuGhe_GheDangGiu` `([SuatDienId], [GheId]) WHERE [GheId] IS NOT NULL AND [TrangThai] = N'DangGiu'`

### Ve

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `ChiTietDonHangId` | `int` | NULL | — |
| `GiuGheId` | `int` | NULL | — |
| `LoaiVeId` | `int` | NOT NULL | — |
| `SuatDienId` | `int` | NOT NULL | — |
| `GheId` | `int` | NULL | — |
| `MaVe` | `nvarchar(64)` | NOT NULL | — |
| `MaQR` | `nvarchar(128)` | NOT NULL | — |
| `TrangThai` | `nvarchar(32)` | NOT NULL | — |
| `NgayPhatHanh` | `datetime2(3)` | NOT NULL | — |
| `DaCheckInLuc` | `datetime2(3)` | NULL | — |

- `[PK_Ve] PRIMARY KEY ([Id])`
- `[CK_Ve_DieuKien1] CHECK (([TrangThai] = N'DaCheckIn' AND [DaCheckInLuc] IS NOT NULL) OR ([TrangThai] = N'DaPhatHanh' AND [DaCheckInLuc] IS NULL) OR [TrangThai] IN (N'DaHuy', N'DaThuHoi'))`
- `[CK_Ve_DieuKien2] CHECK ([GiuGheId] IS NULL OR [ChiTietDonHangId] IS NOT NULL)`
- `[CK_Ve_TrangThai] CHECK ([TrangThai] IN (N'DaPhatHanh', N'DaCheckIn', N'DaHuy', N'DaThuHoi'))`
- `[FK_Ve_ChiTietDonHang_ChiTietDonHangId] FOREIGN KEY ([ChiTietDonHangId]) REFERENCES [ChiTietDonHang] ([Id])`
- `[FK_Ve_Ghe_GheId] FOREIGN KEY ([GheId]) REFERENCES [Ghe] ([Id])`
- `[FK_Ve_GiuGhe_GiuGheId] FOREIGN KEY ([GiuGheId]) REFERENCES [GiuGhe] ([Id])`
- `[FK_Ve_LoaiVe_LoaiVeId_SuatDienId] FOREIGN KEY ([LoaiVeId], [SuatDienId]) REFERENCES [LoaiVe] ([Id], [SuatDienId])`
- `[FK_Ve_SuatDien_SuatDienId] FOREIGN KEY ([SuatDienId]) REFERENCES [SuatDien] ([Id])`
- UNIQUE `IX_Ve_GheDaCap` `([SuatDienId], [GheId]) WHERE [GheId] IS NOT NULL AND [TrangThai] IN (N'DaPhatHanh', N'DaCheckIn')`
- UNIQUE `IX_Ve_GiuGhe` `([GiuGheId]) WHERE [GiuGheId] IS NOT NULL`
- UNIQUE `IX_Ve_MaQR` `([MaQR])`
- UNIQUE `IX_Ve_MaVe` `([MaVe])`
- INDEX `IX_Ve_GheId` `([GheId])`
- INDEX `IX_Ve_HanMuc` `([ChiTietDonHangId], [SuatDienId], [TrangThai])`
- INDEX `IX_Ve_LoaiVeId_SuatDienId` `([LoaiVeId], [SuatDienId])`

### VeMoi

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `VeId` | `int` | NOT NULL | — |
| `HoTenKhachMoi` | `nvarchar(120)` | NOT NULL | — |
| `Email` | `nvarchar(255)` | NULL | — |
| `SoDienThoai` | `nvarchar(15)` | NULL | — |
| `TaiKhoanNhanId` | `int` | NULL | — |
| `MaBamNhanVe` | `nvarchar(128)` | NULL | — |
| `HanNhanVe` | `datetime2(3)` | NULL | — |
| `DaGuiLuc` | `datetime2(3)` | NULL | — |
| `DaNhanLuc` | `datetime2(3)` | NULL | — |
| `TrangThai` | `nvarchar(32)` | NOT NULL | — |

- `[PK_VeMoi] PRIMARY KEY ([Id])`
- `[CK_VeMoi_DieuKien1] CHECK (NULLIF(TRIM([Email]), N'') IS NOT NULL OR NULLIF(TRIM([SoDienThoai]), N'') IS NOT NULL)`
- `[CK_VeMoi_TrangThai] CHECK ([TrangThai] IN (N'ChoGui', N'DaGui', N'DaNhan', N'DaThuHoi', N'HetHan'))`
- `[FK_VeMoi_TaiKhoan_TaiKhoanNhanId] FOREIGN KEY ([TaiKhoanNhanId]) REFERENCES [TaiKhoan] ([Id])`
- `[FK_VeMoi_Ve_VeId] FOREIGN KEY ([VeId]) REFERENCES [Ve] ([Id])`
- INDEX `IX_VeMoi_TaiKhoanNhanId` `([TaiKhoanNhanId])`
- UNIQUE `IX_VeMoi_Ve` `([VeId])`

### NhatKyCheckIn

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `VeId` | `int` | NOT NULL | — |
| `NhanVienId` | `int` | NOT NULL | — |
| `ThoiGianQuet` | `datetime2(3)` | NOT NULL | — |
| `KetQua` | `nvarchar(32)` | NOT NULL | — |
| `HinhThuc` | `nvarchar(32)` | NOT NULL | — |

- `[PK_NhatKyCheckIn] PRIMARY KEY ([Id])`
- `[CK_NhatKyCheckIn_HinhThuc] CHECK ([HinhThuc] IN (N'QuetQR', N'ThuCong'))`
- `[FK_NhatKyCheckIn_TaiKhoan_NhanVienId] FOREIGN KEY ([NhanVienId]) REFERENCES [TaiKhoan] ([Id])`
- `[FK_NhatKyCheckIn_Ve_VeId] FOREIGN KEY ([VeId]) REFERENCES [Ve] ([Id])`
- INDEX `IX_NhatKyCheckIn_NhanVienId` `([NhanVienId])`
- UNIQUE `IX_NhatKyCheckIn_ThanhCong` `([VeId]) WHERE [KetQua] = N'ThanhCong'`

### Voucher

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `MaVoucher` | `nvarchar(64)` | NOT NULL | — |
| `SuKienId` | `int` | NULL | — |
| `SuatDienId` | `int` | NULL | — |
| `KhuVucId` | `int` | NULL | — |
| `LoaiGiam` | `nvarchar(32)` | NOT NULL | — |
| `GiaTri` | `decimal(19,2)` | NOT NULL | — |
| `MucGiamToiDa` | `decimal(19,0)` | NULL | — |
| `GiaTriDonToiThieu` | `decimal(19,0)` | NOT NULL | — |
| `SoLuong` | `int` | NOT NULL | — |
| `LuotMoiTaiKhoan` | `int` | NOT NULL | — |
| `NguonTaiTro` | `nvarchar(32)` | NOT NULL | — |
| `NgayBatDau` | `datetime2(3)` | NOT NULL | — |
| `NgayKetThuc` | `datetime2(3)` | NOT NULL | — |
| `TrangThai` | `nvarchar(32)` | NOT NULL | — |

- `[PK_Voucher] PRIMARY KEY ([Id])`
- `[CK_Voucher_DieuKien1] CHECK ([NgayBatDau] < [NgayKetThuc])`
- `[CK_Voucher_DieuKien2] CHECK ([SoLuong] > 0 AND [LuotMoiTaiKhoan] BETWEEN 1 AND [SoLuong] AND [GiaTriDonToiThieu] >= 0 AND ([MucGiamToiDa] IS NULL OR [MucGiamToiDa] > 0))`
- `[CK_Voucher_DieuKien3] CHECK (([LoaiGiam] = N'PhanTram' AND [GiaTri] > 0 AND [GiaTri] <= 100 AND [MucGiamToiDa] IS NOT NULL) OR ([LoaiGiam] = N'SoTien' AND [GiaTri] > 0 AND [GiaTri] = FLOOR([GiaTri])))`
- `[CK_Voucher_TrangThai] CHECK ([TrangThai] IN (N'Nhap', N'ChoDuyet', N'HoatDong', N'TamDung', N'TuChoi', N'HetHan'))`
- `[FK_Voucher_KhuVuc_KhuVucId] FOREIGN KEY ([KhuVucId]) REFERENCES [KhuVuc] ([Id])`
- `[FK_Voucher_SuKien_SuKienId] FOREIGN KEY ([SuKienId]) REFERENCES [SuKien] ([Id])`
- `[FK_Voucher_SuatDien_SuatDienId] FOREIGN KEY ([SuatDienId]) REFERENCES [SuatDien] ([Id])`
- INDEX `IX_Voucher_KhuVucId` `([KhuVucId])`
- INDEX `IX_Voucher_SuatDienId` `([SuatDienId])`
- INDEX `IX_Voucher_SuKienId` `([SuKienId])`
- UNIQUE `IX_Voucher_Ma` `([MaVoucher])`

## Thanh toán và tài chính

### SuDungVoucher

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `VoucherId` | `int` | NOT NULL | — |
| `DonHangId` | `int` | NOT NULL | — |
| `SoTienGiam` | `decimal(19,0)` | NOT NULL | — |
| `HetHanGiuLuot` | `datetime2(3)` | NOT NULL | — |
| `DaChotLuc` | `datetime2(3)` | NULL | — |
| `TrangThai` | `nvarchar(32)` | NOT NULL | — |

- `[PK_SuDungVoucher] PRIMARY KEY ([Id])`
- `[CK_SuDungVoucher_DieuKien1] CHECK ([SoTienGiam] >= 0)`
- `[CK_SuDungVoucher_DieuKien2] CHECK (([TrangThai] = N'DaSuDung' AND [DaChotLuc] IS NOT NULL) OR ([TrangThai] <> N'DaSuDung' AND [DaChotLuc] IS NULL))`
- `[CK_SuDungVoucher_TrangThai] CHECK ([TrangThai] IN (N'DangGiu', N'DaSuDung', N'DaNha'))`
- `[FK_SuDungVoucher_DonHang_DonHangId] FOREIGN KEY ([DonHangId]) REFERENCES [DonHang] ([Id])`
- `[FK_SuDungVoucher_Voucher_VoucherId] FOREIGN KEY ([VoucherId]) REFERENCES [Voucher] ([Id])`
- INDEX `IX_SuDungVoucher_DemLuot` `([VoucherId], [TrangThai], [HetHanGiuLuot])`
- UNIQUE `IX_SuDungVoucher_DonHang` `([DonHangId])`

### GiaoDichThanhToan

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `DonHangId` | `int` | NULL | — |
| `YeuCauThueId` | `int` | NULL | — |
| `MaDonNhaCungCap` | `nvarchar(128)` | NULL | — |
| `MaGiaoDich` | `nvarchar(128)` | NULL | — |
| `SoTienYeuCau` | `decimal(19,0)` | NOT NULL | — |
| `SoTienThucThu` | `decimal(19,0)` | NULL | — |
| `PhuongThuc` | `nvarchar(32)` | NOT NULL | — |
| `TrangThai` | `nvarchar(32)` | NOT NULL | — |
| `NgayThanhToan` | `datetime2(3)` | NULL | — |
| `ChungTu` | `nvarchar(500)` | NULL | — |

- `[PK_GiaoDichThanhToan] PRIMARY KEY ([Id])`
- `[CK_GiaoDichThanhToan_DieuKien1] CHECK (([DonHangId] IS NULL AND [YeuCauThueId] IS NOT NULL) OR ([DonHangId] IS NOT NULL AND [YeuCauThueId] IS NULL))`
- `[CK_GiaoDichThanhToan_DieuKien2] CHECK ([SoTienYeuCau] >= 0 AND ([SoTienThucThu] IS NULL OR [SoTienThucThu] >= 0))`
- `[CK_GiaoDichThanhToan_PhuongThuc] CHECK ([PhuongThuc] IN (N'PayOS', N'ChuyenKhoan', N'MienPhi'))`
- `[CK_GiaoDichThanhToan_ThucThu] CHECK ([TrangThai] <> N'ThanhCong' OR ([SoTienThucThu] IS NOT NULL AND [SoTienThucThu] = [SoTienYeuCau] AND [NgayThanhToan] IS NOT NULL))`
- `[CK_GiaoDichThanhToan_TrangThai] CHECK ([TrangThai] IN (N'ChoThanhToan', N'ThanhCong', N'ThatBai', N'HetHan', N'NghiVan', N'ThuTrungCanHoan'))`
- `[FK_GiaoDichThanhToan_DonHang_DonHangId] FOREIGN KEY ([DonHangId]) REFERENCES [DonHang] ([Id])`
- `[FK_GiaoDichThanhToan_YeuCauThue_YeuCauThueId] FOREIGN KEY ([YeuCauThueId]) REFERENCES [YeuCauThue] ([Id])`
- UNIQUE `IX_GiaoDichThanhToan_ChotCoc` `([YeuCauThueId]) WHERE [YeuCauThueId] IS NOT NULL AND [TrangThai] = N'ThanhCong'`
- UNIQUE `IX_GiaoDichThanhToan_ChotDon` `([DonHangId]) WHERE [DonHangId] IS NOT NULL AND [TrangThai] = N'ThanhCong'`
- UNIQUE `IX_GiaoDichThanhToan_MaDon` `([MaDonNhaCungCap]) WHERE [MaDonNhaCungCap] IS NOT NULL`
- UNIQUE `IX_GiaoDichThanhToan_MaGiaoDich` `([MaGiaoDich]) WHERE [MaGiaoDich] IS NOT NULL`

### WebhookThanhToan

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `GiaoDichId` | `int` | NULL | — |
| `MaSuKienWebhook` | `nvarchar(128)` | NOT NULL | — |
| `NoiDungDaLoc` | `nvarchar(max)` | NOT NULL | — |
| `ThoiGianNhan` | `datetime2(3)` | NOT NULL | — |
| `TrangThaiXuLy` | `nvarchar(32)` | NOT NULL | — |

- `[PK_WebhookThanhToan] PRIMARY KEY ([Id])`
- `[CK_WebhookThanhToan_NoiDungDaLoc] CHECK (ISJSON([NoiDungDaLoc], OBJECT) = 1)`
- `[CK_WebhookThanhToan_TrangThaiXuLy] CHECK ([TrangThaiXuLy] IN (N'ChoXuLy', N'DaXuLy', N'NghiVan', N'Loi'))`
- `[FK_WebhookThanhToan_GiaoDichThanhToan_GiaoDichId] FOREIGN KEY ([GiaoDichId]) REFERENCES [GiaoDichThanhToan] ([Id])`
- INDEX `IX_WebhookThanhToan_GiaoDichId` `([GiaoDichId])`
- UNIQUE `IX_WebhookThanhToan_MaSuKien` `([MaSuKienWebhook])`

### YeuCauHoanTien

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `TaiKhoanId` | `int` | NOT NULL | — |
| `GiaoDichId` | `int` | NOT NULL | — |
| `VeId` | `int` | NULL | — |
| `KhoaChongLap` | `nvarchar(128)` | NOT NULL | — |
| `SoTienYeuCau` | `decimal(19,0)` | NOT NULL | — |
| `SoTienDuyet` | `decimal(19,0)` | NULL | — |
| `LoaiYeuCau` | `nvarchar(32)` | NOT NULL | — |
| `LyDo` | `nvarchar(max)` | NOT NULL | — |
| `TrangThai` | `nvarchar(32)` | NOT NULL | — |
| `NgayYeuCau` | `datetime2(3)` | NOT NULL | — |
| `NguoiDuyetId` | `int` | NULL | — |
| `NgayDuyet` | `datetime2(3)` | NULL | — |

- `[PK_YeuCauHoanTien] PRIMARY KEY ([Id])`
- `[CK_YeuCauHoanTien_DieuKien1] CHECK ([SoTienYeuCau] > 0 AND ([SoTienDuyet] IS NULL OR [SoTienDuyet] BETWEEN 1 AND [SoTienYeuCau]))`
- `[CK_YeuCauHoanTien_TrangThai] CHECK ([TrangThai] IN (N'ChoDuyet', N'DaDuyet', N'TuChoi', N'DangChuyen', N'HoanTat', N'ThatBai'))`
- `[FK_YeuCauHoanTien_GiaoDichThanhToan_GiaoDichId] FOREIGN KEY ([GiaoDichId]) REFERENCES [GiaoDichThanhToan] ([Id])`
- `[FK_YeuCauHoanTien_TaiKhoan_NguoiDuyetId] FOREIGN KEY ([NguoiDuyetId]) REFERENCES [TaiKhoan] ([Id])`
- `[FK_YeuCauHoanTien_TaiKhoan_TaiKhoanId] FOREIGN KEY ([TaiKhoanId]) REFERENCES [TaiKhoan] ([Id])`
- `[FK_YeuCauHoanTien_Ve_VeId] FOREIGN KEY ([VeId]) REFERENCES [Ve] ([Id])`
- UNIQUE `IX_YeuCauHoanTien_ChongLap` `([KhoaChongLap])`
- INDEX `IX_YeuCauHoanTien_GiaoDichId` `([GiaoDichId])`
- INDEX `IX_YeuCauHoanTien_NguoiDuyetId` `([NguoiDuyetId])`
- INDEX `IX_YeuCauHoanTien_TaiKhoanId` `([TaiKhoanId])`
- INDEX `IX_YeuCauHoanTien_VeId` `([VeId])`

### GiaoDichHoanTien

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `YeuCauHoanTienId` | `int` | NOT NULL | — |
| `KhoaChongLap` | `nvarchar(128)` | NOT NULL | — |
| `MaGiaoDichHoan` | `nvarchar(128)` | NULL | — |
| `SoTien` | `decimal(19,0)` | NOT NULL | — |
| `TrangThai` | `nvarchar(32)` | NOT NULL | — |
| `NgayXuLy` | `datetime2(3)` | NULL | — |
| `ChungTu` | `nvarchar(500)` | NULL | — |

- `[PK_GiaoDichHoanTien] PRIMARY KEY ([Id])`
- `[CK_GiaoDichHoanTien_DieuKien1] CHECK ([SoTien] > 0)`
- `[CK_GiaoDichHoanTien_DieuKien2] CHECK ([TrangThai] <> N'ThanhCong' OR ([NgayXuLy] IS NOT NULL AND ([MaGiaoDichHoan] IS NOT NULL OR [ChungTu] IS NOT NULL)))`
- `[CK_GiaoDichHoanTien_TrangThai] CHECK ([TrangThai] IN (N'ChoChuyen', N'DangChuyen', N'ThanhCong', N'ThatBai'))`
- `[FK_GiaoDichHoanTien_YeuCauHoanTien_YeuCauHoanTienId] FOREIGN KEY ([YeuCauHoanTienId]) REFERENCES [YeuCauHoanTien] ([Id])`
- UNIQUE `IX_GiaoDichHoanTien_ChongLap` `([KhoaChongLap])`
- UNIQUE `IX_GiaoDichHoanTien_MaGiaoDich` `([MaGiaoDichHoan]) WHERE [MaGiaoDichHoan] IS NOT NULL`
- INDEX `IX_GiaoDichHoanTien_YeuCauHoanTienId` `([YeuCauHoanTienId])`

### HopDong

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `BTCId` | `int` | NOT NULL | — |
| `SuKienId` | `int` | NULL | — |
| `LoaiPhi` | `nvarchar(32)` | NOT NULL | — |
| `TyLePhi` | `decimal(5,2)` | NULL | — |
| `PhiMoiVe` | `decimal(19,0)` | NULL | — |
| `NoiDung` | `nvarchar(max)` | NOT NULL | — |
| `NgayKy` | `datetime2(3)` | NOT NULL | — |
| `HieuLucTu` | `datetime2(3)` | NOT NULL | — |
| `HieuLucDen` | `datetime2(3)` | NULL | — |
| `TrangThai` | `nvarchar(32)` | NOT NULL | — |

- `[PK_HopDong] PRIMARY KEY ([Id])`
- `[CK_HopDong_DieuKien1] CHECK ([HieuLucDen] IS NULL OR [HieuLucTu] < [HieuLucDen])`
- `[CK_HopDong_DieuKien2] CHECK (([LoaiPhi] = N'PhanTram' AND [TyLePhi] IS NOT NULL AND [TyLePhi] BETWEEN 0 AND 100 AND [PhiMoiVe] IS NULL) OR ([LoaiPhi] = N'MoiVe' AND [TyLePhi] IS NULL AND [PhiMoiVe] IS NOT NULL AND [PhiMoiVe] >= 0))`
- `[CK_HopDong_TrangThai] CHECK ([TrangThai] IN (N'Nhap', N'CoHieuLuc', N'HetHan', N'DaHuy'))`
- `[FK_HopDong_HoSoBTC_BTCId] FOREIGN KEY ([BTCId]) REFERENCES [HoSoBTC] ([Id])`
- `[FK_HopDong_SuKien_SuKienId] FOREIGN KEY ([SuKienId]) REFERENCES [SuKien] ([Id])`
- INDEX `IX_HopDong_HieuLuc` `([BTCId], [SuKienId], [HieuLucTu], [HieuLucDen])`
- INDEX `IX_HopDong_SuKienId` `([SuKienId])`

### DoiSoat

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `SuKienId` | `int` | NOT NULL | — |
| `PhienBan` | `int` | NOT NULL | — |
| `TuNgay` | `date` | NOT NULL | — |
| `DenNgay` | `date` | NOT NULL | — |
| `TongDoanhThu` | `decimal(19,0)` | NOT NULL | — |
| `TongTienHoan` | `decimal(19,0)` | NOT NULL | — |
| `PhiNenTang` | `decimal(19,0)` | NOT NULL | — |
| `TienDieuChinh` | `decimal(19,0)` | NOT NULL | — |
| `TienBTCNhan` | `decimal(19,0)` | NOT NULL | — |
| `ChiTietDoiSoat` | `nvarchar(max)` | NOT NULL | — |
| `HanXacNhan` | `datetime2(3)` | NOT NULL | — |
| `NguoiDuyetId` | `int` | NULL | — |
| `NgayDuyet` | `datetime2(3)` | NULL | — |
| `TrangThai` | `nvarchar(32)` | NOT NULL | — |

- `[PK_DoiSoat] PRIMARY KEY ([Id])`
- `[CK_DoiSoat_ChiTietDoiSoat] CHECK (ISJSON([ChiTietDoiSoat], OBJECT) = 1)`
- `[CK_DoiSoat_DieuKien1] CHECK ([TuNgay] <= [DenNgay] AND [PhienBan] > 0)`
- `[CK_DoiSoat_DieuKien2] CHECK ([TongDoanhThu] >= 0 AND [TongTienHoan] >= 0 AND [PhiNenTang] >= 0 AND [TienBTCNhan] = [TongDoanhThu] - [TongTienHoan] - [PhiNenTang] - [TienDieuChinh])`
- `[CK_DoiSoat_TrangThai] CHECK ([TrangThai] IN (N'ChoXacNhan', N'DaXacNhan', N'TranhChap', N'ChoGiaiNgan', N'HoanTat'))`
- `[FK_DoiSoat_TaiKhoan_NguoiDuyetId] FOREIGN KEY ([NguoiDuyetId]) REFERENCES [TaiKhoan] ([Id])`
- `[FK_DoiSoat_SuKien_SuKienId] FOREIGN KEY ([SuKienId]) REFERENCES [SuKien] ([Id])`
- UNIQUE `IX_DoiSoat_KyChiTra` `([SuKienId], [TuNgay], [DenNgay]) WHERE [TrangThai] IN (N'ChoGiaiNgan', N'HoanTat')`
- UNIQUE `IX_DoiSoat_PhienBan` `([SuKienId], [TuNgay], [DenNgay], [PhienBan])`
- INDEX `IX_DoiSoat_NguoiDuyetId` `([NguoiDuyetId])`

### GiaiNgan

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `DoiSoatId` | `int` | NOT NULL | — |
| `KhoaChongLap` | `nvarchar(128)` | NOT NULL | — |
| `MaGiaoDich` | `nvarchar(128)` | NULL | — |
| `SoTien` | `decimal(19,0)` | NOT NULL | — |
| `TrangThai` | `nvarchar(32)` | NOT NULL | — |
| `NgayGiaiNgan` | `datetime2(3)` | NULL | — |
| `ChungTu` | `nvarchar(500)` | NULL | — |

- `[PK_GiaiNgan] PRIMARY KEY ([Id])`
- `[CK_GiaiNgan_DieuKien1] CHECK ([SoTien] > 0)`
- `[CK_GiaiNgan_DieuKien2] CHECK ([TrangThai] <> N'ThanhCong' OR ([NgayGiaiNgan] IS NOT NULL AND ([MaGiaoDich] IS NOT NULL OR [ChungTu] IS NOT NULL)))`
- `[CK_GiaiNgan_TrangThai] CHECK ([TrangThai] IN (N'ChoChuyen', N'DangChuyen', N'ThanhCong', N'ThatBai'))`
- `[FK_GiaiNgan_DoiSoat_DoiSoatId] FOREIGN KEY ([DoiSoatId]) REFERENCES [DoiSoat] ([Id])`
- UNIQUE `IX_GiaiNgan_ChongLap` `([KhoaChongLap])`
- UNIQUE `IX_GiaiNgan_DoiSoat` `([DoiSoatId])`
- UNIQUE `IX_GiaiNgan_MaGiaoDich` `([MaGiaoDich]) WHERE [MaGiaoDich] IS NOT NULL`

## Quản trị và hỗ trợ

### ThongBao

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `TaiKhoanId` | `int` | NULL | — |
| `VeMoiId` | `int` | NULL | — |
| `TieuDe` | `nvarchar(200)` | NOT NULL | — |
| `NoiDung` | `nvarchar(max)` | NOT NULL | — |
| `LoaiThongBao` | `nvarchar(32)` | NOT NULL | — |
| `KenhGui` | `nvarchar(32)` | NOT NULL | — |
| `TrangThaiGui` | `nvarchar(32)` | NOT NULL | — |
| `DaDoc` | `bit` | NOT NULL | — |
| `NgayTao` | `datetime2(3)` | NOT NULL | — |
| `NgayGui` | `datetime2(3)` | NULL | — |

- `[PK_ThongBao] PRIMARY KEY ([Id])`
- `[CK_ThongBao_DieuKien1] CHECK (([TaiKhoanId] IS NULL AND [VeMoiId] IS NOT NULL) OR ([TaiKhoanId] IS NOT NULL AND [VeMoiId] IS NULL))`
- `[CK_ThongBao_KenhGui] CHECK ([KenhGui] IN (N'TrongUngDung', N'Email', N'SMS'))`
- `[CK_ThongBao_TrangThaiGui] CHECK ([TrangThaiGui] IN (N'ChoGui', N'DangGui', N'DaGui', N'LoiGui'))`
- `[FK_ThongBao_TaiKhoan_TaiKhoanId] FOREIGN KEY ([TaiKhoanId]) REFERENCES [TaiKhoan] ([Id])`
- `[FK_ThongBao_VeMoi_VeMoiId] FOREIGN KEY ([VeMoiId]) REFERENCES [VeMoi] ([Id])`
- INDEX `IX_ThongBao_HopThu` `([TaiKhoanId], [DaDoc], [NgayTao])`
- INDEX `IX_ThongBao_VeMoiId` `([VeMoiId])`

### KhieuNai

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `TaiKhoanId` | `int` | NOT NULL | — |
| `SuKienId` | `int` | NULL | — |
| `VeId` | `int` | NULL | — |
| `YeuCauThueId` | `int` | NULL | — |
| `TieuDe` | `nvarchar(200)` | NOT NULL | — |
| `NoiDung` | `nvarchar(max)` | NOT NULL | — |
| `TrangThai` | `nvarchar(32)` | NOT NULL | — |
| `NguoiXuLyId` | `int` | NULL | — |
| `NgayTao` | `datetime2(3)` | NOT NULL | — |
| `KetQuaXuLy` | `nvarchar(max)` | NULL | — |

- `[PK_KhieuNai] PRIMARY KEY ([Id])`
- `[CK_KhieuNai_TrangThai] CHECK ([TrangThai] IN (N'ChoXuLy', N'DangXuLy', N'DaGiaiQuyet', N'TuChoi'))`
- `[FK_KhieuNai_TaiKhoan_NguoiXuLyId] FOREIGN KEY ([NguoiXuLyId]) REFERENCES [TaiKhoan] ([Id])`
- `[FK_KhieuNai_TaiKhoan_TaiKhoanId] FOREIGN KEY ([TaiKhoanId]) REFERENCES [TaiKhoan] ([Id])`
- `[FK_KhieuNai_YeuCauThue_YeuCauThueId] FOREIGN KEY ([YeuCauThueId]) REFERENCES [YeuCauThue] ([Id])`
- `[FK_KhieuNai_SuKien_SuKienId] FOREIGN KEY ([SuKienId]) REFERENCES [SuKien] ([Id])`
- `[FK_KhieuNai_Ve_VeId] FOREIGN KEY ([VeId]) REFERENCES [Ve] ([Id])`
- INDEX `IX_KhieuNai_NguoiXuLyId` `([NguoiXuLyId])`
- INDEX `IX_KhieuNai_SuKienId` `([SuKienId])`
- INDEX `IX_KhieuNai_TaiKhoanId` `([TaiKhoanId])`
- INDEX `IX_KhieuNai_VeId` `([VeId])`
- INDEX `IX_KhieuNai_YeuCauThueId` `([YeuCauThueId])`

### DanhGia

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `TaiKhoanId` | `int` | NOT NULL | — |
| `SuKienId` | `int` | NOT NULL | — |
| `SoSao` | `int` | NOT NULL | — |
| `BinhLuan` | `nvarchar(max)` | NULL | — |
| `NgayDanhGia` | `datetime2(3)` | NOT NULL | — |
| `TrangThai` | `nvarchar(32)` | NOT NULL | — |

- `[PK_DanhGia] PRIMARY KEY ([Id])`
- `[CK_DanhGia_DieuKien1] CHECK ([SoSao] BETWEEN 1 AND 5)`
- `[CK_DanhGia_TrangThai] CHECK ([TrangThai] IN (N'ChoDuyet', N'CongKhai', N'DaAn'))`
- `[FK_DanhGia_TaiKhoan_TaiKhoanId] FOREIGN KEY ([TaiKhoanId]) REFERENCES [TaiKhoan] ([Id])`
- `[FK_DanhGia_SuKien_SuKienId] FOREIGN KEY ([SuKienId]) REFERENCES [SuKien] ([Id])`
- INDEX `IX_DanhGia_SuKienId` `([SuKienId])`
- UNIQUE `IX_DanhGia_TaiKhoanSuKien` `([TaiKhoanId], [SuKienId])`

### Banner

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `TieuDe` | `nvarchar(200)` | NOT NULL | — |
| `HinhAnh` | `nvarchar(500)` | NOT NULL | — |
| `DuongDan` | `nvarchar(500)` | NULL | — |
| `ThuTu` | `int` | NOT NULL | — |
| `TrangThai` | `nvarchar(32)` | NOT NULL | — |

- `[PK_Banner] PRIMARY KEY ([Id])`
- `[CK_Banner_DieuKien1] CHECK ([ThuTu] >= 0)`
- `[CK_Banner_TrangThai] CHECK ([TrangThai] IN (N'Hien', N'An'))`

### CauHinhHeThong

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `Khoa` | `nvarchar(100)` | NOT NULL | — |
| `GiaTri` | `nvarchar(max)` | NOT NULL | — |
| `MoTa` | `nvarchar(max)` | NULL | — |
| `NgayCapNhat` | `datetime2(3)` | NOT NULL | — |
| `NguoiCapNhatId` | `int` | NOT NULL | — |

- `[PK_CauHinhHeThong] PRIMARY KEY ([Id])`
- `[FK_CauHinhHeThong_TaiKhoan_NguoiCapNhatId] FOREIGN KEY ([NguoiCapNhatId]) REFERENCES [TaiKhoan] ([Id])`
- UNIQUE `IX_CauHinhHeThong_Khoa` `([Khoa])`
- INDEX `IX_CauHinhHeThong_NguoiCapNhatId` `([NguoiCapNhatId])`

### NhatKyHeThong

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `TaiKhoanId` | `int` | NULL | — |
| `HanhDong` | `nvarchar(100)` | NOT NULL | — |
| `DoiTuong` | `nvarchar(100)` | NOT NULL | — |
| `DoiTuongId` | `int` | NULL | — |
| `ThoiGian` | `datetime2(3)` | NOT NULL | — |
| `DuLieuThayDoi` | `nvarchar(max)` | NULL | — |

- `[PK_NhatKyHeThong] PRIMARY KEY ([Id])`
- `[CK_NhatKyHeThong_DuLieuThayDoi] CHECK ([DuLieuThayDoi] IS NULL OR ISJSON([DuLieuThayDoi], OBJECT) = 1)`
- `[FK_NhatKyHeThong_TaiKhoan_TaiKhoanId] FOREIGN KEY ([TaiKhoanId]) REFERENCES [TaiKhoan] ([Id])`
- INDEX `IX_NhatKyHeThong_TaiKhoanId` `([TaiKhoanId])`

### ChinhSach

| Cột | Kiểu SQL | Nullability | Identity / Default |
|---|---|---|---|
| `Id` | `int` | NOT NULL | `IDENTITY` |
| `TenChinhSach` | `nvarchar(120)` | NOT NULL | — |
| `NoiDung` | `nvarchar(max)` | NOT NULL | — |
| `PhienBan` | `int` | NOT NULL | — |
| `NgayHieuLuc` | `date` | NOT NULL | — |
| `TrangThai` | `nvarchar(32)` | NOT NULL | — |

- `[PK_ChinhSach] PRIMARY KEY ([Id])`
- `[CK_ChinhSach_DieuKien1] CHECK ([PhienBan] > 0)`
- `[CK_ChinhSach_TrangThai] CHECK ([TrangThai] IN (N'Nhap', N'CoHieuLuc', N'HetHan', N'DaHuy'))`
- UNIQUE `IX_ChinhSach_PhienBan` `([TenChinhSach], [PhienBan])`

## Đối tượng khác và phạm vi

| Đối tượng | Kết quả | Bằng chứng |
|---|---|---|
| View, stored procedure, function do QLSK định nghĩa, trigger | Không tìm thấy trong mã cấu hình hoặc DDL sinh. | [AppDbContext](../../Data/AppDbContext.cs) · [Configs](../../Data/Configs) |
| Migration, model snapshot, seed | Không tìm thấy file thực thi trong `Data/Migrations/` hoặc `Data/seed/`. | [Data](../../Data) |
| SQL kiểm thử | `RangBuoc.sql` kiểm tra schema và ràng buộc với dữ liệu thử trong giao dịch. | [RangBuoc.sql](../../test/RangBuoc.sql#L1) |
| Ảnh ERD | 7 ảnh, gồm sáu nhóm và sơ đồ tổng thể. Báo cáo đối chiếu định danh, kiểu và nullable trên bản Mermaid có thể đọc máy. | [so_do_erd.md](../so_do_erd.md#L1) · [anh_erd](../anh_erd) |
