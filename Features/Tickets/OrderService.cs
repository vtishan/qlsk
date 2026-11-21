using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Data;
using QLSK.Common.Http;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using QLSK.Features.Catalog;
using QLSK.Features.Events;
using QLSK.Features.Requests;
using static QLSK.Common.Http.ApiRequest;

namespace QLSK.Features.Tickets;

public sealed partial class OrderService(AppDbContext db, CatalogService catalog, EventService events, RentalPayOs payOs)
{
    internal static readonly string[] UserOperations = ["orders.inventory", "orders.create", "orders.detail", "orders.list", "orders.cancel"];
    internal static readonly string[] WorkOperations = ["work.orders.list", "work.orders.detail"];
    private const int Limit = 10;

    private sealed record BookingSession(SuatDien Slot, SuKien Event);
    private async Task<BookingSession> FindPublished(int id, CancellationToken ct) =>
        await (from slot in db.SuatDien.AsNoTracking() join evt in catalog.Published() on slot.SuKienId equals evt.Id
               where slot.Id == id && slot.TrangThai != TrangThaiSuatDien.Nhap select new BookingSession(slot, evt))
            .SingleOrDefaultAsync(ct) ?? throw Missing();

    private IQueryable<GiuGhe> ActiveHolds(int sessionId, DateTime now) =>
        db.GiuGhe.AsNoTracking().Where(h => h.SuatDienId == sessionId && h.TrangThai == TrangThaiGiuGhe.DangGiu && h.HetHan > now);

    private IQueryable<Ve> Issued(int sessionId) => db.Ve.AsNoTracking().Where(v => v.SuatDienId == sessionId &&
        (v.TrangThai == TrangThaiVe.DaPhatHanh || v.TrangThai == TrangThaiVe.DaCheckIn));

    public async Task<InventoryDto> Inventory(TaiKhoan actor, int id, int? zoneId, CancellationToken ct)
    {
        var current = await FindPublished(id, ct);
        var now = DateTime.UtcNow;
        var saleOpen = CatalogService.SaleOpen(current.Slot, current.Event.TrangThai, now);
        var policy = await TicketPolicy(ct);
        var zones = await db.KhuVuc.AsNoTracking().Where(z => z.DiaDiemId == current.Slot.DiaDiemId &&
            db.LoaiVe.Any(t => t.SuatDienId == id && t.KhuVucId == z.Id && t.NhomVe != NhomVe.VeMoi))
            .OrderBy(z => z.Id).ToListAsync(ct);
        if (zoneId.HasValue && !zones.Any(z => z.Id == zoneId.Value)) throw Missing();
        var selectedZone = zoneId ?? zones.FirstOrDefault()?.Id;
        var seatRows = selectedZone.HasValue ? await db.Ghe.AsNoTracking().Where(s => s.KhuVucId == selectedZone.Value)
            .OrderBy(s => s.MaGhe).ToListAsync(ct) : [];
        var holds = await (from h in ActiveHolds(id, now) join line in db.ChiTietDonHang on h.ChiTietDonHangId equals line.Id
                           select new { h.GheId, line.LoaiVeId }).ToListAsync(ct);
        var sales = await Issued(id).Select(v => new { v.GheId, v.LoaiVeId }).ToListAsync(ct);
        var held = holds.Where(r => r.GheId.HasValue).Select(r => r.GheId!.Value).ToHashSet();
        var sold = sales.Where(r => r.GheId.HasValue).Select(r => r.GheId!.Value).ToHashSet();
        var bookedByType = holds.Select(r => r.LoaiVeId).Concat(sales.Select(r => r.LoaiVeId))
            .GroupBy(typeId => typeId).ToDictionary(group => group.Key, group => group.Count());
        var types = await db.LoaiVe.AsNoTracking().Where(t => t.SuatDienId == id && t.NhomVe != NhomVe.VeMoi &&
            t.BatDauBan <= now && now < t.KetThucBan).OrderBy(t => t.GiaVe).ThenBy(t => t.Id).ToListAsync(ct);
        var byZone = types.Where(t => t.SoLuong > bookedByType.GetValueOrDefault(t.Id)).ToLookup(x => x.KhuVucId);
        var fee = await Fee(ct);
        var canBuy = saleOpen && holds.Count + sales.Count < current.Slot.SucChua;
        var remaining = Math.Max(0, Limit - await Used(actor.Id, id, now, ct));
        return new(current.Event.TenSuKien, ScheduleService.NameOf(current.Slot), Stamp(current.Slot.ThoiGianBatDau), policy, remaining,
            zones.Select(zone => new SeatingZoneDto(zone.Id, zone.TenKhuVuc, seatRows.Where(s => s.KhuVucId == zone.Id).Select(s =>
            {
                var type = byZone[zone.Id].FirstOrDefault();
                var status = sold.Contains(s.Id) ? "BOOKED" : held.Contains(s.Id) ? "HELD" :
                    s.TrangThai != TrangThaiGhe.SuDung || !canBuy || type == null ? "BLOCKED" : "AVAILABLE";
                return new SeatDto(s.Id, s.MaGhe, s.CumGheId, status, type?.Id, type?.TenLoaiVe, type?.GiaVe + fee);
            }).ToArray())).ToArray(), payOs.Configured);
    }

