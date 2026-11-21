using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Security;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using QLSK.Features.Admissions;
using QLSK.Features.Events;
using QLSK.Features.Organizers;
using QLSK.Features.Requests;
using QLSK.Features.Tickets;
using QLSK.Features.Finance;
using static QLSK.Common.Http.ApiRequest;

namespace QLSK.Features.Reports;

public sealed class ReportService(AppDbContext db, EventService events, AdmissionService admissions)
{
    private const int MaxRows = 5000;
    internal static string[] Types(TaiKhoan actor)
    {
        var types = new List<string>();
        if (actor.VaiTro == VaiTroTaiKhoan.BanToChuc) types.AddRange(["orders", "checkins"]);
        if (actor.VaiTro == VaiTroTaiKhoan.QuanLySuKien) types.Add("checkins");
        if (AccountAccess.Platform(actor, OrganizerService.ReviewPermission)) types.Add("applications");
        if (AccountAccess.Platform(actor, RequestService.RentalPermission)) types.Add("rentals");
        if (AccountAccess.Platform(actor, RefundService.ReviewPermission)) types.Add("refunds");
        if (actor.VaiTro == VaiTroTaiKhoan.BanToChuc || AccountAccess.Platform(actor, FinanceService.Permission))
            types.AddRange(["settlements", "payouts"]);
        return types.ToArray();
    }

