# 11 · Backend QLSK theo nhóm nghiệp vụ

API chạy trên ASP.NET Core MVC và EF Core SQL Server. Mỗi nhóm có controller nhận HTTP, service thực hiện quyền, truy vấn và giao dịch, cùng DTO request/response của nhóm. `Program.cs` đăng ký dependency và middleware. Entity và mapping để ở `Domain/` và `Data/`. `Common/` dùng chung cho lỗi HTTP, cookie, CSRF, khóa giao dịch SQL, nhật ký và khởi tạo schema. [Program.cs](../../Program.cs) · [AppDbContext](../../Data/AppDbContext.cs) · [Danh sách API và bảng nguồn](10_FRONTEND_API_FLOWS.md)

| Thư mục | Controller, service và DTO | Phạm vi |
|---|---|---|
| [Capabilities](../../Features/Capabilities) | `CapabilityController`, `CapabilityService`, `CapabilityDtos` | Kiểm kết nối/schema, token CSRF, quyền theo phiên, mục vận hành và loại báo cáo. |
| [Accounts](../../Features/Accounts) | `AccountController`, `AccountService`, `AccountDtos` | Điều khoản, đăng ký, đăng nhập/đăng xuất, hồ sơ, mật khẩu, xác minh email, thông báo, yêu thích. |
| [Catalog](../../Features/Catalog) | `CatalogController` và `CatalogAdminController` với service/DTO tương ứng | Danh mục công khai, lịch, địa điểm, đánh giá; Admin quản lý danh mục, banner, địa điểm, sơ đồ và chính sách. |
| [Organizers](../../Features/Organizers) | `OrganizerController`, `OrganizerService`, `OrganizerDtos` | Nộp multipart, giấy tờ riêng, hàng đợi, yêu cầu bổ sung/từ chối/duyệt, thay đổi quyền BTC. |
| [Events](../../Features/Events) | `EventController`, `EventService`, `ScheduleService`, `EventDtos` | Bản nháp, phiên bản, suất, loại vé, trình/duyệt, công bố hoặc đình chỉ. |
| [Requests](../../Features/Requests) | `RequestController`, `RequestService`, `RefundService`, `RequestDtos` | Thuê, duyệt giữ lịch/cọc, hủy trước giao dịch, hỗ trợ; đọc và duyệt khoản hoàn có chứng cứ thu. |
| [Tickets](../../Features/Tickets) | `TicketController`, `TicketService`, `OrderService`, `TicketDtos` | Tồn ghế, hạn mức, đơn miễn phí hoặc giữ chỗ chờ PayOS, đối soát và phát hành QR/PDF từ vé đã cấp. Dùng renderer QRCoder. [Gói NuGet](https://www.nuget.org/packages/QRCoder/1.9.0) |
| [Admissions](../../Features/Admissions) | `AdmissionController`, `AdmissionService`, `Invitations`, `StaffGrantService`, `AdmissionDtos` | Phân quyền nhân sự và thu hồi; phát hành/thu hồi vé mời, lựa chọn ghế trực tiếp và PDF; tra người tham dự, xác nhận thủ công/QR, lịch sử theo suất. |
| [Operations](../../Features/Operations) | `OperationsController`, `OperationsService`, `OperationsDtos` | Phạm vi theo tài khoản, dashboard, nhóm quyền quản lý, khóa/thu hồi phiên, cấu hình nghiệp vụ, nhật ký. |
| [Finance](../../Features/Finance) | `FinanceController`, `FinanceService`, `FinanceDtos` | Đọc giao dịch, quản lý hợp đồng phí, lập, phản hồi và duyệt kỳ đối soát, đọc tình trạng giải ngân theo phạm vi. |
| [Reports](../../Features/Reports) | `ReportController`, `ReportService`, `ReportDocuments`, `ReportDtos` | Dataset được phân quyền cho đơn, check-in, BTC, thuê, hoàn, đối soát và giải ngân. Excel/Word từ cùng truy vấn, in/lưu PDF từ preview. |

## Chạy với SQL Server

Thiết lập `ConnectionStrings:QLSK` qua môi trường hoặc secret store. Lệnh `--qlsk-init` chạy các migration T-SQL trong `Data/Migrations` và tạo năm danh mục sự kiện ban đầu. Với CSDL QLSK đã có 37 bảng, lệnh kiểm tra tên bảng, nâng cấp các cột cũ và giữ nguyên bản ghi. Phiên bản migration được lưu trong thuộc tính mở rộng `QLSK.SchemaVersion` của SQL Server. [DatabaseSetup](../../Common/Data/DatabaseSetup.cs) · [Migration SQL](../../Data/Migrations/)

### Chạy bộ demo trên Windows

Chạy `dotnet run` để mở bản phát triển tại `http://127.0.0.1:5850`. Profile `Demo` dùng cổng `5849` riêng, nên có thể mở đồng thời hai bản.

Từ thư mục dự án, dùng profile `Demo` với SQL Server LocalDB 2022 trở lên. Profile kết nối tới `(localdb)\MSSQLLocalDB`, cơ sở dữ liệu `quanlysukien`. Trên máy đã cài demo, chuỗi kết nối SQL Authentication cho login `quanlysukien` nằm trong .NET User Secrets của dự án. Trên máy mới, cấu hình Development mặc định dùng Windows Authentication. Có thể thay kết nối bằng `ConnectionStrings__QLSK`.

```powershell
dotnet run -c Release --launch-profile Demo -- --qlsk-seed-demo
dotnet run -c Release --launch-profile Demo
```

Mở `http://127.0.0.1:5849`. Lần nạp đầu hiển thị mật khẩu dùng chung cho bảy tài khoản. Có thể đặt `QLSK__Demo__Password` trước lần nạp đầu để chọn mật khẩu. Mỗi lượt nạp dùng một giao dịch và ghi phiên bản `QLSK.DemoSeedVersion` sau khi hoàn tất. Lệnh chạy lại giữ nguyên mật khẩu, thay đổi của người thử và ngày giờ đã tạo. Để thử lại từ đầu với thời điểm mới, dùng một CSDL mới và chỉ định kết nối qua `ConnectionStrings__QLSK` cùng `ASPNETCORE_ENVIRONMENT=Development`, rồi chạy hai lệnh trên với `--no-launch-profile` thay cho `--launch-profile Demo`. [SQL và dữ liệu thử](../../Data/seed/)

| Email đăng nhập | Dữ liệu để thử |
|---|---|
| `admin@qlsk.test` | Tài khoản, phân quyền, địa điểm và ghế, lịch bảo trì, banner, chính sách, cấu hình và nhật ký. |
| `manager@qlsk.test` | Hàng đợi duyệt hồ sơ, duyệt sự kiện, thuê địa điểm, hỗ trợ, hoàn tiền, đối soát và giải ngân. |
| `organizer@qlsk.test` | Sự kiện nháp và công khai, bản sửa, suất, loại vé, khuyến mãi, đơn hàng, vé mời, nhân sự và báo cáo. |
| `staff@qlsk.test` | Soát QR, tra cứu vé, danh sách khách và lịch sử check-in trong các suất được phân công. |
| `customer@qlsk.test` | Yêu thích, lịch sử xem, thông báo, vé/QR, đơn, thuê, hỗ trợ và hoàn tiền cá nhân. |
| `pending-organizer@qlsk.test` | Hồ sơ BTC đang chờ xét duyệt, tài liệu đính kèm và lịch sử yêu cầu bổ sung. |
| `other-organizer@qlsk.test` | Sự kiện của BTC thứ hai để đối chiếu phạm vi truy cập giữa các đơn vị. |

Sự kiện **Đêm nhạc Sài Gòn – Những thanh âm kết nối** bắt đầu một giờ sau khi nạp để thử check-in. **Hòa nhạc Ánh Sao – Mùa của những ước mơ** diễn ra sau mười ngày để thử chọn ghế, đặt vé và dùng mã `DEMO-WELCOME10` hoặc `DEMO-SAVE50`. Sự kiện **Giai điệu mùa thu** có doanh thu, lịch sử soát vé và các kỳ đối soát. Các trang công khai dùng được khi chưa đăng nhập. Biên nhận thanh toán và hoàn tiền trong bộ demo là dữ liệu mô phỏng. Thao tác gửi email và kết nối PayOS sử dụng cấu hình dịch vụ của môi trường.

### Khởi tạo riêng với quản trị viên tùy chọn

Ví dụ PowerShell với LocalDB. Chạy tại thư mục `C:\IT_01\qlsk`.

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ConnectionStrings__QLSK = 'Server=(localdb)\MSSQLLocalDB;Database=QLSK_Development;Integrated Security=True;TrustServerCertificate=True'
$admin = Get-Credential -Message 'Nhập email và mật khẩu cho Admin QLSK cục bộ'
$env:QLSK__BootstrapAdmin__Email = $admin.UserName
$env:QLSK__BootstrapAdmin__Password = $admin.GetNetworkCredential().Password
dotnet run -- --qlsk-init
Remove-Item Env:QLSK__BootstrapAdmin__Password
Remove-Item Env:QLSK__BootstrapAdmin__Email
dotnet run
```

Bootstrap Admin qua môi trường chạy trong Development khi cơ sở dữ liệu chưa có Admin. Với bản cài chỉ chạy `--qlsk-init`, quản trị viên tạo và công bố `Điều khoản sử dụng`, `Chính sách đặt vé` và `Chính sách thuê địa điểm` trong mục Vận hành. Bộ demo đã có đủ ba chính sách để thử các biểu mẫu ngay.

`QLSK:PrivateFiles` chỉ định kho mã hóa giấy tờ ngoài `wwwroot`. Kho mặc định ở `App_Data/organizer`. Giữ key ring ASP.NET Data Protection của ứng dụng `QLSK` khi di chuyển host hoặc chạy nhiều máy. Cấu hình `QLSK:Smtp:Host`, `From`, `Port` và thông tin xác thực phù hợp để mở xác minh email và khôi phục qua email. Cookie production yêu cầu HTTPS. Khi thiếu cấu hình kết nối, `GET /api/capabilities` trả 503 và shell hiện trạng thái dịch vụ chưa khả dụng.

## Quyền và dữ liệu đã kiểm

- Phân quyền server theo cookie và phiên bản đăng nhập lưu trong `TaiKhoan`. Mọi cập nhật frontend kiểm CSRF, vai trò, trạng thái và phạm vi bản ghi. Admin cấp nhóm quyền nền tảng, BTC cấp từng quyền cổng theo sự kiện hoặc suất. Các đích trực tiếp cũng được kiểm trên controller/service.
- Public catalog chiếu nguồn sự kiện đã công bố. Lịch dùng khoảng nửa mở theo offset giờ Việt Nam. Mã trạng thái UI được ánh xạ từ enum thay vì trả toàn bộ entity. Giấy tờ BTC và chi tiết yêu cầu thuê chỉ mở theo quyền.
- Giao dịch bản ghi nhạy cảm sử dụng khóa SQL Server `sp_getapplock` và ràng buộc của schema. Xác nhận đơn miễn phí cố định theo seat, nhóm/cụm, hạn mức, giá, điều khoản và khóa thử lại. Phát hành QR/PDF từ vé lưu trong `Ve`.
- Kiểm thử trên SQL Server LocalDB 17 trong cơ sở dữ liệu kiểm thử riêng. Dữ liệu thử được tạo bằng API và thông tin tài khoản thử độc lập. [Báo cáo kiểm thử](08_IMPLEMENTATION_REPORT.md)

Để chạy cùng ca kiểm thử trên Windows, dùng Playwright Python với Edge và cấu hình `QLSK_SMOKE_URL`, `QLSK_SMOKE_ADMIN_EMAIL`, `QLSK_SMOKE_ADMIN_PASSWORD` cho host loopback đang nối cơ sở dữ liệu kiểm thử riêng. Đặt `QLSK_SMOKE_DEDICATED_DB=1` khi chạy `python test/qlsk_sql_smoke.py`. Script ghi dữ liệu fixture bằng API vào cơ sở dữ liệu kiểm thử để đối chiếu đọc, ghi và tranh chấp ghế. [Smoke test](../../test/qlsk_sql_smoke.py)

Để thu tiền vé và tiền cọc, cấu hình `QLSK:PayOS:ClientId`, `ApiKey`, `ChecksumKey`, `ReturnUrl` và `CancelUrl` dưới cùng mục PayOS. Hai URL trở về phải dùng HTTPS. Đăng ký callback HTTPS công khai `/api/payments/payos/webhook` với PayOS. Máy chủ kiểm tra chữ ký và đối soát số tiền, mã tham chiếu với nhà cung cấp. Cần nghiệm thu giao dịch thực trên tài khoản merchant của nơi triển khai. Chuyển tiền hoàn và chi trả qua ngân hàng còn phụ thuộc kênh chi thực tế. [GAP](06_GAP_ANALYSIS.md)

Mục cấu hình quản trị hỗ trợ `settlement.closeDays` và `settlement.responseHours`. Nếu chưa có giá trị, hệ thống chờ 3 ngày sau sự kiện và dành 48 giờ cho BTC phản hồi, theo ví dụ BP-16.
