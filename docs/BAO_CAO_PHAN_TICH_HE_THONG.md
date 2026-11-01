# BÁO CÁO PHÂN TÍCH HỆ THỐNG QUẢN LÝ SỰ KIỆN QLSK

**Mốc đối chiếu:** mã nguồn trong workspace `QLY_SKIEN`, ngày 01/11/2026. **Phạm vi công việc:** đọc, phân tích, truy vết và lập tài liệu; không sửa nghiệp vụ, giao diện hay kiến trúc.

Đã khảo sát 318 tệp nguồn, cấu hình, kiểm thử, công cụ, tài liệu và tài nguyên của project: 109 C#, 35 HTML, 12 JavaScript, 4 CSS, 4 project .NET, 5 JSON, 4 Python, 1 PowerShell, 1 CJS, 1 SQL, 8 Markdown, 4 TXT, 1 DOCX, 5 SVG, 120 JPG và 4 tệp không phần mở rộng. Nội dung nguồn được đọc; tài liệu DOCX được trích xuất văn bản; 120 JPG được giải mã kiểm tra kích thước, 5 SVG được đọc nguồn. Danh mục đầy đủ và dấu vân tay tệp nằm trong [PHU_LUC_KIEM_KE_PROJECT.md](PHU_LUC_KIEM_KE_PROJECT.md).

Các thư mục `.git`, `bin`, `obj`, `artifacts`, `publish`, `node_modules`, `__pycache__` được tách khỏi nguồn phân tích kiến trúc: chứa metadata, kết quả build/publish, dependency hoặc sản phẩm kiểm tra cũ. Chúng được kiểm kê theo phạm vi, không coi mọi DLL, cache hay bản sao trong đó là mã do nhóm triển khai. Báo cáo không khẳng định đã đọc ý nghĩa từng byte của các tệp nhị phân này. Workspace đã có thay đổi từ trước; kết luận dựa trên nội dung hiện có, không chỉ dựa vào commit HEAD.

**Quy ước đọc:** đường dẫn trong dấu `...` là đường dẫn thật tính từ gốc project, trừ nơi ghi rõ **ĐỀ XUẤT — CHƯA TỒN TẠI**. Nhận xét về lỗi giao diện là kết quả truy vết tĩnh nếu không ghi đã chạy thử. Tài liệu cũ thể hiện ý định hoặc kết quả lịch sử; mã hiện tại là căn cứ xác định chức năng tồn tại. Các từ “component”, “state” trong phần frontend chỉ khối DOM/hàm render và biến JavaScript, không hàm ý React.

<!-- toc-start -->
**Mục lục 32 phần**

