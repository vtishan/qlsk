# QLSK — Kế hoạch nâng cấp dựa trên mã nguồn hiện có

> Đặc tả phân tích tại nhánh `main` của `vtishan/qlsk`, ngày 19/11/2026. **Chỉ cập nhật tài liệu; chưa đổi code hoặc database.**
>
> Tài liệu: [01. Hiện trạng và khoảng cách](01-CURRENT-GAP.md) · [02. SRS và quyền](02-TARGET-SRS.md) · [03. Thiết kế nâng cấp và kiểm thử](03-IMPLEMENTATION-PLAN.md).

## Mục tiêu

Giữ lại ứng dụng hiện tại sử dụng **ASP.NET Core .NET 10 + EF Core/SQL Server + HTML/CSS/JS tĩnh**, nâng cấp an toàn để đáp ứng mô hình quản lý sự kiện đa suất diễn, vé ghế, phân quyền nhân viên của ban tổ chức, PayOS, hoàn tiền và quản lý thuê địa điểm.

## Quy ước năm vai trò

| Tên nghiệp vụ | Mã dùng trong yêu cầu | Quyền |
|---|---|---|
| Người dùng | `nguoidung` | Mua và quản lý vé; yêu cầu trở thành BTC |
| Ban tổ chức | `bantochuc` | Sở hữu sự kiện; tạo và quản lý nhân sự |
| Quản lý sự kiện (tài khoản con) | `quanlysukien` | Chỉ làm việc trên sự kiện/chức năng được BTC cấp |
| Quản lý hệ thống | `quanly` | Xét duyệt và giám sát nghiệp vụ toàn nền tảng |
| Admin | `admin` | Cấu hình, tài khoản, an toàn và quyền tổng thể |

**Guest không phải role DB.** Tên role trong code hiện hữu phải được chuyển đổi có migration; không xóa/đổi role ID trực tiếp trên production.

## Thứ tự đọc và triển khai

1. Xem báo cáo hiện trạng có dẫn file.
2. Xác nhận đặc tả và các tham số hợp đồng/hoàn tiền.
3. Thiết kế migration tương thích với dữ liệu lịch sử.
4. Tách quyền quản trị hệ thống khỏi `QuanLySuKien` **trước khi** cấp role `quanlysukien` mới.
5. Xây event sessions/venue/seat và giữ chỗ 10 phút.
6. Hoàn thiện PayOS, hoàn tiền, check-in và địa điểm.
7. Kiểm thử đồng thời, bảo mật, tài chính và migration.

## Lưu ý bảo mật

- **Không** tái sử dụng role hiện có `QuanLySuKien` để cấp quyền cho nhân viên ban tổ chức: role này hiện được cho phép duyệt hồ sơ/sự kiện/địa điểm và check-in trên phạm vi rộng.
- Mọi endpoint phải kiểm tra **permission + organizer_id + event_id** tại backend, không chỉ role trong JWT.
- Giữ nguyên mã nguồn, database và nhánh `main` cho đến khi từng hạng mục có migration, kiểm thử và đánh giá.

## Trạng thái

Đã đọc cấu trúc repository, các entity chính, controller/service liên quan, README và cấu hình role. Đây là **đánh giá tĩnh có dẫn chứng**, chưa chạy ứng dụng, kiểm thử hoặc migration trên database thật.
