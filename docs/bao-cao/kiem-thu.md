## 3.3 Kiểm thử

### 3.3.1 Mục tiêu và phạm vi

Kiểm thử nhằm đối chiếu các chức năng QLSK với yêu cầu nghiệp vụ và kiểm tra sự nhất quán giữa giao diện, API và dữ liệu SQL Server. Phạm vi bao gồm khách truy cập, người dùng, ban tổ chức, nhân viên được phân công, quản lý theo nhóm quyền và quản trị viên. Kịch bản tập trung vào khám phá sự kiện, tài khoản, đặt vé, thuê địa điểm, hồ sơ tổ chức, xét duyệt, kiểm soát vào cửa và nghiệp vụ vận hành.

Báo cáo sử dụng hai lớp bằng chứng: bộ nghiệm thu đã có trong dự án và đợt kiểm tra bổ sung phục vụ lập báo cáo. Hai lớp được trình bày riêng; không cộng số ca API bổ sung vào số ca nghiệm thu giao diện vì chúng có phần phạm vi trùng nhau.

### 3.3.2 Phương pháp và môi trường

Bộ nghiệm thu giao diện đã có được thực hiện ngày 21/11/2026 trên Microsoft Edge với CSDL QA riêng `QLSK_UI_c67acc32`. Các biên bản ghi điều kiện, bước thao tác, kết quả mong đợi và kết quả thực tế. API được dùng để chuẩn bị hoặc đối chứng dữ liệu. Nguồn chi tiết gồm [khách và người dùng](../testcases/01-khach-nguoi-dung.md), [ban tổ chức và nhân viên](../testcases/02-ban-to-chuc-nhan-vien.md), [quản lý và quản trị](../testcases/03-quan-ly-quan-tri.md).

Đợt bổ sung thực hiện lúc 18:44 ngày 21/11/2026, giờ Việt Nam, trên ứng dụng QA đang hoạt động tại `http://127.0.0.1:5834`. Chương trình [report_public_smoke.py](../../test/report_public_smoke.py) chỉ gửi HTTP GET công khai bằng thư viện chuẩn Python, không đăng nhập, không tạo hồ sơ và không thay đổi dữ liệu nghiệp vụ. Chương trình kiểm tra mã HTTP, kiểu nội dung JSON và điều kiện dữ liệu của từng chức năng. Các giá trị ID được chọn từ dữ liệu phản hồi để giảm phụ thuộc vào mã cố định; riêng ca lọc giá yêu cầu môi trường có sự kiện giá 100.000 đồng.

Mã nguồn hiện tại cũng được biên dịch bằng `.NET 10` với lệnh `dotnet build QLSK.csproj --no-restore -o obj/bao-cao-build -p:UseAppHost=false`. Thư mục đầu ra riêng giúp không ghi đè bản ứng dụng QA đang chạy. Việc biên dịch mã nguồn hiện tại và việc gọi API của bản QA đang chạy là hai phép xác minh khác nhau; không dùng kết quả API để khẳng định toàn bộ mã nguồn vừa biên dịch đã được chạy lại.

### 3.3.3 Kế hoạch và ca kiểm thử bổ sung

Khi nghiệm thu lại, cần chuẩn bị một CSDL thử riêng, một suất có ghế/loại vé để mua, một sự kiện đã kết thúc để đối soát, hai tài khoản mua khác nhau và tài khoản theo từng nhóm quyền. Các ca làm thay đổi trạng thái cần dùng dữ liệu tách biệt hoặc khôi phục fixture trước khi chạy lại. Kế hoạch sau xác định điểm quan sát và điều kiện kết thúc của từng nhóm.

