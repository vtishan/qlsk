# Nghiệm thu Khách / Người dùng

Môi trường: bản Release đã sắp xếp, http://127.0.0.1:5831, Edge headless thực, CSDL QA QLSK_UI_c67acc32; ngày 21/11/2026. Không dùng CSDL vận hành. Ca được khai báo trước khi chạy. Bảng sau phần kịch bản ghi kết quả thực tế. API chỉ chuẩn bị/đối chứng dữ liệu.

| Mã | Vai trò | Điều kiện | Bước bấm tái lập | Mong đợi |
|---|---|---|---|---|
| K-01 | Khách | Có sự kiện SQL | Mở Khám phá; nhập từ khóa không tồn tại; Lọc sự kiện; tải lại; Back/Forward; Xóa bộ lọc | Rỗng đúng, giữ bộ lọc, khôi phục danh sách |
| K-02 | Khách | Có sự kiện SQL | Bấm thẻ sự kiện; mở suất, địa điểm | Hiện lịch/giá/địa điểm |
| K-03 | Khách | Lịch tháng 11/2026 | Mở Lịch; dùng bàn phím Enter ngày 23; Tháng sau/Tháng trước; tải ICS | Đổi lịch đúng, ICS có VEVENT |
| K-04 | Khách | Có địa điểm | Tìm địa điểm; bấm thẻ; xem lịch trống; chọn ngày | Chi tiết và lịch trống có thể thao tác |
| K-05 | Khách | Chưa đăng nhập | Mở Hướng dẫn; từng tab; mở FAQ | Nội dung đầy đủ |
| K-06 | Khách | Chưa đăng nhập | Mở Vé của tôi; Đăng nhập; đăng nhập tài khoản ca ND-01 | Quay lại đúng Vé của tôi |
| K-07 | Khách | Viewport 375px | Mở khám phá/lịch/địa điểm/đăng nhập; Tab, Escape hộp thoại | Không tràn ngang, focus hữu dụng |
| ND-01 | Người dùng | Email mới QA | Đăng ký; bỏ trống; nhập hợp lệ, xác nhận điều khoản; bấm Đăng ký | Lỗi đầu vào rồi tạo tài khoản |
| ND-02 | Người dùng | Tài khoản ND-01 | Hiện/ẩn mật khẩu; đăng nhập sai rồi đúng | Đổi type, báo sai, vào hồ sơ |
| ND-03 | Người dùng | Đã đăng nhập | Hồ sơ > Cập nhật; sửa tên; Lưu; tải lại | Tên mới tồn tại |
| ND-04 | Người dùng | Đã đăng nhập | Đổi mật khẩu; nhập xác nhận sai rồi đúng; đăng xuất/đăng nhập mật khẩu mới | Chặn sai, nhận mật khẩu mới |
| ND-05 | Người dùng | Email QA | Xác minh liên hệ; gửi mã; nhập mã SMTP nội bộ; Xác minh | Email xác minh |
| ND-06 | Người dùng | Email xác minh | Quên mật khẩu; gửi mã; đọc SMTP nội bộ; nhập mã + mật khẩu mới; đăng nhập | Khôi phục dùng một lần |
| ND-07 | Người dùng | Có sự kiện | Bấm yêu thích; mở Sự kiện yêu thích; Bỏ yêu thích | Danh sách thêm/xóa đúng |
| ND-08 | Người dùng | Có thông báo | Mở Thông báo; Đánh dấu đã đọc; tải lại | Trạng thái đọc được lưu |
| ND-09 | Người dùng | Suất có ghế miễn phí | Chọn suất; bỏ trống ghế; chọn ghế; xác nhận đơn; mở đơn | Chặn trống; đúng đơn, vé |
| ND-10 | Người dùng | Vé ND-09 | Vé của tôi > mở vé > QR/PDF; mở tệp PDF | QR và PDF đúng vé |
| ND-11 | Người dùng | Hai tài khoản QA | Hai trình duyệt cùng mở ghế còn trống; cùng chọn; lần lượt xác nhận | Chỉ một người đặt được |
| ND-12 | Người dùng | Vé trả tiền đủ điều kiện QA | Tạo yêu cầu hoàn; chọn giao dịch/vé/lý do; gửi; xem trạng thái | Yêu cầu được lưu, chưa giả báo hoàn |
| ND-13 | Người dùng | Địa điểm QA | Chọn ngày/giờ; điền hồ sơ thuê đầy đủ; gửi hai lần | Một hồ sơ hợp lệ |
| ND-14 | Người dùng | Hồ sơ ND-13, quản lý duyệt | Mở lại hồ sơ; xem cọc; mở thanh toán cọc; theo dõi xác nhận | Số tiền/hạn rõ; xác nhận sau thanh toán |
| ND-15 | Người dùng | Đã đăng nhập | Hỗ trợ > Gửi yêu cầu; nhập chủ đề/nội dung; gửi; quản lý phản hồi; mở lại | Đúng nội dung và phản hồi hai phía |
| ND-16 | Người dùng | Đã đăng nhập | Truy cập #work/accounts và đơn người khác | Không lộ dữ liệu ngoài quyền |
| ND-17 | Người dùng | Form hỗ trợ đang mở | Thu hồi phiên bằng fixture; bấm gửi | Báo cần đăng nhập, không tạo hồ sơ |
| ND-18 | Người dùng | Viewport 375px | Mở hồ sơ/vé/yêu cầu; hộp sửa hồ sơ; Escape | Không tràn, đóng và trả focus |

