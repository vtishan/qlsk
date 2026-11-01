# API chính

## Quản lý dữ liệu nghiệp vụ (QuanTriVien)

GET /api/admin/catalog
POST /api/admin/catalog/events
PUT /api/admin/catalog/events/{id}
POST /api/admin/catalog/organizers
PUT /api/admin/catalog/organizers/{id}
PUT /api/admin/catalog/accounts/{id}
GET /api/admin/catalog/accounts/{id}/activity
DELETE /api/admin/catalog/{kind}/{id}

`kind`: events, organizers, ticket-types, venues, event-types. Xóa trả 409 nếu có dữ liệu liên quan; không xóa lịch sử đơn/vé/lịch đặt. Tạo sự kiện cần BanToChucId; sửa sự kiện giữ chủ sở hữu và trạng thái duyệt. Lịch sự kiện đã đặt địa điểm được bảo toàn. Các DTO sự kiện/địa điểm nhận thêm HinhAnh (đường dẫn cục bộ hoặc URL HTTP/HTTPS).

Admin có thể tạo/sửa hạng vé qua POST /api/events/{eventId}/ticket-types và PUT /api/ticket-types/{id}. Backend tiếp tục kiểm tra số vé đã bán/đang giữ. Tạo tài khoản dùng API đăng ký, sau đó quản lý email, mật khẩu, khóa và vai trò bằng API quản trị.

GET /api/admin/dashboard/events/{id}/check-in
GET /api/tickets/{id}/exports (chủ vé hoặc admin)

Thống kê và lịch sử xuất vé truy vấn trực tiếp database, không suy diễn từ số người dự kiến hoặc trạng thái đơn.

## Auth / account
POST /api/auth/register
POST /api/auth/login
POST /api/auth/logout
PUT /api/auth/change-password
GET /api/accounts/me
PUT /api/accounts/me
GET /api/accounts (admin)
POST /api/accounts/{id}/lock (admin)
POST /api/accounts/{id}/unlock (admin)
PUT /api/accounts/{id}/role (admin)

## Xác thực BTC
POST /api/organizer-verifications
GET /api/organizer-verifications/me
GET /api/organizer-verifications
POST /api/organizer-verifications/{id}/approve
POST /api/organizer-verifications/{id}/reject

## Sự kiện
GET /api/events
GET /api/events/{id}
GET /api/events/mine
GET /api/events/manage
POST /api/events
PUT /api/events/{id}
POST /api/events/{id}/submit
POST /api/events/{id}/approve
POST /api/events/{id}/reject
POST /api/events/{id}/cancel

## Địa điểm
GET /api/venues
POST /api/venues
PUT /api/venues/{id}
POST /api/venue-bookings
POST /api/venue-bookings/{id}/approve
POST /api/venue-bookings/{id}/reject
POST /api/venue-bookings/{id}/cancel

## Vé / đơn / thanh toán
GET /api/events/{eventId}/ticket-types
POST /api/events/{eventId}/ticket-types
PUT /api/ticket-types/{id}
POST /api/orders
GET /api/orders/me
GET /api/orders/{id}
POST /api/orders/{id}/cancel
POST /api/payments
POST /api/payments/webhook
POST /api/payments/{id}/refund
GET /api/tickets/me
GET /api/tickets/{id}
GET /api/tickets/by-code/{code}
POST /api/check-in
POST /api/tickets/{id}/export
POST /api/tickets/{id}/email

## Admin
GET /api/admin/dashboard
