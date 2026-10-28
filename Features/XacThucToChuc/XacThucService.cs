using Microsoft.EntityFrameworkCore;
using QLSK.Common.Exceptions;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;

namespace QLSK.Features.XacThucToChuc;
public class XacThucService(ApplicationDbContext db)
{
    public async Task<YeuCauXacThucToChuc> Gui(int userId, GuiYeuCauDto dto)
    {
        var user = await db.TaiKhoans.Include(x => x.BanToChuc).SingleOrDefaultAsync(x => x.Id == userId)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy tài khoản.");
        if (user.BanToChuc != null) throw new ApiException(409, "ORGANIZER_ALREADY_VERIFIED", "Tài khoản đã là ban tổ chức.");
        if (await db.YeuCauXacThucToChucs.AnyAsync(x => x.TaiKhoanId == userId))
            throw new ApiException(409, "VERIFICATION_ALREADY_PENDING", "Đã có yêu cầu đang chờ xử lý.");
        var req = new YeuCauXacThucToChuc { TaiKhoanId = userId, TenToChuc = dto.TenToChuc.Trim(), GiayTo = dto.GiayTo.Trim() };
        db.YeuCauXacThucToChucs.Add(req);
        await db.SaveChangesAsync();
        return req;
    }

    public async Task<YeuCauXacThucToChuc?> CuaToi(int userId) => await db.YeuCauXacThucToChucs.AsNoTracking().SingleOrDefaultAsync(x => x.TaiKhoanId == userId);

    public async Task<List<YeuCauXacThucToChuc>> DanhSach() => await db.YeuCauXacThucToChucs.AsNoTracking().Include(x => x.TaiKhoan).OrderBy(x => x.NgayTao).ToListAsync();

    public async Task Duyet(int managerId, int id)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        var req = await db.YeuCauXacThucToChucs.Include(x => x.TaiKhoan).SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy yêu cầu.");
        if (req.TrangThai != TrangThaiYeuCau.ChoDuyet) throw new ApiException(409, "VERIFICATION_INVALID_STATE", "Yêu cầu không còn ở trạng thái chờ duyệt.");
        var now = DateTime.UtcNow;
        db.LichSuYeuCauXacThucToChucs.Add(new LichSuYeuCauXacThucToChuc
        {
            TaiKhoanId = req.TaiKhoanId, TenToChuc = req.TenToChuc, GiayTo = req.GiayTo,
            TrangThai = TrangThaiYeuCau.DaDuyet, NgayTao = req.NgayTao, NgayXuLy = now, NguoiXuLyId = managerId
        });
        db.BanToChucs.Add(new BanToChuc { TaiKhoanId = req.TaiKhoanId, Ten = req.TenToChuc, NgayXacThuc = now });
        req.TaiKhoan.VaiTroId = 2;
        db.YeuCauXacThucToChucs.Remove(req);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task TuChoi(int managerId, int id, string lyDo)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        var req = await db.YeuCauXacThucToChucs.SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy yêu cầu.");
        var now = DateTime.UtcNow;
        db.LichSuYeuCauXacThucToChucs.Add(new LichSuYeuCauXacThucToChuc
        {
            TaiKhoanId = req.TaiKhoanId, TenToChuc = req.TenToChuc, GiayTo = req.GiayTo, LyDo = lyDo.Trim(),
            TrangThai = TrangThaiYeuCau.TuChoi, NgayTao = req.NgayTao, NgayXuLy = now, NguoiXuLyId = managerId
        });
        db.YeuCauXacThucToChucs.Remove(req);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }
}
