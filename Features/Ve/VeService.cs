using Microsoft.EntityFrameworkCore;
using QLSK.Common.Exceptions;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using VeEntity = QLSK.Domain.Entities.Ve;
namespace QLSK.Features.Ve;
public class VeService(ApplicationDbContext nguCanhDuLieu)
{
    public async Task<List<VeEntity>> LayVeCuaTaiKhoan(int taiKhoanId) => await nguCanhDuLieu.DanhSachVe.AsNoTracking().Where(veDaPhatHanhDb => veDaPhatHanhDb.DonVe.TaiKhoanId == taiKhoanId).Include(veDaPhatHanhDb => veDaPhatHanhDb.DonVe).Include(veDaPhatHanhDb => veDaPhatHanhDb.LoaiVe).ThenInclude(loaiVeDb => loaiVeDb.SuKien).OrderByDescending(veDaPhatHanhDb => veDaPhatHanhDb.VeId).ToListAsync();
    public async Task<VeEntity> LayChiTietVe(int taiKhoanId, int veId, bool laNguoiQuanLy)
    {
        var veDaPhatHanh = await nguCanhDuLieu.DanhSachVe.AsNoTracking().Include(veDaPhatHanhDb => veDaPhatHanhDb.DonVe).Include(veDaPhatHanhDb => veDaPhatHanhDb.LoaiVe).ThenInclude(loaiVeDb => loaiVeDb.SuKien).SingleOrDefaultAsync(veDaPhatHanhDb => veDaPhatHanhDb.VeId == veId) ?? throw new ApiException(404,"NOT_FOUND","Không tìm thấy vé.");
        if (!laNguoiQuanLy && veDaPhatHanh.DonVe.TaiKhoanId != taiKhoanId) throw new ApiException(403,"FORBIDDEN","Không có quyền."); return veDaPhatHanh;
    }
    public async Task<VeEntity> LayVeTheoMaVe(int taiKhoanId, string maVe, bool laNguoiQuanLy)
    {
        var veDaPhatHanh = await nguCanhDuLieu.DanhSachVe.AsNoTracking().Include(veDaPhatHanhDb=>veDaPhatHanhDb.DonVe).Include(veDaPhatHanhDb=>veDaPhatHanhDb.LoaiVe).ThenInclude(loaiVeDb=>loaiVeDb.SuKien).SingleOrDefaultAsync(veDaPhatHanhDb=>veDaPhatHanhDb.MaVe==maVe) ?? throw new ApiException(404,"NOT_FOUND","Không tìm thấy vé.");
        if(!laNguoiQuanLy && veDaPhatHanh.DonVe.TaiKhoanId!=taiKhoanId) throw new ApiException(403,"FORBIDDEN","Không có quyền."); return veDaPhatHanh;
    }
    public async Task<VeEntity> CheckInVe(string maVeHoacQr)
    {
        var veDaPhatHanh = await nguCanhDuLieu.DanhSachVe.Include(veDaPhatHanhDb=>veDaPhatHanhDb.LoaiVe).ThenInclude(loaiVeDb=>loaiVeDb.SuKien).SingleOrDefaultAsync(veDaPhatHanhDb=>veDaPhatHanhDb.MaVe==maVeHoacQr || veDaPhatHanhDb.NoiDungMaQr==maVeHoacQr) ?? throw new ApiException(404,"NOT_FOUND","Không tìm thấy vé.");
        if(veDaPhatHanh.TrangThai==TrangThaiVe.DaCheckIn) throw new ApiException(409,"TICKET_ALREADY_CHECKED_IN","Vé đã check-in.");
        if(veDaPhatHanh.TrangThai!=TrangThaiVe.ChuaSuDung) throw new ApiException(409,"TICKET_INVALID_STATE","Vé không hợp lệ.");
        var thoiDiemHienTai = DateTime.UtcNow;
        if(veDaPhatHanh.LoaiVe.SuKien.TrangThai != TrangThaiSuKien.DangDienRa || thoiDiemHienTai < veDaPhatHanh.LoaiVe.SuKien.ThoiGianBatDau || thoiDiemHienTai >= veDaPhatHanh.LoaiVe.SuKien.ThoiGianKetThuc)
            throw new ApiException(409,"EVENT_NOT_IN_PROGRESS","Sự kiện chưa diễn ra hoặc đã kết thúc.");
        veDaPhatHanh.TrangThai=TrangThaiVe.DaCheckIn; veDaPhatHanh.NgayCheckIn=thoiDiemHienTai; await nguCanhDuLieu.SaveChangesAsync(); return veDaPhatHanh;
    }
}
