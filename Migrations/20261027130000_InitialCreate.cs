using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QLSK.Data;

namespace QLSK.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261027130000_InitialCreate")]
public class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder boDungMigration)
    {
        boDungMigration.CreateTable(
            name: "VaiTro",
            columns: cot => new
            {
                Id = cot.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Ten = cot.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                MoTa = cot.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, defaultValue: "")
            },
            constraints: bangDuLieu =>
            {
                bangDuLieu.PrimaryKey("PK_VaiTro", cot => cot.Id);
                bangDuLieu.UniqueConstraint("AK_VaiTro_Ten", cot => cot.Ten);
            });

        boDungMigration.CreateTable(
            name: "TaiKhoan",
            columns: cot => new
            {
                Id = cot.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                HoTen = cot.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                Email = cot.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                MatKhau = cot.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                VaiTroId = cot.Column<int>(type: "int", nullable: false),
                HoatDong = cot.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                NgayTao = cot.Column<DateTime>(type: "datetime2", nullable: false),
                RowVersion = cot.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: bangDuLieu =>
            {
                bangDuLieu.PrimaryKey("PK_TaiKhoan", cot => cot.Id);
                bangDuLieu.ForeignKey(
                    name: "FK_TaiKhoan_VaiTro",
                    column: cot => cot.VaiTroId,
                    principalTable: "VaiTro",
                    principalColumn: "Id");
            });

        boDungMigration.CreateTable(
            name: "YeuCauXacThucToChuc",
            columns: cot => new
            {
                Id = cot.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TaiKhoanId = cot.Column<int>(type: "int", nullable: false),
                TenToChuc = cot.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                GiayTo = cot.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                LyDo = cot.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                TrangThai = cot.Column<int>(type: "int", nullable: false),
                NgayTao = cot.Column<DateTime>(type: "datetime2", nullable: false),
                NgayXuLy = cot.Column<DateTime>(type: "datetime2", nullable: true),
                NguoiXuLyId = cot.Column<int>(type: "int", nullable: true)
            },
            constraints: bangDuLieu =>
            {
                bangDuLieu.PrimaryKey("PK_YeuCauXacThucToChuc", cot => cot.Id);
                bangDuLieu.ForeignKey(
                    name: "FK_YeuCauXacThucToChuc_TaiKhoan_TaiKhoanId",
                    column: cot => cot.TaiKhoanId,
                    principalTable: "TaiKhoan",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                bangDuLieu.ForeignKey(
                    name: "FK_YeuCauXacThucToChuc_TaiKhoan_NguoiXuLyId",
                    column: cot => cot.NguoiXuLyId,
                    principalTable: "TaiKhoan",
                    principalColumn: "Id");
            });

        boDungMigration.CreateTable(
            name: "LichSuYeuCauXacThucToChuc",
            columns: cot => new
            {
                Id = cot.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TaiKhoanId = cot.Column<int>(type: "int", nullable: false),
                TenToChuc = cot.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                GiayTo = cot.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                LyDo = cot.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                TrangThai = cot.Column<int>(type: "int", nullable: false),
                NgayTao = cot.Column<DateTime>(type: "datetime2", nullable: false),
                NgayXuLy = cot.Column<DateTime>(type: "datetime2", nullable: false),
                NguoiXuLyId = cot.Column<int>(type: "int", nullable: true)
            },
            constraints: bangDuLieu =>
            {
                bangDuLieu.PrimaryKey("PK_LichSuYeuCauXacThucToChuc", cot => cot.Id);
                bangDuLieu.ForeignKey(
                    name: "FK_LichSuYeuCauXacThucToChuc_TaiKhoan_TaiKhoanId",
                    column: cot => cot.TaiKhoanId,
                    principalTable: "TaiKhoan",
                    principalColumn: "Id");
                bangDuLieu.ForeignKey(
                    name: "FK_LichSuYeuCauXacThucToChuc_TaiKhoan_NguoiXuLyId",
                    column: cot => cot.NguoiXuLyId,
                    principalTable: "TaiKhoan",
                    principalColumn: "Id");
            });

        boDungMigration.CreateTable(
            name: "BanToChuc",
            columns: cot => new
            {
                Id = cot.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TaiKhoanId = cot.Column<int>(type: "int", nullable: false),
                Ten = cot.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                MoTa = cot.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false, defaultValue: ""),
                NgayXacThuc = cot.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: bangDuLieu =>
            {
                bangDuLieu.PrimaryKey("PK_BanToChuc", cot => cot.Id);
                bangDuLieu.UniqueConstraint("AK_BanToChuc_TaiKhoanId", cot => cot.TaiKhoanId);
                bangDuLieu.ForeignKey(
                    name: "FK_BanToChuc_TaiKhoan_TaiKhoanId",
                    column: cot => cot.TaiKhoanId,
                    principalTable: "TaiKhoan",
                    principalColumn: "Id");
            });

        boDungMigration.CreateTable(
            name: "LoaiSuKien",
            columns: cot => new
            {
                Id = cot.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Ten = cot.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                MoTa = cot.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false, defaultValue: ""),
                HoatDong = cot.Column<bool>(type: "bit", nullable: false, defaultValue: true)
            },
            constraints: bangDuLieu =>
            {
                bangDuLieu.PrimaryKey("PK_LoaiSuKien", cot => cot.Id);
                bangDuLieu.UniqueConstraint("AK_LoaiSuKien_Ten", cot => cot.Ten);
            });

        boDungMigration.CreateTable(
            name: "SuKien",
            columns: cot => new
            {
                Id = cot.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                BanToChucId = cot.Column<int>(type: "int", nullable: false),
                LoaiSuKienId = cot.Column<int>(type: "int", nullable: false),
                Ten = cot.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                MoTa = cot.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false, defaultValue: ""),
                BatDau = cot.Column<DateTime>(type: "datetime2", nullable: false),
                KetThuc = cot.Column<DateTime>(type: "datetime2", nullable: false),
                SoNguoi = cot.Column<int>(type: "int", nullable: false),
                TrangThai = cot.Column<int>(type: "int", nullable: false),
                NgayTao = cot.Column<DateTime>(type: "datetime2", nullable: false),
                RowVersion = cot.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: bangDuLieu =>
            {
                bangDuLieu.PrimaryKey("PK_SuKien", cot => cot.Id);
                bangDuLieu.CheckConstraint("CK_SuKien_ThoiGian", "KetThuc>BatDau");
                bangDuLieu.CheckConstraint("CK_SuKien_SoNguoi", "SoNguoi>0");
                bangDuLieu.ForeignKey(
                    name: "FK_SuKien_BanToChuc_BanToChucId",
                    column: cot => cot.BanToChucId,
                    principalTable: "BanToChuc",
                    principalColumn: "Id");
                bangDuLieu.ForeignKey(
                    name: "FK_SuKien_LoaiSuKien_LoaiSuKienId",
                    column: cot => cot.LoaiSuKienId,
                    principalTable: "LoaiSuKien",
                    principalColumn: "Id");
            });

        boDungMigration.CreateTable(
            name: "DiaDiem",
            columns: cot => new
            {
                Id = cot.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Ten = cot.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                DiaChi = cot.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                SucChua = cot.Column<int>(type: "int", nullable: false),
                GiaThue = cot.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                HoatDong = cot.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                RowVersion = cot.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: bangDuLieu =>
            {
                bangDuLieu.PrimaryKey("PK_DiaDiem", cot => cot.Id);
                bangDuLieu.CheckConstraint("CK_DiaDiem_SucChua", "SucChua>0");
                bangDuLieu.CheckConstraint("CK_DiaDiem_GiaThue", "GiaThue>=0");
            });

        boDungMigration.CreateTable(
            name: "DatDiaDiem",
            columns: cot => new
            {
                Id = cot.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DiaDiemId = cot.Column<int>(type: "int", nullable: false),
                SuKienId = cot.Column<int>(type: "int", nullable: false),
                BatDau = cot.Column<DateTime>(type: "datetime2", nullable: false),
                KetThuc = cot.Column<DateTime>(type: "datetime2", nullable: false),
                TienCoc = cot.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                TongTien = cot.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                TrangThai = cot.Column<int>(type: "int", nullable: false),
                NgayTao = cot.Column<DateTime>(type: "datetime2", nullable: false),
                RowVersion = cot.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: bangDuLieu =>
            {
                bangDuLieu.PrimaryKey("PK_DatDiaDiem", cot => cot.Id);
                bangDuLieu.UniqueConstraint("AK_DatDiaDiem_SuKienId", cot => cot.SuKienId);
                bangDuLieu.CheckConstraint("CK_DatDiaDiem_ThoiGian", "KetThuc>BatDau");
                bangDuLieu.CheckConstraint("CK_DatDiaDiem_Tien", "TienCoc>=0 AND TienCoc<=TongTien");
                bangDuLieu.ForeignKey(
                    name: "FK_DatDiaDiem_DiaDiem_DiaDiemId",
                    column: cot => cot.DiaDiemId,
                    principalTable: "DiaDiem",
                    principalColumn: "Id");
                bangDuLieu.ForeignKey(
                    name: "FK_DatDiaDiem_SuKien_SuKienId",
                    column: cot => cot.SuKienId,
                    principalTable: "SuKien",
                    principalColumn: "Id");
            });

        boDungMigration.CreateTable(
            name: "LoaiVe",
            columns: cot => new
            {
                Id = cot.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SuKienId = cot.Column<int>(type: "int", nullable: false),
                Ten = cot.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                Gia = cot.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                SoLuong = cot.Column<int>(type: "int", nullable: false),
                DaBan = cot.Column<int>(type: "int", nullable: false, defaultValue: 0),
                DangGiu = cot.Column<int>(type: "int", nullable: false, defaultValue: 0),
                MoBan = cot.Column<DateTime>(type: "datetime2", nullable: false),
                DongBan = cot.Column<DateTime>(type: "datetime2", nullable: false),
                RowVersion = cot.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: bangDuLieu =>
            {
                bangDuLieu.PrimaryKey("PK_LoaiVe", cot => cot.Id);
                bangDuLieu.CheckConstraint("CK_LoaiVe", "Gia>=0 AND SoLuong>0 AND DongBan>MoBan AND DaBan>=0 AND DangGiu>=0 AND DaBan+DangGiu<=SoLuong");
                bangDuLieu.ForeignKey(
                    name: "FK_LoaiVe_SuKien_SuKienId",
                    column: cot => cot.SuKienId,
                    principalTable: "SuKien",
                    principalColumn: "Id");
            });

        boDungMigration.CreateTable(
            name: "DonVe",
            columns: cot => new
            {
                Id = cot.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TaiKhoanId = cot.Column<int>(type: "int", nullable: false),
                SuKienId = cot.Column<int>(type: "int", nullable: false),
                SoLuong = cot.Column<int>(type: "int", nullable: false),
                TongTien = cot.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                TrangThai = cot.Column<int>(type: "int", nullable: false),
                NgayTao = cot.Column<DateTime>(type: "datetime2", nullable: false),
                HetHan = cot.Column<DateTime>(type: "datetime2", nullable: true),
                RowVersion = cot.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: bangDuLieu =>
            {
                bangDuLieu.PrimaryKey("PK_DonVe", cot => cot.Id);
                bangDuLieu.CheckConstraint("CK_DonVe", "SoLuong>0 AND TongTien>=0");
                bangDuLieu.ForeignKey(
                    name: "FK_DonVe_TaiKhoan_TaiKhoanId",
                    column: cot => cot.TaiKhoanId,
                    principalTable: "TaiKhoan",
                    principalColumn: "Id");
                bangDuLieu.ForeignKey(
                    name: "FK_DonVe_SuKien_SuKienId",
                    column: cot => cot.SuKienId,
                    principalTable: "SuKien",
                    principalColumn: "Id");
            });

        boDungMigration.CreateTable(
            name: "ChiTietDonVe",
            columns: cot => new
            {
                Id = cot.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DonVeId = cot.Column<int>(type: "int", nullable: false),
                LoaiVeId = cot.Column<int>(type: "int", nullable: false),
                SoLuong = cot.Column<int>(type: "int", nullable: false),
                DonGia = cot.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                ThanhTien = cot.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
            },
            constraints: bangDuLieu =>
            {
                bangDuLieu.PrimaryKey("PK_ChiTietDonVe", cot => cot.Id);
                bangDuLieu.CheckConstraint("CK_ChiTiet", "SoLuong>0 AND DonGia>=0 AND ThanhTien>=0");
                bangDuLieu.ForeignKey(
                    name: "FK_ChiTietDonVe_DonVe_DonVeId",
                    column: cot => cot.DonVeId,
                    principalTable: "DonVe",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                bangDuLieu.ForeignKey(
                    name: "FK_ChiTietDonVe_LoaiVe_LoaiVeId",
                    column: cot => cot.LoaiVeId,
                    principalTable: "LoaiVe",
                    principalColumn: "Id");
            });

        boDungMigration.CreateTable(
            name: "ThanhToan",
            columns: cot => new
            {
                Id = cot.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DonVeId = cot.Column<int>(type: "int", nullable: false),
                SoTien = cot.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                SoTienHoan = cot.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                MaGiaoDich = cot.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                MaLienKet = cot.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                MaHoanTien = cot.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                TrangThai = cot.Column<int>(type: "int", nullable: false),
                NgayTao = cot.Column<DateTime>(type: "datetime2", nullable: false),
                NgayThanhToan = cot.Column<DateTime>(type: "datetime2", nullable: true),
                RowVersion = cot.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: bangDuLieu =>
            {
                bangDuLieu.PrimaryKey("PK_ThanhToan", cot => cot.Id);
                bangDuLieu.CheckConstraint("CK_ThanhToan", "SoTien>0 AND SoTienHoan>=0 AND SoTienHoan<=SoTien");
                bangDuLieu.ForeignKey(
                    name: "FK_ThanhToan_DonVe_DonVeId",
                    column: cot => cot.DonVeId,
                    principalTable: "DonVe",
                    principalColumn: "Id");
            });

        boDungMigration.CreateTable(
            name: "Ve",
            columns: cot => new
            {
                Id = cot.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DonVeId = cot.Column<int>(type: "int", nullable: false),
                LoaiVeId = cot.Column<int>(type: "int", nullable: false),
                MaVe = cot.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                MaQR = cot.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                TrangThai = cot.Column<int>(type: "int", nullable: false),
                NgayTao = cot.Column<DateTime>(type: "datetime2", nullable: false),
                NgayCheckIn = cot.Column<DateTime>(type: "datetime2", nullable: true),
                RowVersion = cot.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: bangDuLieu =>
            {
                bangDuLieu.PrimaryKey("PK_Ve", cot => cot.Id);
                bangDuLieu.ForeignKey(
                    name: "FK_Ve_DonVe_DonVeId",
                    column: cot => cot.DonVeId,
                    principalTable: "DonVe",
                    principalColumn: "Id");
                bangDuLieu.ForeignKey(
                    name: "FK_Ve_LoaiVe_LoaiVeId",
                    column: cot => cot.LoaiVeId,
                    principalTable: "LoaiVe",
                    principalColumn: "Id");
            });

        boDungMigration.CreateTable(
            name: "XuatVe",
            columns: cot => new
            {
                Id = cot.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                VeId = cot.Column<int>(type: "int", nullable: false),
                Kieu = cot.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                DiaChi = cot.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                ThanhCong = cot.Column<bool>(type: "bit", nullable: false),
                NgayTao = cot.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: bangDuLieu =>
            {
                bangDuLieu.PrimaryKey("PK_XuatVe", cot => cot.Id);
                bangDuLieu.ForeignKey(
                    name: "FK_XuatVe_Ve_VeId",
                    column: cot => cot.VeId,
                    principalTable: "Ve",
                    principalColumn: "Id");
            });

        boDungMigration.InsertData(
            table: "VaiTro",
            columns: new[] { "Id", "Ten", "MoTa" },
            columnTypes: new[] { "int", "nvarchar(40)", "nvarchar(200)" },
            values: new object[,]
            {
                { 1, "NguoiDung", "Người dùng" },
                { 2, "BanToChuc", "Ban tổ chức" },
                { 3, "QuanLySuKien", "Quản lý sự kiện" },
                { 4, "QuanTriVien", "Quản trị viên" }
            });

        boDungMigration.CreateIndex(
            name: "UX_TaiKhoan_Email",
            table: "TaiKhoan",
            column: "Email",
            unique: true);

        boDungMigration.CreateIndex(
            name: "UX_YeuCau_TaiKhoan",
            table: "YeuCauXacThucToChuc",
            column: "TaiKhoanId",
            unique: true);

        boDungMigration.CreateIndex(
            name: "IX_SuKien_TrangThai_BatDau",
            table: "SuKien",
            columns: new[] { "TrangThai", "BatDau" });

        boDungMigration.CreateIndex(
            name: "UX_DiaDiem_Ten_DiaChi",
            table: "DiaDiem",
            columns: new[] { "Ten", "DiaChi" },
            unique: true);

        boDungMigration.CreateIndex(
            name: "IX_DatDiaDiem_Lich",
            table: "DatDiaDiem",
            columns: new[] { "DiaDiemId", "BatDau", "KetThuc" });

        boDungMigration.CreateIndex(
            name: "UX_LoaiVe_SuKien_Ten",
            table: "LoaiVe",
            columns: new[] { "SuKienId", "Ten" },
            unique: true);

        boDungMigration.CreateIndex(
            name: "IX_DonVe_User",
            table: "DonVe",
            columns: new[] { "TaiKhoanId", "NgayTao" });

        boDungMigration.CreateIndex(
            name: "IX_DonVe_Expire",
            table: "DonVe",
            columns: new[] { "TrangThai", "HetHan" });

        boDungMigration.CreateIndex(
            name: "UX_ChiTiet_Don_Loai",
            table: "ChiTietDonVe",
            columns: new[] { "DonVeId", "LoaiVeId" },
            unique: true);

        boDungMigration.CreateIndex(
            name: "UX_ThanhToan_MaGiaoDich",
            table: "ThanhToan",
            column: "MaGiaoDich",
            unique: true,
            filter: "MaGiaoDich IS NOT NULL");

        boDungMigration.CreateIndex(
            name: "IX_ThanhToan_Don_TrangThai",
            table: "ThanhToan",
            columns: new[] { "DonVeId", "TrangThai" });

        boDungMigration.CreateIndex(
            name: "UX_Ve_MaVe",
            table: "Ve",
            column: "MaVe",
            unique: true);

        boDungMigration.CreateIndex(
            name: "UX_Ve_MaQR",
            table: "Ve",
            column: "MaQR",
            unique: true);

        boDungMigration.CreateIndex(
            name: "IX_XuatVe_Ve_Ngay",
            table: "XuatVe",
            columns: new[] { "VeId", "NgayTao" });
    }

    protected override void Down(MigrationBuilder boDungMigration)
    {
        boDungMigration.DropTable(name: "XuatVe");
        boDungMigration.DropTable(name: "Ve");
        boDungMigration.DropTable(name: "ThanhToan");
        boDungMigration.DropTable(name: "ChiTietDonVe");
        boDungMigration.DropTable(name: "DonVe");
        boDungMigration.DropTable(name: "LoaiVe");
        boDungMigration.DropTable(name: "DatDiaDiem");
        boDungMigration.DropTable(name: "DiaDiem");
        boDungMigration.DropTable(name: "SuKien");
        boDungMigration.DropTable(name: "LoaiSuKien");
        boDungMigration.DropTable(name: "BanToChuc");
        boDungMigration.DropTable(name: "LichSuYeuCauXacThucToChuc");
        boDungMigration.DropTable(name: "YeuCauXacThucToChuc");
        boDungMigration.DropTable(name: "TaiKhoan");
        boDungMigration.DropTable(name: "VaiTro");
    }
}
