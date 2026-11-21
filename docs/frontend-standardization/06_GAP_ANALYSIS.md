# 06 · GAP của QLSK

Host có hai trạng thái. Khi có `ConnectionStrings:QLSK` và schema đã khởi tạo, ASP.NET Core bật các API đã triển khai. Khi thiếu kết nối, frontend vẫn phục vụ shell và hiển thị trạng thái dịch vụ chưa khả dụng. Capability theo phiên chỉ bật thao tác có handler và quyền phù hợp. [Cấu trúc backend](11_BACKEND_FEATURES.md) · [API và bảng nguồn](10_FRONTEND_API_FLOWS.md)

| Nhóm | Đã nối với SQL Server | Còn cần hoàn thiện / nghiệm thu |
|---|---|---|
| Trang chủ, sự kiện, lịch, địa điểm | Danh mục, banner, chi tiết phiên bản công bố, giá theo suất, nguồn lịch/khung bận, chính sách, đánh giá đã duyệt | Bổ sung metadata thành phố và ảnh địa điểm khi cần quản lý riêng; kiểm thử phân trang và chỉ số tồn theo dữ liệu lớn. |
| Tài khoản | Đăng ký, cookie phiên, đăng xuất/thu hồi, hồ sơ, mật khẩu, thông báo, yêu thích. Endpoint reset/xác minh email điều kiện SMTP | Đã kiểm xác minh và khôi phục qua SMTP thử nghiệm có STARTTLS. Mục yêu thích lưu ở trường `TaiKhoan.SuKienYeuThich`. Khả năng chuyển thư đến hộp thư thật phụ thuộc SMTP triển khai. |
| Ban tổ chức | Nộp cá nhân/tổ chức, tệp mã hóa, quyền mở tệp, trạng thái/lịch sử, quá hạn theo lịch nghỉ và duyệt vai trò | Giao hồ sơ cho người xử lý cụ thể, xác minh chủ tài khoản nhận tiền, hạn lưu và dọn tệp theo chính sách tổ chức. |
| Địa điểm và thuê | Tìm địa điểm, gửi yêu cầu, duyệt giữ lịch, kiểm trùng, tính 30% và hạn 24 giờ, hủy khi chưa có giao dịch, lịch bảo trì | Đã nối checkout cọc và đối soát PayOS. Còn nghiệm thu với merchant thật, xử lý hoàn cọc theo mốc 80/50/0%, gửi một hồ sơ nhiều địa điểm nếu cần. |
| Vé và đơn | Tồn kho, kiểm cụm ghế/hạn mức, chốt 0 ₫ trong giao dịch SQL, chống gửi lặp, vé QR/PDF thật, xem đơn và hủy đơn đủ điều kiện | Đã kiểm đơn thu phí, giữ ghế 10 phút, webhook, giao dịch đến muộn, cấp vé khi khớp khoản thu bằng SQL và phản hồi PayOS kiểm thử. Cần đối chiếu với tài khoản merchant triển khai. |
| Hoàn tiền và hỗ trợ | Gửi và xử lý hỗ trợ; đọc và xét duyệt yêu cầu hoàn đã có, đối chiếu số thực thu cùng giao dịch | Đã hỗ trợ tạo yêu cầu từ vé đủ điều kiện. Lệnh chuyển hoàn qua ngân hàng cần kênh chi và xác nhận thực chuyển. |
| Sự kiện, tổ chức và vận hành | Tạo bản nháp, bản sửa, phiên bản/suất/loại vé, duyệt và công bố, đình chỉ, phân quyền cổng và check-in chống lặp | Hoãn/hủy có vé hoặc tiền cần quy trình thông báo/hoàn trước khi duyệt. Công cụ biên tập sơ đồ trực quan, mời nhân sự chưa có tài khoản, quản lý chiến dịch voucher còn cần hoàn thiện. Đã bổ sung cấp và thu hồi vé mời theo sự kiện. |
| Quản trị và tài chính | Tài khoản và nhóm quyền, danh mục/banner, địa điểm/khu vực/lịch bảo trì, chính sách, ngày nghỉ/phí dịch vụ, giao dịch và hợp đồng, phản hồi/duyệt đối soát có sẵn, đọc giải ngân, giám sát/audit | Nghiệm thu khóa sổ trên dữ liệu triển khai và kết nối kênh chi theo thẩm quyền, nhập chứng từ và xác nhận chuyển tiền. Cấu hình hạ tầng, secrets và sao lưu dùng quy trình vận hành riêng. |
| Báo cáo | Dataset phân quyền của đơn, check-in, hồ sơ BTC, thuê, hoàn; Excel/Word và in/lưu PDF | Báo cáo tài chính tổng hợp, lịch xuất định kỳ, nghiệm thu đối chiếu số liệu trên dữ liệu nhiều kỳ. |

## Khác biệt đã chọn

- Giao diện chính thức mang tên QLSK. Bản mẫu VÉNUE chạy độc lập trong `/giaodientest/` và tiếp tục dùng dữ liệu mô phỏng.
- Mẫu lưu phiên và đổi vai trò trong trình duyệt. QLSK đọc vai trò từ `TaiKhoan`, nhóm quyền nền tảng và quyền theo `PhanCongSuKien`. API kiểm lại quyền trên mỗi phạm vi thao tác.
- Đăng ký chính thức nhận ít nhất một kênh email/số điện thoại và có tên đăng nhập. Việc xác minh và khôi phục email phụ thuộc cấu hình SMTP. [Nghiệp vụ tài khoản](../noidung.txt#L37)
- Mẫu gộp nhiều địa điểm và nhiều giao dịch hoàn tiền. API bao mỗi yêu cầu thuê theo một địa điểm/khung giờ. Khách chọn vé đủ điều kiện để tạo yêu cầu hoàn. Mỗi yêu cầu gắn một vé và một khoản thu. [Quy trình thuê](../noidung.txt#L316)
- Chi tiết địa điểm QLSK dùng ảnh minh họa có nhãn khi API chưa có ảnh địa điểm. Dải lịch hiển thị khung đã bận mà máy chủ công bố. Khung thuê được xác nhận qua xét duyệt, giữ lịch và đối soát cọc. Biểu mẫu nhận ngày từ lịch và điền 09:00 làm giờ bắt đầu gợi ý khi thời điểm đó còn ở tương lai.
- Hộp xuất của bản mẫu tạo tệp từ bảng mô phỏng. QLSK tải Excel/Word từ máy chủ và in bảng đã được phép đọc. Loại báo cáo và nút gửi xuất phụ thuộc quyền và dữ liệu trong phạm vi.
- Tập lệnh bổ sung schema giữ các cột mới ở dạng nullable cho bản ghi cũ. Đã đối chiếu tên bảng, khóa và quan hệ với lược đồ gốc 37 bảng trong tài liệu 04/05. [Schema bổ sung](../../Data/Migrations/001-qlsk-features.sql)
