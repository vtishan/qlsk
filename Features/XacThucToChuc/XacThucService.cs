using Microsoft.EntityFrameworkCore;
using QLSK.Common.Exceptions;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;

namespace QLSK.Features.XacThucToChuc;
public class XacThucService(ApplicationDbContext nguCanhDuLieu)
{
    public async Task<YeuCauXacThucToChuc> GuiYeuCauXacThucToChuc(int taiKhoanId, GuiYeuCauDto duLieuGuiYeuCau)
    {
        var taiKhoan = await nguCanhDuLieu.DanhSachTaiKhoan.Include(taiKhoanDb => taiKhoanDb.BanToChuc).SingleOrDefaultAsync(taiKhoanDb => taiKhoanDb.TaiKhoanId == taiKhoanId)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy tài khoản.");
        if (taiKhoan.BanToChuc != null) throw new ApiException(409, "ORGANIZER_ALREADY_VERIFIED", "Tài khoản đã là ban tổ chức.");
        if (await nguCanhDuLieu.YeuCauXacThucToChuc.AnyAsync(yeuCauXacThucDb => yeuCauXacThucDb.TaiKhoanId == taiKhoanId))
            throw new ApiException(409, "VERIFICATION_ALREADY_PENDING", "Đã có yêu cầu đang chờ xử lý.");
        var yeuCauXacThuc = new YeuCauXacThucToChuc { TaiKhoanId = taiKhoanId, TenToChuc = duLieuGuiYeuCau.TenToChuc.Trim(), GiayTo = duLieuGuiYeuCau.GiayTo.Trim() };
        nguCanhDuLieu.YeuCauXacThucToChuc.Add(yeuCauXacThuc);
        await nguCanhDuLieu.SaveChangesAsync();
        return yeuCauXacThuc;
    }

    public async Task<YeuCauXacThucToChuc?> LayYeuCauDangXuLyCuaTaiKhoan(int taiKhoanId) => await nguCanhDuLieu.YeuCauXacThucToChuc.AsNoTracking().SingleOrDefaultAsync(yeuCauXacThucDb => yeuCauXacThucDb.TaiKhoanId == taiKhoanId);

    public async Task<List<YeuCauXacThucToChuc>> LayDanhSachYeuCauDangXuLy() => await nguCanhDuLieu.YeuCauXacThucToChuc.AsNoTracking().Include(yeuCauXacThucDb => yeuCauXacThucDb.TaiKhoan).OrderBy(yeuCauXacThucDb => yeuCauXacThucDb.NgayTao).ToListAsync();

    public async Task DuyetYeuCauXacThuc(int nguoiDuyetId, int yeuCauXacThucId)
    {
        await using var giaoDich = await nguCanhDuLieu.Database.BeginTransactionAsync();
        var yeuCauXacThuc = await nguCanhDuLieu.YeuCauXacThucToChuc.Include(yeuCauXacThucDb => yeuCauXacThucDb.TaiKhoan).SingleOrDefaultAsync(yeuCauXacThucDb => yeuCauXacThucDb.YeuCauXacThucToChucId == yeuCauXacThucId)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy yêu cầu.");
        if (yeuCauXacThuc.TrangThai != TrangThaiYeuCau.ChoDuyet) throw new ApiException(409, "VERIFICATION_INVALID_STATE", "Yêu cầu không còn ở trạng thái chờ duyệt.");
        var thoiDiemHienTai = DateTime.UtcNow;
        nguCanhDuLieu.LichSuYeuCauXacThucToChuc.Add(new LichSuYeuCauXacThucToChuc
        {
            TaiKhoanId = yeuCauXacThuc.TaiKhoanId, TenToChuc = yeuCauXacThuc.TenToChuc, GiayTo = yeuCauXacThuc.GiayTo,
            TrangThai = TrangThaiYeuCau.DaDuyet, NgayTao = yeuCauXacThuc.NgayTao, NgayXuLy = thoiDiemHienTai, NguoiXuLyId = nguoiDuyetId
        });
        nguCanhDuLieu.DanhSachBanToChuc.Add(new BanToChuc { TaiKhoanId = yeuCauXacThuc.TaiKhoanId, TenBanToChuc = yeuCauXacThuc.TenToChuc, NgayXacThuc = thoiDiemHienTai });
        yeuCauXacThuc.TaiKhoan.VaiTroId = 2;
        nguCanhDuLieu.YeuCauXacThucToChuc.Remove(yeuCauXacThuc);
        await nguCanhDuLieu.SaveChangesAsync();
        await giaoDich.CommitAsync();
    }

    public async Task TuChoiYeuCauXacThuc(int nguoiDuyetId, int yeuCauXacThucId, string lyDoTuChoi)
    {
        await using var giaoDich = await nguCanhDuLieu.Database.BeginTransactionAsync();
        var yeuCauXacThuc = await nguCanhDuLieu.YeuCauXacThucToChuc.SingleOrDefaultAsync(yeuCauXacThucDb => yeuCauXacThucDb.YeuCauXacThucToChucId == yeuCauXacThucId)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy yêu cầu.");
        var thoiDiemHienTai = DateTime.UtcNow;
        nguCanhDuLieu.LichSuYeuCauXacThucToChuc.Add(new LichSuYeuCauXacThucToChuc
        {
            TaiKhoanId = yeuCauXacThuc.TaiKhoanId, TenToChuc = yeuCauXacThuc.TenToChuc, GiayTo = yeuCauXacThuc.GiayTo, LyDo = lyDoTuChoi.Trim(),
            TrangThai = TrangThaiYeuCau.TuChoi, NgayTao = yeuCauXacThuc.NgayTao, NgayXuLy = thoiDiemHienTai, NguoiXuLyId = nguoiDuyetId
        });
        nguCanhDuLieu.YeuCauXacThucToChuc.Remove(yeuCauXacThuc);
        await nguCanhDuLieu.SaveChangesAsync();
        await giaoDich.CommitAsync();
    }
}
