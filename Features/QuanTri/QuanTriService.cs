using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Exceptions;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using SuKienEntity = QLSK.Domain.Entities.SuKien;

namespace QLSK.Features.QuanTri;

public class QuanTriService(ApplicationDbContext db, IPasswordHasher<QLSK.Domain.Entities.TaiKhoan> hasher)
{
    public async Task<object> DanhMuc() => new
    {
        suKien = await db.SuKiens.AsNoTracking().OrderByDescending(x => x.Id).ToListAsync(),
        toChuc = await db.BanToChucs.AsNoTracking().OrderBy(x => x.Ten).ToListAsync(),
        taiKhoan = await db.TaiKhoans.AsNoTracking().OrderBy(x => x.HoTen)
            .Select(x => new { x.Id, x.HoTen, x.Email, x.VaiTroId, x.HoatDong }).ToListAsync(),
        loaiSuKien = await db.LoaiSuKiens.AsNoTracking().OrderBy(x => x.Ten).ToListAsync()
    };

    public async Task<SuKienEntity> LuuSuKien(int? id, QuanTriSuKienDto dto)
    {
        if (dto.KetThuc <= dto.BatDau || dto.SoNguoi <= 0)
            throw new ApiException(400, "VALIDATION_ERROR", "Thời gian hoặc số người không hợp lệ.");
        if (!await db.BanToChucs.AnyAsync(x => x.Id == dto.BanToChucId))
            throw new ApiException(400, "VALIDATION_ERROR", "Không tìm thấy ban tổ chức.");
        if (!await db.LoaiSuKiens.AnyAsync(x => x.Id == dto.LoaiSuKienId && x.HoatDong))
            throw new ApiException(400, "VALIDATION_ERROR", "Loại sự kiện không hoạt động.");
        var item = id.HasValue
            ? await db.SuKiens.FindAsync(id.Value) ?? throw Missing()
            : new SuKienEntity();
        if (id.HasValue)
        {
            // Keep approval, ownership and financial history intact when editing imported events.
            if (item.BanToChucId != dto.BanToChucId)
                throw new ApiException(409, "OWNER_CHANGE_NOT_ALLOWED", "Không thể chuyển chủ sở hữu của sự kiện hiện có.");
            var booking = await db.DatDiaDiems.Include(x => x.DiaDiem).SingleOrDefaultAsync(x => x.SuKienId == item.Id);
            if (booking is not null && (item.BatDau != dto.BatDau || item.KetThuc != dto.KetThuc))
                throw new ApiException(409, "EVENT_HAS_BOOKING", "Sự kiện đã đặt địa điểm; cần giữ nguyên lịch hiện tại.");
            if (booking is not null && dto.SoNguoi > booking.DiaDiem.SucChua)
                throw new ApiException(409, "VENUE_CAPACITY_TOO_SMALL", "Số người vượt sức chứa địa điểm.");
            if (dto.SoNguoi < await db.LoaiVes.Where(x => x.SuKienId == item.Id).SumAsync(x => x.DaBan + x.DangGiu))
                throw new ApiException(409, "TICKET_LIMIT_EXCEEDED", "Số người nhỏ hơn số vé đã bán hoặc đang giữ.");
        }
        else db.SuKiens.Add(item);
        item.BanToChucId = dto.BanToChucId;
        item.LoaiSuKienId = dto.LoaiSuKienId;
        item.Ten = dto.Ten.Trim(); item.MoTa = dto.MoTa.Trim(); item.HinhAnh = dto.HinhAnh;
        item.BatDau = dto.BatDau; item.KetThuc = dto.KetThuc; item.SoNguoi = dto.SoNguoi;
        await db.SaveChangesAsync();
        return item;
    }