1. [Tổng quan hệ thống](#muc-1)
2. [Công nghệ sử dụng](#muc-2)
3. [Cấu trúc project](#muc-3)
4. [Kiến trúc hệ thống](#muc-4)
5. [Đặc điểm kiến trúc](#muc-5)
6. [Kiến trúc Frontend](#muc-6)
7. [Kiến trúc Backend](#muc-7)
8. [Kiến trúc Database](#muc-8)
9. [Luồng tổng thể User → Frontend → Backend → Database](#muc-9)
10. [Authentication/Authorization](#muc-10)
11. [Các luồng nghiệp vụ chính](#muc-11)
12. [Truy vết từng chức năng theo file](#muc-12)
13. [Phân tích chi tiết một request hoàn chỉnh](#muc-13)
14. [Background Worker/Job](#muc-14)
15. [Xử lý lỗi](#muc-15)
16. [Bảng API](#muc-16)
17. [Bảng Frontend ↔ Backend](#muc-17)
18. [Bảng truy vết nghiệp vụ](#muc-18)
19. [Dependency giữa các thành phần](#muc-19)
20. [Lợi ích kiến trúc](#muc-20)
21. [Nhược điểm/hạn chế](#muc-21)
22. [Khả năng mở rộng](#muc-22)
23. [Cách thêm một chức năng mới](#muc-23)
24. [Cách thêm một module mới](#muc-24)
25. [Cách sửa một chức năng hiện có](#muc-25)
26. [Khả năng làm việc song song](#muc-26)
27. [Khả năng bảo trì và debug](#muc-27)
28. [Testing](#muc-28)
29. [Đánh giá kiến trúc](#muc-29)
30. [Kết luận](#muc-30)
31. [Nội dung tóm tắt dùng để thuyết trình](#muc-31)
32. [Bộ câu hỏi giảng viên có thể hỏi và câu trả lời](#muc-32)
<!-- toc-end -->

<a id="muc-1"></a>

## 1. Tổng quan hệ thống

QLSK hỗ trợ vòng đời tổ chức sự kiện và bán vé: tài khoản gửi yêu cầu trở thành ban tổ chức; ban tổ chức tạo sự kiện, đặt địa điểm, cấu hình loại vé; người có quyền duyệt xử lý yêu cầu; người mua tạo đơn và thanh toán; hệ thống phát vé, cho phép kiểm tra vào cửa, xuất PDF/gửi email và tổng hợp thống kê.

| Nhóm | Giá trị role trong mã | Khả năng thực tế tiêu biểu |
|---|---|---|
| Khách chưa đăng nhập | Không có JWT | Xem sự kiện công khai, loại sự kiện, địa điểm hoạt động; đăng ký/đăng nhập |
| Người dùng | `NguoiDung`, ID 1 | Quản lý hồ sơ, gửi xác thực tổ chức, tạo đơn, thanh toán đơn của mình, xem/xuất vé của mình |
| Ban tổ chức | `BanToChuc`, ID 2 | Tạo/quản lý sự kiện của tổ chức, đặt địa điểm, quản lý loại vé theo ownership |
| Quản lý sự kiện | `QuanLySuKien`, ID 3 | Duyệt xác thực, duyệt sự kiện, duyệt đặt địa điểm; check-in; ghi nhận hoàn tiền |
| Quản trị viên | `QuanTriVien`, ID 4 | Quản lý tài khoản, danh mục, dashboard và các nghiệp vụ được controller cấp quyền |

Nguồn role: `Data/Configurations/VaiTroConfig.cs`; áp dụng quyền tại các controller trong `Features/`, không dựa vào tên nút trên HTML. Role không có cơ chế kế thừa: admin không mặc nhiên được gọi một endpoint chỉ ghi `BanToChuc`. Hồ sơ `BanToChuc` được xác thực cũng là điều kiện riêng với một số nghiệp vụ, ngoài role.

Frontend và API cùng thuộc ứng dụng ASP.NET Core trong `QLSK.csproj`. `Program.cs` phục vụ `wwwroot` qua `UseDefaultFiles()`/`UseStaticFiles()` và ánh xạ controller bằng `MapControllers()`. Vì vậy backend là API HTTP, nhưng **không có một project/process API riêng với frontend trong cấu hình hiện tại**. Một host phục vụ cả HTML và `/api`; SQL Server là dịch vụ cơ sở dữ liệu bên ngoài tiến trình đó.

Trình duyệt gọi HTTP bằng `fetch` thông qua `wwwroot/js/api.js::apiFetch`. `wwwroot/config.js` lấy `API_BASE` từ `window.location.origin`. API trả JSON; các service dùng Entity Framework Core qua `Data/ApplicationDbContext.cs` để đọc/ghi SQL Server. Trình duyệt không kết nối trực tiếp database.

<a id="muc-2"></a>

## 2. Công nghệ sử dụng

| Công nghệ | Nơi thể hiện | Nhiệm vụ trong project |
|---|---|---|
| HTML đa trang | `wwwroot/index.html`, `wwwroot/pages/**/*.html` | 35 tài liệu HTML, mỗi trang chứa bố cục/form/đích render riêng |
| CSS thuần | `wwwroot/css/base.css`, `app.css`, `content.css`, `responsive.css` | Token/mặc định, bố cục chung, nội dung, điều chỉnh responsive theo cascade |
| JavaScript thuần | `wwwroot/js/` | Hàm global nhận sự kiện, kiểm tra form, gọi API, lưu cache và cập nhật DOM |
| Fetch API, sessionStorage | `wwwroot/js/api.js`, `auth.js` | HTTP/JSON, bearer token, phiên phía trình duyệt, loading/error |
| ASP.NET Core Web API, C#/.NET 10 | `QLSK.csproj`, `Program.cs`, `Features/*/*Controller.cs` | Hosting, routing, model binding, DI, authorization và controller API |
| EF Core SQL Server 10.0.0 | `QLSK.csproj`, `Data/ApplicationDbContext.cs` | LINQ thành SQL, tracking, Include, transaction, optimistic concurrency và migration |
| SQL Server | `UseSqlServer` trong `Program.cs`, cấu hình `ConnectionStrings:Default` | Lưu 15 bảng nghiệp vụ; cưỡng chế FK, unique, check, rowversion |
| JWT Bearer 10.0.0 | `Common/Auth/JwtService.cs`, `Program.cs` | Token HMAC SHA-256, kiểm tra issuer/audience/chữ ký/thời hạn và role |
| ASP.NET Core PasswordHasher | `Features/TaiKhoan/TaiKhoanService.cs`, DI tại `Program.cs` | Hash và kiểm tra mật khẩu; không lưu mật khẩu dạng rõ |
| Swashbuckle 7.2.0 | `Program.cs` | Swagger/OpenAPI và nhập bearer khi môi trường Development |
| PayOS qua HttpClient | `Infrastructure/PayOS/PayOsService.cs` | Tạo payment link, ký request, kiểm tra HMAC webhook; không dùng SDK PayOS riêng |
| QRCoder 1.7.0 | `Infrastructure/QR/QrService.cs` | Sinh ảnh PNG QR từ `Ve.MaQR` |
| QuestPDF 2025.7.4 | `Infrastructure/File/VePdfService.cs` | Tạo bytes PDF vé khổ A5; Community license được cấu hình trong `Program.cs` |
| SMTP | `Infrastructure/Email/EmailService.cs`, `EmailOptions.cs` | Gửi email với PDF đính kèm; cấu hình qua section `Email` |
| PKCS 10.0.14 | `QLSK.csproj` | Dependency mật mã được tham chiếu; không suy diễn thành một tính năng ký tài liệu riêng |
| Hosted BackgroundService | `Infrastructure/ExpiredOrderWorker.cs`, `EventStateWorker.cs` | Tự giải phóng đơn hết hạn và cập nhật trạng thái thời gian của sự kiện |
| xUnit/WebApplicationFactory/EF InMemory | `tests/QLSK.Tests/QLSK.Tests.csproj`, `ApiFactory.cs`, `DeploymentSetupTests.cs` | Kiểm thử quy tắc, API với SQL Server test riêng, và provisioning không cần SQL thật |
| Node test runner | `tests/frontend-data.test.cjs` | Kiểm tra hành vi JavaScript bằng môi trường mô phỏng; không phải browser E2E |
| Python, PowerShell, .NET CLI, Docker | `scripts/`, `tools/QLSK.Setup/`, `Dockerfile` | Version tài nguyên, chuẩn bị ảnh/dữ liệu, tài khoản admin, migration và đóng gói |

Các nhóm cấu hình nằm trong `appsettings.json`, `appsettings.Development.json` và bản Production hiện có trong workspace; có `UserSecretsId` trong `QLSK.csproj`. Báo cáo không sao chép khóa JWT, mật khẩu, connection secret, PayOS/SMTP credentials. Các URL return/cancel của PayOS chỉ điều hướng browser; kết quả thanh toán tin cậy được nhận bằng webhook.

**KHÔNG TÌM THẤY TRONG PROJECT:** React/Vue/Angular, TypeScript app, Redux/context/hook React, SignalR/WebSocket, Redis, message broker, VNPAY/MoMo/ZaloPay integration, service upload ảnh. Không nên đưa các công nghệ đó vào sơ đồ hiện trạng.

<a id="muc-3"></a>

## 3. Cấu trúc project

```text
QLY_SKIEN/
├── Program.cs, QLSK.csproj, appsettings*.json   # một web host
├── wwwroot/                                  # Frontend được host trực tiếp
│   ├── index.html
│   ├── pages/
│   │   ├── auth/, tai-khoan/, to-chuc/, ban-to-chuc/
│   │   ├── su-kien/, dia-diem/, don-ve/, thanh-toan/, ve/
│   │   └── quan-tri/, errors/
│   ├── js/                                   # API chung + script nghiệp vụ
│   ├── css/
│   ├── assets/icons/, assets/images/
│   └── config.js
├── Features/                                 # Backend chia theo tính năng
│   ├── TaiKhoan/, XacThucToChuc/, SuKien/, DiaDiem/
│   ├── LoaiVe/, DonVe/, ThanhToan/, Ve/, XuatVe/
│   └── ThongKe/, QuanTri/
├── Common/
│   ├── Auth/                                 # JWT, claims, active-user filter
│   ├── Exceptions/                           # ApiException + middleware
│   └── Models/                               # response envelope
├── Domain/Entities/, Domain/Enums/            # mô hình dữ liệu và trạng thái
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── Configurations/                       # mapping 15 entity
│   └── schema.sql
├── Migrations/                               # 4 migration + designer/snapshot
├── Infrastructure/
│   ├── PayOS/, Email/, QR/, File/
│   ├── ExpiredOrderWorker.cs
│   └── EventStateWorker.cs
├── tools/QLSK.Setup/                          # CLI triển khai/admin/legacy
├── tests/
│   ├── QLSK.Tests/                           # test .NET
│   ├── QLSK.LocalData/                       # CLI dữ liệu mẫu local
│   ├── frontend-data.test.cjs
│   └── TEST_PLAN.md
├── scripts/                                  # công cụ bảo trì tài nguyên
├── docs/, README.md                           # tài liệu, không phải runtime
└── Dockerfile, .dockerignore, .gitignore, dotnet-tools.json
```

| Folder | Chức năng/chứa gì | Được gọi bởi | Gọi tới |
|---|---|---|---|
| `wwwroot/pages` | HTML, form và vùng DOM hiển thị | Điều hướng/link trình duyệt | Script trong `wwwroot/js`, CSS/assets |
| `wwwroot/js` | Fetch chung, nghiệp vụ, render, permission UI | Inline handler/DOMContentLoaded và các script khác | DOM, sessionStorage, HTTP `/api` |
| `Features` | Controller, DTO, service gần nhau theo 11 nhóm chức năng | Routing ASP.NET Core | Common, Data, Domain; một số Infrastructure |
| `Domain` | 15 entity và 6 enum, navigation/default property | Service, DbContext, configuration, PDF | Các entity/enum; không gọi controller/service |
| `Data` | DbContext, Fluent configuration, schema tham khảo | Service, filter, worker, CLI/test | EF Core SQL Server, Domain |
| `Migrations` | Các thay đổi schema đã ghi nhận | EF migration CLI/setup/test factory | SQL Server; không chạy mỗi request |
| `Common` | Xác thực, lỗi, response dùng chung | `Program.cs`, controller, service | Auth dùng Data/Domain; middleware dùng EF exception |
| `Infrastructure` | PayOS/SMTP/PDF/QR, xử lý nền | Payment/export service và host | HttpClient, SMTP, library; worker gọi service hoặc Data |
| `tools/QLSK.Setup` | Migrate/provision admin/adopt legacy | Người triển khai chạy CLI/script | DbContext, password hasher, migration |
| `tests/QLSK.LocalData` | Catalog 100 sự kiện và tạo/chuẩn hóa dữ liệu local | Chạy CLI chủ động, integration tests | DbContext, local SQL guard, JSON/ảnh |
| `tests/QLSK.Tests` | Fixture và test độc lập ứng dụng chính | `dotnet test` | WebApplicationFactory, SQL test, test doubles |
| `scripts` | Băm URL asset, ảnh địa điểm, thiết lập admin | Người phát triển/triển khai | HTML/assets hoặc Setup CLI |
| `docs` | Thiết kế, API và báo cáo lịch sử | Người đọc | Không tham gia request runtime |

`QLSK.csproj` loại `tests`, `tools`, `scripts`, `artifacts` khỏi phần build ứng dụng tương ứng; tài liệu và seed cũ không được đóng vào content runtime. `Data/Seed/DbSeeder.cs` không còn tồn tại trong workspace. Không được vẽ luồng startup gọi file đã bị xóa này.

<a id="muc-4"></a>

## 4. Kiến trúc hệ thống

Kết luận từ dependency thực tế: **ứng dụng nguyên khối, backend tổ chức feature-first kết hợp Service Layer và các lớp kỹ thuật dùng chung; frontend HTML đa trang với JavaScript thuần**.

Có thể mô tả là monolith chia nhóm chức năng theo hướng module, nhưng ranh giới module mềm: mọi feature ở cùng assembly, dùng chung `ApplicationDbContext`, có quyền truy cập các `DbSet` khác. Không có project/assembly/module contract độc lập để cưỡng chế isolation. Không phải microservices, cũng không phải Clean Architecture nghiêm ngặt.

```text
┌────────────────── Người dùng / Trình duyệt ──────────────────┐
│ HTML pages + CSS                                            │
│ inline onclick/onsubmit → JS nghiệp vụ → biến cache + DOM    │
│                              ↓ apiFetch                     │
└──────────────────────────────┬───────────────────────────────┘
                               │ HTTP, JSON, Bearer JWT
┌────────────── Cùng một ASP.NET Core host ─────────────────────┐
│ StaticFiles: phục vụ wwwroot                                 │
│ API: middleware → auth/role → ActiveUserFilter → controller   │
│                                  ↓ DTO + service            │
│                                  ↓ business rules           │
│               ApplicationDbContext / EF Core                 │
│                    ↓ Entity + Fluent mapping                │
│ Hosted workers ────┘                                        │
│ Payment service → PayOsService → PayOS bên ngoài              │
│ Export service → QR/PDF → SMTP bên ngoài                     │
└──────────────────────────────┬───────────────────────────────┘
                               │ SQL / transaction
                         SQL Server, 15 bảng
```

ASP.NET Core dùng lớp `ControllerBase` và có MVC infrastructure, nhưng project không có Razor view/model-view-controller phía server. Phần View là HTML tĩnh, nhận dữ liệu JSON bằng JavaScript. **Repository Pattern: KHÔNG TÌM THẤY TRONG PROJECT**; ví dụ `Features/DonVe/DonVeService.cs` inject trực tiếp `ApplicationDbContext` và thực hiện truy vấn/cập nhật ngay trong service.

<a id="muc-5"></a>

## 5. Đặc điểm kiến trúc

| Đặc điểm | Bằng chứng | Ý nghĩa |
|---|---|---|
| Chia theo feature ở backend | `Features/DiaDiem/{DiaDiemController.cs,DiaDiemService.cs,Dtos.cs}` | Tìm request/DTO/nghiệp vụ địa điểm trong một nhóm |
| Chia kỹ thuật dùng chung | `Common/`, `Data/`, `Domain/`, `Infrastructure/` | Auth/lỗi/storage/integration không lặp trong mọi feature |
| Controller mỏng | `Features/DonVe/DonVeController.cs::Tao` gọi `service.Tao(User.LayId(), dto)` | Route, quyền, nhận DTO và định dạng response; nghiệp vụ nằm tại service |
| Service vừa nghiệp vụ vừa persistence | `DonVeService.Tao`, `DiaDiemService.Dat` | Validate liên bảng, transaction và LINQ cùng một nơi; dễ theo dõi nhưng phụ thuộc EF |
| Domain chủ yếu là dữ liệu | `Domain/Entities/LoaiVe.cs`, `DonVe.cs` | Trạng thái/quan hệ được biểu diễn; quy tắc giữ/phát vé không đặt trong entity |
| DI tập trung | `Program.cs` | Concrete service scoped; QR/PDF singleton; typed HttpClient; hai hosted worker |
| Dùng chung DbContext | 11 nhóm trong `Features/` | Giao dịch liên bảng tiện, chưa có database/module isolation |
| Response thường trả entity | `SuKienController`, `DonVeController` | Nhanh triển khai nhưng contract API gần schema/navigation EF; `IgnoreCycles` tránh vòng serialize |
| FE chia page nhưng dùng global script | `wwwroot/js/main.js`, `compat-multipage.js` | Không component/module bundler; thứ tự script và ID DOM là dependency thực tế |
| API/FE gắn bằng tên trường, route, role | `api.js`, `Features/*/Dtos.cs` | Không có client sinh tự động từ OpenAPI; sai contract có thể chỉ lộ khi chạy UI |

Không thấy vòng gọi service A → service B → service A. Tuy nhiên không có cơ chế build ngăn dependency vòng trong tương lai. `Common/Auth/ActiveUserFilter.cs` phụ thuộc `Data`, còn worker thuộc `Infrastructure` gọi feature service; vì vậy sơ đồ dependency không phải các tầng “chỉ hướng vào Domain” theo Clean Architecture.

<a id="muc-6"></a>

## 6. Kiến trúc Frontend

### 6.1. Page, khối giao diện, state và render

`wwwroot/pages/` chứa 34 trang, cùng trang chủ `wwwroot/index.html`. Header/footer/menu được chép trong các HTML; **folder component/layout/hook/context/router kiểu framework: KHÔNG TÌM THẤY TRONG PROJECT**. Khối dùng lại tồn tại dưới dạng hàm như `bangHTML`, `badgeTT`, `skeleton`, `thongBao`, `esc` ở `wwwroot/js/api.js`, và các hàm tạo card trong script nghiệp vụ.

```text
Page HTML: form + input ID + div kết quả
  → inline onsubmit/onclick hoặc DOMContentLoaded
  → hàm global trong script nghiệp vụ
  → apiFetch trong api.js
  → Backend
  → Promise response.data
  → biến cache/state hoặc field form
  → innerHTML/textContent/hidden/classList
  → người dùng thấy card, bảng, badge, thông báo hoặc trang mới
```

| Script thật | Trách nhiệm và state đáng chú ý |
|---|---|
| `wwwroot/config.js` | Gốc API cùng origin |
| `wwwroot/js/api.js` | `store` đọc session; `apiFetch`; normalize response; spinner; format/escape/validate/render table |
| `wwwroot/js/auth.js` | `dangKy`, `dangNhap`, `dangXuat`, hồ sơ, mật khẩu, admin tài khoản |
| `wwwroot/js/main.js` | Điều hướng kiểu section cũ, áp dụng quyền/menu, Enter submit, duyệt tổ chức bằng ID |
| `wwwroot/js/compat-multipage.js` | Bọc `hienTrang` sang các URL đa trang, bọc `apDungQuyen`, dùng `body[data-allow]` |
| `wwwroot/js/sukien.js` | Danh sách/chi tiết/tạo/duyệt sự kiện, danh mục loại; `__suKienCache`, `__suKienTrang`, `__suKienLoai`, `__loaiSuKienCache`; helper tìm không dấu `chuoiTimKiem` |
| `wwwroot/js/diadiem.js` | Danh sách/CRUD/đặt địa điểm; `__diaDiemCache`, `__lanTaiDiaDiem` ngăn response cũ ghi đè |
| `wwwroot/js/tochuc.js` | Gửi/xem/duyệt yêu cầu tổ chức và nhóm số liệu |
| `wwwroot/js/donve.js` | Loại vé, tạo/chi tiết/hủy đơn, payment/refund, giao dịch; `__dsDonCache` và filter DOM |
| `wwwroot/js/checkin.js` | Vé theo đơn, tra mã, check-in, xuất/email/lịch sử |
| `wwwroot/js/quantri.js` | Dashboard, hoạt động theo tài khoản, thống kê check-in |
| `wwwroot/js/admin-catalog.js` | CRUD quản trị qua `danhMucQuanTri`, `veQuanTri`, cờ `dangLuuQuanTri` |

`chuoiTimKiem` nằm ở `sukien.js` nhưng `diadiem.js` cũng sử dụng. Đây là tái sử dụng thật và đồng thời là coupling giữa hai nhóm script. Thêm trang cần bảo đảm nạp script đúng thứ tự và có các ID mà handler tìm kiếm. Thêm CSS tuân thủ cascade hiện có; `responsive.css` có thể ghi đè rule trước đó.

### 6.2. API client và phiên

`apiFetch(path, method, body)` tạo URL `/api` khi cần, gắn `Authorization: Bearer ...`, JSON serialize body, đặt `cache: no-store`, timeout 25 giây bằng AbortController và đếm request để điều khiển spinner. `chuanHoaPhanHoi` duyệt response để hỗ trợ alias tên camelCase/PascalCase cho các script cũ. Không có Axios interceptor hay API client riêng cho từng feature; handler tự ghi route rồi gọi wrapper chung.

`sessionStorage` dùng `sk_token`, `sk_vaitro`, `sk_hoten`, `sk_tkid`. Đây là token ở browser, không phải session server. Một đoạn khởi tạo cũ trong `don-cua-toi.html` còn dò một số key localStorage khác; luồng đăng nhập và `store` chính vẫn dùng sessionStorage.

### 6.3. Điều hướng, tìm/lọc/phân trang và trải nghiệm

Link HTML và `location.assign` tạo chuyển trang thực. `compat-multipage.js` ánh xạ một số tên logic như `dangnhap`, `sukien`, `donve`, `quantri` sang URL; chi tiết đọc query string `id`. Không có client router quản lý toàn ứng dụng.

Sự kiện: tải danh sách theo phạm vi public/mine/manage rồi tìm kiếm/lọc loại/phân trang trong `sukien.js`, 12 mục mỗi trang danh sách, trang chủ hiển thị 6. Địa điểm: tải danh sách rồi lọc tên/địa chỉ không dấu trong `diadiem.js`. Tài khoản: có query/paging server nhưng frontend chỉ gọi trang đầu, size 50. Không được suy diễn tất cả danh sách đều phân trang ở SQL.

Loading dùng spinner chung và skeleton; lỗi dùng `thongBao`/toast, một số nơi có retry/empty state. Không có reactive state tự render: sau mutation, code phải tự gọi hàm tải lại hoặc gán DOM. Ảnh được chọn từ `HinhAnh` hoặc placeholder và xử lý `onerror`; chỉ lưu chuỗi đường dẫn/URL, không upload binary.

`scripts/version-frontend-assets.py` băm nội dung CSS/JS để cập nhật query version trong HTML. Đây là cache busting, không phải bundling/minification. Ảnh gồm 100 ảnh sự kiện, 10 ảnh địa điểm gốc và 10 ảnh địa điểm dẫn xuất; `wwwroot/assets/images/venues/SOURCES.md` ghi nguồn. Các asset không thay thế dữ liệu entity.

Thời gian cần được phân biệt theo đường nhập: helper `ngayDuLieu` diễn giải chuỗi ngày không có offset như UTC để hiển thị; admin catalog dùng `toISOString` và giữ milliseconds khi không đổi. Trong khi đó form tạo event, booking và loại vé thông thường gửi trực tiếp chuỗi `datetime-local` không offset. Backend/worker lại so với UtcNow và SQL datetime2 không lưu timezone. Đây là điểm chưa nhất quán cần kiểm thử ở múi giờ Asia/Saigon, không thể khẳng định mọi form đã chuyển giờ địa phương sang UTC đúng.

<a id="muc-7"></a>

## 7. Kiến trúc Backend

| Thành phần | Trách nhiệm | File minh họa và dependency |
|---|---|---|
| Entry/composition root | Cấu hình DI, SQL, JWT, CORS, JSON, pipeline, worker | `Program.cs` phụ thuộc toàn bộ thành phần được đăng ký |
| Controller | Route/verb, `[Authorize]`, nhận DTO, lấy user ID, gọi service, trả HTTP | `Features/DiaDiem/DiaDiemController.cs` → `DiaDiemService` |
| DTO | Contract input + validation đơn trường; một số response DTO tài khoản | `Features/TaiKhoan/Dtos.cs`, `Features/DonVe/Dtos.cs` |
| Service | Quy tắc liên bảng, quyền sở hữu, trạng thái, giá, tồn vé, transaction | `Features/DonVe/DonVeService.cs` → Data/Domain/Common |
| Domain | Entity/navigation và enum | `Domain/Entities/Ve.cs`, `Domain/Enums/TrangThaiVe.cs` |
| Data | DbSet/Fluent mapping/index/FK/check/rowversion | `Data/ApplicationDbContext.cs`, `Data/Configurations/` |
| Filter | Kiểm tra tài khoản/token còn đồng bộ DB | `Common/Auth/ActiveUserFilter.cs` → `ApplicationDbContext` |
| Middleware | Biến exception thành status và envelope lỗi, ghi log | `Common/Exceptions/ApiExceptionMiddleware.cs` |
| Integration service | I/O PayOS/SMTP và tạo QR/PDF | `Infrastructure/PayOS/PayOsService.cs`, `EmailService.cs`, `QrService.cs`, `VePdfService.cs` |
| Worker | Công việc theo thời gian tự động | `Infrastructure/ExpiredOrderWorker.cs` → DonVeService; `EventStateWorker.cs` → DbContext |

11 feature không đồng nghĩa 11 controller. Có nhiều controller chung file: `LoaiSuKienController` nằm trong `Features/SuKien/SuKienController.cs`; `DatDiaDiemController` nằm trong `Features/DiaDiem/DiaDiemController.cs`; `CheckInController` và `CheckInDto` nằm trong `Features/Ve/VeController.cs`. Hiện có **15 lớp controller, 67 action HTTP**. Không tạo đường dẫn file riêng cho các lớp này khi truy vết.

Service được đăng ký concrete scoped, chưa có interface ứng dụng/repository riêng. `TaiKhoanService` dùng `IPasswordHasher<TaiKhoan>` và `JwtService`; `ThanhToanService` dùng `PayOsService`; `XuatVeService` dùng `QrService`, `VePdfService`, `EmailService`. `ApplicationDbContext` scoped dùng chung trong request giúp các thay đổi tracked được SaveChanges trong cùng unit of work. Worker tạo DI scope mới, không giữ DbContext scoped trong singleton hosted service.

Không phải mọi service chỉ động đến bảng cùng tên. `ThanhToanService` đọc đơn, chi tiết, loại vé, payment, vé; `QuanTriService` đọc/ghi nhiều danh mục; `ThongKeService` tổng hợp nhiều bảng. Đây là các điểm liên kết nghiệp vụ cần kiểm thử khi sửa tồn vé hay trạng thái.

<a id="muc-8"></a>

## 8. Kiến trúc Database

### 8.1. Cơ chế truy cập và 15 bảng

`Program.cs` đăng ký `UseSqlServer(ConnectionStrings:Default)`; `Data/ApplicationDbContext.cs` khai báo DbSet và áp dụng configurations. Tên DbSet số nhiều được map về bảng tên đơn. Service viết LINQ; EF dịch phần truy vấn sang SQL có tham số, materialize entity; tracking nhận biết thay đổi và `SaveChangesAsync()` sinh INSERT/UPDATE/DELETE. `AsNoTracking()` dùng ở nhiều API chỉ đọc.

```text
Service → ApplicationDbContext → DbSet<Entity> + Fluent mapping
        → LINQ được EF Core dịch thành SQL → SQL Server
        ← hàng dữ liệu → entity/projection ← service
```

| Entity/bảng | Dữ liệu/trách nhiệm chính | Quan hệ đáng chú ý |
|---|---|---|
| `VaiTro` | ID, tên role, mô tả | 1–N TaiKhoan; seed 4 role |
| `TaiKhoan` | Họ tên, email, hash mật khẩu, active, role, ngày tạo | N–1 VaiTro; 1–N DonVe; tối đa 1 organizer/yêu cầu hiện hành |
| `YeuCauXacThucToChuc` | Tài khoản, tên tổ chức, giấy tờ, trạng thái, xử lý | Unique TaiKhoanId; hồ sơ đang xử lý |
| `LichSuYeuCauXacThucToChuc` | Kết quả duyệt/từ chối, người xử lý, thời điểm, lý do | N–1 tài khoản yêu cầu và người xử lý nullable |
| `BanToChuc` | Tên, mô tả, tài khoản, NgayXacThuc; không có cờ active riêng | Unique TaiKhoanId; 1–N SuKien |
| `LoaiSuKien` | Tên, mô tả, active | 1–N SuKien |
| `SuKien` | Tên/mô tả/ảnh, lịch, số người, trạng thái, BTC, loại | 1–N LoaiVe/DonVe; tối đa 1 DatDiaDiem |
| `DiaDiem` | Tên, địa chỉ, ảnh, sức chứa, giá thuê, active | 1–N DatDiaDiem |
| `DatDiaDiem` | Event/venue, khoảng thuê, tổng tiền/cọc, trạng thái | Unique SuKienId; FK event và venue |
| `LoaiVe` | Event, tên, giá, tổng số lượng, đã bán/đang giữ, cửa sổ bán | 1–N ChiTietDonVe/Ve |
| `DonVe` | Người mua/event, số lượng/tổng tiền, hạn trả tiền, trạng thái | 1–N chi tiết/payment/vé |
| `ChiTietDonVe` | Order/type, số lượng, đơn giá snapshot, thành tiền | Unique (DonVeId,LoaiVeId) |
| `ThanhToan` | Order, tiền, trạng thái, link/reference, tiền/mã hoàn | N–1 DonVe; không có bảng refund riêng |
| `Ve` | Order/type, MaVe/MaQR, trạng thái, NgayCheckIn; không lưu giá riêng | 1–N XuatVe |
| `XuatVe` | Vé, kiểu xuất, địa chỉ, thành công, ngày tạo | N–1 Ve; nhật ký xuất/email |

Mỗi entity có file `Domain/Entities/<Tên>.cs` và mapping `Data/Configurations/<Tên>Config.cs`. Không có bảng Favorite/Review/Voucher/Session trong model hiện tại.

Một số thuộc tính trong `docs/bảng.txt`/`docs/chức_năng.txt` là thiết kế dự kiến, không có trong model: `BanToChuc.HoatDong`, dữ liệu CCCD chi tiết, `DonVe.MaDonHang` và các khoản giảm giá, `ThanhToan.GiaoDichGocID`/phương thức thanh toán, `Ve.GiaThucTe`/họ tên/email người dùng vé/người check-in, `XuatVe.NguoiXuatID`/file URL/số phôi. **KHÔNG TÌM THẤY TRONG PROJECT** các field/luồng lưu trữ này ở model runtime. Đơn dùng ID số; tiền snapshot ở ChiTietDonVe, không nằm trong từng Ve. Frontend `taiVeTheoDon` đang hiển thị giá từ `v.LoaiVe.Gia`, nên không nên gọi đó là giá thực tế đã lưu riêng cho vé.

### 8.2. Quan hệ và bảo vệ dữ liệu

```text
VaiTro 1 ─── N TaiKhoan 1 ─── 0..1 BanToChuc 1 ─── N SuKien N ─── 1 LoaiSuKien
                 │                                  │
                 ├─ 0..1 YeuCauXacThucToChuc         ├─ 0..1 DatDiaDiem N ─ 1 DiaDiem
                 ├─ N LichSuYeuCauXacThucToChuc      ├─ N LoaiVe
                 └─ N DonVe N ──────────────────────┘
                       ├─ N ChiTietDonVe N ── 1 LoaiVe
                       ├─ N ThanhToan
                       └─ N Ve N ── 1 LoaiVe
                             └─ N XuatVe
```

Đa số quan hệ đặt `DeleteBehavior.Restrict`; account → yêu cầu hiện hành và order → chi tiết có Cascade. Vì vậy xóa danh mục đang dùng thường bị từ chối; `QuanTriService.Xoa` còn kiểm tra trước khi EF gửi DELETE. FK bảo đảm tham chiếu tồn tại, không thay thế ownership hoặc quy tắc “loại vé cùng sự kiện với đơn”.

Unique/index quan trọng: email tài khoản; tên vai trò/loại sự kiện; tài khoản của organizer/yêu cầu hiện hành; cặp tên–địa chỉ địa điểm; SuKienId của booking; (SuKienId,Ten) loại vé; (DonVeId,LoaiVeId) chi tiết; MaVe/MaQR; MaGiaoDich và MaLienKet payment có filtered unique khi không NULL. Có index FK và một số index phục vụ trạng thái/thời hạn như `ThanhToan(DonVeId,TrangThai)`, đơn theo trạng thái/hết hạn trong configuration/migration.

Tiền lưu `decimal(18,2)`, thời gian `datetime2`, enum lưu `int`, rowversion SQL Server lưu `byte[]`. Check constraints bảo vệ số lượng dương, tiền không âm, thời gian có thứ tự, `DaBan + DangGiu <= SoLuong`, cọc không vượt tổng và hoàn không vượt tiền gốc. Một số điều kiện vẫn do service bảo vệ: giá VND nguyên, thuộc đúng event, overlap lịch, quyền sở hữu, trạng thái hợp lệ. Database không có check liên bảng cho tổng số loại vé <= sức chứa sự kiện và không có check tương đương mọi rule trong tài liệu cũ; đặc biệt không có check đẳng thức nhân cho `ChiTietDonVe.ThanhTien` như mô tả thiết kế.

### 8.3. Transaction và concurrency

`DonVeService.Tao/Huy/HetHan`, `DiaDiemService.Dat`, các nhánh payment/webhook/refund dùng transaction, những luồng giữ tồn/đặt chỗ dùng Serializable. Quy tắc được đọc và mutate trong phạm vi transaction để hạn chế hai request cùng lấy phần tồn/lịch còn trống. Dù vậy không có cơ sở để khẳng định đã chứng minh mọi trường hợp deadlock/race bằng load test; middleware chưa có chính sách retry deadlock toàn cục.

8 entity có RowVersion: `TaiKhoan`, `SuKien`, `DiaDiem`, `DatDiaDiem`, `LoaiVe`, `DonVe`, `ThanhToan`, `Ve`. EF đưa version gốc vào UPDATE, không khớp sẽ ném `DbUpdateConcurrencyException`, middleware trả 409. DTO không round-trip RowVersion từ trình duyệt; do đó bảo vệ thay đổi đồng thời trong thời gian entity được tracking, không tự phát hiện mọi lần sửa form cũ đã mở từ lâu. RowVersion không thay thế transaction cho bất biến liên bảng.

### 8.4. Migration, seed và dữ liệu local

| Migration thật | Tác dụng |
|---|---|
| `Migrations/20261027130000_InitialCreate.cs` | Khởi tạo bằng raw SQL, bảng/ràng buộc và 4 role |
| `Migrations/20261027150246_AddUniquePaymentLinkIndex.cs` | Unique filtered `MaLienKet` payment |
| `Migrations/20261031152901_AddEventImagePath.cs` | Cột `HinhAnh` sự kiện |
| `Migrations/20261031170820_AddVenueImagePath.cs` | Cột `HinhAnh` địa điểm |

`Migrations/ApplicationDbContextModelSnapshot.cs` cùng các designer mô tả model để EF so sánh thay đổi, không phải lớp xử lý request. `Data/schema.sql` là script SQL song song, không được `Program.cs` thực thi tự động. Có lệch cần lưu ý: script này chưa có unique MaLienKet mà migration đã bổ sung; tên/check trong initial raw SQL và Fluent snapshot không hoàn toàn giống nhau. Không nên xem ba nguồn schema là byte-identical hoặc chạy chồng tùy ý.

Startup hiện tại **không gọi `Database.Migrate` và không seed dữ liệu nghiệp vụ**. `tools/QLSK.Setup/Program.cs` cung cấp tác vụ `migrate`/`create-admin`/`update-admin`/`adopt-legacy-data`, dùng `AdminProvisioner.cs` và `LegacyDataMigrator.cs`. `scripts/Set-AdminCredential.ps1` hỗ trợ CLI này. Seed trong model chỉ là 4 role.

`tests/QLSK.LocalData/` là project riêng: catalog `catalog/events.json` có 100 sự kiện/10 loại, công cụ tạo dữ liệu gồm organizer/venue/ticket type/booking mẫu. `LocalSqlGuard.cs` giới hạn máy local và tên DB test/dev/local; chỉ chạy khi chủ động gọi CLI hoặc test. Dữ liệu mẫu có 5 organizer, 10 venue, 10 category, 100 event, 200 ticket type, 100 booking theo seeder; không phải bằng chứng có dữ liệu kinh doanh thật trong database đang chạy. `LocalEventNormalizer.cs`/test kiểm tra khả năng chuẩn hóa và giữ định danh theo phạm vi. Không chạy công cụ ghi dữ liệu mẫu trong lần phân tích này.

### 8.5. Trạng thái được định nghĩa

Các enum trong `Domain/Enums/` dùng giá trị số từ 0 theo thứ tự:

| Enum | Các giá trị theo thứ tự |
|---|---|
| `TrangThaiSuKien` | ChoDuyet, DaDuyet, TuChoi, DangDienRa, DaHuy, DaKetThuc |
| `TrangThaiDon` | ChoThanhToan, ThanhCong, HetHan, DaHuy, YeuCauHoanTien, DaHoanTien |
| `TrangThaiThanhToan` | DangXuLy, ThanhCong, ThatBai, NghiVan |
| `TrangThaiVe` | ChuaSuDung, DaCheckIn, DaHuy, DaHoanTien |
| `TrangThaiDatCho` | ChoXacNhan, DaXacNhan, TuChoi, DaHuy, HoanThanh |
| `TrangThaiYeuCau` | ChoDuyet, DaDuyet, TuChoi |

Định nghĩa enum không đồng nghĩa đã có API cho mọi chuyển trạng thái. Hoàn thành booking và vé tự hết hạn: **KHÔNG TÌM THẤY TRONG PROJECT**. Đơn hết hạn là nghiệp vụ khác với vé hết hạn.

<a id="muc-9"></a>

## 9. Luồng tổng thể User → Frontend → Backend → Database

```text
Người dùng mở URL HTML trên cùng web host
  ↓
HTML nạp config.js, CSS, api.js, script nghiệp vụ/main/compat
  ↓ thao tác form/nút hoặc DOMContentLoaded
Hàm JS đọc DOM → kiểm tra cơ bản → apiFetch
  ↓ HTTP /api/... + JSON + Bearer (nếu có)
ApiExceptionMiddleware bao quanh xử lý
  ↓ CORS; StaticFiles không xử lý request /api hợp lệ
StatusCodePages → Authentication → Authorization
  ↓ MVC ActiveUserFilter (với user authenticated)
Model binding + DTO validation → action controller
  ↓ lấy user ID/role, gọi service
Service: ownership + trạng thái + quy tắc liên bảng
  ↓
ApplicationDbContext → EF Core → SQL Server
  ↓ query / SaveChanges / commit nếu có transaction
Service trả entity/DTO/projection
  ↓
Controller trả ApiResponse.Ok → JSON camelCase (enum số)
  ↓
apiFetch đọc JSON, normalize tên trường, kiểm tra HTTP status
  ↓
Handler cập nhật biến cache/form/DOM hoặc chuyển trang
  ↓
Người dùng thấy dữ liệu/notification
```

Đây là luồng API thông thường, không phải thứ tự tuyệt đối cho mọi request: middleware authorization có thể dừng trước MVC; webhook không cần JWT và dùng chữ ký PayOS; endpoint PDF trả binary, không đi qua JSON response; static HTML/assets có thể được trả ngay trước auth. Form validation có thể ngăn request ở browser. Worker cập nhật database không đi qua HTTP/controller và frontend chỉ thấy thay đổi khi tải lại.

<a id="muc-10"></a>

## 10. Authentication/Authorization

### 10.1. Đăng nhập và tạo token

`wwwroot/pages/auth/dang-nhap.html` → `wwwroot/js/auth.js::dangNhap` → `apiFetch('/api/auth/login','POST',{Email,MatKhau})` → `Features/TaiKhoan/AuthController.cs::DangNhap` → `TaiKhoanService.DangNhap`.

Service chuẩn hóa email, đọc `TaiKhoan` kèm `VaiTro`, kiểm tra active và `PasswordHasher.VerifyHashedPassword`. `Common/Auth/JwtService.cs::Tao` tạo JWT có ID/name/email/role và claim `pwd` là SHA-256 fingerprint của password hash, ký HMAC SHA-256. `Common/Auth/JwtOptions.cs` quy định issuer/audience/key/thời hạn, mặc định 120 phút. `Program.cs` yêu cầu signing key ít nhất 32 byte và cấu hình kiểm tra issuer, audience, lifetime, signing key với `ClockSkew=0`.

Response `DangNhapResult` chứa Token và `TaiKhoanDto`. `auth.js` lưu token/role/họ tên/ID bằng các key `sk_*`, cập nhật `store`, áp dụng quyền và điều hướng. Mật khẩu hash không nằm trong DTO đăng nhập.

### 10.2. Request sau đăng nhập

```text
sessionStorage.sk_token → apiFetch → Authorization: Bearer token
  → JWT bearer validation trong Program.cs
  → ClaimsPrincipal (ID, role...) → [Authorize]/Roles
  → ActiveUserFilter đối chiếu DB
  → User.LayId() trong Common/Auth/UserClaims.cs
  → service kiểm tra ownership/trạng thái
```

`ActiveUserFilter` được thêm toàn cục vào MVC. Nếu chưa authenticated thì bỏ qua; nếu có JWT hợp lệ thì query account hiện tại: account không tồn tại/khóa → 403 `ACCOUNT_LOCKED`; role hoặc fingerprint mật khẩu khác token → 401 `TOKEN_STALE`. Filter cũng có thể tác động một action anonymous khi request đã mang identity authenticated. Đây là kiểm tra trạng thái DB mỗi request MVC có identity, không phải JWT hoàn toàn không phụ thuộc database.

`[Authorize(Roles=...)]` kiểm soát nhóm quyền ở controller; ownership được kiểm tra trong service: `DonVeService.Lay` chấp nhận người mua, organizer của event hoặc manager; `ThanhToanService.Tao` chỉ người mua; `SuKienService.Sua/Huy` chỉ chủ event; `XuatVeService` PDF/email chỉ chủ vé. Ẩn menu qua `data-allow` trong frontend chỉ cải thiện trải nghiệm; API mới là ranh giới bảo vệ.

### 10.3. Hết phiên, khóa, đổi mật khẩu và logout

`apiFetch` xử lý 401 bằng xóa sessionStorage/phiên và chuyển đăng nhập qua `hienTrang`; lỗi được đưa cho handler hiển thị. Với 403, thông thường giữ session và thông báo từ chối; backend khóa trả 403 nên không mặc nhiên logout ngay như 401. Đổi role/mật khẩu làm token cũ lệch DB và nhận `TOKEN_STALE` trong request tiếp theo.

Wrapper dựa vào `res.ok`/HTTP status, chưa có nhánh độc lập coi `success:false` trong một response HTTP 200 là lỗi. Backend hiện dùng status lỗi tương ứng; khi mở rộng contract cần giữ sự nhất quán này. Alias normalize tạo thêm tên bắt đầu chữ hoa từ key nhận được, chủ yếu giúp script dùng PascalCase với JSON camelCase; không phải lớp mapping DTO được sinh tự động.

`AuthController.DangXuat` trả thông báo hướng client xóa JWT. `auth.js::dangXuat` gọi API logout theo kiểu best effort rồi xóa dữ liệu browser; không có blacklist, refresh token, server session hoặc endpoint refresh. Token bị sao chép trước logout vẫn có hiệu lực đến hết hạn trừ khi account/role/mật khẩu đổi. Xóa phiên phía client không chờ kết quả API logout.

`doiMatKhau` gửi mật khẩu cũ/mới; service xác minh hash cũ, hash lại mật khẩu mới; FE gọi logout sau thành công. Nội dung HTML nói tiếp tục đăng nhập không phản ánh đúng handler. Độ dài password cũng chưa thống nhất: DTO đăng ký/đổi mật khẩu tối thiểu 6, frontend helper/HTML yêu cầu 8. Các luồng không có reset mật khẩu qua email hay xác minh email: **KHÔNG TÌM THẤY TRONG PROJECT**.

<a id="muc-11"></a>

## 11. Các luồng nghiệp vụ chính

Mọi request JSON dưới đây dùng `wwwroot/js/api.js::apiFetch`; pipeline, DTO validation và error handler chung đã mô tả ở mục 9–10 và 15. Các luồng ghi rõ điểm bắt đầu, payload, controller/service, bảng, quyền, response và chỗ cập nhật UI. Không tự thêm một tầng repository hay component framework.

### 11.1. Đăng ký

**UI → JS:** người dùng nhập họ tên/email/mật khẩu/nhập lại trên `wwwroot/pages/auth/dang-ky.html`; form gọi `wwwroot/js/auth.js::dangKy`. Hàm đọc `dk-hoten`, `dk-email`, `dk-mk`, `dk-nhaplai`, kiểm tra đủ dữ liệu và `checkMatKhau`, khóa `btn-dangky-form` trong khi gửi.

**HTTP → backend:** POST `/api/auth/register`, body `{HoTen,Email,MatKhau}` → `Features/TaiKhoan/AuthController.cs::DangKy`, nhận `DangKyDto` trong `Features/TaiKhoan/Dtos.cs` → `Features/TaiKhoan/TaiKhoanService.cs::DangKy`. Endpoint anonymous; DataAnnotations kiểm tra độ dài/email; service trim/lower email, kiểm tra trùng, gán role người dùng và hash mật khẩu.

**DB → UI:** query/INSERT `TaiKhoan`, FK `VaiTro`, unique email là lớp bảo vệ cuối; `SaveChangesAsync`, trả `TaiKhoanDto` trong envelope. `dangKy` không lưu token; thông báo thành công và `hienTrang('dangnhap')` chuyển trang. Trùng email/validation đi vào catch → thông báo; finally mở nút.

### 11.2. Đăng nhập, đăng xuất, đổi mật khẩu

**Đăng nhập:** `wwwroot/pages/auth/dang-nhap.html` → `auth.js::dangNhap` → POST `/api/auth/login` với `DangNhapDto {Email,MatKhau}` → `AuthController.DangNhap` → `TaiKhoanService.DangNhap` → SELECT `TaiKhoan`/`VaiTro`, kiểm tra hash/active → `JwtService.Tao` → `DangNhapResult` → sessionStorage `sk_*` → điều hướng theo `next` nội bộ hợp lệ hoặc role. Sai mật khẩu/khóa không tạo token.

**Đăng xuất:** nút chung trong các HTML → `auth.js::dangXuat` → POST `/api/auth/logout` → `AuthController.DangXuat` chỉ trả thông báo, không ghi DB → client xóa session, áp dụng lại quyền, về đăng nhập. Không có service logout hay bảng session bị xóa.

**Đổi mật khẩu:** `wwwroot/pages/tai-khoan/doi-mat-khau.html` → `auth.js::doiMatKhau` → PUT `/api/auth/change-password`, `DoiMatKhauDto {MatKhauCu,MatKhauMoi}` → `AuthController.DoiMatKhau` → `TaiKhoanService.DoiMatKhau(UserId, dto)` → SELECT/UPDATE `TaiKhoan.MatKhau` sau xác minh hash cũ → JSON message → thông báo và `dangXuat`. JWT/ActiveUserFilter bảo vệ; fingerprint khiến token cũ stale.

### 11.3. Xem/cập nhật hồ sơ

`wwwroot/pages/tai-khoan/thong-tin.html` có form và `btn-luu-hoso`. DOMContentLoaded → `wwwroot/js/auth.js::taiHoSoCaNhan` → GET `/api/accounts/me` → `Features/TaiKhoan/TaiKhoanController.cs::Me` → `TaiKhoanService.Lay(UserId)` → SELECT tài khoản/role → `TaiKhoanDto` → điền `qtk-id`, `qtk-hoten`, `qtk-email`, cập nhật họ tên session/menu.

Submit → `capNhatTaiKhoan` → PUT cùng endpoint, `SuaTaiKhoanDto {HoTen}` → controller `Sua` → service `Sua` → UPDATE họ tên của user từ JWT → DTO → `sk_hoten` và `apDungQuyen`, thông báo đã lưu. Không nhận ID tùy ý trong body. Cập nhật số điện thoại/email ở hồ sơ cá nhân: **KHÔNG TÌM THẤY TRONG PROJECT**; admin có đường sửa email khác.

### 11.4. Quản lý tài khoản

`wwwroot/pages/quan-tri/tai-khoan.html` → `wwwroot/js/auth.js::taiTaiKhoan` lấy `qtk-tukhoa` → GET `/api/accounts?page=1&size=50&q=...` → `TaiKhoanController.DanhSach` nhận query `q,vaiTroId,hoatDong,page,size` → `TaiKhoanService.DanhSach` xây LINQ lọc họ tên/email, role/active khi được truyền, Order/Skip/Take, clamp size 1–100 → `TaiKhoan`/`VaiTro` → list DTO → `bangHTML` tại `ds-taikhoan`.

Nút khóa/mở/đổi role → `khoaTaiKhoan`, `moKhoaTaiKhoan`, `doiVaiTroTaiKhoan` → POST `/api/accounts/{id}/lock`, POST `.../unlock`, PUT `.../role` với `DoiVaiTroDto {VaiTroId}` → controller `Khoa/MoKhoa/DoiVaiTro` → service `Khoa/DoiVaiTro` UPDATE `TaiKhoan` → message → tải lại bảng. Tất cả yêu cầu admin. Đổi role chỉ đổi VaiTroId, không tự tạo hồ sơ BanToChuc. Chưa có guard ngăn tự khóa hoặc khóa admin cuối; paging/role/status API có nhưng UI không gửi đầy đủ các filter và không có điều hướng trang tiếp.

### 11.5. Gửi và xem yêu cầu xác thực tổ chức

`wwwroot/pages/to-chuc/gui-xac-thuc.html` → `wwwroot/js/tochuc.js::guiYeuCau` đọc `tc-ten`, `tc-giayto`, kiểm tra URL http/https, khóa nút → POST `/api/organizer-verifications`, `GuiYeuCauDto {TenToChuc,GiayTo}` → `Features/XacThucToChuc/XacThucController.cs::Gui` → `Features/XacThucToChuc/XacThucService.cs::Gui(UserId,dto)`.

Service yêu cầu không có hồ sơ chờ và không đã được xác thực; INSERT `YeuCauXacThucToChuc` gắn account. Unique account bảo vệ một hồ sơ hiện hành. Trả entity/envelope → chuyển `wwwroot/pages/to-chuc/yeu-cau-cua-toi.html`. Trang này gọi `taiYeuCauCuaToi` → GET `.../me` → controller `Me` → service `CuaToi` query theo user → entity/null → card trong `ds-yeucau`; null hiện hướng dẫn không có hồ sơ chờ và đăng nhập lại nếu đã duyệt.

`capNhatYeuCau` trong script thực tế vẫn POST hồ sơ mới; API PUT sửa yêu cầu: **KHÔNG TÌM THẤY TRONG PROJECT**. DTO/model không có CCCD/ngày sinh/ảnh hai mặt như tài liệu dự kiến; không có upload binary hoặc API xem lịch sử xác thực.

### 11.6. Duyệt/từ chối xác thực

`wwwroot/pages/quan-tri/duyet-to-chuc.html` khởi tạo `wwwroot/js/main.js::renderDuyetToChuc` và `tochuc.js::taiThongKeToChuc` → GET `/api/organizer-verifications` → `XacThucController.DanhSach` → `XacThucService.DanhSach` SELECT yêu cầu kèm `TaiKhoan`, sắp ngày gửi → số lượng/phần nhập ID xử lý. Hàm render hiện chỉ cho số đếm và thao tác bằng ID, không render bảng hồ sơ đầy đủ/link giấy tờ.

`duyetToChucNhanh`/`tuChoiToChucNhanh` trong `main.js`, hoặc `duyetYeuCauToChuc`/`tuChoiYeuCauToChuc` trong `tochuc.js`, gọi POST `/{id}/approve` hoặc `/{id}/reject`, từ chối gửi `TuChoiYeuCauDto {LyDo}` → controller `Duyet/TuChoi` → service cùng tên. Quyền manager/admin; ID người xử lý lấy từ JWT. Approve kiểm tra ChoDuyet; reject hiện không có kiểm tra trạng thái riêng trước khi lưu lịch sử/xóa, dù luồng bình thường chỉ giữ hồ sơ chờ ở bảng hiện hành.

Trong transaction, approve thêm `LichSuYeuCauXacThucToChuc`, tạo `BanToChuc`, đổi `TaiKhoan.VaiTroId=2`, xóa yêu cầu hiện hành; reject ghi lịch sử/lý do rồi xóa yêu cầu. Commit → message → thông báo/tải lại số liệu. User cần đăng nhập lại do role JWT cũ lệch DB. Trường tên tổ chức nhập khi duyệt nhanh không được truyền vào approve; tên dùng từ yêu cầu gốc.

### 11.7. Xem, tìm, lọc, phân trang và chi tiết sự kiện

`wwwroot/index.html`, `wwwroot/pages/su-kien/danh-sach.html` → `wwwroot/js/sukien.js::timSuKienTrangChu/taiDanhSachSuKien/locLoaiSuKien/chuyenTrangSuKien`. Phạm vi public gọi GET `/api/events`; trang BTC gọi `/api/events/mine`; trang duyệt gọi `/api/events/manage`.

`Features/SuKien/SuKienController.cs::Get/Mine/Manage` → `Features/SuKien/SuKienService.cs::DanhSach/CuaToi` → LINQ trên `SuKien`. DanhSach public/manage Include `LoaiSuKien`, `BanToChuc`; CuaToi tìm organizer ID rồi lọc event, không Include các navigation đó. Public chỉ DaDuyet/DangDienRa/DaKetThuc; mine yêu cầu BTC và lọc chủ sở hữu, manage yêu cầu manager/admin. Backend trả list, không có DTO query tìm kiếm/phân trang sự kiện.

Response vào `__suKienCache`; `veDanhSachSuKien` lọc keyword/loại và cắt trang rồi tạo card trong `ds-sukien`, cập nhật page/count/category UI. Refresh chủ động tải mới; cache không phải kho đồng bộ server. Từ card mở chi tiết bằng ID → `xemChiTietSuKien` → GET `/api/events/{id}` → controller `Lay` → service `Lay` kiểm tra trạng thái công khai hoặc owner/manager → event kèm quan hệ → render mô tả/ảnh/lịch và gọi tải loại vé trong `donve.js`. Error/empty/skeleton xử lý ở JS.

Chi tiết public có include tài khoản tổ chức; `TaiKhoan.MatKhau` có JsonIgnore nhưng trả entity vẫn có thể đưa ra các trường tài khoản khác nhiều hơn nhu cầu card. `IgnoreCycles` giải quyết vòng tham chiếu, không tự tối thiểu hóa dữ liệu.

### 11.8. Tạo, sửa, gửi duyệt, duyệt, từ chối và hủy sự kiện

**Tạo:** `wwwroot/pages/ban-to-chuc/tao-su-kien.html` → `wwwroot/js/sukien.js::taoSuKien` kiểm tra form/lịch/số người → POST `/api/events`, body tên/mô tả/lịch/số người/loại → `SuKienController.Tao`, `TaoSuKienDto` → `SuKienService.Tao`. Role BTC; `LayBanToChucId` tìm hồ sơ organizer theo user và yêu cầu hồ sơ tồn tại, kiểm tra loại active và thời gian; INSERT `SuKien` trạng thái ChoDuyet → entity → toast/chuyển hoặc tải lại danh sách theo handler. Account active do filter kiểm tra, còn entity BanToChuc không có cờ active/xác thực riêng ngoài NgayXacThuc. DTO hỗ trợ HinhAnh nhưng handler tạo thông thường không gửi trường ảnh.

**Sửa/gửi duyệt qua API:** PUT `/api/events/{id}` nhận `TaoSuKienDto` → `SuKienController.Sua` → `SuKienService.Sua`: owner, chỉ chờ duyệt/từ chối, kiểm tra loại/lịch rồi UPDATE. POST `/{id}/submit` → `GuiDuyet`, đặt lại ChoDuyet theo điều kiện service. **Handler/form frontend chuyên sửa và gửi duyệt sự kiện: KHÔNG TÌM THẤY TRONG PROJECT**. Không gán nút giả cho API này; admin có luồng sửa catalog riêng ở 11.21.

**Duyệt/từ chối:** `wwwroot/pages/quan-tri/duyet-su-kien.html` → `sukien.js::duyetSuKien/tuChoiSuKien` → POST `.../approve` hoặc `.../reject` (`TuChoiSuKienDto {LyDo}`) → `SuKienController.Duyet/TuChoi` → `SuKienService.Duyet/TuChoi`. Manager/admin, không tự duyệt event của mình, chỉ trạng thái phù hợp; UPDATE `SuKien.TrangThai`, reject nối lý do vào `MoTa`. JSON message → refresh cache/danh sách. Không có cột audit người/thời gian duyệt sự kiện như phần thiết kế cũ.

**Hủy:** `wwwroot/pages/ban-to-chuc/su-kien-cua-toi.html`/card chức năng → `huySuKien` → POST `/api/events/{id}/cancel` → controller `Huy` → service `Huy` kiểm tra BTC owner rồi đặt DaHuy → Save → thông báo/tải lại. Không có transaction phối hợp hủy tất cả đơn/booking/vé hoặc tự hoàn tiền. Không nên mô tả hủy sự kiện là đã xử lý xong các giao dịch liên quan.

### 11.9. Quản lý loại sự kiện

`wwwroot/pages/quan-tri/loai-su-kien.html` và các select loại trên trang tạo/lọc sự kiện → `wwwroot/js/sukien.js::taiLoaiSuKien`. Admin dùng GET `/api/event-types/all`, còn public dùng GET `/api/event-types` → **`LoaiSuKienController` trong `Features/SuKien/SuKienController.cs`**, action `Get/All` → `SuKienService.LayLoai(all)` → SELECT loại theo active/all → `__loaiSuKienCache`, render select/chips/bảng.

`themLoaiSuKien/capNhatLoaiSuKien/doiTrangThaiLoai` POST hoặc PUT `/api/event-types[/{id}]`, `LoaiSuKienDto {Ten,MoTa,HoatDong}` → controller `Tao/Sua` → `SuKienService.TaoLoai/SuaLoai` → INSERT/UPDATE `LoaiSuKien`; role admin, DataAnnotations và unique tên → entity → thông báo/reload. `xoaLoaiSuKien` gọi catalog DELETE, chỉ được xóa khi chưa dùng. UI cho manager vào trang không làm manager được phép POST/PUT admin.

### 11.10. Tìm/xem và quản lý địa điểm

**Xem/tìm:** `wwwroot/pages/dia-diem/danh-sach.html` → `wwwroot/js/diadiem.js::taiDiaDiem` → GET `/api/venues` hoặc admin `/all` → `Features/DiaDiem/DiaDiemController.cs::Get/All` → `DiaDiemService.DanhSach` → SELECT `DiaDiem` hoạt động/all, OrderBy Ten → list → `__diaDiemCache`, lọc tên/địa chỉ tại browser rồi render card/count. Mục 12 truy vết chi tiết, không có search SQL theo từ khóa.

**CRUD:** `wwwroot/pages/quan-tri/dia-diem.html` → `themDiaDiem`, `suaDiaDiem` điền form từ cache, `capNhatDiaDiem`, `doiTrangThaiDiaDiem` → POST `/api/venues` hoặc PUT `/api/venues/{id}`, `DiaDiemDto {Ten,DiaChi,HinhAnh,SucChua,GiaThue,HoatDong}` → controller `Tao/Sua` → `DiaDiemService.Tao/Sua` INSERT/UPDATE `DiaDiem`. Admin; validate giá/sức chứa/đường dẫn, unique (Ten,DiaChi); hàm sửa chưa đối chiếu sức chứa mới với event đã booking. Response entity → toast và reload cache/cards. `xoaDiaDiem` DELETE `/api/admin/catalog/venues/{id}` dùng `QuanTriService.Xoa`, không có DELETE dưới `/api/venues`.

### 11.11. Đặt địa điểm và quản lý booking

**Ý định UI:** `wwwroot/pages/dia-diem/dat-dia-diem.html` → `wwwroot/js/diadiem.js::datDiaDiem` đọc `dat-sk`, `dat-dd`, `dat-bd`, `dat-kt`, `dat-tc` → POST `/api/venue-bookings`, `DatDiaDiemDto {DiaDiemId,SuKienId,BatDau,KetThuc,TienCoc}` → **`DatDiaDiemController` trong `Features/DiaDiem/DiaDiemController.cs`** → `DiaDiemService.Dat`.

**Điểm đứt hiện tại:** HTML không có `#dat-tc`; `num('dat-tc')` trả null và handler báo tiền cọc không hợp lệ rồi return. Với form hiện tại, đường POST không chạy đến nơi nếu không có DOM bổ sung. Luồng backend dưới đây tồn tại và có thể gọi qua API trực tiếp, không khẳng định form đã hoạt động trọn vẹn.

**Backend:** role chỉ BTC; Serializable transaction; kiểm tra ngày, owner event, event chưa có booking nào, venue active/sức chứa đủ; query overlap `newStart < oldEnd && newEnd > oldStart`, bỏ các booking đã từ chối/hủy. Tính tiền thuê theo giờ, tối thiểu 1 giờ, làm tròn 2 số; kiểm tra cọc <= tổng; INSERT `DatDiaDiem` ChoXacNhan → commit → entity. Không kiểm tra khoảng thuê bao trọn thời gian event. Unique SuKienId khiến event có booking cũ bị hủy/từ chối vẫn không thể đặt bản ghi mới qua hàm này.

**Danh sách/duyệt:** `wwwroot/pages/dia-diem/lich-dat.html`, `wwwroot/pages/quan-tri/duyet-dat-dia-diem.html` → `taiDatDiaDiem` GET `/api/venue-bookings` → controller `Get` → `DiaDiemService.DanhSachDat` Include venue/event → bảng `ds-dat`. Endpoint list chỉ manager/admin; chưa có `/mine` cho BTC, nên “lịch của tôi” không có API tương ứng. `xacNhanDat/tuChoiDat` POST `/{id}/approve|reject` → `XacNhan` chỉ chuyển từ ChoXacNhan; `huyDat` POST `/{id}/cancel` → `Huy` chỉ BTC owner và trạng thái được phép. Save → message → tải lại bảng; UI render cả ba nút bất kể role. Chuyển HoanThanh: **KHÔNG TÌM THẤY TRONG PROJECT**.

### 11.12. Loại vé và cửa sổ bán

`wwwroot/pages/ve/loai-ve.html`, khu loại vé ở chi tiết/tạo đơn → `wwwroot/js/donve.js::taiLoaiVeTheoSuKien` GET `/api/events/{eventId}/ticket-types` → `Features/LoaiVe/LoaiVeController.cs::Get` → `Features/LoaiVe/LoaiVeService.cs::TheoSuKien` kiểm tra sự kiện công khai hoặc owner/manager trước khi SELECT `LoaiVe` → list → option/bảng/card.

`themLoaiVe/capNhatLoaiVe` → POST route trên hoặc PUT `/api/ticket-types/{id}`, `LoaiVeDto {Ten,Gia,SoLuong,MoBan,DongBan}` → controller `Tao/Sua` → service `Tao/Sua`. BTC/admin; service kiểm tra quyền chủ event hoặc admin, giá VND nguyên không âm, số lượng >0, lịch hợp lệ, tên không trùng; sửa không giảm SoLuong dưới DaBan+DangGiu. INSERT/UPDATE → entity → thông báo/tải lại loại vé. Form có `lv-slmoi` nhưng DTO và request không nhận giới hạn mỗi đơn; backend mua vé dùng 1–20 **mỗi dòng**, không dùng trường này.

POST `/api/ticket-types/{id}/open` và `/close` → controller `Mo/Dong` → `LoaiVeService.MoBan/DongBan`, role BTC và ownership; tác động cửa sổ thời gian bán, không có enum trạng thái loại vé riêng. **Nút/hàm frontend gọi hai API open/close: KHÔNG TÌM THẤY TRONG PROJECT**. Service loại vé thông thường chưa kiểm tra tổng SoLuong của tất cả loại <= SuKien.SoNguoi.

### 11.13. Chọn vé và tạo đơn

`wwwroot/pages/don-ve/tao-don.html` nhận ID event, tải danh sách event/type; `chonLoaiVe` trong `wwwroot/js/donve.js` hỗ trợ đi từ loại đã chọn. Người dùng bấm tạo đơn → `taoDon` đọc loại/số lượng, khóa nút → POST `/api/orders`, `TaoDonDto {Items:[{LoaiVeId,SoLuong}]}` → `Features/DonVe/DonVeController.cs::Tao` → `Features/DonVe/DonVeService.cs::Tao(UserId,dto)`.

JWT bắt buộc. DTO kiểm tra list không rỗng, ID dương, mỗi SoLuong 1–20. Service kiểm tra trùng type, cùng event, event **đúng DaDuyet**, thời gian bán và tồn; Serializable; tăng `LoaiVe.DangGiu`, tính tiền bằng `LoaiVe.Gia` trong DB, INSERT `DonVe` và `ChiTietDonVe` snapshot giá. Hết hạn từ `Order:ExpireMinutes` (mặc định 15, clamp 1–120). Save/commit → `Lay` trả đơn kèm quan hệ → JSON → thông báo/confetti và chuyển `chi-tiet-don.html?id=...`. Tạo đơn chưa tạo `Ve` hay `ThanhToan`.

UI chỉ tạo một dòng; API cho nhiều loại khác nhau cùng sự kiện. Giới hạn 20 áp dụng mỗi dòng, do đó tổng đơn nhiều dòng có thể vượt 20. Event đang diễn ra có thể còn trong cửa sổ bán nhưng bị chặn do rule chỉ DaDuyet.

### 11.14. Xem/lọc đơn, chi tiết, giao dịch và hủy đơn

`wwwroot/pages/don-ve/don-cua-toi.html` → `wwwroot/js/donve.js::taiDonCuaToi` → GET `/api/orders/me` → `DonVeController.Me` → `DonVeService.CuaToi` query theo người mua, Include event/chi tiết/type → list → `__dsDonCache`, `capNhatThongKeDon`, `veDonList`, filter/search cục bộ trên `ds-don`. Không có endpoint admin list toàn bộ đơn riêng tại `/api/orders` GET.

`wwwroot/pages/don-ve/chi-tiet-don.html` → `xemChiTietDon` với query id hoặc input → GET `/api/orders/{id}` → controller `Lay` → service `Lay` Include event/BTC, chi tiết/type, payment, vé; kiểm tra buyer hoặc organizer event hoặc manager/admin → entity → bảng `ds-chitiet`, tổng và link thanh toán. `wwwroot/pages/thanh-toan/giao-dich.html` gọi `taiGiaoDich` cũng GET order rồi lấy `ThanhToans` render `ds-giaodich`; không có GET `/api/payments` list riêng.

Nút hủy → `huyDon` → POST `/api/orders/{id}/cancel` → controller `Huy` → `DonVeService.Huy`: chỉ buyer, chỉ ChoThanhToan; Serializable; đọc chi tiết/loại, giảm DangGiu, đổi DaHuy, Save/commit → message → reload danh sách. Không hủy một đơn đã trả tiền bằng API cancel này.

### 11.15. Tạo thanh toán có tiền và đơn miễn phí

`wwwroot/pages/thanh-toan/thanh-toan.html` hoặc nút của đơn → `wwwroot/js/donve.js::thanhToan/thanhToanDon` → POST `/api/payments`, `TaoThanhToanDto {DonVeId}` → `Features/ThanhToan/ThanhToanController.cs::Tao` → `Features/ThanhToan/ThanhToanService.cs::Tao(UserId,donId)`.

Service đọc order/chi tiết/payment/vé, yêu cầu buyer, ChoThanhToan và chưa hết hạn. **Tổng 0:** `PhatVe`, tăng đã bán/giảm giữ, order ThanhCong trong transaction, không tạo ThanhToan (constraint tiền >0); trả `paymentId:null, soTien:0, checkoutUrl:null`. FE hiện thông báo vé miễn phí sẵn sàng/chuyển danh sách.

**Có tiền:** đánh dấu lần chờ cũ thất bại, tạo ThanhToan DangXuLy rồi commit DB trước khi gọi remote. `Infrastructure/PayOS/PayOsService.cs::Tao` POST `v2/payment-requests` trên merchant API với header credentials, amount nguyên dương, orderCode là order.Id, expiry và return/cancel URL, checksum HMAC. Khi trả link, lưu `MaLienKet`; trả dữ liệu checkout → JS kiểm tra URL rồi `location.assign` sang PayOS. Remote lỗi thì đánh dấu payment thất bại, không tự phát vé. DB transaction không bao trùm PayOS; có khoảng giữa ghi DB và nhận remote, được xử lý một phần bằng trạng thái/link matching của webhook.

Trở lại return/cancel URL không cập nhật ThanhCong. Không có polling/realtime hoặc API xác nhận payment từ URL browser; người dùng tải lại đơn để thấy kết quả webhook.

### 11.16. Webhook, chống lặp và phát vé

**Bắt đầu từ PayOS, không từ nút UI:** POST `/api/payments/webhook`, model `PayOsWebhook` trong `Infrastructure/PayOS/PayOsModels.cs` → `ThanhToanController.Webhook` anonymous JWT → `ThanhToanService.XuLyWebhook` → `PayOsService.Verify` tính HMAC từ dữ liệu sắp xếp/canonicalize, so sánh fixed-time, kiểm tra success/code/reference.

Service mở Serializable transaction; kiểm tra `MaGiaoDich` chống lặp; đọc order theo orderCode, ghép payment chính xác bằng MaLienKet; fallback chỉ khi có đúng một pending payment chưa có link. Không chọn tùy ý payment mới nhất. Số tiền phải khớp tổng; order phải còn chờ/chưa hết hạn; kiểm tra toàn bộ tồn trước mutate. Payment không ghép được/số tiền sai/trạng thái bất thường được ghi NghiVan theo nhánh xử lý; nhánh lệch tiền commit dấu vết trước khi trả 409.

Khi hợp lệ: giảm `LoaiVe.DangGiu`, tăng `DaBan`, thêm từng `Ve` theo số lượng với MaVe từ 12 ký tự đầu GUID dạng uppercase, MaQR GUID đầy đủ; order ThanhCong, payment ThanhCong/NgàyThanhToan/reference, payment pending còn lại thất bại; Save/commit atomically. Unique reference/link/code và rowversion bổ trợ. Webhook thành công trả `{success:true}` riêng, không ApiResponse đầy đủ. Frontend không nhận trực tiếp response này; GET order/tickets lần sau mới thấy vé.

Không có API phát vé lại độc lập. `donve.js::phatHanhVe` chỉ GET order và đếm vé, dù tên hàm gợi ý phát hành. Luồng `PhatVe` không kiểm tra lại trạng thái hủy event; order pending đã có có thể vẫn được hoàn tất sau khi event bị hủy nếu đủ điều kiện còn lại.

### 11.17. Hoàn tiền

`wwwroot/pages/thanh-toan/hoan-tien.html` → `wwwroot/js/donve.js::hoanTien` → POST `/api/payments/{id}/refund`, `HoanTienDto {SoTien,MaHoanTien}` → `ThanhToanController.Hoan` → `ThanhToanService.HoanTien`.

Manager/admin. `{id}` là **ID bản ghi ThanhToan**, không phải DonVeId. Service đọc payment gốc thành công, order/vé/type; tiền hoàn nguyên dương, cộng dồn không quá gốc; hoàn toàn phần từ chối nếu có vé đã check-in. Trong transaction tăng SoTienHoan và ghi mã hoàn; partial đặt đơn YeuCauHoanTien; full đặt đơn DaHoanTien, vé DaHoanTien và trả DaBan của vé chưa sử dụng. Commit → JSON message → FE toast.

Đây là **ghi nhận hoàn tiền nội bộ**; không có lệnh chuyển tiền/refund remote trong `PayOsService`. Không có entity từng lần hoàn, MaHoanTien mới ghi đè mã trước, không có idempotency theo mã hoàn riêng. UI `ht-don` ghi “Mã đơn vé” nhưng được nối thẳng thành payment ID, có thể dùng nhầm ID. Sau thành công handler gọi `taiDonCuaToi` dù trang hoàn tiền thiếu `ds-don`, có nguy cơ lỗi render tiếp theo. Không trình bày như một luồng hoàn tiền ngân hàng hoàn chỉnh đã được chứng minh.

### 11.18. Xem vé và tra cứu mã

API GET `/api/tickets/me` → `Features/Ve/VeController.cs::Me` → `Features/Ve/VeService.cs::CuaToi` query vé qua `DonVe.TaiKhoanId`, Include order/type/event → list. GET `/{id}` → `Lay` và `/by-code/{code}` → `Ma`: buyer hoặc manager/admin được đọc; `Ma` chỉ so `MaVe`, chưa tra `MaQR`. Caller frontend riêng của me/id: **KHÔNG TÌM THẤY TRONG PROJECT**; giao diện hiện dùng order và by-code.

`wwwroot/pages/ve/ve-theo-don.html` → `wwwroot/js/checkin.js::taiVeTheoDon` → GET `/api/orders/{id}` → DonVeService.Lay → lấy `Ves` → `ds-ve-theodon` với nút lịch sử xuất. Đây là lý do organizer có thể nhìn vé qua order của sự kiện mình nhưng không mặc nhiên được gọi API vé bất kỳ bằng role BTC.

`wwwroot/pages/ve/tra-cuu-check-in.html` → `checkin.js::traCuuVe` đọc `ci-ma` hoặc `ci-qr` → GET `/api/tickets/by-code/{code}` → VeController.Ma → VeService.Ma query Ve/quan hệ → vé → render `kq-tracuu`, nút `dienVeCheckin` điền mã. UI cho nhập QR nhưng service lookup này chỉ khớp MaVe; QR có thể không ra kết quả dù check-in API chấp nhận.

### 11.19. Check-in

Trang `wwwroot/pages/ve/tra-cuu-check-in.html`, nút check-in → `wwwroot/js/checkin.js::checkIn` đọc `ci-ve` → POST `/api/check-in`, `CheckInController.CheckInDto {Ma}` → **`CheckInController` trong `Features/Ve/VeController.cs`**, action `Post` → `VeService.CheckIn`.

Role manager/admin; tìm bằng MaVe hoặc MaQR, yêu cầu ChuaSuDung, event DangDienRa và `BatDau <= UTC hiện tại < KetThuc`; UPDATE `Ve.TrangThai=DaCheckIn`, `Ve.NgayCheckIn`, Save; RowVersion giúp một request cạnh tranh còn lại gặp conflict. Response vé → `kq-checkin` và toast thành công. Không kiểm tra payment/order success riêng ở hàm check-in, dựa vào invariant chỉ phát vé khi thành công. Không lưu người thao tác check-in trong model hiện tại.

UI cho BTC vào trang nhưng endpoint không cấp role BTC; ô `ci-ve` là number trong khi mã vé/QR là chuỗi chữ-số. Camera scanner/library quét QR: **KHÔNG TÌM THẤY TRONG PROJECT**. “Quét QR” trong nội dung UI không đủ chứng minh có camera hoạt động.

### 11.20. Xuất PDF, gửi email và lịch sử

`wwwroot/pages/ve/xuat-ve.html` → `wwwroot/js/checkin.js::xuatVe` chọn hình thức và đọc ID → POST `/api/tickets/{id}/export` hoặc `/email`. `Features/XuatVe/XuatVeController.cs::Pdf/Email` → `Features/XuatVe/XuatVeService.cs::Pdf/Gui` kiểm tra chủ vé qua order. Export/email chỉ buyer; admin chỉ có ngoại lệ xem lịch sử, không tự có quyền xuất vé người khác.

`Infrastructure/QR/QrService.cs::Tao(v.MaQR)` tạo PNG QR ECC Q; `Infrastructure/File/VePdfService.cs::Tao` tạo PDF A5. PDF: ghi `XuatVe {Kieu='PDF',ThanhCong=true}`, Save rồi controller trả bytes `application/pdf`, filename `ve-{id}.pdf`. Email: `Infrastructure/Email/EmailService.cs::Gui` gửi đến email người mua kèm PDF, ghi XuatVe thành công/thất bại; email fail ghi log DB rồi ném lỗi lên middleware. Export lại dùng mã vé/QR cũ, không phát vé mới.

**Khoảng trống FE:** `apiFetch` luôn cố đọc `res.json()`, parse fail rơi về object rỗng; `xuatVe` hiện toast thành công nhưng không đọc Blob, không tạo download URL. Backend tạo PDF được, nút frontend chưa hoàn thành việc tải file. Các lựa chọn in quầy/kiosk đều rơi về cùng `/export`, không có driver/printer/kiosk integration riêng.

`taiLichSuXuat` → GET `/api/tickets/{id}/exports` → `XuatVeController.LichSu` → `XuatVeService.LichSu`: owner hoặc admin, query `XuatVe` → list → bảng `ds-xuatve` hoặc thay vùng `ds-ve-theodon`. Nhật ký có kiểu/địa chỉ/kết quả/ngày, không có actor/file URL/số phôi như mô tả thiết kế.

### 11.21. CRUD catalog quản trị

`wwwroot/pages/quan-tri/du-lieu.html` → `wwwroot/js/admin-catalog.js::taiDanhMucQuanTri` → GET `/api/admin/catalog` → `Features/QuanTri/QuanTriController.cs::Get` → `Features/QuanTri/QuanTriService.cs::DanhMuc` SELECT các danh mục → object chứa SuKien/ToChuc/TaiKhoan/loại... → `danhMucQuanTri`, render select và bảng `qt-ds-*`.

Chọn sửa chỉ điền form từ cache. `luuQuanTri(event,kind)` dùng cờ chống gửi lặp: event POST/PUT `.../events[/{id}]`, `QuanTriSuKienDto`; organizer POST/PUT `.../organizers[/{id}]`, `QuanTriToChucDto`; account PUT `.../accounts/{id}`, `QuanTriTaiKhoanDto`. Controller → `LuuSuKien/LuuToChuc/SuaTaiKhoan`; admin bắt buộc. Service kiểm tra category/BTC/account tham chiếu, uniqueness, event booking/inventory theo trường hợp, hash mật khẩu mới nếu nhập; INSERT/UPDATE bảng tương ứng → response → reset form/toast/reload danh mục.

Tạo account từ catalog dùng **POST `/api/auth/register`** hiện có, không có POST catalog/accounts. `luuVeQuanTri` và `taiVeQuanTri` dùng API LoaiVe hiện có; kết quả lưu trong `veQuanTri`. `xoaBanGhiQuanTri` DELETE `/api/admin/catalog/{kind}/{id}` → `QuanTriController.Xoa` → `QuanTriService.Xoa`; các kind được hỗ trợ gồm events, organizers, ticket-types, venues, event-types, chỉ khi không có tham chiếu nghiệp vụ. Không có xóa tài chính/tài khoản tùy ý qua hàm này.

Sửa event catalog giữ owner/trạng thái của record đã có, chặn đổi lịch đã booking, kiểm tra sức chứa với booking và tồn bán/giữ. JS có xử lý giữ milliseconds UTC khi không thực sự đổi thời gian. Các guard này không hoàn toàn đồng nhất với luồng sửa của BTC trong `SuKienService`.

### 11.22. Dashboard, thống kê và hoạt động theo tài khoản

`wwwroot/pages/quan-tri/tong-quan.html`, `bao-cao.html` → `wwwroot/js/quantri.js::taiThongKe/taiThongKeTaiKhoanMoRong`, `donve.js::taiThongKeThanhToan` → GET `/api/admin/dashboard` → `Features/ThongKe/ThongKeController.cs::Get` → `Features/ThongKe/ThongKeService.cs::Dashboard` dùng LINQ count/group/sum trên tài khoản, event, order, type, payment, vé... → projection → `hienThongKe`/các hàm render bảng và số liệu.

`taiThongKeCheckIn` → GET `/api/admin/dashboard/events/{id}/check-in` → controller `CheckIn` → service `CheckIn` aggregate vé event → bảng tổng/đã/chưa check-in. Hai endpoint này **admin-only**; nhiều trang/menu cho manager xem dẫn đến 403. Không có stored procedure/view được gọi dù một số HTML ghi chú như vậy. Doanh thu tính tiền thành công trừ SoTienHoan; free order không có payment; các query riêng không nằm trong một transaction snapshot thống kê.

`wwwroot/pages/quan-tri/su-kien-don-theo-tai-khoan.html` → `quantri.js::taiSuKienCuaTaiKhoan/taiDonVeCuaTaiKhoan` → GET `/api/admin/catalog/accounts/{id}/activity` → `QuanTriController.HoatDong` → `QuanTriService.TheoTaiKhoan` query event theo organizer/account và order theo buyer → UI hai bảng. API admin-only, trong khi body HTML còn cho manager. Thống kê tổ chức/booking ở `tochuc.js`/`diadiem.js` dùng GET danh sách rồi group trên client, không phải mọi thống kê đều thuộc ThongKeService.

### 11.23. Công việc theo thời gian, ảnh và các chức năng chưa có

Đơn hết hạn và event chuyển trạng thái đi từ hosted worker xuống service/DbContext, không có thao tác frontend khởi phát; chi tiết ở mục 14. Frontend không tự nhận push khi worker cập nhật.

Ảnh event/venue là trường `HinhAnh` nhận đường dẫn nội bộ hoặc URL http/https với regex DTO. Admin catalog/venue có thể cập nhật chuỗi này, renderer dùng placeholder khi thiếu/hỏng. `scripts/generate-venue-images.py` và các script trong `tests/QLSK.LocalData/scripts` tạo tài nguyên ngoài request runtime. **Upload ảnh multipart, favorite event, review/rating, voucher, thông báo realtime, hoàn tiền PayOS tự động: KHÔNG TÌM THẤY TRONG PROJECT**.

<a id="muc-12"></a>

## 12. Truy vết từng chức năng theo file

### 12.1. Bắt buộc: User A tìm địa điểm

Ví dụ User A nhập “trung tam” tại trang danh sách địa điểm. User có thể chưa đăng nhập. Quan trọng nhất: **từ khóa được dùng ở browser sau khi backend trả danh sách**, không gửi lên backend và không xuất hiện trong WHERE tìm kiếm SQL.

```text
User A mở /pages/dia-diem/danh-sach.html
  ↓ DOMContentLoaded hoặc submit form tìm kiếm
wwwroot/pages/dia-diem/danh-sach.html
  onsubmit="event.preventDefault();taiDiaDiem()"
  input #dd-tukhoa, kết quả #ds-diadiem, #venue-result-count
  ↓
wwwroot/js/diadiem.js::taiDiaDiem()
  lấy tk = val('dd-tukhoa'); tăng __lanTaiDiaDiem; hiển thị skeleton
  ↓ chưa lọc DB theo tk
wwwroot/js/api.js::apiFetch()
  GET /api/venues                 (admin dùng /api/venues/all)
  ↓ HTTP không có ?q= hoặc body search
Features/DiaDiem/DiaDiemController.cs::Get()/All()
  ↓
Features/DiaDiem/DiaDiemService.cs::DanhSach(all)
  db.DiaDiems.AsNoTracking()
    .Where(x => all || x.HoatDong).OrderBy(x => x.Ten).ToListAsync()
  ↓
Data/ApplicationDbContext.cs + Data/Configurations/DiaDiemConfig.cs
  ↓ EF Core sinh SELECT → bảng DiaDiem trong SQL Server
  ↑ list entity
DiaDiemController → ApiResponse.Ok(list) → JSON response
  ↑
apiFetch: parse/normalize response → resolve
  ↓
taiDiaDiem: bỏ response cũ nếu số lượt không còn khớp
  __diaDiemCache = data.map(chuanHoaDiaDiem)
  ↓
wwwroot/js/sukien.js::chuoiTimKiem()
  bỏ dấu NFD/đ, lowercase cho tk và Ten + DiaChi
  filter chuỗi contains tại browser
  ↓
diadiem.js: tạo venue-grid/card, ảnh/công suất/giá/trạng thái
  → #ds-diadiem.innerHTML; cập nhật #venue-result-count/aria-busy
  ↓
User A thấy danh sách khớp hoặc trạng thái không có kết quả/thử lại
```

| STT | File | Hàm/Class | Vai trò |
|---|---|---|---|
| 1 | `wwwroot/pages/dia-diem/danh-sach.html` | Form onsubmit + DOMContentLoaded | Điểm vào và các input/host kết quả |
| 2 | `wwwroot/js/diadiem.js` | `taiDiaDiem`, `__lanTaiDiaDiem` | Đọc từ khóa, loading, chọn public/admin endpoint, chống stale response |
| 3 | `wwwroot/js/api.js` | `val`, `apiFetch`, `chuanHoaPhanHoi` | Đọc DOM, HTTP/JSON/bearer/timeout, normalize dữ liệu |
| 4 | `wwwroot/config.js` | `API_BASE` | Lấy origin của ứng dụng |
| 5 | `Program.cs` | Pipeline, DI, MapControllers | Đưa HTTP tới controller; cấu hình DbContext/auth/serializer |
| 6 | `Common/Auth/ActiveUserFilter.cs` | `OnAuthorizationAsync` | Nếu có identity: kiểm tra account active và token còn đồng bộ; /all còn cần admin |
| 7 | `Features/DiaDiem/DiaDiemController.cs` | `DiaDiemController.Get/All` | Nhận GET, gọi service, trả envelope |
| 8 | `Features/DiaDiem/DiaDiemService.cs` | `DanhSach(bool all=false)` | Chọn active/all và sắp tên; không nhận keyword |
| 9 | `Data/ApplicationDbContext.cs` | `DiaDiems` | Cửa truy cập ORM |
| 10 | `Domain/Entities/DiaDiem.cs` | `DiaDiem` | Hình dạng dữ liệu venue |
| 11 | `Data/Configurations/DiaDiemConfig.cs` | `DiaDiemConfig` | Map bảng, độ dài/kiểu/các ràng buộc |
| 12 | `Common/Models/ApiResponse.cs` | `Ok<T>` | `{success,data,code,message}` thành công |
| 13 | `wwwroot/js/diadiem.js` | `chuanHoaDiaDiem`, `__diaDiemCache` | Chuẩn hóa/cất danh sách đã trả |
| 14 | `wwwroot/js/sukien.js` | `chuoiTimKiem` | Tìm không dấu/không phân biệt hoa thường bằng chuỗi JS |
| 15 | `wwwroot/js/diadiem.js` | `anhDiaDiemHTML`, `taiDiaDiem`, `lamMoiDiaDiem` | Render card/count, xử lý ảnh/empty/error; làm mới xóa từ khóa và tải lại |
| 16 | `wwwroot/css/content.css`, `app.css`, `responsive.css` | Rule venue-grid/card và responsive | Trình bày dữ liệu DOM sau render, không query DB |

**Không có:** DTO tìm địa điểm, `diaDiemApi.js`, `DiaDiemPage.jsx`, component React hoặc API search riêng. `Features/DiaDiem/Dtos.cs` dùng cho mutation/booking, không tham gia bind GET tìm hiện tại. Nếu API trả tất cả venue hợp lệ mà không có kết quả, cần kiểm tra normalize/filter tại browser trước khi nghi ngờ query từ khóa SQL.

### 12.2. Bảng file của các nhóm thao tác còn lại

Các đường dẫn đầy đủ đã nêu ở mục 11; bảng sau giữ nguyên file thật và lớp/hàm để định vị nhanh. `api.js`/`Program.cs`/DbContext là chặng chung, không lặp trong mỗi ô.

| Nhóm thao tác | File frontend chạy | Controller thật | Service/method và file xử lý |
|---|---|---|---|
| Đăng ký/login/logout/password | `wwwroot/js/auth.js` | `Features/TaiKhoan/AuthController.cs` | `Features/TaiKhoan/TaiKhoanService.cs`: DangKy/DangNhap/DoiMatKhau; logout không service |
| Profile/list/lock/role | `wwwroot/js/auth.js` | `Features/TaiKhoan/TaiKhoanController.cs` | `TaiKhoanService.cs`: Lay/Sua/DanhSach/Khoa/DoiVaiTro |
| Gửi/xem hồ sơ tổ chức | `wwwroot/js/tochuc.js` | `Features/XacThucToChuc/XacThucController.cs` | `Features/XacThucToChuc/XacThucService.cs`: Gui/CuaToi |
| Duyệt hồ sơ tổ chức | `wwwroot/js/main.js`, `tochuc.js` | `Features/XacThucToChuc/XacThucController.cs` | `XacThucService.cs`: DanhSach/Duyet/TuChoi |
| Danh sách/chi tiết/tìm event | `wwwroot/js/sukien.js` | `Features/SuKien/SuKienController.cs` | `Features/SuKien/SuKienService.cs`: DanhSach/CuaToi/Lay; tìm/lọc sau response |
| Tạo/duyệt/hủy event | `wwwroot/js/sukien.js` | `Features/SuKien/SuKienController.cs` | `SuKienService.cs`: Tao/Duyet/TuChoi/Huy; Sua/GuiDuyet có API, chưa FE |
| Loại sự kiện | `wwwroot/js/sukien.js` | LoaiSuKienController trong `Features/SuKien/SuKienController.cs` | `SuKienService.cs`: LayLoai/TaoLoai/SuaLoai |
| Venue CRUD/booking | `wwwroot/js/diadiem.js` | Hai controller trong `Features/DiaDiem/DiaDiemController.cs` | `Features/DiaDiem/DiaDiemService.cs`: Tao/Sua/Dat/DanhSachDat/XacNhan/Huy |
| Ticket types | `wwwroot/js/donve.js`, `admin-catalog.js` | `Features/LoaiVe/LoaiVeController.cs` | `Features/LoaiVe/LoaiVeService.cs`: TheoSuKien/Tao/Sua/MoBan/DongBan |
| Orders | `wwwroot/js/donve.js`, `checkin.js` đọc vé theo đơn | `Features/DonVe/DonVeController.cs` | `Features/DonVe/DonVeService.cs`: Tao/CuaToi/Lay/Huy |
| Payment/webhook/refund | `wwwroot/js/donve.js`; webhook đến từ PayOS | `Features/ThanhToan/ThanhToanController.cs` | `Features/ThanhToan/ThanhToanService.cs` + `Infrastructure/PayOS/PayOsService.cs` |
| Vé/check-in | `wwwroot/js/checkin.js` | VeController/CheckInController trong `Features/Ve/VeController.cs` | `Features/Ve/VeService.cs`: CuaToi/Lay/Ma/CheckIn |
| PDF/email/history | `wwwroot/js/checkin.js` | `Features/XuatVe/XuatVeController.cs` | `Features/XuatVe/XuatVeService.cs` + QR/PDF/Email infrastructure |
| Dashboard | `wwwroot/js/quantri.js`, `donve.js` | `Features/ThongKe/ThongKeController.cs` | `Features/ThongKe/ThongKeService.cs`: Dashboard/CheckIn |
| Admin data/activity/delete | `wwwroot/js/admin-catalog.js`, `quantri.js`, danh mục JS | `Features/QuanTri/QuanTriController.cs` | `Features/QuanTri/QuanTriService.cs`: DanhMuc/LuuSuKien/LuuToChuc/SuaTaiKhoan/TheoTaiKhoan/Xoa |

<a id="muc-13"></a>

## 13. Phân tích chi tiết một request hoàn chỉnh

Chọn **tạo đơn mua 2 vé của loại ID 17**, giả sử event đã duyệt, loại còn vé và đang mở bán. Các ID/số lượng sau chỉ là ví dụ request, không khẳng định record 17 tồn tại trong DB local.

### Bước 1 — Frontend render nút

`wwwroot/pages/don-ve/tao-don.html` chứa lựa chọn loại, ô số lượng và nút gọi `taoDon()`. `wwwroot/js/donve.js` khởi tạo từ query string ID event, GET event/type để hiển thị lựa chọn; UI tính tổng tham khảo, không phải nguồn giá có thẩm quyền.

### Bước 2 — Event handler

`wwwroot/js/donve.js::taoDon` đọc loại vé/số lượng qua helper DOM, kiểm tra giá trị và dùng trạng thái disabled của nút để tránh click lặp trong lúc chờ. Không INSERT gì tại browser.

### Bước 3 — API client

Handler gọi `wwwroot/js/api.js::apiFetch('/api/orders','POST', {Items:[...]})`. Wrapper lấy token từ `store`, bắt đầu spinner, JSON stringify và timeout. Request không gửi giá, total, buyer ID hay trạng thái order.

### Bước 4 — HTTP

```http
POST /api/orders HTTP/1.1
Authorization: Bearer <JWT của người mua>
Content-Type: application/json

{"Items":[{"LoaiVeId":17,"SoLuong":2}]}
```

Không đưa JWT thật vào log/báo cáo. Body PascalCase ở client vẫn bind được theo thiết lập JSON ASP.NET Core; response mặc định camelCase.

### Bước 5 — Routing và bind DTO

`Program.cs::MapControllers` đưa request đến `[Route('api/orders')]` + `[HttpPost]` của `Features/DonVe/DonVeController.cs::Tao`. MVC bind `Features/DonVe/Dtos.cs::TaoDonDto`, mỗi dòng là `DatVeItemDto`. Required/MinLength/Range có thể trả 400 trước action nếu invalid.

### Bước 6 — Authentication/Authorization

JWT bearer kiểm tra token theo `Program.cs`; `[Authorize]` trên controller yêu cầu authenticated, không giới hạn riêng role NguoiDung. `Common/Auth/ActiveUserFilter.cs` đối chiếu active/role/password fingerprint; `Common/Auth/UserClaims.cs::LayId` lấy buyer ID từ claim. Token lỗi → 401; account khóa → 403. Không tin buyer ID từ body.

### Bước 7 — Controller

Action gọi `service.Tao(User.LayId(), dto)` và bọc kết quả bằng `Ok(ApiResponse.Ok(...))`. Controller không tính tiền, không trừ vé và không gọi PayOS.

### Bước 8 — Service

`Features/DonVe/DonVeService.cs::Tao` kiểm tra list/trùng loại, mở Serializable transaction, lấy các ID đã sắp xếp và query `LoaiVes.Include(x=>x.SuKien)` theo thứ tự ID. Thiếu type → 404.

### Bước 9 — Business rules

Tất cả type phải cùng event; event phải DaDuyet; mỗi dòng 1–20; now UTC nằm trong MoBan/DongBan; `DaBan+DangGiu+SoLuong <= SoLuong`. Giá lấy từ DB. Ví dụ DB giá 150.000 thì tổng 300.000 cho 2 vé; client không thể tự đặt total=1 để giảm giá.

### Bước 10 — Database và transaction

Đọc `LoaiVe`, `SuKien`; UPDATE type tăng DangGiu thêm 2; INSERT `DonVe` buyer/event/số lượng/tổng tiền/hết hạn/ChoThanhToan; INSERT `ChiTietDonVe` type/số lượng/đơn giá snapshot/thành tiền. `SaveChangesAsync` lưu các thay đổi, FK/check/rowversion được SQL kiểm tra, rồi Commit. Lỗi trước commit rollback các thay đổi trong transaction. Chưa có bản ghi Ve hoặc ThanhToan.

Sau commit, service gọi `Lay(userId,order.Id,false)` đọc đơn kèm quan hệ để trả response. Read này diễn ra sau commit: nếu hiếm khi đọc lại lỗi, đơn đã được tạo vẫn tồn tại. Không có idempotency key tạo đơn chống retry mạng toàn trình; disable nút chỉ giảm gửi lặp trên một UI.

### Bước 11 — Response

HTTP 200 thành công, envelope `{success:true,data:<đơn kèm quan hệ>,code:null,message:null}`; `data.id` là ID mới, `trangThai` là số enum. Những field như `chiTiets`, `thanhToans`, `ves` phụ thuộc dữ liệu được Include; không mô tả thành một DTO riêng chưa tồn tại. Nếu sold out, service ném 409 `TICKET_SOLD_OUT`; lỗi cạnh tranh thành 409; validation 400.

### Bước 12 — Frontend nhận

`apiFetch` parse và normalize response rồi trả Promise. `taoDon` lấy ID từ response được normalize, thông báo thành công, hiệu ứng confetti, chuyển URL chi tiết đơn. Catch hiển thị message, finally khôi phục nút nếu còn trang hiện tại.

### Bước 13 — UI cuối cùng

`wwwroot/pages/don-ve/chi-tiet-don.html?id=<id>` được mở. Khởi tạo trong `donve.js` gọi `xemChiTietDon(id)` và thực hiện **request GET thứ hai**; handler render bảng dòng vé/tổng và link thanh toán. Người dùng đang có một đơn chờ thanh toán/giữ vé, chưa có vé QR. Muốn có vé phải đi tiếp nhánh payment/free order ở mục 11.15–11.16.

<a id="muc-14"></a>

## 14. Background Worker/Job

### 14.1. Hết hạn đơn

`Program.cs` đăng ký `AddHostedService<ExpiredOrderWorker>()`. `Infrastructure/ExpiredOrderWorker.cs::ExecuteAsync` chạy một lượt ngay, sau đó delay 30 giây giữa các lượt; tạo scope, resolve `DonVeService` và gọi `HetHan`.

```text
Host start → ExpiredOrderWorker
  → tạo scope → DonVeService.HetHan
  → SELECT tối đa 100 ID ChoThanhToan và HetHan < UtcNow
     OrderBy HetHan, ThenBy Id
  → với từng ID: transaction Serializable + đọc lại state/time
  → TraVeGiu: giảm LoaiVe.DangGiu
  → DonVe.TrangThai = HetHan → Save/commit
  → ghi log nếu lỗi → chờ 30 giây → lặp
```

Đọc lại trước cập nhật giúp không giải phóng một đơn đã được webhook hoàn tất giữa lúc chọn ID và xử lý. Batch tối đa 100/lượt nên lượng tồn đọng lớn có thể chậm nhiều chu kỳ; không phải quét hết mọi đơn trong một transaction dài.

### 14.2. Trạng thái sự kiện theo thời gian

`Infrastructure/EventStateWorker.cs` được host đăng ký, chạy ngay rồi mỗi 1 phút. Worker tạo scope lấy `ApplicationDbContext`, tìm event được duyệt đang nằm trong khung lịch → DangDienRa; event DaDuyet/DangDienRa đã qua KetThuc → DaKetThuc; SaveChanges. Dùng UTC giống các rule order/check-in.

```text
Host start → EventStateWorker → DbContext.SuKiens
  → so sánh BatDau/KetThuc với UTC → cập nhật trạng thái
  → SaveChanges → chờ 1 phút → lặp
```

Không có controller/service HTTP ở giữa worker thứ hai. Công việc vẫn cần chạy khi người dùng đóng browser, nên không phụ thuộc một nút refresh. Worker không tự đẩy response tới các trang đang mở. Có độ trễ chu kỳ: check-in đòi event DangDienRa nên ngay sau giờ bắt đầu có thể phải đợi lượt worker.

Hai worker nằm trong cùng web process; host dừng thì chúng dừng. Nhiều instance sẽ cùng chạy, chưa có distributed lock/leader election/job scheduler bền vững. Transaction/rowversion hạn chế một số xung đột nhưng không đồng nghĩa đã có điều phối distributed. Logger bắt lỗi để tiếp tục chu kỳ; cancellation của delay theo vòng đời host. **Cron/Hangfire/Quartz, tự hết hạn vé, tự HoanThanh booking: KHÔNG TÌM THẤY TRONG PROJECT**.

<a id="muc-15"></a>

## 15. Xử lý lỗi

```text
DTO invalid → ApiBehaviorOptions.InvalidModelStateResponseFactory → 400 envelope
Service ném ApiException → ApiExceptionMiddleware → status/code/message nghiệp vụ
EF concurrency → middleware log warning → 409 CONCURRENCY_CONFLICT
EF DbUpdateException → log warning → 409 DATABASE_CONFLICT
Lỗi chưa xử lý → log error → 500 SYSTEM_ERROR, thông điệp chung
  ↓ HTTP response
apiFetch → parse/normalize → throw Error hoặc xử lý 401
  ↓ handler catch
thongBao/toast/empty/retry UI; finally dừng spinner/mở nút
```

| Nguồn lỗi | Nơi xử lý thật | Kết quả |
|---|---|---|
| Required/Range/Email/Regex DTO | `Program.cs` cấu hình `InvalidModelStateResponseFactory` | 400 `VALIDATION_ERROR`, lấy thông báo model đầu tiên |
| Business rule | `Features/*/*Service.cs`, `Common/Exceptions/ApiException.cs` | 400/403/404/409... và code chuyên biệt |
| Missing/expired/invalid JWT | JWT bearer + `UseStatusCodePages` trong `Program.cs` | 401 JSON khi response chưa có body |
| Role sai | Authorization + StatusCodePages | 403 `FORBIDDEN` |
| Account khóa/token cũ | `Common/Auth/ActiveUserFilter.cs` | 403 ACCOUNT_LOCKED / 401 TOKEN_STALE |
| Unique/FK/check conflict | `Common/Exceptions/ApiExceptionMiddleware.cs` | 409 DATABASE_CONFLICT; không lộ SQL chi tiết ra client |
| RowVersion conflict | Cùng middleware | 409 CONCURRENCY_CONFLICT, yêu cầu tải lại |
| PayOS cấu hình/gọi/verify lỗi | `Infrastructure/PayOS/PayOsService.cs`, payment service | Exception có nghĩa nghiệp vụ, trạng thái payment cập nhật theo nhánh |
| SMTP lỗi | `Features/XuatVe/XuatVeService.cs::Gui` | Ghi XuatVe failed rồi rethrow, middleware trả lỗi |
| Timeout/mạng/abort | `wwwroot/js/api.js` | Thông điệp thân thiện, spinner kết thúc |

Các handler frontend thường try/catch và gọi thông báo, nên có trường hợp API thành công nhưng reload/render phụ bị lỗi, ví dụ refund gọi tải danh sách không có host DOM. Khi debug cần tách HTTP failure với lỗi JavaScript sau HTTP 200. Response PDF là ngoại lệ quan trọng: parse JSON fail đang bị wrapper biến thành object rỗng thay vì download.

Middleware bảo vệ request HTTP, không bao quanh code chạy worker; worker có try/catch/logger riêng. Không tìm thấy global retry policy, circuit breaker, log tập trung/correlation ID hoặc hàng đợi bền vững cho email. Đây là phạm vi hiện có, không khẳng định log hoặc hệ thống giám sát ngoài project đang tồn tại.

<a id="muc-16"></a>

## 16. Bảng API

**67 action HTTP hiện có**, đếm từ thuộc tính HttpGet/Post/Put/Delete trong controller. Bỏ `:int` trong mẫu URL cho dễ đọc; router thực tế có ràng buộc kiểu ở những ID tương ứng. `kind` của DELETE là một route tham số, không tính mỗi giá trị kind thành endpoint mới. Swagger/static file/CLI không tính là API nghiệp vụ.

Trong bảng: frontend `*.js` nằm tại **`wwwroot/js/`**; backend `Nhóm/File.cs` nằm tại **`Features/`**. Tên service tương ứng file `Features/<Nhóm>/<Service>.cs`, ngoại lệ `XacThucService` ở nhóm `XacThucToChuc`. Mục 12 và 19 có đầy đủ đường dẫn. `Public` = anonymous; `JWT` = tài khoản authenticated; `BTC` = BanToChuc; `QL` = QuanLySuKien; `QT` = QuanTriVien. Ownership/validation cụ thể ở mục 11, không được bỏ chỉ vì role đã đúng.

| Method | Endpoint | Frontend sử dụng tại | Backend xử lý tại | Service | Chức năng/quyền |
|---|---|---|---|---|---|
| POST | `/api/auth/register` | `auth.js::dangKy`; `admin-catalog.js::luuQuanTri` khi tạo account | `TaiKhoan/AuthController.cs::DangKy` | TaiKhoanService.DangKy | Đăng ký, Public |
| POST | `/api/auth/login` | `auth.js::dangNhap` | `TaiKhoan/AuthController.cs::DangNhap` | TaiKhoanService.DangNhap | Đăng nhập, Public |
| POST | `/api/auth/logout` | `auth.js::dangXuat` | `TaiKhoan/AuthController.cs::DangXuat` | Không có | Message logout, JWT |
| PUT | `/api/auth/change-password` | `auth.js::doiMatKhau` | `TaiKhoan/AuthController.cs::DoiMatKhau` | TaiKhoanService.DoiMatKhau | Đổi mật khẩu, JWT |
| GET | `/api/accounts/me` | `auth.js::taiHoSoCaNhan` | `TaiKhoan/TaiKhoanController.cs::Me` | TaiKhoanService.Lay | Hồ sơ của tôi, JWT |
| PUT | `/api/accounts/me` | `auth.js::capNhatTaiKhoan` | `TaiKhoan/TaiKhoanController.cs::Sua` | TaiKhoanService.Sua | Sửa họ tên, JWT |
| GET | `/api/accounts` | `auth.js::taiTaiKhoan` | `TaiKhoan/TaiKhoanController.cs::DanhSach` | TaiKhoanService.DanhSach | Query q/role/active/page/size, QT |
| POST | `/api/accounts/{id}/lock` | `auth.js::khoaTaiKhoan` | `TaiKhoan/TaiKhoanController.cs::Khoa` | TaiKhoanService.Khoa | Khóa, QT |
| POST | `/api/accounts/{id}/unlock` | `auth.js::moKhoaTaiKhoan` | `TaiKhoan/TaiKhoanController.cs::MoKhoa` | TaiKhoanService.Khoa | Mở khóa, QT |
| PUT | `/api/accounts/{id}/role` | `auth.js::doiVaiTroTaiKhoan` | `TaiKhoan/TaiKhoanController.cs::DoiVaiTro` | TaiKhoanService.DoiVaiTro | Đổi role, QT |
| POST | `/api/organizer-verifications` | `tochuc.js::guiYeuCau/capNhatYeuCau` | `XacThucToChuc/XacThucController.cs::Gui` | XacThucService.Gui | Gửi hồ sơ, JWT |
| GET | `/api/organizer-verifications/me` | `tochuc.js::taiYeuCauCuaToi` | `XacThucToChuc/XacThucController.cs::Me` | XacThucService.CuaToi | Hồ sơ hiện hành của tôi, JWT |
| GET | `/api/organizer-verifications` | `tochuc.js::taiThongKeToChuc`, `main.js::renderDuyetToChuc` | `XacThucToChuc/XacThucController.cs::DanhSach` | XacThucService.DanhSach | Danh sách hồ sơ, QL/QT |
| POST | `/api/organizer-verifications/{id}/approve` | `tochuc.js::duyetYeuCauToChuc`, `main.js::duyetToChucNhanh` | `XacThucToChuc/XacThucController.cs::Duyet` | XacThucService.Duyet | Duyệt tổ chức, QL/QT |
| POST | `/api/organizer-verifications/{id}/reject` | `tochuc.js::tuChoiYeuCauToChuc`, `main.js::tuChoiToChucNhanh` | `XacThucToChuc/XacThucController.cs::TuChoi` | XacThucService.TuChoi | Từ chối có lý do, QL/QT |
| GET | `/api/event-types` | `sukien.js::taiLoaiSuKien` | `SuKien/SuKienController.cs::LoaiSuKienController.Get` | SuKienService.LayLoai | Loại active, Public |
| GET | `/api/event-types/all` | `sukien.js::taiLoaiSuKien` nhánh admin | `SuKien/SuKienController.cs::LoaiSuKienController.All` | SuKienService.LayLoai | Tất cả loại, QT |
| POST | `/api/event-types` | `sukien.js::themLoaiSuKien` | `SuKien/SuKienController.cs::LoaiSuKienController.Tao` | SuKienService.TaoLoai | Thêm loại, QT |
| PUT | `/api/event-types/{id}` | `sukien.js::capNhatLoaiSuKien/doiTrangThaiLoai` | `SuKien/SuKienController.cs::LoaiSuKienController.Sua` | SuKienService.SuaLoai | Sửa/bật/tắt loại, QT |
| GET | `/api/events` | `sukien.js::taiDanhSachSuKien`; khởi tạo tạo đơn trong `donve.js` | `SuKien/SuKienController.cs::Get` | SuKienService.DanhSach | Event công khai, Public |
| GET | `/api/events/manage` | `sukien.js::taiDanhSachSuKien` phạm vi manage | `SuKien/SuKienController.cs::Manage` | SuKienService.DanhSach | Event quản lý, QL/QT |
| GET | `/api/events/mine` | `sukien.js::taiDanhSachSuKien` phạm vi mine | `SuKien/SuKienController.cs::Mine` | SuKienService.CuaToi | Event của BTC, BTC |
| GET | `/api/events/{id}` | `sukien.js::xemChiTietSuKien` | `SuKien/SuKienController.cs::Lay` | SuKienService.Lay | Chi tiết; public hoặc kiểm tra private |
| POST | `/api/events` | `sukien.js::taoSuKien` | `SuKien/SuKienController.cs::Tao` | SuKienService.Tao | Tạo event, BTC và có hồ sơ tổ chức |
| PUT | `/api/events/{id}` | **KHÔNG TÌM THẤY TRONG PROJECT** caller FE | `SuKien/SuKienController.cs::Sua` | SuKienService.Sua | Sửa event owner, BTC |
| POST | `/api/events/{id}/submit` | **KHÔNG TÌM THẤY TRONG PROJECT** caller FE | `SuKien/SuKienController.cs::Gui` | SuKienService.GuiDuyet | Gửi duyệt, BTC |
| POST | `/api/events/{id}/approve` | `sukien.js::duyetSuKien` | `SuKien/SuKienController.cs::Duyet` | SuKienService.Duyet | Duyệt event, QL/QT |
| POST | `/api/events/{id}/reject` | `sukien.js::tuChoiSuKien` | `SuKien/SuKienController.cs::TuChoi` | SuKienService.TuChoi | Từ chối event, QL/QT |
| POST | `/api/events/{id}/cancel` | `sukien.js::huySuKien` | `SuKien/SuKienController.cs::Huy` | SuKienService.Huy | Hủy event owner, BTC |
| GET | `/api/venues` | `diadiem.js::taiDiaDiem` | `DiaDiem/DiaDiemController.cs::Get` | DiaDiemService.DanhSach | Venue active, Public |
| GET | `/api/venues/all` | `diadiem.js::taiDiaDiem` nhánh admin | `DiaDiem/DiaDiemController.cs::All` | DiaDiemService.DanhSach | Venue kể cả inactive, QT |
| POST | `/api/venues` | `diadiem.js::themDiaDiem` | `DiaDiem/DiaDiemController.cs::Tao` | DiaDiemService.Tao | Tạo venue, QT |
| PUT | `/api/venues/{id}` | `diadiem.js::capNhatDiaDiem/doiTrangThaiDiaDiem` | `DiaDiem/DiaDiemController.cs::Sua` | DiaDiemService.Sua | Sửa/bật/tắt venue, QT |
| POST | `/api/venue-bookings` | `diadiem.js::datDiaDiem` (form đang bị chặn trước gửi) | `DiaDiem/DiaDiemController.cs::DatDiaDiemController.Dat` | DiaDiemService.Dat | Đặt venue, BTC owner |
| GET | `/api/venue-bookings` | `diadiem.js::taiDatDiaDiem/taiThongKeDatDiaDiem` | `DiaDiem/DiaDiemController.cs::DatDiaDiemController.Get` | DiaDiemService.DanhSachDat | List booking, QL/QT |
| POST | `/api/venue-bookings/{id}/approve` | `diadiem.js::xacNhanDat` | `DiaDiem/DiaDiemController.cs::DatDiaDiemController.Duyet` | DiaDiemService.XacNhan | Xác nhận, QL/QT |
| POST | `/api/venue-bookings/{id}/reject` | `diadiem.js::tuChoiDat` | `DiaDiem/DiaDiemController.cs::DatDiaDiemController.TuChoi` | DiaDiemService.XacNhan | Từ chối, QL/QT |
| POST | `/api/venue-bookings/{id}/cancel` | `diadiem.js::huyDat` | `DiaDiem/DiaDiemController.cs::DatDiaDiemController.Huy` | DiaDiemService.Huy | Hủy owner, BTC |
| GET | `/api/events/{eventId}/ticket-types` | `donve.js::taiLoaiVeTheoSuKien`, khởi tạo tạo đơn; `admin-catalog.js::taiVeQuanTri` | `LoaiVe/LoaiVeController.cs::Get` | LoaiVeService.TheoSuKien | Type của event, kiểm tra public/private |
| POST | `/api/events/{eventId}/ticket-types` | `donve.js::themLoaiVe`; `admin-catalog.js::luuVeQuanTri` | `LoaiVe/LoaiVeController.cs::Tao` | LoaiVeService.Tao | Tạo type, BTC owner/QT |
| PUT | `/api/ticket-types/{id}` | `donve.js::capNhatLoaiVe`; `admin-catalog.js::luuVeQuanTri` | `LoaiVe/LoaiVeController.cs::Sua` | LoaiVeService.Sua | Sửa type, BTC owner/QT |
| POST | `/api/ticket-types/{id}/open` | **KHÔNG TÌM THẤY TRONG PROJECT** caller FE | `LoaiVe/LoaiVeController.cs::Mo` | LoaiVeService.MoBan | Mở bán, BTC owner |
| POST | `/api/ticket-types/{id}/close` | **KHÔNG TÌM THẤY TRONG PROJECT** caller FE | `LoaiVe/LoaiVeController.cs::Dong` | LoaiVeService.DongBan | Đóng bán, BTC owner |
| POST | `/api/orders` | `donve.js::taoDon` | `DonVe/DonVeController.cs::Tao` | DonVeService.Tao | Tạo đơn/giữ vé, JWT |
| GET | `/api/orders/me` | `donve.js::taiDonCuaToi` | `DonVe/DonVeController.cs::Me` | DonVeService.CuaToi | Đơn buyer, JWT |
| GET | `/api/orders/{id}` | `donve.js::xemChiTietDon/taiGiaoDich/taiThanhToanTheoDon/phatHanhVe`; `checkin.js::taiVeTheoDon` | `DonVe/DonVeController.cs::Lay` | DonVeService.Lay | Chi tiết và quan hệ, JWT + quyền đọc |
| POST | `/api/orders/{id}/cancel` | `donve.js::huyDon` | `DonVe/DonVeController.cs::Huy` | DonVeService.Huy | Hủy đơn chờ, buyer |
| POST | `/api/payments` | `donve.js::thanhToanDon` | `ThanhToan/ThanhToanController.cs::Tao` | ThanhToanService.Tao | Link/free order, buyer |
| POST | `/api/payments/webhook` | Không có caller FE; PayOS gọi | `ThanhToan/ThanhToanController.cs::Webhook` | ThanhToanService.XuLyWebhook + PayOsService.Verify | Webhook anonymous JWT, kiểm tra HMAC |
| POST | `/api/payments/{id}/refund` | `donve.js::hoanTien` | `ThanhToan/ThanhToanController.cs::Hoan` | ThanhToanService.HoanTien | Ghi nhận hoàn theo payment ID, QL/QT |
| GET | `/api/tickets/me` | **KHÔNG TÌM THẤY TRONG PROJECT** caller FE | `Ve/VeController.cs::Me` | VeService.CuaToi | Vé buyer, JWT |
| GET | `/api/tickets/{id}` | **KHÔNG TÌM THẤY TRONG PROJECT** caller FE | `Ve/VeController.cs::Lay` | VeService.Lay | Vé theo ID, buyer/QL/QT |
| GET | `/api/tickets/by-code/{code}` | `checkin.js::traCuuVe` | `Ve/VeController.cs::Ma` | VeService.Ma | Tra MaVe, buyer/QL/QT |
| POST | `/api/check-in` | `checkin.js::checkIn` | `Ve/VeController.cs::CheckInController.Post` | VeService.CheckIn | Check-in MaVe/MaQR, QL/QT |
| GET | `/api/tickets/{id}/exports` | `checkin.js::taiLichSuXuat` | `XuatVe/XuatVeController.cs::LichSu` | XuatVeService.LichSu | Nhật ký xuất, buyer/QT |
| POST | `/api/tickets/{id}/export` | `checkin.js::xuatVe/xuatVeNhanh` | `XuatVe/XuatVeController.cs::Pdf` | XuatVeService.Pdf | PDF binary, buyer |
| POST | `/api/tickets/{id}/email` | `checkin.js::xuatVe/guiEmailVe` | `XuatVe/XuatVeController.cs::Email` | XuatVeService.Gui | SMTP/PDF, buyer |
| GET | `/api/admin/dashboard` | `quantri.js::taiThongKe/taiThongKeTaiKhoanMoRong`; `donve.js::taiThongKeThanhToan` | `ThongKe/ThongKeController.cs::Get` | ThongKeService.Dashboard | Tổng hợp LINQ, QT |
| GET | `/api/admin/dashboard/events/{id}/check-in` | `quantri.js::taiThongKeCheckIn` | `ThongKe/ThongKeController.cs::CheckIn` | ThongKeService.CheckIn | Đếm trạng thái vé event, QT |
| GET | `/api/admin/catalog` | `admin-catalog.js::taiDanhMucQuanTri` | `QuanTri/QuanTriController.cs::Get` | QuanTriService.DanhMuc | Danh mục quản trị, QT |
| POST | `/api/admin/catalog/events` | `admin-catalog.js::luuQuanTri` | `QuanTri/QuanTriController.cs::TaoSuKien` | QuanTriService.LuuSuKien | Tạo event quản trị, QT |
| PUT | `/api/admin/catalog/events/{id}` | `admin-catalog.js::luuQuanTri` | `QuanTri/QuanTriController.cs::SuaSuKien` | QuanTriService.LuuSuKien | Sửa event quản trị, QT |
| POST | `/api/admin/catalog/organizers` | `admin-catalog.js::luuQuanTri` | `QuanTri/QuanTriController.cs::TaoToChuc` | QuanTriService.LuuToChuc | Tạo organizer, QT |
| PUT | `/api/admin/catalog/organizers/{id}` | `admin-catalog.js::luuQuanTri` | `QuanTri/QuanTriController.cs::SuaToChuc` | QuanTriService.LuuToChuc | Sửa organizer, QT |
| PUT | `/api/admin/catalog/accounts/{id}` | `admin-catalog.js::luuQuanTri` | `QuanTri/QuanTriController.cs::SuaTaiKhoan` | QuanTriService.SuaTaiKhoan | Sửa tên/email/password account, QT |
| GET | `/api/admin/catalog/accounts/{id}/activity` | `quantri.js::taiSuKienCuaTaiKhoan/taiDonVeCuaTaiKhoan` | `QuanTri/QuanTriController.cs::HoatDong` | QuanTriService.TheoTaiKhoan | Event/order theo account, QT |
| DELETE | `/api/admin/catalog/{kind}/{id}` | `admin-catalog.js::xoaBanGhiQuanTri`; `diadiem.js::xoaDiaDiem`; `sukien.js::xoaLoaiSuKien` | `QuanTri/QuanTriController.cs::Xoa` | QuanTriService.Xoa | Xóa danh mục chưa dùng, QT |

API list tài khoản hỗ trợ query parameters đã nêu, nhưng không có query filter tương đương cho mọi API list khác. Tổng hợp theo nhóm: Auth 4 + Accounts 6 + Organizer verification 5 + Event types 4 + Events 10 + Venues 4 + Bookings 5 + Ticket types 5 + Orders 4 + Payments 3 + Tickets/check-in/export 7 + Dashboard 2 + Catalog 8 = **67**.

<a id="muc-17"></a>

## 17. Bảng Frontend ↔ Backend

Để chứa đủ 35 trang mà vẫn đọc được, cột Page dưới đây dùng tiền tố **`wwwroot/`**; cột API frontend dùng tiền tố **`wwwroot/js/`**. “Khối DOM/hàm render” là component theo nghĩa giao diện của project. Tên controller/service tra về file thật tại mục 12/16, không phải module React. Những trang có nhiều action dùng chung bảng API ở mục 16 để tra từng HTTP verb.

| Chức năng | Frontend Page (sau `wwwroot/`) | Component/khối DOM | API Frontend | Endpoint Backend | Controller | Service | Entity/Bảng |
|---|---|---|---|---|---|---|---|
| Trang chủ/tìm event | `index.html` | Hero form, card `ds-sukien` | `sukien.js::timSuKienTrangChu/taiDanhSachSuKien` | GET `/api/events`, `/api/event-types` | SuKien/LoaiSuKienController | SuKienService | SuKien, LoaiSuKien, BanToChuc |
| Đăng ký | `pages/auth/dang-ky.html` | Form `dk-*` | `auth.js::dangKy` | POST `/api/auth/register` | AuthController | TaiKhoanService | TaiKhoan, VaiTro |
| Đăng nhập | `pages/auth/dang-nhap.html` | Input `dn-*` | `auth.js::dangNhap` | POST `/api/auth/login` | AuthController | TaiKhoanService + JwtService | TaiKhoan, VaiTro |
| Hồ sơ | `pages/tai-khoan/thong-tin.html` | Form `qtk-*` | `auth.js::taiHoSoCaNhan/capNhatTaiKhoan` | GET/PUT `/api/accounts/me` | TaiKhoanController | TaiKhoanService | TaiKhoan, VaiTro |
| Đổi password | `pages/tai-khoan/doi-mat-khau.html` | Input `dmk-*` | `auth.js::doiMatKhau` | PUT `/api/auth/change-password` | AuthController | TaiKhoanService | TaiKhoan |
| Gửi xác thực | `pages/to-chuc/gui-xac-thuc.html` | Form `tc-*` | `tochuc.js::guiYeuCau` | POST `/api/organizer-verifications` | XacThucController | XacThucService | YeuCauXacThucToChuc, BanToChuc |
| Xem hồ sơ chờ | `pages/to-chuc/yeu-cau-cua-toi.html` | Card `ds-yeucau` | `tochuc.js::taiYeuCauCuaToi` | GET `.../organizer-verifications/me` | XacThucController | XacThucService | YeuCauXacThucToChuc |
| Danh sách sự kiện | `pages/su-kien/danh-sach.html` | Search/chips/cards/paging | `sukien.js::taiDanhSachSuKien` | GET `/api/events`, `/api/event-types` | SuKien/LoaiSuKienController | SuKienService | SuKien, LoaiSuKien, BanToChuc |
| Chi tiết sự kiện | `pages/su-kien/chi-tiet.html` | Chi tiết + loại vé | `sukien.js::xemChiTietSuKien`, `donve.js::taiLoaiVeTheoSuKien` | GET `/api/events/{id}`, `/api/events/{id}/ticket-types` | SuKien/LoaiVeController | SuKienService, LoaiVeService | SuKien, BTC, LoaiVe |
| Sự kiện BTC | `pages/ban-to-chuc/su-kien-cua-toi.html` | Card scope mine | `sukien.js::taiDanhSachSuKien/huySuKien` | GET `/api/events/mine`; POST cancel | SuKienController | SuKienService | SuKien, BanToChuc |
| Tạo sự kiện | `pages/ban-to-chuc/tao-su-kien.html` | Form event/select loại | `sukien.js::taoSuKien/taiLoaiSuKien` | POST `/api/events`; GET types | SuKien/LoaiSuKienController | SuKienService | SuKien, BanToChuc, LoaiSuKien |
| Tìm địa điểm | `pages/dia-diem/danh-sach.html` | `dd-tukhoa`, `ds-diadiem` | `diadiem.js::taiDiaDiem` | GET `/api/venues` hoặc `/all`; lọc local | DiaDiemController | DiaDiemService | DiaDiem |
| Đặt địa điểm | `pages/dia-diem/dat-dia-diem.html` | Input `dat-*`, thiếu `dat-tc` | `diadiem.js::datDiaDiem` | POST `/api/venue-bookings` dự định; hiện return trước gửi | DatDiaDiemController | DiaDiemService | SuKien, DiaDiem, DatDiaDiem |
| Lịch đặt | `pages/dia-diem/lich-dat.html` | Bảng `ds-dat` | `diadiem.js::taiDatDiaDiem` | GET `/api/venue-bookings` QL/QT | DatDiaDiemController | DiaDiemService | DatDiaDiem, SuKien, DiaDiem |
| Tạo đơn | `pages/don-ve/tao-don.html` | Select `don-loai`, input `don-sl` | `donve.js::taoDon` + khởi tạo event/type | GET events/types; POST `/api/orders` | SuKien, LoaiVe, DonVeController | Các service tương ứng | LoaiVe, SuKien, DonVe, ChiTietDonVe |
| Đơn của tôi | `pages/don-ve/don-cua-toi.html` | `ds-don`, filter, counter | `donve.js::taiDonCuaToi/huyDon/thanhToanDon` | GET `/api/orders/me`; POST cancel/payments | DonVe/ThanhToanController | DonVeService, ThanhToanService | DonVe, ChiTiet, Type, Payment |
| Chi tiết đơn | `pages/don-ve/chi-tiet-don.html` | `ds-chitiet` | `donve.js::xemChiTietDon` | GET `/api/orders/{id}` | DonVeController | DonVeService | DonVe và quan hệ |
| Thanh toán | `pages/thanh-toan/thanh-toan.html` | `tt-don` | `donve.js::thanhToan/thanhToanDon` | POST `/api/payments` | ThanhToanController | ThanhToanService, PayOsService | DonVe, ThanhToan, LoaiVe, Ve |
| Giao dịch | `pages/thanh-toan/giao-dich.html` | `ds-giaodich`, thống kê | `donve.js::taiGiaoDich/taiThongKeThanhToan` | GET order; GET dashboard | DonVe/ThongKeController | DonVeService, ThongKeService | ThanhToan, DonVe |
| Hoàn tiền | `pages/thanh-toan/hoan-tien.html` | `ht-*`; nhãn ID lệch contract | `donve.js::hoanTien` | POST `/api/payments/{id}/refund` | ThanhToanController | ThanhToanService | ThanhToan, DonVe, Ve, LoaiVe |
| Quản lý loại vé | `pages/ve/loai-ve.html` | Form `lv-*`, list loại | `donve.js::themLoaiVe/capNhatLoaiVe/taiLoaiVeTheoSuKien` | GET/POST event ticket-types; PUT type | LoaiVeController | LoaiVeService | LoaiVe, SuKien |
| Vé theo đơn | `pages/ve/ve-theo-don.html` | `ds-ve-theodon` | `checkin.js::taiVeTheoDon/taiLichSuXuat` | GET order; GET ticket exports | DonVe/XuatVeController | DonVeService, XuatVeService | DonVe, Ve, XuatVe |
| Tra cứu/check-in | `pages/ve/tra-cuu-check-in.html` | `kq-tracuu`, `kq-checkin` | `checkin.js::traCuuVe/checkIn` | GET by-code; POST `/api/check-in` | Ve/CheckInController | VeService | Ve, LoaiVe, SuKien |
| Xuất/email/history | `pages/ve/xuat-ve.html` | Form `xv-*`, `ds-xuatve` | `checkin.js::xuatVe/taiLichSuXuat` | POST export/email; GET exports | XuatVeController | XuatVeService | Ve, DonVe, XuatVe |
| Tổng quan admin | `pages/quan-tri/tong-quan.html` | Khối số liệu/bảng | `quantri.js::taiThongKe` | GET `/api/admin/dashboard` | ThongKeController | ThongKeService | Các bảng thống kê |
| Báo cáo | `pages/quan-tri/bao-cao.html` | Các vùng `tk-*` | `quantri.js`, `tochuc.js`, `donve.js` | GET dashboard, verification, dashboard event check-in | ThongKe/XacThucController | ThongKeService, XacThucService | Account/event/order/payment/ticket/request |
| Quản lý account | `pages/quan-tri/tai-khoan.html` | `ds-taikhoan` | `auth.js::taiTaiKhoan/khoaTaiKhoan/moKhoaTaiKhoan/doiVaiTroTaiKhoan` | GET accounts; POST lock/unlock; PUT role | TaiKhoanController | TaiKhoanService | TaiKhoan, VaiTro |
| Duyệt tổ chức | `pages/quan-tri/duyet-to-chuc.html` | Số đếm/form ID duyệt | `main.js::renderDuyetToChuc`, `tochuc.js` | GET verification; POST approve/reject | XacThucController | XacThucService | Request, History, BTC, Account |
| Duyệt event | `pages/quan-tri/duyet-su-kien.html` | Card scope manage | `sukien.js::taiDanhSachSuKien/duyetSuKien/tuChoiSuKien` | GET manage; POST approve/reject | SuKienController | SuKienService | SuKien, BanToChuc |
| Quản lý venue | `pages/quan-tri/dia-diem.html` | Form `dd-*`, venue-grid | `diadiem.js::themDiaDiem/capNhatDiaDiem/...` | GET all; POST/PUT venue; DELETE catalog | DiaDiem/QuanTriController | DiaDiemService, QuanTriService | DiaDiem, DatDiaDiem khi kiểm tra xóa |
| Duyệt booking | `pages/quan-tri/duyet-dat-dia-diem.html` | `ds-dat`, thống kê local | `diadiem.js::taiDatDiaDiem/xacNhanDat/tuChoiDat` | GET bookings; POST approve/reject | DatDiaDiemController | DiaDiemService | DatDiaDiem, DiaDiem, SuKien |
| Quản lý category | `pages/quan-tri/loai-su-kien.html` | Form/bảng loại | `sukien.js::taiLoaiSuKien/themLoaiSuKien/capNhatLoaiSuKien` | GET all; POST/PUT type; DELETE catalog | LoaiSuKien/QuanTriController | SuKienService, QuanTriService | LoaiSuKien, SuKien |
| Event/order theo account | `pages/quan-tri/su-kien-don-theo-tai-khoan.html` | Hai bảng kết quả | `quantri.js::taiSuKienCuaTaiKhoan/taiDonVeCuaTaiKhoan` | GET `/api/admin/catalog/accounts/{id}/activity` | QuanTriController | QuanTriService | BTC, SuKien, DonVe |
| Catalog tổng hợp | `pages/quan-tri/du-lieu.html` | Form/bảng `qt-*` | `admin-catalog.js` | Catalog CRUD + type APIs + register | QuanTri/LoaiVe/AuthController | Service tương ứng | Event/BTC/Account/Category/Type |
| Trang lỗi tĩnh | `pages/errors/404.html` | Nội dung/link điều hướng | Không có API nghiệp vụ riêng | Không có | Không có | Không có | Không có |

Nút logout/header trên các trang cùng gọi `wwwroot/js/auth.js::dangXuat`. Trang lỗi 404 có tệp HTML nhưng `Program.cs` chưa cấu hình mọi URL không tồn tại tự chuyển tới tệp đó; StatusCodePages hiện trả JSON cho response lỗi chưa có body. Bảng không coi sự tồn tại file là bằng chứng có fallback route.

<a id="muc-18"></a>

## 18. Bảng truy vết nghiệp vụ

Trong mỗi hàng, sau JS đều qua `wwwroot/js/api.js` và trước bảng dữ liệu đều qua `Data/ApplicationDbContext.cs`. Đây là hai chặng chung để rút gọn bảng; không phải truy cập SQL trực tiếp từ controller hoặc JS.

| Nghiệp vụ | Điểm bắt đầu | Các file đi qua | Database | Kết quả cuối |
|---|---|---|---|---|
| Đăng ký | Form dang-ky | `wwwroot/js/auth.js` → `Features/TaiKhoan/AuthController.cs` → `TaiKhoanService.cs` | TaiKhoan, VaiTro | Account mới, chuyển login |
| Login | Nút dang-nhap | `auth.js` → `Features/TaiKhoan/AuthController.cs` → `TaiKhoanService.cs` → `Common/Auth/JwtService.cs` | Account/role read | JWT/session, trang theo role |
| Logout | Header button | `auth.js` → `Features/TaiKhoan/AuthController.cs` | Không mutation | Xóa phiên browser |
| Hồ sơ/password | Trang tai-khoan | `auth.js` → controller TaiKhoan/Auth → `Features/TaiKhoan/TaiKhoanService.cs` | Account update | Tên mới hoặc yêu cầu login lại |
| Admin account | Trang quản lý account | `auth.js` → `Features/TaiKhoan/TaiKhoanController.cs` → `TaiKhoanService.cs` | Account/role | Bảng mới, khóa/mở/role đổi |
| Gửi xác thực | Form gui-xac-thuc | `tochuc.js` → `Features/XacThucToChuc/XacThucController.cs` → `XacThucService.cs` | Request insert | Trang hồ sơ chờ |
| Duyệt xác thực | Form ID admin | `main.js`/`tochuc.js` → XacThucController → XacThucService | Request delete, History/BTC insert, Account update | Hồ sơ chính thức/role mới |
| Search/category/paging event | Trang chủ/list | `sukien.js` → `Features/SuKien/SuKienController.cs` → `SuKienService.cs` → lọc/render JS | Event/category/BTC read | Card phân trang local |
| Event detail | Link card?id | `sukien.js` → SuKienController/Service; `donve.js` → LoaiVeController/Service | Event/type read | Chi tiết và hạng vé |
| Tạo event | Form BTC | `sukien.js` → `Features/SuKien/SuKienController.cs` → `SuKienService.cs` | Event insert, BTC/type read | Event chờ duyệt |
| Sửa/gửi event | API client ngoài UI | SuKienController → SuKienService | Event update | API có, FE chuyên dụng chưa có |
| Duyệt/từ chối/hủy event | Card manage/mine | `sukien.js` → SuKienController → SuKienService | Event update | Trạng thái đổi, list refresh |
| Category CRUD | Trang loại event | `sukien.js` → LoaiSuKienController trong SuKienController.cs → SuKienService | Category insert/update | Select/chips/bảng mới |
| Tìm venue | Form danh-sach địa điểm | `diadiem.js` → DiaDiemController → DiaDiemService → `sukien.js::chuoiTimKiem` sau response | Venue read active/all | Card khớp tên/địa chỉ |
| Venue CRUD | Trang venue admin | `diadiem.js` → DiaDiemController → DiaDiemService | Venue insert/update | Danh sách mới/ảnh/active |
| Đặt venue | Form bị thiếu tiền cọc; hoặc API trực tiếp | `diadiem.js` dự định → DatDiaDiemController trong DiaDiemController.cs → DiaDiemService | Event/venue/booking | Backend tạo booking; UI hiện bị chặn |
| Duyệt/cancel booking | Bảng ds-dat | `diadiem.js` → DatDiaDiemController → DiaDiemService | Booking update | Badge/state đổi; quyền UI còn lệch |
| Type CRUD | Form lv/qt-ve | `donve.js`/`admin-catalog.js` → `Features/LoaiVe/LoaiVeController.cs` → `LoaiVeService.cs` | Type insert/update | Hạng vé mới |
| Tạo order | Nút taoDon | `donve.js` → `Features/DonVe/DonVeController.cs` → `DonVeService.cs` | Type held update, Order/detail insert | Order chờ + trang chi tiết |
| Xem/hủy order | Trang don-cua-toi | `donve.js` → DonVeController → DonVeService | Order/detail/type | List hoặc release held |
| Tạo payment | Nút thanh toán | `donve.js` → `Features/ThanhToan/ThanhToanController.cs` → `ThanhToanService.cs` → `Infrastructure/PayOS/PayOsService.cs` | Order/payment/type/ticket | Checkout URL hoặc vé miễn phí |
| Webhook/phát vé | PayOS server | PayOsModels → ThanhToanController → ThanhToanService → PayOsService.Verify | Payment/order/type/ticket | Thành công atomically hoặc nghi vấn |
| Refund nội bộ | Form refund | `donve.js` → ThanhToanController → ThanhToanService | Payment/order/ticket/type | Ghi tiền hoàn/trạng thái/tồn |
| Order expiry | Host worker 30s | `Infrastructure/ExpiredOrderWorker.cs` → `Features/DonVe/DonVeService.cs` | Order/type | Hết hạn, trả giữ |
| Event time state | Host worker 1m | `Infrastructure/EventStateWorker.cs` → DbContext | Event | Đang diễn ra/đã kết thúc |
| Tra cứu/check-in | Trang tra-cuu | `checkin.js` → Ve/CheckInController trong `Features/Ve/VeController.cs` → `VeService.cs` | Ticket/type/event | Vé hoặc check-in một lần |
| PDF/email/history | Trang xuat-ve | `checkin.js` → `Features/XuatVe/XuatVeController.cs` → `XuatVeService.cs` → QR/PDF/Email | Ticket/order/history | PDF bytes/SMTP/log; download FE còn thiếu |
| Dashboard/activity | Trang quản trị | `quantri.js` → `Features/ThongKe/ThongKeController.cs` / `Features/QuanTri/QuanTriController.cs` → service cùng nhóm | Aggregate/read | Bảng số liệu admin |
| Catalog/delete | Trang du-lieu | `admin-catalog.js` → QuanTriController → QuanTriService | Danh mục/FK check | Lưu/xóa khi hợp lệ, refresh |

<a id="muc-19"></a>

## 19. Dependency giữa các thành phần

### 19.1. Dependency thực tế

```text
HTML → script nghiệp vụ → api.js → HTTP contract → Controller → Service
       │                    │                       │          ├→ Common
       ├→ helper ở script khác                     └→ Common  ├→ Domain
       └→ DOM/sessionStorage                                  ├→ Data → EF → SQL
                                                              └→ Infrastructure I/O

Program.cs → đăng ký/cấu hình tất cả runtime dependencies
ActiveUserFilter (Common/Auth) → Data → Domain
ExpiredOrderWorker (Infrastructure) → DonVeService → Data
EventStateWorker (Infrastructure) → Data
XuatVeService → QR/PDF/Email
ThanhToanService → PayOsService → HttpClient
Data/Configurations → Domain + EF mapping
Migration/Setup/LocalData/tests → Data/Domain/ứng dụng theo tác vụ
```

Đây là **dependency đang có**, không có quy tắc compiler cưỡng chế feature chỉ được gọi feature nào. Khi phát triển theo cấu trúc hiện tại, nên giữ controller gọi service, business rule phía server, JS qua HTTP, integration ở Infrastructure; đây là hướng dẫn duy trì trách nhiệm, không phải khẳng định có architecture-test đang enforce.

Frontend phụ thuộc contract HTTP (route, field, enum/role), không tham chiếu assembly C#. Backend không gọi DOM. Domain không gọi frontend/controller. Data biết entity; service biết Data. Database bảo vệ ràng buộc lưu trữ; Infrastructure thực hiện I/O ngoài/format; frontend nhận tương tác/render; backend chịu quyền và nghiệp vụ. Tách trách nhiệm theo cách này giải thích vì sao không được tin giá/role từ browser.

### 19.2. Bảng file quan trọng

| File thật | Thuộc phần | Trách nhiệm | Được gọi bởi | Gọi tới |
|---|---|---|---|---|
| `Program.cs` | Host | DI/pipeline/auth/static/API/workers | .NET entry point | Tất cả đăng ký runtime |
| `QLSK.csproj` | Build | net10, package, excludes/publish | dotnet/Docker | Dependency NuGet/static content |
| `appsettings.json` | Config | Section SQL/JWT/PayOS/SMTP/CORS/Order | Configuration host | Options/connection, không là business logic |
| `wwwroot/config.js` | FE config | API origin | HTML, api.js | window.location |
| `wwwroot/js/api.js` | FE shared | Fetch/token/format/escape/response | Mọi handler nghiệp vụ | HTTP/DOM/sessionStorage |
| `wwwroot/js/main.js` | FE shell | Menu/quyền/Enter/duyệt nhanh | HTML DOMContentLoaded | Các hàm feature và DOM |
| `wwwroot/js/compat-multipage.js` | FE routing | Bọc quyền/điều hướng đa trang | HTML | main.js/store/location |
| `wwwroot/js/auth.js` | FE account | Auth/profile/admin account | Các trang auth/tai-khoan/quản trị | apiFetch, session/DOM |
| `wwwroot/js/sukien.js` | FE event | Event/type/render/tìm không dấu | Trang event, script venue | API event/type, DOM |
| `wwwroot/js/diadiem.js` | FE venue | Search/CRUD/booking | Trang venue/admin | apiFetch, chuoiTimKiem, DOM |
| `wwwroot/js/donve.js` | FE commerce | Type/order/payment/refund | Trang order/payment/type | API tương ứng, DOM/location |
| `wwwroot/js/checkin.js` | FE ticket | Lookup/check-in/export | Trang ve | API order/ticket/check-in |
| `wwwroot/js/quantri.js` | FE reporting | Dashboard/activity | Trang quản trị | API dashboard/catalog activity |
| `wwwroot/js/admin-catalog.js` | FE admin | Catalog CRUD | `wwwroot/pages/quan-tri/du-lieu.html` | API catalog/type/register |
| `Common/Models/ApiResponse.cs` | Common | Envelope success/data/code/message | Controller/middleware/filter | JSON serializer |
| `Common/Exceptions/ApiException.cs` | Common | Exception có status/code | Service/integration | Middleware bắt |
| `Common/Exceptions/ApiExceptionMiddleware.cs` | Common | Error mapping/logging | Pipeline | ApiResponse/ILogger |
| `Common/Auth/JwtService.cs` | Auth | Fingerprint và tạo JWT | TaiKhoanService/filter | Options/crypto/claims |
| `Common/Auth/ActiveUserFilter.cs` | Auth | Active/role/password consistency | MVC toàn cục | DbContext account/role |
| `Common/Auth/UserClaims.cs` | Auth | Lấy ID từ claim | Controller | ClaimsPrincipal |
| `Data/ApplicationDbContext.cs` | Data | 15 DbSet và configuration | Service/filter/worker/CLI/test | EF SQL/Domain/mapping |
| `Data/Configurations/LoaiVeConfig.cs` | Mapping | Tồn vé, unique type, rowversion | OnModelCreating | LoaiVe model/EF |
| `Data/Configurations/ThanhToanConfig.cs` | Mapping | Money/link/reference/index | OnModelCreating | ThanhToan model/EF |
| `Features/TaiKhoan/TaiKhoanService.cs` | Backend account | Auth/hash/profile/role/lock | Auth/TaiKhoanController | DbContext, hasher, JWT |
| `Features/XacThucToChuc/XacThucService.cs` | Backend verification | Request/history/organizer approval | XacThucController | Account/BTC/request/history |
| `Features/SuKien/SuKienService.cs` | Backend event | Event lifecycle và category | SuKien/LoaiSuKienController | DbContext event/BTC/category |
| `Features/DiaDiem/DiaDiemService.cs` | Backend venue | Venue CRUD + booking lifecycle | DiaDiem/DatDiaDiemController | DbContext venue/event/booking |
| `Features/LoaiVe/LoaiVeService.cs` | Backend type | Type ownership/inventory/sale | LoaiVeController | DbContext type/event |
| `Features/DonVe/DonVeService.cs` | Backend order | Reserve/cancel/expiry/read | DonVeController/ExpiredOrderWorker | DbContext/transaction/config |
| `Features/ThanhToan/ThanhToanService.cs` | Backend payment | Link/webhook/issue/refund | ThanhToanController | PayOS, DB order/type/payment/ticket |
| `Features/Ve/VeService.cs` | Backend ticket | Read/lookup/check-in | Ve/CheckInController | DbContext ticket/event |
| `Features/XuatVe/XuatVeService.cs` | Backend export | Ownership/PDF/email/log | XuatVeController | QR/PDF/SMTP/DbContext |
| `Features/ThongKe/ThongKeService.cs` | Backend reporting | Counts/groups/revenue | ThongKeController | EF aggregate |
| `Features/QuanTri/QuanTriService.cs` | Backend catalog | Multi-catalog CRUD/activity/delete | QuanTriController | EF + password hasher |
| `Infrastructure/PayOS/PayOsService.cs` | External integration | HMAC/link/webhook verification | ThanhToanService | HttpClient/PayOsOptions |
| `Infrastructure/PayOS/PayOsModels.cs` | Integration model | Link/webhook data shape | Controller/PayOS/payment service | JSON model |
| `Infrastructure/Email/EmailService.cs` | SMTP | Email/PDF attachment | XuatVeService | SMTP + EmailOptions |
| `Infrastructure/QR/QrService.cs` | QR | PNG QR bytes | XuatVeService | QRCoder |
| `Infrastructure/File/VePdfService.cs` | Document | PDF A5 vé | XuatVeService | QuestPDF + entity/QR |
| `Infrastructure/ExpiredOrderWorker.cs` | Background | Chu kỳ 30s, scope/log | Host | DonVeService.HetHan |
| `Infrastructure/EventStateWorker.cs` | Background | Chu kỳ 1m, scope/log | Host | DbContext.SuKiens |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Schema history | Model mốc cho EF diff | EF design-time | Entity mapping metadata |
| `Data/schema.sql` | SQL reference | Script schema song song | Triển khai thủ công nếu chọn | SQL Server; cần đối chiếu migration |
| `tools/QLSK.Setup/Program.cs` | Deploy CLI | Migrate/admin/adopt legacy | dotnet run/script triển khai | AdminProvisioner/LegacyDataMigrator/DB |
| `tools/QLSK.Setup/AdminProvisioner.cs` | Deploy | Tạo/cập nhật admin có kiểm soát | Setup/test | DbContext/hasher |
| `tools/QLSK.Setup/LegacyDataMigrator.cs` | Deploy | Tiếp nhận dữ liệu legacy | Setup CLI | SQL/schema hiện tại |
| `tests/QLSK.Tests/ApiFactory.cs` | Test | Host/DB cô lập/mock/cleanup | API tests | WebApplicationFactory/migrate/test doubles |
| `tests/QLSK.Tests/ApiRegressionTests.cs` | Test | Invariant auth/payment/ticket... | xUnit | HTTP API + xác nhận DB |
| `tests/QLSK.LocalData/LocalEventSeeder.cs` | Local tool | Bộ dữ liệu mẫu explicit | LocalData CLI/tests | Catalog + DbContext |
| `tests/QLSK.LocalData/LocalSqlGuard.cs` | Local tool | Chặn target ngoài local/test | LocalData CLI | Connection target validation |
| `tests/frontend-data.test.cjs` | FE test | Kiểm tra fetch/ảnh/category/UTC | Node test runner | Script FE trong VM/mocks |
| `scripts/version-frontend-assets.py` | Maintenance | Query version bằng SHA256 | Developer/CI thủ công | HTML và CSS/JS |
| `Dockerfile` | Deploy | Build/publish/runtime web | Docker build | .NET SDK/runtime + published app |

<a id="muc-20"></a>

## 20. Lợi ích kiến trúc

| Lợi ích thực tế | Dẫn chứng | Tác dụng với nhóm |
|---|---|---|
| Dễ tìm luồng API | Controller/DTO/service địa điểm cùng `Features/DiaDiem/` | Từ route có thể tìm tiếp quy tắc và query trong một feature |
| Controller gọn | `Features/DonVe/DonVeController.cs` chỉ bind/quyền/call/bọc | Reviewer dễ tách route sai với nghiệp vụ tồn vé sai |
| Quy tắc quan trọng nằm ở server | `DonVeService.Tao` tự tính tiền và giữ vé | Thay input browser không thay giá có thẩm quyền |
| Tái sử dụng nghiệp vụ giữa API và nền | `DonVeService.HetHan` được worker gọi | Quy tắc trả vé giữ không phụ thuộc trình duyệt còn mở |
| Lỗi/phản hồi nhất quán phần lớn | ApiResponse + middleware + ApiBehaviorOptions | Handler frontend dùng chung wrapper và thông báo |
| Auth không chỉ ẩn nút | `[Authorize]`, `ActiveUserFilter`, ownership trong service | Gọi HTTP trực tiếp vẫn bị kiểm tra role/chủ sở hữu |
| Giao dịch liên bảng thuận tiện | DbContext chung trong create order/webhook/refund | Cập nhật payment/order/tồn/vé trong một SQL transaction |
| DB có lớp bảo vệ bổ sung | Configurations: unique/check/FK/rowversion | Một số sai sót hoặc cạnh tranh bị SQL/EF chặn dù service có lỗi |
| External I/O được gom | `Infrastructure/PayOS`, `Email`, `QR`, `File` | Có một chỗ xử lý protocol/chữ ký/format, không chép vào controller |
| Frontend triển khai đơn giản | StaticFiles và API cùng host | Một publish triển khai HTML+API, mặc định không cần cấu hình cross-origin |
| Không phụ thuộc frontend framework để hiểu flow | HTML inline handler → JS → fetch | Sinh viên có thể theo stack từ nút tới service trực tiếp |
| Công cụ triển khai tách runtime | `tools/QLSK.Setup`, `tests/QLSK.LocalData`, excludes trong csproj | Không seed business data/admin ngầm mỗi khi web host khởi động |
| Có hồi quy dựa trên database thật | `tests/QLSK.Tests/ApiFactory.cs`, `ApiRegressionTests.cs` | Có thể kiểm tra migration/constraint/transaction, vượt quá chỉ mock repository |
| FE có bảo vệ dữ liệu thực tế | `diadiem.js::__lanTaiDiaDiem`, test freshness/UTC | Giảm response cũ ghi đè và lỗi đổi thời gian ngoài ý muốn |

Các lợi ích về thay frontend đến từ contract HTTP: một client khác có thể dùng API hiện có. Thay SQL Server không “chỉ đổi connection string”: còn provider, raw SQL migration, rowversion, filtered index và CLI SQL-specific. Tương tự, tách PayOS vào Infrastructure giúp khoanh vùng protocol, nhưng `ThanhToanService` vẫn phụ thuộc concrete model/semantics PayOS.

<a id="muc-21"></a>

## 21. Nhược điểm/hạn chế

Các nhận xét sau là **hiện trạng quan sát trong source**, không phải thay đổi đã thực hiện. Mức ảnh hưởng giúp ưu tiên kiểm tra; không suy diễn đã có sự cố ngoài thực tế.

### 21.1. Chưa khớp UI ↔ API và nghiệp vụ còn thiếu

| Điểm | File căn cứ | Hệ quả/giới hạn |
|---|---|---|
| Form đặt venue thiếu `dat-tc` | `wwwroot/pages/dia-diem/dat-dia-diem.html`, `wwwroot/js/diadiem.js::datDiaDiem` | Return trước POST do null tiền cọc; đây là điểm đứt tĩnh rõ ràng |
| Lịch đặt BTC không có endpoint mine | `diadiem.js::taiDatDiaDiem`, `Features/DiaDiem/DiaDiemController.cs` | GET list chỉ QL/QT; BTC không dùng được luồng “của tôi” như mong đợi |
| Nhiều trang cho QL nhưng API QT-only | `quan-tri/tong-quan.html`, `bao-cao.html`, `dia-diem.html`, `loai-su-kien.html`, `su-kien-don-theo-tai-khoan.html`; controllers tương ứng | Dashboard/catalog/mutation danh mục có thể 403 dù UI cho vào |
| Check-in UI cho BTC và dùng input number | `wwwroot/pages/ve/tra-cuu-check-in.html`, `Features/Ve/VeController.cs` | Sai quyền và khó nhập mã chữ-số; chưa có camera scanner |
| Tra QR đi vào service chỉ tra MaVe | `wwwroot/js/checkin.js::traCuuVe`, `Features/Ve/VeService.cs::Ma` | Nhãn QR không khớp khả năng lookup endpoint |
| PDF thiếu xử lý Blob/download | `wwwroot/js/api.js::apiFetch`, `wwwroot/js/checkin.js::xuatVe` | Backend trả PDF nhưng UI chỉ báo thành công, không tải file |
| Nhãn refund là mã đơn nhưng API nhận payment ID | `wwwroot/pages/thanh-toan/hoan-tien.html`, `donve.js::hoanTien`, `ThanhToanService.HoanTien` | Dễ gửi nhầm ID; reload sau success còn gọi host DOM không tồn tại |
| Refund chỉ ghi nhận nội bộ | `ThanhToanService.HoanTien`, `Infrastructure/PayOS/PayOsService.cs` | Chưa có chuyển tiền remote, lịch sử từng lần/idempotency refund |
| Hủy event không đồng bộ order/vé/booking | `Features/SuKien/SuKienService.cs::Huy`, `ThanhToanService.PhatVe` | Pending order còn có thể hoàn tất, không tự hoàn tiền hoặc thu hồi vé |
| Booking một bản ghi/event | `DiaDiemService.Dat`, `DatDiaDiemConfig.cs` | Đã hủy/từ chối vẫn chặn tạo booking thay thế; không có complete transition |
| Rule sức chứa chưa đồng nhất | `LoaiVeService`, `DiaDiemService.Sua`, `QuanTriService.LuuSuKien`, `SuKienService.Sua` | Không có invariant tổng lượng type<=event; giảm sức chứa venue chưa đối chiếu các event đã đặt; đường sửa admin/BTC có guard khác nhau |
| Giới hạn vé UI không thật sự configurable | `ve/loai-ve.html` có `lv-slmoi`, `LoaiVeDto`, `DonVeService.Tao` | Field không dùng; 20 mỗi dòng khác 20 cả đơn |
| Mật khẩu/đổi mật khẩu mô tả lệch | `TaiKhoan/Dtos.cs`, `api.js::checkMatKhau`, `doi-mat-khau.html`, `auth.js` | Min 6/8 khác nhau; HTML nói giữ phiên nhưng handler logout |
| Một số API chưa có caller UI | PUT/submit event, open/close type, tickets/me/id | API và giao diện chưa phủ nhau; không thể demo mọi action chỉ bằng UI hiện có |
| Audit chưa đủ như tài liệu thiết kế | `SuKien.cs`, `Ve.cs`, `XuatVe.cs`, dịch vụ tương ứng | Không lưu approver event/check-in actor/export actor/file path như tài liệu cũ mô tả |
| Hồ sơ BTC là bằng chứng tồn tại, chưa có trạng thái hoạt động riêng | `Domain/Entities/BanToChuc.cs`, `SuKienService.LayBanToChucId` | Tạo event kiểm tra role/account active và hồ sơ tồn tại; không được nói service kiểm tra cờ BTC active hoặc cờ verified không có trong entity |
| Giá hiển thị từng vé lấy giá loại vé hiện tại | `wwwroot/js/checkin.js::taiVeTheoDon`, `Domain/Entities/Ve.cs` | Ve không có GiaThucTe; snapshot mua nằm ở ChiTietDonVe, giá UI có thể khác giá đã mua nếu loại vé được sửa |

### 21.2. Coupling, trùng lặp và vận hành

`wwwroot/js/donve.js` có 418 dòng, gom loại vé, đơn, thanh toán, refund, phát hành giả lập theo tên hàm và khởi tạo trang. `sukien.js` 276 dòng, `api.js` 264 dòng có cả transport/format/render/validation/hiệu ứng; `diadiem.js` 203 dòng có cả venue và booking. Số dòng tự nó không phải lỗi; vấn đề là nhiều thay đổi nghiệp vụ khác nhau phải chạm một file chung.

`Features/ThanhToan/ThanhToanService.cs` gánh link payment, webhook, phát vé và refund. `Features/QuanTri/QuanTriService.cs` chạm nhiều entity và lặp một phần rule của event/account/type services. Cùng một thao tác sửa event có hai policy không hoàn toàn đồng nhất. Một số file như `ThongKeService.cs`, `XuatVeService.cs` viết nhiều statement trên một dòng, khó đặt breakpoint/review dù không dài theo số dòng.

Header/footer/menu lặp ở 35 HTML; thay nav hoặc quyền dễ bỏ sót. `main.js` còn cơ chế section cũ, `compat-multipage.js` bọc lại, nhiều trang có inline init ngoài init trong JS; cần tránh request/initialization lặp. Global function và helper liên feature như `chuoiTimKiem`/`groupCount` tạo dependency thứ tự nạp khó thấy bằng import graph. Tên tiếng Việt không dấu cho service/entity và tiếng Anh cho routes là quy ước hiện có; alias response và tên cũ (ví dụ biến SoCCCD nhưng đang chứa TenToChuc) làm onboarding khó hơn.

Service trả entity EF làm API phụ thuộc navigation/schema và có nguy cơ trả thừa dữ liệu; `ReferenceHandler.IgnoreCycles` chỉ cắt vòng. Ví dụ event detail include tài khoản BTC; mật khẩu được JsonIgnore nhưng email/metadata khác có thể xuất hiện. DTO response tối thiểu chưa được dùng nhất quán. `Common` hiện nhỏ, chưa phải “siêu thư mục shared”, song auth filter phụ thuộc DB và `api.js` đang là điểm dùng chung rộng.

Tìm event/venue tải list rồi lọc client, nhiều list không có paging server. Khi tăng dữ liệu, response/DOM/cost DB tăng dù màn hình chỉ hiện vài card. Dashboard chạy nhiều query riêng; các phép đếm có thể ở các thời điểm khác nhau. Không có cache phân tán, realtime, background queue cho email, outbox/reconciliation payment, rate limiting được cấu hình trong code, idempotency key tạo order/payment toàn trình hoặc distributed scheduler.

Hai worker ở mọi instance làm tăng cạnh tranh khi scale ngang. Serializable giữ invariant nhưng có thể tăng blocking/deadlock; chưa có retry SQL deadlock tổng quát hoặc chứng cứ kiểm thử tải. Startup không tự migrate là lựa chọn triển khai có kiểm soát nhưng người vận hành phải chạy setup đúng thứ tự. Script schema và migration còn drift, nên tài liệu cũ không đủ làm runbook chính xác.

JWT trong sessionStorage có thể bị đọc bởi script cùng origin; nếu có XSS, token có nguy cơ bị lấy. Code có `esc` nhiều chỗ và kiểm tra URL, nhưng chưa có bằng chứng audit bảo mật toàn bộ HTML/JS hoặc CSP được cấu hình. Logout không thu hồi token server-side. Những nhận xét này mô tả ranh giới hiện tại, không khẳng định có lỗ hổng khai thác cụ thể đã được chứng minh.

<a id="muc-22"></a>

## 22. Khả năng mở rộng

**Mở rộng chức năng:** tương đối thuận lợi ở quy mô hiện tại. Thêm feature theo mẫu controller/DTO/service, model/configuration/migration nếu cần, một script/page và DI trong `Program.cs`. Những nơi chạm chung đã rõ, nhưng feature dùng toàn DbContext nên cần tự kiểm soát coupling và invariant liên bảng.

**Tăng dữ liệu/người dùng:** cần ưu tiên query/filter/paging server cho list lớn, projection response nhỏ và index theo kế hoạch truy vấn thực tế. Địa điểm hiện tải toàn bộ danh sách và tìm JS; chuyển tìm backend sẽ là thay đổi contract, không chỉ sửa một dòng `.filter`. Dashboard có thể tách query theo nhu cầu/cache khi có đo đạc; hiện chưa có cache đó.

**Chạy nhiều instance:** web API bearer và HTML tĩnh thuận lợi hơn server session để scale, nhưng worker cùng chạy, tồn vé Serializable, state DB, payment remote và SMTP là nút cần thiết kế/đo thử. Cần giải pháp điều phối worker, retry có kiểm soát/idempotency và reconciliation trước khi tuyên bố đáp ứng tải lớn. Chưa có load test chứng minh số người dùng đồng thời cụ thể.

**Đổi frontend:** có thể viết client mới gọi 67 API sau khi làm rõ response/enum/quyền. Backend không phụ thuộc DOM nên không cần chuyển toàn bộ business logic. Tuy nhiên client phải xử lý binary PDF, token, response entity hiện tại và những route chưa có UI.

**Đổi database:** thay provider trong `Program.cs` chỉ là bước đầu; SQL Server rowversion, filtered indexes, `datetime2`, raw SQL initial migration, `Data/schema.sql`, Setup/LegacyDataMigrator, test factory và local guard đều cần rà soát. Không có repository/interface persistence để cô lập hoàn toàn SQL provider.

**Đổi payment/email:** Infrastructure đã gom I/O, nhưng phải sửa DI/options/contract cụ thể và cách payment link/reference được ghép trong `ThanhToanService`, kèm webhook/refund tests. Không nên thêm một cổng thanh toán bằng cách chỉ đổi BaseAddress.

Nếu project lớn gấp 5 lần, vẫn có thể giữ một deploy monolith nhưng cần tăng rõ ranh giới feature, contract DTO và kiểm thử; không có căn cứ bắt buộc chuyển microservices chỉ vì số chức năng tăng. Mọi hướng ở mục này là đánh giá/đề xuất, chưa được áp dụng vào code.

<a id="muc-23"></a>

## 23. Cách thêm một chức năng mới

Ví dụ **yêu thích sự kiện**. Hiện tại: **KHÔNG TÌM THẤY TRONG PROJECT**. Toàn bộ tên file/class/endpoint mới trong mục này là **ĐỀ XUẤT — CHƯA TỒN TẠI**, chỉ minh họa cách đi theo cấu trúc thật; không có thay đổi code kèm báo cáo.

| Bước | File/phạm vi đề xuất | Công việc cần làm |
|---|---|---|
| 1. Chốt rule | Tài liệu feature đề xuất | User authenticated được favorite event công khai; thêm lặp là idempotent; bỏ favorite; list của mình; quy định event bị hủy/ẩn |
| 2. Entity mới | **Đề xuất** `Domain/Entities/YeuThichSuKien.cs` | Id, TaiKhoanId, SuKienId, NgayTao; navigation tối thiểu |
| 3. Mapping mới | **Đề xuất** `Data/Configurations/YeuThichSuKienConfig.cs` | FK account/event, unique (TaiKhoanId,SuKienId), policy delete rõ ràng |
| 4. DbContext hiện có | `Data/ApplicationDbContext.cs` | Thêm DbSet; cơ chế apply configurations hiện có nhận mapping; cân nhắc navigation entity cũ nếu cần |
| 5. Migration mới | **Đề xuất** một migration EF mới trong `Migrations/` | Tạo bảng/index; review SQL, snapshot; không sửa migration cũ đã triển khai; đồng bộ schema tham khảo |
| 6. Feature mới | **Đề xuất** `Features/YeuThich/Dtos.cs`, `YeuThichService.cs`, `YeuThichController.cs` | DTO response nhỏ; service inject DbContext, query user từ claims, kiểm tra event, thao tác idempotent, controller mỏng |
| 7. DI/HTTP | `Program.cs` hiện có | AddScoped cho service; `[Authorize]`; ApiResponse/ApiException dùng lại |
| 8. API mới | **Đề xuất** GET `/api/favorites/me`, POST `/api/favorites/events/{eventId}`, DELETE cùng route event | Không nhận TaiKhoanId từ browser; POST trả trạng thái cuối; DELETE chỉ bản ghi của mình |
| 9. Script mới | **Đề xuất** `wwwroot/js/yeuthich.js` | Hàm gọi `apiFetch` hiện có; disabled/loading/error; state các event được thích; cập nhật nút sau success |
| 10. UI hiện có/page mới | `wwwroot/pages/su-kien/chi-tiet.html`, `wwwroot/js/sukien.js`; **đề xuất** `wwwroot/pages/tai-khoan/yeu-thich.html` | Thêm nút DOM và list; không tạo React component; nạp script trên HTML cần dùng |
| 11. Điều hướng/quyền | Header/menu HTML; `main.js`/`compat-multipage.js` nếu dùng tên route logic | Thêm link/page allow; không bắt buộc central router vì hiện dùng URL thật |
| 12. Tests/docs/assets | **Đề xuất** `tests/QLSK.Tests/YeuThichTests.cs`; mở rộng FE tests, `docs/API.md` | Test thiếu JWT, cross-user, event private, duplicate/race và unfavorite; cập nhật version assets |

Luồng đề xuất: click nút yêu thích → `yeuthich.js` → `apiFetch` → endpoint favorites → YeuThichController → YeuThichService → DbContext/bảng mới → ApiResponse → state nút/list. Unique index và service cùng bảo vệ thêm lặp; không cần thêm PayOS/SMTP/worker cho chức năng này. Đây là mô phỏng mở rộng, không được đưa các endpoint này vào bảng API hiện trạng ở mục 16.

<a id="muc-24"></a>

## 24. Cách thêm một module mới

Ví dụ **đánh giá sự kiện**. Hiện tại: **KHÔNG TÌM THẤY TRONG PROJECT**. Các artifact/API mới dưới đây đều **ĐỀ XUẤT — CHƯA TỒN TẠI**. “Module” ở đây là nhóm feature trong cùng monolith, chưa phải dịch vụ deploy riêng.

**Database đề xuất:** bảng `DanhGiaSuKien` với account/event, số sao 1–5, nội dung giới hạn độ dài, thời gian tạo/sửa; unique account/event nếu mỗi người một đánh giá. Chốt điều kiện người mua có vé đã check-in/sự kiện đã kết thúc trước khi triển khai. Thêm `Domain/Entities/DanhGiaSuKien.cs`, `Data/Configurations/DanhGiaSuKienConfig.cs`, DbSet tại file DbContext hiện có và migration mới. DB check stars bảo vệ cùng hàng; đủ điều kiện tham dự là rule liên bảng ở service.

**Backend đề xuất:** `Features/DanhGia/{Dtos.cs,DanhGiaService.cs,DanhGiaController.cs}`. Controller nhận request, claims/role và bọc response. Service đọc `SuKien`/`DonVe`/`Ve` nếu rule cần chứng minh tham dự, kiểm tra ownership khi sửa/xóa, trả projection tối thiểu tránh lộ account entity. Đăng ký scoped ở `Program.cs`, dùng auth/filter/error pipeline hiện có.

| API đề xuất, chưa tồn tại | Mục đích/quyền cần thiết kế |
|---|---|
| GET `/api/events/{eventId}/reviews?page=...&size=...` | List công khai nếu event công khai, server paging |
| POST `/api/events/{eventId}/reviews` | User đủ điều kiện, DTO stars/content; không nhận authorId từ client |
| PUT `/api/reviews/{id}` | Chỉ author hoặc chính sách admin được định nghĩa rõ |
| DELETE `/api/reviews/{id}` | Author/admin theo rule, chốt soft/hard delete/audit |

**Frontend đề xuất:** `wwwroot/js/danhgia.js`, khối danh sách/form trong `wwwroot/pages/su-kien/chi-tiet.html`; nếu quản trị riêng thêm `wwwroot/pages/quan-tri/danh-gia.html`. Handler gọi `apiFetch`, có loading/empty/error, escape nội dung user bằng helper `esc`, cập nhật list và trung bình sao sau response. Không tạo `components/*.jsx`, hook/context hoặc khai báo router framework vì project hiện không có các cấu trúc đó.

**Routing đề xuất:** trang chi tiết đã có query id; lấy eventId để tải review. Page mới dùng link URL HTML; chỉ mở rộng `compat-multipage.js` nếu cần `hienTrang` theo tên logic. Header/footer còn lặp nên cần rà tất cả vị trí liên quan, chạy version asset script sau đổi JS/CSS.

**Test đề xuất:** `tests/QLSK.Tests/DanhGiaTests.cs`, fixture API/SQL độc lập; các case stars=0/6, chưa tham dự, duplicate, cross-user update/delete, hidden event, paging ổn định và chuỗi HTML nguy hiểm ở UI. FE test có thể bổ sung file Node, browser E2E cần hạ tầng mới vì project chưa có. Cập nhật `docs/API.md`, `tests/TEST_PLAN.md` và schema tham khảo.

**Dependency cần giữ:** review feature → DbContext/Domain/Common; đọc entity event/order/ticket để kiểm tra eligibility nếu cần. Không để payment gọi review hay domain entity gọi controller; không cần PayOS/Email/worker trừ khi requirement bổ sung thật. Nếu dùng lại rule từ service khác phải tránh vòng gọi và nêu contract rõ. Hiện code không tự enforce boundary này; reviewer/test phải kiểm soát.

<a id="muc-25"></a>

## 25. Cách sửa một chức năng hiện có

Ví dụ đổi tìm địa điểm, cần phân biệt hai phạm vi:

```text
A. Vẫn lọc dữ liệu ở browser
  wwwroot/pages/dia-diem/danh-sach.html        # input/label/event/host
  wwwroot/js/diadiem.js                       # taiDiaDiem, filter/render/cache
  wwwroot/js/sukien.js                        # chuoiTimKiem nếu đổi quy tắc chung
  wwwroot/css/content.css, responsive.css     # chỉ nếu đổi trình bày
  tests/frontend-data.test.cjs                # kiểm tra hồi quy phù hợp

B. Chuyển tìm/lọc/phân trang lên SQL qua API
  các file FE trên
  Features/DiaDiem/DiaDiemController.cs        # nhận q/page/size (đề xuất sửa contract)
  Features/DiaDiem/DiaDiemService.cs           # query trước materialize, count/page
  Features/DiaDiem/Dtos.cs                    # query/response DTO nếu chọn thêm (đề xuất)
  Data/Configurations/DiaDiemConfig.cs         # index nếu kết quả đo đạc cần
  Migrations/                                # chỉ nếu đổi schema/index
  tests/QLSK.Tests/ApiEndpointTests.cs hoặc test mới phù hợp
  docs/API.md, scripts/version-frontend-assets.py (chạy sau sửa tài nguyên)
```

Hiện GET venues không nhận q và trả list. Nếu thêm search server, cần chốt API trả list hay `{items,total,page,size}`; cập nhật `taiDiaDiem` vì nó đang kiểm tra `Array.isArray`. Bảo toàn phân biệt public active và admin all, luôn kiểm tra quyền ở backend; không cho query `all=true` từ user thường làm lộ inactive. Quy tắc tìm không dấu của JS không tự tương đương SQL collation; phải thử tiếng Việt/đ/hoa-thường và kế hoạch index thực tế.

`__lanTaiDiaDiem` vẫn cần khi response tìm đến không đúng thứ tự. Nếu dùng debounce/input change là thay đổi handler mới, hiện form chủ yếu submit. Không cần sửa payment, PDF, account hoặc Domain entity nếu chỉ đổi cách lọc. Tuy vậy sửa helper `chuoiTimKiem` ảnh hưởng cả tìm sự kiện vì dùng chung; thay form `ds-diadiem` có thể ảnh hưởng trang quản trị vì handler dùng chung. Đây là giới hạn cô lập thực tế, không phải mọi feature hoàn toàn tách biệt.

<a id="muc-26"></a>

## 26. Khả năng làm việc song song

Cấu trúc hỗ trợ 5–7 developer ở mức khá do backend feature folders riêng, nhưng frontend/shared model/migration là điểm cần phối hợp.

| Developer/phạm vi | Khu vực có thể làm tương đối độc lập | Chỗ cần thống nhất |
|---|---|---|
| A — Event | `Features/SuKien`, pages event, `sukien.js` | Category, helper tìm dùng bởi venue, event state/ticket rules |
| B — Type/Order | `Features/LoaiVe`, `Features/DonVe`, pages order/type | `donve.js` dùng chung C; inventory contract với payment |
| C — Payment | `Features/ThanhToan`, `Infrastructure/PayOS`, pages payment | `donve.js`, DonVe/LoaiVe/Ve state và test fixtures |
| D — Venue | `Features/DiaDiem`, `diadiem.js`, pages venue | Capacity event, shared search helper, booking unique |
| E — Auth/Profile | `Features/TaiKhoan`, `Common/Auth`, `auth.js`, pages profile | Claims/role thay đổi ảnh hưởng mọi controller; menu toàn trang |
| F — Verification/Ticket export | `Features/XacThucToChuc`, `Features/XuatVe`, `tochuc.js`, `checkin.js` | Người khác làm check-in có thể chạm checkin.js; role BTC và account |
| G — Dashboard/Catalog | `Features/ThongKe`, `Features/QuanTri`, `quantri.js`, `admin-catalog.js` | Catalog chạm nhiều entity, rule event/account; quyền UI |

Các điểm dễ conflict: `Program.cs` đăng ký DI; `Data/ApplicationDbContext.cs`; entity/navigation dùng chung; `Migrations/ApplicationDbContextModelSnapshot.cs`; `wwwroot/js/api.js`, `main.js`, `compat-multipage.js`; `donve.js`; CSS toàn cục; header/footer 35 HTML; `ApiTestData.cs`/`ApiFactory.cs`; tài liệu API.

Quy trình phù hợp với hiện trạng: thống nhất DTO/route/enum/quyền trước; chia ownership theo feature và phối hợp người phụ trách shared file; mỗi nhánh chỉ migration cho thay đổi của mình rồi đồng bộ snapshot theo thứ tự merge; không ghép raw SQL và migration tùy tiện. PR nên kèm test invariant bị ảnh hưởng, không chỉ test endpoint 200. Có thể dùng branch/worktree riêng để tránh sửa đè; đây là hướng dẫn tổ chức, không thay đổi repo trong lần phân tích.

Khả năng giảm conflict đến từ file controller/service riêng theo feature, không phải “module độc lập tuyệt đối”. Hai người sửa order và payment vẫn cần kiểm tra cùng một invariant `DaBan/DangGiu`, dù Git không báo conflict dòng nào.

<a id="muc-27"></a>

## 27. Khả năng bảo trì và debug

### 27.1. “Tìm địa điểm không ra kết quả”

1. Mở đúng `wwwroot/pages/dia-diem/danh-sach.html`; kiểm tra input `dd-tukhoa`, host `ds-diadiem`, script có nạp thành công/version đúng và console có ReferenceError hay không.
2. Đặt breakpoint `wwwroot/js/diadiem.js::taiDiaDiem`: submit có chạy không, `val` trả gì, host có tồn tại không, `chuoiTimKiem` từ `sukien.js` có được nạp không.
3. Xem Network: GET `/api/venues` hay `/all`, HTTP status/body/envelope, gốc API từ `config.js`, có token bị stale/role sai không. Không tìm `?q=` vì hiện không gửi keyword.
4. Nếu 401/403: kiểm tra token/session, `Program.cs` bearer/authorize và `Common/Auth/ActiveUserFilter.cs`. `/all` chỉ QT; dữ liệu frontend role có thể cũ.
5. Nếu 200: kiểm tra response.data là array, `chuanHoaDiaDiem`, `__diaDiemCache`, số request `__lanTaiDiaDiem`; thử xóa keyword. API có data nhưng filter không khớp là lỗi phía client hoặc dữ liệu, chưa phải backend search.
6. Nếu API rỗng/lỗi: đặt breakpoint `Features/DiaDiem/DiaDiemController.cs::Get/All`, `DiaDiemService.DanhSach`; xem filter active/all và tên DB/provider của `ApplicationDbContext`.
7. Kiểm tra SQL dữ liệu `DiaDiem`, `HoatDong`, tên/địa chỉ, migration ảnh/index và connection target. Dùng truy vấn đọc, không seed hay sửa dữ liệu thật chỉ để “thử”.
8. Nếu DOM có card nhưng không thấy: kiểm tra `hidden`, quyền UI, CSS cascade/venue-grid, ảnh fallback. Đối chiếu `venue-result-count` với số hàng lọc.

### 27.2. Các lỗi điển hình khác

| Triệu chứng | Thứ tự kiểm tra theo project |
|---|---|
| Không gửi được đặt venue | HTML có `dat-tc`? → `num`/validation trong diadiem.js → Network có POST không → role BTC → `DiaDiemService.Dat` ownership/capacity/overlap/unique |
| Tạo đơn báo hết vé | don-loai/don-sl → POST body → `DonVeService.Tao` sale/event state/DaBan/DangGiu → worker expiry → SQL transaction/constraint; không sửa counter bằng tay để vượt rule |
| Đã trả tiền nhưng chưa có vé | Link/payment ID trong order → log webhook có đến không → HMAC/reference/amount/link match → state/expiry/inventory/NghiVan → transaction → GET order lại; return URL không xác nhận payment |
| Refund báo không thấy payment | Xác định payment ID khác order ID → DTO/role → trạng thái payment → cumulative amount/vé đã check-in → DB; HTTP success nhưng UI lỗi thì kiểm tra reload ds-don |
| Check-in không được | Mã chữ-số và input type → lookup MaVe khác QR → role QL/QT → Ve state → event DangDienRa/thời gian UTC → worker 1m → rowversion conflict |
| Export báo thành công không có file | Network Content-Type/bytes PDF → wrapper đang parse JSON → handler không Blob/download; không kết luận backend PDF hỏng chỉ từ UI |
| Đổi role xong bị đăng xuất | `ActiveUserFilter` TOKEN_STALE đúng thiết kế → đăng nhập lại → kiểm tra có hồ sơ BTC chưa; role2 đơn thuần không tạo BTC |
| Số liệu khác nhau giữa refresh | Dashboard query thời điểm khác nhau, free order không có payment, partial/full refund effect; phân biệt grouping client và aggregation server |

Nên ghi lại method/URL/status/code/ID liên quan/thời điểm UTC để ghép với log `ILogger`. Không ghi JWT, password hoặc signature secret. Project hiện chưa có correlation ID tự động, nên không hướng dẫn tra một hệ thống trace chưa tồn tại. Sau khi tìm root cause, thêm test tái hiện đúng invariant hoặc UI contract cần bảo vệ; phạm vi sửa được xác định từ chain thực tế.

<a id="muc-28"></a>

## 28. Testing

### 28.1. Test đang tồn tại

| Loại | File/công cụ | Phạm vi thật và giới hạn |
|---|---|---|
| Rule/unit | `tests/QLSK.Tests/RuleTests.cs` | Chữ ký PayOS mẫu, VND nguyên và cấu hình thiếu; không thay cho toàn bộ nghiệp vụ SQL |
| Setup/in-memory | `tests/QLSK.Tests/DeploymentSetupTests.cs` | Tạo admin ban đầu/idempotent/không tự nâng quyền, validation mật khẩu với EF InMemory |
| Auth/API integration | `tests/QLSK.Tests/AuthTests.cs`, `ApiEndpointTests.cs` | Gọi HTTP qua host test, route/quyền/model; nhiều test happy path chủ yếu assert thành công |
| Regression integration | `tests/QLSK.Tests/ApiRegressionTests.cs` | Đối chiếu effect DB cho role token/private type/check-in/free order/webhook/refund... |
| Data/schema integration | `tests/QLSK.Tests/RealDataTests.cs`, `LocalEventDataTests.cs` | Bảng/schema/dữ liệu local, populate/idempotence/normalization/giữ ID/guards theo test |
| Fixture/test doubles | `tests/QLSK.Tests/ApiFactory.cs`, `ApiTestData.cs` | SQL Server DB `QLSK_Test_<GUID>` riêng, migrate, tạo dữ liệu theo test, PayOS HTTP giả/SMTP thay thế, remove workers, cleanup DB do fixture sở hữu |
| FE JavaScript | `tests/frontend-data.test.cjs` | 4 test freshness event fetch, ảnh venue theo HinhAnh/fallback, category động, giữ UTC milliseconds khi sửa; dùng VM/mocks, không click browser thật |
| Asset consistency | `scripts/version-frontend-assets.py --check` | Kiểm tra URL version HTML khớp nội dung tài nguyên; không kiểm thử nghiệp vụ |
| Kế hoạch test | `tests/TEST_PLAN.md` | Mô tả các kịch bản cần thử; không đồng nghĩa tất cả kịch bản đã được tự động hóa hoặc đã chạy |

Không thấy project Playwright/Cypress/Selenium/browser E2E, load/stress framework hoặc architecture test. Kế hoạch có cạnh tranh mua vé nhưng chưa thấy test tạo nhiều request đồng thời để chứng minh mọi race/locking. EF InMemory không mô phỏng đầy đủ SQL Server transaction/check/index/rowversion, nên các rule inventory cần SQL integration test.

### 28.2. Kết quả thực thi trong lần phân tích này

| Lệnh đã chạy | Kết quả được quan sát |
|---|---|
| `dotnet test tests/QLSK.Tests/QLSK.Tests.csproj --filter "FullyQualifiedName~RuleTests\|FullyQualifiedName~DeploymentSetupTests" --verbosity minimal` | **17 passed, 0 failed, 0 skipped**; build các project liên quan thành công |
| `node --test tests/frontend-data.test.cjs` | **4 passed, 0 failed** |
| `python scripts/version-frontend-assets.py --check` | **0 trang cần cập nhật version** |

Ký tự `\|` trong ô lệnh chỉ để escape bảng Markdown; khi chạy PowerShell dùng dấu `|` nằm bên trong chuỗi filter, không có dấu backslash.

**Không chạy toàn bộ SQL integration suite, browser E2E, PayOS production hoặc SMTP thật trong lần phân tích này.** Không dùng kết quả 17+4 để khẳng định 67 endpoint đều đã được chạy lại. Các sai lệch UI ở mục 21 được xác định bằng mã nguồn, không được gắn nhãn đã tái hiện bằng browser.

`docs/Bao_cao_kiem_thu_va_khac_phuc_QLSK.docx` ghi kết quả lịch sử ngày 27/10/2026: 66 test, 57 endpoint. Đó là bằng chứng cho phiên bản/phạm vi lịch sử được tài liệu mô tả; hiện controller có 67 action và đã thêm catalog/ảnh/công cụ. Không sao chép kết luận “không còn lỗi đã biết” của tài liệu cũ sang trạng thái workspace hiện tại.

### 28.3. Test nên bổ sung khi phát triển tiếp (đề xuất)

Ưu tiên kiểm tra UI-contract cho booking deposit, refund ID, PDF download và quyền từng page; negative ownership/role/active/token stale; concurrency SQL thật cho reserve/webhook/expiry/check-in; event cancel phối hợp pending order; refund lặp; định dạng thời gian/collation tiếng Việt. Khi sửa một rule, test cần assert state/tồn/tổng/side effects thay vì chỉ status 200. Chưa triển khai các test đề xuất này trong nhiệm vụ chỉ viết báo cáo.

<a id="muc-29"></a>

## 29. Đánh giá kiến trúc

Điểm là nhận định kỹ thuật theo mã hiện có và quy mô đồ án, không là benchmark hoặc điểm kiểm định bảo mật. Tách cấu trúc tốt với mức hoàn thiện hành vi UI–API.

| Tiêu chí | Điểm /10 | Căn cứ |
|---|---:|---|
| Tổ chức code | 8 | Feature folders rõ, Data/Domain/Common/Infrastructure có vai trò; vài controller chung file và script nhiều nghiệp vụ làm khó định vị hơn |
| Separation of Concerns | 7 | Controller mỏng, external I/O tách; service vẫn gộp rule/persistence, admin trùng rule, api.js gộp transport/render/validation |
| Maintainability | 6.5 | Chain request dễ theo, có middleware/test; HTML shell lặp, global script, contract và quyền UI lệch cần nhiều kiểm tra thủ công |
| Scalability | 5.5 | SQL transaction/index và stateless bearer là nền tảng; list tải toàn bộ, worker mọi instance, blocking Serializable, chưa load test/distributed coordination |
| Testability | 7 | WebApplicationFactory, SQL DB cô lập, test doubles và FE regression; concrete EF services cần integration, thiếu browser E2E/concurrency coverage |
| Khả năng làm việc nhóm | 7 | Feature backend chia được; Program/DbContext/snapshot/HTML nav/donve.js vẫn dễ conflict và cần thống nhất contract |
| Khả năng mở rộng chức năng | 7.5 | Có mẫu DTO/controller/service/entity/configuration/DI; boundaries mềm và invariant liên feature tăng chi phí khi mở rộng |
| Độ dễ hiểu | 7 | Handler và service trực tiếp, tên nghiệp vụ gần đồ án; alias/tên cũ, script global và một số code nén một dòng giảm độ rõ |

Không nên lấy trung bình điểm để tuyên bố sẵn sàng production. Cần xem trực tiếp các điểm đứt chức năng, chất lượng vận hành và phạm vi kiểm thử phù hợp với mục đích triển khai.

<a id="muc-30"></a>

## 30. Kết luận

**1. Project đang sử dụng kiến trúc gì?** Monolith ASP.NET Core, backend feature-first + Service Layer + các lớp kỹ thuật chung; frontend static HTML đa trang với JavaScript thuần. Có thể gọi monolith được chia theo nhóm module, với boundaries mềm.

**2. Vì sao xác định như vậy?** `Program.cs` host cùng static/API; các controller inject service cùng feature; service truy cập DbContext trực tiếp; toàn ứng dụng chung assembly và database. Kết luận dựa vào call chain, không chỉ tên folder.

**3. Frontend tổ chức thế nào?** `wwwroot/pages` theo nhóm màn hình, `wwwroot/js` theo nghiệp vụ và shared wrapper, CSS/assets chung; biến global + DOM, link HTML/query string và compatibility routing. Không React component/hook/router.

**4. Backend tổ chức thế nào?** 11 feature, 15 controller class/67 action, DTO/service, cùng `Common`, `Domain`, `Data`, `Infrastructure`. Một file có thể chứa hơn một controller.

**5. Business Logic nằm ở đâu?** Chủ yếu `Features/*/*Service.cs`: ownership, trạng thái, thời gian, tồn/giá, transaction; worker event có logic thời gian truy cập DB trực tiếp; một số validation đơn trường nằm ở DTO/DB/frontend.

**6. Database được truy cập từ đâu?** Service, ActiveUserFilter, worker event và các công cụ/test qua `Data/ApplicationDbContext.cs`; EF Core SQL Server sinh SQL. Không có repository riêng, frontend không truy cập SQL.

**7. Một thao tác user qua những lớp nào?** HTML → handler JS → apiFetch → HTTP/auth/filter/DTO → controller → service → DbContext/SQL → service/controller JSON → apiFetch → cache/DOM/điều hướng. PDF/webhook/worker là các ngoại lệ cần nêu riêng.

**8. Mạnh ở điểm nào?** Luồng trực tiếp dễ truy vết, controller gọn, nghiệp vụ server tập trung, DB có constraint/transaction/concurrency, integration và công cụ triển khai tách, có tests.

**9. Yếu ở điểm nào?** UI–API chưa khớp ở một số luồng, frontend global/lặp layout, trả entity và rule admin trùng, boundaries mềm, list thiếu paging server, worker chưa điều phối distributed và một số chức năng mới chỉ hoàn thiện phía API.

**10. Phù hợp với quy mô hiện tại không?** Phù hợp đồ án/ứng dụng nhỏ–vừa cần giao dịch SQL và một đội nhóm nhỏ, với điều kiện xử lý các điểm đứt UI/contract và xác minh tích hợp trước sử dụng thực tế. Đơn giản triển khai và truy vết là lợi thế.

**11. Nếu lớn gấp 5 lần?** Có thể tiếp tục monolith, nhưng cách chia file/schema/query/contract/test phải chặt hơn. Dữ liệu và lưu lượng tăng cần đo đạc paging/index/locking/worker/payment; không có bằng chứng quy mô tải hiện tại để bảo đảm tự động đáp ứng.

**12. Có nên giữ kiến trúc trong tương lai?** Nên giữ nền tảng feature-first/service/EF khi còn phù hợp đội ngũ và deployment, cải thiện nhất quán từng phần. Tách dịch vụ chỉ khi có nhu cầu vận hành/ownership/scale độc lập rõ; báo cáo này không thực hiện refactor hoặc đổi kiến trúc.

<a id="muc-31"></a>

## 31. Nội dung tóm tắt dùng để thuyết trình

### TÓM TẮT ĐỂ THUYẾT TRÌNH

“Hệ thống quản lý sự kiện của nhóm có ba phần trách nhiệm: giao diện trong trình duyệt, backend API xử lý nghiệp vụ và SQL Server lưu dữ liệu. Trong bản triển khai hiện tại, HTML và API cùng được phục vụ bởi một ứng dụng ASP.NET Core. Đây là một ứng dụng nguyên khối, không phải nhiều microservice.

Frontend dùng HTML, CSS và JavaScript thuần. Mỗi trang có form, nút và vùng hiển thị kết quả. Khi người dùng thao tác, hàm JavaScript tương ứng đọc dữ liệu trên trang rồi gọi hàm `apiFetch`. Hàm này gửi HTTP request, gắn JWT nếu đã đăng nhập và xử lý response/lỗi. Frontend không truy cập trực tiếp cơ sở dữ liệu.

Backend được chia theo chức năng trong thư mục `Features`, ví dụ sự kiện, địa điểm, đơn vé và thanh toán. Mỗi nhóm thường có controller, DTO và service. Controller nhận đường dẫn/phương thức HTTP, kiểm tra quyền và gọi service. DTO xác định dữ liệu đầu vào và các điều kiện như bắt buộc hoặc số lượng hợp lệ. Service xử lý quy tắc nghiệp vụ như ai được sửa sự kiện, còn đủ vé hay không và giá tiền phải tính thế nào.

Để truy cập database, service gọi `ApplicationDbContext` của Entity Framework Core. ORM chuyển LINQ thành SQL. Project hiện không có lớp repository riêng. SQL Server lưu 15 bảng và có khóa ngoại, unique, check constraint và rowversion để hỗ trợ tính đúng đắn.

Một ví dụ cụ thể là tìm địa điểm. Người dùng nhập từ khóa ở trang danh sách địa điểm, form gọi `taiDiaDiem` trong `diadiem.js`. Hàm gọi GET `/api/venues`; backend đi qua DiaDiemController, DiaDiemService rồi query bảng DiaDiem. Sau khi JSON trả về, JavaScript mới bỏ dấu và lọc tên hoặc địa chỉ trong trình duyệt. Vì vậy tìm kiếm hiện tại không phải backend nhận từ khóa để chạy SQL search.

Với mua vé, frontend gửi ID loại vé và số lượng tới POST `/api/orders`. Service đọc giá từ database, kiểm tra sự kiện đã duyệt, thời gian mở bán và số vé còn lại. Sau đó hệ thống tăng số đang giữ, tạo đơn và chi tiết trong transaction. Bước này mới giữ chỗ, chưa phát vé. Khi thanh toán thành công qua webhook PayOS được xác minh chữ ký, hệ thống mới chuyển giữ sang đã bán và tạo từng vé QR. Đơn miễn phí có nhánh hoàn tất riêng không tạo payment có tiền.

Đăng nhập sử dụng JWT. Ngoài chữ ký và thời hạn token, backend còn kiểm tra tài khoản đang hoạt động, role và fingerprint mật khẩu có còn khớp database hay không. Quyền ở API và quyền sở hữu trong service mới quyết định có được xử lý; ẩn nút ở giao diện chỉ giúp người dùng nhìn đúng chức năng.

Hệ thống có hai worker: một worker xử lý đơn hết hạn khoảng 30 giây mỗi lượt, một worker cập nhật sự kiện đang diễn ra hoặc kết thúc mỗi phút. Chúng chạy trong host mà không cần người dùng mở trang. QR, PDF, SMTP và PayOS được đặt trong Infrastructure để gom phần tích hợp kỹ thuật.

Cấu trúc này giúp nhóm dễ tìm file, chia công việc và lần theo request. Khi thêm chức năng, nhóm mở rộng entity/configuration/migration nếu cần, thêm DTO/service/controller và nối một script hoặc trang HTML vào API. Ví dụ yêu thích hay đánh giá sự kiện trong báo cáo là hướng dẫn đề xuất, chưa phải chức năng đã có.

Qua đọc mã, nhóm cũng cần phân biệt phần đã hoàn chỉnh với phần còn hạn chế: một số quyền UI chưa khớp API, form đặt địa điểm thiếu trường tiền cọc, xuất PDF chưa xử lý tải tệp ở frontend, hoàn tiền mới ghi nhận nội bộ. Lần phân tích này chạy đạt 17 test quy tắc/thiết lập và 4 test frontend; chưa chạy lại toàn bộ tích hợp SQL hoặc giao dịch ngoài thật. Vì vậy nhóm trình bày đúng phạm vi đã có bằng chứng, và dùng các bảng truy vết để xác định chỗ cần hoàn thiện.”

Khi demo, có thể mở đồng thời `wwwroot/pages/dia-diem/danh-sach.html`, `wwwroot/js/diadiem.js`, `Features/DiaDiem/DiaDiemController.cs`, `Features/DiaDiem/DiaDiemService.cs`, `Data/ApplicationDbContext.cs` và Network browser. Nếu demo mua vé, dùng môi trường test được chuẩn bị riêng, không dùng payment thật chỉ để chứng minh kiến trúc.

<a id="muc-32"></a>

## 32. Bộ câu hỏi giảng viên có thể hỏi và câu trả lời

**1. Vì sao gọi đây là feature-first?** Vì request, DTO và service của cùng nghiệp vụ nằm gần nhau trong `Features/DiaDiem`, `Features/DonVe`... Controller gọi service cùng nhóm; không chỉ do folder có tên Features.

**2. Có phải Clean Architecture không?** Không theo nghĩa nghiêm ngặt. `Features/*Service.cs` biết trực tiếp EF DbContext, `Common/Auth/ActiveUserFilter.cs` biết Data, worker Infrastructure gọi feature; các tầng không độc lập theo dependency rule của Clean Architecture.

**3. Có phải MVC không?** Có ASP.NET Core controller/MVC infrastructure, nhưng không có Razor View. Giao diện là HTML tĩnh gọi Web API, nên mô tả đầy đủ hơn là monolith Web API + frontend đa trang.

**4. Service dùng để làm gì?** `Features/DonVe/DonVeService.cs` kiểm tra event/type/tồn, tính giá và transaction; controller chỉ tiếp nhận/gọi. Quy tắc không bị rải vào các action route.

**5. Vì sao controller không xử lý toàn bộ logic?** Để route/HTTP/quyền và quy tắc xử lý được nhìn riêng; ví dụ DonVeController.Tao một lời gọi service, còn transaction tạo đơn ở DonVeService.Tao. Worker cũng gọi được service mà không cần giả lập HTTP.

**6. Frontend gọi backend bằng gì?** Handler JS gọi `wwwroot/js/api.js::apiFetch`, wrapper dùng fetch tới gốc `window.location.origin` trong `config.js`, gửi JSON/bearer và đọc response.

**7. API trả dữ liệu dạng gì?** Phần lớn `ApiResponse<T>` với success/data/code/message, JSON mặc định camelCase, enum thường là số. Webhook trả success riêng; export trả application/pdf. Một số projection thống kê dùng tên trạng thái chuỗi do code `.ToString()`.

**8. Frontend có React component/state/hook không?** Không. State là biến global như `__diaDiemCache`, `__dsDonCache`; render là innerHTML/textContent của host DOM. Không nên gọi chúng là useState/useEffect.

**9. User tìm địa điểm có gửi từ khóa lên server không?** Không. `diadiem.js::taiDiaDiem` GET `/api/venues` hoặc `/all`, nhận list rồi dùng `chuoiTimKiem` từ `sukien.js` lọc Ten+DiaChi trên browser. SQL chỉ lọc active/all và sắp tên.

**10. Repository nằm đâu?** **KHÔNG TÌM THẤY TRONG PROJECT**. Service inject `Data/ApplicationDbContext.cs` và viết LINQ trực tiếp.

**11. JWT được tạo/kiểm tra ở đâu?** `Common/Auth/JwtService.cs` tạo HMAC token; `Program.cs` cấu hình bearer validation. `ActiveUserFilter.cs` bổ sung active/role/password consistency với DB.

**12. Nếu đổi role mà token cũ còn hạn thì sao?** Role claim không khớp DB nên ActiveUserFilter trả 401 TOKEN_STALE khi request đi tới filter. User cần đăng nhập lại. Endpoint authorization có thể từ chối sớm nếu role token không được phép.

**13. Logout có thu hồi token ở server không?** Không. `auth.js::dangXuat` best-effort POST logout rồi xóa phiên client; controller chỉ message. Chưa có blacklist/refresh/session server.

**14. Hệ thống chống truy cập dữ liệu người khác thế nào?** Ngoài JWT/role, service kiểm tra ownership. `DonVeService.Lay` cho buyer/organizer event/manager; payment chỉ buyer; PDF/email chỉ chủ vé. Không dựa vào ID hoặc nút bị ẩn ở frontend.

**15. Tại sao cần DTO?** DTO giới hạn input và có DataAnnotations, ví dụ `TaoDonDto` chỉ nhận Items/type ID/số lượng, không nhận giá tự chọn. Tuy vậy response entity vẫn dùng nhiều, chưa DTO hóa toàn bộ output.

**16. Chống bán quá vé bằng cách nào?** `DonVeService.Tao` dùng Serializable, kiểm tra `DaBan+DangGiu+qty<=SoLuong`, tăng DangGiu cùng tạo đơn/chi tiết trong transaction. DB check và rowversion bổ sung. Chưa có test tải chứng minh mọi cạnh tranh quy mô lớn.

**17. RowVersion khác transaction thế nào?** RowVersion phát hiện một hàng đã bị thay đổi từ khi EF đọc; transaction bảo đảm nhóm thao tác/đọc-ghi thuộc một phạm vi với isolation. RowVersion không tự bảo vệ overlap hai booking hoặc invariant nhiều bảng.

**18. Click tạo đơn đã phát vé chưa?** Chưa. Nó giữ vé và tạo order/chi tiết. `ThanhToanService` mới phát vé khi webhook hợp lệ hoặc hoàn tất đơn 0 đồng. Không có endpoint phát lại vé riêng.

**19. Vì sao không tin tổng tiền từ client?** Browser có thể bị sửa request. `DonVeService.Tao` đọc giá LoaiVe từ DB, tính lại tổng và lưu DonGia snapshot trong ChiTietDonVe; body client không quyết định tiền.

**20. Webhook khác return URL thế nào?** PayOS server gọi webhook, backend xác minh HMAC/reference/link/amount trước cập nhật. Return URL chỉ đưa browser về trang đơn, không là bằng chứng thanh toán thành công.

**21. Chống webhook lặp ra sao?** `ThanhToanService.XuLyWebhook` kiểm tra MaGiaoDich và ghép MaLienKet trong transaction; SQL có unique filtered reference/link. Order state và kiểm tra phát vé tránh tạo vé lại trong luồng hợp lệ.

**22. Vé miễn phí xử lý thế nào?** `ThanhToanService.Tao` với tổng 0 phát vé và hoàn tất đơn trong transaction, không tạo ThanhToan.SoTien=0 vì DB yêu cầu tiền payment >0. Response không có checkoutUrl.

**23. Refund có chuyển tiền qua PayOS thật không?** Chưa. Service chỉ ghi SoTienHoan/MaHoanTien và điều chỉnh trạng thái/tồn khi hoàn đủ. Refund external API: **KHÔNG TÌM THẤY TRONG PROJECT**.

**24. Worker khác API ở điểm nào?** Worker do host tự chạy theo chu kỳ, không nhận request người dùng. `ExpiredOrderWorker` gọi service hết hạn 30s; `EventStateWorker` query/update event 1m. API chạy khi có HTTP và trả response cho caller.

**25. Vì sao tách Infrastructure?** `PayOsService` gom HTTP/HMAC; QR/PDF/Email gom tạo tài nguyên và SMTP. Service nghiệp vụ gọi chúng mà không phải đặt protocol/format ngay trong controller, dù vẫn phụ thuộc concrete implementation.

**26. Nếu thêm yêu thích thì sửa đâu?** Theo đề xuất mục 23: entity/configuration/DbContext/migration, feature DTO/service/controller/DI, script và page/nút HTML, test và docs. Hiện feature yêu thích **KHÔNG TÌM THẤY TRONG PROJECT**.

**27. Nếu đổi database thì ảnh hưởng gì?** Provider/connection trong Program, Fluent mapping, raw SQL migration/schema, rowversion/filtered index, Setup/LegacyDataMigrator và test/local tools. Không chỉ đổi một connection string.

**28. Nếu đổi frontend thì backend có cần viết lại không?** Business logic có thể giữ vì contract HTTP tách DOM. Client mới phải tương thích route/DTO/enum/JWT, response entity và PDF binary; có thể cần cải thiện contract nhưng không mặc định viết lại toàn backend.

**29. Có nhiều người code thì giúp gì?** Folder Features chia phạm vi event/venue/auth/payment. Nhưng Program/DbContext/snapshot và donve.js/header CSS vẫn dùng chung, nên phải phối hợp DTO/migration/invariant. Không có conflict Git không có nghĩa không có xung đột nghiệp vụ.

**30. Lỗi không có kết quả tìm kiếm bắt đầu debug ở đâu?** UI ID/script → handler/từ khóa → apiFetch/Network → status/auth → controller/service/query → DB → response.data/cache/filter/DOM. Với venue hiện tại phải nhớ từ khóa được lọc sau response.

**31. Tại sao PDF API thành công mà người dùng không tải được?** Backend `XuatVeController.Pdf` trả bytes, nhưng wrapper frontend parse JSON và handler không Blob/download. Đó là vấn đề contract xử lý response phía UI, chưa đủ để kết luận PDF server lỗi.

**32. Model có trạng thái HoanThanh booking thì có chức năng hoàn thành chưa?** Chưa thấy API/service/worker chuyển sang HoanThanh. Enum là khả năng biểu diễn, không chứng minh có luồng thực hiện.

**33. Tất cả test đã chạy lại chưa?** Chưa. Lần phân tích này xác nhận 17 test RuleTests/DeploymentSetupTests, 4 test frontend và asset check. Full SQL integration, browser và cổng thanh toán/SMTP thật chưa chạy lại; báo cáo DOCX 66 test là kết quả lịch sử.

**34. Điểm cần hoàn thiện trước demo UI là gì?** Kiểm tra form booking thiếu deposit, quyền page/API, input/check-in QR, payment ID của refund và PDF download, theo các file/chain ở mục 11/21. Báo cáo chỉ đánh giá, chưa sửa code.

**35. Có nên chuyển microservices ngay không?** Chưa có căn cứ. Với quy mô/đội ngũ hiện tại, giữ monolith feature-first và làm rõ contract/rule/test thường phù hợp hơn. Tách deploy khi xuất hiện nhu cầu độc lập có bằng chứng; không lấy tên folder làm bằng chứng đã có module isolation.




