# Chương 1. Phát biểu bài toán

## 1.1. Giới thiệu và phát biểu bài toán

Một ban tổ chức muốn mở hai suất diễn trong cùng một đêm nhạc. Hai suất có thể dùng chung hội trường và sơ đồ ghế, nhưng giờ mở bán, số chỗ còn lại và danh sách người đã vào cổng đều khác nhau. Khi một khách chọn ghế A01 cho suất đầu, hệ thống phải biết ghế đó thuộc khu vực nào, suất còn bán hay không, có người khác đang giữ ghế không và giá nào sẽ ghi vào đơn. Cùng lúc, khách của suất sau vẫn có thể mua ghế A01. Tên sự kiện và số ghế chưa đủ để xác định một chỗ tham dự.

Một khó khăn khác nằm ở các quyết định có hiệu lực tại những thời điểm khác nhau. Quản lý có thể duyệt nội dung buổi diễn khi ban tổ chức còn chờ thời điểm công bố. Khách có thể hoàn tất chuyển tiền sau khi chỗ giữ đã hết hạn. Nhân viên tại cổng có thể dùng hai thiết bị để kiểm tra cùng một vé. Sau sự kiện, số tiền khách đã trả, khoản hoàn đã duyệt và tiền thực tế đã chuyển lại có thể mang ba giá trị khác nhau. Bài toán đòi hỏi lưu được nguồn gốc, trạng thái và thời điểm của từng giao dịch.

**Phát biểu bài toán.** Xây dựng hệ thống web QLSK để khách tìm sự kiện hoặc địa điểm, người có tài khoản thuê địa điểm và đặt vé theo suất, ban tổ chức lập lịch, định giá và công bố sự kiện đã qua xét duyệt. Hệ thống cấp vé đủ điều kiện, xác nhận người vào cổng theo phân công, đồng thời cung cấp dữ liệu cho bộ phận quản lý xử lý hỗ trợ, hoàn tiền, đối soát và xuất báo cáo. Mỗi quyết định phải truy về được tài khoản, sự kiện, suất, địa điểm hoặc giao dịch liên quan.

### 1.1.1. Người sử dụng và quyết định của từng bên

| Tác nhân | Công việc cần hoàn thành | Phạm vi dữ liệu |
|---|---|---|
| Khách chưa đăng nhập | Tìm sự kiện theo nhu cầu, so sánh suất và giá, tra lịch địa điểm, tải lịch ICS. | Danh mục, lịch và thông tin đã công khai. |
| Người dùng | Mua vé, xem QR và đơn, yêu thích và xem lại sự kiện, gửi yêu cầu thuê, hoàn, hỗ trợ hoặc đăng ký làm ban tổ chức. | Tài khoản, vé, đơn và yêu cầu của mình. |
| Ban tổ chức (BTC) | Lập sự kiện và phiên bản, cấu hình suất và giá, công bố lần đầu, cấp vé mời, phân công nhân viên, theo dõi doanh thu và phản hồi đối soát. | Các sự kiện thuộc hồ sơ BTC của mình và những hồ sơ tài chính liên quan. |
| Nhân viên sự kiện | Quét mã, tra cứu thủ công hoặc theo dõi người tham dự theo nhiệm vụ được giao. Trong mã nguồn vai trò này mang tên `QuanLySuKien`. | Sự kiện hoặc suất đang có phân công còn hiệu lực. |
| Quản lý hệ thống | Xử lý hàng đợi nội dung, hồ sơ, thuê, hỗ trợ hoặc tài chính theo nhóm được giao. | Quyền nền tảng lưu theo tài khoản. Có nhóm duyệt nội dung, nhóm thuê và nhóm tài chính. |
| Quản trị viên | Quản lý tài khoản và nhóm quyền, địa điểm và khu vực, lịch bảo trì, danh mục, chính sách, cấu hình và nhật ký. | Các nghiệp vụ quản trị và hàng đợi nền tảng. Việc mua, tổ chức hay soát vé vẫn có điều kiện riêng trong từng dịch vụ. |

