# 02 · Cấu trúc frontend QLSK

Mở `/#account` để thử màn hình tài khoản, sửa [account.html](../../wwwroot/features/account/account.html), [account.js](../../wwwroot/features/account/account.js) và [account.css](../../wwwroot/features/account/account.css). Router gắn HTML của chức năng vào shell khi điều hướng. Đường dẫn cũ `/views/account.html` chuyển hướng HTTP 301 tới tệp HTML thành phần.

| Vị trí trong wwwroot | Trách nhiệm |
|---|---|
| [index.html](../../wwwroot/index.html) | Shell tại `/`, header, footer, các liên kết CSS và module khởi động |
| [app/app.js](../../wwwroot/app/app.js) và [app/api.js](../../wwwroot/app/api.js) | Điều hướng hash, vòng đời màn hình, cập nhật focus, đọc capability, gửi yêu cầu API |
| [features/](../../wwwroot/features) | HTML, hành vi JS và CSS riêng của từng chức năng |
| [shared/ui/](../../wwwroot/shared/ui) | DOM, form, bảng, sidebar và [bộ nạp template](../../wwwroot/shared/ui/templates.js). Tên chi tiết `event`, `venue`, `export` và `work-overview` được ánh xạ tường minh vào thư mục chức năng |
| [shared/styles/](../../wwwroot/shared/styles) và [shared/date.js](../../wwwroot/shared/date.js) | Kiểu nền, bố cục responsive, component, thẻ danh mục và ngày giờ Việt Nam |
| [assets/images/](../../wwwroot/assets/images) | 14 ảnh WebP dùng chung cho ứng dụng và mẫu |
| [giaodientest/](../../wwwroot/giaodientest) | Mẫu VÉNUE độc lập. Mở `/giaodientest/` hoặc `/giaodientest/giaodienmau.html`. Shell mẫu nạp `css/`, `js/data.js`, các `js/venue-*.js` rồi `js/app.js` |

## Tệp cần mở theo màn hình

Đường dẫn tệp trong mỗi hàng tính từ thư mục chức năng được liên kết. CSS dùng chung trong `shared/styles/` áp dụng cho cả các màn hình có CSS riêng.

| Màn hình trên shell | Thư mục | Tệp |
|---|---|---|
| `#home`, `#home/discover` | [home](../../wwwroot/features/home) | home.html, home.js, home.css |
| `#events`, `#event/{id}/{sessionId?}` | [events](../../wwwroot/features/events) | events.html, event.html, events.js, event-card.js. Kiểu thẻ lấy từ shared/styles/catalog.css |
| `#calendar/{tháng?}/{ngày?}` | [calendar](../../wwwroot/features/calendar) | calendar.html, calendar.js, calendar.css |
| `#venues`, `#venue/{id}?date=YYYY-MM-DD` | [venues](../../wwwroot/features/venues) | venues.html, venue.html, venues.js, venues.css |
| `#guide` | [guide](../../wwwroot/features/guide) | guide.html, guide.js |
| `#account`, đăng nhập, đăng ký, xác minh, mật khẩu, hồ sơ, yêu thích, thông báo | [account](../../wwwroot/features/account) | account.html, account.js, account.css |
| `#tickets/{mine,orders,invitations,book}` | [tickets](../../wwwroot/features/tickets) | tickets.html, tickets.js, tickets.css. Nhánh book chọn suất, ghế và xác nhận |
| `#requests/{rentals,refunds,support}` | [requests](../../wwwroot/features/requests) | requests.html, requests.js |
| `#organizer/{apply,status}` | [organizer](../../wwwroot/features/organizer) | organizer.html, organizer.js, organizer.css |
| `#work/{mục}/{new hoặc id}` | [work](../../wwwroot/features/work) | work.html, work-overview.html, work.js, work.css. work-sections.js khai báo biểu mẫu/lệnh và work-context.js chọn phạm vi sự kiện, suất, mục theo quyền |
| `#export` | [reports](../../wwwroot/features/reports) | export.html, reports.js, reports.css. Dùng cùng phạm vi sự kiện/suất với work |

Khi thêm màn hình, đăng ký module trong router và đặt template cạnh module. Khai báo CSS riêng trong index.html khi chức năng cần stylesheet. Đường dẫn ảnh bên trong template được trình duyệt giải từ shell sau khi gắn DOM. Import ES module và url() trong CSS được giải từ tệp chứa chúng. Quyền hiển thị menu và thao tác đến từ API capability của phiên hiện tại. [API frontend](10_FRONTEND_API_FLOWS.md)

Các bookmark từ cấu trúc cũ được xử lý ở host trước middleware tệp tĩnh. Khi bung ZIP, đặt index.html ngay ở gốc thư mục web root. Bản chạy có tài khoản và dữ liệu cần host ASP.NET Core kết nối CSDL. [Quy tắc chuyển hướng](../../Common/Http/FrontendRedirects.cs) · [Host](../../Program.cs) · [Nghiệm thu trình duyệt](../testcases/README.md)
