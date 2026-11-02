using Microsoft.EntityFrameworkCore;
using QLSK.Common.Exceptions;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;

using SuKienEntity = QLSK.Domain.Entities.SuKien;
namespace QLSK.Features.SuKien;
public class SuKienService(ApplicationDbContext nguCanhDuLieu)
{
    public async Task<List<LoaiSuKien>> LayDanhSachLoaiSuKien(bool baoGomNgungHoatDong = false)
        => await nguCanhDuLieu.DanhSachLoaiSuKien.AsNoTracking().Where(loaiSuKienDb => baoGomNgungHoatDong || loaiSuKienDb.HoatDong).OrderBy(loaiSuKienDb => loaiSuKienDb.TenLoaiSuKien).ToListAsync();

    public async Task<LoaiSuKien> TaoLoaiSuKien(LoaiSuKienDto duLieuLoaiSuKien)
    {
        if (await nguCanhDuLieu.DanhSachLoaiSuKien.AnyAsync(loaiSuKienDb => loaiSuKienDb.TenLoaiSuKien == duLieuLoaiSuKien.TenLoaiSuKien.Trim())) throw new ApiException(409, "TYPE_EXISTS", "Loại sự kiện đã tồn tại.");
        var loaiSuKien = new LoaiSuKien { TenLoaiSuKien = duLieuLoaiSuKien.TenLoaiSuKien.Trim(), MoTaLoaiSuKien = (duLieuLoaiSuKien.MoTaLoaiSuKien ?? "").Trim(), HoatDong = duLieuLoaiSuKien.HoatDong };
        nguCanhDuLieu.DanhSachLoaiSuKien.Add(loaiSuKien); await nguCanhDuLieu.SaveChangesAsync(); return loaiSuKien;
    }

    public async Task<LoaiSuKien> CapNhatLoaiSuKien(int loaiSuKienId, LoaiSuKienDto duLieuLoaiSuKien)
    {
        var loaiSuKien = await nguCanhDuLieu.DanhSachLoaiSuKien.FindAsync(loaiSuKienId) ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy loại sự kiện.");
        if (await nguCanhDuLieu.DanhSachLoaiSuKien.AnyAsync(loaiSuKienDb => loaiSuKienDb.LoaiSuKienId != loaiSuKienId && loaiSuKienDb.TenLoaiSuKien == duLieuLoaiSuKien.TenLoaiSuKien.Trim())) throw new ApiException(409, "TYPE_EXISTS", "Loại sự kiện đã tồn tại.");
        loaiSuKien.TenLoaiSuKien = duLieuLoaiSuKien.TenLoaiSuKien.Trim(); loaiSuKien.MoTaLoaiSuKien = (duLieuLoaiSuKien.MoTaLoaiSuKien ?? "").Trim(); loaiSuKien.HoatDong = duLieuLoaiSuKien.HoatDong; await nguCanhDuLieu.SaveChangesAsync(); return loaiSuKien;
    }

    public async Task<List<SuKienEntity>> LayDanhSachSuKien(bool laNguoiQuanLy = false)
    {
        var truyVanSuKien = nguCanhDuLieu.DanhSachSuKien.AsNoTracking().Include(suKienDb => suKienDb.LoaiSuKien).Include(suKienDb => suKienDb.BanToChuc).AsQueryable();
        if (!laNguoiQuanLy) truyVanSuKien = truyVanSuKien.Where(suKienDb => suKienDb.TrangThai == TrangThaiSuKien.DaDuyet || suKienDb.TrangThai == TrangThaiSuKien.DangDienRa || suKienDb.TrangThai == TrangThaiSuKien.DaKetThuc);
        return await truyVanSuKien.OrderBy(suKienDb => suKienDb.ThoiGianBatDau).ToListAsync();
    }

    public async Task<SuKienEntity> LayChiTietSuKien(int suKienId, int? taiKhoanId = null, bool laNguoiQuanLy = false)
    {
        var suKien = await nguCanhDuLieu.DanhSachSuKien.AsNoTracking().Include(suKienDb => suKienDb.LoaiSuKien).Include(suKienDb => suKienDb.BanToChuc).ThenInclude(banToChucDb => banToChucDb.TaiKhoan)
            .Include(suKienDb => suKienDb.DatDiaDiem).ThenInclude(datDiaDiemDb => datDiaDiemDb!.DiaDiem).Include(suKienDb => suKienDb.DanhSachLoaiVe).SingleOrDefaultAsync(suKienDb => suKienDb.SuKienId == suKienId)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy sự kiện.");
        var laTrangThaiCongKhai = suKien.TrangThai is TrangThaiSuKien.DaDuyet or TrangThaiSuKien.DangDienRa or TrangThaiSuKien.DaKetThuc;
        var laChuSoHuu = taiKhoanId.HasValue && suKien.BanToChuc.TaiKhoanId == taiKhoanId.Value;
        if (!laTrangThaiCongKhai && !laNguoiQuanLy && !laChuSoHuu) throw new ApiException(403, "FORBIDDEN", "Không có quyền xem sự kiện.");
        return suKien;
    }

