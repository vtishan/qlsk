# Nghiệm thu QLSK · 21/11/2026

**81 ca Đạt, 0 ca Lỗi, 0 ca Bị chặn.** ND-23 đã chạy lại trên Edge sau khi bổ sung lịch sử xem. Hồ sơ hiển thị các sự kiện gần đây, mới nhất trước và mỗi sự kiện xuất hiện một lần. [Biên bản ND-23](01-khach-nguoi-dung.md)

| Vai trò đăng nhập | Đạt | Lỗi | Bị chặn | Mở biên bản |
|---|---:|---:|---:|---|
| Khách | 7 | 0 | 0 | [Khách và người dùng](01-khach-nguoi-dung.md) |
| Người dùng | 23 | 0 | 0 | [Khách và người dùng](01-khach-nguoi-dung.md) |
| Ban tổ chức | 20 | 0 | 0 | [Ban tổ chức và nhân viên](02-ban-to-chuc-nhan-vien.md) |
| Nhân viên được BTC phân công | 6 | 0 | 0 | [Ban tổ chức và nhân viên](02-ban-to-chuc-nhan-vien.md) |
| Quản lý hệ thống theo nhóm quyền | 7 | 0 | 0 | [Quản lý và quản trị](03-quan-ly-quan-tri.md) |
| Quản trị viên | 18 | 0 | 0 | [Quản lý và quản trị](03-quan-ly-quan-tri.md) |

Mỗi mã được tính một lần theo vai trò thực hiện. Biên bản ghi điều kiện, các bước bấm, kết quả mong đợi và kết quả quan sát. Phần đối chiếu cuối mục lục chỉ dùng [noidung.txt](../noidung.txt) và [chức_năng.txt](../chức_năng.txt) làm nguồn yêu cầu. Những tiêu chí chưa có bằng chứng được ghi riêng khỏi 81 ca đã chạy.

Xem thêm [kiểm kê 13 trang giao diện chính thức và 18 ảnh thao tác](04-kiem-ke-trang-giao-dien.md).

## Mở bản bàn giao

