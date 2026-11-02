# Thiết lập máy chủ QLSK

Công cụ này chạy riêng theo yêu cầu của người quản trị. Backend không tự chạy migration hoặc tạo tài khoản khi khởi động. Công cụ không chứa danh mục sự kiện hay dữ liệu kiểm thử.

## Thao tác trực tiếp trong terminal

Mở terminal tại `tools/QLSK.Setup` rồi chạy:

```powershell
dotnet run
```

Chọn **1** để tạo admin mới, **2** để đổi email/mật khẩu admin, **3** để cập nhật schema hoặc **0** để thoát. Công cụ tự đọc connection string từ `appsettings.Development.json` ở thư mục gốc mã nguồn nếu chưa đặt `ConnectionStrings__Default`, và hiển thị máy chủ/database trước khi hỏi thông tin tài khoản.

Nhập email, mật khẩu và xác nhận mật khẩu theo hướng dẫn. Mật khẩu được nhập ẩn, cần ít nhất 12 ký tự và ít nhất ba nhóm chữ hoa, chữ thường, số, ký tự đặc biệt. Không cần đặt `QLSK_ADMIN_*` khi nhập trực tiếp. Có thể chạy thẳng `dotnet run -- create-admin` hoặc `dotnet run -- update-admin` để bỏ qua menu.

`create-admin` chỉ tạo quản trị viên đầu tiên. Nếu đã có admin, chọn **2** để đổi thông tin admin đang có. Việc đổi mật khẩu thu hồi các phiên đăng nhập cũ và giữ nguyên trạng thái khóa. Không có tài khoản seed hoặc mật khẩu mặc định; tài khoản tạo bằng công cụ được lưu vào database với mật khẩu đã băm.

## Chạy tự động hoặc từ gói triển khai

1. Đặt biến môi trường `ConnectionStrings__Default` theo cơ sở dữ liệu của khách hàng, bằng trình quản lý bí mật hoặc cấu hình phiên làm việc. Không sử dụng cơ sở dữ liệu hệ thống của SQL Server.
2. Chạy `dotnet QLSK.Setup.dll migrate` để cập nhật cấu trúc và bốn vai trò hệ thống. Lệnh không tạo sự kiện, địa điểm, ban tổ chức hoặc tài khoản.
3. Đặt thêm `QLSK_ADMIN_EMAIL` và `QLSK_ADMIN_PASSWORD`, rồi chạy `dotnet QLSK.Setup.dll create-admin`. Mật khẩu cần ít nhất 12 ký tự, và ít nhất ba nhóm chữ hoa, chữ thường, số, ký tự đặc biệt.
4. Xóa các biến thông tin quản trị khỏi phiên làm việc sau khi hoàn tất.

`create-admin` giữ nguyên quản trị viên đã có, kể cả tài khoản đã khóa. Email trùng tài khoản thông thường không được nâng quyền. Công cụ không xóa hoặc tạo lại cơ sở dữ liệu.

Khi đầu vào bị chuyển hướng, công cụ không hỏi nhập liệu và không tự đọc cấu hình phát triển: phải cấp biến môi trường cho database và thông tin tài khoản. Chạy không có tham số trong trường hợp này chỉ hiển thị hướng dẫn; `--help` luôn hiển thị hướng dẫn mà không truy cập database.

Không truyền thông tin kết nối hoặc mật khẩu qua tham số lệnh. Công cụ không in mật khẩu, connection string hoặc chi tiết lỗi SQL.

Từ mã nguồn: `dotnet run --project tools/QLSK.Setup -- migrate` hoặc `dotnet run --project tools/QLSK.Setup -- create-admin`. Khi bàn giao, publish riêng công cụ vào thư mục `setup/`.

## Đổi thông tin admin và chuyển đổi dữ liệu cũ

- `update-admin`: yêu cầu `QLSK_ADMIN_ID`, `QLSK_ADMIN_EMAIL`, `QLSK_ADMIN_PASSWORD`. Chỉ cập nhật quản trị viên được chỉ định; băm mật khẩu mới và thu hồi phiên cũ, giữ nguyên trạng thái khóa. Có thể dùng script nhập mật khẩu ẩn `scripts/Set-AdminCredential.ps1 -Operation Update -AdminId <ID>`.
- `adopt-legacy-data`: chạy chủ động sau khi sao lưu database cũ và đặt email/mật khẩu quản trị mới qua biến môi trường. Cập nhật tài khoản admin seed cũ tại chỗ; khóa và thu hồi mật khẩu tài khoản tổ chức cũ, giữ nguyên ID/quan hệ sự kiện. Xóa tài khoản smoke test chưa có lịch sử, lưu trữ tài khoản có lịch sử. Không seed dữ liệu và không chạy trong startup.

Không lưu mật khẩu thật trong README, shell history, cấu hình nguồn hoặc tham số lệnh.
