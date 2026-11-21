# Chương 3. Triển khai và kiểm thử

## 3.1. Kế hoạch và phân công

Bảng sau là phương án phân công cho nhóm bốn người, dùng ký hiệu TV1–TV4 vì dự án chưa cung cấp danh sách thành viên. Mốc tuần thể hiện thứ tự phụ thuộc giữa các đầu việc. Có thể điền họ tên khi nộp báo cáo.

| Thời gian | Phụ trách | Nhiệm vụ và đầu ra | Tiêu chí bàn giao |
|---|---|---|---|
| Tuần 1 | TV1 phân tích, TV2 thiết kế dữ liệu | Chốt tác nhân, quy trình công bố, thuê và bán vé. Vẽ use case và ERD, xác định điều kiện chấp nhận từng luồng. | Truy được từ một yêu cầu đến màn hình, bảng và trường hợp ngoại lệ. |
| Tuần 2 | TV2 phụ trách dữ liệu và tài khoản, TV3 phụ trách giao diện | Tạo migration và seed, kết nối API danh mục, tài khoản, hồ sơ BTC. Dựng trang khám phá, lịch và địa điểm. | CSDL mới khởi tạo được, người thử đăng nhập đúng vai trò, tra được một sự kiện và một địa điểm. |
| Tuần 3 | TV1 phụ trách nghiệp vụ sự kiện, TV2 phụ trách giao dịch | Nối thuê, duyệt, phiên bản, công bố, giá theo suất, đặt đơn, giữ ghế và phát hành vé. TV3 nối biểu mẫu vào API. | Chạy được luồng từ trình duyệt sự kiện đến vé hợp lệ. Hai người chọn cùng chỗ cho cùng suất chỉ có một giao dịch giữ chỗ thành công. |
| Tuần 4 | TV2 phụ trách backend, TV3 phụ trách giao diện vận hành | Phân công tại cổng, vé mời, QR, hoàn tiền, hợp đồng, khóa sổ và xuất báo cáo theo quyền. | Dùng vé đã phát hành để soát một lần, truy được nhật ký. Đối chiếu được một tập dữ liệu trên giao diện và tệp xuất. |
| Tuần 5 | TV4 phụ trách kiểm thử, cả nhóm sửa lỗi theo phân hệ | Chạy ma trận theo vai trò, kiểm tra ràng buộc SQL, lưu bằng chứng và lập danh sách nội dung cần nghiệm thu môi trường thật. | Bàn giao mã nguồn, dữ liệu demo, biên bản có bước tái hiện và báo cáo có minh chứng. |

## 3.2. Hiện thực giao diện và backend

### 3.2.1. Cấu trúc và công nghệ

Dự án dùng ASP.NET Core trên .NET 10, Entity Framework Core SQL Server 10.0.14 và QRCoder 1.9.0. Máy chủ phục vụ tệp tĩnh và API cùng nguồn. Phía trình duyệt dùng HTML, CSS và các module JavaScript trong `wwwroot`. Các controller trả DTO hoặc tệp. Phần trình bày dùng mẫu HTML trong trình duyệt. [Cấu hình dự án](../../QLSK.csproj) · [Khởi động ứng dụng](../../Program.cs)

| Thành phần | Trách nhiệm trong QLSK |
|---|---|
| `wwwroot/app`, `wwwroot/features`, `wwwroot/shared` | Bộ định tuyến, trình gọi API, mẫu màn hình và thành phần dùng chung. Gửi lựa chọn của người dùng, hiển thị dữ liệu/đơn vị/trạng thái do API trả. |
| `Features/Accounts`, `Capabilities`, `Organizers` | Xác thực cookie, tính quyền hiệu lực, cá nhân hóa, nộp và duyệt hồ sơ BTC. |
| `Features/Catalog`, `Events`, `Requests` | Danh mục công khai, địa điểm, lịch, bản nháp và phiên bản sự kiện, yêu cầu thuê, hỗ trợ và hoàn vé. |
| `Features/Tickets`, `Admissions` | Đơn, tồn ghế, thanh toán, QR/PDF, mã khuyến mãi, vé mời, phân công và check-in. |
| `Features/Finance`, `Operations`, `Reports` | Hợp đồng, đối soát, tra cứu giải ngân, quản trị, nhật ký và xuất tệp theo phạm vi. |
| `Domain`, `Data`, `Common` | Entity và enum, ánh xạ và migration SQL, dữ liệu demo, quy tắc HTTP, xác thực phiên và khóa giao dịch. |

### 3.2.2. Từ nút “Đặt vé” đến giao dịch SQL