Ca bổ sung được khai báo trước khi chạy:

| Mã | Vai trò | Điều kiện | Bước bấm tái lập | Mong đợi |
|---|---|---|---|---|
| ND-19 | Người dùng | Ghế trả phí còn trống | Chọn ghế trả phí khác; giữ chỗ; bấm Hủy giữ chỗ; mở lại đơn | Đã hủy, không phát hành vé |
| ND-20 | Người dùng | Yêu cầu thuê mới QA | Gửi hồ sơ thuê khác; mở Hủy thuê; nhập lý do; Gửi yêu cầu hủy | Hồ sơ Đã hủy, lưu lý do |
| ND-21 | Người dùng | Ghế còn trống | Chọn ghế; điền mã khuyến mãi không tồn tại; giữ chỗ; xóa mã; giữ lại | Mã sai báo lỗi; sửa đầu vào tiếp tục được |
| ND-22 | Người dùng | Token reset đã dùng ND-06, lưu ở SMTP nội bộ | Mở Đặt mật khẩu mới; nhập lại mã đã dùng + mật khẩu hợp lệ; xác nhận | Từ chối mã đã dùng |
| ND-23 | Người dùng | Đã đăng nhập | Mở Hồ sơ; đọc Lịch sử xem; bấm Khám phá sự kiện, xem một sự kiện; trở lại Hồ sơ | Hồ sơ liệt kê sự kiện đã xem, mới nhất trước và mỗi sự kiện chỉ xuất hiện một lần |


Kết quả thực chạy (21/11/2026, 15:26–15:43 giờ Việt Nam):

