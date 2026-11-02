using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Exceptions;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using SuKienEntity = QLSK.Domain.Entities.SuKien;

namespace QLSK.Features.QuanTri;

public class QuanTriService(ApplicationDbContext nguCanhDuLieu, IPasswordHasher<QLSK.Domain.Entities.TaiKhoan> boBamMatKhau)
{
    public async Task<object> LayDanhMucQuanTri() => new
    {
        suKien = await nguCanhDuLieu.DanhSachSuKien.AsNoTracking().OrderByDescending(suKienDb => suKienDb.SuKienId).ToListAsync(),
        toChuc = await nguCanhDuLieu.DanhSachBanToChuc.AsNoTracking().OrderBy(banToChucDb => banToChucDb.TenBanToChuc).ToListAsync(),
        taiKhoan = await nguCanhDuLieu.DanhSachTaiKhoan.AsNoTracking().OrderBy(taiKhoanDb => taiKhoanDb.HoTen)
            .Select(taiKhoanDb => new { Id = taiKhoanDb.TaiKhoanId, taiKhoanDb.HoTen, taiKhoanDb.Email, taiKhoanDb.VaiTroId, taiKhoanDb.HoatDong }).ToListAsync(),
        loaiSuKien = await nguCanhDuLieu.DanhSachLoaiSuKien.AsNoTracking().OrderBy(loaiSuKienDb => loaiSuKienDb.TenLoaiSuKien).ToListAsync()
    };

    public async Task<SuKienEntity> TaoSuKienQuanTri(QuanTriSuKienDto duLieuQuanTriSuKien)
    {
        await KiemTraDuLieuSuKienQuanTri(duLieuQuanTriSuKien);
        var suKien = new SuKienEntity();
        nguCanhDuLieu.DanhSachSuKien.Add(suKien);
        GanThongTinSuKien(suKien, duLieuQuanTriSuKien);
        await nguCanhDuLieu.SaveChangesAsync();
        return suKien;
    }

    public async Task<SuKienEntity> CapNhatSuKienQuanTri(int suKienId, QuanTriSuKienDto duLieuQuanTriSuKien)
    {
        await KiemTraDuLieuSuKienQuanTri(duLieuQuanTriSuKien);
        var suKien = await nguCanhDuLieu.DanhSachSuKien.FindAsync(suKienId) ?? throw TaoLoiKhongTimThay();
        // Keep approval, ownership and financial history intact when editing imported events.
        if (suKien.BanToChucId != duLieuQuanTriSuKien.BanToChucId)
            throw new ApiException(409, "OWNER_CHANGE_NOT_ALLOWED", "Không thể chuyển chủ sở hữu của sự kiện hiện có.");
        var datDiaDiem = await nguCanhDuLieu.DanhSachDatDiaDiem.Include(datDiaDiemDb => datDiaDiemDb.DiaDiem).SingleOrDefaultAsync(datDiaDiemDb => datDiaDiemDb.SuKienId == suKien.SuKienId);
        if (datDiaDiem is not null && (suKien.ThoiGianBatDau != duLieuQuanTriSuKien.ThoiGianBatDau || suKien.ThoiGianKetThuc != duLieuQuanTriSuKien.ThoiGianKetThuc))
            throw new ApiException(409, "EVENT_HAS_BOOKING", "Sự kiện đã đặt địa điểm; cần giữ nguyên lịch hiện tại.");
        if (datDiaDiem is not null && duLieuQuanTriSuKien.SoNguoi > datDiaDiem.DiaDiem.SucChua)
            throw new ApiException(409, "VENUE_CAPACITY_TOO_SMALL", "Số người vượt sức chứa địa điểm.");
        if (duLieuQuanTriSuKien.SoNguoi < await nguCanhDuLieu.DanhSachLoaiVe.Where(loaiVeDb => loaiVeDb.SuKienId == suKien.SuKienId).SumAsync(loaiVeDb => loaiVeDb.SoLuongDaBan + loaiVeDb.SoLuongDangGiu))
            throw new ApiException(409, "TICKET_LIMIT_EXCEEDED", "Số người nhỏ hơn số vé đã bán hoặc đang giữ.");
        GanThongTinSuKien(suKien, duLieuQuanTriSuKien);
        await nguCanhDuLieu.SaveChangesAsync();
        return suKien;
    }

    private async Task KiemTraDuLieuSuKienQuanTri(QuanTriSuKienDto duLieuQuanTriSuKien)
    {
        if (duLieuQuanTriSuKien.ThoiGianKetThuc <= duLieuQuanTriSuKien.ThoiGianBatDau || duLieuQuanTriSuKien.SoNguoi <= 0)
            throw new ApiException(400, "VALIDATION_ERROR", "Thời gian hoặc số người không hợp lệ.");
        if (!await nguCanhDuLieu.DanhSachBanToChuc.AnyAsync(banToChucDb => banToChucDb.BanToChucId == duLieuQuanTriSuKien.BanToChucId))
            throw new ApiException(400, "VALIDATION_ERROR", "Không tìm thấy ban tổ chức.");
        if (!await nguCanhDuLieu.DanhSachLoaiSuKien.AnyAsync(loaiSuKienDb => loaiSuKienDb.LoaiSuKienId == duLieuQuanTriSuKien.LoaiSuKienId && loaiSuKienDb.HoatDong))
            throw new ApiException(400, "VALIDATION_ERROR", "Loại sự kiện không hoạt động.");
    }