- **Bản chạy từ ZIP** mở tại [QLSK](http://127.0.0.1:5834/) hoặc [VÉNUE](http://127.0.0.1:5834/giaodientest/giaodienmau.html). Cả hai lấy tệp từ bản giải nén của [wwwroot.zip](../../wwwroot.zip). [Bản publish](../../obj/qlsk-parity-final) · [Thư mục web root đã kiểm tra](../../obj/handoff-qa/extracted/5d86c016-7vuhd1zu/wwwroot)
- **Đăng nhập thủ công** bằng một trong bảy tài khoản thử tại `%TEMP%\qlsk-ui-c67acc32\handoff-logins.private.json` trên máy QA. Có ba tài khoản quản lý tương ứng hồ sơ/sự kiện, thuê/hỗ trợ và tài chính. Đã đăng nhập qua biểu mẫu Edge trên cổng 5834 với từng tài khoản. Mở các vai trò đối ứng trong hai hồ sơ trình duyệt riêng. Các cổng dùng chung cookie trên 127.0.0.1.
- **Dữ liệu tái lập** dùng riêng LocalDB `QLSK_UI_c67acc32`. Các mã trong biên bản dùng để đọc lại bằng chứng. Khi thử lại hành động thay đổi trạng thái, tạo hồ sơ, đơn, ghế hoặc khung giờ mới đủ điều kiện. Sự kiện 18 và 21 đã kết thúc. Nhân viên của ca NV-05 đã bị thu hồi quyền thủ công.
- **SMTP và thanh toán mô phỏng** dùng cổng 5835. Trên máy QA, chạy `python "$env:TEMP/qlsk-ui-c67acc32/mo-edge-qa.py"` trong PowerShell để mở Edge có chuyển hướng trang thanh toán sang cổng 5836 nội bộ. Trong ứng dụng chọn mở thanh toán, bấm **Xác nhận thanh toán QA**, trở về QLSK, mở lại đơn hoặc hồ sơ và bấm **Kiểm tra thanh toán**. Với ca nhận mã, đọc thư đúng địa chỉ và tiêu đề tại `obj/handoff-qa/work/mail/`. Đối soát đã kiểm tra chữ ký và số tiền mô phỏng. Thư chỉ đến hộp nhận SMTP nội bộ.
- **Khởi động lại cổng 5834** khi host đã dừng bằng lệnh bên dưới, từ thư mục dự án. Lệnh dùng cấu hình QA hiện có và chỉ phục vụ trên máy này.

```powershell
& "$env:TEMP/qlsk-ui-c67acc32/run.ps1" -EnvironmentOnly
$site = (Resolve-Path 'obj/qlsk-parity-final').Path
$assets = (Resolve-Path 'obj/handoff-qa/extracted/5d86c016-7vuhd1zu/wwwroot').Path
dotnet (Join-Path $site 'QLSK.dll') --contentRoot $site --webroot $assets --urls http://127.0.0.1:5834
```

## Đối chiếu giao diện VÉNUE sau phản hồi

Lấy mẫu tại `/giaodientest/giaodienmau.html` làm chuẩn bố cục. Các bộ lọc trên QLSK sử dụng dữ liệu công khai của SQL Server. Cổng `localhost:5000` trong ảnh phản hồi đang trả HTTP 503 do tiến trình Debug chưa cấu hình kết nối QLSK. Bản QA đã cấu hình ở cổng 5834.

| Màn hình | Điều đã so và kiểm tra lại trên Edge |
|---|---|
| Sự kiện | Chọn khoảng giá, thành phố và địa điểm. Mức miễn phí và 100.000 ₫ từ fixture QA lọc đúng, giữ lựa chọn khi tải lại và Back, xóa lọc hoạt động. Liên kết `maxPrice` cũ vẫn mở đúng ngưỡng. [Mẫu](evidence/parity-events/mau-1440.png) · [QLSK desktop](evidence/parity-events/qlsk-1440.png) · [QLSK mobile](evidence/parity-events/qlsk-390.png) |
| Trang chủ và lịch | Bộ lọc tại chỗ, nút chọn nhanh thành phố, Xem thêm giữ khoảng giá và thành phố. Lịch giữ bộ lọc khi đổi tháng, dùng phím để mở ngày, có Chia sẻ và Tải ICS. [Bằng chứng 1440/390 px](evidence/parity-home-calendar/results.json) |
| Địa điểm và hướng dẫn | Giá thuê tối đa và thành phố là danh sách chọn. Địa điểm có nút Chỉ đường, Hướng dẫn có thẻ Quy định và FAQ theo mẫu. Luồng thuê hiện xử lý từng địa điểm. [Địa điểm](evidence/parity-account-venues/venues-375.png) · [Hướng dẫn](evidence/parity-account-venues/guide-375.png) |
| Vé và yêu cầu | Thẻ vé tách vé hiện tại và lịch sử. Vé QA #27 mở QR trên thẻ, tải PDF và in một trang. Vé mời, hỗ trợ và đơn hàng mở bằng menu tài khoản thống nhất. [Thẻ vé mobile](evidence/parity-account-venues/tickets-375.png) |
| Vận hành | Chỉ số ở đầu trang, Lối tắt và Hoạt động gần đây chia hai cột desktop, dồn một cột trên mobile. Tìm và lọc trạng thái đã thử bằng phiên quản trị QA. |

Các kiểm tra bổ sung tập trung vào màn hình thay đổi. Số 81 ca ở đầu mục lục là kết quả của bộ nghiệm thu trước đó cộng ND-23.

## Chức năng trên giao diện và mã ca

| Vai trò / lối vào | Điều đã thử | Mã ca |
|---|---|---|
| Khách · Khám phá, chi tiết sự kiện, lịch, địa điểm | Tìm/lọc, dữ liệu rỗng, xem suất, chia sẻ/lịch ICS, ngày trống và điều hướng | K-01–04, K-07 và smoke Edge |
| Khách · Hướng dẫn, lối vào chức năng riêng | Các tab, FAQ, đăng nhập quay về Vé của tôi, Tab/Escape và màn hình nhỏ | K-05–07 |
| Người dùng · Tài khoản | Đăng ký, điều khoản, đăng nhập/đăng xuất, hiện ẩn mật khẩu, sửa hồ sơ, đổi mật khẩu, xác minh, khôi phục và từ chối mã đã dùng | ND-01–06, ND-22 |
| Người dùng · Hồ sơ và menu cá nhân | Yêu thích, thông báo đã đọc, quyền truy cập, hết phiên, focus và lịch sử xem | ND-07–08, ND-16–18, ND-23 |
| Người dùng · Vé, đơn và hoàn tiền | Chọn ghế, mua vé, mở đơn, tải QR/PDF, tranh ghế, thử mã giảm giá sai, hủy giữ, trả tiền và yêu cầu hoàn | ND-09–12, ND-19, ND-21, QL-TC01 |
| Người dùng · Thuê, hỗ trợ | Chọn ngày giờ, nộp hai lần, nhận kết quả, cọc/đối soát, hủy thuê, gửi và đọc phản hồi | ND-13–15, ND-20, QL-02–03, QL-07, QT-14–15 |
| BTC · Hồ sơ, sự kiện và phiên bản | Nộp giấy tờ, bổ sung, trạng thái, tạo bản nháp, suất, loại vé, trình duyệt, đọc kết quả sửa đổi và hoàn tất | BTC-01–09, BTC-13, BTC-17–19 |
| BTC · Khách mời, nhân sự, tài chính | Phát PDF/thu hồi vé mời, cấp/thu hồi quyền, xem đơn, hợp đồng và giải ngân, xác nhận đối soát; bàn phím, hết phiên | BTC-10–12, BTC-14–16, BTC-20 |
| Nhân viên · Tại cổng | Chọn phạm vi, tìm người tham dự, check-in thủ công, nhật ký, quét lại/sai suất/vé thu hồi và mất quyền khi đang mở form | NV-01–06, BTC-11–12 |
| Quản lý nhóm 1 · Hồ sơ và xét duyệt | Yêu cầu bổ sung, duyệt hồ sơ nộp lại, duyệt sự kiện; người nộp đọc lại | QL-06, QL-08, BTC-17, BTC-19 |
| Quản lý nhóm 2 · Thuê và hỗ trợ | Menu theo quyền, hàng đợi, lọc/duyệt, phản hồi khiếu nại và hai người xin cùng khung giờ | QL-01–03, QL-07 |
| Quản lý nhóm 3 · Hoàn tiền | Duyệt yêu cầu, đọc lý do và số tiền bên người mua, tìm lại bằng bộ lọc trạng thái | QL-TC01, ND-12 |
| Quản trị · Tài khoản, danh mục, địa điểm | Danh sách, phân quyền, khóa/mở khóa, thu hồi phiên, danh mục, banner, địa điểm, khu ghế và chặn/gỡ lịch | QT-01–05, QT-10–13 |
| Quản trị · Chính sách, cấu hình và nhật ký | Soạn/công bố, đối chiếu nhãn sau sửa, giám sát, nhật ký và trường cấu hình | QT-06–08 |
| Quản trị · Tài chính và báo cáo | Hợp đồng phí, đối soát, quyền hành động theo trạng thái, tổng quan giao dịch; xuất và đọc nội dung Excel/Word | QT-01, QT-09, QT-16–18, BTC-14 |

QT-01 ghi nhận các mục quản trị viên mở được. BTC-20 ghi nhận các mục BTC mở được dù danh sách trống. Ví dụ trang Giải ngân mở được, còn chuyển tiền thực tế chưa có bằng chứng. Chi tiết báo cáo thuê gồm 21 dòng trong phạm vi quản trị ở thời điểm xuất. [Xem trước](evidence/manager-admin/report-preview.json) · [Excel](evidence/manager-admin/rentals.xlsx) · [Word](evidence/manager-admin/rentals.docx)

## Bằng chứng chuyển giao

| Chứng từ QA | Bên xử lý | Bên gửi kiểm tra lại |
|---|---|---|
| Hồ sơ BTC #15 → #16 | Quản lý nhóm 1 yêu cầu bổ sung rồi duyệt hồ sơ nộp lại | Người nộp đọc đúng lý do và kết quả. [Ảnh quản lý](evidence/btc-11-quan-ly-nhom1-duyet.png) · [Ảnh người nộp](evidence/btc-10-ho-so-bo-sung-da-duyet.png) |
| Thuê #19 | Quản trị viên duyệt giữ lịch. Ở ca QL-07, quản lý nhóm 2 duyệt #26, yêu cầu #27 cùng giờ bị từ chối khi duyệt | Người thuê #19 thấy cọc 300.000 ₫ và trạng thái xác nhận sau đối soát. [Cọc #19](evidence/guest-user/ND-14.png) · [Xung đột lịch](evidence/manager-admin/QL-07-conflict.png) |
| Đơn #36, yêu cầu hoàn #12 | Quản lý tài chính duyệt 100.000 ₫ | Người mua thấy lý do và trạng thái đã duyệt, chưa lập lệnh chuyển. [Phía người mua](evidence/guest-user/ND-12-approved.png) · [Bộ lọc đã duyệt](evidence/manager-admin/QL-TC01-loc-da-duyet.png) |
| Hỗ trợ #12 | Quản trị viên giải quyết; ca QL-03 kiểm tra riêng quyền xử lý hỗ trợ của quản lý nhóm 2 | Người gửi mở lại và đọc phản hồi. [Phía người gửi](evidence/guest-user/ND-15.png) |
| Sự kiện #21, kỳ đối soát #6 | Sau duyệt và hoàn tất sự kiện, quản trị lập kỳ, BTC xác nhận | Quản trị mở lại thấy BTC xác nhận. [Xác nhận của BTC](evidence/btc-09-doi-soat-btc-xac-nhan.png) |

## Gói frontend, bản mẫu và kiểm tra cuối

| Kiểm tra | Kết quả |
|---|---|
| Release và web root | Build/publish thành công. Cổng 5834 nạp trực tiếp thư mục giải nén sạch. NuGet báo NU1902 và NU1903 ở phụ thuộc bắc cầu System.Security.Cryptography.Pkcs 9.0.11. |
| ZIP | 2.185.602 byte, 76 tệp gồm 16 HTML, 32 JS, 14 CSS, 14 WebP. SHA-256 của ZIP là `5d86c016f5f3c8aefd64fc98d4b8a9925d657dd429697c4becf305715fbb6ec0`. Tên và SHA-256 từng tệp trùng nguồn, bản giải nén và nội dung HTTP từ cổng 5834. [Danh sách SHA-256](evidence/dong-goi/wwwroot-sha256.csv) |
| Import, template và liên kết | Các luồng Edge đã nạp module, CSS, ảnh và template từ ZIP. Bản cập nhật kiểm tra lại 10 chuyển hướng tiêu biểu và đối chiếu nội dung của cả 76 tệp qua HTTP. `/#account` mở màn hình tài khoản. `/views/account.html` trả 301 tới HTML thành phần. [Kiểm tra HTTP và Edge](evidence/dong-goi/zip-browser.json) |
| Mẫu VÉNUE | Dùng mẫu tại `obj/qlsk-publish/wwwroot/giaodientest` làm chuẩn. 14 ảnh trùng từng byte. Bảy module mẫu giữ nội dung sau chuẩn hóa đường dẫn ảnh và dấu xuống dòng. Hai khối JS và một khối CSS được tách ra tệp riêng. Trên Edge 158, mẫu chuẩn và mẫu bung ZIP cho cùng tiêu đề, văn bản chính và số liên kết ở trang chủ, sự kiện, địa điểm, lịch và hướng dẫn. [Mẫu chuẩn](evidence/dong-goi/venue-mau.png) · [Bản ZIP](evidence/dong-goi/venue-zip.png) |
| SQL smoke | `test/qlsk_sql_smoke.py` chạy lại thành công trên cổng 5834. Kiểm tra giao dịch duyệt qua các trang kết quả để tìm đơn vừa tạo khi CSDL QA đã có nhiều bản ghi. [Kết quả smoke](evidence/dong-goi/sql-smoke.txt) |
| Ràng buộc và lược đồ | Lịch sử xem dùng cột nullable `TaiKhoan.SuKienDaXem`. Đối chiếu trước và sau khi thêm cột cho thấy cùng 37 bảng, 37 PK và 75 FK. `test/RangBuoc.sql` đã chạy lại và hoàn tác dữ liệu mẫu. [Kết quả ràng buộc](evidence/dong-goi/rang-buoc.txt) · [Trước nâng cấp](evidence/dong-goi/schema-before.json) · [Sau nâng cấp](evidence/dong-goi/schema-after.json) |

### Lỗi đã sửa và bước kiểm tra lại

- Mở `/views/home.html` chuyển đúng đến template nhưng ảnh tìm dưới `/features/home/assets/images/` và trả 404. Đã sửa đường dẫn ảnh trong template trang chủ và tài khoản. Edge tải được ảnh thật khi mở đường dẫn cũ và vẫn hiển thị trên shell.
- Công bố chính sách rồi mở chi tiết hiển thị `ACTIVE`. Chi tiết giờ dùng nhãn trạng thái như danh sách. QT-06 đã kiểm tra lại trạng thái **Đang áp dụng**.
- Yêu cầu hoàn #12 đã duyệt nhưng bộ lọc quản lý dùng mã `AUTO_APPROVED`. Đã đồng bộ với mã `APPROVED` của dịch vụ. QL-TC01 đã chọn **Đã duyệt** và mở lại đúng yêu cầu. [Trước sửa](evidence/manager-admin/QL-TC01-loc-truoc-sua.png)
- Hợp đồng #8 hiện `CoHieuLuc` trong giao diện. Dịch vụ bổ sung nhãn tiếng Việt cho bốn trạng thái và giữ mã trạng thái API. QT-18 đã mở lại danh sách, chi tiết hợp đồng và kỳ đối soát liên quan. [Sau sửa](evidence/manager-admin/QT-16-detail-fixed.png)
- ND-23 từng báo lịch sử xem chưa được cung cấp. Hồ sơ giờ nạp tối đa 20 sự kiện công khai gần đây từ tài khoản. Đã xem lại sự kiện từ danh sách, tải lại, đăng nhập ở phiên Edge sạch và kiểm tra sự cách ly giữa hai tài khoản. Cơ sở dữ liệu QA đã nâng cấp. Nếu triển khai trên cơ sở dữ liệu QLSK cũ khác, chạy lệnh `--qlsk-init` từ bản mới trước khi khởi động host. [Biên bản ND-23](01-khach-nguoi-dung.md) · [Nâng cấp CSDL](../frontend-standardization/11_BACKEND_FEATURES.md)

## Đối chiếu nội dung và chức năng

Hai bảng quy trình và tiêu chí lấy từ `noidung.txt`. Bảng nhóm chức năng lấy từ `chức_năng.txt`. Mã 01, 02, 03 dẫn tới ba biên bản ở đầu mục lục. “Trực tiếp” chỉ bước có bằng chứng tương ứng. “Một phần” chỉ rõ bước đã làm và điều kiện còn thiếu. SQL là kịch bản smoke trên bản cuối. PAY-SQL là kiểm tra bổ sung ở tầng dịch vụ với SQL Server và bộ mô phỏng thanh toán. [Kết quả PAY-SQL](evidence/dong-goi/thanh-toan-sql.txt)

### 18 quy trình liên vai trò

| Yêu cầu noidung | Mức chứng minh | Ca/bằng chứng | Phần còn thiếu cụ thể |
|---|---|---|---|
| BP-01 Đăng ký tài khoản | Trực tiếp ở QA | 01 ND-01,02; SQL enroll/login | Không thay thế kiểm thử mọi tổ hợp dữ liệu đầu vào |
| BP-02 Đăng ký BTC | Một phần | 02 BTC-01–04,17; 03 QL-06 | SLA tối đa 7 ngày làm việc và lịch nghỉ; từ chối khác yêu cầu bổ sung chưa có ca đầy đủ |
| BP-03 Tạo/duyệt sự kiện | Một phần | 02 BTC-05–09,19 | Nhiều suất/nhiều venue trong cùng sự kiện và điều kiện thuê trước công khai chưa chứng minh |
| BP-04 Duyệt thay đổi | Một phần | 02 BTC-18 | Đã tạo/gửi/duyệt/readback; chưa chụp/assert bản công khai cũ khi pending, nhánh reject và tác động vé đã bán |
| BP-05 Giữ ghế | Một phần | 01 ND-09,11,19; SQL hai request đồng thời 201/409 | Đúng 10 phút, worker không nhả hold mới, hạn mức 6+4, cụm ghế/capacity chưa chứng minh |
| BP-06 Áp voucher | Một phần | 01 ND-21 | Chỉ mã sai; mã hợp lệ, phạm vi, giữ/trả lượt và cạnh tranh lượt cuối chưa chạy |
| BP-07 Thanh toán/phát vé | Một phần, QA mô phỏng | 01 ND-09,10,12,14; PAY-SQL signed checkout/reconcile/replay/forged/amount mismatch | Có service/SQL assert callback lặp không cấp trùng, chữ ký giả bị từ chối, lệch tiền NghiVan; chưa mất mạng/ngân hàng thật |
| BP-08 Thanh toán muộn | Một phần ở service/SQL | PAY-SQL late lost-seat → PAID_OUT_OF_STOCK, refund request, replacement hold preserved | Chưa nhánh còn ghế và số tiền hoàn 100%/ưu tiên xử lý; chưa UI/tiền thực |
| BP-09 Vé mời | Một phần | 02 BTC-10,11; NV-02 | Phát/PDF/thu hồi/check-in có; email/SMS ra ngoài, secure-link hết hạn, nhận vé không tài khoản/liên kết đúng chủ chưa đủ |
| BP-10 Phân công | Trực tiếp cho cấp/thu hồi phạm vi suất | 02 BTC-12; NV-01,05 | Không coi đó là nghiệm thu mọi permission hoặc mọi giao cắt BTC/event |
| BP-11 Check-in | Trực tiếp ở QA | 02 NV-02–04,06; SQL lượt lặp=1 | Đã thủ công, QR lặp/sai suất, thời điểm/nhật ký; không có yêu cầu kết nối ngoài |
| BP-12 Tự hủy/hoàn | Một phần | 01 ND-12; 03 QL-TC01 | Gửi/duyệt 100.000đ chưa chuyển tiền; ranh giới 72 giờ, giá gốc/phí dịch vụ, trạng thái thất bại và đối soát tiền thực chưa chạy |
| BP-13 Hoãn/hủy sự kiện | Chưa chứng minh | Không có ca quyết định hoãn/hủy và đối ứng người mua | Giữ vé khi hoãn, khóa vé khi hủy, thông báo/lịch mới, hoàn tổng tiền |
| BP-14 Thuê/cọc | Một phần | 01 ND-13,14; 03 QL-02,07; QT-14; SQL rental | Cọc 30% và xác nhận sau mô phỏng có; hết 24 giờ nhả lịch/không nhả nhầm chưa chứng minh |
| BP-15 Hủy thuê | Một phần | 01 ND-20; SQL hủy rental | Đã hủy hồ sơ; ba mốc 15/10/5 ngày và hoàn 80/50/0% cọc, tiền chuyển thực chưa chạy |
| BP-16 Đối soát/chi trả | Một phần | BTC-13–14, QT-17, PAY-SQL | Đã xác nhận kỳ #6 ở hai phía và kiểm tra phí trên đơn trả tiền. Còn nhánh quá hạn 48 giờ, quyết định chi trả và xử lý khi chuyển tiền thất bại hoặc lặp. |
| BP-17 Voucher/hợp đồng | Một phần | QT-16, QT-18 tạo và đọc hợp đồng trên UI. PAY-SQL kiểm tra lưu phí tại thời điểm mua. ND-21 thử mã sai | Còn hiệu lực nhiều hợp đồng chồng lấn, áp mã hợp lệ và hạch toán chi phí chiến dịch. |
| BP-18 Hỗ trợ/đánh giá | Một phần | 01 ND-15; 03 QL-03,QT-15 | Hỗ trợ gửi/phản hồi/readback có; đánh giá sau tham dự, chống đánh giá trùng/thay người khác chưa có ca |

### Toàn bộ 36 tiêu chí nghiệm thu, kể cả điều kiện biên

| Tiêu chí noidung | Mức chứng minh | Bằng chứng thực có | Điều kiện chưa chứng minh / giới hạn |
|---|---|---|---|
| TC-01 Công khai và dữ liệu riêng | Một phần | 01 K-01–05; ND-16 | Trang công khai có; chặn dữ liệu riêng chủ yếu thử tài khoản khác/người dùng, chưa đủ tất cả endpoint guest |
| TC-02 Guest không đặt/thuê chính thức | Một phần | 01 K-06 quay lại Vé của tôi sau đăng nhập | Chưa có assertion guest trực tiếp đặt/giữ ghế và gửi thuê bị từ chối trong phạm vi nguồn bằng chứng |
| TC-03 Đăng ký là Người dùng | Trực tiếp ở QA | 01 ND-01; SQL caps sau enroll, hồ sơ cần duyệt mới thành organizer | Không tự có quyền BTC sau đăng ký |
| TC-04 Từ chối/lý do/nộp lại BTC | Một phần | 02 BTC-17; 03 QL-06 | Yêu cầu bổ sung 15→16/duyệt/readback có; nhánh REJECTED chưa chạy |
| TC-05 BTC chưa duyệt không công khai | Một phần | SQL đăng ký→duyệt mới workspace organizer | Chưa tái hiện trực tiếp create/publish bằng tài khoản hồ sơ chưa duyệt |
| TC-06 Hai suất/hai địa điểm/sức chứa riêng | Chưa chứng minh | BTC-06 chỉ một suất mỗi fixture | Cần cùng event có hai suất ở hai venue, đo tồn kho tách biệt |
| TC-07 Pending không đổi bản công khai | Một phần | 02 BTC-18 tạo/gửi/duyệt bản sửa | Chưa đọc bản công khai trong lúc PENDING_UPDATE và đối chiếu trước/sau |
| TC-08 Suspend chặn mua/nội dung | Chưa chứng minh | Mở menu không thay thao tác suspend | Cần quyết định suspend và guest mua/xem lại |
| TC-09 Tranh cùng ghế | Trực tiếp ở QA | 01 ND-11 hai browser; SQL ThreadPool 201/409 | UI chọn cùng ghế, một thành công; SQL chứng minh đồng thời |
| TC-10 Một ghế cụm bị giữ → cả cụm thất bại | Chưa chứng minh | Ghế đơn ND-11 không đủ | Cần cụm nhiều ghế, một bị giữ, kiểm tra không tạo hold một phần |
| TC-11 Đúng 10 phút/worker không nhả hold mới | Chưa chứng minh | ND-19 hủy thủ công không thay worker | Cần ranh giới 10 phút và cạnh tranh hold cũ/mới |
| TC-12 6 đã trả+4 giữ không thêm | Chưa chứng minh | Không assertion quota biên 6+4 | Cần 10 vé hợp lệ trong cùng tài khoản/suất rồi từ chối vé 11 |
| TC-13 Suất khác quota độc lập | Chưa chứng minh | Fixture mỗi event một suất không đủ | Cần cùng event, cùng account, hai suất, quota 10 độc lập |
| TC-14 Bán+mời không vượt capacity | Một phần | 02 BTC-07,10 cấu hình/phát dưới giới hạn | Chưa chạm capacity hoặc cạnh tranh vé bán với vé mời |
| TC-15 Voucher hết lượt/hạn/sai phạm vi | Một phần | 01 ND-21 mã không tồn tại bị từ chối | Không chứng minh ba nhánh hết lượt/hết hạn/sai phạm vi |
| TC-16 Tranh lượt voucher cuối | Chưa chứng minh | Không có assertion tương ứng | Cần hai account đồng thời, limit 1 |
| TC-17 Trả lượt hết hạn/chốt một lần | Chưa chứng minh | Hủy giữ ghế không đối chiếu lượt voucher | Cần voucher reserve/expire/replay receipt và lượt thực dùng |
| TC-18 Webhook lặp không cấp vé hai lần | Trực tiếp ở service/SQL mô phỏng | PAY-SQL gọi signed webhook hai lần sau reconcile; đếm đúng 1 vé; forged signature bị từ chối | Không phải browser/PayOS bên ngoài; chưa mọi tổ hợp callback trùng |
| TC-19 Thanh toán muộn còn ghế | Chưa chứng minh | Simulator thành công đúng hạn không đủ | Cần hết hold, callback trễ, tái cấp nguyên tử và quota |
| TC-20 Thanh toán muộn mất ghế | Một phần ở service/SQL | PAY-SQL ép hold quá hạn, account khác giữ ghế, reconcile PAID_OUT_OF_STOCK; không phát vé; có refund request; hold mới còn PENDING_PAYMENT | Chưa assert số tiền hoàn đủ thực trả/ưu tiên hàng đợi/chuyển tiền; chưa UI |
| TC-21 Mời email chưa tài khoản/check-in trực tiếp | Một phần | 02 BTC-10/PDF/NV-02 | Khách nhận ca check-in đã có account fixture; email chưa account ở BTC-11 bị thu hồi, không phải ca vào cổng thành công; chưa gửi thư ngoài |
| TC-22 Liên kết vé mời đúng chủ sau xác minh | Chưa chứng minh | Phát PDF không thay claim-link | Cần người đúng/sai liên hệ, token hết hạn và claim |
| TC-23 QR lần 2 bị chặn/timestamp đầu | Trực tiếp ở QA | 02 NV-03; SQL lặp cùng timestamp và nhật ký 1 | QR Vé đã check-in, thời điểm gốc; không tăng lượt |
| TC-24 Nhân viên chỉ quét sự kiện giao | Một phần | 02 NV-01,04: đúng suất và QR sai suất | Chưa dùng link suất của event không được giao rồi gửi callback; QR sai suất trong event được giao chỉ chứng minh kiểm tra vé |
| TC-25 Thu hồi ngay/không tự cấp quyền | Một phần | 02 NV-05 backend từ chối khi form còn mở | Chưa thử nhân viên tự POST/click cấp quyền; không suy luận từ thiếu menu |
| TC-26 Nhân viên không duyệt toàn nền tảng | Một phần | 02 NV-01 menu chỉ tại cổng | Chưa thử đủ liên kết/actions duyệt BTC/event/rental/refund bằng nhân viên |
| TC-27 Tự hủy≥72 giờ/đúng giá/phí | Một phần | 01 ND-12; 03 QL-TC01; PAY-SQL: 71 giờ không eligible,120 ngày eligible; yêu cầu refund replay cùngID/trạng thái REQUESTED | Chưa đúng 72 giờ, tách giá gốc/phí, tự duyệt/chuyển tiền thực; kiểm tra service không thay UI |
| TC-28 Vé check-in không tự hoàn/còn khiếu nại | Chưa chứng minh | Check-in và hỗ trợ đã chạy độc lập | Cần chính vé đã check-in thử auto-refund, rồi mở khiếu nại đặc biệt |
| TC-29 Hoãn giữ vé/hủy hoàn đủ | Chưa chứng minh | Không có ca hoãn/hủy đủ đối ứng | Cần hai nhánh thay đổi, vé cũ và số tiền gồm phí |
| TC-30 Không cọc 30%/24 giờ nhả lịch | Một phần | 01 ND-14,03 QL-02 cọc 30%,signed simulator xác nhận | Chưa 24 giờ/worker nhả lịch và người khác chọn lại |
| TC-31 Mốc 15/10/5 ngày,80/50/0% | Chưa chứng minh | ND-20 hủy hồ sơ chưa chứng minh cọc hoàn | Cần đủ ba fixture có cọc thực thu và mốc ngày chính xác |
| TC-32 Phí hợp đồng/snapshot đơn | Một phần ở service/SQL | PAY-SQL tạo hợp đồng 10% hiệu lực; đơn 100.000đ có HopDongId đúng và PhiNenTang 10.000; 02 BTC-14 kỳ 0đ | Chưa sửa hợp đồng sau mua rồi đối soát giữ snapshot cũ; chưa mọi kiểu/phạm vi/hiệu lực chồng lấn |
| TC-33 Payout retry không 2lần | Chưa chứng minh | Mở menu payouts không thay retry | Cần một settlement hai request payout cùng khóa và bằng chứng lệnh/chuyển tiền |
| TC-34 Chưa/failed chuyển không PAID | Chưa chứng minh | ND-12 phân biệt duyệt hoàn không phải settlement PAID | Cần payout PROCESSING/FAILED và đọc settlement hai phía |
| TC-35 Admin/manager đúng thẩm quyền | Một phần | 03 QL-01,QT-02,QT-11; SQL manager group 1 chặn admin/accounts | Đã nhóm 1/2 và tài chính QA; chưa toàn bộ ma trận thao tác/ủy quyền/chống leo quyền |
| TC-36 Reset/tài liệu/quyền/audit | Một phần | 01 ND-06,22; SQL tài liệu chủ/admin/khác 404,CSRF; 03 QT-07 | Reset dùng một lần có SMTP nội bộ; thời hạn 30 phút, mọi loại tài liệu, retention/PII chưa chứng minh |

### 20 nhóm chức năng trong chức_năng.txt

| STT / nhóm | Mức chứng minh | Ca thực có | Phần chưa chứng minh theo yêu cầu gốc |
|---|---|---|---|
| 1 Tài khoản | Một phần | ND-01–06,22; QT-11 | Token 30 phút, mọi tổ hợp trùng liên hệ/đầu vào; SMTP ngoài |
| 2 Phân quyền | Một phần | BTC-12,NV-05; QL-01; QT-02,11 | Ma trận đầy đủ account+BTC+event+session+permission |
| 3 Đăng ký BTC | Một phần | BTC-01–04,17; QL-06 | Nhánh từ chối, SLA7 ngày/lịch nghỉ |
| 4 Quản lý sự kiện | Một phần | BTC-05,08,09,13,19 | Xóa nháp, suspend, hoãn/hủy và đối ứng người mua |
| 5 Duyệt thay đổi | Một phần | BTC-18 | Pending giữ bản cũ; reject không mất bản cũ; tác động vé bán |
| 6 Suất diễn | Một phần | BTC-06,07; K-02,03 | Cùng event nhiều ngày/suất/venue, tồn kho độc lập |
| 7 Địa điểm | Một phần | K-04; QT-04,13; QL-07 | Mọi bộ lọc, sửa/ẩn nơi có hợp đồng và ranh giới lịch |
| 8 Ghế/sơ đồ | Một phần | QT-05; ND-09,11 | Ghế đôi/cụm, atomic nhóm và capacity biên |
| 9 Thuê địa điểm | Một phần | ND-13,14,20; QL-02,07 | Hết 24 giờ, ba mốc hoàn cọc, tiền thực |
| 10 Loại vé | Một phần | BTC-07; ND-09; SQL Standard | VIP/quyền lợi/thời gian bán biên; Invitation không lọt giỏ public chưa assert |
| 11 Đặt vé | Một phần | ND-09,11,19; SQL concurrency | Đúng 10 phút/quota 6+4/suất độc lập/cụm |
| 12 Thanh toán | Một phần, simulator ký | ND-12,14 | PAY-SQL đã kiểm tra webhook lặp, chữ ký sai, lệch tiền và tiền muộn khi mất ghế. Còn nhánh tiền muộn khi còn ghế, mất kết nối và ngân hàng thật |
| 13 Quản lý vé | Một phần | ND-09,10; BTC-10 | PDF hiện chỉ QR, không tên/ghế (ND-10); mọi lịch sử/trạng thái/hủy vé |
| 14 Vé mời | Một phần | BTC-10,11; NV-02 | Gửi email/SMS thật, secure-link/claim/khách không account vào thành công |
| 15 Check-in | Trực tiếp với bộ ca QA đã nêu | NV-01–06 | Chưa chạy mọi tổ hợp cấp quyền theo từng suất và từng sự kiện |
| 16 Khuyến mãi | Một phần | ND-21 | Tạo/áp voucher hợp lệ, giảm%/fixed/trần/minimum, lượt và expiry |
| 17 Hoàn tiền | Một phần | ND-12; QL-TC01 | ≥72 giờ/hoãn/hủy/phí, chuyển/failed/completed có chứng từ |
| 18 Hợp đồng/đối soát | Một phần | QT-16–18; SQL contract; BTC-13,14; PAY-SQL | PAY-SQL kiểm tra chốt phí trên đơn trả tiền. Còn đổi hợp đồng sau mua, quá hạn phản hồi 48 giờ, duyệt giải ngân, retry/thất bại và chứng từ chuyển tiền |
| 19 Thông báo/hỗ trợ | Một phần | ND-08, ND-15, QL-03, QT-15 | Đánh giá sau tham dự, chống gửi trùng và gửi email/SMS ra ngoài cần bằng chứng riêng |
| 20 Quản trị | Một phần | QT-02–13, QT-16–18; SQL reports | Backup/restore,HTTPS/2FA,phê duyệt cấu hình/version/rollback chưa nghiệm thu |

Các quy tắc phần 7 noidung còn phải nghiệm thu ngoài các bảng trên: bảo vệ PII và thời hạn lưu, snapshot giá/discount/phí, hiệu lực hợp đồng chồng lấn, ai tài trợ voucher/cơ sở hoa hồng, cấu hình lịch nghỉ/SLA/48 giờ/khóa sổ, chứng từ hoàn/chi trả và điều kiện hợp đồng ngân hàng. Kiểm tra 37 bảng/khóa/quan hệ do kiểm tra schema riêng xác nhận; danh sách khái niệm nghiệp vụ không được dùng làm lý do đổi tên/thêm/bớt bảng trong đợt bàn giao. Mệnh đề cũ “database chỉ 20 bảng” trong chức_năng.txt là ngữ cảnh mô tả, không phải bằng chứng nghiệm thu schema hiện tại.
