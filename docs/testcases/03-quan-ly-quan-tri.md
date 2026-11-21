# Biên bản nghiệm thu Quản lý hệ thống / Quản trị viên

Ngày 21/11/2026. Bản Release đã sắp xếp, `http://127.0.0.1:5833`, Edge Chromium qua Playwright. CSDL QA duy nhất `QLSK_UI_c67acc32`; không sửa CSDL vận hành. Các bước và kết quả mong đợi được khai báo trước thao tác. Mỗi mã ở dưới ghi lại kịch bản tương ứng. API chỉ chuẩn bị fixture, phiên đăng nhập và đối chiếu nội dung; kết quả giao diện chỉ ghi Đạt sau thao tác thực tế. Đăng nhập QA admin từ cấu hình QA riêng; quản lý nhóm 2 được tạo thành Người dùng và được admin cấp quyền bằng giao diện (QT-02).

## Kết quả theo vai trò

| Vai trò | Đạt | Lỗi | Bị chặn |
|---|---:|---:|---:|
| Quản lý | 7 | 0 | 0 |
| Quản trị viên | 18 | 0 | 0 |

## Các ca đã chạy

Điều kiện chung: đăng nhập đúng vai trò, QA host hoạt động, dùng fixture QA. Mã/tên có `[marker]` là hậu tố duy nhất được runner tạo. Lần cuối bộ chính: `7662d6a3`; quản lý nhóm 2 tài khoản #64. Ca chạy lại CRUD tạo danh mục #14, địa điểm #22, khu vực #28, chính sách #26. Không cần nộp lại quyết định đã kết thúc; để tái hiện nguyên luồng hãy dùng tài khoản QA, tạo bản ghi mới theo điều kiện trong ca. Các ca khóa/quyền chỉ nhắm tài khoản QA.

### QL-01 — Quản lý — Đạt

- Bước bấm: Đăng nhập tài khoản vừa cấp quyền → mở các menu → nhập liên kết /#work/accounts.
- Mong đợi trước chạy: Chỉ có menu trong nhóm thuê/hỗ trợ; liên kết quản trị bị từ chối.
- Thực tế: Nhóm 2: rentals,support; liên kết tài khoản ngoài quyền bị chặn.

### QL-02 — Quản lý — Đạt

- Bước bấm: Duyệt yêu cầu thuê → Trạng thái Đã gửi → mở Thuê nghiệm thu [marker] → Quyết định thuê/Phê duyệt/lý do; người gửi mở Yêu cầu của tôi → thuê tương ứng.
- Mong đợi trước chạy: Quản lý và người gửi cùng thấy Chờ cọc; cọc 30% của 1.000.000 = 300.000 ₫.
- Thực tế: Hồ sơ thuê #21: quản lý duyệt; người gửi thấy cọc 300.000 ₫ trên giao diện.

### QL-03 — Quản lý — Đạt

- Bước bấm: Hỗ trợ và tranh chấp → mở yêu cầu fixture → Phản hồi và xử lý → Giải quyết → nhập lý do; người gửi mở hỗ trợ tương ứng.
- Mong đợi trước chạy: Người gửi thấy phản hồi và trạng thái Đã giải quyết.
- Thực tế: Yêu cầu hỗ trợ #13: hai phía Đã giải quyết, người gửi đọc được phản hồi.

### QL-06 — Quản lý — Đạt

