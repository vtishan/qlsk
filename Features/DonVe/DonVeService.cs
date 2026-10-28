using System.Data;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Exceptions;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;

using DonVeEntity = QLSK.Domain.Entities.DonVe;
namespace QLSK.Features.DonVe;
public class DonVeService(ApplicationDbContext db, IConfiguration config)
{
    public async Task<DonVeEntity> Tao(int userId, TaoDonDto dto)
    {
        if (dto.Items.Count == 0) throw new ApiException(400, "VALIDATION_ERROR", "Đơn phải có ít nhất một loại vé.");
        if (dto.Items.GroupBy(x => x.LoaiVeId).Any(g => g.Count() > 1)) throw new ApiException(400, "VALIDATION_ERROR", "Không được lặp loại vé trong đơn.");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var ids = dto.Items.Select(x => x.LoaiVeId).OrderBy(x => x).ToList();
        var types = await db.LoaiVes.Include(x => x.SuKien).Where(x => ids.Contains(x.Id)).OrderBy(x => x.Id).ToListAsync();
        if (types.Count != ids.Count) throw new ApiException(404, "NOT_FOUND", "Có loại vé không tồn tại.");
        var eventId = types[0].SuKienId;
        if (types.Any(x => x.SuKienId != eventId)) throw new ApiException(400, "VALIDATION_ERROR", "Các loại vé phải thuộc cùng sự kiện.");
        if (types[0].SuKien.TrangThai != TrangThaiSuKien.DaDuyet) throw new ApiException(409, "EVENT_NOT_APPROVED", "Sự kiện chưa được duyệt.");
        var now = DateTime.UtcNow;
        decimal total = 0; int count = 0;
        foreach (var type in types)
        {
            var item = dto.Items.Single(x => x.LoaiVeId == type.Id);
            if (item.SoLuong <= 0 || item.SoLuong > 20) throw new ApiException(400, "TICKET_LIMIT_EXCEEDED", "Số lượng vé không hợp lệ.");
            if (now < type.MoBan || now > type.DongBan) throw new ApiException(409, "TICKET_NOT_ON_SALE", $"Loại vé {type.Ten} chưa mở bán hoặc đã đóng bán.");
            if (type.DaBan + type.DangGiu + item.SoLuong > type.SoLuong) throw new ApiException(409, "TICKET_SOLD_OUT", $"Loại vé {type.Ten} không đủ số lượng.");
            type.DangGiu += item.SoLuong;
            count += item.SoLuong; total += type.Gia * item.SoLuong;
        }
        var minutes = Math.Clamp(config.GetValue("Order:ExpireMinutes", 15), 1, 120);
        var order = new DonVeEntity { TaiKhoanId = userId, SuKienId = eventId, SoLuong = count, TongTien = total, HetHan = now.AddMinutes(minutes) };
        foreach (var type in types)
        {
            var item = dto.Items.Single(x => x.LoaiVeId == type.Id);
            order.ChiTiets.Add(new ChiTietDonVe { LoaiVeId = type.Id, SoLuong = item.SoLuong, DonGia = type.Gia, ThanhTien = type.Gia * item.SoLuong });
        }
        db.DonVes.Add(order); await db.SaveChangesAsync(); await tx.CommitAsync(); return await Lay(userId, order.Id, false);
    }

    public async Task<List<DonVeEntity>> CuaToi(int userId)
        => await db.DonVes.AsNoTracking().Where(x => x.TaiKhoanId == userId).Include(x => x.SuKien).Include(x => x.ChiTiets).ThenInclude(x => x.LoaiVe).OrderByDescending(x => x.Id).ToListAsync();

    public async Task<DonVeEntity> Lay(int userId, int id, bool manager)
    {
        var x = await db.DonVes.AsNoTracking().Include(x => x.SuKien).ThenInclude(x => x.BanToChuc).Include(x => x.ChiTiets).ThenInclude(x => x.LoaiVe).Include(x => x.ThanhToans).Include(x => x.Ves).SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy đơn vé.");
        var owner = x.TaiKhoanId == userId;
        var organizer = x.SuKien.BanToChuc.TaiKhoanId == userId;
        if (!owner && !organizer && !manager) throw new ApiException(403, "FORBIDDEN", "Không có quyền xem đơn.");
        return x;
    }

    public async Task Huy(int userId, int id)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var order = await db.DonVes.Include(x => x.ChiTiets).SingleOrDefaultAsync(x => x.Id == id && x.TaiKhoanId == userId)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy đơn vé.");
        if (order.TrangThai != TrangThaiDon.ChoThanhToan) throw new ApiException(409, "ORDER_INVALID_STATE", "Đơn không thể hủy.");
        await TraVeGiu(order);
        order.TrangThai = TrangThaiDon.DaHuy;
        await db.SaveChangesAsync(); await tx.CommitAsync();
    }

    public async Task<int> HetHan()
    {
        var now = DateTime.UtcNow;
        var ids = await db.DonVes.AsNoTracking().Where(x => x.TrangThai == TrangThaiDon.ChoThanhToan && x.HetHan < now).OrderBy(x => x.HetHan).ThenBy(x => x.Id).Select(x => x.Id).Take(100).ToListAsync();
        var done = 0;
        foreach (var id in ids)
        {
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var order = await db.DonVes.Include(x => x.ChiTiets).SingleOrDefaultAsync(x => x.Id == id);
            if (order == null || order.TrangThai != TrangThaiDon.ChoThanhToan || order.HetHan >= DateTime.UtcNow) { await tx.RollbackAsync(); continue; }
            await TraVeGiu(order); order.TrangThai = TrangThaiDon.HetHan; await db.SaveChangesAsync(); await tx.CommitAsync(); done++;
        }
        return done;
    }

    async Task TraVeGiu(DonVeEntity order)
    {
        var ids = order.ChiTiets.Select(x => x.LoaiVeId).OrderBy(x => x).ToList();
        var types = await db.LoaiVes.Where(x => ids.Contains(x.Id)).OrderBy(x => x.Id).ToListAsync();
        foreach (var detail in order.ChiTiets)
        {
            var type = types.Single(x => x.Id == detail.LoaiVeId);
            type.DangGiu = Math.Max(0, type.DangGiu - detail.SoLuong);
        }
    }
}
