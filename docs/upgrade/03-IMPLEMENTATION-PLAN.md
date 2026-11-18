# 03 — Thiết kế nâng cấp, chuyển đổi dữ liệu, API và nghiệm thu

> Kế hoạch áp dụng trên **ASP.NET Core .NET 10 / EF Core SQL Server / frontend HTML-JS** của QLSK, không phải dự án mới. Các tên class/bảng/endpoint dưới là **đề xuất**; chỉ chốt khi migration được review. Mỗi bước nên tạo PR riêng, không gộp thay đổi nghiệp vụ nhạy cảm vào một migration lớn.

## 1. Thứ tự ưu tiên và phạm vi thay đổi

| Phase | Việc thực hiện | Thành phần hiện tại cần tác động | Điều kiện hoàn tất |
|---|---|---|---|
| P0 – Safety/RBAC | Kiểm kê tài khoản role ID 3, đổi ý nghĩa thành `quanly`, thêm `quanlysukien` cấp dưới, scope permission | `Domain/Entities/TaiKhoan.cs`, `VaiTro.cs`, `Data/Configurations/VaiTroConfig.cs`, `Common/Auth/*`, `Features/*Controller.cs`, migrations | Không có staff nào truy cập duyệt toàn hệ thống; các chức năng cũ vẫn phân quyền đúng |
| P1 – Sessions | `Session`, `Zone`, `Seat`, `SessionZone`, migration/backfill event cũ có 1 session mặc định | `Domain/Entities/SuKien.cs`, `LoaiVe.cs`, `DatDiaDiem.cs`, `Data/ApplicationDbContext.cs`, cấu hình EF | Mọi event cũ còn thể hiện đúng lịch/venue/vé |
| P2 – Versioning | Event revision, duyệt theo phiên bản, đổi lịch có impact validation | `Features/SuKien/*`, event entity, frontend ban-to-chuc/quan-tri | Pending update không làm thay đổi event public |
| P3 – Seats/booking | Seat inventory, chọn ghế, quota 10/session, hold 10 phút | `Features/DonVe/*`, `Features/LoaiVe/*`, `Common/Inventory/*`, `ExpiredOrderWorker` | Không oversell dưới tải đồng thời, cả cụm ghế nguyên tử |
| P4 – Payment | Reconciliation PayOS, paid late, yêu cầu refund có flow riêng | `Infrastructure/PayOS/*`, `Features/ThanhToan/*`, background worker mới | Lặp webhook/retry không xuất vé, hoàn hoặc thu tiền trùng |
| P5 – Staff/check-in | Tạo/mời account con, cấp/revoke quyền từng event, log scan/manual | `Features/TaiKhoan/*`, `Features/Ve/*`, `wwwroot/js/checkin.js` | Người của BTC A không truy cập event BTC B |
| P6 – Venue/finance | Cọc 30%/24h, phạt cọc, settlement, payout, báo cáo | `Features/DiaDiem/*`, `Features/ThongKe/*`, new ledger/settlement | Đối soát hợp lệ, không trùng đơn/tiền hoàn/chi trả |
| P7 – QA rollout | API/DB/front regression, test concurrency, staging migration, release flags | `tests/QLSK.Tests/*`, `tests/TEST_PLAN.md`, deploy | UAT & rollback thử nghiệm thành công |

## 2. Thiết kế RBAC cụ thể (ưu tiên số 1)

### 2.1. Không cấp nguyên role hiện hữu `QuanLySuKien` cho nhân viên

Role ID 3 hiện được API coi là quản lý toàn hệ thống. Kế hoạch migration:
1. Kiểm tra **danh sách account có role ID 3** ở DB thật và chủ sở hữu của từng account.
2. Chuyển tên/chức năng role ID 3 thành `QuanLy` (system manager), giữ ID để tránh hỏng FK. Cập nhật tất cả `Authorize(Roles)`, `User.IsInRole`, claims/token, UI guards và tests cùng lúc.
3. Tạo role mới ID 5 `QuanLySuKien`, KHÔNG thêm vào danh sách được duyệt global.
4. Mở rộng bảng cấp quyền `NhanSuBanToChuc` (`OrganizerId`, `UserId`, trạng thái), `PhanQuyenNhanSu` (`StaffId`, `EventId`, `PermissionCode`, `GrantedBy`, `RevokedAt`).
5. Middleware/filter/authorization handler trả `403` nếu thiếu quyền **hoặc** sai organizer/event scope. Không tin event ID từ client để suy ra sở hữu.
6. `bantochuc` có thể tự quét sự kiện của mình; `quanlysukien` chỉ quét khi có `checkin.scan`; không thể tự cấp quyền.