| Mã | Trạng thái | Kết quả thực tế và bằng chứng |
|---|---|---|
| K-01 | Đạt | Lọc rỗng, reload và Back/Forward giữ từ khóa; xóa khôi phục danh sách. |
| K-02 | Đạt | Mở sự kiện5: Suất SQL 23/11/2026 14:25–16:25, địa điểm6, Standard 0đ và mở/đóng bán hiện đúng. [Ảnh](evidence/guest-user/K-02.png) |
| K-03 | Đạt | Ngày 23 nhận focus; đổi tháng; calendar.ics có VEVENT. |
| K-04 | Đạt | Tìm địa điểm, mở chi tiết, bấm Xem lịch địa điểm và chọn ngày xem lịch trống. [Ảnh](evidence/guest-user/K-04.png) |
| K-05 | Đạt | Bấm năm tab Tổng quan/Đặt vé/Quy định/Câu hỏi thường gặp/Quyền riêng tư và mở các câu FAQ. Giới thiệu nằm ở chân trang. |
| K-06 | Đạt | Sau đăng nhập quay lại http://127.0.0.1:5831/#tickets/mine |
| K-07 | Đạt | Khách 375px mở sự kiện/lịch/địa điểm/đăng nhập không tràn; focus email khi mở, Tab/Escape đóng được. [Ảnh](evidence/guest-user/K-07-mobile.png) |
| ND-01 | Đạt | Tạo uat_user_1f9c3a48@example.test bằng form sau khi kiểm tra rỗng. [Ảnh](evidence/guest-user/ND-01.png) |
| ND-02 | Đạt | Hiện/ẩn mật khẩu, sai mật khẩu bị từ chối, đăng nhập đúng thành công. |
| ND-03 | Đạt | Tên mới Đã sửa 1f9c3a48 tồn tại sau reload. [Ảnh](evidence/guest-user/ND-03.png) |
| ND-04 | Đạt | Chặn xác nhận mật khẩu sai; đổi đúng; đăng xuất và đăng nhập mật khẩu mới thành công. |
| ND-05 | Đạt | Gửi mã UI, nhận email SMTP nội bộ, xác nhận qua UI, hồ sơ hiện Đã xác minh email. [Ảnh](evidence/guest-user/ND-05.png) |
| ND-06 | Đạt | Gửi mã qua SMTP nội bộ, nhập mã/mật khẩu mới và đăng nhập được; không gửi thư ngoài. [Ảnh](evidence/guest-user/ND-06.png) |
| ND-07 | Đạt | Thêm từ chi tiết rồi bỏ tại danh sách; danh sách rỗng đúng. |
| ND-08 | Đạt | Có thông báo quản lý duyệt thuê và giải quyết hỗ trợ; đánh dấu đọc, tải lại vẫn Đã đọc. [Ảnh](evidence/guest-user/ND-08.png) |
| ND-09 | Đạt | Đơn miễn phí tickets/book/checkout/35 sau chặn trống và chọn ghế F-1. [Ảnh](evidence/guest-user/ND-09.png) |
| ND-10 | Đạt | Vé tickets/mine/27 có QR tải được và PDF mở được, 3565 byte. Mở PDF bằng PDFium và kết xuất một trang QR; PDF hiện chỉ gồm QR, không có chữ tên/ghế. [Ảnh](evidence/guest-user/ND-10.png) |
| ND-11 | Đạt | Hai Edge context chọn cùng Ghế F-3 · Standard · Còn trống · 0 ₫; người đầu có tickets/book/checkout/39; người sau bị từ chối, vẫn ở chọn ghế. [Ảnh](evidence/guest-user/ND-11-rival.png) |
| ND-12 | Đạt | Đơn36 trả 100.000đ simulator ký; gửi hoàn12 bằng UI; quản lý duyệt và người gửi thấy Đã duyệt, lý do Đã đối chiếu 100.000 đồng với đơn QA 36. Chưa có chuyển tiền ngân hàng. [Ảnh](evidence/guest-user/ND-12-approved.png) |
| ND-13 | Đạt | requests/rentals/19 tạo bằng form và nhấn hai lần. [Ảnh](evidence/guest-user/ND-13.png) |
| ND-14 | Đạt | Quản trị viên duyệt hồ sơ thuê #19 (QT-14); người gửi thấy cọc 300.000đ; thanh toán QA có chữ ký, đối soát chuyển Xác nhận thuê. [Ảnh](evidence/guest-user/ND-14.png) |
| ND-15 | Đạt | Người gửi đọc lại support12: Đã giải quyết, QA support 12 resolved via manager UI. [Ảnh](evidence/guest-user/ND-15.png) |
| ND-16 | Đạt | Người dùng bị chặn #work/accounts; tài khoản rival mở đơn35 không thấy sự kiện/chi tiết của chủ đơn. [Ảnh](evidence/guest-user/ND-16-foreign.png) |
| ND-17 | Đạt | Thu hồi phiên QA khi form đang mở; bấm gửi hiển thị cần phiên đăng nhập, không chuyển tới hồ sơ. [Ảnh](evidence/guest-user/ND-17.png) |
| ND-18 | Đạt | 375px: hồ sơ/vé/yêu cầu không tràn; Escape trả focus nút mở. [Ảnh](evidence/guest-user/ND-18.png) |
| ND-19 | Đạt | tickets/book/checkout/43 hủy qua nút Hủy giữ chỗ, reload vẫn Đã hủy. [Ảnh](evidence/guest-user/ND-19.png) |
| ND-20 | Đạt | requests/rentals/28 hủy qua form, Đã hủy và có lý do QA hủy hồ sơ thử. [Ảnh](evidence/guest-user/ND-20.png) |
| ND-21 | Đạt | Mã INVALID_QA bị từ chối; xóa mã tạo được tickets/book/checkout/43. |
| ND-22 | Đạt | Mã SMTP đã dùng ND-06 bị từ chối khi gửi lại trên form; không đổi mật khẩu lần hai. [Ảnh](evidence/guest-user/ND-22.png) |
| ND-23 | Đạt | Đã chạy lại trên 5834 qua biểu mẫu Edge. Xem sự kiện #5 rồi #6 thì #6 đứng đầu. Xem lại #5 hai lần thì #5 lên đầu và chỉ xuất hiện một lần. Tải lại, Back/Forward và đăng nhập trong cửa sổ Edge mới đều thấy cùng lịch sử. [Trước sửa](evidence/guest-user/ND-23-before-fix.png) · [Sau sửa](evidence/guest-user/ND-23-after-fix.png) · [Kết quả A–F](evidence/guest-user/ND-23-retest.json) |