    public async Task<int> Place(TaiKhoan actor, PlaceOrderRequest request, CancellationToken ct)
    {
        if (!Guid.TryParse(request.IdempotencyKey, out var key)) throw Invalid("Khóa gửi đơn không hợp lệ.");
        var selections = request.Items?.OrderBy(i => i.SeatId).ToArray() ?? [];
        if (selections.Length is < 1 or > Limit || selections.Any(x => x.SeatId <= 0 || x.TicketTypeId <= 0) ||
            selections.Select(o => o.SeatId).Distinct().Count() != selections.Length ||
            !selections.Select(o => o.SeatId).SequenceEqual((request.SeatIds ?? []).Order()))
            throw Invalid("Chọn từ 1 đến 10 ghế hợp lệ, không trùng lặp.");
        var voucher = Optional(request.VoucherCode, 64, "mã khuyến mãi")?.ToUpperInvariant();
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(
            new { request.SessionId, request.PolicyId, voucher, selections }))));
        var clientKey = key.ToString("N");

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:order:{actor.Id}:{clientKey}", ct);
        var previous = await db.DonHang.AsNoTracking().SingleOrDefaultAsync(d => d.TaiKhoanId == actor.Id && d.KhoaTaoDon == clientKey, ct);
        if (previous != null)
        {
            if (previous.DauVanTayTaoDon != fingerprint) throw Conflict("Khóa gửi lại thuộc một yêu cầu khác.");
            return previous.Id;
        }
        var eventId = await db.SuatDien.Where(s => s.Id == request.SessionId).Select(s => (int?)s.SuKienId).SingleOrDefaultAsync(ct) ?? throw Missing();
        await SqlOperations.Lock(db, $"qlsk:event:{eventId}", ct);
        await SqlOperations.Lock(db, $"qlsk:session:{request.SessionId}", ct);
        var booking = await FindPublished(request.SessionId, ct);
        await SqlOperations.Lock(db, $"qlsk:contract:{booking.Event.BTCId}", ct);
        var now = DateTime.UtcNow;
        await Expire(booking.Slot.Id, now, ct);
        if (!CatalogService.SaleOpen(booking.Slot, booking.Event.TrangThai, now)) throw Conflict("Suất chưa mở bán hoặc đã đóng bán.");
        var policy = await TicketPolicy(ct);
        if (!policy.HasValue || request.PolicyId != policy.Value) throw Conflict("Chính sách đặt vé đã đổi hoặc chưa công bố.");
        if (await Used(actor.Id, booking.Slot.Id, now, ct) + selections.Length > Limit)
            throw Conflict("Tổng vé đã mua và đang giữ của tài khoản vượt 10 vé trong suất.");

        var ids = selections.Select(o => o.SeatId).ToArray();
        var seats = await db.Ghe.AsNoTracking().Where(g => ids.Contains(g.Id)).ToDictionaryAsync(g => g.Id, ct);
        var types = await db.LoaiVe.AsNoTracking().Where(t => t.SuatDienId == booking.Slot.Id && t.NhomVe != NhomVe.VeMoi &&
            t.BatDauBan <= now && now < t.KetThucBan).ToDictionaryAsync(t => t.Id, ct);
        var allowedZones = await db.KhuVuc.AsNoTracking().Where(z => z.DiaDiemId == booking.Slot.DiaDiemId)
            .ToDictionaryAsync(z => z.Id, ct);
        foreach (var item in selections)
        {
            if (!seats.TryGetValue(item.SeatId, out var seat) || seat.TrangThai != TrangThaiGhe.SuDung ||
                !types.TryGetValue(item.TicketTypeId, out var type) || seat.KhuVucId != type.KhuVucId || !allowedZones.ContainsKey(seat.KhuVucId))
                throw Invalid("Ghế hoặc loại vé đã thay đổi. Tải lại sơ đồ.");
        }
        var groups = seats.Values.Where(s => s.CumGheId.HasValue).Select(s => s.CumGheId!.Value).Distinct().ToArray();
        if (groups.Length > 0)
        {
            var members = await db.Ghe.AsNoTracking().Where(g => g.CumGheId.HasValue && groups.Contains(g.CumGheId.Value)).ToListAsync(ct);
            var expected = await db.CumGhe.AsNoTracking().Where(g => groups.Contains(g.Id)).ToDictionaryAsync(g => g.Id, g => g.SoCho, ct);
            if (members.Any(m => !ids.Contains(m.Id)) || groups.Any(g => !expected.TryGetValue(g, out var count) || count != members.Count(m => m.CumGheId == g)))
                throw Invalid("Chọn trọn cụm ghế theo sơ đồ đã công bố.");
        }

        var held = ActiveHolds(booking.Slot.Id, now);
        var sold = Issued(booking.Slot.Id);
        if (await held.AnyAsync(h => h.GheId.HasValue && ids.Contains(h.GheId.Value), ct) ||
            await sold.AnyAsync(v => v.GheId.HasValue && ids.Contains(v.GheId.Value), ct))
            throw Conflict("Một hoặc nhiều ghế đã được giữ hoặc phát hành.");
        if (await held.CountAsync(ct) + await sold.CountAsync(ct) + selections.Length > booking.Slot.SucChua)
            throw Conflict("Suất đã đạt giới hạn sức chứa.");
        var heldTypes = from h in held join line in db.ChiTietDonHang on h.ChiTietDonHangId equals line.Id select line.LoaiVeId;
        foreach (var items in selections.GroupBy(i => i.TicketTypeId))
        {
            var type = types[items.Key];
            if (await heldTypes.CountAsync(t => t == type.Id, ct) + await sold.CountAsync(v => v.LoaiVeId == type.Id, ct) + items.Count() > type.SoLuong)
                throw Conflict("Loại vé đã hết hạn mức bán.");
        }
        foreach (var selected in selections.GroupBy(i => types[i.TicketTypeId].KhuVucId))
        {
            var zone = selected.Key;
            var currentTypes = db.LoaiVe.Where(t => t.SuatDienId == booking.Slot.Id && t.KhuVucId == zone);
            if (await heldTypes.CountAsync(t => currentTypes.Any(x => x.Id == t), ct) +
                await sold.CountAsync(v => currentTypes.Any(t => t.Id == v.LoaiVeId), ct) + selected.Count() > allowedZones[zone].SucChua)
                throw Conflict("Khu vực đã đạt giới hạn sức chứa.");
        }

        var subtotal = selections.Sum(i => types[i.TicketTypeId].GiaVe);
        var fee = await Fee(ct);
        var offer = voucher == null ? null : await VoucherFor(actor.Id, booking.Slot, seats.Values, voucher, subtotal, now, ct);
        var discount = offer?.Amount ?? 0;
        var contract = await db.HopDong.AsNoTracking().Where(c => c.BTCId == booking.Event.BTCId &&
            (c.SuKienId == booking.Event.Id || c.SuKienId == null) && c.TrangThai == TrangThaiHopDong.CoHieuLuc &&
            c.HieuLucTu <= now && (!c.HieuLucDen.HasValue || c.HieuLucDen > now))
            .OrderByDescending(c => c.SuKienId.HasValue).ThenByDescending(c => c.HieuLucTu).ThenByDescending(c => c.Id).FirstOrDefaultAsync(ct);
        var netTickets = subtotal - discount;
        var platformFee = contract == null ? 0 : Math.Min(netTickets,
            contract.LoaiPhi == LoaiPhiHopDong.PhanTram ? decimal.Floor(netTickets * (contract.TyLePhi ?? 0) / 100)
                : (contract.PhiMoiVe ?? 0) * selections.Length);
        var total = subtotal - discount + fee;
        var free = total == 0;
        if (!free && (decimal.Truncate(total) != total || total > long.MaxValue)) throw Invalid("Tổng thanh toán phải là số nguyên VND hợp lệ.");
        if (!free && !payOs.Configured)
            throw new ApiException(503, "Đơn có số tiền phải trả cần cổng thanh toán PayOS. Hiện chỉ xác nhận đơn miễn phí.");
        var order = new DonHang {
            TaiKhoanId = actor.Id, SuatDienId = booking.Slot.Id, ChinhSachId = policy.Value, TienVe = subtotal, TienGiam = discount,
            HopDongId = contract?.Id, PhiDichVu = fee, PhiNenTang = platformFee, TongTien = total, NgayTao = now, HanThanhToan = now.AddMinutes(10),
            TrangThai = free ? TrangThaiDonHang.DaThanhToan : TrangThaiDonHang.ChoThanhToan, KhoaTaoDon = clientKey, DauVanTayTaoDon = fingerprint
        };
        db.DonHang.Add(order);
        await db.SaveChangesAsync(ct);
        foreach (var item in selections)
        {
            var line = new ChiTietDonHang { DonHangId = order.Id, LoaiVeId = item.TicketTypeId, SoLuong = 1,
                DonGia = types[item.TicketTypeId].GiaVe, ThanhTien = types[item.TicketTypeId].GiaVe };
            db.ChiTietDonHang.Add(line);
            await db.SaveChangesAsync(ct);
            var hold = new GiuGhe { SuatDienId = booking.Slot.Id, GheId = item.SeatId, ChiTietDonHangId = line.Id,
                HetHan = order.HanThanhToan, TrangThai = free ? TrangThaiGiuGhe.DaChot : TrangThaiGiuGhe.DangGiu };
            db.GiuGhe.Add(hold);
            await db.SaveChangesAsync(ct);
            if (free) db.Ve.Add(new Ve { SuatDienId = booking.Slot.Id, LoaiVeId = item.TicketTypeId, GheId = item.SeatId,
                ChiTietDonHangId = line.Id, GiuGheId = hold.Id, MaVe = "QLSK-" + Guid.NewGuid().ToString("N")[..16].ToUpperInvariant(),
                MaQR = Convert.ToHexString(RandomNumberGenerator.GetBytes(24)), TrangThai = TrangThaiVe.DaPhatHanh, NgayPhatHanh = now });
        }
        if (offer != null)
            db.SuDungVoucher.Add(new SuDungVoucher { VoucherId = offer.Id, DonHangId = order.Id, SoTienGiam = discount,
                HetHanGiuLuot = order.HanThanhToan, DaChotLuc = free ? now : null, TrangThai = free ? TrangThaiSuDungVoucher.DaSuDung : TrangThaiSuDungVoucher.DangGiu });
        if (free) db.GiaoDichThanhToan.Add(new GiaoDichThanhToan { DonHangId = order.Id, SoTienYeuCau = 0, SoTienThucThu = 0,
            PhuongThuc = PhuongThucThanhToan.MienPhi, TrangThai = TrangThaiGiaoDichThanhToan.ThanhCong, NgayThanhToan = now });
        SqlOperations.Notify(db, actor.Id, free ? "Vé đã được phát hành" : "Đã giữ ghế", "Kiểm tra Đơn #" + order.Id + ".", "ticket");
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return order.Id;
    }

    private sealed record Discount(int Id, decimal Amount);

    private async Task<Discount> VoucherFor(int account, SuatDien slot, IEnumerable<Ghe> seats, string code, decimal subtotal, DateTime now, CancellationToken ct)
    {
        var id = await db.Voucher.AsNoTracking().Where(v => v.MaVoucher == code).Select(v => (int?)v.Id).SingleOrDefaultAsync(ct) ??
            throw Invalid("Mã khuyến mãi không hợp lệ.");
        await SqlOperations.Lock(db, $"qlsk:voucher:{id}", ct);
        var voucher = await db.Voucher.AsNoTracking().SingleAsync(v => v.Id == id, ct);
        if (voucher.TrangThai != TrangThaiVoucher.HoatDong || now < voucher.NgayBatDau || now >= voucher.NgayKetThuc ||
            (voucher.SuKienId.HasValue && voucher.SuKienId != slot.SuKienId) || (voucher.SuatDienId.HasValue && voucher.SuatDienId != slot.Id) ||
            (voucher.KhuVucId.HasValue && seats.Any(s => s.KhuVucId != voucher.KhuVucId)) || subtotal < voucher.GiaTriDonToiThieu)
            throw Invalid("Mã giảm giá chưa hợp lệ cho đơn này.");
        var usage = db.SuDungVoucher.Where(x => x.VoucherId == id && (x.TrangThai == TrangThaiSuDungVoucher.DaSuDung ||
            x.TrangThai == TrangThaiSuDungVoucher.DangGiu && x.HetHanGiuLuot > now));
        if (await usage.CountAsync(ct) >= voucher.SoLuong || await usage.CountAsync(x =>
            db.DonHang.Any(o => o.Id == x.DonHangId && o.TaiKhoanId == account), ct) >= voucher.LuotMoiTaiKhoan)
            throw Conflict("Mã khuyến mãi đã hết lượt.");
        var reduction = voucher.LoaiGiam == LoaiGiamGia.PhanTram
            ? Math.Min(voucher.MucGiamToiDa ?? 0, decimal.Floor(subtotal * voucher.GiaTri / 100)) : voucher.GiaTri;
        return new(id, Math.Min(subtotal, reduction));
    }

    private async Task<int> Used(int owner, int session, DateTime now, CancellationToken ct)
    {
        var lineIds = from line in db.ChiTietDonHang join order in db.DonHang on line.DonHangId equals order.Id
                      where order.TaiKhoanId == owner && order.SuatDienId == session select line.Id;
        var held = await ActiveHolds(session, now).CountAsync(h => lineIds.Contains(h.ChiTietDonHangId), ct);
        var issued = await Issued(session).CountAsync(v => v.ChiTietDonHangId.HasValue && lineIds.Contains(v.ChiTietDonHangId.Value), ct);
        return held + issued;
    }

    private Task<int?> TicketPolicy(CancellationToken ct) => catalog.PublishedPolicies("ticketing")
        .OrderByDescending(p => p.NgayHieuLuc).ThenByDescending(p => p.PhienBan).Select(p => (int?)p.Id).FirstOrDefaultAsync(ct);

    private async Task<decimal> Fee(CancellationToken ct)
    {
        var fee = await db.CauHinhHeThong.AsNoTracking().Where(c => c.Khoa == "booking.serviceFee").Select(c => c.GiaTri).SingleOrDefaultAsync(ct);
        return int.TryParse(fee, out var value) && value >= 0 ? value : 0;
    }

    private sealed class OrderRow
    {
        public required DonHang Order { get; init; }
        public required SuatDien Session { get; init; }
        public required string Event { get; init; }
    }

    private IQueryable<OrderRow> Rows() => from order in db.DonHang.AsNoTracking()
        join slot in db.SuatDien on order.SuatDienId equals slot.Id join evt in db.SuKien on slot.SuKienId equals evt.Id
        select new OrderRow { Order = order, Session = slot, Event = evt.TenSuKien };

    public async Task<PageDto<OrderDto>> List(TaiKhoan actor, int? eventId, string? status, string? cursor, CancellationToken ct)
    {
        if (eventId.HasValue) await events.NeedOwner(actor, eventId.Value, ct);
        var after = After(cursor);
        const int size = 30;
        var query = Rows().Where(r => r.Order.Id > after);
        query = eventId.HasValue ? query.Where(r => r.Session.SuKienId == eventId.Value) : query.Where(r => r.Order.TaiKhoanId == actor.Id);
        if (status != null)
        {
            var code = Enum.GetValues<TrangThaiDonHang>().SingleOrDefault(p => State(p).Code == status);
            query = query.Where(r => r.Order.TrangThai == code);
        }
        var rows = await query.OrderBy(r => r.Order.Id).Take(size + 1).ToListAsync(ct);
        return Page(rows.Select(row => Project(row, null, [])).ToArray(), size, row => row.Id);
    }

    public async Task<OrderDto> Detail(TaiKhoan actor, int id, CancellationToken ct)
    {
        var row = await Rows().SingleOrDefaultAsync(r => r.Order.Id == id, ct) ?? throw Missing();
        if (row.Order.TaiKhoanId != actor.Id && !await events.Owns(actor, row.Session.SuKienId, ct)) throw Missing();
        var lines = await (from l in db.ChiTietDonHang.AsNoTracking() join t in db.LoaiVe on l.LoaiVeId equals t.Id
            join z in db.KhuVuc on t.KhuVucId equals z.Id where l.DonHangId == id orderby l.Id
            select new OrderItemDto(t.TenLoaiVe, z.TenKhuVuc, (from hold in db.GiuGhe join seat in db.Ghe on hold.GheId equals seat.Id
                where hold.ChiTietDonHangId == l.Id select seat.MaGhe).FirstOrDefault(), l.DonGia)).ToListAsync(ct);
        var canCancel = row.Order.TaiKhoanId == actor.Id && row.Order.TrangThai == TrangThaiDonHang.ChoThanhToan &&
            row.Order.HanThanhToan > DateTime.UtcNow && !await PaymentStarted(id, ct);
        var actions = new List<string>();
        if (canCancel) actions.Add("cancel");
        if (payOs.Configured && row.Order.TaiKhoanId == actor.Id && row.Order.TrangThai == TrangThaiDonHang.ChoThanhToan && row.Order.HanThanhToan > DateTime.UtcNow) actions.Add("pay");
        if (payOs.Configured && row.Order.TaiKhoanId == actor.Id && await PaymentStarted(id, ct)) actions.Add("reconcile");
        var result = Project(row, lines, actions);
        if (await db.GiaoDichThanhToan.AnyAsync(p => p.DonHangId == id && p.TrangThai == TrangThaiGiaoDichThanhToan.NghiVan, ct))
            result = result with { StatusLabel = "Khoản thu cần xác minh · chưa phát hành vé" };
        return result;
    }

    public async Task<OrderDto> Cancel(TaiKhoan actor, int id, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var session = await db.DonHang.Where(r => r.Id == id && r.TaiKhoanId == actor.Id).Select(r => (int?)r.SuatDienId).SingleOrDefaultAsync(ct)
            ?? throw Missing();
        await SqlOperations.Lock(db, $"qlsk:session:{session}", ct);
        var row = await db.DonHang.SingleAsync(o => o.Id == id, ct);
        if (row.TrangThai != TrangThaiDonHang.ChoThanhToan || await PaymentStarted(id, ct))
            throw Conflict("Đơn đã thay đổi hoặc có giao dịch cần đối soát.");
        row.TrangThai = row.HanThanhToan <= DateTime.UtcNow ? TrangThaiDonHang.HetHan : TrangThaiDonHang.DaHuy;
        await db.GiuGhe.Where(h => h.TrangThai == TrangThaiGiuGhe.DangGiu &&
            db.ChiTietDonHang.Any(line => line.Id == h.ChiTietDonHangId && line.DonHangId == id))
            .ExecuteUpdateAsync(set => set.SetProperty(h => h.TrangThai, TrangThaiGiuGhe.DaNha), ct);
        await db.SuDungVoucher.Where(v => v.DonHangId == id && v.TrangThai == TrangThaiSuDungVoucher.DangGiu)
            .ExecuteUpdateAsync(set => set.SetProperty(v => v.TrangThai, TrangThaiSuDungVoucher.DaNha), ct);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await Detail(actor, id, ct);
    }

    private Task<bool> PaymentStarted(int order, CancellationToken ct) => db.GiaoDichThanhToan.AnyAsync(p => p.DonHangId == order &&
        (p.TrangThai == TrangThaiGiaoDichThanhToan.ChoThanhToan || p.TrangThai == TrangThaiGiaoDichThanhToan.ThanhCong), ct);

    private static OrderDto Project(OrderRow row, IReadOnlyList<OrderItemDto>? items, IReadOnlyList<string> actions)
    {
        var o = row.Order;
        var status = State(o.TrangThai == TrangThaiDonHang.ChoThanhToan && o.HanThanhToan <= DateTime.UtcNow ?
            TrangThaiDonHang.HetHan : o.TrangThai);
        return new(o.Id, $"Đơn #{o.Id}", row.Event, ScheduleService.NameOf(row.Session), Stamp(o.NgayTao),
            o.TienVe, o.TienGiam, o.PhiDichVu, o.TongTien, status.Code, status.Label, Stamp(o.HanThanhToan), items, actions);
    }

    internal static (string Code, string Label) State(TrangThaiDonHang state) => state switch {
        TrangThaiDonHang.ChoThanhToan => ("PENDING_PAYMENT", "Chờ thanh toán"), TrangThaiDonHang.DaThanhToan => ("PAID", "Đã xác nhận"),
        TrangThaiDonHang.DaHuy => ("CANCELLED", "Đã hủy"), TrangThaiDonHang.DaThuTienHetCho => ("PAID_OUT_OF_STOCK", "Tiền đến sau khi hết chỗ"),
        TrangThaiDonHang.ChoHoanTien => ("REFUND_PENDING", "Chờ hoàn"), TrangThaiDonHang.DaHoanTien => ("REFUNDED", "Đã hoàn"),
        TrangThaiDonHang.HoanMotPhan => ("PARTIALLY_REFUNDED", "Hoàn một phần"), _ => ("EXPIRED", "Hết hạn")
    };
}
