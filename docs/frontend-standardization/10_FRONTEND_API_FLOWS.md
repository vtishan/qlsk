# 10 · API và luồng dữ liệu QLSK

Các đường dẫn trong bảng có tiền tố `/api`. Đây là các route ASP.NET Core đã có, trừ những phần được ghi ở mục chờ tích hợp. [Cấu trúc controller, service và DTO](11_BACKEND_FEATURES.md) · [Trình gọi từ frontend](../../wwwroot/app/api.js) · [AppDbContext](../../Data/AppDbContext.cs)

`GET /api/capabilities` trả `version`, `authenticated`, `operations`, `csrfToken`, `workspace.sections`, `verificationChannels` và `reportTypes`. Cookie phiên xác định tài khoản. Các lệnh POST/PUT/DELETE của ứng dụng gửi `X-CSRF-Token` và được kiểm lại quyền/state trong server. Danh sách trả `items` và `nextCursor` nếu hỗ trợ phân trang. Ngày giờ API lưu UTC, biểu mẫu giờ Việt Nam gửi kèm offset `+07:00`. Mã quy trình trong DTO được ánh xạ từ enum của domain.

## Đọc công khai và tài khoản

| HTTP và URL | Dữ liệu gửi, xử lý và kết quả | SQL Server |
|---|---|---|
| GET `/capabilities` | Kiểm schema/phiên → khả năng sẵn sàng, nhóm quyền và CSRF | `TaiKhoan, PhanCongSuKien` |
| GET `/banners`, `/event-categories` | Trả banner và danh mục đang hiển thị | `Banner, DanhMucSuKien` |
| GET `/event-filter-options` | Tập hợp giá vé công khai thành tối đa bốn khoảng như mẫu VÉNUE. Kèm danh mục, thành phố và địa điểm đang hoạt động. Frontend gửi cận giá đã chọn qua `minPrice` và `maxPrice`. | `SuKien, SuatDien, LoaiVe, DanhMucSuKien, DiaDiem` |
| GET `/events`, `/events/{id}` | Từ khóa, danh mục, địa điểm, ngày, giá, tồn, cursor → sự kiện đã công bố, suất, chính sách và giá | `SuKien, PhienBanSuKien, SuatDien, DanhMucSuKien, HoSoBTC, DiaDiem, LoaiVe`; tồn đọc `GiuGhe, Ve` |
| GET `/sessions` | `from, to, city, category, cursor` → suất giao khoảng, tên sự kiện và địa điểm. Lịch truy vấn theo tháng hoặc ngày, browser tạo ICS và URL tới chi tiết suất | `SuatDien, SuKien, PhienBanSuKien, DiaDiem` |
| GET `/venues`, `/venues/{id}`, `/venues/{id}/availability` | Lọc địa chỉ, sức chứa, giá và tiện ích. Chi tiết hiển thị khu vực và khung bận công khai cho bảy ngày, truyền ngày và địa điểm đã chọn vào biểu mẫu thuê | `DiaDiem, KhuVuc, Ghe, LichDatDiaDiem, YeuCauThue` |
| GET `/policies` | Chính sách công bố theo hiệu lực và loại, trả ID dùng để ghi nhận khi đăng ký, đặt vé hoặc thuê | `ChinhSach` |
| GET/POST `/events/{id}/reviews` | Đọc đánh giá công khai, người đã check-in có thể gửi điểm/nhận xét vào hàng chờ | `DanhGia, Ve, ChiTietDonHang, DonHang, SuatDien` |
| POST `/auth/register`, POST/DELETE `/auth/session` | Tên, tài khoản, liên hệ, mật khẩu băm và bản điều khoản → người dùng. Email/SĐT và mật khẩu → cookie. Đăng xuất thu hồi phiên | `TaiKhoan, ChinhSach, NhatKyHeThong` |
| POST `/auth/password-resets`, `/auth/password-resets/confirm` | Yêu cầu gửi mã email, xác nhận bằng mã dùng một lần có hạn; yêu cầu gửi cần SMTP | `TaiKhoan` lưu hash và hạn; SMTP gửi mã |
| GET/PUT `/me`; PUT `/me/password` | Xem/sửa hồ sơ, đổi mật khẩu và phiên bản phiên | `TaiKhoan, NhatKyHeThong` |
| POST `/me/contact-verifications`, `/me/contact-verifications/confirm` | Chọn email, gửi mã qua SMTP, xác minh và cập nhật ngày xác minh | `TaiKhoan` |
| GET `/me/notifications`; PUT `/me/notifications/{id}/read` | Đọc thông báo theo tài khoản, đánh dấu đã đọc | `ThongBao` |
| GET `/me/favorites`; PUT/DELETE `/me/favorites/{eventId}` | Bật/tắt yêu thích, chỉ trả sự kiện còn công khai | `TaiKhoan.SuKienYeuThich, SuKien` |
| GET `/me/view-history`; PUT `/me/view-history/{eventId}` | Hồ sơ liệt kê tối đa 20 sự kiện công khai đã xem, mới nhất trước. Khi tài khoản mở chi tiết, frontend gửi PUT để đưa sự kiện lên đầu danh sách. | `TaiKhoan.SuKienDaXem, SuKien` |