Tổng hợp: Khách **7 Đạt / 0 Lỗi / 0 Bị chặn**; Người dùng **23 Đạt / 0 Lỗi / 0 Bị chặn**. ND-23 đã bổ sung API và danh sách trong hồ sơ, sau đó chạy lại trên bản Release từ ZIP ở cổng 5834.

Dữ liệu để tự làm lại:

- Tạo tài khoản mới qua `/#account/register` (email riêng), xác nhận điều khoản. Tài khoản lần chạy này là `uat_user_1f9c3a48@example.test`; mật khẩu và cookie QA chỉ lưu riêng dưới `obj/guest-user/`, không nằm trong tài liệu/ZIP.
- Ca công khai dùng sự kiện5 (`/#event/5`), địa điểm6 (`/#venue/6`), lịch `/#calendar/2026-11`. ND-09 trở đi dùng sự kiện19, suất24, khu miễn phí24 và trả phí25, ngày 01/03/2027. API chuẩn bị riêng sự kiện/suất/khu/loại vé và SQL bật cho phép tự hủy cho đúng sự kiện19; không tính bước chuẩn bị là bước UI.
- Chọn vé: mở `/#event/19` > **Chọn khu vực và ghế**; chọn khu **Miễn phí user-3925dc** hoặc **Trả phí user-3925dc**. Lần chạy tạo đơn35 miễn phí F-1, vé27; đơn36 trả phí P-1 100.000đ; tranh chấp F-3 tạo đơn39 chỉ ở tài khoản thắng. Ghế đã dùng không thể tái dùng: chọn ghế còn trống khác hoặc chuẩn bị suất QA mới.
- Thuê19: địa điểm6, 01/06/2027 09:00–12:00 giờ Việt Nam, 10 người; tên **Hồ sơ UAT 1f9c3a48**, loại Hội thảo; mục đích **Kiểm thử bàn giao hai vai trò**; kịch bản **Đón khách và hội thảo**; đại diện **Người dùng QA**, điện thoại `0901234567`. Tổng tiền 1.000.000đ, cọc 300.000đ. Bấm hai lần nút gửi chỉ tạo một hồ sơ: SQL QA đối chứng theo chủ tài khoản + khoảng giờ trả COUNT=1. Ngày giờ lưu UTC 02:00–05:00. Nếu làm lại, chọn ngày khác chưa bị giữ.
- Quản lý mở `/#work/rentals/19`, bấm duyệt, nhập lý do **QA cross-role rental 19 approved**. Người gửi mở lại `/#requests/rentals/19`, thấy cọc và hạn, bấm **Mở thanh toán cọc**, xác nhận trên simulator, trở về hồ sơ, **Kiểm tra thanh toán cọc**. Kết quả **Xác nhận thuê**. Thuê28 là hồ sơ riêng để kiểm tra hủy, không hủy thuê19 đã xác nhận.
- Hỗ trợ12: `/#requests/support/12`, chủ đề **Hỗ trợ UAT 1f9c3a48**, nội dung **Vui lòng phản hồi hồ sơ QA 1f9c3a48**. Quản lý xử lý bằng lý do **QA support 12 resolved via manager UI**; người gửi đọc lại được đúng phản hồi và **Đã giải quyết**.
- Hoàn12: từ đơn36 chọn giao dịch, chọn vé, lý do **Thay đổi kế hoạch**, gửi. Quản lý duyệt với lý do **Đã đối chiếu 100.000 đồng với đơn QA 36.** Người gửi mở `/#requests/refunds/12`: **Đã duyệt**, 100.000đ, **Chưa lập lệnh chuyển**. Không ghi nhận đã chuyển tiền ngân hàng.
- Hai tài khoản cùng ghế: mở cùng suất/khu trên hai context Edge riêng, cùng chọn một ghế còn trống trước khi bên nào gửi; bên A gửi thành công, bên B bấm gửi sau nhận **Một hoặc nhiều ghế đã được giữ hoặc phát hành.** ND-17 dùng phiên đối thủ, thu hồi bằng API QA trong khi form hỗ trợ còn mở, sau đó bấm gửi thật; hiện **Vui lòng đăng nhập để tiếp tục. Đăng nhập lại**.

Môi trường tích hợp và giới hạn:

