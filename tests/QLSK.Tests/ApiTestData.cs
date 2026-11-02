using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;

namespace QLSK.Tests;

public sealed record TestScenario(
    int UserId,
    int OrganizerUserId,
    int ManagerId,
    int AdminId,
    int CategoryId,
    int EventId,
    int VenueId,
    int TicketTypeId,
    int OrderId,
    int PaymentId,
    int TicketId,
    int BookingId);

public static class ApiTestData
{
    public static async Task<TestScenario> TaoDuLieuKiemThuAsync(
        ApplicationDbContext nguCanhDuLieu,
        TrangThaiSuKien trangThaiSuKien = TrangThaiSuKien.DaDuyet,
        TrangThaiDon trangThaiDonVe = TrangThaiDon.ChoThanhToan,
        TrangThaiThanhToan trangThaiThanhToan = TrangThaiThanhToan.ThanhCong,
        TrangThaiVe trangThaiVe = TrangThaiVe.ChuaSuDung,
        TrangThaiDatCho trangThaiDatDiaDiem = TrangThaiDatCho.DaXacNhan)
    {
        var hauToDuyNhat = Guid.NewGuid().ToString("N");
        var taiKhoanNguoiDung = TaoTaiKhoanKiemThu(1, $"user-{hauToDuyNhat}@test.local");
        var taiKhoanBanToChuc = TaoTaiKhoanKiemThu(2, $"organizer-{hauToDuyNhat}@test.local");
        var taiKhoanQuanLy = TaoTaiKhoanKiemThu(3, $"manager-{hauToDuyNhat}@test.local");
        var taiKhoanQuanTri = TaoTaiKhoanKiemThu(4, $"admin-{hauToDuyNhat}@test.local");
        nguCanhDuLieu.DanhSachTaiKhoan.AddRange(taiKhoanNguoiDung, taiKhoanBanToChuc, taiKhoanQuanLy, taiKhoanQuanTri);

        var banToChucHienCo = new BanToChuc
        {
            TaiKhoan = taiKhoanBanToChuc,
            TenBanToChuc = $"Ban tổ chức {hauToDuyNhat}"
        };
        var loaiSuKienKiemThu = new LoaiSuKien
        {
            TenLoaiSuKien = $"Loại {hauToDuyNhat}",
            MoTaLoaiSuKien = "Dữ liệu integration test",
            HoatDong = true
        };
        nguCanhDuLieu.AddRange(banToChucHienCo, loaiSuKienKiemThu);

        var thoiDiemBatDau = trangThaiSuKien switch
        {
            TrangThaiSuKien.DangDienRa => DateTime.UtcNow.AddMinutes(-30),
            TrangThaiSuKien.DaKetThuc => DateTime.UtcNow.AddDays(-1),
            _ => DateTime.UtcNow.AddDays(10)
        };
        var suKienKiemThu = new SuKien
        {
            BanToChuc = banToChucHienCo,
            LoaiSuKien = loaiSuKienKiemThu,
            TenSuKien = $"Sự kiện {hauToDuyNhat}",
            MoTaSuKien = "Dữ liệu integration test",
            ThoiGianBatDau = thoiDiemBatDau,
            ThoiGianKetThuc = thoiDiemBatDau.AddHours(3),
            SoNguoi = 500,
            TrangThai = trangThaiSuKien
        };
        var diaDiemSuKien = new DiaDiem
        {
            TenDiaDiem = $"Địa điểm {hauToDuyNhat}",
            DiaChi = $"Địa chỉ {hauToDuyNhat}",
            SucChua = 1000,
            GiaThue = 1_000_000,
            HoatDong = true
        };
        nguCanhDuLieu.AddRange(suKienKiemThu, diaDiemSuKien);

        var loaiVeKiemThu = new LoaiVe
        {
            SuKien = suKienKiemThu,
            TenLoaiVe = $"Vé {hauToDuyNhat}",
            GiaVe = 100_000,
            SoLuong = 100,
            SoLuongDaBan = trangThaiDonVe == TrangThaiDon.ThanhCong ? 1 : 0,
            SoLuongDangGiu = trangThaiDonVe == TrangThaiDon.ChoThanhToan ? 1 : 0,
            ThoiGianMoBan = DateTime.UtcNow.AddDays(-1),
            ThoiGianDongBan = DateTime.UtcNow.AddDays(5)
        };
        var donVeDangXuLy = new DonVe
        {
            TaiKhoan = taiKhoanNguoiDung,
            SuKien = suKienKiemThu,
            SoLuong = 1,
            TongTien = 100_000,
            TrangThai = trangThaiDonVe,
            ThoiGianHetHan = DateTime.UtcNow.AddMinutes(15)
        };
        donVeDangXuLy.ChiTietDonVe.Add(new ChiTietDonVe
        {
            LoaiVe = loaiVeKiemThu,
            SoLuong = 1,
            DonGia = 100_000,
            ThanhTien = 100_000
        });
        var thanhToanKiemThu = new ThanhToan
        {
            DonVe = donVeDangXuLy,
            SoTien = 100_000,
            TrangThai = trangThaiThanhToan,
            MaGiaoDich = $"PAY-{hauToDuyNhat}",
            NgayThanhToan = trangThaiThanhToan == TrangThaiThanhToan.ThanhCong ? DateTime.UtcNow : null
        };
        var veKiemThu = new Ve
        {
            DonVe = donVeDangXuLy,
            LoaiVe = loaiVeKiemThu,
            MaVe = $"VE-{hauToDuyNhat}",
            NoiDungMaQr = $"QR-{hauToDuyNhat}",
            TrangThai = trangThaiVe
        };
        var phieuDatDiaDiem = new DatDiaDiem
        {
            DiaDiem = diaDiemSuKien,
            SuKien = suKienKiemThu,
            ThoiGianBatDau = thoiDiemBatDau,
            ThoiGianKetThuc = thoiDiemBatDau.AddHours(3),
            TienCoc = 100_000,
            TongTien = diaDiemSuKien.GiaThue,
            TrangThai = trangThaiDatDiaDiem
        };
        nguCanhDuLieu.AddRange(loaiVeKiemThu, donVeDangXuLy, thanhToanKiemThu, veKiemThu, phieuDatDiaDiem);
        await nguCanhDuLieu.SaveChangesAsync();

        return new TestScenario(
            taiKhoanNguoiDung.TaiKhoanId,
            taiKhoanBanToChuc.TaiKhoanId,
            taiKhoanQuanLy.TaiKhoanId,
            taiKhoanQuanTri.TaiKhoanId,
            loaiSuKienKiemThu.LoaiSuKienId,
            suKienKiemThu.SuKienId,
            diaDiemSuKien.DiaDiemId,
            loaiVeKiemThu.LoaiVeId,
            donVeDangXuLy.DonVeId,
            thanhToanKiemThu.ThanhToanId,
            veKiemThu.VeId,
            phieuDatDiaDiem.DatDiaDiemId);
    }

