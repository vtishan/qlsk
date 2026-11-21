-- Initial SQL Server schema for QLSK.
CREATE TABLE [dbo].[Banner] (
    [Id] int NOT NULL IDENTITY,
    [TieuDe] nvarchar(200) NOT NULL,
    [HinhAnh] nvarchar(500) NOT NULL,
    [DuongDan] nvarchar(500) NULL,
    [ThuTu] int NOT NULL,
    [TrangThai] nvarchar(32) NOT NULL,
    CONSTRAINT [PK_Banner] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Banner_DieuKien1] CHECK ([ThuTu] >= 0),
    CONSTRAINT [CK_Banner_TrangThai] CHECK ([TrangThai] IN (N'Hien', N'An'))
);
GO


CREATE TABLE [dbo].[ChinhSach] (
    [Id] int NOT NULL IDENTITY,
    [TenChinhSach] nvarchar(120) NOT NULL,
    [NoiDung] nvarchar(max) NOT NULL,
    [PhienBan] int NOT NULL,
    [NgayHieuLuc] date NOT NULL,
    [TrangThai] nvarchar(32) NOT NULL,
    CONSTRAINT [PK_ChinhSach] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ChinhSach_DieuKien1] CHECK ([PhienBan] > 0),
    CONSTRAINT [CK_ChinhSach_TrangThai] CHECK ([TrangThai] IN (N'Nhap', N'CoHieuLuc', N'HetHan', N'DaHuy'))
);
GO


CREATE TABLE [dbo].[DanhMucSuKien] (
    [Id] int NOT NULL IDENTITY,
    [TenDanhMuc] nvarchar(120) NOT NULL,
    [MoTa] nvarchar(max) NULL,
    [TrangThai] nvarchar(32) NOT NULL,
    CONSTRAINT [PK_DanhMucSuKien] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_DanhMucSuKien_TrangThai] CHECK ([TrangThai] IN (N'Hien', N'An'))
);
GO


CREATE TABLE [dbo].[DiaDiem] (
    [Id] int NOT NULL IDENTITY,
    [TenDiaDiem] nvarchar(200) NOT NULL,
    [DiaChi] nvarchar(300) NOT NULL,
    [SucChua] int NOT NULL,
    [GiaThue] decimal(19,0) NOT NULL,
    [TienIch] nvarchar(max) NULL,
    [TrangThai] nvarchar(32) NOT NULL,
    CONSTRAINT [PK_DiaDiem] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_DiaDiem_DieuKien1] CHECK ([SucChua] > 0 AND [GiaThue] >= 0),
    CONSTRAINT [CK_DiaDiem_TrangThai] CHECK ([TrangThai] IN (N'HoatDong', N'TamDung', N'DaAn'))
);
GO


CREATE TABLE [dbo].[TaiKhoan] (
    [Id] int NOT NULL IDENTITY,
    [Email] nvarchar(255) NULL,
    [SoDienThoai] nvarchar(15) NULL,
    [HoTen] nvarchar(100) NOT NULL,
    [TenDangNhap] nvarchar(50) NOT NULL,
    [MatKhauBam] nvarchar(500) NOT NULL,
    [VaiTro] int NOT NULL DEFAULT 1,
    [TrangThai] nvarchar(32) NOT NULL DEFAULT N'HoatDong',
    [NgayTao] datetime2(3) NOT NULL DEFAULT (SYSUTCDATETIME()),
    [EmailXacMinhLuc] datetime2(3) NULL,
    [SoDienThoaiXacMinhLuc] datetime2(3) NULL,
    [MaBamKhoiPhuc] nvarchar(128) NULL,
    [HanKhoiPhuc] datetime2(3) NULL,
    [PhienBanDangNhap] int NOT NULL,
    [QuyenQuanLy] nvarchar(max) NULL,
    [SuKienYeuThich] nvarchar(max) NULL,
    [SuKienDaXem] nvarchar(max) NULL,
    CONSTRAINT [PK_TaiKhoan] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_TaiKhoan_KhoiPhuc] CHECK (([MaBamKhoiPhuc] IS NULL AND [HanKhoiPhuc] IS NULL) OR ([MaBamKhoiPhuc] IS NOT NULL AND [HanKhoiPhuc] IS NOT NULL)),
    CONSTRAINT [CK_TaiKhoan_LienHe] CHECK (([Email] IS NOT NULL OR [SoDienThoai] IS NOT NULL) AND ([Email] IS NULL OR TRIM([Email]) <> N'') AND ([SoDienThoai] IS NULL OR TRIM([SoDienThoai]) <> N'')),
    CONSTRAINT [CK_TaiKhoan_LienHeXacMinh] CHECK (([EmailXacMinhLuc] IS NULL OR NULLIF(TRIM([Email]), N'') IS NOT NULL) AND ([SoDienThoaiXacMinhLuc] IS NULL OR NULLIF(TRIM([SoDienThoai]), N'') IS NOT NULL)),
    CONSTRAINT [CK_TaiKhoan_QuyenQuanLy] CHECK ([QuyenQuanLy] IS NULL OR ISJSON([QuyenQuanLy], OBJECT) = 1),
    CONSTRAINT [CK_TaiKhoan_TrangThai] CHECK ([TrangThai] IN (N'HoatDong', N'DaKhoa', N'ChoXacMinh') AND [PhienBanDangNhap] >= 0),
    CONSTRAINT [CK_TaiKhoan_VaiTro] CHECK ([VaiTro] BETWEEN 1 AND 5)
);
GO


CREATE TABLE [dbo].[KhuVuc] (
    [Id] int NOT NULL IDENTITY,
    [DiaDiemId] int NOT NULL,
    [TenKhuVuc] nvarchar(120) NOT NULL,
    [SucChua] int NOT NULL,
    CONSTRAINT [PK_KhuVuc] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_KhuVuc_DieuKien1] CHECK ([SucChua] > 0),
    CONSTRAINT [FK_KhuVuc_DiaDiem_DiaDiemId] FOREIGN KEY ([DiaDiemId]) REFERENCES [dbo].[DiaDiem] ([Id])
);
GO


CREATE TABLE [dbo].[CauHinhHeThong] (
    [Id] int NOT NULL IDENTITY,
    [Khoa] nvarchar(100) NOT NULL,
    [GiaTri] nvarchar(max) NOT NULL,
    [MoTa] nvarchar(max) NULL,
    [NgayCapNhat] datetime2(3) NOT NULL,
    [NguoiCapNhatId] int NOT NULL,
    CONSTRAINT [PK_CauHinhHeThong] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CauHinhHeThong_TaiKhoan_NguoiCapNhatId] FOREIGN KEY ([NguoiCapNhatId]) REFERENCES [dbo].[TaiKhoan] ([Id])
);
GO


CREATE TABLE [dbo].[HoSoBTC] (
    [Id] int NOT NULL IDENTITY,
    [TaiKhoanId] int NOT NULL,
    [TenToChuc] nvarchar(200) NOT NULL,
    [NguoiDaiDien] nvarchar(120) NOT NULL,
    [MaSoThue] nvarchar(20) NULL,
    [DiaChi] nvarchar(300) NOT NULL,
    [ThongTinLienHe] nvarchar(300) NOT NULL,
    [ThongTinNhanTienMaHoa] nvarchar(max) NULL,
    [TrangThai] nvarchar(32) NOT NULL,
    [LyDoTuChoi] nvarchar(max) NULL,
    [NgayNop] datetime2(3) NULL,
    [HanDuyet] datetime2(3) NULL,
    [NguoiDuyetId] int NULL,
    [NgayDuyet] datetime2(3) NULL,
    CONSTRAINT [PK_HoSoBTC] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_HoSoBTC_DieuKien1] CHECK ([NgayNop] IS NULL OR [HanDuyet] IS NULL OR [NgayNop] <= [HanDuyet]),
    CONSTRAINT [CK_HoSoBTC_TrangThai] CHECK ([TrangThai] IN (N'Nhap', N'ChoDuyet', N'CanBoSung', N'TuChoi', N'DaDuyet')),
    CONSTRAINT [FK_HoSoBTC_TaiKhoan_NguoiDuyetId] FOREIGN KEY ([NguoiDuyetId]) REFERENCES [dbo].[TaiKhoan] ([Id]),
    CONSTRAINT [FK_HoSoBTC_TaiKhoan_TaiKhoanId] FOREIGN KEY ([TaiKhoanId]) REFERENCES [dbo].[TaiKhoan] ([Id])
);
GO


CREATE TABLE [dbo].[NhatKyHeThong] (
    [Id] int NOT NULL IDENTITY,
    [TaiKhoanId] int NULL,
    [HanhDong] nvarchar(100) NOT NULL,
    [DoiTuong] nvarchar(100) NOT NULL,
    [DoiTuongId] int NULL,
    [ThoiGian] datetime2(3) NOT NULL,
    [DuLieuThayDoi] nvarchar(max) NULL,
    CONSTRAINT [PK_NhatKyHeThong] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_NhatKyHeThong_DuLieuThayDoi] CHECK ([DuLieuThayDoi] IS NULL OR ISJSON([DuLieuThayDoi], OBJECT) = 1),
    CONSTRAINT [FK_NhatKyHeThong_TaiKhoan_TaiKhoanId] FOREIGN KEY ([TaiKhoanId]) REFERENCES [dbo].[TaiKhoan] ([Id])
);
GO