- Cổng5831 là bản Release đã sắp xếp không bật SMTP/PayOS. Các ca xác minh/khôi phục, đơn trả phí và cọc chạy cùng tài nguyên frontend đã sắp xếp trên cổng5835, cùng CSDL QA. Trình duyệt Edge 1440×1000 và 375×812, thao tác Playwright vào nút/liên kết/trường/hộp thoại thật.
- SMTP là máy nhận TLS nội bộ, đọc thư riêng ở `obj/handoff-qa/work/mail/` theo email/tiêu đề; không gửi thư ra ngoài, không chép mã xác minh/reset vào biên bản. Mã khôi phục cũ bị từ chối qua UI ở ND-22.
- PayOS là mô phỏng HTTP có kiểm tra/chữ ký. Browser bắt URL `https://pay.payos.vn/**` và chuyển về máy mô phỏng `http://127.0.0.1:5836`, bấm **Xác nhận thanh toán QA**, **Trở về QLSK** rồi đối soát bằng nút trong ứng dụng. Không có giao dịch tiền thật. Lần đầu provider QA chưa đọc body chunked khiến checkout báo dịch vụ chưa khả dụng; đã sửa máy mô phỏng, chạy lại thành công đơn36/cọc19. Đây là lỗi hạ tầng QA, không sửa mã sản phẩm.
- PDF vé27 tải qua nút UI, mở bằng PDFium được 1 trang QR. Xem [PDF](evidence/guest-user/ticket.pdf), [ảnh trang PDF](evidence/guest-user/ticket-pdf-opened.png); PDF hiện chỉ có QR. [ICS](evidence/guest-user/calendar.ics) mở nội dung có `BEGIN:VEVENT`, `DTSTART`, `DTEND`. [Kết quả JSON](evidence/guest-user/results.json) và [SHA-256 bằng chứng](evidence/guest-user/manifest.json) không chứa mật khẩu/cookie/mã email.
- Khi chuẩn bị runner, có lần chọn sai tên/role của nút hoặc đọc trước khi render hoàn tất. Các lần này được sửa selector/đợi trạng thái và chạy lại, không coi là lỗi sản phẩm. Đặc biệt ND-11 đã chạy lại tới phản hồi từ chối cuối cùng, không chỉ nhìn màn hình “Đang gửi”.

### Chạy lại ND-23 trên Edge, cổng 5834

| Nhánh ND-23 | Tiền điều kiện | Bước thao tác Edge | Mong đợi |
|---|---|---|---|
| A · Người dùng | Phiên QA riêng đã đăng nhập | Mở Khám phá sự kiện; bấm sự kiện A; trở về Khám phá; bấm sự kiện B; mở Hồ sơ | Lịch sử xem có B trước A, liên kết mở đúng sự kiện |
| B · Lặp và sắp xếp | Đã có A/B | Bấm lại A từ Khám phá hai lần; mở Hồ sơ | A đứng đầu, chỉ có một hàng cho A và một hàng cho B |
| C · Lưu lâu dài | Có lịch sử | Tải lại Hồ sơ; Back/Forward; mở cửa sổ Edge sạch, đăng nhập lại bằng biểu mẫu, mở Hồ sơ | Dữ liệu không mất, thứ tự nhất quán |
| D · Cách ly tài khoản | Hai tài khoản QA khác nhau | Tài khoản đối thủ mở Hồ sơ; xem sự kiện C; trở lại tài khoản chính | Đối thủ không thấy lịch sử riêng của chính; sự kiện C không chen vào tài khoản chính |
| E · Khách | Context sạch chưa đăng nhập | Mở Khám phá; bấm sự kiện; mở Hồ sơ | Không hiện lịch sử cá nhân của tài khoản khác, không ghi lịch sử cá nhân cho khách |
| F · Hồi quy yêu thích | Tài khoản chính | Lưu yêu thích một sự kiện đã xem; mở danh sách yêu thích; bỏ yêu thích; trở lại Hồ sơ | Yêu thích hoạt động, lịch sử xem vẫn tồn tại |

Cả sáu nhánh Đạt. Tài khoản thứ hai xem sự kiện #7 mà không thay đổi lịch sử của tài khoản chính. Khách mở #7 không gửi yêu cầu ghi và không thấy dữ liệu tài khoản. Lưu rồi bỏ yêu thích sự kiện #5 vẫn giữ lịch sử xem. [Phiên Edge sạch](evidence/guest-user/ND-23-new-context.png) · [Tài khoản thứ hai](evidence/guest-user/ND-23-isolation.png) · [Khách](evidence/guest-user/ND-23-guest.png)
