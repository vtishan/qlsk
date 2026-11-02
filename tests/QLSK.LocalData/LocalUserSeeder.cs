using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;

namespace QLSK.LocalData;

public sealed record UserSeedResult(int AddedUsers, int AddedOrders, int AddedPayments, int AddedTickets, int SkippedUsers);

public static class LocalUserSeeder
{
    public const int UserCount = 100;
    public const string EmailDomain = "sample.qlsk.test";

    private static readonly string[] Surnames =
        ["Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Huỳnh", "Phan", "Vũ", "Võ", "Đặng"];
    private static readonly string[] GivenNames =
        ["Minh Anh", "Thanh Bình", "Ngọc Chi", "Quốc Dũng", "Thu Hà", "Gia Huy", "Khánh Linh", "Bảo Ngọc", "Đức Phúc", "Phương Thảo"];

    public static async Task<UserSeedResult> TaoDuLieuNguoiDungMauAsync(ApplicationDbContext nguCanhDuLieu, IPasswordHasher<TaiKhoan> boBamMatKhau,
        string matKhauNhapLieu, DateTime? thoiDiemUtcChiDinh = null)
    {
        if (string.IsNullOrWhiteSpace(matKhauNhapLieu) || matKhauNhapLieu.Length < 12)
            throw new InvalidOperationException("The sample user password must contain at least 12 characters.");
        var thoiDiemHienTai = thoiDiemUtcChiDinh ?? DateTime.UtcNow;
        await using var giaoDichCoSoDuLieu = nguCanhDuLieu.Database.IsRelational()
            ? await nguCanhDuLieu.Database.BeginTransactionAsync(IsolationLevel.Serializable)
            : null;

        var danhSachEmailEmailTaiKhoan = Enumerable.Range(1, UserCount).Select(TaoEmailNguoiDungMau).ToArray();
        var duLieuDaTonTai = (await nguCanhDuLieu.DanhSachTaiKhoan.AsNoTracking().Where(taiKhoanDb => danhSachEmailEmailTaiKhoan.Contains(taiKhoanDb.Email))
            .Select(taiKhoanDb => taiKhoanDb.Email).ToListAsync()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var duLieuConThieu = Enumerable.Range(1, UserCount).Where(soThuTuBanGhi => !duLieuDaTonTai.Contains(TaoEmailNguoiDungMau(soThuTuBanGhi))).ToArray();
        if (duLieuConThieu.Length == 0) return new UserSeedResult(0, 0, 0, 0, UserCount);
        if (!await nguCanhDuLieu.DanhSachVaiTro.AnyAsync(vaiTroDb => vaiTroDb.VaiTroId == 1))
            throw new InvalidOperationException("Run the schema migrations before populating sample users.");

        // Use existing events only, and keep all inventory changes in the same transaction as the orders.
        var danhSachLoaiVeKiemThu = await nguCanhDuLieu.DanhSachLoaiVe.Include(loaiVeDb => loaiVeDb.SuKien)
            .Where(loaiVeDb => loaiVeDb.SuKien.TrangThai == TrangThaiSuKien.DaDuyet &&
                loaiVeDb.SuKien.ThoiGianBatDau > thoiDiemHienTai.AddMinutes(15) && loaiVeDb.GiaVe >= 2 && loaiVeDb.GiaVe == decimal.Truncate(loaiVeDb.GiaVe) &&
                loaiVeDb.ThoiGianMoBan <= thoiDiemHienTai.AddMinutes(-30) && loaiVeDb.ThoiGianDongBan > thoiDiemHienTai.AddMinutes(15))
            .OrderBy(loaiVeDb => loaiVeDb.LoaiVeId).ToListAsync();
        if (danhSachLoaiVeKiemThu.Count == 0)
            throw new InvalidOperationException("Sample users need an approved upcoming event with paid tickets already on sale for at least 30 minutes.");

        // Plan the entire batch before adding records, so insufficient inventory cannot leave a partial batch.
        var tonVeTheoKeHoach = danhSachLoaiVeKiemThu.ToDictionary(loaiVeDb => loaiVeDb.LoaiVeId, loaiVeDb => loaiVeDb.SoLuong - loaiVeDb.SoLuongDaBan - loaiVeDb.SoLuongDangGiu);
        var keHoachTaoDonVe = new List<(int Number, LoaiVe Type, int Quantity, TrangThaiDon State)>();
        foreach (var soThuTuBanGhi in duLieuConThieu)
        {
            var chiSoBanGhi = soThuTuBanGhi - 1;
            var soLuongVeDat = 1 + chiSoBanGhi % 3;
            var loaiVeDangXuLy = Enumerable.Range(0, danhSachLoaiVeKiemThu.Count).Select(doLechNgay => danhSachLoaiVeKiemThu[(chiSoBanGhi + doLechNgay) % danhSachLoaiVeKiemThu.Count])
                .FirstOrDefault(loaiVeDb => tonVeTheoKeHoach[loaiVeDb.LoaiVeId] >= soLuongVeDat)
                ?? throw new InvalidOperationException("There are not enough available tickets for the sample user orders.");
            var trangThaiDonVe = LayTrangThaiDonMau(chiSoBanGhi);
            if (trangThaiDonVe is TrangThaiDon.ChoThanhToan or TrangThaiDon.ThanhCong or TrangThaiDon.YeuCauHoanTien)
                tonVeTheoKeHoach[loaiVeDangXuLy.LoaiVeId] -= soLuongVeDat;
            keHoachTaoDonVe.Add((soThuTuBanGhi, loaiVeDangXuLy, soLuongVeDat, trangThaiDonVe));
        }

        var addedTickets = 0;
        foreach (var (soThuTuBanGhi, loaiVeDangXuLy, soLuongVeDat, trangThaiDonVe) in keHoachTaoDonVe)
        {
            var chiSoBanGhi = soThuTuBanGhi - 1;
            var taiKhoanNguoiDung = new TaiKhoan
            {
                HoTen = $"{Surnames[chiSoBanGhi / GivenNames.Length]} {GivenNames[chiSoBanGhi % GivenNames.Length]}",
                Email = TaoEmailNguoiDungMau(soThuTuBanGhi), VaiTroId = 1, HoatDong = soThuTuBanGhi % 11 != 0,
                NgayTao = thoiDiemHienTai.AddDays(-60 - soThuTuBanGhi).AddMinutes(-soThuTuBanGhi)
            };
            taiKhoanNguoiDung.MatKhauBam = boBamMatKhau.HashPassword(taiKhoanNguoiDung, matKhauNhapLieu);
            var soPhutMoBan = (int)Math.Min(14 * 24 * 60, (thoiDiemHienTai - loaiVeDangXuLy.ThoiGianMoBan).TotalMinutes);
            var thoiDiemTaoDon = trangThaiDonVe == TrangThaiDon.ChoThanhToan
                ? thoiDiemHienTai.AddMinutes(-1 - chiSoBanGhi % 5)
                : thoiDiemHienTai.AddMinutes(-30 - chiSoBanGhi * 17 % Math.Max(1, soPhutMoBan - 29));
            var tongTienDonVe = loaiVeDangXuLy.GiaVe * soLuongVeDat;
            var donVeDangXuLy = new DonVe
            {
                TaiKhoan = taiKhoanNguoiDung, SuKienId = loaiVeDangXuLy.SuKienId, SoLuong = soLuongVeDat, TongTien = tongTienDonVe,
                TrangThai = trangThaiDonVe, NgayTao = thoiDiemTaoDon, ThoiGianHetHan = thoiDiemTaoDon.AddMinutes(15)
            };
            donVeDangXuLy.ChiTietDonVe.Add(new ChiTietDonVe
            {
                LoaiVe = loaiVeDangXuLy, SoLuong = soLuongVeDat, DonGia = loaiVeDangXuLy.GiaVe, ThanhTien = tongTienDonVe
            });

            var daThanhToan = trangThaiDonVe is TrangThaiDon.ThanhCong or TrangThaiDon.YeuCauHoanTien or TrangThaiDon.DaHoanTien;
            var thanhToanKiemThu = new ThanhToan
            {
                SoTien = tongTienDonVe, NgayTao = thoiDiemTaoDon.AddSeconds(10),
                TrangThai = daThanhToan ? TrangThaiThanhToan.ThanhCong : trangThaiDonVe == TrangThaiDon.ChoThanhToan
                    ? TrangThaiThanhToan.DangXuLy : TrangThaiThanhToan.ThatBai,
                NgayThanhToan = daThanhToan ? thoiDiemTaoDon.AddMinutes(1) : null,
                MaLienKet = $"SAMPLE-U{soThuTuBanGhi:000}-LINK",
                MaGiaoDich = daThanhToan ? $"SAMPLE-U{soThuTuBanGhi:000}-PAY" : null,
                SoTienHoan = trangThaiDonVe == TrangThaiDon.DaHoanTien ? tongTienDonVe
                    : trangThaiDonVe == TrangThaiDon.YeuCauHoanTien ? decimal.Truncate(tongTienDonVe / 2) : 0,
                MaHoanTien = trangThaiDonVe is TrangThaiDon.DaHoanTien or TrangThaiDon.YeuCauHoanTien
                    ? $"SAMPLE-U{soThuTuBanGhi:000}-REFUND" : null
            };
            donVeDangXuLy.DanhSachThanhToan.Add(thanhToanKiemThu);
            if (trangThaiDonVe == TrangThaiDon.ChoThanhToan) loaiVeDangXuLy.SoLuongDangGiu += soLuongVeDat;
            if (daThanhToan)
            {
                if (trangThaiDonVe != TrangThaiDon.DaHoanTien) loaiVeDangXuLy.SoLuongDaBan += soLuongVeDat;
                for (var veKiemThu = 1; veKiemThu <= soLuongVeDat; veKiemThu++)
                {
                    donVeDangXuLy.DanhSachVe.Add(new Ve
                    {
                        LoaiVe = loaiVeDangXuLy, MaVe = $"SAMPLE-U{soThuTuBanGhi:000}-T{veKiemThu:00}",
                        NoiDungMaQr = $"SAMPLE-QR-U{soThuTuBanGhi:000}-T{veKiemThu:00}", NgayTao = thanhToanKiemThu.NgayThanhToan!.Value,
                        TrangThai = trangThaiDonVe == TrangThaiDon.DaHoanTien ? TrangThaiVe.DaHoanTien : TrangThaiVe.ChuaSuDung
                    });
                    addedTickets++;
                }
            }
            nguCanhDuLieu.DanhSachDonVe.Add(donVeDangXuLy);
        }

        await nguCanhDuLieu.SaveChangesAsync();
        if (giaoDichCoSoDuLieu is not null) await giaoDichCoSoDuLieu.CommitAsync();
        return new UserSeedResult(duLieuConThieu.Length, duLieuConThieu.Length, duLieuConThieu.Length, addedTickets, duLieuDaTonTai.Count);
    }

    public static string TaoEmailNguoiDungMau(int soThuTuBanGhi) => $"nguoidung{soThuTuBanGhi:000}@{EmailDomain}";

    private static TrangThaiDon LayTrangThaiDonMau(int chiSoBanGhi) => (chiSoBanGhi % 10) switch
    {
        < 5 => TrangThaiDon.ThanhCong,
        5 or 6 => TrangThaiDon.ChoThanhToan,
        7 => TrangThaiDon.DaHuy,
        8 => TrangThaiDon.HetHan,
        _ => chiSoBanGhi / 10 % 2 == 0 ? TrangThaiDon.YeuCauHoanTien : TrangThaiDon.DaHoanTien
    };
}