Ngoài sáu tác nhân con người, cổng payOS cung cấp kết quả thu tiền và máy chủ SMTP chuyển thư xác minh hoặc khôi phục tài khoản khi môi trường đã cấu hình. Một hồ sơ BTC đang chờ duyệt vẫn dùng quyền hiện tại của tài khoản. Một phân công soát vé cho suất A cũng không trao quyền quyết định hồ sơ thuê hoặc đọc doanh thu của BTC. [Vai trò tài khoản](../../Domain/Enums/NhomTaiKhoan.cs) · [Quyền theo phiên](../../Features/Capabilities/CapabilityService.cs)

### 1.1.2. Dữ liệu đầu vào, trách nhiệm xử lý và đầu ra

| Nghiệp vụ | Dữ liệu đầu vào | Xử lý và đầu ra có thể đối chiếu |
|---|---|---|
| Tìm và theo dõi sự kiện | Từ khóa, ngày, danh mục, địa điểm hoặc khoảng giá. | Trả thẻ sự kiện công khai, suất, lịch và chi tiết. Tài khoản đã đăng nhập có thể lưu yêu thích, mở lại tối đa 20 sự kiện gần đây. |
| Xin làm BTC | Thông tin cá nhân hoặc tổ chức, liên hệ, giấy tờ. | Lưu hồ sơ và tệp riêng, chuyển hàng đợi duyệt. Quyết định duyệt, yêu cầu bổ sung hoặc từ chối thể hiện trong lịch sử. |
| Thuê địa điểm | Một địa điểm, giờ bắt đầu/kết thúc, mục đích, chương trình, người phụ trách, chính sách. | Tạo yêu cầu. Khi duyệt, hệ thống kiểm lại lịch, xác định cọc và tạo khung giữ hoặc xác nhận. Người thuê theo dõi hạn và kết quả. |
| Chuẩn bị bán vé | Nội dung phiên bản, địa điểm, lịch diễn, khung bán, sức chứa, loại vé và giá theo khu vực. | Lưu bản nháp, trình duyệt, xem trước và công bố. Trang khách chỉ lấy phiên bản đã công khai. |
| Đặt và thanh toán | Tài khoản, suất, ghế, loại vé, phiên bản chính sách, voucher nếu có. | Chốt giá và phí, tạo đơn và các dòng vé. Đơn tổng bằng 0 có thể phát hành ngay. Đơn có tiền phải thu cần đối soát thành công và giữ ghế hợp lệ để phát hành. |
| Vận hành cổng | Suất được chọn, mã QR hoặc vé tìm thủ công, tài khoản nhân viên. | Trả kết quả tại cổng. Lần vào thành công cập nhật vé và lưu nhân viên, giờ, phương thức. Lần quét lặp trả lại thời điểm đã ghi. |
| Hậu mãi, tài chính và báo cáo | Lý do yêu cầu, vé và giao dịch gốc, hợp đồng, kỳ, các bộ lọc và quyền người xem. | Tạo hồ sơ hoàn, phản hồi hỗ trợ, tính số tiền BTC nhận, duyệt kỳ, tra cứu giải ngân. Báo cáo truy về dữ liệu đơn, lượt vào hoặc hồ sơ trong phạm vi được phép. |

### 1.1.3. Các quy tắc quyết định thiết kế