CREATE TABLE [dbo].[YeuCauThue] (
    [Id] int NOT NULL IDENTITY,
    [TaiKhoanId] int NOT NULL,
    [DiaDiemId] int NOT NULL,
    [ThoiGianBatDau] datetime2(3) NOT NULL,
    [ThoiGianKetThuc] datetime2(3) NOT NULL,
    [TongTien] decimal(19,0) NOT NULL,
    [TienCoc] decimal(19,0) NOT NULL,
    [HanDatCoc] datetime2(3) NULL,
    [ChinhSachId] int NOT NULL,
    [TrangThai] nvarchar(32) NOT NULL,
    [NguoiDuyetId] int NULL,
    [LyDoXuLy] nvarchar(max) NULL,
    [NgayHuy] datetime2(3) NULL,
    [NgayTao] datetime2(3) NULL,
    [NoiDungYeuCau] nvarchar(max) NULL,
    CONSTRAINT [PK_YeuCauThue] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_YeuCauThue_Id_DiaDiemId] UNIQUE ([Id], [DiaDiemId]),
    CONSTRAINT [CK_YeuCauThue_DieuKien1] CHECK ([ThoiGianBatDau] < [ThoiGianKetThuc]),
    CONSTRAINT [CK_YeuCauThue_DieuKien2] CHECK ([TongTien] >= 0 AND [TienCoc] BETWEEN 0 AND [TongTien]),
    CONSTRAINT [CK_YeuCauThue_HanDatCoc] CHECK ([TrangThai] <> N'ChoCoc' OR [HanDatCoc] IS NOT NULL),
    CONSTRAINT [CK_YeuCauThue_TienCoc] CHECK ([TienCoc] = ROUND([TongTien] * 0.30, 0)),
    CONSTRAINT [CK_YeuCauThue_TrangThai] CHECK ([TrangThai] IN (N'ChoDuyet', N'ChoCoc', N'DaXacNhan', N'TuChoi', N'HetHan', N'DaHuy')),
    CONSTRAINT [FK_YeuCauThue_ChinhSach_ChinhSachId] FOREIGN KEY ([ChinhSachId]) REFERENCES [dbo].[ChinhSach] ([Id]),
    CONSTRAINT [FK_YeuCauThue_DiaDiem_DiaDiemId] FOREIGN KEY ([DiaDiemId]) REFERENCES [dbo].[DiaDiem] ([Id]),
    CONSTRAINT [FK_YeuCauThue_TaiKhoan_NguoiDuyetId] FOREIGN KEY ([NguoiDuyetId]) REFERENCES [dbo].[TaiKhoan] ([Id]),
    CONSTRAINT [FK_YeuCauThue_TaiKhoan_TaiKhoanId] FOREIGN KEY ([TaiKhoanId]) REFERENCES [dbo].[TaiKhoan] ([Id])
);
GO


CREATE TABLE [dbo].[CumGhe] (
    [Id] int NOT NULL IDENTITY,
    [KhuVucId] int NOT NULL,
    [TenCum] nvarchar(120) NOT NULL,
    [LoaiCum] nvarchar(32) NOT NULL,
    [SoCho] int NOT NULL,
    CONSTRAINT [PK_CumGhe] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_CumGhe_Id_KhuVucId] UNIQUE ([Id], [KhuVucId]),
    CONSTRAINT [CK_CumGhe_DieuKien1] CHECK ([SoCho] >= 2),
    CONSTRAINT [CK_CumGhe_LoaiCum] CHECK ([LoaiCum] IN (N'GheDoi', N'GheCum')),
    CONSTRAINT [FK_CumGhe_KhuVuc_KhuVucId] FOREIGN KEY ([KhuVucId]) REFERENCES [dbo].[KhuVuc] ([Id])
);
GO


CREATE TABLE [dbo].[GiayToBTC] (
    [Id] int NOT NULL IDENTITY,
    [HoSoBTCId] int NOT NULL,
    [LoaiGiayTo] nvarchar(32) NOT NULL,
    [KhoaLuuTru] nvarchar(500) NOT NULL,
    [TenTep] nvarchar(255) NOT NULL,
    [LoaiNoiDung] nvarchar(32) NOT NULL,
    [NgayTaiLen] datetime2(3) NOT NULL,
    CONSTRAINT [PK_GiayToBTC] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GiayToBTC_HoSoBTC_HoSoBTCId] FOREIGN KEY ([HoSoBTCId]) REFERENCES [dbo].[HoSoBTC] ([Id])
);
GO


CREATE TABLE [dbo].[Ghe] (
    [Id] int NOT NULL IDENTITY,
    [KhuVucId] int NOT NULL,
    [CumGheId] int NULL,
    [MaGhe] nvarchar(32) NOT NULL,
    [LoaiGhe] nvarchar(32) NOT NULL,
    [TrangThai] nvarchar(32) NOT NULL,
    CONSTRAINT [PK_Ghe] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Ghe_LoaiGhe] CHECK ([LoaiGhe] IN (N'GheDon', N'GheDoi', N'GheCum')),
    CONSTRAINT [CK_Ghe_TrangThai] CHECK ([TrangThai] IN (N'SuDung', N'BaoTri', N'NgungSuDung')),
    CONSTRAINT [FK_Ghe_CumGhe_CumGheId_KhuVucId] FOREIGN KEY ([CumGheId], [KhuVucId]) REFERENCES [dbo].[CumGhe] ([Id], [KhuVucId]),
    CONSTRAINT [FK_Ghe_KhuVuc_KhuVucId] FOREIGN KEY ([KhuVucId]) REFERENCES [dbo].[KhuVuc] ([Id])
);
GO


CREATE TABLE [dbo].[ChiTietDonHang] (
    [Id] int NOT NULL IDENTITY,
    [DonHangId] int NOT NULL,
    [LoaiVeId] int NOT NULL,
    [SoLuong] int NOT NULL,
    [DonGia] decimal(19,0) NOT NULL,
    [ThanhTien] decimal(19,0) NOT NULL,
    CONSTRAINT [PK_ChiTietDonHang] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ChiTietDonHang_DieuKien1] CHECK ([SoLuong] > 0 AND [DonGia] >= 0 AND [ThanhTien] = [SoLuong] * [DonGia])
);
GO


CREATE TABLE [dbo].[DanhGia] (
    [Id] int NOT NULL IDENTITY,
    [TaiKhoanId] int NOT NULL,
    [SuKienId] int NOT NULL,
    [SoSao] int NOT NULL,
    [BinhLuan] nvarchar(max) NULL,
    [NgayDanhGia] datetime2(3) NOT NULL,
    [TrangThai] nvarchar(32) NOT NULL,
    CONSTRAINT [PK_DanhGia] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_DanhGia_DieuKien1] CHECK ([SoSao] BETWEEN 1 AND 5),
    CONSTRAINT [CK_DanhGia_TrangThai] CHECK ([TrangThai] IN (N'ChoDuyet', N'CongKhai', N'DaAn')),
    CONSTRAINT [FK_DanhGia_TaiKhoan_TaiKhoanId] FOREIGN KEY ([TaiKhoanId]) REFERENCES [dbo].[TaiKhoan] ([Id])
);
GO


CREATE TABLE [dbo].[DoiSoat] (
    [Id] int NOT NULL IDENTITY,
    [SuKienId] int NOT NULL,
    [PhienBan] int NOT NULL,
    [TuNgay] date NOT NULL,
    [DenNgay] date NOT NULL,
    [TongDoanhThu] decimal(19,0) NOT NULL,
    [TongTienHoan] decimal(19,0) NOT NULL,
    [PhiNenTang] decimal(19,0) NOT NULL,
    [TienDieuChinh] decimal(19,0) NOT NULL,
    [TienBTCNhan] decimal(19,0) NOT NULL,
    [ChiTietDoiSoat] nvarchar(max) NOT NULL,
    [HanXacNhan] datetime2(3) NOT NULL,
    [NguoiDuyetId] int NULL,
    [NgayDuyet] datetime2(3) NULL,
    [TrangThai] nvarchar(32) NOT NULL,
    CONSTRAINT [PK_DoiSoat] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_DoiSoat_ChiTietDoiSoat] CHECK (ISJSON([ChiTietDoiSoat], OBJECT) = 1),
    CONSTRAINT [CK_DoiSoat_DieuKien1] CHECK ([TuNgay] <= [DenNgay] AND [PhienBan] > 0),
    CONSTRAINT [CK_DoiSoat_DieuKien2] CHECK ([TongDoanhThu] >= 0 AND [TongTienHoan] >= 0 AND [PhiNenTang] >= 0 AND [TienBTCNhan] = [TongDoanhThu] - [TongTienHoan] - [PhiNenTang] - [TienDieuChinh]),
    CONSTRAINT [CK_DoiSoat_TrangThai] CHECK ([TrangThai] IN (N'ChoXacNhan', N'DaXacNhan', N'TranhChap', N'ChoGiaiNgan', N'HoanTat')),
    CONSTRAINT [FK_DoiSoat_TaiKhoan_NguoiDuyetId] FOREIGN KEY ([NguoiDuyetId]) REFERENCES [dbo].[TaiKhoan] ([Id])
);
GO


