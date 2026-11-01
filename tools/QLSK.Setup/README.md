# Thiết lập máy chủ QLSK

Công cụ này chạy riêng theo yêu cầu của người quản trị. Backend không tự chạy migration hoặc tạo tài khoản khi khởi động. Công cụ không chứa danh mục sự kiện hay dữ liệu kiểm thử.

1. Đặt biến môi trường `ConnectionStrings__Default` theo cơ sở dữ liệu của khách hàng, bằng trình quản lý bí mật hoặc cấu hình phiên làm việc. Không sử dụng cơ sở dữ liệu hệ thống của SQL Server.
2. Chạy `dotnet QLSK.Setup.dll migrate` để cập nhật cấu trúc và bốn vai trò hệ thống. Lệnh không tạo sự kiện, địa điểm, ban tổ chức hoặc tài khoản.
3. Đặt thêm `QLSK_ADMIN_EMAIL` và `QLSK_ADMIN_PASSWORD`, rồi chạy `dotnet QLSK.Setup.dll create-admin`. Mật khẩu cần ít nhất 12 ký tự, và ít nhất ba nhóm chữ hoa, chữ thường, số, ký tự đặc biệt.
4. Xóa các biến thông tin quản trị khỏi phiên làm việc sau khi hoàn tất.

`create-admin` chỉ tạo tài khoản quản trị đầu tiên. Nếu đã có quản trị viên, kể cả tài khoản đã khóa, công cụ giữ nguyên trạng thái và mật khẩu. Email trùng tài khoản thông thường không được nâng quyền. Không có chức năng đặt lại mật khẩu, xóa hoặc tạo lại cơ sở dữ liệu.

Không truyền thông tin kết nối hoặc mật khẩu qua tham số lệnh. Công cụ không in các giá trị này hoặc chi tiết lỗi SQL. Chạy không có tham số chỉ hiển thị hướng dẫn, không truy cập cơ sở dữ liệu.

Từ mã nguồn: `dotnet run --project tools/QLSK.Setup -- migrate` hoặc `dotnet run --project tools/QLSK.Setup -- create-admin`. Khi bàn giao, publish riêng công cụ vào thư mục `setup/`.

## Đổi thông tin admin và chuyển đổi dữ liệu cũ

- `update-admin`: yêu cầu `QLSK_ADMIN_ID`, `QLSK_ADMIN_EMAIL`, `QLSK_ADMIN_PASSWORD`. Chỉ cập nhật quản trị viên được chỉ định; băm mật khẩu mới và thu hồi phiên cũ, giữ nguyên trạng thái khóa. Có thể dùng script nhập mật khẩu ẩn `scripts/Set-AdminCredential.ps1 -Operation Update -AdminId <ID>`.
- `adopt-legacy-data`: chạy chủ động sau khi sao lưu database cũ và đặt email/mật khẩu quản trị mới qua biến môi trường. Cập nhật tài khoản admin seed cũ tại chỗ; khóa và thu hồi mật khẩu tài khoản tổ chức cũ, giữ nguyên ID/quan hệ sự kiện. Xóa tài khoản smoke test chưa có lịch sử, lưu trữ tài khoản có lịch sử. Không seed dữ liệu và không chạy trong startup.

Không lưu mật khẩu thật trong README, shell history, cấu hình nguồn hoặc tham số lệnh.
