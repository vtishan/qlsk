# Sơ đồ cơ sở dữ liệu QLSK

Mở bản tổng quan để lần theo một vé từ sự kiện đến lượt vào cổng. Khi cần tên cột hoặc muốn biết một bảng có những khóa ngoại nào, chọn bảng từ mục lục của bản tra cứu. [Sơ đồ tổng quan](00-tong-quan.svg) · [Bản tra cứu 37 bảng, 75 khóa ngoại (PDF)](Tra-cuu-37-bang.pdf) · [Tra trên trình duyệt](Tra-cuu-37-bang.html)

| Hình trong mục 2.2.4 | Luồng nghiệp vụ | SVG để phóng to | PNG để chèn Word |
|---|---|---|---|
| 2.2.1 | Từ sự kiện đến lượt vào cổng | [SVG](00-tong-quan.svg) | [PNG](00-tong-quan.png) |
| 2.2.2 | Tài khoản, hồ sơ BTC và giấy tờ | [SVG](luong/01-ho-so.svg) | [PNG](luong/01-ho-so.png) |
| 2.2.3 | Danh mục, sự kiện, phiên bản và suất | [SVG](luong/02-cong-bo.svg) | [PNG](luong/02-cong-bo.png) |
| 2.2.4 | Địa điểm, khu vực, cụm và ghế | [SVG](luong/03-dia-diem.svg) | [PNG](luong/03-dia-diem.png) |
| 2.2.5 | Yêu cầu thuê và lịch địa điểm | [SVG](luong/04-thue.svg) | [PNG](luong/04-thue.png) |
| 2.2.6 | Tài khoản, đơn, chi tiết và loại vé | [SVG](luong/05-dat-ve.svg) | [PNG](luong/05-dat-ve.png) |
| 2.2.7 | Giữ ghế, phát hành, mời và soát vé | [SVG](luong/06-phat-hanh.svg) | [PNG](luong/06-phat-hanh.png) |
| 2.2.8 | Thanh toán, webhook và hoàn tiền | [SVG](luong/07-thanh-toan.svg) | [PNG](luong/07-thanh-toan.png) |
| 2.2.9 | Hợp đồng, đối soát và giải ngân | [SVG](luong/08-doi-soat.svg) | [PNG](luong/08-doi-soat.png) |

Các hình nghiệp vụ chọn những cột cần để giải thích quan hệ. Bản vật lý dùng tên bảng, tên cột và thứ tự cột trong khóa ghép của SQL Server. [DDL của QLSK](../../../Data/Migrations/000-initial-schema.sql) · [Migration bổ sung](../../../Data/Migrations/001-qlsk-features.sql) · [Từ điển cột](../../frontend-standardization/04_DATABASE_INVENTORY.md)