CREATE TABLE [dbo].[GiaiNgan] (
    [Id] int NOT NULL IDENTITY,
    [DoiSoatId] int NOT NULL,
    [KhoaChongLap] nvarchar(128) NOT NULL,
    [MaGiaoDich] nvarchar(128) NULL,
    [SoTien] decimal(19,0) NOT NULL,
    [TrangThai] nvarchar(32) NOT NULL,
    [NgayGiaiNgan] datetime2(3) NULL,
    [ChungTu] nvarchar(500) NULL,
    CONSTRAINT [PK_GiaiNgan] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_GiaiNgan_DieuKien1] CHECK ([SoTien] > 0),
    CONSTRAINT [CK_GiaiNgan_DieuKien2] CHECK ([TrangThai] <> N'ThanhCong' OR ([NgayGiaiNgan] IS NOT NULL AND ([MaGiaoDich] IS NOT NULL OR [ChungTu] IS NOT NULL))),
    CONSTRAINT [CK_GiaiNgan_TrangThai] CHECK ([TrangThai] IN (N'ChoChuyen', N'DangChuyen', N'ThanhCong', N'ThatBai')),
    CONSTRAINT [FK_GiaiNgan_DoiSoat_DoiSoatId] FOREIGN KEY ([DoiSoatId]) REFERENCES [dbo].[DoiSoat] ([Id])
);
GO


CREATE TABLE [dbo].[DonHang] (
    [Id] int NOT NULL IDENTITY,
    [TaiKhoanId] int NOT NULL,
    [SuatDienId] int NOT NULL,
    [HopDongId] int NULL,
    [ChinhSachId] int NOT NULL,
    [TienVe] decimal(19,0) NOT NULL,
    [TienGiam] decimal(19,0) NOT NULL,
    [PhiDichVu] decimal(19,0) NOT NULL,
    [PhiNenTang] decimal(19,0) NOT NULL,
    [TongTien] decimal(19,0) NOT NULL,
    [TrangThai] nvarchar(32) NOT NULL,
    [NgayTao] datetime2(3) NOT NULL,
    [HanThanhToan] datetime2(3) NOT NULL,
    [KhoaTaoDon] nvarchar(64) NULL,
    [DauVanTayTaoDon] nvarchar(64) NULL,
    CONSTRAINT [PK_DonHang] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_DonHang_DieuKien1] CHECK ([TienVe] >= 0 AND [TienGiam] BETWEEN 0 AND [TienVe] AND [PhiDichVu] >= 0 AND [PhiNenTang] >= 0 AND [TongTien] = [TienVe] - [TienGiam] + [PhiDichVu]),
    CONSTRAINT [CK_DonHang_DieuKien2] CHECK ([HanThanhToan] > [NgayTao]),
    CONSTRAINT [CK_DonHang_TrangThai] CHECK ([TrangThai] IN (N'ChoThanhToan', N'DaThanhToan', N'HetHan', N'DaHuy', N'DaThuTienHetCho', N'ChoHoanTien', N'HoanMotPhan', N'DaHoanTien')),
    CONSTRAINT [FK_DonHang_ChinhSach_ChinhSachId] FOREIGN KEY ([ChinhSachId]) REFERENCES [dbo].[ChinhSach] ([Id]),
    CONSTRAINT [FK_DonHang_TaiKhoan_TaiKhoanId] FOREIGN KEY ([TaiKhoanId]) REFERENCES [dbo].[TaiKhoan] ([Id])
);
GO


CREATE TABLE [dbo].[GiaoDichThanhToan] (
    [Id] int NOT NULL IDENTITY,
    [DonHangId] int NULL,
    [YeuCauThueId] int NULL,
    [MaDonNhaCungCap] nvarchar(128) NULL,
    [MaGiaoDich] nvarchar(128) NULL,
    [SoTienYeuCau] decimal(19,0) NOT NULL,
    [SoTienThucThu] decimal(19,0) NULL,
    [PhuongThuc] nvarchar(32) NOT NULL,
    [TrangThai] nvarchar(32) NOT NULL,
    [NgayThanhToan] datetime2(3) NULL,
    [ChungTu] nvarchar(500) NULL,
    CONSTRAINT [PK_GiaoDichThanhToan] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_GiaoDichThanhToan_DieuKien1] CHECK (([DonHangId] IS NULL AND [YeuCauThueId] IS NOT NULL) OR ([DonHangId] IS NOT NULL AND [YeuCauThueId] IS NULL)),
    CONSTRAINT [CK_GiaoDichThanhToan_DieuKien2] CHECK ([SoTienYeuCau] >= 0 AND ([SoTienThucThu] IS NULL OR [SoTienThucThu] >= 0)),
    CONSTRAINT [CK_GiaoDichThanhToan_PhuongThuc] CHECK ([PhuongThuc] IN (N'PayOS', N'ChuyenKhoan', N'MienPhi')),
    CONSTRAINT [CK_GiaoDichThanhToan_ThucThu] CHECK ([TrangThai] <> N'ThanhCong' OR ([SoTienThucThu] IS NOT NULL AND [SoTienThucThu] = [SoTienYeuCau] AND [NgayThanhToan] IS NOT NULL)),
    CONSTRAINT [CK_GiaoDichThanhToan_TrangThai] CHECK ([TrangThai] IN (N'ChoThanhToan', N'ThanhCong', N'ThatBai', N'HetHan', N'NghiVan', N'ThuTrungCanHoan')),
    CONSTRAINT [FK_GiaoDichThanhToan_DonHang_DonHangId] FOREIGN KEY ([DonHangId]) REFERENCES [dbo].[DonHang] ([Id]),
    CONSTRAINT [FK_GiaoDichThanhToan_YeuCauThue_YeuCauThueId] FOREIGN KEY ([YeuCauThueId]) REFERENCES [dbo].[YeuCauThue] ([Id])
);
GO


CREATE TABLE [dbo].[WebhookThanhToan] (
    [Id] int NOT NULL IDENTITY,
    [GiaoDichId] int NULL,
    [MaSuKienWebhook] nvarchar(128) NOT NULL,
    [NoiDungDaLoc] nvarchar(max) NOT NULL,
    [ThoiGianNhan] datetime2(3) NOT NULL,
    [TrangThaiXuLy] nvarchar(32) NOT NULL,
    CONSTRAINT [PK_WebhookThanhToan] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_WebhookThanhToan_NoiDungDaLoc] CHECK (ISJSON([NoiDungDaLoc], OBJECT) = 1),
    CONSTRAINT [CK_WebhookThanhToan_TrangThaiXuLy] CHECK ([TrangThaiXuLy] IN (N'ChoXuLy', N'DaXuLy', N'NghiVan', N'Loi')),
    CONSTRAINT [FK_WebhookThanhToan_GiaoDichThanhToan_GiaoDichId] FOREIGN KEY ([GiaoDichId]) REFERENCES [dbo].[GiaoDichThanhToan] ([Id])
);
GO


CREATE TABLE [dbo].[GiaoDichHoanTien] (
    [Id] int NOT NULL IDENTITY,
    [YeuCauHoanTienId] int NOT NULL,
    [KhoaChongLap] nvarchar(128) NOT NULL,
    [MaGiaoDichHoan] nvarchar(128) NULL,
    [SoTien] decimal(19,0) NOT NULL,
    [TrangThai] nvarchar(32) NOT NULL,
    [NgayXuLy] datetime2(3) NULL,
    [ChungTu] nvarchar(500) NULL,
    CONSTRAINT [PK_GiaoDichHoanTien] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_GiaoDichHoanTien_DieuKien1] CHECK ([SoTien] > 0),
    CONSTRAINT [CK_GiaoDichHoanTien_DieuKien2] CHECK ([TrangThai] <> N'ThanhCong' OR ([NgayXuLy] IS NOT NULL AND ([MaGiaoDichHoan] IS NOT NULL OR [ChungTu] IS NOT NULL))),
    CONSTRAINT [CK_GiaoDichHoanTien_TrangThai] CHECK ([TrangThai] IN (N'ChoChuyen', N'DangChuyen', N'ThanhCong', N'ThatBai'))
);
GO


CREATE TABLE [dbo].[GiuGhe] (
    [Id] int NOT NULL IDENTITY,
    [SuatDienId] int NOT NULL,
    [GheId] int NULL,
    [ChiTietDonHangId] int NOT NULL,
    [HetHan] datetime2(3) NOT NULL,
    [TrangThai] nvarchar(32) NOT NULL,
    CONSTRAINT [PK_GiuGhe] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_GiuGhe_TrangThai] CHECK ([TrangThai] IN (N'DangGiu', N'DaChot', N'DaNha', N'HetHan')),
    CONSTRAINT [FK_GiuGhe_ChiTietDonHang_ChiTietDonHangId] FOREIGN KEY ([ChiTietDonHangId]) REFERENCES [dbo].[ChiTietDonHang] ([Id]),
    CONSTRAINT [FK_GiuGhe_Ghe_GheId] FOREIGN KEY ([GheId]) REFERENCES [dbo].[Ghe] ([Id])
);
GO