Quyền gợi ý: `checkin.scan`, `checkin.manual`, `attendees.view`, `tickets.view`, `orders.view`, `reports.view`, `event.edit`, `invitation.manage`. Chỉ cấp quyền tối thiểu.

**Đặc biệt:** Token cũ chứa role `QuanLySuKien` phải bị vô hiệu hóa sau chuyển đổi (token version/rotation), để không tiếp tục được dùng với các endpoint legacy.

### 2.2. Migrations và compatibility

- Không đổi tên/ID `NguoiDung` (1) và `BanToChuc` (2) nếu không cần.
- ID 4 `QuanTriVien` hiện có thể giữ nguyên trong storage và map hiển thị `admin` hoặc thay tên cùng lúc cập nhật toàn bộ controller/claims.
- Cần có migration seed-role deterministic và test trên DB sao chép dữ liệu cũ; không để một migration deploy nửa chừng làm khóa người quản trị.

## 3. Thiết kế dữ liệu đề xuất (EF Core)

| Entity mới hoặc mở rộng | Cột chính dự kiến | Index/check |
|---|---|---|
| `SuatDien` | `Id, SuKienId, DiaDiemId, BatDauUtc, KetThucUtc, TrangThai` | FK event/venue, end > start |
| `KhuVuc` | `Id, DiaDiemId, Ten, SucChua` | unique venue/name |
| `Ghe` | `Id, KhuVucId, Hang, SoGhe, LoaiGhe` | unique zone/row/seat |
| `SuatDienKhuVuc` | `SuatDienId, KhuVucId, SucChua` | unique session/zone |
| `TonKhoGhe` | `SuatDienId, GheId, HoldId, HetHanGiuUtc, VeId, TrangThai` | **unique session/seat** |
| `SuatDienLoaiVe` | `SuatDienId, LoaiVeId, ZoneId, Quota, Gia, Loai` | prevent quota > capacity |
| `KhachMoi` | `SuatDienId, Email, SoLuong, NguoiMoi, TrangThai` | inventory allotted before sending |
| `PhienBanSuKien` | `SuKienId, SoPhienBan, Payload, TrangThai, NguoiDuyet` | unique event/revision |
| `NhanSuBanToChuc`/`PhanQuyenNhanSu` | Staff/org/event/permission | unique active grant |
| `LichSuCheckin` | `VeId, NhanVienId, ThoiGian, PhuongThuc, KetQua` | audit all attempts |
| `YeuCauHoanTien` | `DonVeId, VeId?, SoTienVe, Phi, TongHoan, LyDo, TrangThai` | idempotency |
| `GiaoDichDoiSoat` | `PaymentId, ProviderRef, Result, CheckedAt` | uniqueness provider transaction |
| `PhieuDoiSoat`/`DongDoiSoat` | event/organizer/gross/refund/fees/net | traceable ledger, unique settle item |
| `DatCocDiaDiem` | `DatDiaDiemId, SoTien, HanThanhToanUtc, TrangThai` | avoid duplicate deposit |
| `NhatKyPhanQuyen` | actor/subject/event/permission/action/time | append-only |

**Nâng cấp có dữ liệu cũ:**
- Backfill **mỗi event cũ thành một session mặc định** tương ứng `ThoiGianBatDau/ThoiGianKetThuc`, liên kết địa điểm từ `DatDiaDiem` nếu đã có; trường hợp event thiếu venue cần tạo báo cáo dữ liệu lỗi và xử lý trước publish.
- Loại vé cũ chuyển sang liên kết session mới; **không** xóa `SuKienId` trước khi migration/backfill và mọi API đã đọc session.
- Vé đã phát hành trước khi có sơ đồ ghế cần chính sách `UNASSIGNED`/không ghế riêng; không tự gán ghế tùy tiện.
- Khi thay đổi giá/zone/venue sau bán, lưu snapshot `donGia`, `seat`, `session` tại thời điểm phát vé để lịch sử không biến đổi.
- Chuyển dần từ đếm theo `LoaiVe.SoLuongDaBan` sang inventory session/seat; đối chiếu tổng trước/sau ở script migration.

