using Microsoft.EntityFrameworkCore;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Features.Accounts;
using QLSK.Features.Catalog;
using QLSK.Features.Organizers;
using QLSK.Common.Security;
using QLSK.Domain.Enums;
using QLSK.Features.Requests;
using QLSK.Features.Events;
using QLSK.Features.Operations;
using QLSK.Features.Tickets;
using QLSK.Features.Admissions;
using QLSK.Features.Reports;
using QLSK.Features.Finance;

namespace QLSK.Features.Capabilities;

public sealed class CapabilityService(AppDbContext db, AccountService accounts, RentalPayOs payOs)
{
    public async Task<CapabilityDto> Get(TaiKhoan? actor, string? token, CancellationToken ct)
    {
        // Verify that the configured database contains the QLSK schema before advertising operations.
        await db.TaiKhoan.AnyAsync(ct);
        var operations = new List<string>(CatalogService.PublicOperations) { "auth.login", "auth.register" };
        var sections = new HashSet<string>(StringComparer.Ordinal);
        if (accounts.MailConfigured) operations.AddRange(["auth.reset", "auth.confirmReset"]);
        if (actor != null)
        {
            operations.AddRange(AccountService.AccountOperations);
            operations.AddRange(TicketService.UserOperations.Concat(OrderService.UserOperations));
            operations.AddRange(["invitations.resolve", "invitations.claim"]);
            operations.AddRange(OrganizerService.UserOperations.Where(op => op != "organizer.submit" ||
                actor.VaiTro is VaiTroTaiKhoan.NguoiDung or VaiTroTaiKhoan.BanToChuc));
            if (AccountAccess.Platform(actor, OrganizerService.ReviewPermission))
            {
                operations.AddRange(OrganizerService.ReviewOperations);
                sections.Add("applications");
            }
            operations.AddRange(RequestService.UserOperations.Concat(RefundService.UserOperations));
            if (payOs.Configured) operations.Add("requests.rentals.deposit");
            if (payOs.Configured) operations.AddRange(["orders.checkout", "orders.reconcile"]);
            if (AccountAccess.Platform(actor, RequestService.RentalPermission))
            {
                operations.AddRange(RequestService.RentalOperations);
                sections.Add("rentals");
            }
            if (AccountAccess.Platform(actor, RequestService.SupportPermission))
            {
                operations.AddRange(RequestService.SupportOperations);
                sections.Add("support");
            }
            if (AccountAccess.Platform(actor, RefundService.ReviewPermission))
            {
                operations.AddRange(RefundService.ReviewOperations);
                sections.Add("refunds");
            }
            var finance = AccountAccess.Platform(actor, FinanceService.Permission);
            if (finance || actor.VaiTro == VaiTroTaiKhoan.BanToChuc)
            {
                operations.AddRange(FinanceService.ReadOperations);
                sections.UnionWith(["contracts", "settlements", "payouts"]);
            }
            if (finance)
            {
                operations.AddRange(FinanceService.FinanceOperations);
                if (!sections.Contains("rentals")) operations.Add("work.rentals.detail");
                sections.Add("payments");
            }
            if (AccountAccess.Platform(actor, OperationsService.EventReviewPermission) || actor.VaiTro == VaiTroTaiKhoan.BanToChuc)
            {
                operations.AddRange(EventService.ReadOperations);
                sections.UnionWith(["events", "versions", "sessions", "ticketTypes"]);
            }
            if (actor.VaiTro == VaiTroTaiKhoan.BanToChuc)
            {
                operations.AddRange(EventService.OrganizerOperations.Concat(OrderService.WorkOperations).Concat(StaffGrantService.Operations).Concat(AdmissionService.InvitationOperations)
                    .Concat(FinanceService.OrganizerOperations).Concat(VoucherService.ReadOperations));
                sections.UnionWith(["orders", "staff", "guests", "vouchers"]);
            }
            if (actor.VaiTro is VaiTroTaiKhoan.BanToChuc or VaiTroTaiKhoan.QuanLySuKien)
            {
                var grants = actor.VaiTro == VaiTroTaiKhoan.BanToChuc ? StaffGrantService.Permissions.Keys.ToArray() :
                    await db.PhanCongSuKien.Where(g => g.TaiKhoanId == actor.Id && g.TrangThai == TrangThaiPhanCongSuKien.DangHieuLuc)
                        .Select(g => g.LoaiQuyen).Distinct().ToArrayAsync(ct);
                if (grants.Contains(AdmissionService.Scan))
                {
                    operations.AddRange(AdmissionService.ScanOperations);
                    sections.Add("scan");
                }
                if (grants.Contains(AdmissionService.Manual))
                {
                    operations.AddRange(AdmissionService.ManualOperations);
                    sections.Add("manual");
                }
                if (grants.Contains(AdmissionService.View))
                {
                    operations.AddRange(AdmissionService.AttendeeOperations.Concat(AdmissionService.CheckInLogOperations));
                    sections.UnionWith(["attendees", "checkins"]);
                }
            }
            if (AccountAccess.Platform(actor, OperationsService.EventReviewPermission))
            {
                operations.AddRange(EventService.ReviewOperations);
                sections.Add("reviews");
            }
            if (AccountAccess.Admin(actor))
            {
                operations.AddRange(OperationsService.AdminOperations.Concat(CatalogAdminService.Operations).Concat(FinanceService.AdminOperations));
                sections.UnionWith(["accounts", "settings", "audit", "monitoring", "venues", "zones", "venueBlocks", "catalog", "policies"]);
            }
            else if (AccountAccess.Platform(actor, OperationsService.MonitorPermission))
            {
                operations.Add("work.monitoring.list");
                sections.Add("monitoring");
            }
            if (accounts.MailConfigured) operations.AddRange(["account.requestVerification", "account.confirmVerification"]);
        }
        WorkspaceDto? workspace = null;
        if (actor is not null)
        {
            var role = actor.VaiTro switch
            {
                VaiTroTaiKhoan.BanToChuc => "organizer", VaiTroTaiKhoan.QuanLySuKien => "staff",
                VaiTroTaiKhoan.QuanLyHeThong => "manager", VaiTroTaiKhoan.QuanTriVien => "admin", _ => "user"
            };
            workspace = new(role, AccountAccess.RoleLabel(actor.VaiTro), sections.ToArray());
            if (role != "user") operations.AddRange(["work.scopes", "work.overview"]);
        }
        var reportTypes = actor == null ? [] : ReportService.Types(actor)
            .Where(type => type != "checkins" || sections.Contains("checkins")).ToArray();
        if (reportTypes.Length > 0) operations.AddRange(["reports.preview", "reports.export", "reports.print"]);
        var unread = actor is null ? 0 : await db.ThongBao.CountAsync(n =>
            n.TaiKhoanId == actor.Id && n.KenhGui == KenhGuiThongBao.TrongUngDung && !n.DaDoc, ct);
        return new(1, actor is not null, operations, token, workspace, accounts.MailConfigured ? ["email"] : [], reportTypes,
            actor?.HoTen, unread);
    }
}