CREATE TABLE [dbo].[HopDong] (
    [Id] int NOT NULL IDENTITY,
    [BTCId] int NOT NULL,
    [SuKienId] int NULL,
    [LoaiPhi] nvarchar(32) NOT NULL,
    [TyLePhi] decimal(5,2) NULL,
    [PhiMoiVe] decimal(19,0) NULL,
    [NoiDung] nvarchar(max) NOT NULL,
    [NgayKy] datetime2(3) NOT NULL,
    [HieuLucTu] datetime2(3) NOT NULL,
    [HieuLucDen] datetime2(3) NULL,
    [TrangThai] nvarchar(32) NOT NULL,
    CONSTRAINT [PK_HopDong] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_HopDong_DieuKien1] CHECK ([HieuLucDen] IS NULL OR [HieuLucTu] < [HieuLucDen]),
    CONSTRAINT [CK_HopDong_DieuKien2] CHECK (([LoaiPhi] = N'PhanTram' AND [TyLePhi] IS NOT NULL AND [TyLePhi] BETWEEN 0 AND 100 AND [PhiMoiVe] IS NULL) OR ([LoaiPhi] = N'MoiVe' AND [TyLePhi] IS NULL AND [PhiMoiVe] IS NOT NULL AND [PhiMoiVe] >= 0)),
    CONSTRAINT [CK_HopDong_TrangThai] CHECK ([TrangThai] IN (N'Nhap', N'CoHieuLuc', N'HetHan', N'DaHuy')),
    CONSTRAINT [FK_HopDong_HoSoBTC_BTCId] FOREIGN KEY ([BTCId]) REFERENCES [dbo].[HoSoBTC] ([Id])
);
GO


CREATE TABLE [dbo].[KhieuNai] (
    [Id] int NOT NULL IDENTITY,
    [TaiKhoanId] int NOT NULL,
    [SuKienId] int NULL,
    [VeId] int NULL,
    [YeuCauThueId] int NULL,
    [TieuDe] nvarchar(200) NOT NULL,
    [NoiDung] nvarchar(max) NOT NULL,
    [TrangThai] nvarchar(32) NOT NULL,
    [NguoiXuLyId] int NULL,
    [NgayTao] datetime2(3) NOT NULL,
    [KetQuaXuLy] nvarchar(max) NULL,
    CONSTRAINT [PK_KhieuNai] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_KhieuNai_TrangThai] CHECK ([TrangThai] IN (N'ChoXuLy', N'DangXuLy', N'DaGiaiQuyet', N'TuChoi')),
    CONSTRAINT [FK_KhieuNai_TaiKhoan_NguoiXuLyId] FOREIGN KEY ([NguoiXuLyId]) REFERENCES [dbo].[TaiKhoan] ([Id]),
    CONSTRAINT [FK_KhieuNai_TaiKhoan_TaiKhoanId] FOREIGN KEY ([TaiKhoanId]) REFERENCES [dbo].[TaiKhoan] ([Id]),
    CONSTRAINT [FK_KhieuNai_YeuCauThue_YeuCauThueId] FOREIGN KEY ([YeuCauThueId]) REFERENCES [dbo].[YeuCauThue] ([Id])
);
GO


CREATE TABLE [dbo].[LichDatDiaDiem] (
    [Id] int NOT NULL IDENTITY,
    [DiaDiemId] int NOT NULL,
    [YeuCauThueId] int NULL,
    [SuatDienId] int NULL,
    [BatDau] datetime2(3) NOT NULL,
    [KetThuc] datetime2(3) NOT NULL,
    [LoaiLich] nvarchar(32) NOT NULL,
    [TrangThai] nvarchar(32) NOT NULL,
    CONSTRAINT [PK_LichDatDiaDiem] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_LichDatDiaDiem_DieuKien1] CHECK ([BatDau] < [KetThuc]),
    CONSTRAINT [CK_LichDatDiaDiem_LoaiLich] CHECK ([LoaiLich] IN (N'Thue', N'SuatDien', N'BaoTri')),
    CONSTRAINT [CK_LichDatDiaDiem_TrangThai] CHECK ([TrangThai] IN (N'DangGiu', N'DaXacNhan', N'HetHan', N'DaHuy')),
    CONSTRAINT [FK_LichDatDiaDiem_DiaDiem_DiaDiemId] FOREIGN KEY ([DiaDiemId]) REFERENCES [dbo].[DiaDiem] ([Id]),
    CONSTRAINT [FK_LichDatDiaDiem_YeuCauThue_YeuCauThueId_DiaDiemId] FOREIGN KEY ([YeuCauThueId], [DiaDiemId]) REFERENCES [dbo].[YeuCauThue] ([Id], [DiaDiemId])
);
GO


CREATE TABLE [dbo].[LoaiVe] (
    [Id] int NOT NULL IDENTITY,
    [SuatDienId] int NOT NULL,
    [KhuVucId] int NOT NULL,
    [TenLoaiVe] nvarchar(120) NOT NULL,
    [NhomVe] nvarchar(32) NOT NULL,
    [GiaVe] decimal(19,0) NOT NULL,
    [SoLuong] int NOT NULL,
    [QuyenLoi] nvarchar(max) NULL,
    [BatDauBan] datetime2(3) NOT NULL,
    [KetThucBan] datetime2(3) NOT NULL,
    CONSTRAINT [PK_LoaiVe] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_LoaiVe_Id_SuatDienId] UNIQUE ([Id], [SuatDienId]),
    CONSTRAINT [CK_LoaiVe_DieuKien1] CHECK ([GiaVe] >= 0 AND [SoLuong] >= 0),
    CONSTRAINT [CK_LoaiVe_DieuKien2] CHECK ([BatDauBan] < [KetThucBan]),
    CONSTRAINT [CK_LoaiVe_DieuKien3] CHECK ([NhomVe] IN (N'TieuChuan', N'VIP', N'VeMoi') AND ([NhomVe] <> N'VeMoi' OR [GiaVe] = 0)),
    CONSTRAINT [FK_LoaiVe_KhuVuc_KhuVucId] FOREIGN KEY ([KhuVucId]) REFERENCES [dbo].[KhuVuc] ([Id])
);
GO


CREATE TABLE [dbo].[NhatKyCheckIn] (
    [Id] int NOT NULL IDENTITY,
    [VeId] int NOT NULL,
    [NhanVienId] int NOT NULL,
    [ThoiGianQuet] datetime2(3) NOT NULL,
    [KetQua] nvarchar(32) NOT NULL,
    [HinhThuc] nvarchar(32) NOT NULL,
    CONSTRAINT [PK_NhatKyCheckIn] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_NhatKyCheckIn_HinhThuc] CHECK ([HinhThuc] IN (N'QuetQR', N'ThuCong')),
    CONSTRAINT [FK_NhatKyCheckIn_TaiKhoan_NhanVienId] FOREIGN KEY ([NhanVienId]) REFERENCES [dbo].[TaiKhoan] ([Id])
);
GO


CREATE TABLE [dbo].[PhanCongSuKien] (
    [Id] int NOT NULL IDENTITY,
    [SuKienId] int NOT NULL,
    [SuatDienId] int NULL,
    [TaiKhoanId] int NULL,
    [EmailMoi] nvarchar(255) NULL,
    [MaBamLoiMoi] nvarchar(128) NULL,
    [HanLoiMoi] datetime2(3) NULL,
    [NguoiPhanCongId] int NOT NULL,
    [LoaiQuyen] nvarchar(64) NOT NULL,
    [NgayPhanCong] datetime2(3) NOT NULL,
    [NgayThuHoi] datetime2(3) NULL,
    [TrangThai] nvarchar(32) NOT NULL,
    CONSTRAINT [PK_PhanCongSuKien] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_PhanCongSuKien_DieuKien1] CHECK ([TaiKhoanId] IS NOT NULL OR NULLIF(TRIM([EmailMoi]), N'') IS NOT NULL),
    CONSTRAINT [CK_PhanCongSuKien_DieuKien2] CHECK ([TrangThai] <> N'DangHieuLuc' OR ([TaiKhoanId] IS NOT NULL AND [NgayThuHoi] IS NULL)),
    CONSTRAINT [CK_PhanCongSuKien_DieuKien3] CHECK ([NgayThuHoi] IS NULL OR [NgayThuHoi] >= [NgayPhanCong]),
    CONSTRAINT [CK_PhanCongSuKien_TrangThai] CHECK ([TrangThai] IN (N'ChoNhan', N'DangHieuLuc', N'DaThuHoi', N'HetHan', N'TuChoi')),
    CONSTRAINT [FK_PhanCongSuKien_TaiKhoan_NguoiPhanCongId] FOREIGN KEY ([NguoiPhanCongId]) REFERENCES [dbo].[TaiKhoan] ([Id]),
    CONSTRAINT [FK_PhanCongSuKien_TaiKhoan_TaiKhoanId] FOREIGN KEY ([TaiKhoanId]) REFERENCES [dbo].[TaiKhoan] ([Id])
);
GO


