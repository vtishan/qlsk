# 02 — Đặc tả nâng cấp nghiệp vụ (SRS v1.0)

> **Mục tiêu sau nâng cấp** dựa trên quy tắc người dùng đã chốt. Không coi các chức năng ở đây là đã có trong code `main`. Đối chiếu [01-CURRENT-GAP.md](01-CURRENT-GAP.md).

## 1. Mục tiêu và tác nhân

Hệ thống nâng cấp là nền tảng quản lý sự kiện, bán vé, thuê địa điểm do Admin cung cấp, xét duyệt BTC/sự kiện, check-in, hoàn tiền, báo cáo và đối soát. Không có role chủ địa điểm độc lập.

### 1.1. Phân nhóm người truy cập

- **Guest:** chưa đăng nhập; xem/truy vấn sự kiện công khai và địa điểm, đăng ký/đăng nhập.
- **`nguoidung`:** mặc định khi đăng ký; hồ sơ, đăng ký BTC, xem/chọn/mua vé, thanh toán, vé QR, hủy/hoàn theo chính sách, yêu cầu thuê, lịch sử, thông báo, khiếu nại.
- **`bantochuc`:** User đã được `quanly` xét duyệt; tạo/điều chỉnh sự kiện, suất diễn, venue/zone/seat, loại vé, danh sách khách mời, cấu hình hoàn, yêu cầu duyệt; quản lý đơn/vé/báo cáo của BTC; cấp và thu hồi quyền tài khoản con theo sự kiện.
- **`quanlysukien`:** tài khoản con được BTC tạo hoặc mời; chỉ truy cập các sự kiện được gán và quyền tương ứng. Mặc định tối thiểu `checkin.scan`; có thể cấp thêm theo quyết định BTC, không có quyền hệ thống.
- **`quanly`:** quản lý nền tảng, xét duyệt BTC (SLA 7 ngày làm việc), sự kiện/bản sửa, thuê địa điểm, hoãn/hủy, khiếu nại, ngoại lệ hoàn tiền và xác nhận đối soát trong quyền Admin cấp.
- **`admin`:** quản trị kỹ thuật, role/permission, tài khoản, venue catalog, cấu hình phí, tài chính, an toàn/giám sát, đình chỉ khẩn cấp và audit.

`quanlysukien` **không** là `quanly`. Quyền đối tượng phải check ở backend. Mỗi nhân viên có thể được gán nhiều sự kiện, phạm vi từng nhiệm vụ riêng; không tự nâng quyền.

## 2. Nhóm Use Case theo vai trò

| Vai trò | Các use case bắt buộc |
|---|---|
| Guest | Xem trang chủ/danh mục, lọc/tìm, xem sự kiện/venue công khai, đăng ký/đăng nhập |
| nguoidung | Đổi hồ sơ/mật khẩu, nộp/sửa hồ sơ BTC, chọn session & ghế, giữ 10 phút, mua/PayOS, xem mã vé/đơn, hủy/hoàn, theo dõi thông báo |
| bantochuc | Tạo sự kiện, tạo session & zone, cấu hình vé, phát vé mời, chọn thuê venue, nộp duyệt, tạo revision, yêu cầu hoãn/hủy, tạo staff/ủy quyền, xem check-in & doanh thu |
| quanlysukien | Xem event được giao, quét QR, tra cứu thủ công nếu có quyền; chức năng khác chỉ khi được cấp tường minh |
| quanly | Xét duyệt BTC/revision/event/venue booking, xử lý vi phạm, hoãn/hủy, xác minh refund, đối soát, xử lý khiếu nại |
| admin | Quản lý venue catalog, accounts & roles, cấu hình phí, payout, bảo mật, theo dõi audit và can thiệp khẩn cấp |

## 3. Yêu cầu chức năng bắt buộc và tiêu chí nghiệm thu

