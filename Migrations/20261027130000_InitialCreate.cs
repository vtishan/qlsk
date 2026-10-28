using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QLSK.Data;

namespace QLSK.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261027130000_InitialCreate")]
public class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
CREATE TABLE VaiTro(Id int IDENTITY PRIMARY KEY, Ten nvarchar(40) NOT NULL UNIQUE, MoTa nvarchar(200) NOT NULL DEFAULT '');
SET IDENTITY_INSERT VaiTro ON;
INSERT VaiTro(Id,Ten,MoTa) VALUES (1,N'NguoiDung',N'Người dùng'),(2,N'BanToChuc',N'Ban tổ chức'),(3,N'QuanLySuKien',N'Quản lý sự kiện'),(4,N'QuanTriVien',N'Quản trị viên');
SET IDENTITY_INSERT VaiTro OFF;
CREATE TABLE TaiKhoan(Id int IDENTITY PRIMARY KEY,HoTen nvarchar(120) NOT NULL,Email nvarchar(200) NOT NULL,MatKhau nvarchar(500) NOT NULL,VaiTroId int NOT NULL,HoatDong bit NOT NULL DEFAULT 1,NgayTao datetime2 NOT NULL,RowVersion rowversion NOT NULL,CONSTRAINT FK_TaiKhoan_VaiTro FOREIGN KEY(VaiTroId) REFERENCES VaiTro(Id));
CREATE UNIQUE INDEX UX_TaiKhoan_Email ON TaiKhoan(Email);
CREATE TABLE YeuCauXacThucToChuc(Id int IDENTITY PRIMARY KEY,TaiKhoanId int NOT NULL,TenToChuc nvarchar(200) NOT NULL,GiayTo nvarchar(500) NOT NULL,LyDo nvarchar(500) NULL,TrangThai int NOT NULL,NgayTao datetime2 NOT NULL,NgayXuLy datetime2 NULL,NguoiXuLyId int NULL,FOREIGN KEY(TaiKhoanId) REFERENCES TaiKhoan(Id) ON DELETE CASCADE,FOREIGN KEY(NguoiXuLyId) REFERENCES TaiKhoan(Id));
CREATE UNIQUE INDEX UX_YeuCau_TaiKhoan ON YeuCauXacThucToChuc(TaiKhoanId);
CREATE TABLE LichSuYeuCauXacThucToChuc(Id int IDENTITY PRIMARY KEY,TaiKhoanId int NOT NULL,TenToChuc nvarchar(200) NOT NULL,GiayTo nvarchar(500) NOT NULL,LyDo nvarchar(500) NULL,TrangThai int NOT NULL,NgayTao datetime2 NOT NULL,NgayXuLy datetime2 NOT NULL,NguoiXuLyId int NULL,FOREIGN KEY(TaiKhoanId) REFERENCES TaiKhoan(Id),FOREIGN KEY(NguoiXuLyId) REFERENCES TaiKhoan(Id));
CREATE TABLE BanToChuc(Id int IDENTITY PRIMARY KEY,TaiKhoanId int NOT NULL UNIQUE,Ten nvarchar(200) NOT NULL,MoTa nvarchar(1000) NOT NULL DEFAULT '',NgayXacThuc datetime2 NOT NULL,FOREIGN KEY(TaiKhoanId) REFERENCES TaiKhoan(Id));
CREATE TABLE LoaiSuKien(Id int IDENTITY PRIMARY KEY,Ten nvarchar(120) NOT NULL UNIQUE,MoTa nvarchar(500) NOT NULL DEFAULT '',HoatDong bit NOT NULL DEFAULT 1);
CREATE TABLE SuKien(Id int IDENTITY PRIMARY KEY,BanToChucId int NOT NULL,LoaiSuKienId int NOT NULL,Ten nvarchar(200) NOT NULL,MoTa nvarchar(4000) NOT NULL DEFAULT '',BatDau datetime2 NOT NULL,KetThuc datetime2 NOT NULL,SoNguoi int NOT NULL,TrangThai int NOT NULL,NgayTao datetime2 NOT NULL,RowVersion rowversion NOT NULL,CONSTRAINT CK_SuKien_ThoiGian CHECK(KetThuc>BatDau),CONSTRAINT CK_SuKien_SoNguoi CHECK(SoNguoi>0),FOREIGN KEY(BanToChucId) REFERENCES BanToChuc(Id),FOREIGN KEY(LoaiSuKienId) REFERENCES LoaiSuKien(Id));
CREATE INDEX IX_SuKien_TrangThai_BatDau ON SuKien(TrangThai,BatDau);
CREATE TABLE DiaDiem(Id int IDENTITY PRIMARY KEY,Ten nvarchar(200) NOT NULL,DiaChi nvarchar(300) NOT NULL,SucChua int NOT NULL,GiaThue decimal(18,2) NOT NULL,HoatDong bit NOT NULL DEFAULT 1,RowVersion rowversion NOT NULL,CONSTRAINT CK_DiaDiem_SucChua CHECK(SucChua>0),CONSTRAINT CK_DiaDiem_GiaThue CHECK(GiaThue>=0));
CREATE UNIQUE INDEX UX_DiaDiem_Ten_DiaChi ON DiaDiem(Ten,DiaChi);
CREATE TABLE DatDiaDiem(Id int IDENTITY PRIMARY KEY,DiaDiemId int NOT NULL,SuKienId int NOT NULL UNIQUE,BatDau datetime2 NOT NULL,KetThuc datetime2 NOT NULL,TienCoc decimal(18,2) NOT NULL,TongTien decimal(18,2) NOT NULL,TrangThai int NOT NULL,NgayTao datetime2 NOT NULL,RowVersion rowversion NOT NULL,CONSTRAINT CK_DatDiaDiem_ThoiGian CHECK(KetThuc>BatDau),CONSTRAINT CK_DatDiaDiem_Tien CHECK(TienCoc>=0 AND TienCoc<=TongTien),FOREIGN KEY(DiaDiemId) REFERENCES DiaDiem(Id),FOREIGN KEY(SuKienId) REFERENCES SuKien(Id));
CREATE INDEX IX_DatDiaDiem_Lich ON DatDiaDiem(DiaDiemId,BatDau,KetThuc);
CREATE TABLE LoaiVe(Id int IDENTITY PRIMARY KEY,SuKienId int NOT NULL,Ten nvarchar(120) NOT NULL,Gia decimal(18,2) NOT NULL,SoLuong int NOT NULL,DaBan int NOT NULL DEFAULT 0,DangGiu int NOT NULL DEFAULT 0,MoBan datetime2 NOT NULL,DongBan datetime2 NOT NULL,RowVersion rowversion NOT NULL,CONSTRAINT CK_LoaiVe CHECK(Gia>=0 AND SoLuong>0 AND DongBan>MoBan AND DaBan>=0 AND DangGiu>=0 AND DaBan+DangGiu<=SoLuong),FOREIGN KEY(SuKienId) REFERENCES SuKien(Id));
CREATE UNIQUE INDEX UX_LoaiVe_SuKien_Ten ON LoaiVe(SuKienId,Ten);
CREATE TABLE DonVe(Id int IDENTITY PRIMARY KEY,TaiKhoanId int NOT NULL,SuKienId int NOT NULL,SoLuong int NOT NULL,TongTien decimal(18,2) NOT NULL,TrangThai int NOT NULL,NgayTao datetime2 NOT NULL,HetHan datetime2 NULL,RowVersion rowversion NOT NULL,CONSTRAINT CK_DonVe CHECK(SoLuong>0 AND TongTien>=0),FOREIGN KEY(TaiKhoanId) REFERENCES TaiKhoan(Id),FOREIGN KEY(SuKienId) REFERENCES SuKien(Id));
CREATE INDEX IX_DonVe_User ON DonVe(TaiKhoanId,NgayTao); CREATE INDEX IX_DonVe_Expire ON DonVe(TrangThai,HetHan);
CREATE TABLE ChiTietDonVe(Id int IDENTITY PRIMARY KEY,DonVeId int NOT NULL,LoaiVeId int NOT NULL,SoLuong int NOT NULL,DonGia decimal(18,2) NOT NULL,ThanhTien decimal(18,2) NOT NULL,CONSTRAINT CK_ChiTiet CHECK(SoLuong>0 AND DonGia>=0 AND ThanhTien>=0),FOREIGN KEY(DonVeId) REFERENCES DonVe(Id) ON DELETE CASCADE,FOREIGN KEY(LoaiVeId) REFERENCES LoaiVe(Id));
CREATE UNIQUE INDEX UX_ChiTiet_Don_Loai ON ChiTietDonVe(DonVeId,LoaiVeId);
CREATE TABLE ThanhToan(Id int IDENTITY PRIMARY KEY,DonVeId int NOT NULL,SoTien decimal(18,2) NOT NULL,SoTienHoan decimal(18,2) NOT NULL DEFAULT 0,MaGiaoDich nvarchar(120) NULL,MaLienKet nvarchar(120) NULL,MaHoanTien nvarchar(120) NULL,TrangThai int NOT NULL,NgayTao datetime2 NOT NULL,NgayThanhToan datetime2 NULL,RowVersion rowversion NOT NULL,CONSTRAINT CK_ThanhToan CHECK(SoTien>0 AND SoTienHoan>=0 AND SoTienHoan<=SoTien),FOREIGN KEY(DonVeId) REFERENCES DonVe(Id));
CREATE UNIQUE INDEX UX_ThanhToan_MaGiaoDich ON ThanhToan(MaGiaoDich) WHERE MaGiaoDich IS NOT NULL; CREATE INDEX IX_ThanhToan_Don_TrangThai ON ThanhToan(DonVeId,TrangThai);
CREATE TABLE Ve(Id int IDENTITY PRIMARY KEY,DonVeId int NOT NULL,LoaiVeId int NOT NULL,MaVe nvarchar(40) NOT NULL,MaQR nvarchar(120) NOT NULL,TrangThai int NOT NULL,NgayTao datetime2 NOT NULL,NgayCheckIn datetime2 NULL,RowVersion rowversion NOT NULL,FOREIGN KEY(DonVeId) REFERENCES DonVe(Id),FOREIGN KEY(LoaiVeId) REFERENCES LoaiVe(Id));
CREATE UNIQUE INDEX UX_Ve_MaVe ON Ve(MaVe); CREATE UNIQUE INDEX UX_Ve_MaQR ON Ve(MaQR);
CREATE TABLE XuatVe(Id int IDENTITY PRIMARY KEY,VeId int NOT NULL,Kieu nvarchar(30) NOT NULL,DiaChi nvarchar(250) NULL,ThanhCong bit NOT NULL,NgayTao datetime2 NOT NULL,FOREIGN KEY(VeId) REFERENCES Ve(Id));
CREATE INDEX IX_XuatVe_Ve_Ngay ON XuatVe(VeId,NgayTao);

""");
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""DROP TABLE IF EXISTS XuatVe; DROP TABLE IF EXISTS Ve; DROP TABLE IF EXISTS ThanhToan; DROP TABLE IF EXISTS ChiTietDonVe; DROP TABLE IF EXISTS DonVe; DROP TABLE IF EXISTS LoaiVe; DROP TABLE IF EXISTS DatDiaDiem; DROP TABLE IF EXISTS DiaDiem; DROP TABLE IF EXISTS SuKien; DROP TABLE IF EXISTS LoaiSuKien; DROP TABLE IF EXISTS BanToChuc; DROP TABLE IF EXISTS LichSuYeuCauXacThucToChuc; DROP TABLE IF EXISTS YeuCauXacThucToChuc; DROP TABLE IF EXISTS TaiKhoan; DROP TABLE IF EXISTS VaiTro;""");
}
