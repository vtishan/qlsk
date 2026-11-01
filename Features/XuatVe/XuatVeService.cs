using Microsoft.EntityFrameworkCore; using QLSK.Common.Exceptions; using QLSK.Data; using QLSK.Domain.Entities; using QLSK.Infrastructure.Email; using QLSK.Infrastructure.File; using QLSK.Infrastructure.QR;
using VeEntity = QLSK.Domain.Entities.Ve;
using XuatVeEntity = QLSK.Domain.Entities.XuatVe;
namespace QLSK.Features.XuatVe;
public class XuatVeService(ApplicationDbContext db,QrService qr,VePdfService pdf,EmailService email)
{
 public async Task<List<XuatVeEntity>> LichSu(int userId, int id, bool admin = false)
 {
  var ticket = await db.Ves.AsNoTracking().Include(x => x.DonVe).SingleOrDefaultAsync(x => x.Id == id)
      ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy vé.");
  if (!admin && ticket.DonVe.TaiKhoanId != userId) throw new ApiException(403, "FORBIDDEN", "Không có quyền xem lịch sử xuất vé.");
  return await db.XuatVes.AsNoTracking().Where(x => x.VeId == id).OrderByDescending(x => x.NgayTao).ToListAsync();
 }
 async Task<VeEntity> Lay(int userId,int id){var v=await db.Ves.Include(x=>x.DonVe).ThenInclude(x=>x.TaiKhoan).Include(x=>x.LoaiVe).ThenInclude(x=>x.SuKien).SingleOrDefaultAsync(x=>x.Id==id)??throw new ApiException(404,"NOT_FOUND","Không tìm thấy vé.");if(v.DonVe.TaiKhoanId!=userId)throw new ApiException(403,"FORBIDDEN","Không có quyền.");return v;}
 public async Task<byte[]> Pdf(int userId,int id){var v=await Lay(userId,id);var data=pdf.Tao(v,qr.Tao(v.MaQR));db.XuatVes.Add(new XuatVeEntity{VeId=v.Id,Kieu="PDF",ThanhCong=true});await db.SaveChangesAsync();return data;}
 public async Task Gui(int userId,int id){var v=await Lay(userId,id);var data=pdf.Tao(v,qr.Tao(v.MaQR));try{await email.Gui(v.DonVe.TaiKhoan.Email,$"Vé {v.LoaiVe.SuKien.Ten}",$"Mã vé: {v.MaVe}",data);db.XuatVes.Add(new XuatVeEntity{VeId=v.Id,Kieu="Email",DiaChi=v.DonVe.TaiKhoan.Email,ThanhCong=true});}catch{db.XuatVes.Add(new XuatVeEntity{VeId=v.Id,Kieu="Email",DiaChi=v.DonVe.TaiKhoan.Email,ThanhCong=false});await db.SaveChangesAsync();throw;}await db.SaveChangesAsync();}
}
