using QLSK.Common.Exceptions; using Microsoft.EntityFrameworkCore; using QLSK.Data; using QLSK.Domain.Enums;
namespace QLSK.Features.ThongKe;
public class ThongKeService(ApplicationDbContext db)
{
 public async Task<object> Dashboard()=>new{
  taiKhoan=await db.TaiKhoans.CountAsync(),taiKhoanHoatDong=await db.TaiKhoans.CountAsync(x=>x.HoatDong),
  suKien=await db.SuKiens.CountAsync(),suKienChoDuyet=await db.SuKiens.CountAsync(x=>x.TrangThai==TrangThaiSuKien.ChoDuyet),
  don=await db.DonVes.CountAsync(),donThanhCong=await db.DonVes.CountAsync(x=>x.TrangThai==TrangThaiDon.ThanhCong),
  ve=await db.Ves.CountAsync(),veDaCheckIn=await db.Ves.CountAsync(x=>x.TrangThai==TrangThaiVe.DaCheckIn),
  doanhThu=(await db.ThanhToans.Where(x=>x.TrangThai==TrangThaiThanhToan.ThanhCong).SumAsync(x=>(decimal?)x.SoTien)??0)-(await db.ThanhToans.SumAsync(x=>(decimal?)x.SoTienHoan)??0),
  tongThanhToan=await db.ThanhToans.CountAsync(),hoanTien=await db.ThanhToans.SumAsync(x=>(decimal?)x.SoTienHoan)??0,
  theoVaiTro=await db.TaiKhoans.GroupBy(x=>x.VaiTro.Ten).Select(g=>new{vaiTro=g.Key,soLuong=g.Count()}).ToListAsync(),
  theoTrangThaiVe=await db.Ves.GroupBy(x=>x.TrangThai).Select(g=>new{trangThai=g.Key.ToString(),soLuong=g.Count()}).ToListAsync(),
  theoTrangThaiThanhToan=await db.ThanhToans.GroupBy(x=>x.TrangThai).Select(g=>new{trangThai=g.Key.ToString(),soLuong=g.Count(),soTien=g.Sum(x=>x.SoTien),soTienHoan=g.Sum(x=>x.SoTienHoan)}).ToListAsync(),
  theoTrangThaiSuKien=await db.SuKiens.GroupBy(x=>x.TrangThai).Select(g=>new{trangThai=g.Key.ToString(),soLuong=g.Count()}).ToListAsync()
 };
 public async Task<object> CheckIn(int eventId)
 {
  if (!await db.SuKiens.AnyAsync(x=>x.Id==eventId)) throw new ApiException(404,"NOT_FOUND","Không tìm thấy sự kiện.");
  return await db.Ves.Where(x=>x.LoaiVe.SuKienId==eventId).GroupBy(x=>x.TrangThai)
   .Select(g=>new{trangThai=g.Key.ToString(),soLuong=g.Count()}).ToListAsync();
 }
}