| Nhóm ca | Thao tác đại diện | Điều kiện đạt và cách đối chứng |
|---|---|---|
| Công khai và tài khoản | Khách tìm theo giá, mở chi tiết và lịch. Người dùng đăng nhập, xem lịch sử và thông báo. | Chi tiết và lịch có cùng mã suất. Hai tài khoản có lịch sử riêng. So sánh API, giao diện và ca ND-23. |
| Nộp hồ sơ và công bố sự kiện | Người nộp bổ sung chứng từ, quản lý xét duyệt, BTC gửi phiên bản và công bố lần đầu. | Kết quả xét duyệt có lý do, người nộp thấy đúng trạng thái. Khách chỉ thấy sự kiện sau bước công bố. |
| Vé và cổng | Hai khách chọn cùng ghế, mở lại đơn sau khi gửi, nhân viên quét hai lần và thử lại sau khi thu hồi phân công. | Một chỗ giữ hợp lệ, một lần vào thành công. Đối chiếu trạng thái đơn, vé, nhật ký và quyền theo suất. |
| Thuê, hoàn và tài chính | Xét hai yêu cầu cùng khung giờ, duyệt hồ sơ hoàn có chứng cứ thu, lập đối soát sau khi hoàn tất sự kiện. | Lịch đã duyệt không chồng lấn, số duyệt hoàn trong giới hạn, số tiền kỳ truy về các giao dịch gốc. |
| Quản trị và báo cáo | Đổi nhóm quyền, mở đúng hàng đợi, lọc trạng thái rồi xuất tệp. | Quyền thay đổi có hiệu lực ở API. Các dòng và giá trị của tệp khớp bản xem trước với cùng bộ lọc. |

Các mã K trong bảng dưới đây chỉ nhóm chức năng liên quan của kế hoạch nghiệm thu, không có nghĩa ca API đã thực hiện đầy đủ thao tác giao diện của mã K đó.

| Mã bổ sung | Chức năng liên quan | Thao tác kiểm tra | Kết quả mong đợi | Kết quả thực tế |
|---|---|---|---|---|
| BC-01 | Khách, tiền điều kiện K-06 | GET `/api/capabilities` trong phiên chưa đăng nhập | Chưa xác thực, có quyền đọc danh mục công khai | Đạt; `authenticated=false`, có `catalog.events` |
| BC-02 | Khám phá sự kiện, K-01 | GET `/api/events` | Có sự kiện với ID và tên hợp lệ | Đạt; trang đầu có 8 sự kiện, chọn ID 5 |
| BC-03 | Tìm kiếm, K-01 | Tìm đúng tên sự kiện đã đọc ở BC-02 | Danh sách chứa sự kiện được chọn và đúng từ khóa | Đạt; trả về 1 kết quả |
| BC-04 | Bộ lọc sự kiện, K-01 | Lọc giá thấp nhất và cao nhất cùng bằng 100.000 đồng | Có kết quả, mọi thẻ có giá thấp nhất đúng ngưỡng | Đạt; 3 kết quả đúng giá |
| BC-05 | Chi tiết sự kiện, K-02 | GET chi tiết sự kiện của BC-02 | Tên khớp danh sách, có suất và địa điểm, có giờ bắt đầu/kết thúc | Đạt; sự kiện 5 có 1 suất |
| BC-06 | Lịch sự kiện, K-03 | GET lịch trong khoảng bao quanh suất BC-05 | Lịch chứa suất đã đọc từ chi tiết | Đạt; tìm thấy suất 5 trong 13 suất trả về |
| BC-07 | Địa điểm, K-04 | GET danh sách với giá thuê tối đa 1.000.000 đồng | Có kết quả; giá không vượt ngưỡng, sức chứa dương | Đạt; trang đầu có 12 địa điểm |
| BC-08 | Chi tiết địa điểm, K-04 | GET địa điểm được chọn từ BC-07 | Tên khớp danh sách và có khu ghế | Đạt; địa điểm 6 có 1 khu |
| BC-09 | Dữ liệu bộ lọc, K-01 | GET `/api/event-filter-options` | Có thành phố, địa điểm, danh mục; khoảng giá hợp lệ | Đạt; 3 thành phố, 28 địa điểm, 17 danh mục, 3 khoảng giá |
| BC-10 | Nội dung chính sách, liên quan K-05 | GET `/api/policies` | Chính sách công khai có tiêu đề và nội dung | Đạt; phản hồi có 44 bản ghi chính sách |

