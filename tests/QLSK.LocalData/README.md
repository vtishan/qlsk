# Công cụ dữ liệu cục bộ

Dự án này chạy riêng trong khu vực kiểm thử. Backend không tham chiếu hoặc gọi công cụ, danh mục và mã này. Không đưa thư mục `tests` vào gói triển khai.

Danh mục `catalog/events.json` chứa 100 nội dung sử dụng trên máy phát triển. Các tên đơn vị và địa điểm được biên soạn để phục vụ dữ liệu cục bộ; đây không phải danh mục sự kiện hay doanh nghiệp được xác minh từ bên ngoài.

## Chuẩn hóa dữ liệu đang có

Chạy từ thư mục gốc dự án để kiểm tra trước:

```powershell
dotnet run --project tests/QLSK.LocalData -- --normalize-existing --dry-run
dotnet run --project tests/QLSK.LocalData -- --normalize-existing
```

Lệnh chỉ cập nhật các tài khoản, ban tổ chức, địa điểm và phần mô tả nhận diện được từ bộ dữ liệu cũ. Mã bản ghi, khóa ngoại, ảnh, thời gian, giá vé, mật khẩu đã băm và nội dung do người dùng chỉnh sửa được giữ lại. `--dry-run` chỉ đọc và báo số bản ghi dự kiến cập nhật; không thay đổi dữ liệu hoặc đối tượng đang được theo dõi trong DbContext.

## Tạo danh mục trên database cục bộ đã có schema

Sau khi chủ động chạy migration bằng công cụ thiết lập, đặt `LOCALDATA_PASSWORD` trong môi trường của phiên chạy cho các tài khoản ban tổ chức mới rồi chạy:

```powershell
dotnet run --project tests/QLSK.LocalData -- --populate
```

Chỉ được populate vào database riêng tên `QLSK_Test_*`, `QLSK_Dev_*` hoặc `QLSK_Local*`, không dùng database nghiệp vụ `QLSK`. Không có mật khẩu mặc định. Chạy lại giữ dữ liệu đã chỉnh sửa, chỉ bổ sung các sự kiện còn thiếu. Công cụ không tự tạo schema hoặc tài khoản quản trị.

Cấu hình được đọc lần lượt từ `appsettings.json`, `appsettings.Development.json`, `appsettings.Local.json` nếu có, rồi biến môi trường. Chỉ chấp nhận SQL Server trên máy hiện tại và database ứng dụng không thuộc nhóm database hệ thống. Dùng `--root <thư mục dự án>` nếu gọi từ vị trí khác.

## Tạo lại tệp nội dung và ảnh

```powershell
py -3 tests/QLSK.LocalData/scripts/create-event-catalog.py
py -3 tests/QLSK.LocalData/scripts/generate-event-images.py
```

Lệnh tạo ảnh cần Pillow và chủ động ghi lại bộ ảnh tại `wwwroot/assets/images/events/`. Không cần chạy khi chỉ chuẩn hóa database hiện có.