    public static async Task<int> AddUserAsync(
        ApplicationDbContext nguCanhDuLieu,
        int vaiTroId,
        bool duocKichHoat = true)
    {
        var taiKhoanNguoiDung = TaoTaiKhoanKiemThu(vaiTroId, $"user-{Guid.NewGuid():N}@test.local", duocKichHoat);
        nguCanhDuLieu.DanhSachTaiKhoan.Add(taiKhoanNguoiDung);
        await nguCanhDuLieu.SaveChangesAsync();
        return taiKhoanNguoiDung.TaiKhoanId;
    }

    public static async Task<int> AddPendingVerificationAsync(ApplicationDbContext nguCanhDuLieu)
    {
        var taiKhoanNguoiDung = TaoTaiKhoanKiemThu(1, $"verification-{Guid.NewGuid():N}@test.local");
        var yeuCauXacThucToChuc = new YeuCauXacThucToChuc
        {
            TaiKhoan = taiKhoanNguoiDung,
            TenToChuc = $"Tổ chức {Guid.NewGuid():N}",
            GiayTo = "https://test.local/document.pdf"
        };
        nguCanhDuLieu.Add(yeuCauXacThucToChuc);
        await nguCanhDuLieu.SaveChangesAsync();
        return yeuCauXacThucToChuc.YeuCauXacThucToChucId;
    }

    public static Task<TestScenario> TaoKichBanKiemThuAsync(ApiFactory mayChuKiemThu,
        TrangThaiSuKien trangThaiSuKien = TrangThaiSuKien.DaDuyet,
        TrangThaiDon trangThaiDonVe = TrangThaiDon.ChoThanhToan,
        TrangThaiThanhToan trangThaiThanhToan = TrangThaiThanhToan.ThanhCong,
        TrangThaiVe trangThaiVe = TrangThaiVe.ChuaSuDung,
        TrangThaiDatCho trangThaiDatDiaDiem = TrangThaiDatCho.DaXacNhan)
        => mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(nguCanhDuLieu => TaoDuLieuKiemThuAsync(nguCanhDuLieu, trangThaiSuKien, trangThaiDonVe, trangThaiThanhToan, trangThaiVe, trangThaiDatDiaDiem));

    public static async Task<int> AddPasswordUserAsync(ApplicationDbContext nguCanhDuLieu, IPasswordHasher<TaiKhoan> boBamMatKhau)
    {
        var taiKhoanNguoiDung = TaoTaiKhoanKiemThu(1, $"password-{Guid.NewGuid():N}@test.local");
        taiKhoanNguoiDung.MatKhauBam = boBamMatKhau.HashPassword(taiKhoanNguoiDung, "Old@123456");
        nguCanhDuLieu.DanhSachTaiKhoan.Add(taiKhoanNguoiDung);
        await nguCanhDuLieu.SaveChangesAsync();
        return taiKhoanNguoiDung.TaiKhoanId;
    }

    private static TaiKhoan TaoTaiKhoanKiemThu(int vaiTroId, string emailTaiKhoan, bool duocKichHoat = true) => new()
    {
        HoTen = $"Test role {vaiTroId}",
        Email = emailTaiKhoan,
        MatKhauBam = "not-used-by-token-tests",
        VaiTroId = vaiTroId,
        HoatDong = duocKichHoat
    };
}