- Bước bấm: Tài khoản quản lý nhóm 1 mở hồ sơ BTC #15 → Quyết định hồ sơ → Yêu cầu bổ sung → nhập Bổ sung giấy tờ rõ nét → xác nhận. Người nộp mở trạng thái, chuẩn bị nộp lại và gửi hồ sơ #16. Quản lý mở #16 → Phê duyệt → Giấy tờ bổ sung hợp lệ. Người nộp tải lại trạng thái.
- Mong đợi trước chạy: Người nộp thấy lý do bổ sung, nộp lại được; chỉ sau duyệt mới thấy Đã duyệt và lý do.
- Thực tế: Đã chạy Edge với tài khoản manager_group1_bb6e5.ba807f38@example.test (#68), vai trò manager nhóm 1; hai lần quyết định thật và người nộp đọc lại cả hai kết quả. Bằng chứng 10-ho-so-bo-sung-da-duyet.png, 11-quan-ly-nhom1-duyet.png trong bộ BTC.

### QL-07 — Quản lý — Đạt

- Bước bấm: Hai tài khoản riêng cùng mở Yêu cầu thuê mới, chọn địa điểm #22, 04/09/2027 09:00–11:00, điền thông tin và gửi. Quản lý nhóm 2 lần lượt Phê duyệt cả hai; hai người gửi tải lại chi tiết.
- Mong đợi trước chạy: Chỉ một hồ sơ được duyệt giữ lịch; hồ sơ sau báo xung đột, chưa có cọc; hai phía đọc đúng trạng thái.
- Thực tế: Hai người gửi bằng form thật hồ sơ #26, #27, địa điểm22, 04/09/2027 09:00–11:00. Quản lý nhóm2 duyệt đầu; lần duyệt sau bị từ chối và giữ dialog. Hai người gửi đọc lại lần lượt Chờ đặt cọc / Chờ duyệt. Quyết định thuê
Đóng
Đối tượng
Thuê Địa điểm UI bbd61119
Trạng thái hiện tại
Chờ duyệt
Quyết định *
Chọn quyết định
Phê duyệt
Từ chối
Lý do / ghi chú *
Quyết định thuê

Địa điểm hoặc khung giờ thuê không còn khả dụng.

### QL-08 — Quản lý — Đạt

- Bước bấm: BTC có sự kiện21/bản21/suất26 đang chờ duyệt. Quản lý nhóm1 mở Duyệt sự kiện và bản sửa → lọc Sự kiện chờ duyệt → mở Handoff BTC quản lý → Quyết định phiên bản → Phê duyệt → lý do Nhóm1 duyệt chương trình hợp lệ → xác nhận. BTC mở Phiên bản sự kiện21.
- Mong đợi trước chạy: Sự kiện được duyệt; BTC đọc được trạng thái và lý do của đúng quản lý.
- Thực tế: Đã chạy Edge với tài khoản manager nhóm1 reviewmanager_d00e8.ba807f38@example.test (#72). Quyết định bằng UI thật, BTC mở lại thấy Đã duyệt và đúng lý do. Bằng chứng ../btc-12-su-kien-nhom1-duyet.png, ../btc-13-quan-ly-nhom1-duyet-su-kien.png. Fixture PENDING chuẩn bị bằng API; kết quả được xác minh bằng UI cả hai phía.

### QL-TC01 — Quản lý — Đạt

- Bước bấm: Đăng nhập quản lý nhóm tài chính → Xét hoàn tiền → yêu cầu #12 → Quyết định hoàn → Phê duyệt → lý do Đã đối chiếu 100.000 đồng với đơn QA 36 → xác nhận. Quay về danh sách, lọc Đã duyệt rồi mở lại #12.
- Mong đợi trước chạy: Hoàn #12 được duyệt 100.000 ₫, xuất hiện đúng trong bộ lọc Đã duyệt; chưa chuyển tiền thì không báo Đã hoàn.
- Thực tế: Root đã chạy Edge với manager nhóm 3. Duyệt #12 thành công; phát hiện bộ lọc thiếu APPROVED, sửa work-sections.js và chạy lại, lọc Đã duyệt mở được đúng #12 số tiền 100.000 ₫. Bằng chứng QL-TC01-duyet-hoan.png, QL-TC01-loc-truoc-sua.png, QL-TC01-loc-da-duyet.png. Server không cung cấp thao tác chuyển tiền cho trạng thái này; chưa có giao dịch hoàn bên ngoài.

### QT-01 — Quản trị viên — Đạt

- Bước bấm: Đăng nhập QA admin → Khu làm việc → lần lượt mở mọi menu nhóm Quản trị, Xét duyệt, Tài chính, Sự kiện, Tại cổng.
- Mong đợi trước chạy: Từng màn hình tải được, danh mục đúng quyền, các màn cần phạm vi yêu cầu chọn sự kiện/suất.
- Thực tế: applications, rentals, support, refunds, contracts, settlements, payouts, payments, events, versions, sessions, ticketTypes, reviews, accounts, settings, audit, monitoring, venues, zones, venueBlocks, catalog, policies

### QT-02 — Quản trị viên — Đạt

- Bước bấm: Tài khoản và phân quyền → mở tài khoản fixture → Điều chỉnh quyền quản lý → chọn Thuê địa điểm và hỗ trợ → điền lý do → xác nhận.
- Mong đợi trước chạy: Vai trò đổi Quản lý hệ thống, phiên cũ hết hiệu lực.
- Thực tế: Tài khoản 64 (mgmt_accept.7662d6a3@example.test) được cấp nhóm Thuê địa điểm và hỗ trợ bằng hộp thoại.

### QT-03 — Quản trị viên — Đạt

- Bước bấm: Danh mục và banner → Danh mục sự kiện → nhập tên Danh mục UI [marker], thứ tự 9 → Thêm nội dung.
- Mong đợi trước chạy: Tạo và mở đúng chi tiết danh mục.
- Thực tế: http://127.0.0.1:5833/#work/catalog/14?resource=event-categories

### QT-04 — Quản trị viên — Đạt

- Bước bấm: Địa điểm và khu vực → điền tên/địa chỉ/sức chứa 12/giá 123000/tiện ích → Thêm địa điểm.
- Mong đợi trước chạy: Chi tiết địa điểm giữ đúng giá, sức chứa.
- Thực tế: http://127.0.0.1:5833/#work/venues/22

### QT-05 — Quản trị viên — Đạt

- Bước bấm: Khu vực, ghế và cụm ghế → chọn địa điểm UI → tên khu/sức chứa 3/Ghế đánh số/Q-01,Q-02,Q-03 → Khai khu vực và sơ đồ ghế.
- Mong đợi trước chạy: Khu vực và ba ghế được tạo.
- Thực tế: http://127.0.0.1:5833/#work/zones/28

### QT-06 — Quản trị viên — Đạt

- Bước bấm: Chính sách → soạn tên/phạm vi QA/nội dung/ngày hiệu lực → Soạn phiên bản → Công bố phiên bản, lý do.
- Mong đợi trước chạy: Nháp chuyển Đang áp dụng.
- Thực tế: http://127.0.0.1:5833/#work/policies/26

### QT-07 — Quản trị viên — Đạt

- Bước bấm: Giám sát dịch vụ → đọc trạng thái; Nhật ký kiểm toán → Tìm trong trang này = assign.
- Mong đợi trước chạy: Dịch vụ và nhật ký thao tác thật hiển thị.
- Thực tế: Giám sát dịch vụ tải; nhật ký tìm assign có thao tác cấp quyền QT-02.

### QT-08 — Quản trị viên — Đạt

- Bước bấm: Cấu hình nghiệp vụ → Phí dịch vụ → Gửi thay đổi cấu hình → giá -1/lý do → xác nhận.
- Mong đợi trước chạy: Giá âm bị từ chối, không đổi cấu hình QA.
- Thực tế: Giá trị âm không được lưu; hộp thoại giữ nguyên để sửa.

### QT-09 — Quản trị viên — Đạt

- Bước bấm: Báo cáo và định dạng xuất → chọn Duyệt yêu cầu thuê → Chọn định dạng xuất → Excel (.xlsx) → Xuất báo cáo; lặp lại với Word (.docx); mở nội dung tệp tải xuống.
- Mong đợi trước chạy: Các tệp mở được; tất cả dòng, số tiền và trạng thái khớp bản xem trước.
- Thực tế: Tải thật qua Edge, mở gói OpenXML XLSX/DOCX; đối chiếu từng ô với toàn bộ 21 dòng của bản xem trước. Giữ nguyên tệp và bản xem trước tại thời điểm xuất để đối chiếu.

### QT-10 — Quản trị viên — Đạt

- Bước bấm: Mở Tài khoản và phân quyền → thu màn hình còn 390×844 → Tab → tải lại → Back → Forward.
- Mong đợi trước chạy: Tiêu đề/điều khiển truy cập được; bàn phím có focus; điều hướng vẫn hoạt động.
- Thực tế: Edge 390px: tiêu đề hiện; Tab đặt focus vào điều khiển; tải lại, Back và Forward hoạt động.

### QT-11 — Quản trị viên — Đạt

- Bước bấm: Tài khoản và phân quyền → mở QA #64 → Quản lý truy cập → Khóa/lý do/xác nhận → Mở khóa → Thu hồi phiên.
- Mong đợi trước chạy: Khóa rồi mở khóa thành công; thu hồi phiên không khóa tài khoản.
- Thực tế: Tài khoản QA #64: khóa, mở khóa, thu hồi phiên bằng ba lần xác nhận; cuối cùng hoạt động.

### QT-12 — Quản trị viên — Đạt

- Bước bấm: Danh mục và banner → Banner → Thêm nội dung/tên/thứ tự 3 → tạo → Điều chỉnh hiển thị/Công khai → Điều chỉnh hiển thị/Ẩn.
- Mong đợi trước chạy: Banner lưu đúng thông tin, chuyển Công khai rồi Đã ẩn.
- Thực tế: http://127.0.0.1:5833/#work/catalog/7?resource=banners; tạo, công khai rồi ẩn banner.

### QT-13 — Quản trị viên — Đạt

- Bước bấm: Lịch bảo trì địa điểm → Chặn lịch địa điểm → chọn địa điểm UI #22, 01/08/2027 09:00–11:00/lý do → tạo → Gỡ chặn lịch/lý do.
- Mong đợi trước chạy: Lịch không khả dụng được tạo, sau gỡ không còn chặn.
- Thực tế: http://127.0.0.1:5833/#work/venueBlocks/23; lịch bảo trì 01/08/2027 09:00–11:00 được tạo rồi gỡ.

### QT-14 — Quản trị viên — Đạt

- Bước bấm: Mở hồ sơ thuê #19 → Quyết định thuê → Phê duyệt → lý do QA cross-role rental 19 approved; người gửi mở Yêu cầu thuê #19, thanh toán cọc mô phỏng có chữ ký rồi đối soát.
- Mong đợi trước chạy: Hai bên thấy chờ cọc 300.000 ₫; chỉ xác nhận sau đối soát cọc.
- Thực tế: Thao tác duyệt thực hiện bằng phiên admin trong Edge. Người gửi xác nhận Chờ đặt cọc 300.000 ₫, sau mô phỏng có chữ ký và đối soát chuyển Xác nhận thuê. Xem QL-04-rental19.png và biên bản người dùng ND-14.

### QT-15 — Quản trị viên — Đạt

- Bước bấm: Mở Hỗ trợ và tranh chấp/#12 → Phản hồi và xử lý → Giải quyết → lý do QA support 12 resolved via manager UI; người gửi mở hỗ trợ #12.
- Mong đợi trước chạy: Hai phía thấy đã giải quyết và đúng nội dung phản hồi.
- Thực tế: Thao tác xử lý bằng phiên admin trong Edge. Người gửi đã mở lại, thấy Đã giải quyết và đúng phản hồi; QL-05-support12.png và ND-15.

### QT-16 — Quản trị viên — Đạt

- Bước bấm: Hợp đồng phí → Lập hợp đồng → chọn BTC hồ sơ16, Phần trăm doanh thu, mức2, hiệu lực 01/01/2030–31/12/2030 → Lập hợp đồng → đọc chi tiết.
- Mong đợi trước chạy: Lưu đúng BTC, phí2%, khoảng hiệu lực; không thay hợp đồng đơn cũ.
- Thực tế: Hợp đồng8 được tạo bằng form thật; đọc đúng BTC16 Người bổ sung, phí2.00%, hiệu lực năm2030. Lần đầu nhãn hiện CoHieuLuc; đã sửa nguồn và chạy lại danh sách/chi tiết trên Release5838 tại QT-18, nay hiện Có hiệu lực.

### QT-17 — Quản trị viên — Đạt

- Bước bấm: Đối soát → mở kỳ6 do bộ BTC vừa lập → sau BTC xác nhận tải lại → đọc số liệu và thao tác được cung cấp.
- Mong đợi trước chạy: Đọc được trạng thái BTC xác nhận và các bước duyệt/chi trả hiện có, không tự coi là đã chi trả.
- Thực tế: Đối soát6 sau BTC phản hồi: BTC xác nhận. Nút có trên UI: Áp dụng phạm vi, Xóa bộ lọc, Duyệt đối soát

### QT-18 — Quản trị viên — Đạt

- Bước bấm: Trên bản Release cập nhật5838, Hợp đồng phí → đọc dòng hợp đồng8 → mở chi tiết → đọc nhãn trạng thái và phí; mở Đối soát6 để kiểm tra luồng liên quan.
- Mong đợi trước chạy: Danh sách/chi tiết hiển thị Có hiệu lực; API giữ nguyên status, bổ sung statusLabel; đối soát liên quan không đổi.
- Thực tế: Release cập nhật trên5838: danh sách và chi tiết hợp đồng8 hiện Có hiệu lực; phí2.00% giữ nguyên; API status=CoHieuLuc vẫn giữ, thêm statusLabel. Kỳ đối soát6 vẫn BTC xác nhận và có Duyệt đối soát.

## Phạm vi chức năng đã mở

QT-01 đã mở 22 mục của admin: hồ sơ BTC, thuê, hỗ trợ, hoàn, hợp đồng, đối soát, giải ngân, thanh toán, sự kiện, phiên bản, suất, loại vé, xét sự kiện, tài khoản, cấu hình, nhật ký, giám sát, địa điểm, khu vực, bảo trì, danh mục, chính sách. Đây là kiểm tra mở màn hình và quyền truy cập, không thay thế nghiệm thu mọi hành động trong mỗi màn hình. Nội dung đã mở được lưu tại [admin-menu.json](evidence/manager-admin/admin-menu.json).

## Lỗi đã sửa và chạy lại

Chi tiết chính sách sau Công bố phiên bản hiển thị mã `ACTIVE` thay vì tiếng Việt. Tái hiện: QT-06 soạn → công bố → mở chi tiết. Sửa `wwwroot/features/work/work.js`: phần chi tiết dùng nhãn trạng thái từ khai báo section khi API không có statusLabel, giống danh sách. Đã cập nhật static file của bản QA và chạy lại QT-03, QT-04, QT-05, QT-06; chính sách #26 hiện Đang áp dụng. Các lỗi chọn locator trong runner (tên thuê khác tên sự kiện, liên kết `/new?resource=...`, phần tử meta description trùng tên, nút đóng dialog type=submit) đã sửa trong runner và chạy lại; không ghi là lỗi sản phẩm.

## Bằng chứng tệp xuất

[rentals.xlsx](evidence/manager-admin/rentals.xlsx), [rentals.docx](evidence/manager-admin/rentals.docx), [report-preview.json](evidence/manager-admin/report-preview.json) chứa báo cáo thuê toàn phạm vi admin tại thời điểm xuất: 21 dòng. Runner tải bằng nút trên giao diện, mở ZIP/OpenXML và đọc nội dung, kiểm tra mọi giá trị của từng dòng có trong cả hai tệp. Nội dung giải mã lưu ở `rentals-xlsx-content.txt` và `rentals-docx-content.txt`. Không suy diễn đây là báo cáo giao dịch ngân hàng thật; cọc QA của ca chuyển giao dùng bộ mô phỏng có chữ ký.

## Đối chiếu yêu cầu và phần chưa chứng minh

Chỉ dùng [noidung.txt](../noidung.txt) và [chức_năng.txt](../chức_năng.txt) làm nguồn yêu cầu; mã nguồn chỉ giúp tìm màn hình và dữ liệu QA.

| Yêu cầu | Ca/bằng chứng | Giới hạn còn lại |
|---|---|---|
| noidung 4: hàng đợi, duyệt thuê, hỗ trợ; chức_năng 9,19 | QL-01–03; QT-14–15 và người gửi ND-14–15 | Chuyển tiền/cọc ngoài hệ thống không được mô phỏng là giao dịch thật |
| noidung 4, BP-02: hồ sơ BTC yêu cầu bổ sung rồi duyệt | QL-06, BTC-17 | SLA 7 ngày/ngày nghỉ chưa có ca chờ quá hạn thực |
| noidung 5.1; chức_năng 1,2 | QT-02, QT-11; QL-01 ngoài quyền | Chưa nghiệm thu giao diện tạo lời mời Quản lý mới; chỉ cấp quyền cho Người dùng có sẵn |
| noidung 5.2; chức_năng 7,8,20 | QT-03–06, QT-12–13 | Chưa chứng minh sửa/ẩn địa điểm đang có hợp đồng; ghế đôi/cụm ghế chưa chạy ở vai trò admin |
| noidung 5.5; chức_năng 20 | QT-07–08, QT-10 | Backup/restore, HTTPS, 2FA, phê duyệt cấu hình nhiều người chưa có luồng giao diện đã chạy |
| noidung 5.4; chức_năng 18 | QT-16 hợp đồng8, QT-17 kỳ đối soát6 đã được BTC xác nhận; báo cáo thuê QT-09 | Chưa duyệt/chi tiền đối soát qua giao dịch thật; xem biên bản BTC về lập kỳ và xác nhận |
| noidung BP-03,04,13: xét sự kiện/phiên bản/hoãn-hủy | QL-08 quyết định bằng quản lý nhóm1 và BTC đọc lại | Hoãn/hủy và giữ bản công khai trong khi chờ bản sửa chưa được ca này chứng minh |

Các giới hạn trên là phạm vi chưa được chứng minh, không cộng giả vào số ca Đạt. Trạng thái từng ca đã chạy nằm trong bảng và [results.json](evidence/manager-admin/results.json).

## Bổ sung sửa nhãn hợp đồng và tái nghiệm thu Release

QT-16 phát hiện mã `CoHieuLuc` xuất hiện ở danh sách/chi tiết hợp đồng. Sửa `Features/Finance/FinanceDtos.cs` bổ sung trường tùy chọn `statusLabel`, giữ nguyên `status` và các tham số cũ; `FinanceService.cs` ánh xạ đầy đủ bốn trạng thái hợp đồng: Nhap → Bản nháp, CoHieuLuc → Có hiệu lực, HetHan → Hết hạn, DaHuy → Đã hủy. Không thêm bộ lọc trạng thái vì API hợp đồng chưa hỗ trợ lọc.

Đã publish Release riêng tại `%TEMP%/qlsk-ui-c67acc32/contract-label-publish`, chạy `http://127.0.0.1:5838` cùng CSDL QA và không dừng host cũ. QT-18 dùng Edge mở danh sách, nhấp hợp đồng8, đối chiếu nhãn/2.00% và kiểm tra liên quan đối soát6 vẫn BTC xác nhận, nút Duyệt đối soát còn hiện. API vẫn trả status=CoHieuLuc, thêm statusLabel=Có hiệu lực. Bằng chứng: [danh sách](evidence/manager-admin/QT-16-list-fixed.png), [chi tiết](evidence/manager-admin/QT-16-detail-fixed.png), [đối soát liên quan](evidence/manager-admin/QT-16-related-settlement-fixed.png), [JSON đối chiếu](evidence/manager-admin/contract8-fixed.json).
