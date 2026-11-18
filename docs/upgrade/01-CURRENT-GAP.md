# 01 — Kiểm kê hiện trạng và Gap Analysis

**Repository:** `vtishan/qlsk`, branch `main`, đọc ngày 19/11/2026. Dẫn chứng là đường dẫn mã nguồn GitHub. Phân loại: **Đã có**, **Một phần**, **Chưa thấy trong entity/controller/service đã đọc** (không có nghĩa tuyệt đối không tồn tại).

## 1. Kiến trúc có sẵn — cần kế thừa

- [README](../../README.md), [QLSK.csproj](../../QLSK.csproj): ASP.NET Core **.NET 10**, EF Core SQL Server, JWT, SMTP, QuestPDF, PayOS; frontend HTML/CSS/JS trong `wwwroot`, dùng chung backend. Kiến trúc feature-first modular monolith.
- [ApplicationDbContext](../../Data/ApplicationDbContext.cs): 16 DbSet nghiệp vụ (bao gồm bảng lịch sử xét duyệt); các entity đã có gồm `TaiKhoan`, `VaiTro`, `BanToChuc`, `SuKien`, `DiaDiem`, `DatDiaDiem`, `LoaiVe`, `DonVe`, `ThanhToan`, `HoanTien`, `Ve`.
- [Migrations](../../Migrations) quản lý schema, có migrations cải thiện index và bảo mật; đã có [tests/QLSK.Tests](../../tests/QLSK.Tests).
- [Program.cs](../../Program.cs): đăng ký EF Core/SQL Server, JWT, PayOS `HttpClient`, `ExpiredOrderWorker` và `EventStateWorker`.
- Mô hình đang sử dụng **event là đơn vị bán vé** và số lượng vé theo loại; giữ nguyên dữ liệu hiện tại khi bổ sung session/seat.

## 2. Điểm cần sửa đặc biệt: sai ý nghĩa role hiện hữu

Đọc [VaiTroConfig.cs](../../Data/Configurations/VaiTroConfig.cs): bốn role đang seed là ID 1 `NguoiDung`, ID 2 `BanToChuc`, ID 3 `QuanLySuKien`, ID 4 `QuanTriVien`.

Theo mã hiện tại, `QuanLySuKien` thực tế là quyền **quản lý hệ thống**:
- [SuKienController.cs](../../Features/SuKien/SuKienController.cs): có quyền `GET /api/events/manage`, phê duyệt hoặc từ chối sự kiện.
- [XacThucController.cs](../../Features/XacThucToChuc/XacThucController.cs): có quyền xem và xét duyệt hồ sơ ban tổ chức.
- [DiaDiemController.cs](../../Features/DiaDiem/DiaDiemController.cs): có quyền xét duyệt đặt địa điểm.
- [ThanhToanController.cs](../../Features/ThanhToan/ThanhToanController.cs): có quyền ghi nhận hoàn tiền.
- [VeController.cs](../../Features/Ve/VeController.cs): có quyền check-in và xem vé của người khác.

**Rủi ro cao nhất khi nâng cấp:** Nếu chỉ đổi nhãn role `QuanLySuKien` thành quyền nhân viên BTC mà không đổi từng `[Authorize(Roles=...)]`, nhân viên BTC sẽ có thể truy cập các endpoint quản trị toàn hệ thống. Không được thực hiện cách này.

**Đề xuất migration:** giữ role ID, chuyển **ý nghĩa ID 3** từ `QuanLySuKien` hiện hữu sang `QuanLy` (system manager), đổi seed, token/claim, kiểm tra tài khoản đã cấp và viết test; thêm role ID 5 `QuanLySuKien` mới với phạm vi cấp quyền theo BTC/sự kiện. Vai trò ID 4 `QuanTriVien` map `admin` ở tầng thuật ngữ; không đổi ID để bảo toàn FK. Có thể chọn tên lưu trữ `QuanLy`/`QuanLySuKien`/`QuanTriVien` như C# hiện có, với UI/mô tả `quanly`/`quanlysukien`/`admin`. Xác minh migration dữ liệu và tất cả authorization trước khi áp dụng thực tế.

## 3. Bảng chênh lệch theo nghiệp vụ

