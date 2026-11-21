# Bàn giao QLSK ngày 21/11/2026

QLSK chạy bằng ASP.NET Core và SQL Server. Mẫu đối chiếu là `obj/qlsk-publish/wwwroot/giaodientest/giaodienmau.html` cùng CSS nhúng, `venue.css`, các script và ảnh đi kèm. Trang chính sử dụng dữ liệu từ API QLSK. Khi thử nghiệm, mẫu và ứng dụng được mở ở hai tab Edge trên hai cổng loopback riêng.

## Màn hình và thao tác

| Người dùng | Màn hình và thao tác đã kiểm tra |
|---|---|
| Khách | Trang chủ, tìm sự kiện, chi tiết và suất, lịch, địa điểm và lịch trống, hướng dẫn và chính sách. Liên kết đến chức năng cá nhân giữ đích cần quay về sau đăng nhập. |
| Chủ tài khoản | `/#account`, hộp đăng nhập có ảnh nhận diện, hiện/ẩn mật khẩu, đăng ký, xác minh và khôi phục. Đã thử sửa hồ sơ, đổi mật khẩu, lưu sự kiện, chọn ghế, mở lại đơn, QR/PDF, gửi hồ sơ thuê và theo dõi trả lời. |
| Ban tổ chức | Nộp hồ sơ và giấy tờ từ điện thoại, theo dõi xét duyệt, tạo sự kiện và suất, trình công bố, phát và thu hồi vé mời, phân công nhân sự theo suất, xem tài chính và báo cáo trong phạm vi. |
| Nhân viên | Chọn suất được giao, tra người tham dự, check-in và xem nhật ký theo quyền. Đã thử lại cùng vé và thu hồi phân công. |
| Quản lý | Hàng đợi theo quyền, xét hồ sơ, xét sự kiện, giữ lịch/cọc, xử lý hoàn tiền, hỗ trợ, xem và xuất dữ liệu theo phạm vi. |
| Quản trị viên | Tài khoản, phân quyền, danh mục, địa điểm/khu vực, chính sách, tài chính và nhật ký. Báo cáo Excel, Word và bản in dùng dữ liệu từ cùng truy vấn có phân quyền. |

Đã kiểm bố cục desktop 1440 px, tablet 768 px, điện thoại 375–390 px và các trang công khai ở 320 px. Đã mở màn hình mẫu bằng từng vai trò, đối chiếu trang tài khoản, hộp thoại, menu, kết quả tìm, chi tiết, lịch và khu vực vận hành. Nội dung sự kiện, số hàng, tên tài khoản và các nút được cấp phụ thuộc dữ liệu SQL. Giao diện dùng thương hiệu QLSK và vai trò của phiên đăng nhập.

## Lược đồ SQL Server

Đã sao lưu lược đồ gốc `QLSK_KiemTra37_HoanThien_20261120`, khôi phục sang CSDL kiểm thử riêng, áp dụng `--qlsk-init` và đọc catalog SQL Server trước, sau nâng cấp.

| Hạng mục | Trước | Sau |
|---|---:|---:|
| Bảng nghiệp vụ | 37 | 37 |
| Khóa chính | 37 | 37 |
| Ràng buộc UNIQUE | 6 | 6 |
| Khóa ngoại | 75 | 75 |
| Ràng buộc CHECK | 92 | 92 |
| Cột | 339 | 344 |

Đã đối chiếu tên bảng, khóa, từng cặp cột tham chiếu, hành vi cập nhật/xóa và cột trong các chỉ mục. Tất cả chênh lệch trên bản sao nghiệm thu đều là cột nullable.

| Bảng | Trường bổ sung so với lược đồ gốc | Dùng cho |
|---|---|---|
| `TaiKhoan` | `SuKienYeuThich` | Danh sách ID sự kiện. Giá trị NULL của tài khoản cũ tương ứng danh sách trống. |
| `YeuCauThue` | `NgayTao`, `NoiDungYeuCau` | Thời điểm, nội dung hồ sơ mã hóa và nhận diện lần gửi lại. |
| `DonHang` | `KhoaTaoDon`, `DauVanTayTaoDon` | Nhận diện lần gửi lại đơn và đối chiếu nội dung. |

