CREATE TABLE VaiTro (
    Id   INT            IDENTITY PRIMARY KEY,
    Ten  NVARCHAR (40)  NOT NULL UNIQUE,
    MoTa NVARCHAR (200) DEFAULT '' NOT NULL
);

SET IDENTITY_INSERT VaiTro ON;

INSERT  VaiTro (
    Id,
    Ten,
    MoTa
)
VALUES         (1, N'NguoiDung', N'Người dùng'),
               (2, N'BanToChuc', N'Ban tổ chức'),
               (3, N'QuanLySuKien', N'Quản lý sự kiện'),
               (4, N'QuanTriVien', N'Quản trị viên');

SET IDENTITY_INSERT VaiTro OFF;

CREATE TABLE TaiKhoan (
    Id         INT            IDENTITY PRIMARY KEY,
    HoTen      NVARCHAR (120) NOT NULL,
    Email      NVARCHAR (200) NOT NULL,
    MatKhau    NVARCHAR (500) NOT NULL,
    VaiTroId   INT            NOT NULL,
    HoatDong   BIT            DEFAULT 1 NOT NULL,
    NgayTao    DATETIME2      NOT NULL,
    RowVersion ROWVERSION     NOT NULL,
    CONSTRAINT FK_TaiKhoan_VaiTro FOREIGN KEY (VaiTroId) REFERENCES VaiTro (Id)
);

CREATE UNIQUE INDEX UX_TaiKhoan_Email
    ON TaiKhoan(Email);

CREATE TABLE YeuCauXacThucToChuc (
    Id          INT            IDENTITY PRIMARY KEY,
    TaiKhoanId  INT            NOT NULL,
    TenToChuc   NVARCHAR (200) NOT NULL,
    GiayTo      NVARCHAR (500) NOT NULL,
    LyDo        NVARCHAR (500) NULL,
    TrangThai   INT            NOT NULL,
    NgayTao     DATETIME2      NOT NULL,
    NgayXuLy    DATETIME2      NULL,
    NguoiXuLyId INT            NULL,
    FOREIGN KEY (TaiKhoanId) REFERENCES TaiKhoan (Id) ON DELETE CASCADE,
    FOREIGN KEY (NguoiXuLyId) REFERENCES TaiKhoan (Id)
);

CREATE UNIQUE INDEX UX_YeuCau_TaiKhoan
    ON YeuCauXacThucToChuc(TaiKhoanId);

CREATE TABLE LichSuYeuCauXacThucToChuc (
    Id          INT            IDENTITY PRIMARY KEY,
    TaiKhoanId  INT            NOT NULL,
    TenToChuc   NVARCHAR (200) NOT NULL,
    GiayTo      NVARCHAR (500) NOT NULL,
    LyDo        NVARCHAR (500) NULL,
    TrangThai   INT            NOT NULL,
    NgayTao     DATETIME2      NOT NULL,
    NgayXuLy    DATETIME2      NOT NULL,
    NguoiXuLyId INT            NULL,
    FOREIGN KEY (TaiKhoanId) REFERENCES TaiKhoan (Id),
    FOREIGN KEY (NguoiXuLyId) REFERENCES TaiKhoan (Id)
);

CREATE TABLE BanToChuc (
    Id          INT             IDENTITY PRIMARY KEY,
    TaiKhoanId  INT             NOT NULL UNIQUE,
    Ten         NVARCHAR (200)  NOT NULL,
    MoTa        NVARCHAR (1000) DEFAULT '' NOT NULL,
    NgayXacThuc DATETIME2       NOT NULL,
    FOREIGN KEY (TaiKhoanId) REFERENCES TaiKhoan (Id)
);

CREATE TABLE LoaiSuKien (
    Id       INT            IDENTITY PRIMARY KEY,
    Ten      NVARCHAR (120) NOT NULL UNIQUE,
    MoTa     NVARCHAR (500) DEFAULT '' NOT NULL,
    HoatDong BIT            DEFAULT 1 NOT NULL
);

