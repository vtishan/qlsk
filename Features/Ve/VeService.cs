using Microsoft.EntityFrameworkCore;
using QLSK.Common.Exceptions;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using VeEntity = QLSK.Domain.Entities.Ve;
namespace QLSK.Features.Ve;
public class VeService(ApplicationDbContext db)
{
    public async Task<List<VeEntity>> CuaToi(int userId) => await db.Ves.AsNoTracking().Where(x => x.DonVe.TaiKhoanId == userId).Include(x => x.DonVe).Include(x => x.LoaiVe).ThenInclude(x => x.SuKien).OrderByDescending(x => x.Id).ToListAsync();
    public async Task<VeEntity> Lay(int userId, int id, bool manager)
    {
        var x = await db.Ves.AsNoTracking().Include(x => x.DonVe).Include(x => x.LoaiVe).ThenInclude(x => x.SuKien).SingleOrDefaultAsync(x => x.Id == id) ?? throw new ApiException(404,"NOT_FOUND","Không tìm thấy vé.");
        if (!manager && x.DonVe.TaiKhoanId != userId) throw new ApiException(403,"FORBIDDEN","Không có quyền."); return x;
    }
    public async Task<VeEntity> Ma(int userId, string code, bool manager)
    {
        var x = await db.Ves.AsNoTracking().Include(x=>x.DonVe).Include(x=>x.LoaiVe).ThenInclude(x=>x.SuKien).SingleOrDefaultAsync(x=>x.MaVe==code) ?? throw new ApiException(404,"NOT_FOUND","Không tìm thấy vé.");
        if(!manager && x.DonVe.TaiKhoanId!=userId) throw new ApiException(403,"FORBIDDEN","Không có quyền."); return x;
    }
    public async Task<VeEntity> CheckIn(string code)
    {
        var x = await db.Ves.Include(x=>x.LoaiVe).ThenInclude(x=>x.SuKien).SingleOrDefaultAsync(x=>x.MaVe==code || x.MaQR==code) ?? throw new ApiException(404,"NOT_FOUND","Không tìm thấy vé.");
        if(x.TrangThai==TrangThaiVe.DaCheckIn) throw new ApiException(409,"TICKET_ALREADY_CHECKED_IN","Vé đã check-in.");
        if(x.TrangThai!=TrangThaiVe.ChuaSuDung) throw new ApiException(409,"TICKET_INVALID_STATE","Vé không hợp lệ.");
        var now = DateTime.UtcNow;
        if(x.LoaiVe.SuKien.TrangThai != TrangThaiSuKien.DangDienRa || now < x.LoaiVe.SuKien.BatDau || now >= x.LoaiVe.SuKien.KetThuc)
            throw new ApiException(409,"EVENT_NOT_IN_PROGRESS","Sự kiện chưa diễn ra hoặc đã kết thúc.");
        x.TrangThai=TrangThaiVe.DaCheckIn; x.NgayCheckIn=now; await db.SaveChangesAsync(); return x;
    }
}
