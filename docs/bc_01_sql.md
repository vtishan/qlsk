# Báo cáo kiểm thử cơ sở dữ liệu QLSK

## Nạp dữ liệu demo và kiểm tra các trang ngày 21/11/2026

Đã chạy migration và seed trên SQL Server LocalDB `(localdb)\MSSQLLocalDB` cho cơ sở dữ liệu `quanlysukien`. Ứng dụng kết nối bằng SQL login `quanlysukien`. Tài khoản quản trị dùng để đăng nhập trang web là `admin@qlsk.test`.

### Dữ liệu đã đối chiếu trên SQL Server

Cả **37/37 bảng nghiệp vụ** đều có bản ghi. SQL Server ghi nhận `QLSK.SchemaVersion = 2` và `QLSK.DemoSeedVersion = 1`. Khi chạy lại `--qlsk-seed-demo`, số dòng của từng bảng giữ nguyên. Kịch bản kiểm tra khóa và ràng buộc cũng chạy thành công trên cơ sở dữ liệu này. [Migration SQL](../Data/Migrations/) · [Bộ seed](../Data/seed/) · [Kịch bản kiểm tra ràng buộc](../test/RangBuoc.sql)

| Trang và nghiệp vụ | Dữ liệu thực tế sau khi nạp |
|---|---|
| Trang chủ, sự kiện, lịch và địa điểm | 4 banner, 6 danh mục, 14 sự kiện, 17 phiên bản, 14 suất diễn, 5 địa điểm và 256 ghế. Có suất đang bán vé, sự kiện đã kết thúc, hoãn và hủy. |
| Tài khoản và ban tổ chức | 7 tài khoản, 5 hồ sơ BTC, 5 giấy tờ mẫu và 6 thông báo. Khách mua vé có danh sách yêu thích và lịch sử xem. |
| Đặt vé, khuyến mãi và soát vé | 38 loại vé, 21 đơn hàng, 37 vé, 5 lời mời, 7 voucher, 11 phân công và 14 lượt ghi nhật ký check-in. |
| Thuê địa điểm và hỗ trợ | 10 yêu cầu thuê, 14 bản ghi lịch địa điểm và 4 hồ sơ hỗ trợ. Có hồ sơ chờ duyệt, chờ cọc, đã xác nhận và quá hạn. |
| Tài chính và báo cáo | 25 giao dịch thanh toán, 10 yêu cầu hoàn tiền, 6 giao dịch hoàn tiền, 14 hợp đồng, 6 kỳ đối soát và 2 lệnh giải ngân. |
| Quản trị | 4 chính sách, 4 thiết lập nghiệp vụ và 46 bản ghi nhật ký hệ thống. |

### Kết quả kiểm thử theo vai trò

| Phép kiểm tra | Môi trường | Kết quả |
|---|---|---|
| Đăng nhập, tải dữ liệu và biểu mẫu, đọc chi tiết, mở tài liệu BTC, xem và xuất báo cáo | `quanlysukien` | **244/244 lượt kiểm tra đạt** trên phiên khách và 7 tài khoản demo. Bao gồm kiểm tra dữ liệu theo quyền và xuất tệp Excel, Word. |
| Mở các trang bằng Microsoft Edge, chọn đúng sự kiện và suất diễn theo quyền | `QLSK_Demo`, cơ sở dữ liệu thử dùng trong quá trình xây dựng bộ seed | **420 lượt mở, 208 đường dẫn phân biệt**, không ghi nhận lỗi trình duyệt hoặc HTTP ngoài dự kiến. Đã mở sơ đồ ghế tại ba địa điểm đang hoạt động và các trang chi tiết đại diện. |

Các phép kiểm tra dùng giao dịch tài chính mô phỏng. Thanh toán và chuyển tiền qua PayOS thật nằm ngoài phạm vi đợt này. Danh sách từng phép kiểm tra trên `quanlysukien` và số lượt mở trang theo vai trò được lưu cùng báo cáo. [Bằng chứng kiểm thử](testcases/evidence/seed-demo/kiem-thu.json) · [Lệnh chạy lại và tài khoản theo vai trò](frontend-standardization/11_BACKEND_FEATURES.md#chạy-bộ-demo-trên-windows)

## Kiểm tra lược đồ và ràng buộc ngày 20/11/2026

**Ngày kiểm tra** 20/11/2026  
**Môi trường** .NET 10, EF Core 10.0.14 và SQL Server 2025 LocalDB  
**Phạm vi** 37 bảng, khóa, chỉ mục và ràng buộc dữ liệu theo [bảng.txt](bảng.txt).

| Mã | Thao tác | Kết quả thực tế | Kết luận |
|---|---|---|---|
| KT-01 | Biên dịch và sinh mã SQL | Biên dịch thành công, 0 cảnh báo, 0 lỗi. SQL sinh 37 lệnh `CREATE TABLE`. | Đạt |
| KT-02 | Tạo cơ sở dữ liệu và đối chiếu cấu trúc với tài liệu | Có 37 bảng, 37 khóa chính `Id` tự tăng, 15 chỉ mục duy nhất có lọc. Tên cột và khả năng nhận NULL khớp tài liệu. Khóa ngoại dùng `NO ACTION`. | Đạt |
| KT-03 | Thêm tài khoản chỉ có email và bỏ qua vai trò | Hai tài khoản dùng số điện thoại NULL được tạo với `VaiTro = 1`. SQL Server từ chối email trùng và tài khoản thiếu cả hai liên hệ. Số bảng `VaiTro` là 0. | Đạt |
| KT-04 | Tạo sự kiện, thêm phiên bản và cập nhật phiên bản công khai | Sự kiện nhận mã tự tăng và lưu được tham chiếu tới phiên bản của chính nó. | Đạt |
| KT-05 | Cấp quyền toàn sự kiện và theo suất, sau đó lặp lại từng quyền | Hai phạm vi cùng tồn tại. SQL Server từ chối phân công trùng trong từng phạm vi. | Đạt |
| KT-06 | Tạo hai lượt giữ cùng ghế, sau đó thử phát hai vé còn hiệu lực cho ghế đó | SQL Server từ chối lượt giữ thứ hai và vé thứ hai. | Đạt |
| KT-07 | Ghi hai lượt check-in thành công cho cùng vé | SQL Server từ chối lượt thứ hai. | Đạt |
| KT-08 | Ghi lặp mã webhook | SQL Server từ chối bản ghi thứ hai. | Đạt |

Lần chạy đầu phát hiện lỗi `SuKien.Id` thiếu `IDENTITY` vì cột tham gia khóa ngoại ghép. Đã khai báo tự tăng trong EF Core, tạo lại cơ sở dữ liệu kiểm thử và chạy lại KT-01 đến KT-08. [Cấu hình sự kiện](../Data/Configs/CauHinhSuKien.cs)

Để chạy lại, cấu hình `ConnectionStrings__Qlsk` cho cơ sở dữ liệu kiểm thử trống, chạy `dotnet run -- --qlsk-init`, rồi thực thi kịch bản SQL trên cơ sở dữ liệu vừa tạo. Kịch bản hoàn tác dữ liệu mẫu khi kết thúc. [RangBuoc.sql](../test/RangBuoc.sql)

Các luồng chưa được nghiệm thu ở cấp ứng dụng gồm giữ ghế đồng thời giữa nhiều yêu cầu, thanh toán PayOS, tự động hết hạn và hoàn tiền. Báo cáo này ghi nhận kết quả ở cấp cơ sở dữ liệu.