    public async Task<List<SuKienEntity>> LaySuKienCuaTaiKhoan(int taiKhoanId)
    {
        var banToChucId = await LayBanToChucId(taiKhoanId);
        return await nguCanhDuLieu.DanhSachSuKien.AsNoTracking().Where(suKienDb => suKienDb.BanToChucId == banToChucId).OrderByDescending(suKienDb => suKienDb.SuKienId).ToListAsync();
    }

    public async Task<SuKienEntity> TaoSuKien(int taiKhoanId, TaoSuKienDto duLieuTaoSuKien)
    {
        KiemTraDuLieuSuKien(duLieuTaoSuKien);
        var banToChucId = await LayBanToChucId(taiKhoanId);
        if (!await nguCanhDuLieu.DanhSachLoaiSuKien.AnyAsync(loaiSuKienDb => loaiSuKienDb.LoaiSuKienId == duLieuTaoSuKien.LoaiSuKienId && loaiSuKienDb.HoatDong)) throw new ApiException(400, "VALIDATION_ERROR", "Loại sự kiện không hoạt động.");
        var suKien = new SuKienEntity { BanToChucId = banToChucId, LoaiSuKienId = duLieuTaoSuKien.LoaiSuKienId, TenSuKien = duLieuTaoSuKien.TenSuKien.Trim(), MoTaSuKien = (duLieuTaoSuKien.MoTaSuKien ?? "").Trim(), HinhAnh = duLieuTaoSuKien.HinhAnh, ThoiGianBatDau = duLieuTaoSuKien.ThoiGianBatDau, ThoiGianKetThuc = duLieuTaoSuKien.ThoiGianKetThuc, SoNguoi = duLieuTaoSuKien.SoNguoi, TrangThai = TrangThaiSuKien.ChoDuyet };
        nguCanhDuLieu.DanhSachSuKien.Add(suKien); await nguCanhDuLieu.SaveChangesAsync(); return suKien;
    }

    public async Task<SuKienEntity> CapNhatVaGuiLaiSuKien(int taiKhoanId, int suKienId, TaoSuKienDto duLieuTaoSuKien)
    {
        KiemTraDuLieuSuKien(duLieuTaoSuKien);
        var suKien = await LaySuKienVaKiemTraChuSoHuu(taiKhoanId, suKienId);
        if (suKien.TrangThai is not (TrangThaiSuKien.ChoDuyet or TrangThaiSuKien.TuChoi)) throw new ApiException(409, "EVENT_INVALID_STATE", "Không thể sửa sự kiện ở trạng thái hiện tại.");
        if (!await nguCanhDuLieu.DanhSachLoaiSuKien.AnyAsync(loaiSuKienDb => loaiSuKienDb.LoaiSuKienId == duLieuTaoSuKien.LoaiSuKienId && loaiSuKienDb.HoatDong)) throw new ApiException(400, "VALIDATION_ERROR", "Loại sự kiện không hoạt động.");
        suKien.LoaiSuKienId = duLieuTaoSuKien.LoaiSuKienId; suKien.TenSuKien = duLieuTaoSuKien.TenSuKien.Trim(); suKien.MoTaSuKien = (duLieuTaoSuKien.MoTaSuKien ?? "").Trim(); suKien.HinhAnh = duLieuTaoSuKien.HinhAnh; suKien.ThoiGianBatDau = duLieuTaoSuKien.ThoiGianBatDau; suKien.ThoiGianKetThuc = duLieuTaoSuKien.ThoiGianKetThuc; suKien.SoNguoi = duLieuTaoSuKien.SoNguoi; suKien.TrangThai = TrangThaiSuKien.ChoDuyet;
        await nguCanhDuLieu.SaveChangesAsync(); return suKien;
    }

    public async Task<SuKienEntity> GuiDuyetSuKien(int taiKhoanId, int suKienId)
    {
        var suKien = await LaySuKienVaKiemTraChuSoHuu(taiKhoanId, suKienId);
        if (suKien.TrangThai is not (TrangThaiSuKien.ChoDuyet or TrangThaiSuKien.TuChoi)) throw new ApiException(409, "EVENT_INVALID_STATE", "Sự kiện không thể gửi duyệt.");
        suKien.TrangThai = TrangThaiSuKien.ChoDuyet; await nguCanhDuLieu.SaveChangesAsync(); return suKien;
    }

    public async Task DuyetSuKien(int nguoiDuyetId, int suKienId)
    {
        var suKien = await nguCanhDuLieu.DanhSachSuKien.Include(suKienDb => suKienDb.BanToChuc).SingleOrDefaultAsync(suKienDb => suKienDb.SuKienId == suKienId) ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy sự kiện.");
        if (suKien.TrangThai != TrangThaiSuKien.ChoDuyet) throw new ApiException(409, "EVENT_INVALID_STATE", "Sự kiện không ở trạng thái chờ duyệt.");
        if (suKien.BanToChuc.TaiKhoanId == nguoiDuyetId) throw new ApiException(403, "FORBIDDEN", "Người tạo không được tự duyệt sự kiện.");
        suKien.TrangThai = TrangThaiSuKien.DaDuyet; await nguCanhDuLieu.SaveChangesAsync();
    }