## Hồ sơ, thuê, hỗ trợ, hoàn và vé

| HTTP và URL | Dữ liệu gửi, xử lý và kết quả | SQL Server |
|---|---|---|
| GET/POST `/me/organizer-applications`; GET `/organizer-applications/{id}` | Hồ sơ multipart cá nhân/tổ chức, giấy tờ, liên hệ, thông tin thụ hưởng → hồ sơ chờ duyệt, lịch sử và hạn xử lý | `HoSoBTC, GiayToBTC, TaiKhoan, NhatKyHeThong`; nội dung giấy tờ mã hóa ngoài webroot |
| GET `/organizer-applications`, `/organizer-applications/{id}/documents/{documentId}`; POST `/organizer-applications/{id}/decisions` | Hàng đợi/quá hạn, tải giấy tờ theo quyền, xét duyệt kèm lý do và trạng thái kỳ vọng, cập nhật vai trò khi duyệt | `HoSoBTC, GiayToBTC, TaiKhoan, NhatKyHeThong, ThongBao` |
| GET/POST `/venue-rentals`; GET `/venue-rentals/{id}`; POST `/venue-rentals/{id}/decisions`, `/venue-rentals/{id}/cancellations` | Một địa điểm, thời gian, số khách, điều khoản → yêu cầu, giữ lịch, tiền cọc 30%, hạn 24 giờ, hủy khi chưa phát sinh giao dịch | `YeuCauThue, DiaDiem, LichDatDiaDiem, ChinhSach, GiaoDichThanhToan, NhatKyHeThong` |
| POST `/venue-rentals/{id}/deposit`, `/venue-rentals/{id}/deposit/reconciliation`, `/orders/{id}/checkout`, `/orders/{id}/reconcile`, `/payments/payos/webhook` | Tạo liên kết PayOS, kiểm chứng khoản thu và đối soát lại. Giao dịch cọc cập nhật lịch thuê. Giao dịch vé kiểm tra hạn giữ ghế trước khi phát hành. | `GiaoDichThanhToan, WebhookThanhToan, LichDatDiaDiem, GiuGhe, Ve` |
| GET/POST `/support-cases`; GET/PUT `/support-cases/{id}` | Chủ đề, nội dung và liên kết được sở hữu → mở, phản hồi, giải quyết hoặc đóng | `KhieuNai, TaiKhoan, Ve, YeuCauThue, SuKien, ThongBao, NhatKyHeThong` |
| GET `/me/refundable-tickets`, GET/POST `/refund-requests`, GET `/refund-requests/{id}`; POST `/refund-requests/{id}/decisions` | Tạo yêu cầu từ vé đủ điều kiện, tra cứu và xét duyệt. So số đề nghị với số thực thu còn được hoàn; trạng thái chuyển tiền đọc riêng | `YeuCauHoanTien, GiaoDichThanhToan, GiaoDichHoanTien` |
| GET `/sessions/{id}/inventory` | Giới hạn 10 vé theo tài khoản/suất, khu vực, loại vé, giá, trạng thái ghế, bản điều khoản | `SuatDien, LoaiVe, KhuVuc, Ghe, CumGhe, GiuGhe, Ve, DonHang, ChinhSach` |
| GET/POST `/orders`; GET `/orders/{id}`; POST `/orders/{id}/cancel` | Chọn ghế/cụm, loại vé, voucher, điều khoản, khóa chống lặp. SQL xác nhận đơn miễn phí hoặc giữ ghế 10 phút cho đơn trả phí khi đã cấu hình PayOS. Hủy đơn đang chờ nếu chưa phát sinh giao dịch | `DonHang, ChiTietDonHang, GiuGhe, Ve, GiaoDichThanhToan, Voucher, SuDungVoucher, ThongBao` |
| GET `/me/tickets`, `/tickets/{id}`, `/tickets/{id}/qr`, `/tickets/{id}/pdf` | Trả vé thuộc tài khoản hoặc vé mời đã được liên kết, mã QR/PDF của vé còn hiệu lực | `Ve, LoaiVe, SuatDien, DiaDiem, KhuVuc, Ghe, VeMoi, ChiTietDonHang, DonHang` |

