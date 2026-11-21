# Nguồn sơ đồ hệ thống QLSK

Thư mục gồm 18 sơ đồ, mỗi sơ đồ có ba bản cùng tên: `.puml` để sửa nội dung, `.svg` để phóng to và `.png` để chèn Word. Tất cả văn bản nguồn dùng UTF-8. Phần diễn giải và căn cứ mã nguồn nằm trong [he-thong.md](../he-thong.md).

Các ảnh được tạo bằng PlantUML 1.2025.10 và Eclipse Temurin JRE 21.0.13.1 trên máy cục bộ. Chỉ tải công cụ công khai; không gửi mã nguồn dự án tới dịch vụ render. Runtime và tệp tải xuống không thuộc bộ tài liệu.

Để render lại sau khi sửa, cài Java và tải PlantUML JAR từ trang phát hành chính thức, sau đó chạy tại thư mục chứa các `.puml`:

```powershell
java -jar C:/tools/plantuml.jar -charset UTF-8 -tpng *.puml
java -jar C:/tools/plantuml.jar -charset UTF-8 -tsvg *.puml
```

Biểu đồ lớp và use case có thể cần Graphviz tùy hệ điều hành/bản PlantUML. Bản Windows dùng ở lần tạo này tự cung cấp renderer phù hợp. Đặt ảnh lớn trên trang riêng trong Word, giữ tỷ lệ; bản SVG dùng khi cần chữ rõ ở mức phóng lớn.

Đã kiểm tra: 18 nguồn đều render thành công, 18 PNG giải mã được, 18 SVG phân tích XML được và không chứa thông báo lỗi cú pháp; kiểm tra trực quan toàn bộ ảnh cùng ảnh tổng quan, đặt vé và luồng thuê ở kích thước lớn.
