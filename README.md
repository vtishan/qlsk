# QLSK - Hệ thống quản lý sự kiện

QLSK là hệ thống quản lý sự kiện được xây dựng bằng **ASP.NET Core Web API** kết hợp với **static frontend HTML/CSS/JavaScript**.

Project được tổ chức theo kiến trúc:

- **Feature-first Modular Monolith**
- **Service Layer**
- Backend và frontend chạy chung trong một ứng dụng ASP.NET Core.
- Frontend được phục vụ trực tiếp từ thư mục `wwwroot`.

---

## 1. Công nghệ sử dụng

### Backend

- ASP.NET Core
- Entity Framework Core
- SQL Server
- JWT Authentication
- Swagger / OpenAPI
- QuestPDF
- SMTP Email
- PayOS
- Background Service
- xUnit / ASP.NET Core Integration Test

### Frontend

- HTML
- CSS
- JavaScript
- Fetch API

---

## 2. Các chức năng đã triển khai

### Phase 1 - Database và Entity Model

- 15 entity nghiệp vụ.
- Enum.
- `DbContext`.
- Primary Key / Foreign Key.
- Index.
- Unique Constraint.
- Check Constraint.
- RowVersion.
- Seed role.
- Migration `InitialCreate`.
- File `schema.sql`.

### Phase 2 - Tài khoản và xác thực

- Đăng ký.
- Đăng nhập.
- Đăng xuất.
- Đổi mật khẩu.
- JWT Authentication.
- Xem thông tin tài khoản hiện tại.
- Khóa / mở khóa tài khoản.
- Phân quyền theo role.
- Kiểm tra ownership.

### Phase 3 - Xác thực ban tổ chức

- Gửi yêu cầu xác thực ban tổ chức.
- Duyệt yêu cầu.
- Từ chối yêu cầu.
- Lưu lịch sử xử lý.
- Tạo `BanToChuc`.
- Cập nhật role sau khi được duyệt.

### Phase 4 - Quản lý sự kiện

- Quản lý loại sự kiện.
- Tạo và cập nhật sự kiện.
- Kiểm tra chủ sở hữu sự kiện.
- Duyệt sự kiện.
- Từ chối sự kiện.
- Hủy sự kiện.
- Ngăn người tạo tự duyệt sự kiện.

### Phase 5 - Địa điểm

- Quản lý địa điểm.
- Đặt địa điểm cho sự kiện.
- Kiểm tra sức chứa.
- Chống trùng lịch.
- Sử dụng `Serializable Transaction` để xử lý concurrency.

### Phase 6 - Vé và đơn vé

- Quản lý loại vé.
- Tạo đơn vé.
- Giữ vé tạm thời.
- Tính tổng tiền tại backend.
- Hủy đơn.
- Tự động hết hạn đơn.
- Hoàn lại số lượng vé đang giữ.
- Background Worker xử lý đơn hết hạn.

### Phase 7 - Thanh toán PayOS

- Tạo payment link.
- Xác minh webhook bằng `HMAC-SHA256`.
- Xử lý idempotency theo `MaGiaoDich`.
- Phát hành vé sau khi thanh toán thành công.
- Ghi nhận giao dịch hoàn tiền.

### Phase 8 - Vé và check-in

- Xem danh sách vé.
- Sinh mã vé.
- Sinh QR Code.
- Check-in vé.
- Mỗi vé chỉ được check-in một lần.

### Phase 9 - Xuất vé

- Sinh QR.
- Xuất PDF bằng QuestPDF.
- Gửi vé qua SMTP Email.
- Lưu lịch sử `XuatVe`.

### Phase 10 - Dashboard

- Thống kê dữ liệu hệ thống.
- Tổng hợp dữ liệu phục vụ dashboard.

### Phase 11 - Frontend

- Giao diện HTML/CSS/JavaScript.
- Gọi API trực tiếp từ frontend.
- Backend và frontend sử dụng chung host.

### Phase 12 - Hoàn thiện hệ thống

- Global error handling.
- Logging.
- Dockerfile.
- Swagger.
- Test project.
- Test plan.

---

## 3. Cấu trúc chạy hệ thống

Frontend được đặt trong:

```text
wwwroot/
```

Trang chủ:

```text
wwwroot/index.html
```

Tiêu đề giao diện:

```text
SựKiện Hub — Khám phá sự kiện
```

ASP.NET Core phục vụ đồng thời:

```text
Frontend
wwwroot/*

API
/api/*
```

Frontend tự gọi API dựa trên địa chỉ hiện tại của ứng dụng, vì vậy không cần cấu hình cứng hostname hoặc port.

Khi publish project, thư mục `wwwroot` cũng được đóng gói cùng backend.

---

## 4. Chạy project

