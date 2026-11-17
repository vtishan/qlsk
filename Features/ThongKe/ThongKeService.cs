using QLSK.Common.Exceptions; using Microsoft.EntityFrameworkCore; using QLSK.Data; using QLSK.Domain.Enums;
namespace QLSK.Features.ThongKe;
public class ThongKeService(ApplicationDbContext nguCanhDuLieu)
{
 public async Task<object> LayThongKeTongQuan()=>new{
  taiKhoan=await nguCanhDuLieu.DanhSachTaiKhoan.CountAsync(nguCanhDuLieu.RequestAborted),taiKhoanHoatDong=await nguCanhDuLieu.DanhSachTaiKhoan.CountAsync(taiKhoanDb=>taiKhoanDb.HoatDong, nguCanhDuLieu.RequestAborted),
  suKien=await nguCanhDuLieu.DanhSachSuKien.CountAsync(nguCanhDuLieu.RequestAborted),suKienChoDuyet=await nguCanhDuLieu.DanhSachSuKien.CountAsync(suKienDb=>suKienDb.TrangThai==TrangThaiSuKien.ChoDuyet, nguCanhDuLieu.RequestAborted),
  don=await nguCanhDuLieu.DanhSachDonVe.CountAsync(nguCanhDuLieu.RequestAborted),donThanhCong=await nguCanhDuLieu.DanhSachDonVe.CountAsync(donVeDb=>donVeDb.TrangThai==TrangThaiDon.ThanhCong, nguCanhDuLieu.RequestAborted),
  ve=await nguCanhDuLieu.DanhSachVe.CountAsync(nguCanhDuLieu.RequestAborted),veDaCheckIn=await nguCanhDuLieu.DanhSachVe.CountAsync(veDaPhatHanhDb=>veDaPhatHanhDb.TrangThai==TrangThaiVe.DaCheckIn, nguCanhDuLieu.RequestAborted),
  doanhThu=(await nguCanhDuLieu.DanhSachThanhToan.Where(thanhToanDb=>thanhToanDb.TrangThai==TrangThaiThanhToan.ThanhCong).SumAsync(thanhToanDb=>(decimal?)thanhToanDb.SoTien, nguCanhDuLieu.RequestAborted)??0)-(await nguCanhDuLieu.DanhSachThanhToan.SumAsync(thanhToanDb=>(decimal?)thanhToanDb.SoTienHoan, nguCanhDuLieu.RequestAborted)??0),
  tongThanhToan=await nguCanhDuLieu.DanhSachThanhToan.CountAsync(nguCanhDuLieu.RequestAborted),hoanTien=await nguCanhDuLieu.DanhSachThanhToan.SumAsync(thanhToanDb=>(decimal?)thanhToanDb.SoTienHoan, nguCanhDuLieu.RequestAborted)??0,
  theoVaiTro=await nguCanhDuLieu.DanhSachTaiKhoan.GroupBy(taiKhoanDb=>taiKhoanDb.VaiTro.TenVaiTro).Select(nhomTaiKhoan=>new{vaiTro=nhomTaiKhoan.Key,soLuong=nhomTaiKhoan.Count()}).ToListAsync(nguCanhDuLieu.RequestAborted),
  theoTrangThaiVe=await nguCanhDuLieu.DanhSachVe.GroupBy(veDaPhatHanhDb=>veDaPhatHanhDb.TrangThai).Select(nhomVe=>new{trangThai=nhomVe.Key.ToString(),soLuong=nhomVe.Count()}).ToListAsync(nguCanhDuLieu.RequestAborted),
  theoTrangThaiThanhToan=await nguCanhDuLieu.DanhSachThanhToan.GroupBy(thanhToanDb=>thanhToanDb.TrangThai).Select(nhomThanhToan=>new{trangThai=nhomThanhToan.Key.ToString(),soLuong=nhomThanhToan.Count(),soTien=nhomThanhToan.Sum(thanhToanDb=>thanhToanDb.SoTien),soTienHoan=nhomThanhToan.Sum(thanhToanDb=>thanhToanDb.SoTienHoan)}).ToListAsync(nguCanhDuLieu.RequestAborted),
  theoTrangThaiSuKien=await nguCanhDuLieu.DanhSachSuKien.GroupBy(suKienDb=>suKienDb.TrangThai).Select(nhomSuKien=>new{trangThai=nhomSuKien.Key.ToString(),soLuong=nhomSuKien.Count()}).ToListAsync(nguCanhDuLieu.RequestAborted)
 };
 public async Task<object> ThongKeTrangThaiVeTheoSuKien(int suKienId)
 {
  if (!await nguCanhDuLieu.DanhSachSuKien.AnyAsync(suKienDb=>suKienDb.SuKienId==suKienId, nguCanhDuLieu.RequestAborted)) throw new ApiException(404,"NOT_FOUND","Không tìm thấy sự kiện.");
  return await nguCanhDuLieu.DanhSachVe.Where(veDaPhatHanhDb=>veDaPhatHanhDb.LoaiVe.SuKienId==suKienId).GroupBy(veDaPhatHanhDb=>veDaPhatHanhDb.TrangThai)
   .Select(nhomVe=>new{trangThai=nhomVe.Key.ToString(),soLuong=nhomVe.Count()}).ToListAsync(nguCanhDuLieu.RequestAborted);
 }
}