## Vận hành, quản trị và tài chính

| HTTP và URL | Dữ liệu gửi, xử lý và kết quả | SQL Server |
|---|---|---|
| GET `/manage/scopes`, `/admin/dashboard` | Sự kiện/suất/danh mục nhập trong phạm vi đã cấp, nhóm quyền Admin và số lượng hàng đợi | `TaiKhoan, HoSoBTC, PhanCongSuKien, SuKien, SuatDien, NhatKyHeThong` và bảng hàng đợi |
| GET/POST `/manage/events`; GET `/manage/events/{id}`; POST `/manage/events/{id}/suspensions` | Danh sách sự kiện hoặc hàng đợi theo `review=true`; tạo bản nháp của BTC, xem chi tiết, đình chỉ theo quyền kiểm duyệt | `SuKien, HoSoBTC, DanhMucSuKien, PhienBanSuKien, NhatKyHeThong` |
| GET/POST `/manage/events/{id}/versions`; GET `…/versions/{versionId}`; POST `…/versions/{versionId}/submissions`, `…/versions/{versionId}/decisions` | Bản đề xuất có tên/nội dung, trình duyệt, chấp thuận hoặc từ chối theo trạng thái kỳ vọng. Công bố cùng lịch đã kiểm trùng | `SuKien, PhienBanSuKien, SuatDien, LichDatDiaDiem, NhatKyHeThong, ThongBao` |
| GET/POST `/manage/events/{id}/sessions`; GET/PUT `…/sessions/{sessionId}`; GET/POST `/sessions/{sessionId}/ticket-types`; GET/PUT `…/ticket-types/{id}` | Quản lý suất và loại vé của bản nháp, ngày giờ, sức chứa, khu vực và thời gian bán | `PhienBanSuKien, SuatDien, LoaiVe, DiaDiem, KhuVuc` |
| GET/POST `/manage/events/{id}/staff-grants`; GET/DELETE `…/staff-grants/{grantId}` | Email đã xác minh, quyền cổng, phạm vi sự kiện/suất, cấp và thu hồi quyền | `PhanCongSuKien, TaiKhoan, SuKien, SuatDien, NhatKyHeThong` |
| GET/POST `/events/{id}/invitations`; GET `…/invitations/options?sessionId=…`, `…/invitations/{invitationId}`, `…/{invitationId}/pdf`; POST `/invitations/{id}/revoke`, `…/{invitationId}/links`, `/invitations/resolve`, `/invitations/claim` | BTC chủ sự kiện lấy loại vé/ghế lẻ còn trống, phát hành vé mời miễn phí và tải PDF để tự gửi. Kiểm lại ghế giữ, hạn mức loại vé/khu vực/suất dưới khóa giao dịch; liên kết tài khoản có liên hệ đã xác minh. Không thu hồi vé đã check-in. Không tự gửi email. | `VeMoi, Ve, LoaiVe, Ghe, GiuGhe, KhuVuc, SuatDien, TaiKhoan, NhatKyHeThong, ThongBao` |
| POST `/sessions/{id}/attendees/lookup`; GET `/sessions/{id}/attendees`, `…/attendees/{ticketId}`; GET/POST `/sessions/{id}/check-ins`; GET `…/check-ins/{entryId}` | Tra cứu theo mã hoặc liên hệ trong thân request, xác nhận thủ công/QR có quyền riêng, chống quét lặp và trả giờ check-in trước | `Ve, DonHang, VeMoi, PhanCongSuKien, NhatKyCheckIn` |
| GET `/admin/accounts`; GET/PUT `/admin/accounts/{id}` | Liệt kê/tra chi tiết, gán nhóm quản lý cho tài khoản hiện có, khóa/mở, thu hồi phiên, ghi lý do | `TaiKhoan, NhatKyHeThong` |
| GET/POST `/admin/catalog/{resource}`; GET/PUT `/admin/catalog/{resource}/{id}` | Quản trị `venues, zones, venue-blocks, event-categories, banners, policies`; trường và quyền theo nhóm, lịch bảo trì kiểm xung đột | `DiaDiem, KhuVuc, Ghe, LichDatDiaDiem, DanhMucSuKien, Banner, ChinhSach` |
| GET/PUT `/admin/settings`; GET `/admin/monitoring`, `/admin/audit`, `/admin/audit/{id}` | Cấu hình lịch nghỉ/phí dịch vụ theo khóa; số liệu cần kiểm tra và lịch sử có phân quyền | `CauHinhHeThong, WebhookThanhToan, GiuGhe, YeuCauThue, NhatKyHeThong` |
| GET `/payments`, `/payments/{id}` | Giao dịch, nguồn đơn/thuê, số tiền yêu cầu/thực thu, trường đối soát | `GiaoDichThanhToan, DonHang, YeuCauThue` |
| GET/POST `/manage/contracts`; GET `/manage/contracts/{id}` | Công thức phí %, cố định/vé, thời gian hiệu lực, kiểm chồng kỳ và quyền chủ sở hữu | `HopDong, HoSoBTC, NhatKyHeThong` |
| GET/POST `/manage/settlements`, GET `/manage/settlements/eligible-events`, `/manage/settlements/{id}`; POST `…/{id}/responses`, `…/{id}/approval` | Lập kỳ từ khoản thu và chứng từ hoàn, chốt phí theo đơn; BTC xác nhận hoặc mở tranh chấp và quản lý duyệt. Việc duyệt tạo trạng thái sẵn sàng, chưa xác nhận đã chuyển | `DoiSoat, SuKien, HoSoBTC, GiaiNgan, NhatKyHeThong` |
| GET `/manage/payouts`, `/manage/payouts/{id}` | Đọc lệnh đã tồn tại và thời điểm, trạng thái chuyển theo quyền | `GiaiNgan, DoiSoat, SuKien, HoSoBTC` |
| GET `/reports`, `/reports/export` | Loại, phạm vi, trạng thái và ngày → bảng. `/manage/scopes` cấp các lựa chọn sự kiện và suất. Xuất Excel/Word từ cùng tập truy vấn, in/lưu PDF từ bảng đã được cấp quyền đọc | Đơn từ `DonHang, SuatDien`. Check-in từ `SuatDien, Ve, NhatKyCheckIn`. Hồ sơ từ `HoSoBTC`. Thuê từ `YeuCauThue, DiaDiem`. Hoàn tiền từ `YeuCauHoanTien`, gồm khóa tham chiếu giao dịch. |

## Còn chờ tích hợp

Báo cáo `settlements` và `payouts` hiện đọc dữ liệu `DoiSoat, GiaiNgan, SuKien, HoSoBTC`, có lọc trạng thái/ngày/sự kiện. BTC chỉ đọc sự kiện của mình; quyền `finance.review` mở phạm vi tài chính nền tảng. Excel/Word dùng cùng tập dữ liệu và giới hạn 5.000 dòng như bản xem trước. BTC có thể sao chép liên kết nhận vé hoặc tải PDF để gửi cho người nhận. Khi nhận bằng liên kết, hệ thống đối chiếu liên hệ đã xác minh của tài khoản.

Lệnh chuyển tiền hoàn và giải ngân qua ngân hàng cần tích hợp kênh chi. Mỗi hồ sơ thuê hiện xử lý một địa điểm và khung giờ. Các điểm gọi frontend nằm trong module liên quan và bật theo capability. [GAP](06_GAP_ANALYSIS.md) · [Đề xuất hợp đồng mở rộng](07_PROPOSED_API_AND_SCHEMA.md)