Tại thư mục chứa file:

```text
QLSK.csproj
```

chạy:

```bash
dotnet run
```

Sau đó mở:

```text
http://localhost:5291/
```

API có dạng:

```text
http://localhost:5291/api/...
```

Không cần chạy thêm:

- Live Server.
- Server frontend riêng.
- Node.js server.

Backend và frontend chạy trong cùng một tiến trình ASP.NET Core.

Để dừng ứng dụng:

```text
Ctrl + C
```

---

## 5. Khởi tạo database

Project sử dụng SQL Server.

Trước lần chạy đầu tiên cần cấu hình connection string, sau đó chạy:

```bash
dotnet ef database update
```

---

## 6. Build và Test

Build project:

```bash
dotnet build
```

Chạy test:

```bash
dotnet test tests/QLSK.Tests/QLSK.Tests.csproj
```

Có thể chạy lần lượt:

```bash
dotnet ef database update
dotnet build
dotnet test tests/QLSK.Tests/QLSK.Tests.csproj
```

---

## 7. Migration

Nếu cần tạo lại migration từ Entity Model:

```bash
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

> Không nên xóa hoặc tạo lại migration trên database production nếu chưa kiểm tra ảnh hưởng dữ liệu.

---

## 8. Cấu hình ứng dụng

Cấu hình chung nằm tại:

```text
appsettings.json
```

Các thông tin bí mật khi triển khai production nên được cấp bằng:

- Environment Variables.
- .NET User Secrets.
- Secret Manager của môi trường deploy.

### Các cấu hình chính

```text
ConnectionStrings__Default
Jwt__Key

PayOS__ClientId
PayOS__ApiKey
PayOS__ChecksumKey

Email__User
Email__Password
```

### JWT

`Jwt__Key` phải có độ dài tối thiểu:

```text
32 byte
```

Không nên lưu JWT secret production trực tiếp trong source code.

Khóa JWT trong `appsettings.json` là giá trị mẫu dùng cho phát triển. Khi triển khai, đặt `Jwt__Key` bằng khóa riêng. File `appsettings.Production.json` có thể chứa cấu hình riêng của máy và được Git bỏ qua; ASP.NET Core tự đọc file này khi chạy ở môi trường `Production`.

---

## 9. Development Configuration

File:

```text
appsettings.Development.json
```

chỉ dùng cho môi trường development.

Có thể chứa:

- JWT development key.
- Tài khoản admin mẫu.
- Các cấu hình local.

Ứng dụng chỉ tự động:

- migrate database;
- seed admin;

trong môi trường `Development`.

Production nên chạy migration như một bước deploy riêng.

---

## 10. Tài khoản Admin Development

Tài khoản mặc định trong môi trường development:

```text
Email: admin@qlsk.local
Password: Admin@123456
```

> Phải đổi hoặc xóa tài khoản/mật khẩu mặc định trước khi deploy production.

---

## 11. PayOS

Project hỗ trợ tạo payment link thông qua PayOS.

Luồng cơ bản:

```text
Tạo đơn
   ↓
Tạo payment link
   ↓
Người dùng thanh toán
   ↓
PayOS gửi webhook
   ↓
Backend xác minh HMAC
   ↓
Cập nhật giao dịch
   ↓
Cập nhật đơn
   ↓
Phát hành vé
```

Trạng thái thanh toán cuối cùng được xác định từ **webhook backend**, không dựa hoàn toàn vào việc người dùng quay lại Return URL.

### Return URL

Sau thanh toán, người dùng được chuyển về:

```text
/pages/don-ve/don-cua-toi.html
```

Cancel URL cũng quay về giao diện quản lý đơn vé.

---

## 12. PayOS Mock Mode

Nếu chưa cấu hình PayOS, API tạo thanh toán có thể trả về:

```text
/pages/thanh-toan/mo-phong.html
```

Trang này chỉ phục vụ development/testing.

Mock payment page không được phép giả mạo hoặc thay thế webhook production.

---

## 13. Hoàn tiền

Endpoint hoàn tiền hiện tại dùng để ghi nhận:

> Một giao dịch hoàn tiền đã được thực hiện bên ngoài hệ thống.

Thông tin được lưu bằng:

```text
MaHoanTien
```

Schema hiện tại không có thông tin như:

- tài khoản nhận tiền;
- ngân hàng nhận tiền;
- refund destination;
- payment refund instruction.

Ngoài ra PayOS Payment Link API không cung cấp luồng hoàn tiền tự động chỉ dựa trên giao dịch gốc trong phạm vi hiện tại của project.

Do đó hệ thống không tự tạo thêm logic chuyển tiền ngoài tài liệu nghiệp vụ.

---

## 14. Giới hạn của schema hiện tại

Tài liệu nghiệp vụ yêu cầu role:

```text
QuanLySuKien
```

có thể check-in theo sự kiện được phân công.

Tuy nhiên schema 15 bảng hiện tại không có bảng thể hiện quan hệ:

```text
QuanLySuKien <-> SuKien
```

Ví dụ bảng còn thiếu có thể là:

```text
PhanCongSuKien
```

với quan hệ:

```text
TaiKhoan
   |
   | 1 - N
   |