    private static void GanThongTinSuKien(SuKienEntity suKien, QuanTriSuKienDto duLieuQuanTriSuKien)
    {
        suKien.BanToChucId = duLieuQuanTriSuKien.BanToChucId;
        suKien.LoaiSuKienId = duLieuQuanTriSuKien.LoaiSuKienId;
        suKien.TenSuKien = duLieuQuanTriSuKien.TenSuKien.Trim(); suKien.MoTaSuKien = duLieuQuanTriSuKien.MoTaSuKien.Trim(); suKien.HinhAnh = duLieuQuanTriSuKien.HinhAnh;
        suKien.ThoiGianBatDau = duLieuQuanTriSuKien.ThoiGianBatDau; suKien.ThoiGianKetThuc = duLieuQuanTriSuKien.ThoiGianKetThuc; suKien.SoNguoi = duLieuQuanTriSuKien.SoNguoi;
    }

    public async Task<BanToChuc> TaoBanToChucQuanTri(QuanTriToChucDto duLieuQuanTriToChuc)
    {
        var banToChuc = new BanToChuc();
        var taiKhoan = await nguCanhDuLieu.DanhSachTaiKhoan.FindAsync(duLieuQuanTriToChuc.TaiKhoanId) ?? throw TaoLoiKhongTimThay();
        if (!taiKhoan.HoatDong || taiKhoan.VaiTroId is not (1 or 2))
            throw new ApiException(409, "INVALID_ORGANIZER_ACCOUNT", "Cần chọn tài khoản người dùng đang hoạt động.");
        if (await nguCanhDuLieu.DanhSachBanToChuc.AnyAsync(banToChucDb => banToChucDb.TaiKhoanId == taiKhoan.TaiKhoanId))
            throw new ApiException(409, "ORGANIZER_EXISTS", "Tài khoản đã có ban tổ chức.");
        banToChuc.TaiKhoanId = taiKhoan.TaiKhoanId; taiKhoan.VaiTroId = 2;
        nguCanhDuLieu.DanhSachBanToChuc.Add(banToChuc);
        GanThongTinBanToChuc(banToChuc, duLieuQuanTriToChuc);
        await nguCanhDuLieu.SaveChangesAsync();
        return banToChuc;
    }

    public async Task<BanToChuc> CapNhatBanToChucQuanTri(int banToChucId, QuanTriToChucDto duLieuQuanTriToChuc)
    {
        var banToChuc = await nguCanhDuLieu.DanhSachBanToChuc.FindAsync(banToChucId) ?? throw TaoLoiKhongTimThay();
        if (banToChuc.TaiKhoanId != duLieuQuanTriToChuc.TaiKhoanId)
            throw new ApiException(409, "OWNER_CHANGE_NOT_ALLOWED", "Không thể chuyển tài khoản sở hữu ban tổ chức.");
        _ = await nguCanhDuLieu.DanhSachTaiKhoan.FindAsync(duLieuQuanTriToChuc.TaiKhoanId) ?? throw TaoLoiKhongTimThay();
        GanThongTinBanToChuc(banToChuc, duLieuQuanTriToChuc);
        await nguCanhDuLieu.SaveChangesAsync();
        return banToChuc;
    }

    private static void GanThongTinBanToChuc(BanToChuc banToChuc, QuanTriToChucDto duLieuQuanTriToChuc)
    {
        banToChuc.TenBanToChuc = duLieuQuanTriToChuc.TenBanToChuc.Trim(); banToChuc.MoTaBanToChuc = duLieuQuanTriToChuc.MoTaBanToChuc.Trim();
    }

    public async Task CapNhatTaiKhoanQuanTri(int taiKhoanId, QuanTriTaiKhoanDto duLieuQuanTriTaiKhoan)
    {
        var taiKhoan = await nguCanhDuLieu.DanhSachTaiKhoan.FindAsync(taiKhoanId) ?? throw TaoLoiKhongTimThay();
        var emailTaiKhoan = duLieuQuanTriTaiKhoan.Email.Trim().ToLowerInvariant();
        if (await nguCanhDuLieu.DanhSachTaiKhoan.AnyAsync(taiKhoanDb => taiKhoanDb.TaiKhoanId != taiKhoanId && taiKhoanDb.Email == emailTaiKhoan))
            throw new ApiException(409, "EMAIL_EXISTS", "Email đã được sử dụng.");
        taiKhoan.HoTen = duLieuQuanTriTaiKhoan.HoTen.Trim(); taiKhoan.Email = emailTaiKhoan;
        if (!string.IsNullOrEmpty(duLieuQuanTriTaiKhoan.MatKhauMoi)) taiKhoan.MatKhauBam = boBamMatKhau.HashPassword(taiKhoan, duLieuQuanTriTaiKhoan.MatKhauMoi);
        await nguCanhDuLieu.SaveChangesAsync();
    }

