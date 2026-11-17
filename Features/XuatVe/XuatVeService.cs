using QLSK.Common.Models;
using Microsoft.EntityFrameworkCore; using QLSK.Common.Exceptions; using QLSK.Data; using QLSK.Domain.Entities; using QLSK.Infrastructure.Email; using QLSK.Infrastructure.File; using QLSK.Infrastructure.QR;
using VeEntity = QLSK.Domain.Entities.Ve;
using XuatVeEntity = QLSK.Domain.Entities.XuatVe;
namespace QLSK.Features.XuatVe;
public class XuatVeService(ApplicationDbContext nguCanhDuLieu,QrService dichVuTaoMaQr,VePdfService dichVuTaoPdfVe,EmailService emailTaiKhoan)
{
 public async Task<List<XuatVeEntity>> LayLichSuXuatVe(int taiKhoanId, int veId, bool laQuanTriVien = false)
 {
  var veDaPhatHanh = await nguCanhDuLieu.DanhSachVe.AsNoTracking().Include(veDaPhatHanhDb => veDaPhatHanhDb.DonVe).SingleOrDefaultAsync(veDaPhatHanhDb => veDaPhatHanhDb.VeId == veId, nguCanhDuLieu.RequestAborted)
      ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy vé.");
  if (!laQuanTriVien && veDaPhatHanh.DonVe.TaiKhoanId != taiKhoanId) throw new ApiException(403, "FORBIDDEN", "Không có quyền xem lịch sử xuất vé.");
  return await nguCanhDuLieu.LichSuXuatVe.AsNoTracking().Where(xuatVeDb => xuatVeDb.VeId == veId).OrderByDescending(xuatVeDb => xuatVeDb.NgayTao).Paginate(nguCanhDuLieu).ToListAsync(nguCanhDuLieu.RequestAborted);
 }
 async Task<VeEntity> LayVeCuaTaiKhoan(int taiKhoanId,int veId){var veDaPhatHanh=await nguCanhDuLieu.DanhSachVe.Include(veDaPhatHanhDb=>veDaPhatHanhDb.DonVe).ThenInclude(donVeDb=>donVeDb.TaiKhoan).Include(veDaPhatHanhDb=>veDaPhatHanhDb.LoaiVe).ThenInclude(loaiVeDb=>loaiVeDb.SuKien).SingleOrDefaultAsync(veDaPhatHanhDb=>veDaPhatHanhDb.VeId==veId, nguCanhDuLieu.RequestAborted)??throw new ApiException(404,"NOT_FOUND","Không tìm thấy vé.");if(veDaPhatHanh.DonVe.TaiKhoanId!=taiKhoanId)throw new ApiException(403,"FORBIDDEN","Không có quyền.");return veDaPhatHanh;}
 public async Task<byte[]> XuatVePdf(int taiKhoanId,int veId){var veDaPhatHanh=await LayVeCuaTaiKhoan(taiKhoanId,veId);var noiDungPdf=dichVuTaoPdfVe.TaoPdfVe(veDaPhatHanh,dichVuTaoMaQr.TaoAnhQrPng(veDaPhatHanh.NoiDungMaQr));nguCanhDuLieu.LichSuXuatVe.Add(new XuatVeEntity{VeId=veDaPhatHanh.VeId,HinhThucXuat="PDF",ThanhCong=true});await nguCanhDuLieu.SaveChangesAsync(nguCanhDuLieu.RequestAborted);return noiDungPdf;}
 public async Task GuiVeQuaEmail(int taiKhoanId,int veId){var veDaPhatHanh=await LayVeCuaTaiKhoan(taiKhoanId,veId);var noiDungPdf=dichVuTaoPdfVe.TaoPdfVe(veDaPhatHanh,dichVuTaoMaQr.TaoAnhQrPng(veDaPhatHanh.NoiDungMaQr));try{await emailTaiKhoan.GuiEmailVe(veDaPhatHanh.DonVe.TaiKhoan.Email,$"Vé {veDaPhatHanh.LoaiVe.SuKien.TenSuKien}",$"Mã vé: {veDaPhatHanh.MaVe}",noiDungPdf,nguCanhDuLieu.RequestAborted);nguCanhDuLieu.LichSuXuatVe.Add(new XuatVeEntity{VeId=veDaPhatHanh.VeId,HinhThucXuat="Email",EmailNhan=veDaPhatHanh.DonVe.TaiKhoan.Email,ThanhCong=true});}catch{nguCanhDuLieu.LichSuXuatVe.Add(new XuatVeEntity{VeId=veDaPhatHanh.VeId,HinhThucXuat="Email",EmailNhan=veDaPhatHanh.DonVe.TaiKhoan.Email,ThanhCong=false});await nguCanhDuLieu.SaveChangesAsync(nguCanhDuLieu.RequestAborted);throw;}await nguCanhDuLieu.SaveChangesAsync(nguCanhDuLieu.RequestAborted);}
}