    public async Task<BanToChuc> LuuToChuc(int? id, QuanTriToChucDto dto)
    {
        var item = id.HasValue ? await db.BanToChucs.FindAsync(id.Value) ?? throw Missing() : new BanToChuc();
        if (id.HasValue && item.TaiKhoanId != dto.TaiKhoanId)
            throw new ApiException(409, "OWNER_CHANGE_NOT_ALLOWED", "Không thể chuyển tài khoản sở hữu ban tổ chức.");
        var user = await db.TaiKhoans.FindAsync(dto.TaiKhoanId) ?? throw Missing();
        if (!id.HasValue)
        {
            if (!user.HoatDong || user.VaiTroId is not (1 or 2))
                throw new ApiException(409, "INVALID_ORGANIZER_ACCOUNT", "Cần chọn tài khoản người dùng đang hoạt động.");
            if (await db.BanToChucs.AnyAsync(x => x.TaiKhoanId == user.Id))
                throw new ApiException(409, "ORGANIZER_EXISTS", "Tài khoản đã có ban tổ chức.");
            item.TaiKhoanId = user.Id; user.VaiTroId = 2;
            db.BanToChucs.Add(item);
        }
        item.Ten = dto.Ten.Trim(); item.MoTa = dto.MoTa.Trim();
        await db.SaveChangesAsync();
        return item;
    }

    public async Task SuaTaiKhoan(int id, QuanTriTaiKhoanDto dto)
    {
        var user = await db.TaiKhoans.FindAsync(id) ?? throw Missing();
        var email = dto.Email.Trim().ToLowerInvariant();
        if (await db.TaiKhoans.AnyAsync(x => x.Id != id && x.Email == email))
            throw new ApiException(409, "EMAIL_EXISTS", "Email đã được sử dụng.");
        user.HoTen = dto.HoTen.Trim(); user.Email = email;
        if (!string.IsNullOrEmpty(dto.MatKhauMoi)) user.MatKhau = hasher.HashPassword(user, dto.MatKhauMoi);
        await db.SaveChangesAsync();
    }

    public async Task Xoa(string kind, int id)
    {
        object entity;
        bool inUse;
        switch (kind)
        {
            case "events":
                var item = await db.SuKiens.FindAsync(id) ?? throw Missing();
                inUse = await db.DonVes.AnyAsync(x => x.SuKienId == id) ||
                    await db.DatDiaDiems.AnyAsync(x => x.SuKienId == id) ||
                    await db.LoaiVes.AnyAsync(x => x.SuKienId == id);
                entity = item; break;
            case "organizers":
                var organizer = await db.BanToChucs.Include(x => x.TaiKhoan).SingleOrDefaultAsync(x => x.Id == id) ?? throw Missing();
                inUse = await db.SuKiens.AnyAsync(x => x.BanToChucId == id);
                if (!inUse && organizer.TaiKhoan.VaiTroId == 2) organizer.TaiKhoan.VaiTroId = 1;
                entity = organizer; break;
            case "ticket-types":
                var ticket = await db.LoaiVes.FindAsync(id) ?? throw Missing();
                inUse = ticket.DaBan > 0 || ticket.DangGiu > 0 ||
                    await db.ChiTietDonVes.AnyAsync(x => x.LoaiVeId == id) || await db.Ves.AnyAsync(x => x.LoaiVeId == id);
                entity = ticket; break;
            case "venues":
                entity = await db.DiaDiems.FindAsync(id) ?? throw Missing();
                inUse = await db.DatDiaDiems.AnyAsync(x => x.DiaDiemId == id); break;
            case "event-types":
                entity = await db.LoaiSuKiens.FindAsync(id) ?? throw Missing();
                inUse = await db.SuKiens.AnyAsync(x => x.LoaiSuKienId == id); break;
            default: throw Missing();
        }
        if (inUse) throw new ApiException(409, "RECORD_IN_USE", "Bản ghi đã có dữ liệu liên quan. Hãy giữ lại lịch sử và sử dụng thao tác tắt hoặc đóng bán.");
        db.Remove(entity);
        await db.SaveChangesAsync();
    }

    public async Task<object> TheoTaiKhoan(int id)
    {
        if (!await db.TaiKhoans.AnyAsync(x => x.Id == id)) throw Missing();
        return new
        {
            suKien = await db.SuKiens.AsNoTracking().Where(x => x.BanToChuc.TaiKhoanId == id).ToListAsync(),
            donVe = await db.DonVes.AsNoTracking().Where(x => x.TaiKhoanId == id).ToListAsync()
        };
    }

    private static ApiException Missing() => new(404, "NOT_FOUND", "Không tìm thấy bản ghi.");
}