PhanCongSuKien
   |
   | N - 1
   |
SuKien
```

Vì chưa có bảng phân công nên hiện tại role `QuanLySuKien` có quyền check-in toàn hệ thống.

Nếu muốn phân quyền quản lý theo từng sự kiện thì cần thay đổi schema.

---

## 15. Concurrency

Hệ thống sử dụng transaction và constraint để hạn chế lỗi khi nhiều request chạy đồng thời.

### Đặt địa điểm

Sử dụng:

```text
Serializable Transaction
+
Index lịch
```

nhằm hạn chế hai sự kiện đặt cùng một địa điểm và cùng thời gian.

### Giữ vé

Sử dụng `Serializable Transaction`.

Điều kiện luôn phải đảm bảo:

```text
DaBan + DangGiu <= SoLuong
```

### Hủy và hết hạn đơn

Khi đơn bị:

- hủy;
- hết hạn;

hệ thống thực hiện transaction để trả số vé đang giữ.

### Webhook

Webhook thanh toán sử dụng:

```text
Transaction
+
MaGiaoDich Unique
+
Kiểm tra trạng thái đơn
```

để hạn chế việc xử lý cùng một giao dịch nhiều lần.

### RowVersion

`RowVersion` hỗ trợ phát hiện update conflict.

Tuy nhiên:

```text
RowVersion != Transaction
```

RowVersion không thay thế transaction trong các nghiệp vụ cần đảm bảo tính nguyên tử.

---

## 16. Luồng tổng quát của hệ thống

```text
Người dùng
    |
    v
Đăng ký / Đăng nhập
    |
    v
JWT Authentication
    |
    +--------------------+
    |                    |
    v                    v
Người dùng          Ban tổ chức
    |                    |
    |                    v
    |              Tạo sự kiện
    |                    |
    |                    v
    |               Chờ duyệt
    |                    |
    v                    v
Xem sự kiện <------ Sự kiện được duyệt
    |
    v
Chọn loại vé
    |
    v
Tạo đơn vé
    |
    v
Giữ vé
    |
    v
Thanh toán PayOS
    |
    v
Webhook
    |
    v
Phát hành vé
    |
    v
QR / PDF / Email
    |
    v
Check-in
```

---

## 17. Môi trường Development và Production

### Development

Có thể sử dụng:

- local SQL Server;
- admin seed;
- JWT development key;
- mock payment;
- Swagger.

### Production

Cần:

- secret riêng.
- connection string production.
- JWT key mạnh.
- PayOS thật.
- SMTP thật.
- HTTPS.
- migration riêng trong quy trình deploy.
- không sử dụng tài khoản admin mặc định.
- không commit secret vào Git.

---

## 18. Một số lệnh thường dùng

### Chạy project

```bash
dotnet run
```

### Build

```bash
dotnet build
```

### Test

```bash
dotnet test
```

### Xem migration

```bash
dotnet ef migrations list
```

### Tạo migration

```bash
dotnet ef migrations add TenMigration
```

### Cập nhật database

```bash
dotnet ef database update
```

### Kiểm tra thay đổi model chưa có migration

```bash
dotnet ef migrations has-pending-model-changes
```

---

## 19. Trạng thái project

Project hiện đã có đầy đủ các nhóm chức năng chính:

- Authentication.
- Authorization.
- Quản lý tài khoản.
- Xác thực ban tổ chức.
- Quản lý sự kiện.
- Quản lý địa điểm.
- Quản lý vé.
- Đơn vé.
- Thanh toán.
- Webhook.
- Phát hành vé.
- QR.
- PDF.
- Email.
- Check-in.
- Dashboard.
- Frontend.
- Logging.
- Error Handling.
- Testing.
- Docker.

---

## 20. Ghi chú

Project được thiết kế theo hướng phục vụ đồ án và có thể tiếp tục mở rộng.

Một số chức năng nâng cao có thể bổ sung trong tương lai:

- Phân công quản lý theo từng sự kiện.
- Refresh Token.
- Quên mật khẩu qua email.
- Audit Log.
- Notification.
- Upload ảnh sự kiện.
- Object Storage.
- Rate Limiting.
- Cache.
- CI/CD.
- Monitoring.
- Phân quyền chi tiết theo Permission.