## 4. Giao dịch đặt ghế và thanh toán an toàn

### 4.1. Đặt ghế (SQL Server)

1. Request gồm `sessionId`, danh sách `seatIds`, loại vé; không nhận tổng tiền do client làm nguồn chân lý.
2. Bắt đầu transaction **ngắn**, khóa bản ghi quota user/session và inventory ghế theo thứ tự ổn định; dùng `UPDLOCK, HOLDLOCK`/isolation phù hợp SQL Server hoặc rowversion/retry với ràng buộc unique. (`SELECT FOR UPDATE` là cú pháp PostgreSQL/MySQL, **không phải SQL Server** của dự án này.)
3. Kiểm tra `paid + active holds + requested <= 10`, không trùng seat, zone capacity, loại vé còn bán.
4. Tạo hold có `expiresAtUtc=now+10m`, booking chờ trả tiền, commit ngay.
5. Worker lặp 10–15s (tần suất mục tiêu) giải phóng hold hết hạn **nếu HoldId và status vẫn khớp**, tránh giải phóng hold mới.
6. Nhận PayOS success: khóa order, payment + inventory; nếu hold còn, phát vé; nếu hold hết mà ghế còn thì cố gắng tái đặt nguyên tử, nếu mất thì `PAID_OUT_OF_STOCK` + refund 100%.
7. Mọi request/webhook có idempotency key; số liệu doanh thu được cập nhật một lần.

**Không** đặt unique trần `(sessionId,seatId)` trên toàn bộ lịch sử `Ve` nếu cho hủy/tái bán; dùng một hàng inventory/session/seat hoặc filtered unique trên trạng thái đang chiếm chỗ.

### 4.2. State machine đề xuất

- Hold: `HELD → BOOKED | EXPIRED | CANCELLED`.
- Order: `PENDING_PAYMENT → PAID | EXPIRED | CANCELLED | PAID_OUT_OF_STOCK`.
- Payment: `PENDING → SUCCESS | FAILED | SUSPECT`; lưu trạng thái provider độc lập mapping.
- Ticket: `ISSUED → CHECKED_IN | CANCELLED | REFUNDED`.
- Event revision: `DRAFT → PENDING_UPDATE → APPROVED/APPLIED | REJECTED`.
- Venue reservation: `REQUESTED → APPROVED_HOLD → AWAITING_DEPOSIT → CONFIRMED | EXPIRED | CANCELLED`.
- Refund: `REQUESTED → APPROVED_AUTO | PENDING_MANUAL → PROCESSING → COMPLETED | FAILED | REJECTED`.
- Settlement: `DRAFT → ORGANIZER_CONFIRMED → MANAGER_APPROVED → PAID`.

Không đánh dấu refund/payout `COMPLETED` khi mới tạo lệnh chuyển khoản.

## 5. API kế thừa và API bổ sung

| Hành động | Hiện có (nguồn) | Dự kiến sau nâng cấp |
|---|---|---|
| Đăng ký/đăng nhập | `POST /api/auth/register`, `POST /api/auth/login` | Giữ |
| Nộp và duyệt BTC | `/api/organizer-verifications` | Giữ route, thêm SLA, bổ sung, phân quyền `quanly` |
| Tạo/duyệt sự kiện | `/api/events`, `/api/events/{id}/approve` | Giữ, thêm `/api/events/{id}/revisions`, `/api/events/{id}/sessions` |
| Loại vé | `/api/ticket-types` (xác minh route chính xác khi code PR) | Thêm liên kết session + zone |
| Giữ vé | `POST /api/orders` | Giữ compatibility; bổ sung payload seatIds/sessionId |
| Thanh toán | `POST /api/payments`, `POST /api/payments/webhook` | Thêm trạng thái/reconciliation/refund-request |
| Check-in | `POST /api/check-in` | Thêm session/event scope, manual & audit |
| Địa điểm | `/api/venues`, `/api/venue-bookings` | Thêm deposit, cancellation policy và status |
| Staff BTC | Chưa có module được tìm thấy | `/api/organizers/{id}/staff`, `/api/events/{id}/staff-permissions` |
| Settlement | Chưa có module được tìm thấy | `/api/settlements`, `/api/settlements/{id}/approve` |