1. Giao diện tải tồn bằng `GET /api/sessions/{id}/inventory`. DTO trả các khu vực, ghế, loại vé, giá, hạn mức còn lại và mã chính sách. Giả sử người dùng chọn hai ghế. FE gửi lại mã suất, danh sách ghế, loại vé từng ghế, chính sách, voucher tùy chọn và khóa gửi đơn. Mọi thay đổi từ trình duyệt đi kèm token CSRF.
2. `TicketController` đọc tài khoản từ phiên. `OrderService.Place` kiểm lại lựa chọn dựa trên dữ liệu máy chủ. Như vậy một mức giá cũ còn hiển thị trên trang không quyết định số tiền ghi vào đơn.
3. Trong giao dịch SQL, service khóa theo tài khoản/khóa gửi đơn, sự kiện, suất và phạm vi hợp đồng liên quan. Khi hai khách cùng nhấn đặt chỗ, yêu cầu chạy sau đọc được giữ chỗ mà yêu cầu trước vừa ghi. Gửi lại một yêu cầu do mạng chậm trả về cùng mã đơn khi dấu vân tay nội dung trùng khớp.
4. Tổng tiền khách trả bằng tiền vé trừ giảm giá cộng phí dịch vụ. Phí nền tảng theo hợp đồng được chốt riêng để đối soát với BTC. Service tạo `DonHang`, từng `ChiTietDonHang`, `GiuGhe` và bản ghi sử dụng voucher nếu có. Với tổng bằng 0, service cấp vé và ghi giao dịch miễn phí trong cùng giao dịch. Với tổng dương, đơn chuyển sang chờ thanh toán khi payOS đã cấu hình.
5. Khi nhận thông báo thanh toán, backend kiểm chữ ký và tra kết quả từ nhà cung cấp. Nếu hợp lệ và giữ ghế còn hiệu lực, backend chốt ghế, cấp vé và lưu kết quả. Người dùng mở QR/PDF từ vé đã cấp. [Đặt vé và tính tiền](../../Features/Tickets/OrderService.cs) · [Đối soát khoản thu](../../Features/Tickets/OrderPayments.cs) · [Xuất QR/PDF](../../Features/Tickets/TicketService.cs)

Đoạn mã dưới đây là điểm nối thực tế giữa HTTP và nghiệp vụ. `Actor` lấy từ ngữ cảnh đăng nhập. DTO gửi lên chỉ mang thông tin chọn chỗ và chính sách. [TicketController](../../Features/Tickets/TicketController.cs) · [TicketDtos](../../Features/Tickets/TicketDtos.cs)

```csharp
[HttpPost("orders")]
public async Task<IActionResult> PlaceOrder(
    PlaceOrderRequest request, CancellationToken ct) =>
    StatusCode(201, new CreatedDto(await orders.Place(Actor, request, ct)));
```

### 3.2.3. Các quyết định triển khai cần đối chiếu khi vận hành

| Điểm kiểm soát | Cách triển khai và tác động |
|---|---|
| Phiên và quyền | Cookie `qlsk.session` có thời hạn 8 giờ. FE đọc `/api/capabilities` để mở đúng menu và nhận token CSRF. Backend kiểm tài khoản, quyền nền tảng hoặc phân công theo bản ghi ở các hành động tương ứng. |
| Tệp hồ sơ BTC | Dữ liệu tệp được bảo vệ bằng ASP.NET Data Protection ngoài thư mục phục vụ web. Đọc tệp qua controller có kiểm quyền. Việc di chuyển hệ thống phải bảo toàn kho khóa để đọc lại tài liệu. |
| Giao diện xét duyệt | Khi quản lý xét phiên bản lần đầu, `EventService.Decide` kiểm lịch và giá rồi ghi trạng thái đã duyệt. Lệnh `PublishInitial` của BTC kiểm quyền sở hữu, phiên bản, lịch và hạn mức lần nữa rồi mới công bố. |
| Hợp đồng và kỳ đối soát | Ưu tiên hợp đồng hiệu lực riêng cho sự kiện khi chốt đơn. Khi lập kỳ, hệ thống kiểm sự kiện hoàn tất, thời hạn khóa sổ và các kỳ chồng lấn. Các khoản thu còn nghi vấn có thể làm gián đoạn việc lập kỳ. |
| Báo cáo | Có bảy loại dữ liệu theo quyền gồm đơn hàng, check-in, hồ sơ BTC, thuê, hoàn tiền, đối soát và giải ngân. API tạo Excel/Word. Lệnh in hoặc lưu PDF đi qua bản xem trước và chức năng in của trình duyệt. |
| Cấu hình tích hợp | Chuỗi kết nối SQL, bí mật payOS, đường dẫn phản hồi và thông tin SMTP lấy từ cấu hình môi trường. Nếu thiếu cấu hình thu tiền, đơn tổng dương chưa thể mở thanh toán. |

[Cấu hình phiên và middleware](../../Program.cs) · [Xử lý tệp BTC](../../Features/Organizers/OrganizerService.cs) · [Công bố sự kiện](../../Features/Events/EventService.cs) · [Đối soát tài chính](../../Features/Finance/FinanceSettlements.cs) · [Báo cáo](../../Features/Reports/ReportService.cs)
