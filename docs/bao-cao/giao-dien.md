# Chương 2. Phân tích và thiết kế

## 2.1. Thiết kế giao diện

Người mua bắt đầu từ một câu hỏi đơn giản như “Tối đó có suất nào và còn ghế giá phù hợp không?”. Vì vậy, màn hình công khai tổ chức theo sự kiện, lịch và địa điểm. Khi chuyển vào vận hành, đối tượng đầu tiên người dùng cần chọn lại là sự kiện hoặc suất thuộc phạm vi làm việc. Hai cách điều hướng này dùng chung phiên tài khoản và cùng đọc dữ liệu qua API.

### 2.1.1. Bản đồ màn hình

| Khu vực | Trang chính trong bộ định tuyến | Việc người dùng thực hiện |
|---|---|---|
| Khám phá | `#home`, `#events`, `#event/{id}` | Lọc theo nhu cầu, mở chi tiết, xem suất, giá và chuyển sang chọn vé. |
| Tra cứu theo thời gian và không gian | `#calendar`, `#venues`, `#venue/{id}` | Chọn ngày xem suất, tải lịch ICS, tìm địa điểm và xem khung bận trước khi lập yêu cầu thuê. |
| Trợ giúp | `#guide` | Tra câu hỏi thường gặp, quy trình giữ ghế hoặc đường dẫn tới các chức năng liên quan. |
| Tài khoản cá nhân | `#account`, `#tickets`, `#requests`, `#organizer` | Đăng ký/đăng nhập, đọc thông báo, xem vé và đơn, lập yêu cầu, nộp và theo dõi hồ sơ BTC. |
| Vận hành và báo cáo | `#work`, `#export` | Chọn nhiệm vụ, lọc hàng đợi, mở bản ghi và thực hiện quyết định. Xem trước, tải báo cáo hoặc mở hộp thoại in. |

Tổng cộng có 13 trang cấp đầu. Riêng trang Vận hành chứa 30 mục trong năm nhóm gồm sự kiện, tại cổng, xét duyệt, tài chính và quản trị. Menu được dựng theo phiên. Một nhân viên chỉ được giao quyền quét có thể làm việc tại cổng mà không cần thấy hợp đồng, doanh thu hay hồ sơ thuê. [Bộ định tuyến](../../wwwroot/app/app.js) · [Các mục vận hành](../../wwwroot/features/work/work-sections.js) · [Kiểm kê trang chính thức](../testcases/04-kiem-ke-trang-giao-dien.md)

### 2.1.2. Thiết kế ba màn hình trọng tâm

**Chi tiết sự kiện.** Phần đầu trang cho biết người dùng đang xem sự kiện nào. Phần lịch trình liệt kê từng suất với thời điểm, địa điểm và giá vé. Khi chọn mua, đường dẫn phải mang theo mã suất. Cùng một sự kiện có thể có hai giá khác nhau cho hai suất hoặc hai khu vực. Mức “từ” trên thẻ tìm kiếm chỉ dùng để khách quyết định có mở sự kiện hay không. [Giao diện sự kiện](../../wwwroot/features/events/events.js) · [Truy vấn danh mục](../../Features/Catalog/CatalogService.cs)

**Chọn ghế và theo dõi đơn.** Khi mở sơ đồ, ứng dụng đọc khu vực, ghế, loại vé, giá và hạn mức tài khoản từ API tồn kho. Kết quả trả về sau khi gửi đơn quyết định đường đi tiếp theo. Đơn có tiền cần thu có bước thanh toán; đơn tổng bằng 0 có thể đi thẳng tới vé đã phát hành. Nếu hai người cùng chọn chỗ cuối, người bị từ chối cần tải lại sơ đồ. Mục Vé của tôi cho phép mở QR/PDF, còn lịch sử đơn thể hiện số tiền và trạng thái của cả lần mua. [Giao diện vé](../../wwwroot/features/tickets/tickets.js) · [Dữ liệu tồn và đơn](../../Features/Tickets/TicketDtos.cs)

**Hàng đợi xử lý.** Danh sách có bộ lọc trạng thái. Màn hình chi tiết trình bày dữ liệu người nộp và hành động hiện còn được phép. Với yêu cầu thuê, quản lý đọc địa điểm và khung giờ rồi nhập quyết định cùng lý do. API kiểm tra trạng thái kỳ vọng trước khi ghi. Nếu người khác đã xử lý bản ghi, người dùng nhận thông báo và phải mở lại trạng thái mới. [Màn hình vận hành](../../wwwroot/features/work/work.js) · [Xử lý yêu cầu](../../Features/Requests/RequestService.cs)

### 2.1.3. Hành vi dùng chung

Trên màn hình hẹp, bố cục cho phép tiếp tục từ danh mục sang chi tiết và mở thao tác cá nhân. Điều hướng dùng đoạn sau dấu `#` để mở đúng trang và giữ đích quay lại sau đăng nhập. Khi đổi trang, mã giao diện hủy yêu cầu cũ, cập nhật tiêu đề và chuyển điểm lấy nét cho bàn phím. Người dùng có thể bỏ qua thanh điều hướng để đến nội dung chính. Lỗi tải API hiển thị nút thử lại hoặc đường dẫn đăng nhập lại. [Khung ứng dụng](../../wwwroot/index.html) · [Vòng đời màn hình](../../wwwroot/app/app.js)

Việc hiển thị một nút chỉ hỗ trợ người dùng chọn thao tác. Máy chủ tiếp tục kiểm quyền, phạm vi bản ghi, trạng thái hiện tại và dữ liệu gửi. Các biểu mẫu vì vậy có thể giải thích riêng cho ba tình huống thường gặp là nhập sai, bản ghi vừa thay đổi và phiên đã hết hạn. [Bộ gọi API giao diện](../../wwwroot/app/api.js) · [Xử lý lỗi HTTP](../../Common/Http/ApiRequest.cs)