Tái lập từ thư mục dự án bằng `python test/report_public_smoke.py --base http://127.0.0.1:5834`. Cần khởi động bản QA có kết nối SQL Server và dữ liệu mẫu đáp ứng điều kiện trên. Kết quả máy đọc được lưu tại [public-smoke.json](bang-chung/public-smoke.json), gồm thời điểm, địa chỉ máy chủ, trạng thái từng ca và số lượng quan sát. Chỉ số lượng trang đầu được báo cáo đối với API phân trang; không suy diễn thành tổng số bản ghi trong CSDL.

### 3.3.4 Tổng hợp kết quả

Theo [mục lục nghiệm thu đã có](../testcases/README.md), bộ giao diện ghi nhận 81 ca đạt, 0 lỗi và 0 bị chặn. Ca lịch sử xem ND-23 đã được kiểm tra lại sau khi bổ sung chức năng. Phân bố theo vai trò như sau:

| Vai trò | Số ca đạt đã ghi nhận |
|---|---:|
| Khách truy cập | 7 |
| Người dùng | 23 |
| Ban tổ chức | 20 |
| Nhân viên được phân công | 6 |
| Quản lý theo nhóm quyền | 7 |
| Quản trị viên | 18 |
| Tổng | 81 |

Đây là kết quả tái sử dụng từ hồ sơ nghiệm thu, không phải 81 ca vừa được chạy lại khi soạn báo cáo. Ngoài ra, dự án có [bằng chứng kiểm tra dữ liệu mẫu](../testcases/evidence/seed-demo/kiem-thu.json) ghi 244 kiểm tra API đạt; chỉ số này thuộc một lượt kiểm tra khác và không cộng vào 81 ca giao diện.

Đợt bổ sung mới có **10/10 ca API đạt**, không có ca lỗi. Ba trang công khai được mở và chụp trực tiếp bằng Microsoft Edge với viewport 1440 × 960 px, không ghi nhận lỗi JavaScript trong quá trình chụp. Chương trình [report_capture.py](../../test/report_capture.py) dùng phiên khách riêng, chặn yêu cầu khác GET; [manifest ảnh](bang-chung/captures.json) ghi đường dẫn, tiêu đề và thời điểm chụp. Ảnh trang chủ, chi tiết sự kiện và danh sách địa điểm nằm trong thư mục [ảnh báo cáo](anh/). Đây là ảnh dữ liệu thử QA; tên sự kiện, banner và địa điểm trong ảnh không đại diện cho dữ liệu vận hành thực tế.

Biên dịch thành công với **0 lỗi và 2 cảnh báo** NU1902/NU1903 về gói `System.Security.Cryptography.Pkcs` phiên bản 9.0.11. Các cảnh báo cần được đánh giá và xử lý trong đợt bảo trì phụ thuộc; kết quả biên dịch thành công không đồng nghĩa không còn rủi ro bảo mật.

### 3.3.5 Đánh giá và giới hạn

Kết quả bổ sung xác nhận các luồng đọc công khai hoạt động trên môi trường QA và dữ liệu giữa danh sách, chi tiết, lịch cùng địa điểm có thể đối chiếu được. Các biên bản nghiệm thu đã có cung cấp bằng chứng rộng hơn về luồng nghiệp vụ theo vai trò. Tuy nhiên, 10 ca API mới không thay thế kiểm tra đặt vé, thanh toán, xét duyệt hoặc phân quyền qua giao diện.

Phạm vi chưa có đủ bằng chứng nghiệm thu được ghi riêng trong mục đối chiếu yêu cầu của hồ sơ cũ: chuyển tiền thực tế, email/SMS ra ngoài, retry thanh toán hoặc giải ngân, sao lưu/khôi phục, HTTPS/2FA và một số quy tắc thời hạn, hợp đồng, bảo vệ dữ liệu cá nhân. Thanh toán và SMTP trong các ca trước sử dụng mô phỏng nội bộ. Đợt lập báo cáo không thực hiện đo tải, không đặt ngưỡng hiệu năng và không kết luận hệ thống đã đủ điều kiện triển khai thực tế chỉ từ các kết quả trên.
