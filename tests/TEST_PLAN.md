# TEST PLAN
1. Hai user mua vé cuối cùng: chỉ một đơn giữ được tồn kho, DaBan + DangGiu <= SoLuong.
2. Hai request đặt cùng địa điểm/cùng thời gian: chỉ một request thành công.
3. Webhook cùng MaGiaoDich gửi nhiều lần: chỉ xử lý một lần, không tạo trùng vé.
4. Đơn hết hạn trong lúc thanh toán: webhook đến muộn chuyển thanh toán sang NghiVan, không phát hành vé.
5. Check-in lần hai: trả TICKET_ALREADY_CHECKED_IN.
6. User đọc đơn user khác: 403.
7. BanToChuc chưa xác thực tạo sự kiện: ORGANIZER_NOT_VERIFIED.
8. Người tạo tự duyệt sự kiện: 403.
9. Hoàn tiền vượt SoTien: REFUND_EXCEEDS_PAYMENT.
10. Email trùng: 409 EMAIL_EXISTS.
11. Tài khoản khóa: không đăng nhập/thao tác protected API.
12. Giá/tổng tiền đơn lấy từ DB, không nhận từ frontend.