    public async Task XoaSuKienChuaSuDung(int suKienId)
    {
        var suKien = await nguCanhDuLieu.DanhSachSuKien.FindAsync(suKienId) ?? throw TaoLoiKhongTimThay();
        var dangDuocSuDung = await nguCanhDuLieu.DanhSachDonVe.AnyAsync(donVeDb => donVeDb.SuKienId == suKienId) ||
            await nguCanhDuLieu.DanhSachDatDiaDiem.AnyAsync(datDiaDiemDb => datDiaDiemDb.SuKienId == suKienId) ||
            await nguCanhDuLieu.DanhSachLoaiVe.AnyAsync(loaiVeDb => loaiVeDb.SuKienId == suKienId);
        await XoaBanGhiChuaSuDung(suKien, dangDuocSuDung);
    }

    public async Task XoaBanToChucChuaSuDung(int banToChucId)
    {
        var banToChuc = await nguCanhDuLieu.DanhSachBanToChuc.Include(banToChucDb => banToChucDb.TaiKhoan).SingleOrDefaultAsync(banToChucDb => banToChucDb.BanToChucId == banToChucId) ?? throw TaoLoiKhongTimThay();
        var dangDuocSuDung = await nguCanhDuLieu.DanhSachSuKien.AnyAsync(suKienDb => suKienDb.BanToChucId == banToChucId);
        if (!dangDuocSuDung && banToChuc.TaiKhoan.VaiTroId == 2) banToChuc.TaiKhoan.VaiTroId = 1;
        await XoaBanGhiChuaSuDung(banToChuc, dangDuocSuDung);
    }

    public async Task XoaLoaiVeChuaSuDung(int loaiVeId)
    {
        var loaiVe = await nguCanhDuLieu.DanhSachLoaiVe.FindAsync(loaiVeId) ?? throw TaoLoiKhongTimThay();
        var dangDuocSuDung = loaiVe.SoLuongDaBan > 0 || loaiVe.SoLuongDangGiu > 0 ||
            await nguCanhDuLieu.ChiTietDonVe.AnyAsync(chiTietDonVeDb => chiTietDonVeDb.LoaiVeId == loaiVeId) || await nguCanhDuLieu.DanhSachVe.AnyAsync(veDaPhatHanhDb => veDaPhatHanhDb.LoaiVeId == loaiVeId);
        await XoaBanGhiChuaSuDung(loaiVe, dangDuocSuDung);
    }

    public async Task XoaDiaDiemChuaSuDung(int diaDiemId)
    {
        var diaDiem = await nguCanhDuLieu.DanhSachDiaDiem.FindAsync(diaDiemId) ?? throw TaoLoiKhongTimThay();
        var dangDuocSuDung = await nguCanhDuLieu.DanhSachDatDiaDiem.AnyAsync(datDiaDiemDb => datDiaDiemDb.DiaDiemId == diaDiemId);
        await XoaBanGhiChuaSuDung(diaDiem, dangDuocSuDung);
    }

    public async Task XoaLoaiSuKienChuaSuDung(int loaiSuKienId)
    {
        var loaiSuKien = await nguCanhDuLieu.DanhSachLoaiSuKien.FindAsync(loaiSuKienId) ?? throw TaoLoiKhongTimThay();
        var dangDuocSuDung = await nguCanhDuLieu.DanhSachSuKien.AnyAsync(suKienDb => suKienDb.LoaiSuKienId == loaiSuKienId);
        await XoaBanGhiChuaSuDung(loaiSuKien, dangDuocSuDung);
    }

    private async Task XoaBanGhiChuaSuDung(object entityCanXuLy, bool dangDuocSuDung)
    {
        if (dangDuocSuDung) throw new ApiException(409, "RECORD_IN_USE", "Bản ghi đã có dữ liệu liên quan. Hãy giữ lại lịch sử và sử dụng thao tác tắt hoặc đóng bán.");
        nguCanhDuLieu.Remove(entityCanXuLy);
        await nguCanhDuLieu.SaveChangesAsync();
    }

    public async Task<object> LaySuKienVaDonVeTheoTaiKhoan(int taiKhoanId)
    {
        if (!await nguCanhDuLieu.DanhSachTaiKhoan.AnyAsync(taiKhoanDb => taiKhoanDb.TaiKhoanId == taiKhoanId)) throw TaoLoiKhongTimThay();
        return new
        {
            suKien = await nguCanhDuLieu.DanhSachSuKien.AsNoTracking().Where(suKienDb => suKienDb.BanToChuc.TaiKhoanId == taiKhoanId).ToListAsync(),
            donVe = await nguCanhDuLieu.DanhSachDonVe.AsNoTracking().Where(donVeDb => donVeDb.TaiKhoanId == taiKhoanId).ToListAsync()
        };
    }

    private static ApiException TaoLoiKhongTimThay() => new(404, "NOT_FOUND", "Không tìm thấy bản ghi.");
}