CREATE TABLE SuKien (
    Id           INT             IDENTITY PRIMARY KEY,
    BanToChucId  INT             NOT NULL,
    LoaiSuKienId INT             NOT NULL,
    Ten          NVARCHAR (200)  NOT NULL,
    MoTa         NVARCHAR (4000) DEFAULT '' NOT NULL,
    BatDau       DATETIME2       NOT NULL,
    KetThuc      DATETIME2       NOT NULL,
    SoNguoi      INT             NOT NULL,
    TrangThai    INT             NOT NULL,
    NgayTao      DATETIME2       NOT NULL,
    RowVersion   ROWVERSION      NOT NULL,
    CONSTRAINT CK_SuKien_ThoiGian CHECK (KetThuc > BatDau),
    CONSTRAINT CK_SuKien_SoNguoi CHECK (SoNguoi > 0),
    FOREIGN KEY (BanToChucId) REFERENCES BanToChuc (Id),
    FOREIGN KEY (LoaiSuKienId) REFERENCES LoaiSuKien (Id)
);

CREATE INDEX IX_SuKien_TrangThai_BatDau
    ON SuKien(TrangThai, BatDau);

CREATE TABLE DiaDiem (
    Id         INT             IDENTITY PRIMARY KEY,
    Ten        NVARCHAR (200)  NOT NULL,
    DiaChi     NVARCHAR (300)  NOT NULL,
    SucChua    INT             NOT NULL,
    GiaThue    DECIMAL (18, 2) NOT NULL,
    HoatDong   BIT             DEFAULT 1 NOT NULL,
    RowVersion ROWVERSION      NOT NULL,
    CONSTRAINT CK_DiaDiem_SucChua CHECK (SucChua > 0),
    CONSTRAINT CK_DiaDiem_GiaThue CHECK (GiaThue >= 0)
);

CREATE UNIQUE INDEX UX_DiaDiem_Ten_DiaChi
    ON DiaDiem(Ten, DiaChi);

CREATE TABLE DatDiaDiem (
    Id         INT             IDENTITY PRIMARY KEY,
    DiaDiemId  INT             NOT NULL,
    SuKienId   INT             NOT NULL UNIQUE,
    BatDau     DATETIME2       NOT NULL,
    KetThuc    DATETIME2       NOT NULL,
    TienCoc    DECIMAL (18, 2) NOT NULL,
    TongTien   DECIMAL (18, 2) NOT NULL,
    TrangThai  INT             NOT NULL,
    NgayTao    DATETIME2       NOT NULL,
    RowVersion ROWVERSION      NOT NULL,
    CONSTRAINT CK_DatDiaDiem_ThoiGian CHECK (KetThuc > BatDau),
    CONSTRAINT CK_DatDiaDiem_Tien CHECK (TienCoc >= 0
                                         AND TienCoc <= TongTien),
    FOREIGN KEY (DiaDiemId) REFERENCES DiaDiem (Id),
    FOREIGN KEY (SuKienId) REFERENCES SuKien (Id)
);

CREATE INDEX IX_DatDiaDiem_Lich
    ON DatDiaDiem(DiaDiemId, BatDau, KetThuc);

CREATE TABLE LoaiVe (
    Id         INT             IDENTITY PRIMARY KEY,
    SuKienId   INT             NOT NULL,
    Ten        NVARCHAR (120)  NOT NULL,
    Gia        DECIMAL (18, 2) NOT NULL,
    SoLuong    INT             NOT NULL,
    DaBan      INT             DEFAULT 0 NOT NULL,
    DangGiu    INT             DEFAULT 0 NOT NULL,
    MoBan      DATETIME2       NOT NULL,
    DongBan    DATETIME2       NOT NULL,
    RowVersion ROWVERSION      NOT NULL,
    CONSTRAINT CK_LoaiVe CHECK (Gia >= 0
                                AND SoLuong > 0
                                AND DongBan > MoBan
                                AND DaBan >= 0
                                AND DangGiu >= 0
                                AND DaBan + DangGiu <= SoLuong),
    FOREIGN KEY (SuKienId) REFERENCES SuKien (Id)
);

CREATE UNIQUE INDEX UX_LoaiVe_SuKien_Ten
    ON LoaiVe(SuKienId, Ten);