1. **Duyệt và công bố.** Bản tạo mới đã duyệt có thể được BTC xem trước, công bố hoặc mở lại để sửa. Mở lại đòi hỏi trình duyệt lại. Với sự kiện đã có bản công khai, bản sửa còn chờ duyệt phải tồn tại độc lập để khách tiếp tục thấy nội dung có hiệu lực. Quyết định duyệt lưu cả người duyệt và lý do. [Dịch vụ sự kiện](../../Features/Events/EventService.cs)
2. **Lịch thuê và lịch diễn.** Hai yêu cầu có thể cùng chờ xét một khung giờ. Tại lúc duyệt, dịch vụ khóa theo địa điểm rồi kiểm lại xung đột trước khi ghi lịch. Với khoản cọc dương, cọc bằng 30% giá thuê, làm tròn đến đồng, và hạn cọc là 24 giờ từ lúc duyệt. Nếu BTC chọn hồ sơ thuê đã xác nhận khi lập suất, suất phải nằm trong khung thuê đó. [Xử lý thuê](../../Features/Requests/RequestService.cs) · [Lập suất](../../Features/Events/ScheduleService.cs)
3. **Sức chứa và hạn giữ.** Tổng vé đã mua và ghế đang giữ của một tài khoản tối đa 10 trong một suất. Hệ thống kiểm đồng thời số chỗ của suất, sức chứa khu vực, hạn mức loại vé và tình trạng từng ghế. Chọn một phần cụm bị từ chối. Đơn cần thanh toán giữ chỗ 10 phút. Việc nhấn gửi lại với cùng khóa và cùng nội dung trả lại đơn đã tạo. [Dịch vụ đặt đơn](../../Features/Tickets/OrderService.cs)
4. **Đối chiếu khoản thu.** Hệ thống chốt đơn giá, giảm giá và phí khi đặt. Máy chủ đối chiếu trạng thái nhà cung cấp, số tiền và tham chiếu trước khi cấp vé. Nếu xác minh được khoản thu đúng nhưng giữ chỗ đã mất hiệu lực, hệ thống ghi nhận tiền đã thu và mở yêu cầu xét hoàn. Trường hợp số liệu thu không khớp chuyển sang trạng thái nghi vấn. [Xử lý thanh toán vé](../../Features/Tickets/OrderPayments.cs)
5. **Quyền tại cổng.** Quyền quét mã, quyền xác nhận thủ công và quyền xem danh sách là ba quyền riêng. Vé phải thuộc đúng suất và có trạng thái đã phát hành. Chỉ một lần xác nhận thành công được ghi cho cùng một vé. [Dịch vụ vào cổng](../../Features/Admissions/AdmissionService.cs)
6. **Hoàn tiền và khóa sổ.** Để gửi yêu cầu tự hủy vé, sự kiện phải cho phép, suất còn ít nhất 72 giờ, vé thuộc người gửi, còn ở trạng thái đã phát hành và có khoản thực thu dương. Số tiền xét hoàn tính đến phần giảm giá. Kiểm soát tổng số tiền tránh duyệt vượt khoản thu. Phần tài chính khóa số liệu theo kỳ để BTC phản hồi và người có quyền xét duyệt. [Dịch vụ hoàn](../../Features/Requests/RefundService.cs) · [Lập kỳ đối soát](../../Features/Finance/FinanceSettlements.cs)

### 1.1.4. Tình huống nghiệm thu xuyên suốt

Giả sử BTC muốn bán hai ghế cho một suất và giá mỗi ghế là 100.000 đồng. BTC chuẩn bị suất, khu vực và loại vé rồi gửi xét duyệt. Sau khi quản lý duyệt lần đầu, người thử mở liên kết công khai bằng tài khoản khách để kiểm tra sự kiện còn ở trạng thái riêng. BTC xem trước rồi xác nhận công bố. Khách lúc này mới có thể tìm thấy suất trên danh mục. Khả năng mua còn phụ thuộc khung giờ bán và lượng tồn.

Người mua đăng nhập và chọn hai ghế. Trong ví dụ giả định phí dịch vụ bằng 0, hai ghế không thuộc cụm, mã giảm 10% còn đủ lượt và trần giảm ít nhất 20.000 đồng. Tiền vé là 200.000 đồng, tiền giảm là 20.000 đồng và tiền cần thu là 180.000 đồng. Một tài khoản khác thử chọn cùng ghế khi đơn còn hạn phải nhận thông báo ghế đã giữ. Sau đối soát hợp lệ, người mua có hai vé riêng. Nhân viên quét một vé hai lần chỉ làm tăng số lượt vào thành công thêm một. Khi xem kết quả, BTC có thể đối chiếu số vé đã cấp, số người thực tế vào và tiền thu qua các nguồn dữ liệu khác nhau. Những con số này minh họa phép tính cho một ca nghiệm thu.

## 1.2. Khảo sát website và chuyển quan sát thành mô hình