CREATE TABLE [dbo].[PhienBanSuKien] (
    [Id] int NOT NULL IDENTITY,
    [SuKienId] int NOT NULL,
    [SoPhienBan] int NOT NULL,
    [LoaiYeuCau] nvarchar(32) NOT NULL,
    [NoiDung] nvarchar(max) NOT NULL,
    [LyDo] nvarchar(max) NULL,
    [TrangThaiDuyet] nvarchar(32) NOT NULL,
    [NguoiGuiId] int NOT NULL,
    [NgayGui] datetime2(3) NULL,
    [NguoiDuyetId] int NULL,
    [NgayDuyet] datetime2(3) NULL,
    [KetQuaDuyet] nvarchar(max) NULL,
    CONSTRAINT [PK_PhienBanSuKien] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_PhienBanSuKien_Id_SuKienId] UNIQUE ([Id], [SuKienId]),
    CONSTRAINT [CK_PhienBanSuKien_DieuKien1] CHECK ([SoPhienBan] > 0),
    CONSTRAINT [CK_PhienBanSuKien_LoaiYeuCau] CHECK ([LoaiYeuCau] IN (N'TaoMoi', N'CapNhat', N'Hoan', N'Huy')),
    CONSTRAINT [CK_PhienBanSuKien_NoiDung] CHECK (ISJSON([NoiDung], OBJECT) = 1),
    CONSTRAINT [CK_PhienBanSuKien_TrangThaiDuyet] CHECK ([TrangThaiDuyet] IN (N'Nhap', N'ChoDuyet', N'DaDuyet', N'TuChoi')),
    CONSTRAINT [FK_PhienBanSuKien_TaiKhoan_NguoiDuyetId] FOREIGN KEY ([NguoiDuyetId]) REFERENCES [dbo].[TaiKhoan] ([Id]),
    CONSTRAINT [FK_PhienBanSuKien_TaiKhoan_NguoiGuiId] FOREIGN KEY ([NguoiGuiId]) REFERENCES [dbo].[TaiKhoan] ([Id])
);
GO


CREATE TABLE [dbo].[SuKien] (
    [Id] int NOT NULL IDENTITY,
    [BTCId] int NOT NULL,
    [DanhMucId] int NOT NULL,
    [TenSuKien] nvarchar(200) NOT NULL,
    [MoTa] nvarchar(max) NOT NULL,
    [AnhBia] nvarchar(500) NOT NULL,
    [TrangThai] nvarchar(32) NOT NULL,
    [NgayTao] datetime2(3) NOT NULL,
    [PhienBanCongKhaiId] int NULL,
    [ChoPhepTuHuyVe] bit NOT NULL,
    [HanHoanKhiHoan] datetime2(3) NULL,
    CONSTRAINT [PK_SuKien] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_SuKien_TrangThai] CHECK ([TrangThai] IN (N'Nhap', N'ChoDuyet', N'DaDuyet', N'DangMoBan', N'Hoan', N'DinhChi', N'DaHuy', N'HoanTat', N'TuChoi')),
    CONSTRAINT [FK_SuKien_DanhMucSuKien_DanhMucId] FOREIGN KEY ([DanhMucId]) REFERENCES [dbo].[DanhMucSuKien] ([Id]),
    CONSTRAINT [FK_SuKien_HoSoBTC_BTCId] FOREIGN KEY ([BTCId]) REFERENCES [dbo].[HoSoBTC] ([Id]),
    CONSTRAINT [FK_SuKien_PhienBanSuKien_PhienBanCongKhaiId_Id] FOREIGN KEY ([PhienBanCongKhaiId], [Id]) REFERENCES [dbo].[PhienBanSuKien] ([Id], [SuKienId])
);
GO


CREATE TABLE [dbo].[SuatDien] (
    [Id] int NOT NULL IDENTITY,
    [SuKienId] int NOT NULL,
    [DiaDiemId] int NOT NULL,
    [ThoiGianBatDau] datetime2(3) NOT NULL,
    [ThoiGianKetThuc] datetime2(3) NOT NULL,
    [ThoiGianMoBan] datetime2(3) NOT NULL,
    [ThoiGianDongBan] datetime2(3) NOT NULL,
    [SoDoChot] nvarchar(max) NOT NULL,
    [SucChua] int NOT NULL,
    [TrangThai] nvarchar(32) NOT NULL,
    CONSTRAINT [PK_SuatDien] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_SuatDien_Id_DiaDiemId] UNIQUE ([Id], [DiaDiemId]),
    CONSTRAINT [AK_SuatDien_Id_SuKienId] UNIQUE ([Id], [SuKienId]),
    CONSTRAINT [CK_SuatDien_DieuKien1] CHECK ([SucChua] > 0),
    CONSTRAINT [CK_SuatDien_DieuKien2] CHECK ([ThoiGianBatDau] < [ThoiGianKetThuc]),
    CONSTRAINT [CK_SuatDien_DieuKien3] CHECK ([ThoiGianMoBan] < [ThoiGianDongBan] AND [ThoiGianDongBan] <= [ThoiGianBatDau]),
    CONSTRAINT [CK_SuatDien_SoDoChot] CHECK (ISJSON([SoDoChot], OBJECT) = 1),
    CONSTRAINT [CK_SuatDien_TrangThai] CHECK ([TrangThai] IN (N'Nhap', N'DaXacNhan', N'DangMoBan', N'DongBan', N'Hoan', N'DaHuy', N'HoanTat')),
    CONSTRAINT [FK_SuatDien_DiaDiem_DiaDiemId] FOREIGN KEY ([DiaDiemId]) REFERENCES [dbo].[DiaDiem] ([Id]),
    CONSTRAINT [FK_SuatDien_SuKien_SuKienId] FOREIGN KEY ([SuKienId]) REFERENCES [dbo].[SuKien] ([Id])
);
GO


CREATE TABLE [dbo].[Ve] (
    [Id] int NOT NULL IDENTITY,
    [ChiTietDonHangId] int NULL,
    [GiuGheId] int NULL,
    [LoaiVeId] int NOT NULL,
    [SuatDienId] int NOT NULL,
    [GheId] int NULL,
    [MaVe] nvarchar(64) NOT NULL,
    [MaQR] nvarchar(128) NOT NULL,
    [TrangThai] nvarchar(32) NOT NULL,
    [NgayPhatHanh] datetime2(3) NOT NULL,
    [DaCheckInLuc] datetime2(3) NULL,
    CONSTRAINT [PK_Ve] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Ve_DieuKien1] CHECK (([TrangThai] = N'DaCheckIn' AND [DaCheckInLuc] IS NOT NULL) OR ([TrangThai] = N'DaPhatHanh' AND [DaCheckInLuc] IS NULL) OR [TrangThai] IN (N'DaHuy', N'DaThuHoi')),
    CONSTRAINT [CK_Ve_DieuKien2] CHECK ([GiuGheId] IS NULL OR [ChiTietDonHangId] IS NOT NULL),
    CONSTRAINT [CK_Ve_TrangThai] CHECK ([TrangThai] IN (N'DaPhatHanh', N'DaCheckIn', N'DaHuy', N'DaThuHoi')),
    CONSTRAINT [FK_Ve_ChiTietDonHang_ChiTietDonHangId] FOREIGN KEY ([ChiTietDonHangId]) REFERENCES [dbo].[ChiTietDonHang] ([Id]),
    CONSTRAINT [FK_Ve_Ghe_GheId] FOREIGN KEY ([GheId]) REFERENCES [dbo].[Ghe] ([Id]),
    CONSTRAINT [FK_Ve_GiuGhe_GiuGheId] FOREIGN KEY ([GiuGheId]) REFERENCES [dbo].[GiuGhe] ([Id]),
    CONSTRAINT [FK_Ve_LoaiVe_LoaiVeId_SuatDienId] FOREIGN KEY ([LoaiVeId], [SuatDienId]) REFERENCES [dbo].[LoaiVe] ([Id], [SuatDienId]),
    CONSTRAINT [FK_Ve_SuatDien_SuatDienId] FOREIGN KEY ([SuatDienId]) REFERENCES [dbo].[SuatDien] ([Id])
);
GO


CREATE TABLE [dbo].[Voucher] (
    [Id] int NOT NULL IDENTITY,
    [MaVoucher] nvarchar(64) NOT NULL,
    [SuKienId] int NULL,
    [SuatDienId] int NULL,
    [KhuVucId] int NULL,
    [LoaiGiam] nvarchar(32) NOT NULL,
    [GiaTri] decimal(19,2) NOT NULL,
    [MucGiamToiDa] decimal(19,0) NULL,
    [GiaTriDonToiThieu] decimal(19,0) NOT NULL,
    [SoLuong] int NOT NULL,
    [LuotMoiTaiKhoan] int NOT NULL,
    [NguonTaiTro] nvarchar(32) NOT NULL,
    [NgayBatDau] datetime2(3) NOT NULL,
    [NgayKetThuc] datetime2(3) NOT NULL,
    [TrangThai] nvarchar(32) NOT NULL,
    CONSTRAINT [PK_Voucher] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Voucher_DieuKien1] CHECK ([NgayBatDau] < [NgayKetThuc]),
    CONSTRAINT [CK_Voucher_DieuKien2] CHECK ([SoLuong] > 0 AND [LuotMoiTaiKhoan] BETWEEN 1 AND [SoLuong] AND [GiaTriDonToiThieu] >= 0 AND ([MucGiamToiDa] IS NULL OR [MucGiamToiDa] > 0)),
    CONSTRAINT [CK_Voucher_DieuKien3] CHECK (([LoaiGiam] = N'PhanTram' AND [GiaTri] > 0 AND [GiaTri] <= 100 AND [MucGiamToiDa] IS NOT NULL) OR ([LoaiGiam] = N'SoTien' AND [GiaTri] > 0 AND [GiaTri] = FLOOR([GiaTri]))),
    CONSTRAINT [CK_Voucher_TrangThai] CHECK ([TrangThai] IN (N'Nhap', N'ChoDuyet', N'HoatDong', N'TamDung', N'TuChoi', N'HetHan')),
    CONSTRAINT [FK_Voucher_KhuVuc_KhuVucId] FOREIGN KEY ([KhuVucId]) REFERENCES [dbo].[KhuVuc] ([Id]),
    CONSTRAINT [FK_Voucher_SuKien_SuKienId] FOREIGN KEY ([SuKienId]) REFERENCES [dbo].[SuKien] ([Id]),
    CONSTRAINT [FK_Voucher_SuatDien_SuatDienId] FOREIGN KEY ([SuatDienId]) REFERENCES [dbo].[SuatDien] ([Id])
);
GO


