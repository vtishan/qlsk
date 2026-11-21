# Chương 5. Tài liệu tham khảo

## 5.1. Nguồn khảo sát nghiệp vụ

1. Ticketbox. *Quy chế hoạt động*, phần quy trình mua vé và chọn chỗ. Tham khảo cách thể hiện khu vực ghế, thông tin đặt vé và thanh toán. Truy cập ngày 21/11/2026. [Tài liệu Ticketbox](https://static.ticketbox.vn/static-page/landingpages/vba-quychehoatdong-v2/docs/quy-che-hoat-dong.pdf)
2. Eventbrite Help Center. *Check in attendees at your event with the Eventbrite app for organizers*. Tham khảo thao tác chọn sự kiện, quét vé, xác nhận thủ công và xử lý vé đã dùng. Truy cập ngày 21/11/2026. [Hướng dẫn check-in](https://support.eventbrite.com/hc/en-us/articles/34865085832594-Check-in-attendees-at-your-event-with-the-Eventbrite-app-for-organizers)
3. Eventbrite Help Center. *Create, manage, and check in guests on the Organizer App*. Tham khảo danh sách khách mời và sử dụng vé mời tại cổng. Truy cập ngày 21/11/2026. [Hướng dẫn khách mời](https://support.eventbrite.com/hc/en-us/articles/34862205815698-Create-manage-and-check-in-guests-on-the-Organizer-App)
4. payOS. *Kiểm tra dữ liệu với signature*. Tham khảo cấu trúc thông báo và kiểm tra chữ ký thanh toán. Truy cập ngày 21/11/2026. [Tài liệu tích hợp webhook](https://payos.vn/docs/tich-hop-webhook/kiem-tra-du-lieu-voi-signature/)

## 5.2. Nguồn nội bộ và cơ sở đối chiếu

| Nguồn | Nội dung sử dụng |
|---|---|
| `docs/noidung.txt`, `docs/chức_năng.txt` | Yêu cầu, mô tả vai trò và quy tắc nghiệp vụ. Phạm vi đã triển khai được đối chiếu lại trong từng service. [Phát biểu và quy trình](../noidung.txt) · [Danh mục chức năng](../chức_năng.txt) |
| `Program.cs`, `Features/`, `wwwroot/app/`, `wwwroot/features/` | Công nghệ, xử lý ở backend, route FE/API và điều kiện thực hiện từng trường hợp sử dụng. [Đăng ký dịch vụ](../../Program.cs) · [Danh mục backend](../frontend-standardization/11_BACKEND_FEATURES.md) |
| `Data/Migrations/`, `Data/AppDbContext.cs`, `Data/Configs/`, `Data/seed/` | Lược đồ vật lý, khóa, ràng buộc, quan hệ, cấu hình EF và dữ liệu minh họa. [Migration](../../Data/Migrations/) · [Ánh xạ](../../Data/AppDbContext.cs) · [Seed](../../Data/seed/) |
| `docs/bao-cao/erd/`, `docs/so_do_erd.md`, `docs/bao-cao/uml/` | Sơ đồ ERD theo luồng, bản tra cứu từng khóa ngoại, nguồn Mermaid chi tiết theo cột và PlantUML cho use case, hoạt động, lớp. [Hình ERD](erd/README.md) · [PDF 37 bảng](erd/Tra-cuu-37-bang.pdf) · [Nguồn Mermaid](../so_do_erd.md) · [Sơ đồ UML](uml/) |
| `docs/testcases/`, `test/`, `docs/bao-cao/bang-chung/` | Ma trận theo vai trò, phép kiểm SQL/API, ảnh giao diện, kết quả chạy công khai và xác minh seed. [Mục lục biên bản](../testcases/README.md) · [Mã ca kiểm thử bổ sung](../../test/report_public_smoke.py) · [Bằng chứng đợt lập báo cáo](bang-chung/) |