| Mã | Yêu cầu mục tiêu | Nghiệm thu |
|---|---|---|
| FR-01 | Công khai sự kiện/venue đã duyệt, bộ lọc | Guest không thấy draft, event bị suspend |
| FR-02 | Quản lý account và 5 vai trò | Endpoint server-side từ chối vai trò trái quyền |
| FR-03 | Hồ sơ BTC và xét duyệt ≤7 ngày làm việc | Có người duyệt, thời điểm, lý do, lịch sử gửi lại |
| FR-04 | Sự kiện đa ngày/nhiều session/nhiều venue | 1 event có tối thiểu 2 sessions tại venue khác nhau |
| FR-05 | Sự kiện revision `PENDING_UPDATE` | Bản public còn nguyên cho đến khi manager duyệt |
| FR-06 | Đình chỉ khẩn cấp và phê duyệt hoãn/hủy | Ngừng bán ngay khi suspend; lưu audit |
| FR-07 | Zone và seat map/venue capacity theo session | Kiểm tra suất diễn/khu vực; không tính cộng dồn giữa sessions |
| FR-08 | Standard/VIP/Invitation | Vé mời không công khai, chiếm inventory trước khi phát |
| FR-09 | Giữ đúng ghế 10 phút; đặt cả cụm nguyên tử | Tranh chấp 1 ghế => một request thành công |
| FR-10 | Tối đa 10 vé/tài khoản/session | Đếm paid + hold chưa hết hạn + đang chọn |
| FR-11 | PayOS QR, verify chữ ký, idempotency, status query | Webhook trùng không tạo vé trùng |
| FR-12 | Xử lý thanh toán đến muộn | Ghế còn => có thể cấp lại trong transaction; đã chiếm => PAID_OUT_OF_STOCK + hoàn 100% |
| FR-13 | Vé QR duy nhất & check-in 1 lần/session | Quét lại hiển thị đã check-in lúc nào |
| FR-14 | Nhân viên BTC được cấp quyền theo event | Staff không xem/quét sự kiện ngoài scope |
| FR-15 | Check-in thủ công theo booking code/email/phone | Chỉ staff có permission; tìm kiếm có audit/masking |
| FR-16 | Tự hủy trước ít nhất 72h nếu chính sách bật | Vé chưa check-in; tính hoàn hợp lệ |
| FR-17 | Hoãn/hủy event và thông báo người mua | Hoãn giữ giá trị vé; hủy hoàn đủ số tiền thực trả |
| FR-18 | Thuê venue do Admin quản lý, manager duyệt | Chống chồng giờ, lịch giữ/đặt cọc chính xác |
| FR-19 | Cọc venue 30%, hạn 24h, hủy theo ba mốc | Hủy ≥14d hoàn 80%; 7–<14d 50%; <7d 0% cọc |
| FR-20 | Đối soát sau sự kiện 3–5 ngày làm việc | Tổng gross - refunds - platform fee - adjustments = net, không lặp |
| FR-21 | Thông báo/complaint/audit & báo cáo theo phạm vi | Có trace event/actor/time, không lộ dữ liệu chéo |

## 4. Quy tắc chi tiết

### 4.1. Xét duyệt BTC

Đăng ký tài khoản `nguoidung` → gửi hồ sơ định danh cá nhân/doanh nghiệp → `PENDING` → `quanly` duyệt/từ chối/yêu cầu bổ sung. Duyệt mới có quyền BTC; từ chối được sửa/nộp lại. SLA **7 ngày làm việc** cần lịch ngày lễ, điểm bắt đầu và thời gian tạm dừng khi bổ sung thông tin; admin cấu hình và giám sát SLA.

### 4.2. Sự kiện & versioning

Event chứa nhiều `Session`; mỗi session có khoảng thời gian, venue, zones và quyền bán độc lập. Điều kiện duyệt: nội dung hợp pháp, banner/dữ liệu đầy đủ, địa điểm hợp lệ/đã thuê hoặc giữ lịch theo quy trình phù hợp. Ban tổ chức gửi phiên bản thay đổi `PENDING_UPDATE`, chỉ được publish khi `quanly` duyệt; nếu từ chối, bản public không đổi. `SUSPENDED` khẩn cấp có thể ẩn và dừng bán. Trước khi áp revision thay đổi lịch/venue/seat/giá, đánh giá tác động đơn đã thanh toán, khách mời, refund và thông báo; không được làm mất quyền khách cũ.

### 4.3. Sức chứa & loại vé

Mỗi `session_id + zone_id`: vé bán/đã giữ hợp lệ + khách mời đã phân bổ không vượt capacity. Với seat đánh số, `session_id + seat_id` không bị hai hold/booking cùng hiệu lực. `Standard`, `VIP`, `Invitation`; Invitation được BTC gửi email, không phát sinh doanh thu vé và không cộng hoa hồng. Ngày mở/đóng bán theo loại vé, đóng tự động khi hết chỗ, hết hạn hoặc bắt đầu session.

### 4.4. Giữ chỗ và mua vé

Server áp quota `paid tickets + active holds + requested quantity <= 10` cho **một user/một session**. Ở transaction ngắn: xác thực user/quota, lock seat inventory theo ID ổn định, kiểm tra mọi ghế, tạo hold & booking `expires_at = DB now + 10 minutes`, commit. Không giữ transaction suốt lúc trả PayOS. Nếu cụm ghế có 1 ghế không trống, rollback tất cả, trả danh sách vị trí xung đột theo cách không lộ thông tin khách khác. Worker chạy định kỳ (mục tiêu 10–15 giây) giải phóng hold hết hạn với kiểm tra điều kiện chống nhả nhầm.

### 4.5. PayOS và giao dịch trễ