CREATE TABLE [dbo].[VeMoi] (
    [Id] int NOT NULL IDENTITY,
    [VeId] int NOT NULL,
    [HoTenKhachMoi] nvarchar(120) NOT NULL,
    [Email] nvarchar(255) NULL,
    [SoDienThoai] nvarchar(15) NULL,
    [TaiKhoanNhanId] int NULL,
    [MaBamNhanVe] nvarchar(128) NULL,
    [HanNhanVe] datetime2(3) NULL,
    [DaGuiLuc] datetime2(3) NULL,
    [DaNhanLuc] datetime2(3) NULL,
    [TrangThai] nvarchar(32) NOT NULL,
    CONSTRAINT [PK_VeMoi] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_VeMoi_DieuKien1] CHECK (NULLIF(TRIM([Email]), N'') IS NOT NULL OR NULLIF(TRIM([SoDienThoai]), N'') IS NOT NULL),
    CONSTRAINT [CK_VeMoi_TrangThai] CHECK ([TrangThai] IN (N'ChoGui', N'DaGui', N'DaNhan', N'DaThuHoi', N'HetHan')),
    CONSTRAINT [FK_VeMoi_TaiKhoan_TaiKhoanNhanId] FOREIGN KEY ([TaiKhoanNhanId]) REFERENCES [dbo].[TaiKhoan] ([Id]),
    CONSTRAINT [FK_VeMoi_Ve_VeId] FOREIGN KEY ([VeId]) REFERENCES [dbo].[Ve] ([Id])
);
GO


CREATE TABLE [dbo].[YeuCauHoanTien] (
    [Id] int NOT NULL IDENTITY,
    [TaiKhoanId] int NOT NULL,
    [GiaoDichId] int NOT NULL,
    [VeId] int NULL,
    [KhoaChongLap] nvarchar(128) NOT NULL,
    [SoTienYeuCau] decimal(19,0) NOT NULL,
    [SoTienDuyet] decimal(19,0) NULL,
    [LoaiYeuCau] nvarchar(32) NOT NULL,
    [LyDo] nvarchar(max) NOT NULL,
    [TrangThai] nvarchar(32) NOT NULL,
    [NgayYeuCau] datetime2(3) NOT NULL,
    [NguoiDuyetId] int NULL,
    [NgayDuyet] datetime2(3) NULL,
    CONSTRAINT [PK_YeuCauHoanTien] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_YeuCauHoanTien_DieuKien1] CHECK ([SoTienYeuCau] > 0 AND ([SoTienDuyet] IS NULL OR [SoTienDuyet] BETWEEN 1 AND [SoTienYeuCau])),
    CONSTRAINT [CK_YeuCauHoanTien_TrangThai] CHECK ([TrangThai] IN (N'ChoDuyet', N'DaDuyet', N'TuChoi', N'DangChuyen', N'HoanTat', N'ThatBai')),
    CONSTRAINT [FK_YeuCauHoanTien_GiaoDichThanhToan_GiaoDichId] FOREIGN KEY ([GiaoDichId]) REFERENCES [dbo].[GiaoDichThanhToan] ([Id]),
    CONSTRAINT [FK_YeuCauHoanTien_TaiKhoan_NguoiDuyetId] FOREIGN KEY ([NguoiDuyetId]) REFERENCES [dbo].[TaiKhoan] ([Id]),
    CONSTRAINT [FK_YeuCauHoanTien_TaiKhoan_TaiKhoanId] FOREIGN KEY ([TaiKhoanId]) REFERENCES [dbo].[TaiKhoan] ([Id]),
    CONSTRAINT [FK_YeuCauHoanTien_Ve_VeId] FOREIGN KEY ([VeId]) REFERENCES [dbo].[Ve] ([Id])
);
GO


CREATE TABLE [dbo].[SuDungVoucher] (
    [Id] int NOT NULL IDENTITY,
    [VoucherId] int NOT NULL,
    [DonHangId] int NOT NULL,
    [SoTienGiam] decimal(19,0) NOT NULL,
    [HetHanGiuLuot] datetime2(3) NOT NULL,
    [DaChotLuc] datetime2(3) NULL,
    [TrangThai] nvarchar(32) NOT NULL,
    CONSTRAINT [PK_SuDungVoucher] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_SuDungVoucher_DieuKien1] CHECK ([SoTienGiam] >= 0),
    CONSTRAINT [CK_SuDungVoucher_DieuKien2] CHECK (([TrangThai] = N'DaSuDung' AND [DaChotLuc] IS NOT NULL) OR ([TrangThai] <> N'DaSuDung' AND [DaChotLuc] IS NULL)),
    CONSTRAINT [CK_SuDungVoucher_TrangThai] CHECK ([TrangThai] IN (N'DangGiu', N'DaSuDung', N'DaNha')),
    CONSTRAINT [FK_SuDungVoucher_DonHang_DonHangId] FOREIGN KEY ([DonHangId]) REFERENCES [dbo].[DonHang] ([Id]),
    CONSTRAINT [FK_SuDungVoucher_Voucher_VoucherId] FOREIGN KEY ([VoucherId]) REFERENCES [dbo].[Voucher] ([Id])
);
GO


CREATE TABLE [dbo].[ThongBao] (
    [Id] int NOT NULL IDENTITY,
    [TaiKhoanId] int NULL,
    [VeMoiId] int NULL,
    [TieuDe] nvarchar(200) NOT NULL,
    [NoiDung] nvarchar(max) NOT NULL,
    [LoaiThongBao] nvarchar(32) NOT NULL,
    [KenhGui] nvarchar(32) NOT NULL,
    [TrangThaiGui] nvarchar(32) NOT NULL,
    [DaDoc] bit NOT NULL,
    [NgayTao] datetime2(3) NOT NULL,
    [NgayGui] datetime2(3) NULL,
    CONSTRAINT [PK_ThongBao] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ThongBao_DieuKien1] CHECK (([TaiKhoanId] IS NULL AND [VeMoiId] IS NOT NULL) OR ([TaiKhoanId] IS NOT NULL AND [VeMoiId] IS NULL)),
    CONSTRAINT [CK_ThongBao_KenhGui] CHECK ([KenhGui] IN (N'TrongUngDung', N'Email', N'SMS')),
    CONSTRAINT [CK_ThongBao_TrangThaiGui] CHECK ([TrangThaiGui] IN (N'ChoGui', N'DangGui', N'DaGui', N'LoiGui')),
    CONSTRAINT [FK_ThongBao_TaiKhoan_TaiKhoanId] FOREIGN KEY ([TaiKhoanId]) REFERENCES [dbo].[TaiKhoan] ([Id]),
    CONSTRAINT [FK_ThongBao_VeMoi_VeMoiId] FOREIGN KEY ([VeMoiId]) REFERENCES [dbo].[VeMoi] ([Id])
);
GO


CREATE UNIQUE INDEX [IX_CauHinhHeThong_Khoa] ON [dbo].[CauHinhHeThong] ([Khoa]);
GO


CREATE INDEX [IX_CauHinhHeThong_NguoiCapNhatId] ON [dbo].[CauHinhHeThong] ([NguoiCapNhatId]);
GO


CREATE UNIQUE INDEX [IX_ChinhSach_PhienBan] ON [dbo].[ChinhSach] ([TenChinhSach], [PhienBan]);
GO


CREATE INDEX [IX_ChiTietDonHang_DonHangId] ON [dbo].[ChiTietDonHang] ([DonHangId]);
GO


CREATE INDEX [IX_ChiTietDonHang_LoaiVeId] ON [dbo].[ChiTietDonHang] ([LoaiVeId]);
GO


CREATE INDEX [IX_CumGhe_KhuVucId] ON [dbo].[CumGhe] ([KhuVucId]);
GO


CREATE INDEX [IX_DanhGia_SuKienId] ON [dbo].[DanhGia] ([SuKienId]);
GO


CREATE UNIQUE INDEX [IX_DanhGia_TaiKhoanSuKien] ON [dbo].[DanhGia] ([TaiKhoanId], [SuKienId]);
GO


CREATE UNIQUE INDEX [IX_DoiSoat_KyChiTra] ON [dbo].[DoiSoat] ([SuKienId], [TuNgay], [DenNgay]) WHERE [TrangThai] IN (N'ChoGiaiNgan', N'HoanTat');
GO


CREATE INDEX [IX_DoiSoat_NguoiDuyetId] ON [dbo].[DoiSoat] ([NguoiDuyetId]);
GO


CREATE UNIQUE INDEX [IX_DoiSoat_PhienBan] ON [dbo].[DoiSoat] ([SuKienId], [TuNgay], [DenNgay], [PhienBan]);
GO


CREATE INDEX [IX_DonHang_ChinhSachId] ON [dbo].[DonHang] ([ChinhSachId]);
GO