    public async Task TuChoiSuKien(int nguoiDuyetId, int suKienId, string lyDoTuChoi)
    {
        var suKien = await nguCanhDuLieu.DanhSachSuKien.Include(suKienDb => suKienDb.BanToChuc).SingleOrDefaultAsync(suKienDb => suKienDb.SuKienId == suKienId) ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy sự kiện.");
        if (suKien.TrangThai != TrangThaiSuKien.ChoDuyet) throw new ApiException(409, "EVENT_INVALID_STATE", "Sự kiện không ở trạng thái chờ duyệt.");
        if (suKien.BanToChuc.TaiKhoanId == nguoiDuyetId) throw new ApiException(403, "FORBIDDEN", "Người tạo không được tự xử lý sự kiện.");
        suKien.TrangThai = TrangThaiSuKien.TuChoi;
        suKien.MoTaSuKien = string.IsNullOrWhiteSpace(lyDoTuChoi) ? suKien.MoTaSuKien : $"{suKien.MoTaSuKien}\n[Lý do từ chối: {lyDoTuChoi.Trim()}]";
        await nguCanhDuLieu.SaveChangesAsync();
    }

    public async Task HuySuKien(int taiKhoanId, int suKienId)
    {
        var suKien = await LaySuKienVaKiemTraChuSoHuu(taiKhoanId, suKienId);
        if (suKien.TrangThai is TrangThaiSuKien.DaKetThuc or TrangThaiSuKien.DaHuy) throw new ApiException(409, "EVENT_INVALID_STATE", "Không thể hủy sự kiện.");
        suKien.TrangThai = TrangThaiSuKien.DaHuy; await nguCanhDuLieu.SaveChangesAsync();
    }

    async Task<int> LayBanToChucId(int taiKhoanId)
    {
        var banToChuc = await nguCanhDuLieu.DanhSachBanToChuc.AsNoTracking().SingleOrDefaultAsync(banToChucDb => banToChucDb.TaiKhoanId == taiKhoanId)
            ?? throw new ApiException(403, "ORGANIZER_NOT_VERIFIED", "Ban tổ chức chưa được xác thực.");
        return banToChuc.BanToChucId;
    }
    async Task<SuKienEntity> LaySuKienVaKiemTraChuSoHuu(int taiKhoanId, int suKienId)
    {
        var suKien = await nguCanhDuLieu.DanhSachSuKien.Include(suKienDb => suKienDb.BanToChuc).SingleOrDefaultAsync(suKienDb => suKienDb.SuKienId == suKienId) ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy sự kiện.");
        if (suKien.BanToChuc.TaiKhoanId != taiKhoanId) throw new ApiException(403, "FORBIDDEN", "Không phải sự kiện của bạn.");
        return suKien;
    }
    static void KiemTraDuLieuSuKien(TaoSuKienDto duLieuTaoSuKien)
    {
        if (duLieuTaoSuKien.ThoiGianKetThuc <= duLieuTaoSuKien.ThoiGianBatDau) throw new ApiException(400, "VALIDATION_ERROR", "Thời gian kết thúc phải sau thời gian bắt đầu.");
        if (duLieuTaoSuKien.SoNguoi <= 0) throw new ApiException(400, "VALIDATION_ERROR", "Số người phải lớn hơn 0.");
    }
    public async Task CapNhatVongDoiSuKienTheoThoiGian(CancellationToken tokenDungWorker)
    {
        var thoiDiemHienTai = DateTime.UtcNow;

        var suKienDangBatDau = await nguCanhDuLieu.DanhSachSuKien
            .Where(suKienDb =>
                suKienDb.TrangThai == TrangThaiSuKien.DaDuyet &&
                suKienDb.ThoiGianBatDau <= thoiDiemHienTai &&
                suKienDb.ThoiGianKetThuc > thoiDiemHienTai)
            .ToListAsync(tokenDungWorker);

        foreach (var suKien in suKienDangBatDau)
        {
            suKien.TrangThai =
                TrangThaiSuKien.DangDienRa;
        }

        var suKienDaKetThuc = await nguCanhDuLieu.DanhSachSuKien
            .Where(suKienDb =>
                (
                    suKienDb.TrangThai == TrangThaiSuKien.DaDuyet ||
                    suKienDb.TrangThai == TrangThaiSuKien.DangDienRa
                )
                && suKienDb.ThoiGianKetThuc <= thoiDiemHienTai)
            .ToListAsync(tokenDungWorker);

        foreach (var suKien in suKienDaKetThuc)
        {
            suKien.TrangThai =
                TrangThaiSuKien.DaKetThuc;
        }

        if (suKienDangBatDau.Count + suKienDaKetThuc.Count > 0)
        {
            await nguCanhDuLieu.SaveChangesAsync(tokenDungWorker);
        }
    }
}