Khảo sát dùng văn bản và hình minh họa công khai của Ticketbox, Eventbrite và tài liệu tích hợp payOS. Với mỗi thao tác, báo cáo xác định đối tượng người dùng chọn, quyết định hệ thống cần ghi và khóa liên kết cần có. Các bảng dưới đây là thiết kế của QLSK, suy ra từ nghiệp vụ rồi kiểm lại bằng migration của dự án.

| Nguồn và quan sát | Yêu cầu rút ra cho QLSK | Dữ liệu và chức năng hiện có |
|---|---|---|
| Ticketbox minh họa bước chọn chỗ theo khu vực và màn hình tổng hợp loại vé, số lượng và tổng tiền trước thanh toán. [Quy chế, phần hướng dẫn mua vé](https://static.ticketbox.vn/static-page/landingpages/vba-quychehoatdong-v2/docs/quy-che-hoat-dong.pdf) | Người mua cần thấy chỗ và giá tương ứng với suất đang chọn. Hệ thống phải lưu giá tại thời điểm giao dịch để mở lại đơn về sau. | `SuatDien`, `KhuVuc`, `Ghe`, `LoaiVe` phục vụ sơ đồ và giá. `DonHang`, `ChiTietDonHang`, `GiuGhe` lưu quyết định đặt. |
| Eventbrite mô tả hai cách xác nhận khách đến là quét QR và tìm vé thủ công. Nhân viên chọn sự kiện và cần quyền soát vé. Vé đã dùng cho kết quả riêng khi quét lại. [Hướng dẫn check-in](https://support.eventbrite.com/hc/en-us/articles/34865085832594-Check-in-attendees-at-your-event-with-the-Eventbrite-app-for-organizers) | Phân công cần có phạm vi. Mỗi lần vào thành công cần xác định được vé, suất, người thao tác và thời điểm. | `PhanCongSuKien`, `Ve`, `NhatKyCheckIn`. Màn hình tại cổng tách quét mã, xác nhận thủ công và xem nhật ký. |
| Eventbrite cho phép quản lý danh sách khách mời, gửi thông tin vé khi có email và dùng mã vé để vào cổng. [Hướng dẫn quản lý khách mời](https://support.eventbrite.com/hc/en-us/articles/34862205815698-Create-manage-and-check-in-guests-on-the-Organizer-App) | Lời mời có vòng đời riêng. Khi vé đã cấp, cổng có thể dùng chung quy tắc kiểm vé bán và vé mời. | `VeMoi` lưu ngữ cảnh lời mời, `Ve` giữ mã để soát. BTC có thao tác phát hành, thu hồi và mở PDF. Việc chuyển thư hoặc SMS tới khách cần kiểm chứng theo kênh triển khai. |
| payOS mô tả dữ liệu webhook có mã đơn, số tiền và chữ ký cho phép bên nhận kiểm tra tính toàn vẹn. [Tài liệu chữ ký webhook](https://payos.vn/docs/tich-hop-webhook/kiem-tra-du-lieu-voi-signature/) | Cần tách sự kiện thông báo khỏi giao dịch thu. Một thông báo gửi lại phải truy về cùng giao dịch để tránh phát hành vé lần hai. | `GiaoDichThanhToan`, `WebhookThanhToan`, `DonHang` hoặc `YeuCauThue`. Dịch vụ ghi kết quả sau khi xác minh và tra trạng thái. |

Phần thuê địa điểm, duyệt BTC và đối soát thu nhập xuất phát từ yêu cầu nội bộ QLSK. Chẳng hạn, danh từ “địa điểm” cho một thực thể vật lý; “yêu cầu thuê” cho một hồ sơ có chủ thể và trạng thái; “giữ lịch” cho một bản ghi khoảng thời gian. Tách ba đối tượng này giúp quản lý từ chối một hồ sơ mà vẫn tra được địa điểm, lịch bảo trì và lịch sử xét duyệt. [Yêu cầu nghiệp vụ](../noidung.txt) · [Lược đồ triển khai](../../Data/Migrations/000-initial-schema.sql)