CREATE INDEX [IX_DonHang_HanMuc] ON [dbo].[DonHang] ([SuatDienId], [TaiKhoanId], [TrangThai]);
GO


CREATE INDEX [IX_DonHang_HopDongId] ON [dbo].[DonHang] ([HopDongId]);
GO


CREATE INDEX [IX_DonHang_TaiKhoanId] ON [dbo].[DonHang] ([TaiKhoanId]);
GO


CREATE INDEX [IX_Ghe_CumGheId_KhuVucId] ON [dbo].[Ghe] ([CumGheId], [KhuVucId]);
GO


CREATE UNIQUE INDEX [IX_Ghe_MaTheoKhuVuc] ON [dbo].[Ghe] ([KhuVucId], [MaGhe]);
GO


CREATE UNIQUE INDEX [IX_GiaiNgan_ChongLap] ON [dbo].[GiaiNgan] ([KhoaChongLap]);
GO


CREATE UNIQUE INDEX [IX_GiaiNgan_DoiSoat] ON [dbo].[GiaiNgan] ([DoiSoatId]);
GO


CREATE UNIQUE INDEX [IX_GiaiNgan_MaGiaoDich] ON [dbo].[GiaiNgan] ([MaGiaoDich]) WHERE [MaGiaoDich] IS NOT NULL;
GO


CREATE UNIQUE INDEX [IX_GiaoDichHoanTien_ChongLap] ON [dbo].[GiaoDichHoanTien] ([KhoaChongLap]);
GO


CREATE UNIQUE INDEX [IX_GiaoDichHoanTien_MaGiaoDich] ON [dbo].[GiaoDichHoanTien] ([MaGiaoDichHoan]) WHERE [MaGiaoDichHoan] IS NOT NULL;
GO


CREATE INDEX [IX_GiaoDichHoanTien_YeuCauHoanTienId] ON [dbo].[GiaoDichHoanTien] ([YeuCauHoanTienId]);
GO


CREATE UNIQUE INDEX [IX_GiaoDichThanhToan_ChotCoc] ON [dbo].[GiaoDichThanhToan] ([YeuCauThueId]) WHERE [YeuCauThueId] IS NOT NULL AND [TrangThai] = N'ThanhCong';
GO


CREATE UNIQUE INDEX [IX_GiaoDichThanhToan_ChotDon] ON [dbo].[GiaoDichThanhToan] ([DonHangId]) WHERE [DonHangId] IS NOT NULL AND [TrangThai] = N'ThanhCong';
GO


CREATE UNIQUE INDEX [IX_GiaoDichThanhToan_MaDon] ON [dbo].[GiaoDichThanhToan] ([MaDonNhaCungCap]) WHERE [MaDonNhaCungCap] IS NOT NULL;
GO


CREATE UNIQUE INDEX [IX_GiaoDichThanhToan_MaGiaoDich] ON [dbo].[GiaoDichThanhToan] ([MaGiaoDich]) WHERE [MaGiaoDich] IS NOT NULL;
GO


CREATE INDEX [IX_GiayToBTC_HoSoBTCId] ON [dbo].[GiayToBTC] ([HoSoBTCId]);
GO


CREATE INDEX [IX_GiuGhe_ChiTietDonHangId] ON [dbo].[GiuGhe] ([ChiTietDonHangId]);
GO


CREATE UNIQUE INDEX [IX_GiuGhe_GheDangGiu] ON [dbo].[GiuGhe] ([SuatDienId], [GheId]) WHERE [GheId] IS NOT NULL AND [TrangThai] = N'DangGiu';
GO


CREATE INDEX [IX_GiuGhe_GheId] ON [dbo].[GiuGhe] ([GheId]);
GO


CREATE INDEX [IX_GiuGhe_HetHan] ON [dbo].[GiuGhe] ([TrangThai], [HetHan]);
GO


CREATE INDEX [IX_HopDong_HieuLuc] ON [dbo].[HopDong] ([BTCId], [SuKienId], [HieuLucTu], [HieuLucDen]);
GO


CREATE INDEX [IX_HopDong_SuKienId] ON [dbo].[HopDong] ([SuKienId]);
GO


CREATE INDEX [IX_HoSoBTC_NguoiDuyetId] ON [dbo].[HoSoBTC] ([NguoiDuyetId]);
GO


CREATE INDEX [IX_HoSoBTC_TaiKhoanId] ON [dbo].[HoSoBTC] ([TaiKhoanId]);
GO


CREATE INDEX [IX_KhieuNai_NguoiXuLyId] ON [dbo].[KhieuNai] ([NguoiXuLyId]);
GO


CREATE INDEX [IX_KhieuNai_SuKienId] ON [dbo].[KhieuNai] ([SuKienId]);
GO


CREATE INDEX [IX_KhieuNai_TaiKhoanId] ON [dbo].[KhieuNai] ([TaiKhoanId]);
GO


CREATE INDEX [IX_KhieuNai_VeId] ON [dbo].[KhieuNai] ([VeId]);
GO


CREATE INDEX [IX_KhieuNai_YeuCauThueId] ON [dbo].[KhieuNai] ([YeuCauThueId]);
GO


CREATE UNIQUE INDEX [IX_KhuVuc_TenTheoDiaDiem] ON [dbo].[KhuVuc] ([DiaDiemId], [TenKhuVuc]);
GO


CREATE INDEX [IX_LichDatDiaDiem_SuatDienId_DiaDiemId] ON [dbo].[LichDatDiaDiem] ([SuatDienId], [DiaDiemId]);
GO


CREATE INDEX [IX_LichDatDiaDiem_TraLich] ON [dbo].[LichDatDiaDiem] ([DiaDiemId], [TrangThai], [BatDau], [KetThuc]);
GO


CREATE INDEX [IX_LichDatDiaDiem_YeuCauThueId_DiaDiemId] ON [dbo].[LichDatDiaDiem] ([YeuCauThueId], [DiaDiemId]);
GO


CREATE INDEX [IX_LoaiVe_KhuVucId] ON [dbo].[LoaiVe] ([KhuVucId]);
GO


CREATE INDEX [IX_LoaiVe_SuatDienId] ON [dbo].[LoaiVe] ([SuatDienId]);
GO


CREATE INDEX [IX_NhatKyCheckIn_NhanVienId] ON [dbo].[NhatKyCheckIn] ([NhanVienId]);
GO


CREATE UNIQUE INDEX [IX_NhatKyCheckIn_ThanhCong] ON [dbo].[NhatKyCheckIn] ([VeId]) WHERE [KetQua] = N'ThanhCong';
GO


CREATE INDEX [IX_NhatKyHeThong_TaiKhoanId] ON [dbo].[NhatKyHeThong] ([TaiKhoanId]);
GO


CREATE INDEX [IX_PhanCongSuKien_NguoiPhanCongId] ON [dbo].[PhanCongSuKien] ([NguoiPhanCongId]);
GO


CREATE INDEX [IX_PhanCongSuKien_SuatDienId_SuKienId] ON [dbo].[PhanCongSuKien] ([SuatDienId], [SuKienId]);
GO


CREATE INDEX [IX_PhanCongSuKien_SuKienId] ON [dbo].[PhanCongSuKien] ([SuKienId]);
GO


CREATE UNIQUE INDEX [IX_PhanCongSuKien_TheoSuat] ON [dbo].[PhanCongSuKien] ([TaiKhoanId], [SuKienId], [SuatDienId], [LoaiQuyen]) WHERE [TaiKhoanId] IS NOT NULL AND [SuatDienId] IS NOT NULL AND [TrangThai] = N'DangHieuLuc';
GO


CREATE UNIQUE INDEX [IX_PhanCongSuKien_ToanSuKien] ON [dbo].[PhanCongSuKien] ([TaiKhoanId], [SuKienId], [LoaiQuyen]) WHERE [TaiKhoanId] IS NOT NULL AND [SuatDienId] IS NULL AND [TrangThai] = N'DangHieuLuc';
GO


CREATE INDEX [IX_PhanCongSuKien_TraQuyen] ON [dbo].[PhanCongSuKien] ([TaiKhoanId], [SuKienId], [SuatDienId], [LoaiQuyen], [TrangThai]);
GO


CREATE INDEX [IX_PhienBanSuKien_NguoiDuyetId] ON [dbo].[PhienBanSuKien] ([NguoiDuyetId]);
GO


CREATE INDEX [IX_PhienBanSuKien_NguoiGuiId] ON [dbo].[PhienBanSuKien] ([NguoiGuiId]);
GO


CREATE UNIQUE INDEX [IX_PhienBanSuKien_SoPhienBan] ON [dbo].[PhienBanSuKien] ([SuKienId], [SoPhienBan]);
GO


CREATE INDEX [IX_SuatDien_DiaDiemId] ON [dbo].[SuatDien] ([DiaDiemId]);
GO


CREATE INDEX [IX_SuatDien_LichSuKien] ON [dbo].[SuatDien] ([SuKienId], [ThoiGianBatDau]);
GO


CREATE INDEX [IX_SuDungVoucher_DemLuot] ON [dbo].[SuDungVoucher] ([VoucherId], [TrangThai], [HetHanGiuLuot]);
GO


CREATE UNIQUE INDEX [IX_SuDungVoucher_DonHang] ON [dbo].[SuDungVoucher] ([DonHangId]);
GO


CREATE INDEX [IX_SuKien_BTCId] ON [dbo].[SuKien] ([BTCId]);
GO