    public async Task<ReportDto> Preview(TaiKhoan actor, ReportFilter filter, CancellationToken ct)
    {
        var allowed = Types(actor);
        var type = filter.Type ?? allowed.FirstOrDefault();
        if (type == null || !allowed.Contains(type)) throw Denied();
        if (filter.From > filter.To || filter.From is { Year: < 1970 or > 9998 } || filter.To is { Year: < 1970 or > 9998 })
            throw Invalid("Khoảng ngày báo cáo không hợp lệ.");
        var start = filter.From?.ToDateTime(TimeOnly.MinValue).AddHours(-7);
        var end = filter.To?.AddDays(1).ToDateTime(TimeOnly.MinValue).AddHours(-7);
        var rangeStart = start; var rangeEnd = end;
        string title;
        string[] columns;
        string[][] data;

        if (type == "orders")
        {
            if (!filter.EventId.HasValue) throw Invalid("Chọn sự kiện cần báo cáo.");
            await events.NeedOwner(actor, filter.EventId.Value, ct);
            var query = from o in db.DonHang.AsNoTracking() join s in db.SuatDien on o.SuatDienId equals s.Id
                        where s.SuKienId == filter.EventId && (!filter.SessionId.HasValue || s.Id == filter.SessionId)
                            && (!rangeStart.HasValue || o.NgayTao >= rangeStart) && (!rangeEnd.HasValue || o.NgayTao < rangeEnd)
                        select o;
            if (filter.Status != null)
            {
                var status = filter.Status switch {
                    "PENDING_PAYMENT" => TrangThaiDonHang.ChoThanhToan, "PAID" => TrangThaiDonHang.DaThanhToan,
                    "EXPIRED" => TrangThaiDonHang.HetHan, "CANCELLED" => TrangThaiDonHang.DaHuy,
                    "PAID_OUT_OF_STOCK" => TrangThaiDonHang.DaThuTienHetCho, "REFUND_PENDING" => TrangThaiDonHang.ChoHoanTien,
                    "REFUNDED" => TrangThaiDonHang.DaHoanTien, _ => throw Invalid("Trạng thái đơn không hợp lệ.")
                };
                query = query.Where(o => o.TrangThai == status);
            }
            var rows = await query.OrderBy(o => o.Id).Take(MaxRows + 1).ToListAsync(ct);
            title = "Đơn hàng sự kiện";
            columns = ["Đơn", "Suất", "Ngày đặt", "Tổng chốt (₫)", "Trạng thái"];
            data = rows.Select(r => new[] { r.Id.ToString(), r.SuatDienId.ToString(), Date(r.NgayTao), Money(r.TongTien),
                OrderService.State(r.TrangThai).Label }).ToArray();
        }
        else if (type == "checkins")
        {
            if (!filter.SessionId.HasValue) throw Invalid("Chọn suất diễn cần báo cáo.");
            var slot = await db.SuatDien.AsNoTracking().SingleOrDefaultAsync(s => s.Id == filter.SessionId &&
                (!filter.EventId.HasValue || s.SuKienId == filter.EventId), ct) ?? throw Missing();
            if (!await admissions.Has(actor, slot, AdmissionService.View, ct)) throw Denied();
            var rows = await (from log in db.NhatKyCheckIn.AsNoTracking() join v in db.Ve on log.VeId equals v.Id
                              where v.SuatDienId == slot.Id && (!rangeStart.HasValue || log.ThoiGianQuet >= rangeStart) &&
                                (!rangeEnd.HasValue || log.ThoiGianQuet < rangeEnd) orderby log.Id
                              select new { v.MaVe, log.ThoiGianQuet, log.HinhThuc, log.KetQua }).Take(MaxRows + 1).ToListAsync(ct);
            title = "Check-in theo suất";
            columns = ["Mã vé", "Thời điểm", "Hình thức", "Kết quả"];
            data = rows.Select(r => new[] { r.MaVe, Date(r.ThoiGianQuet), r.HinhThuc == HinhThucCheckIn.QuetQR ? "QR" : "Thủ công",
                r.KetQua == "ThanhCong" ? "Thành công" : r.KetQua }).ToArray();
        }
        else if (type == "applications")
        {
            var query = db.HoSoBTC.AsNoTracking().Where(h => (!rangeStart.HasValue || h.NgayNop >= rangeStart) && (!rangeEnd.HasValue || h.NgayNop < rangeEnd));
            if (filter.Status != null)
            {
                var state = filter.Status switch {
                    "PENDING" => TrangThaiHoSoToChuc.ChoDuyet, "APPROVED" => TrangThaiHoSoToChuc.DaDuyet,
                    "NEEDS_INFO" => TrangThaiHoSoToChuc.CanBoSung, "REJECTED" => TrangThaiHoSoToChuc.TuChoi,
                    "DRAFT" => TrangThaiHoSoToChuc.Nhap, _ => throw Invalid("Trạng thái hồ sơ không hợp lệ.")
                };
                query = query.Where(h => h.TrangThai == state);
            }
            var rows = await query.OrderBy(h => h.Id).Take(MaxRows + 1).Select(h => new { h.Id, h.TenToChuc, h.NgayNop, h.HanDuyet, h.TrangThai }).ToListAsync(ct);
            title = "Xét duyệt Ban tổ chức";
            columns = ["Hồ sơ", "Đơn vị", "Ngày gửi", "Hạn xử lý", "Trạng thái"];
            data = rows.Select(r => new[] { r.Id.ToString(), r.TenToChuc, Date(r.NgayNop), Date(r.HanDuyet),
                OrganizerService.Status(r.TrangThai).Label }).ToArray();
        }
        else if (type == "rentals")
        {
            var now = DateTime.UtcNow;
            var query = db.YeuCauThue.AsNoTracking().Where(r => (!rangeStart.HasValue || r.ThoiGianBatDau >= rangeStart) && (!rangeEnd.HasValue || r.ThoiGianBatDau < rangeEnd));
            if (filter.Status != null)
            {
                var state = filter.Status switch {
                    "REQUESTED" => TrangThaiYeuCauThue.ChoDuyet, "AWAITING_DEPOSIT" => TrangThaiYeuCauThue.ChoCoc,
                    "CONFIRMED" => TrangThaiYeuCauThue.DaXacNhan, "REJECTED" => TrangThaiYeuCauThue.TuChoi,
                    "EXPIRED" => TrangThaiYeuCauThue.HetHan, "CANCELLED" => TrangThaiYeuCauThue.DaHuy,
                    _ => throw Invalid("Trạng thái thuê không hợp lệ.")
                };
                if (state == TrangThaiYeuCauThue.HetHan)
                    query = query.Where(r => r.TrangThai == state || r.TrangThai == TrangThaiYeuCauThue.ChoCoc && r.HanDatCoc <= now);
                else query = query.Where(r => r.TrangThai == state && (state != TrangThaiYeuCauThue.ChoCoc || r.HanDatCoc > now));
            }
            var rows = await (from r in query join place in db.DiaDiem on r.DiaDiemId equals place.Id orderby r.Id
                select new { r.Id, place.TenDiaDiem, r.ThoiGianBatDau, r.TongTien, r.TienCoc, r.TrangThai, r.HanDatCoc }).Take(MaxRows + 1).ToListAsync(ct);
            title = "Yêu cầu thuê địa điểm";
            columns = ["Yêu cầu", "Địa điểm", "Bắt đầu", "Tiền thuê (₫)", "Cọc (₫)", "Trạng thái"];
            data = rows.Select(r => new[] { r.Id.ToString(), r.TenDiaDiem, Date(r.ThoiGianBatDau), Money(r.TongTien), Money(r.TienCoc),
                RequestService.RentalState(RequestService.Effective(r.TrangThai, r.HanDatCoc, now)).Label }).ToArray();
        }
        else if (type is "settlements" or "payouts")
        {
            var financialEvents = AccountAccess.Platform(actor, FinanceService.Permission) ? db.SuKien.AsNoTracking() :
                db.SuKien.AsNoTracking().Where(e => db.HoSoBTC.Any(h => h.Id == e.BTCId && h.TaiKhoanId == actor.Id &&
                    h.TrangThai == TrangThaiHoSoToChuc.DaDuyet));
            if (filter.EventId.HasValue) financialEvents = financialEvents.Where(e => e.Id == filter.EventId);
            if (type == "settlements")
            {
                var settlements = db.DoiSoat.AsNoTracking();
                if (filter.Status is "MANAGER_APPROVED" or "PAYOUT_PENDING")
                    settlements = settlements.Where(d => d.TrangThai == TrangThaiDoiSoat.ChoGiaiNgan &&
                        db.GiaiNgan.Any(p => p.DoiSoatId == d.Id) == (filter.Status == "PAYOUT_PENDING"));
                else if (filter.Status != null)
                {
                    var phase = filter.Status switch {
                        "DRAFT" => TrangThaiDoiSoat.ChoXacNhan, "ORGANIZER_CONFIRMED" => TrangThaiDoiSoat.DaXacNhan,
                        "DISPUTED" => TrangThaiDoiSoat.TranhChap, "PAID" => TrangThaiDoiSoat.HoanTat,
                        _ => throw Invalid("Trạng thái đối soát không hợp lệ.")
                    };
                    settlements = settlements.Where(d => d.TrangThai == phase);
                }
                var query = from d in settlements join e in financialEvents on d.SuKienId equals e.Id
                    where (!filter.From.HasValue || d.DenNgay >= filter.From) && (!filter.To.HasValue || d.TuNgay <= filter.To)
                    orderby d.Id select new { Value = d, e.TenSuKien };
                var rows = await query.Take(MaxRows + 1).ToListAsync(ct);
                title = "Doanh thu và đối soát";
                columns = ["Kỳ đối soát", "Sự kiện", "Từ ngày", "Đến ngày", "Doanh thu (₫)", "Đã hoàn (₫)", "Phí (₫)", "Điều chỉnh (₫)", "Thực nhận (₫)", "Trạng thái"];
                data = rows.Select(r => new[] { r.Value.Id.ToString(), r.TenSuKien, r.Value.TuNgay.ToString("dd/MM/yyyy"),
                    r.Value.DenNgay.ToString("dd/MM/yyyy"), Money(r.Value.TongDoanhThu), Money(r.Value.TongTienHoan),
                    Money(r.Value.PhiNenTang), Money(r.Value.TienDieuChinh), Money(r.Value.TienBTCNhan), r.Value.TrangThai switch {
                        TrangThaiDoiSoat.ChoXacNhan => "Chờ BTC xác nhận", TrangThaiDoiSoat.DaXacNhan => "BTC đã xác nhận",
                        TrangThaiDoiSoat.TranhChap => "Tranh chấp", TrangThaiDoiSoat.ChoGiaiNgan => "Chờ giải ngân", _ => "Hoàn tất"
                    } }).ToArray();
            }
            else
            {
                var payouts = db.GiaiNgan.AsNoTracking();
                if (filter.Status != null)
                {
                    var phase = filter.Status switch {
                        "PENDING_APPROVAL" => TrangThaiChuyenTien.ChoChuyen, "PROCESSING" => TrangThaiChuyenTien.DangChuyen,
                        "SUCCESS" => TrangThaiChuyenTien.ThanhCong, "FAILED" => TrangThaiChuyenTien.ThatBai,
                        _ => throw Invalid("Trạng thái giải ngân không hợp lệ.")
                    };
                    payouts = payouts.Where(p => p.TrangThai == phase);
                }
                var query = from p in payouts join d in db.DoiSoat on p.DoiSoatId equals d.Id
                    join e in financialEvents on d.SuKienId equals e.Id
                    where (!rangeStart.HasValue || p.NgayGiaiNgan >= rangeStart) && (!rangeEnd.HasValue || p.NgayGiaiNgan < rangeEnd)
                    orderby p.Id select new { Value = p, e.TenSuKien };
                var rows = await query.Take(MaxRows + 1).ToListAsync(ct);
                title = "Giải ngân";
                columns = ["Lệnh giải ngân", "Kỳ đối soát", "Sự kiện", "Số tiền (₫)", "Ngày chuyển", "Trạng thái"];
                data = rows.Select(r => new[] { r.Value.Id.ToString(), r.Value.DoiSoatId.ToString(), r.TenSuKien,
                    Money(r.Value.SoTien), Date(r.Value.NgayGiaiNgan), r.Value.TrangThai switch {
                        TrangThaiChuyenTien.DangChuyen => "Đang chuyển", TrangThaiChuyenTien.ThanhCong => "Đã chuyển",
                        TrangThaiChuyenTien.ThatBai => "Thất bại", _ => "Chờ chuyển"
                    } }).ToArray();
            }
        }
        else
        {
            var query = db.YeuCauHoanTien.AsNoTracking().Where(r => (!rangeStart.HasValue || r.NgayYeuCau >= rangeStart) && (!rangeEnd.HasValue || r.NgayYeuCau < rangeEnd));
            if (filter.Status != null)
            {
                var state = filter.Status switch {
                    "REQUESTED" or "MANUAL_REVIEW" => TrangThaiYeuCauHoanTien.ChoDuyet, "APPROVED" => TrangThaiYeuCauHoanTien.DaDuyet,
                    "PROCESSING" => TrangThaiYeuCauHoanTien.DangChuyen, "COMPLETED" => TrangThaiYeuCauHoanTien.HoanTat,
                    "FAILED" => TrangThaiYeuCauHoanTien.ThatBai, "REJECTED" => TrangThaiYeuCauHoanTien.TuChoi,
                    _ => throw Invalid("Trạng thái hoàn tiền không hợp lệ.")
                };
                query = query.Where(r => r.TrangThai == state);
            }
            var rows = await query.OrderBy(r => r.Id).Take(MaxRows + 1)
                .Select(r => new { r.Id, r.GiaoDichId, r.NgayYeuCau, r.SoTienYeuCau, r.SoTienDuyet, r.TrangThai }).ToListAsync(ct);
            title = "Yêu cầu hoàn tiền";
            columns = ["Yêu cầu", "Giao dịch", "Thời điểm", "Số tiền yêu cầu", "Số tiền duyệt", "Trạng thái"];
            data = rows.Select(r => new[] { r.Id.ToString(), r.GiaoDichId.ToString(), Date(r.NgayYeuCau), Money(r.SoTienYeuCau),
                r.SoTienDuyet.HasValue ? Money(r.SoTienDuyet.Value) : "", RefundService.State(r.TrangThai).Label }).ToArray();
        }
        if (data.Length > MaxRows) throw Invalid("Thu hẹp khoảng ngày trước khi xem báo cáo trên 5.000 dòng.");
        return new(title, columns, data, data.Length > 0 ? ["export"] : []);
    }

    public async Task<ReportFile> Export(TaiKhoan actor, ReportFilter filter, CancellationToken ct)
    {
        var report = await Preview(actor, filter, ct);
        if (report.Rows.Count == 0) throw Conflict("Báo cáo chưa có dữ liệu trong phạm vi đã chọn.");
        return filter.Format switch {
            "xlsx" => ReportDocuments.Excel(report),
            "docx" => ReportDocuments.Word(report),
            _ => throw Invalid("Chọn Excel hoặc Word. Bản in/PDF được mở từ giao diện báo cáo.")
        };
    }

    private static string Date(DateTime? value) => value?.AddHours(7).ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) ?? "";
    private static string Money(decimal n) => n.ToString("0", CultureInfo.InvariantCulture);
}