Tài liệu này không khóa cứng route mới: thống nhất với convention hiện tại và API contract tests trước khi implement.

## 6. Kế hoạch kiểm thử bắt buộc

| Test | Kịch bản / kết quả |
|---|---|
| SEC-01 | `quanlysukien` đăng nhập, gọi approve organizer/event/venue/refund => 403 |
| SEC-02 | Staff BTC A quét hoặc xem người mua BTC B => 403 |
| SEC-03 | BTC thu hồi quyền staff, token cũ không thể tiếp tục quét |
| ORG-01 | Hồ sơ BTC bị từ chối → sửa → gửi lại, lưu lịch sử đầy đủ |
| EVT-01 | Event đang bán, revision pending: trang public và đơn hiện hữu vẫn dùng bản cũ |
| EVT-02 | Đổi seat/venue khi đã bán vé: buộc impact validation, không xóa dữ liệu đã phát |
| SEAT-01 | 100 request cùng giữ ghế, chỉ một thành công |
| SEAT-02 | Couple seats bị chiếm 1 ghế, toàn bộ giỏ không được giữ |
| QUOTA-01 | 6 vé paid + 4 hold/session cho phép, thêm 1 bị chặn; session khác độc lập |
| TTL-01 | Hold tự hết sau 10 phút; worker chạy đồng thời webhook không tạo double booking |
| PAY-01 | Webhook lặp/timeout/retry chỉ hạch toán và cấp vé một lần |
| PAY-02 | Thanh toán đến muộn, ghế còn: cấp lại nếu quota/capacity đạt |
| PAY-03 | Thanh toán đến muộn, ghế mất: `PAID_OUT_OF_STOCK`, hoàn 100% |
| CHECK-01 | Một vé quét lần hai => cảnh báo, có timestamp của lần quét đầu |
| CHECK-02 | Manual check-in phải có permission riêng, log người thực hiện |
| REF-01 | Khách tự hủy >=72h, policy bật, chưa check-in: tính hoàn đúng |
| REF-02 | Tự hủy <72h hoặc vé đã check-in: hệ thống không tự hoàn |
| CANCEL-01 | Hoãn giữ giá trị vé; hủy đóng bán và tính hoàn 100% số thực trả |
| VEN-01 | Booking venue đã duyệt không cọc trong 24h tự hết hạn |
| VEN-02 | Cọc 30%, hủy 15/10/5 ngày tương ứng hoàn 80/50/0% tiền cọc |
| FIN-01 | Settlement có refund + fee, tính ra net chính xác; chạy lại không double payout |
| DATA-01 | Backfill giữ nguyên ID, FK, đơn đã trả tiền, vé PDF/QR và lịch thuê hiện hữu |

Cần chạy `dotnet test tests/QLSK.Tests/QLSK.Tests.csproj` sau từng PR, bổ sung integration tests với SQL Server thật và PayOS sandbox/mock. **Chưa chạy các lệnh này trong đợt tài liệu.**

## 7. Quy tắc release

1. Tạo backup + rehearsal restore của DB staging.
2. Migration dạng additive/backfill, có script kiểm tra dữ liệu, rollback hoặc forward-fix được diễn tập.
3. Mở feature flag cho RBAC, session, seat booking, PayOS reconcile theo từng giai đoạn.
4. Không chuyển role ID 3 sang tài khoản con cho đến khi 100% endpoint quản trị đã bảo vệ.
5. Build/test + security/concurrency/finance UAT; chỉ sau đó triển khai production.
6. Giám sát webhook backlog, trạng thái `PAID_OUT_OF_STOCK`, oversell attempts, refund/payout discrepancy và lỗi migration.