CREATE INDEX [IX_SuKien_DanhMucId] ON [dbo].[SuKien] ([DanhMucId]);
GO


CREATE INDEX [IX_SuKien_PhienBanCongKhaiId_Id] ON [dbo].[SuKien] ([PhienBanCongKhaiId], [Id]);
GO


CREATE UNIQUE INDEX [IX_TaiKhoan_Email] ON [dbo].[TaiKhoan] ([Email]) WHERE [Email] IS NOT NULL;
GO


CREATE UNIQUE INDEX [IX_TaiKhoan_SoDienThoai] ON [dbo].[TaiKhoan] ([SoDienThoai]) WHERE [SoDienThoai] IS NOT NULL;
GO


CREATE UNIQUE INDEX [IX_TaiKhoan_TenDangNhap] ON [dbo].[TaiKhoan] ([TenDangNhap]);
GO


CREATE INDEX [IX_ThongBao_HopThu] ON [dbo].[ThongBao] ([TaiKhoanId], [DaDoc], [NgayTao]);
GO


CREATE INDEX [IX_ThongBao_VeMoiId] ON [dbo].[ThongBao] ([VeMoiId]);
GO


CREATE UNIQUE INDEX [IX_Ve_GheDaCap] ON [dbo].[Ve] ([SuatDienId], [GheId]) WHERE [GheId] IS NOT NULL AND [TrangThai] IN (N'DaPhatHanh', N'DaCheckIn');
GO


CREATE INDEX [IX_Ve_GheId] ON [dbo].[Ve] ([GheId]);
GO


CREATE UNIQUE INDEX [IX_Ve_GiuGhe] ON [dbo].[Ve] ([GiuGheId]) WHERE [GiuGheId] IS NOT NULL;
GO


CREATE INDEX [IX_Ve_HanMuc] ON [dbo].[Ve] ([ChiTietDonHangId], [SuatDienId], [TrangThai]);
GO


CREATE INDEX [IX_Ve_LoaiVeId_SuatDienId] ON [dbo].[Ve] ([LoaiVeId], [SuatDienId]);
GO


CREATE UNIQUE INDEX [IX_Ve_MaQR] ON [dbo].[Ve] ([MaQR]);
GO


CREATE UNIQUE INDEX [IX_Ve_MaVe] ON [dbo].[Ve] ([MaVe]);
GO


CREATE INDEX [IX_VeMoi_TaiKhoanNhanId] ON [dbo].[VeMoi] ([TaiKhoanNhanId]);
GO


CREATE UNIQUE INDEX [IX_VeMoi_Ve] ON [dbo].[VeMoi] ([VeId]);
GO


CREATE INDEX [IX_Voucher_KhuVucId] ON [dbo].[Voucher] ([KhuVucId]);
GO


CREATE UNIQUE INDEX [IX_Voucher_Ma] ON [dbo].[Voucher] ([MaVoucher]);
GO


CREATE INDEX [IX_Voucher_SuatDienId] ON [dbo].[Voucher] ([SuatDienId]);
GO


CREATE INDEX [IX_Voucher_SuKienId] ON [dbo].[Voucher] ([SuKienId]);
GO


CREATE INDEX [IX_WebhookThanhToan_GiaoDichId] ON [dbo].[WebhookThanhToan] ([GiaoDichId]);
GO


CREATE UNIQUE INDEX [IX_WebhookThanhToan_MaSuKien] ON [dbo].[WebhookThanhToan] ([MaSuKienWebhook]);
GO


CREATE UNIQUE INDEX [IX_YeuCauHoanTien_ChongLap] ON [dbo].[YeuCauHoanTien] ([KhoaChongLap]);
GO


CREATE INDEX [IX_YeuCauHoanTien_GiaoDichId] ON [dbo].[YeuCauHoanTien] ([GiaoDichId]);
GO


CREATE INDEX [IX_YeuCauHoanTien_NguoiDuyetId] ON [dbo].[YeuCauHoanTien] ([NguoiDuyetId]);
GO


CREATE INDEX [IX_YeuCauHoanTien_TaiKhoanId] ON [dbo].[YeuCauHoanTien] ([TaiKhoanId]);
GO


CREATE INDEX [IX_YeuCauHoanTien_VeId] ON [dbo].[YeuCauHoanTien] ([VeId]);
GO


CREATE INDEX [IX_YeuCauThue_ChinhSachId] ON [dbo].[YeuCauThue] ([ChinhSachId]);
GO


CREATE INDEX [IX_YeuCauThue_DiaDiemId] ON [dbo].[YeuCauThue] ([DiaDiemId]);
GO


CREATE INDEX [IX_YeuCauThue_NguoiDuyetId] ON [dbo].[YeuCauThue] ([NguoiDuyetId]);
GO


CREATE INDEX [IX_YeuCauThue_TaiKhoanId] ON [dbo].[YeuCauThue] ([TaiKhoanId]);
GO


ALTER TABLE [dbo].[ChiTietDonHang] ADD CONSTRAINT [FK_ChiTietDonHang_DonHang_DonHangId] FOREIGN KEY ([DonHangId]) REFERENCES [dbo].[DonHang] ([Id]);
GO


ALTER TABLE [dbo].[ChiTietDonHang] ADD CONSTRAINT [FK_ChiTietDonHang_LoaiVe_LoaiVeId] FOREIGN KEY ([LoaiVeId]) REFERENCES [dbo].[LoaiVe] ([Id]);
GO


ALTER TABLE [dbo].[DanhGia] ADD CONSTRAINT [FK_DanhGia_SuKien_SuKienId] FOREIGN KEY ([SuKienId]) REFERENCES [dbo].[SuKien] ([Id]);
GO


ALTER TABLE [dbo].[DoiSoat] ADD CONSTRAINT [FK_DoiSoat_SuKien_SuKienId] FOREIGN KEY ([SuKienId]) REFERENCES [dbo].[SuKien] ([Id]);
GO


ALTER TABLE [dbo].[DonHang] ADD CONSTRAINT [FK_DonHang_HopDong_HopDongId] FOREIGN KEY ([HopDongId]) REFERENCES [dbo].[HopDong] ([Id]);
GO


ALTER TABLE [dbo].[DonHang] ADD CONSTRAINT [FK_DonHang_SuatDien_SuatDienId] FOREIGN KEY ([SuatDienId]) REFERENCES [dbo].[SuatDien] ([Id]);
GO


ALTER TABLE [dbo].[GiaoDichHoanTien] ADD CONSTRAINT [FK_GiaoDichHoanTien_YeuCauHoanTien_YeuCauHoanTienId] FOREIGN KEY ([YeuCauHoanTienId]) REFERENCES [dbo].[YeuCauHoanTien] ([Id]);
GO


ALTER TABLE [dbo].[GiuGhe] ADD CONSTRAINT [FK_GiuGhe_SuatDien_SuatDienId] FOREIGN KEY ([SuatDienId]) REFERENCES [dbo].[SuatDien] ([Id]);
GO


ALTER TABLE [dbo].[HopDong] ADD CONSTRAINT [FK_HopDong_SuKien_SuKienId] FOREIGN KEY ([SuKienId]) REFERENCES [dbo].[SuKien] ([Id]);
GO


ALTER TABLE [dbo].[KhieuNai] ADD CONSTRAINT [FK_KhieuNai_SuKien_SuKienId] FOREIGN KEY ([SuKienId]) REFERENCES [dbo].[SuKien] ([Id]);
GO


ALTER TABLE [dbo].[KhieuNai] ADD CONSTRAINT [FK_KhieuNai_Ve_VeId] FOREIGN KEY ([VeId]) REFERENCES [dbo].[Ve] ([Id]);
GO


ALTER TABLE [dbo].[LichDatDiaDiem] ADD CONSTRAINT [FK_LichDatDiaDiem_SuatDien_SuatDienId_DiaDiemId] FOREIGN KEY ([SuatDienId], [DiaDiemId]) REFERENCES [dbo].[SuatDien] ([Id], [DiaDiemId]);
GO


ALTER TABLE [dbo].[LoaiVe] ADD CONSTRAINT [FK_LoaiVe_SuatDien_SuatDienId] FOREIGN KEY ([SuatDienId]) REFERENCES [dbo].[SuatDien] ([Id]);
GO


ALTER TABLE [dbo].[NhatKyCheckIn] ADD CONSTRAINT [FK_NhatKyCheckIn_Ve_VeId] FOREIGN KEY ([VeId]) REFERENCES [dbo].[Ve] ([Id]);
GO


ALTER TABLE [dbo].[PhanCongSuKien] ADD CONSTRAINT [FK_PhanCongSuKien_SuKien_SuKienId] FOREIGN KEY ([SuKienId]) REFERENCES [dbo].[SuKien] ([Id]);
GO


ALTER TABLE [dbo].[PhanCongSuKien] ADD CONSTRAINT [FK_PhanCongSuKien_SuatDien_SuatDienId_SuKienId] FOREIGN KEY ([SuatDienId], [SuKienId]) REFERENCES [dbo].[SuatDien] ([Id], [SuKienId]);
GO


ALTER TABLE [dbo].[PhienBanSuKien] ADD CONSTRAINT [FK_PhienBanSuKien_SuKien_SuKienId] FOREIGN KEY ([SuKienId]) REFERENCES [dbo].[SuKien] ([Id]);
GO