CREATE TABLE DonVe (
    Id         INT             IDENTITY PRIMARY KEY,
    TaiKhoanId INT             NOT NULL,
    SuKienId   INT             NOT NULL,
    SoLuong    INT             NOT NULL,
    TongTien   DECIMAL (18, 2) NOT NULL,
    TrangThai  INT             NOT NULL,
    NgayTao    DATETIME2       NOT NULL,
    HetHan     DATETIME2       NULL,
    RowVersion ROWVERSION      NOT NULL,
    CONSTRAINT CK_DonVe CHECK (SoLuong > 0
                               AND TongTien >= 0),
    FOREIGN KEY (TaiKhoanId) REFERENCES TaiKhoan (Id),
    FOREIGN KEY (SuKienId) REFERENCES SuKien (Id)
);

CREATE INDEX IX_DonVe_User
    ON DonVe(TaiKhoanId, NgayTao);

CREATE INDEX IX_DonVe_Expire
    ON DonVe(TrangThai, HetHan);

CREATE TABLE ChiTietDonVe (
    Id        INT             IDENTITY PRIMARY KEY,
    DonVeId   INT             NOT NULL,
    LoaiVeId  INT             NOT NULL,
    SoLuong   INT             NOT NULL,
    DonGia    DECIMAL (18, 2) NOT NULL,
    ThanhTien DECIMAL (18, 2) NOT NULL,
    CONSTRAINT CK_ChiTiet CHECK (SoLuong > 0
                                 AND DonGia >= 0
                                 AND ThanhTien >= 0),
    FOREIGN KEY (DonVeId) REFERENCES DonVe (Id) ON DELETE CASCADE,
    FOREIGN KEY (LoaiVeId) REFERENCES LoaiVe (Id)
);

CREATE UNIQUE INDEX UX_ChiTiet_Don_Loai
    ON ChiTietDonVe(DonVeId, LoaiVeId);

CREATE TABLE ThanhToan (
    Id            INT             IDENTITY PRIMARY KEY,
    DonVeId       INT             NOT NULL,
    SoTien        DECIMAL (18, 2) NOT NULL,
    SoTienHoan    DECIMAL (18, 2) DEFAULT 0 NOT NULL,
    MaGiaoDich    NVARCHAR (120)  NULL,
    MaLienKet     NVARCHAR (120)  NULL,
    MaHoanTien    NVARCHAR (120)  NULL,
    TrangThai     INT             NOT NULL,
    NgayTao       DATETIME2       NOT NULL,
    NgayThanhToan DATETIME2       NULL,
    RowVersion    ROWVERSION      NOT NULL,
    CONSTRAINT CK_ThanhToan CHECK (SoTien > 0
                                   AND SoTienHoan >= 0
                                   AND SoTienHoan <= SoTien),
    FOREIGN KEY (DonVeId) REFERENCES DonVe (Id)
);

CREATE UNIQUE INDEX UX_ThanhToan_MaGiaoDich
    ON ThanhToan(MaGiaoDich) WHERE MaGiaoDich IS NOT NULL;

CREATE INDEX IX_ThanhToan_Don_TrangThai
    ON ThanhToan(DonVeId, TrangThai);

CREATE TABLE Ve (
    Id          INT            IDENTITY PRIMARY KEY,
    DonVeId     INT            NOT NULL,
    LoaiVeId    INT            NOT NULL,
    MaVe        NVARCHAR (40)  NOT NULL,
    MaQR        NVARCHAR (120) NOT NULL,
    TrangThai   INT            NOT NULL,
    NgayTao     DATETIME2      NOT NULL,
    NgayCheckIn DATETIME2      NULL,
    RowVersion  ROWVERSION     NOT NULL,
    FOREIGN KEY (DonVeId) REFERENCES DonVe (Id),
    FOREIGN KEY (LoaiVeId) REFERENCES LoaiVe (Id)
);

CREATE UNIQUE INDEX UX_Ve_MaVe
    ON Ve(MaVe);

CREATE UNIQUE INDEX UX_Ve_MaQR
    ON Ve(MaQR);

CREATE TABLE XuatVe (
    Id        INT            IDENTITY PRIMARY KEY,
    VeId      INT            NOT NULL,
    Kieu      NVARCHAR (30)  NOT NULL,
    DiaChi    NVARCHAR (250) NULL,
    ThanhCong BIT            NOT NULL,
    NgayTao   DATETIME2      NOT NULL,
    FOREIGN KEY (VeId) REFERENCES Ve (Id)
);

CREATE INDEX IX_XuatVe_Ve_Ngay
    ON XuatVe(VeId, NgayTao);