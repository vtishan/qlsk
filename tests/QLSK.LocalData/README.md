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

## Tạo 100 người dùng và dữ liệu liên quan

```powershell
dotnet run --project tests/QLSK.LocalData -- --populate-users
```

Lệnh này bổ sung dữ liệu mẫu vào database **cục bộ đang cấu hình**, bao gồm `QLSK` khi bạn chủ động chạy lệnh, với điều kiện đã có schema và sự kiện sắp diễn ra có vé trả phí đang mở bán. Đây là thao tác riêng với `--populate` danh mục sự kiện ở trên.

- 100 tài khoản vai trò `NguoiDung`, tên khác nhau, email từ `nguoidung001@sample.qlsk.test` đến `nguoidung100@sample.qlsk.test`; 91 hoạt động và 9 bị khóa.
- 100 đơn vé và 100 chi tiết đơn: 50 thành công, 20 chờ thanh toán, 10 đã hủy, 10 hết hạn, 5 hoàn tiền một phần và 5 hoàn tiền toàn phần.
- 100 bản ghi thanh toán mẫu và 118 vé, bao gồm 9 vé đã hoàn tiền. Mã thanh toán/vé có tiền tố `SAMPLE-`. Không gọi PayOS, gửi email hoặc ghi nhận check-in trước khi sự kiện diễn ra.

Mật khẩu dùng `LOCALDATA_PASSWORD` trong môi trường hoặc `LocalData:UserPassword` trong .NET User Secrets của project chính; cần ít nhất 12 ký tự. Mật khẩu được băm riêng cho từng tài khoản, không có mật khẩu mặc định trong source. Chạy lại bỏ qua các email mẫu đã có, giữ nguyên chỉnh sửa, mật khẩu và lịch sử của chúng. Nếu sửa email nhận diện của tài khoản mẫu, lệnh sẽ coi email gốc là tài khoản còn thiếu.

Đơn và số lượng vé bán/giữ được ghi trong cùng transaction, kiểm tra đủ chỗ trước khi thêm. Chỉ bổ sung người dùng và dữ liệu mua vé; giữ nguyên admin, ban tổ chức, nội dung sự kiện và cấu trúc database. Công cụ tiếp tục từ chối SQL Server từ xa và database hệ thống.

## Tạo lại tệp nội dung và ảnh

```powershell
py -3 tests/QLSK.LocalData/scripts/create-event-catalog.py
py -3 tests/QLSK.LocalData/scripts/generate-event-images.py
```

Lệnh tạo ảnh cần Pillow và chủ động ghi lại bộ ảnh tại `wwwroot/assets/images/events/`. Không cần chạy khi chỉ chuẩn hóa database hiện có.