PayOS QR ngân hàng; tạo link có orderCode, amount, expiry. Chữ ký webhook/response được kiểm tra; đối chiếu số tiền và order, có idempotency ở DB. Khi webhook mất, cron/reconciliation gọi API trạng thái theo giới hạn tốc độ, có retry/backoff; khi giao dịch trễ sau hết hold, trong transaction kiểm tra ghế + quota + capacity và cấp vé nếu còn hợp lệ. Nếu mất chỗ, `PAID_OUT_OF_STOCK`, cảnh báo người dùng và ưu tiên hoàn **100% tổng số tiền đã thực trả**. Không đánh dấu đã hoàn nếu chưa xác minh lệnh chuyển tiền thành công.

### 4.6. Hoàn tiền

- Tự hủy: chỉ khi organizer bật chính sách, **>=72 giờ** trước lúc bắt đầu session, vé chưa check-in. Hoàn 100% **giá vé gốc**, phí dịch vụ không hoàn theo quy định hiển thị khi checkout. Mô hình thu phí cụ thể phải chọn rõ (không đồng thời trừ thêm 5% nếu chưa xác định).
- BTC hoãn: vé cũ còn hiệu lực tại lịch mới; cho khách không tham dự được yêu cầu hoàn 100% tiền thực trả trong khung thời hạn thông báo.
- Event hủy: hoàn 100% **tổng tiền thực trả bao gồm phí**.
- Vé `CHECKED_IN`: không tự hoàn qua nghiệp vụ khách hủy; trường hợp sự kiện bị gián đoạn hoặc có tranh chấp quyền lợi được `quanly/admin` đánh giá thủ công và tuân thủ pháp luật, không bỏ qua khiếu nại.

### 4.7. Check-in

Chỉ `bantochuc` hoặc `quanlysukien` có permission `checkin.scan` tại **đúng session/event**; one-time check-in, timestamp và log người quét/cổng/method. Nếu quét lần hai, trả thông tin đã dùng theo giới hạn PII. Check-in thủ công chỉ cấp `checkin.manual`; tìm theo mã đặt chỗ/phone/email có xác minh phụ và audit.

### 4.8. Địa điểm và cọc

Admin tạo/ẩn/cập nhật venue. `quanly` duyệt booking; giữ lịch **24 giờ** để BTC cọc **30%** tiền thuê. Sau hạn không có tiền đã đối soát thì hủy hold. Hủy thuê theo thời điểm bắt đầu trong hợp đồng: trước >=14 ngày hoàn 80% cọc; 7–<14 ngày hoàn 50%; <7 ngày hoàn 0%. Tách trạng thái duyệt giữ lịch, cọc thành công, xác nhận chính thức, hết hạn và hoàn cọc. Không chấp nhận chồng lịch trong cùng venue/khu độc lập.

### 4.9. Tài chính và đối soát

Tính platform fee theo % theo từng hợp đồng/gói (ví dụ 5–10%, **chưa phải giá chốt toàn nền tảng**), không áp lên Invitation 0đ. Sau event hoàn thành, lập report đối soát trong 3–5 ngày làm việc; tổ chức và `quanly` xác nhận, `admin` duyệt giải ngân. Tiền về tài khoản ngân hàng do nền tảng quản lý là **mô hình nghiệp vụ mong muốn**, cần xác minh quyền thu hộ/chi hộ, sản phẩm PayOS thực tế, phí ngân hàng/thuế và thủ tục hợp đồng trước khi triển khai.

## 5. Bất biến và phi chức năng

- Chống overbooking ở tầng SQL Server với unique/index hoặc một hàng seat inventory/session/seat, transaction, ownership check; index chỉ duy nhất vé `BOOKED` có thể chặn tái bán vé hủy, nên cần chọn cấu trúc có lịch sử.
- Đảm bảo `idempotency` cho webhook, manual reconcile, hoàn tiền và payout.
- Không tin role, event_id hoặc amount do frontend đưa lên; luôn kiểm tra ownership/scope server-side.
- Bảo vệ hồ sơ định danh; không log secret, checksum key hay số tài khoản người mua.
- Audit thay đổi role, phê duyệt, check-in, hoàn/payout.
- Cần backup/migration rollback, test race condition, quan sát worker và sự cố PayOS.
- SLO hiệu năng được xác lập bằng đo tải trên staging; transaction <100ms là mục tiêu không phải bảo đảm tuyệt đối.

## 6. Các câu hỏi bắt buộc xác minh trước production

1. Phí dịch vụ thu trực tiếp người mua hay trừ vào gross organizer; có phụ thu 5% hay không?
2. Cách refund/payout qua ngân hàng, hợp đồng PayOS, tư cách đơn vị thu hộ và xử lý thuế.
3. Biểu đồ ghế với nhiều venue: seat vật lý có dùng lại theo session không; có hỗ trợ vé không ghế không?
4. Người dùng thường được yêu cầu thuê venue độc lập hay chỉ dành cho BTC có sự kiện? (Yêu cầu trước có cả User; code cũ chỉ dành BTC.)
5. Ngày làm việc/holiday calendar cho SLA 7 ngày và payout 3–5 ngày, thời hạn yêu cầu hoàn khi hoãn.