Bốn trường của đơn hàng và hồ sơ thuê đã có trong mã nguồn khi bắt đầu. Trường mới trong lần sửa này là `TaiKhoan.SuKienYeuThich`. Mô hình và script trước đó còn khai báo một bảng yêu thích thứ 38. Mã hiện tại lưu mục yêu thích trên tài khoản và kiểm tra danh sách bảng khi khởi tạo. Bản QA lịch sử 38 bảng được giữ nguyên. Chưa áp dụng thay đổi lên CSDL hoạt động của người dùng.

## Kết quả kiểm tra

- Build thành công. NuGet báo NU1902 và NU1903 cho phụ thuộc gián tiếp `System.Security.Cryptography.Pkcs` 9.0.11.
- `test/RangBuoc.sql` đạt trên bản sao đã nâng cấp. `test/qlsk_sql_smoke.py` chạy với SQL Server LocalDB, ASP.NET và Edge. Đã kiểm quyền ở API, CSRF, thu hồi phiên, xung đột ghế/lịch, QR/PDF, check-in, tải XLSX/DOCX, bản in, Back/Forward và focus sau Escape.
- Đã bấm qua hai luồng khách hàng trên điện thoại. Từ tìm sự kiện đến vé đã phát hành, và từ tìm địa điểm đến hồ sơ đã duyệt chờ cọc. Gửi lại biểu mẫu thuê trả về cùng mã hồ sơ. Khi hết phiên, biểu mẫu giữ giá trị đã nhập. Host thiếu cấu hình SQL hiển thị trạng thái dịch vụ và nút thử lại.
- SMTP STARTTLS thử nghiệm trên loopback đã chuyển thư xác minh và khôi phục. Đã xác nhận mã dùng một lần, thu hồi phiên cũ và đăng nhập lại trong Edge bằng mật khẩu mới.
- Vé mời đã đi từ phát hành đến nhận bằng liên kết, liên kết vào tài khoản có liên hệ đã xác minh và mở QR trong Edge. Đã thử cấp lại liên kết, hết hạn, thu hồi và check-in đồng thời.
- Trong Edge, quản trị viên mở sự kiện đã kết thúc, xác nhận “Hoàn tất sự kiện” và chọn được sự kiện đó khi lập kỳ đối soát. Máy chủ chặn hoàn tất sớm, thay đổi đang chờ xử lý và tài khoản ngoài phạm vi. Gửi lại yêu cầu hoàn tất chỉ tạo một bản ghi nhật ký.
- Đã lập kỳ đối soát từ khoản thu và chứng từ hoàn, xác nhận bằng tài khoản BTC, duyệt bằng quyền tài chính và xuất Excel. Ca kiểm tra có doanh thu vé 100.000 ₫, hoàn 10.000 ₫ và phí nền tảng 10.000 ₫ cho kết quả BTC nhận 80.000 ₫. Hệ thống kiểm tra kỳ chồng lấn và khiếu nại đang mở. Hai mặc định khi chưa cấu hình là chờ 3 ngày sau khi kết thúc sự kiện và dành 48 giờ cho BTC phản hồi. Quản trị viên có thể đổi trong cấu hình.
- Ca thanh toán dùng SQL thật và phản hồi PayOS kiểm thử có chữ ký. Đã kiểm giữ ghế khi hai người đặt cùng lúc, cấp đúng một vé khi nhận đủ tiền, webhook lặp, sai số tiền, nhận tiền sau hạn, yêu cầu hoàn có chứng cứ thu và xác nhận lịch thuê sau cọc. Ca báo cáo có dữ liệu đối soát/giải ngân đã kiểm quyền giữa hai BTC và nội dung tệp xuất.

## Điều kiện triển khai

PayOS thật cần ClientId, ApiKey, ChecksumKey, hai URL trở về HTTPS và callback công khai `/api/payments/payos/webhook`. Chưa thực hiện giao dịch tiền thật hoặc kiểm tra khả năng chuyển thư ra hộp thư ngoài. Việc chuyển tiền hoàn và giải ngân qua ngân hàng cần kênh chi và bằng chứng thực chuyển của đơn vị vận hành.

Mỗi hồ sơ thuê xử lý một địa điểm và khung giờ. QLSK sử dụng dữ liệu và quyền nghiệp vụ của hệ thống. [Cấu hình và cách chạy](11_BACKEND_FEATURES.md)