| Mã | Nghiệp vụ đã chốt | Bằng chứng code hiện tại | Đánh giá / nâng cấp |
|---|---|---|---|
| GAP-01 | 5 role; quanlysukien là nhân viên BTC có scope | `VaiTroConfig.cs`, `TaiKhoan.cs` mỗi account một `VaiTroId` | **Chưa đáp ứng**; cần tách role và staff permission theo event |
| GAP-02 | Nộp lại hồ sơ BTC, SLA 7 ngày làm việc | `XacThucService.cs` đã nộp/duyệt/từ chối/lưu lịch sử; xóa yêu cầu sau quyết định | **Một phần**; thêm SLA, vòng đời dễ theo dõi, yêu cầu bổ sung |
| GAP-03 | Revision `PENDING_UPDATE`, bản public giữ nguyên | `SuKienService.cs` chỉ sửa `ChoDuyet`/`TuChoi`, ghi đè hàng `SuKien` | **Chưa đáp ứng**; thêm `SuKienRevision` và phê duyệt bản sửa |
| GAP-04 | Multi-day, multi-session, multi-venue | `SuKien.cs` có một khoảng thời gian; `DatDiaDiem.cs` gắn `SuKienId` | **Chưa đáp ứng**; thêm `SuatDien`/`Session`, venue theo session |
| GAP-05 | Zone, Seat, Standard/VIP/Invitation, seat map | `LoaiVe.cs` có giá, số lượng, thời gian mở bán; `Ve.cs` không có `SeatId/SessionId` | **Một phần**; tạo zone, seat, loại vé có mã chuẩn, vé mời |
| GAP-06 | Giữ chỗ 10 phút, atomic seat lock | `DonVeService.cs` có transaction + `TransactionLock`, mặc định `Order:ExpireMinutes=15`, giữ **số lượng** theo loại | **Một phần**; đổi TTL 10, inventory theo session-seat, giữ lock ngắn |
| GAP-07 | Tối đa 10 vé/tài khoản/session tính cả hold | `DonVeService.cs` giới hạn **20 vé mỗi đơn**, có hạn chế số đơn pending | **Chưa đáp ứng**; quota theo user/session bao gồm paid + hold |
| GAP-08 | PayOS QR, checksum/idempotency, payment late | `PayOsService.cs` xác minh chữ ký; `ThanhToanService.cs` có lock và chống trùng tham chiếu, ghi `NghiVan` nếu đơn hết hạn | **Một phần**; bổ sung state `PAID_OUT_OF_STOCK`, worker đối soát, hoàn tự động |
| GAP-09 | QR một lần/session + nhân viên check-in theo event | `VeService.cs` chặn quét lại, `VeController.cs` cho role `QuanLySuKien,QuanTriVien` check-in toàn cục | **Một phần**; chuyển quyền scan theo event, thêm log/check-in thủ công |
| GAP-10 | Hoàn tự động >=72h, phí và hoàn đặc biệt | `ThanhToanController.cs` chỉ thấy route quản lý ghi nhận hoàn tiền | **Một phần**; cần self-service refund workflow, policy, audit/outbound transfer |
| GAP-11 | Hoãn/hủy phải qua xét duyệt, giữ vé khi hoãn | `SuKienService.cs` có `HuySuKien` do BTC gọi trực tiếp | **Chưa đáp ứng**; đề nghị hoãn/hủy, manager quyết định, xử lý vé/notify |
| GAP-12 | Địa điểm Admin có sẵn, quản lý duyệt + cọc 30%/24h | `DiaDiemService.cs` có chống trùng bằng Serializable và xác nhận/từ chối, `DatDiaDiem.cs` có `TienCoc` | **Một phần**; thêm chứng từ thanh toán cọc, hạn giữ lịch và hoàn theo mốc |
| GAP-13 | Đối soát 3–5 ngày, phí, settlement và payout | Có `ThanhToan`/`HoanTien` và thống kê, chưa thấy entity Settlement trong `DbContext` | **Chưa thấy**; thêm settlement/ledger và quy trình duyệt chi trả |
| GAP-14 | Nhân viên BTC không xem dữ liệu ngoài scope | Controller check-in và lookup hiện căn cứ role `QuanLySuKien` toàn cục | **Chưa đáp ứng**; bắt buộc authorize theo event và permission |

## 4. Điểm hiện có cần giữ

1. Luồng đăng ký/đăng nhập, JWT, `TokenVersion`, ownership check đang có.
2. Phê duyệt organizer và event có lịch sử/kiểm soát người tự duyệt.
3. Tồn kho dạng số lượng, khóa transaction và background worker; dùng làm nền cho seat inventory mới.
4. PayOS có xác thực chữ ký và kiểm tra amount/order code, cần mở rộng thay vì thay thế.
5. QR, PDF, email, dashboard, API/test cơ bản.
6. Chống trùng đặt địa điểm bằng Serializable; giữ logic và mở rộng thành booking chờ cọc.

## 5. Những việc chưa xác nhận

Chưa chạy code, build, test, migration hay kiểm tra database production. Chưa kiểm chứng SDK PayOS mới, điều kiện tài khoản nhận tiền hoặc API chi hộ ngân hàng. Các kiểm tra khác ở frontend/worker/tests cần được rà thêm theo PR implementation. Báo cáo này **không** tuyên bố có lỗi exploitable ở production khi chưa có thử nghiệm.
