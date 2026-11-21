# Physical ERD hệ thống quản lý và tổ chức sự kiện

Sáu sơ đồ thể hiện **37 bảng** và **75 khóa ngoại** theo mô hình EF Core. Bảng được khai báo đủ cột trong nhóm của nó. Để đọc trên giấy A4, mở [sơ đồ theo luồng nghiệp vụ](bao-cao/erd/README.md) hoặc [bản tra cứu từng bảng và khóa ngoại](bao-cao/erd/Tra-cuu-37-bang.pdf).

[Ảnh PNG ERD tổng thể](anh_erd/07-erd-tong-the-37-bang.png) trình bày đủ 37 bảng và 75 khóa ngoại trên cùng một sơ đồ, kích thước **11.124 × 6.263 px**. Màu tiêu đề bảng cho biết phân hệ.

Sáu ảnh theo phân hệ cùng kích thước **5.200 × 5.440 px**.

| Nhóm | Số bảng | Ảnh |
|---|---:|---|
| [Tài khoản và phân quyền](#1-tài-khoản-và-phân-quyền) | 4 | [PNG](anh_erd/01-tai-khoan-va-phan-quyen.png) |
| [Quản lý sự kiện](#2-quản-lý-sự-kiện) | 4 | [PNG](anh_erd/02-quan-ly-su-kien.png) |
| [Địa điểm và ghế](#3-địa-điểm-và-ghế) | 6 | [PNG](anh_erd/03-dia-diem-va-ghe.png) |
| [Đặt vé và check-in](#4-đặt-vé-và-check-in) | 8 | [PNG](anh_erd/04-dat-ve-va-check-in.png) |
| [Thanh toán và tài chính](#5-thanh-toán-và-tài-chính) | 8 | [PNG](anh_erd/05-thanh-toan-va-tai-chinh.png) |
| [Quản trị và hỗ trợ](#6-quản-trị-và-hỗ-trợ) | 7 | [PNG](anh_erd/06-quan-tri-va-ho-tro.png) |

Mỗi ảnh PNG có chú giải và đường nối vuông góc. Ảnh 04, 05 và 06 có các khung theo luồng nghiệp vụ. Ô **Tham chiếu** chỉ đến bảng được trình bày đầy đủ ở khung hoặc phân hệ khác.

| Ký hiệu | Ý nghĩa |
|---|---|
| Hai vạch | Đúng một (1..1) |
| Vòng tròn và vạch | Không hoặc một (0..1) |
| Vạch và chân quạ | Một hoặc nhiều (1..N) |
| Vòng tròn và chân quạ | Không hoặc nhiều (0..N) |
| Nét liền | Quan hệ định danh. Khóa ngoại tham gia xác định khóa chính của bản ghi con. |
| Nét đứt | Quan hệ không định danh. Bản ghi con có khóa chính độc lập. |
| `PK`, `FK`, `UK` | Khóa chính, khóa ngoại, cột duy nhất. |
| `NULL`, `—` | Cột cho phép NULL hoặc bắt buộc có giá trị. |
| `FK ghép C1`, `FK ghép C2` | Khóa ngoại ghép, đánh số trong từng bảng con. Cặp danh sách cột có thứ tự nằm ở phần ràng buộc của bảng. |
| `UQ`, `UX`, `UXF` | Ràng buộc SQL `UNIQUE`, chỉ mục duy nhất, chỉ mục duy nhất có lọc. |
| `JSON` | Cột `nvarchar(max)` lưu JSON. Kiểm tra `ISJSON` áp dụng với các cột có ràng buộc tương ứng. |

## 1. Tài khoản và phân quyền

```mermaid
erDiagram
    direction TB
    TaiKhoan {
        int Id PK
        nvarchar(100) HoTen
        nvarchar(255) Email UK "NULL"
        nvarchar(15) SoDienThoai UK "NULL"
        nvarchar(50) TenDangNhap UK
        nvarchar(500) MatKhauBam
        int VaiTro "mặc định 1"
        nvarchar(32) TrangThai
        datetime2(3) NgayTao
        datetime2(3) EmailXacMinhLuc "NULL"
        datetime2(3) SoDienThoaiXacMinhLuc "NULL"
        nvarchar(128) MaBamKhoiPhuc "NULL"
        datetime2(3) HanKhoiPhuc "NULL"
        int PhienBanDangNhap
        nvarchar(max) QuyenQuanLy "NULL · JSON"
        nvarchar(max) SuKienYeuThich "NULL · JSON"
        nvarchar(max) SuKienDaXem "NULL · JSON"
    }

    HoSoBTC {
        int Id PK
        int TaiKhoanId FK
        nvarchar(200) TenToChuc
        nvarchar(120) NguoiDaiDien
        nvarchar(20) MaSoThue "NULL"
        nvarchar(300) DiaChi
        nvarchar(300) ThongTinLienHe
        nvarchar(max) ThongTinNhanTienMaHoa "NULL"
        nvarchar(32) TrangThai
        nvarchar(max) LyDoTuChoi "NULL"
        datetime2(3) NgayNop "NULL"
        datetime2(3) HanDuyet "NULL"
        int NguoiDuyetId FK "NULL"
        datetime2(3) NgayDuyet "NULL"
    }

    GiayToBTC {
        int Id PK
        int HoSoBTCId FK
        nvarchar(32) LoaiGiayTo
        nvarchar(500) KhoaLuuTru
        nvarchar(255) TenTep
        nvarchar(32) LoaiNoiDung
        datetime2(3) NgayTaiLen
    }

    PhanCongSuKien {
        int Id PK
        int SuKienId FK
        int SuatDienId FK "NULL"
        int TaiKhoanId FK "NULL"
        nvarchar(255) EmailMoi "NULL"
        nvarchar(128) MaBamLoiMoi "NULL"
        datetime2(3) HanLoiMoi "NULL"
        int NguoiPhanCongId FK
        nvarchar(64) LoaiQuyen
        datetime2(3) NgayPhanCong
        datetime2(3) NgayThuHoi "NULL"
        nvarchar(32) TrangThai
    }

    TaiKhoan |o..o{ HoSoBTC : "NguoiDuyetId"
    TaiKhoan ||..o{ HoSoBTC : "TaiKhoanId"
    HoSoBTC ||..o{ GiayToBTC : "HoSoBTCId"
    SuKien ||..o{ PhanCongSuKien : "SuKienId"
    SuatDien |o..o{ PhanCongSuKien : "FK ghép C1"
    TaiKhoan ||..o{ PhanCongSuKien : "NguoiPhanCongId"
    TaiKhoan |o..o{ PhanCongSuKien : "TaiKhoanId"
```

`TaiKhoan.VaiTro` mặc định `1`. `PhanCongSuKien` có hai chỉ mục duy nhất có lọc cho quyền đang hiệu lực, theo toàn sự kiện hoặc theo từng suất.

`TaiKhoan.SuKienYeuThich` và `TaiKhoan.SuKienDaXem` là mảng ID lưu bằng JSON. Lịch sử xem giữ tối đa 20 sự kiện theo thứ tự xem gần nhất. API đọc danh sách theo tài khoản đăng nhập và chỉ trả sự kiện còn công khai.

`PhanCongSuKien.C1`: `(SuatDienId, SuKienId)` → `SuatDien(Id, SuKienId)`. Cặp đích là ràng buộc `UQ`.

## 2. Quản lý sự kiện

```mermaid
erDiagram
    direction TB
    SuKien {
        int Id PK
        int BTCId FK
        int DanhMucId FK
        nvarchar(200) TenSuKien
        nvarchar(max) MoTa
        nvarchar(500) AnhBia
        nvarchar(32) TrangThai
        datetime2(3) NgayTao
        int PhienBanCongKhaiId FK "NULL"
        bit ChoPhepTuHuyVe
        datetime2(3) HanHoanKhiHoan "NULL"
    }

    PhienBanSuKien {
        int Id PK
        int SuKienId FK
        int SoPhienBan
        nvarchar(32) LoaiYeuCau
        nvarchar(max) NoiDung "JSON"
        nvarchar(max) LyDo "NULL"
        nvarchar(32) TrangThaiDuyet
        int NguoiGuiId FK
        datetime2(3) NgayGui "NULL"
        int NguoiDuyetId FK "NULL"
        datetime2(3) NgayDuyet "NULL"
        nvarchar(max) KetQuaDuyet "NULL"
    }

    SuatDien {
        int Id PK
        int SuKienId FK
        int DiaDiemId FK
        datetime2(3) ThoiGianBatDau
        datetime2(3) ThoiGianKetThuc
        datetime2(3) ThoiGianMoBan
        datetime2(3) ThoiGianDongBan
        nvarchar(max) SoDoChot "JSON"
        int SucChua
        nvarchar(32) TrangThai
    }

    DanhMucSuKien {
        int Id PK
        nvarchar(120) TenDanhMuc
        nvarchar(max) MoTa "NULL"
        nvarchar(32) TrangThai
    }

    DanhMucSuKien ||..o{ SuKien : "DanhMucId"
    HoSoBTC ||..o{ SuKien : "BTCId"
    PhienBanSuKien |o..o| SuKien : "FK ghép C1"
    SuKien ||..o{ PhienBanSuKien : "SuKienId"
    TaiKhoan |o..o{ PhienBanSuKien : "NguoiDuyetId"
    TaiKhoan ||..o{ PhienBanSuKien : "NguoiGuiId"
    DiaDiem ||..o{ SuatDien : "DiaDiemId"
    SuKien ||..o{ SuatDien : "SuKienId"
```

`SuKien` sở hữu nhiều phiên bản qua `PhienBanSuKien.SuKienId`. `SuKien.Id` là PK độc lập và cũng xuất hiện trong danh sách cột của ràng buộc tham chiếu phiên bản.

`SuKien.C1`: `(PhienBanCongKhaiId, Id)` → `PhienBanSuKien(Id, SuKienId)`. Cặp đích là `UQ`, giới hạn phiên bản công khai trong cùng sự kiện.

Khi duyệt lần đầu, `SuKien.TrangThai = DaDuyet` và `PhienBanCongKhaiId` vẫn là `NULL`. BTC xem trước nội dung, suất và `LoaiVe.GiaVe` rồi bấm **Công khai** để gán phiên bản công khai. Mở lại bản nháp giữ các suất và giá vé, đồng thời yêu cầu duyệt lại.

`SuatDien.SoDoChot` lưu thêm khóa JSON `rentalId` khi BTC gắn hồ sơ thuê. Ứng dụng kiểm tra ID trỏ đến `YeuCauThue` đã xác nhận của BTC, đúng địa điểm và phủ đủ giờ diễn. Liên kết trong JSON do ứng dụng kiểm tra. Các khóa ngoại vật lý của lịch nằm ở `LichDatDiaDiem`.

## 3. Địa điểm và ghế

```mermaid
erDiagram
    direction TB
    DiaDiem {
        int Id PK
        nvarchar(200) TenDiaDiem
        nvarchar(300) DiaChi
        int SucChua
        decimal GiaThue "19,0"
        nvarchar(max) TienIch "NULL"
        nvarchar(32) TrangThai
    }

    KhuVuc {
        int Id PK
        int DiaDiemId FK
        nvarchar(120) TenKhuVuc
        int SucChua
    }

    Ghe {
        int Id PK
        int KhuVucId FK
        int CumGheId FK "NULL"
        nvarchar(32) MaGhe
        nvarchar(32) LoaiGhe
        nvarchar(32) TrangThai
    }

    CumGhe {
        int Id PK
        int KhuVucId FK
        nvarchar(120) TenCum
        nvarchar(32) LoaiCum
        int SoCho
    }

    YeuCauThue {
        int Id PK
        int TaiKhoanId FK
        int DiaDiemId FK
        datetime2(3) ThoiGianBatDau
        datetime2(3) ThoiGianKetThuc
        decimal TongTien "19,0"
        decimal TienCoc "19,0"
        datetime2(3) HanDatCoc "NULL"
        int ChinhSachId FK
        nvarchar(32) TrangThai
        int NguoiDuyetId FK "NULL"
        nvarchar(max) LyDoXuLy "NULL"
        datetime2(3) NgayHuy "NULL"
        datetime2(3) NgayTao "NULL"
        nvarchar(max) NoiDungYeuCau "NULL"
    }

    LichDatDiaDiem {
        int Id PK
        int DiaDiemId FK
        int YeuCauThueId FK "NULL"
        int SuatDienId FK "NULL"
        datetime2(3) BatDau
        datetime2(3) KetThuc
        nvarchar(32) LoaiLich
        nvarchar(32) TrangThai
    }

    DiaDiem ||..o{ KhuVuc : "DiaDiemId"
    CumGhe |o..o{ Ghe : "FK ghép C1"
    KhuVuc ||..o{ Ghe : "KhuVucId"
    KhuVuc ||..o{ CumGhe : "KhuVucId"
    ChinhSach ||..o{ YeuCauThue : "ChinhSachId"
    DiaDiem ||..o{ YeuCauThue : "DiaDiemId"
    TaiKhoan |o..o{ YeuCauThue : "NguoiDuyetId"
    TaiKhoan ||..o{ YeuCauThue : "TaiKhoanId"
    DiaDiem ||..o{ LichDatDiaDiem : "DiaDiemId"
    SuatDien |o..o{ LichDatDiaDiem : "FK ghép C1"
    YeuCauThue |o..o{ LichDatDiaDiem : "FK ghép C2"
```

Khóa ghép của `Ghe` bảo đảm ghế thuộc cùng khu vực với cụm. Hai khóa ghép của `LichDatDiaDiem` bảo đảm lịch cùng địa điểm với yêu cầu thuê hoặc suất diễn được liên kết.

| FK ghép | Cột tham chiếu | Cặp cột đích có `UQ` |
|---|---|---|
| `Ghe.C1` | `(CumGheId, KhuVucId)` | `CumGhe(Id, KhuVucId)` |
| `LichDatDiaDiem.C1` | `(SuatDienId, DiaDiemId)` | `SuatDien(Id, DiaDiemId)` |
| `LichDatDiaDiem.C2` | `(YeuCauThueId, DiaDiemId)` | `YeuCauThue(Id, DiaDiemId)` |

## 4. Đặt vé và check-in

```mermaid
erDiagram
    direction TB
    LoaiVe {
        int Id PK
        int SuatDienId FK
        int KhuVucId FK
        nvarchar(120) TenLoaiVe
        nvarchar(32) NhomVe
        decimal GiaVe "19,0"
        int SoLuong
        nvarchar(max) QuyenLoi "NULL"
        datetime2(3) BatDauBan
        datetime2(3) KetThucBan
    }

    DonHang {
        int Id PK
        int TaiKhoanId FK
        int SuatDienId FK
        int HopDongId FK "NULL"
        int ChinhSachId FK
        decimal TienVe "19,0"
        decimal TienGiam "19,0"
        decimal PhiDichVu "19,0"
        decimal PhiNenTang "19,0"
        decimal TongTien "19,0"
        nvarchar(32) TrangThai
        datetime2(3) NgayTao
        datetime2(3) HanThanhToan
        nvarchar(64) KhoaTaoDon "NULL"
        nvarchar(64) DauVanTayTaoDon "NULL"
    }

    ChiTietDonHang {
        int Id PK
        int DonHangId FK
        int LoaiVeId FK
        int SoLuong
        decimal DonGia "19,0"
        decimal ThanhTien "19,0"
    }

    GiuGhe {
        int Id PK
        int SuatDienId FK
        int GheId FK "NULL"
        int ChiTietDonHangId FK
        datetime2(3) HetHan
        nvarchar(32) TrangThai
    }

    Ve {
        int Id PK
        int ChiTietDonHangId FK "NULL"
        int GiuGheId FK, UK "NULL"
        int LoaiVeId FK
        int SuatDienId FK
        int GheId FK "NULL"
        nvarchar(64) MaVe UK
        nvarchar(128) MaQR UK
        nvarchar(32) TrangThai
        datetime2(3) NgayPhatHanh
        datetime2(3) DaCheckInLuc "NULL"
    }

    VeMoi {
        int Id PK
        int VeId FK, UK
        nvarchar(120) HoTenKhachMoi
        nvarchar(255) Email "NULL"
        nvarchar(15) SoDienThoai "NULL"
        int TaiKhoanNhanId FK "NULL"
        nvarchar(128) MaBamNhanVe "NULL"
        datetime2(3) HanNhanVe "NULL"
        datetime2(3) DaGuiLuc "NULL"
        datetime2(3) DaNhanLuc "NULL"
        nvarchar(32) TrangThai
    }

    NhatKyCheckIn {
        int Id PK
        int VeId FK
        int NhanVienId FK
        datetime2(3) ThoiGianQuet
        nvarchar(32) KetQua
        nvarchar(32) HinhThuc
    }

    Voucher {
        int Id PK
        nvarchar(64) MaVoucher UK
        int SuKienId FK "NULL"
        int SuatDienId FK "NULL"
        int KhuVucId FK "NULL"
        nvarchar(32) LoaiGiam
        decimal GiaTri "19,2"
        decimal MucGiamToiDa "19,0 · NULL"
        decimal GiaTriDonToiThieu "19,0"
        int SoLuong
        int LuotMoiTaiKhoan
        nvarchar(32) NguonTaiTro
        datetime2(3) NgayBatDau
        datetime2(3) NgayKetThuc
        nvarchar(32) TrangThai
    }

    KhuVuc ||..o{ LoaiVe : "KhuVucId"
    SuatDien ||..o{ LoaiVe : "SuatDienId"
    ChinhSach ||..o{ DonHang : "ChinhSachId"
    HopDong |o..o{ DonHang : "HopDongId"
    SuatDien ||..o{ DonHang : "SuatDienId"
    TaiKhoan ||..o{ DonHang : "TaiKhoanId"
    DonHang ||..o{ ChiTietDonHang : "DonHangId"
    LoaiVe ||..o{ ChiTietDonHang : "LoaiVeId"
    ChiTietDonHang ||..o{ GiuGhe : "ChiTietDonHangId"
    Ghe |o..o{ GiuGhe : "GheId"
    SuatDien ||..o{ GiuGhe : "SuatDienId"
    ChiTietDonHang |o..o{ Ve : "ChiTietDonHangId"
    Ghe |o..o{ Ve : "GheId"
    GiuGhe |o..o| Ve : "GiuGheId"
    LoaiVe ||..o{ Ve : "FK ghép C1"
    SuatDien ||..o{ Ve : "SuatDienId"
    TaiKhoan |o..o{ VeMoi : "TaiKhoanNhanId"
    Ve ||..o| VeMoi : "VeId"
    TaiKhoan ||..o{ NhatKyCheckIn : "NhanVienId"
    Ve ||..o{ NhatKyCheckIn : "VeId"
    KhuVuc |o..o{ Voucher : "KhuVucId"
    SuKien |o..o{ Voucher : "SuKienId"
    SuatDien |o..o{ Voucher : "SuatDienId"
```

Các chỉ mục có lọc giới hạn mỗi ghế đang giữ và mỗi ghế đã cấp còn hiệu lực theo suất. `NhatKyCheckIn` lưu nhiều lần quét, với tối đa một lần thành công trên mỗi vé.

`Ve.C1`: `(LoaiVeId, SuatDienId)` → `LoaiVe(Id, SuatDienId)`. Cặp đích là ràng buộc `UQ`.

## 5. Thanh toán và tài chính

```mermaid
erDiagram
    direction TB
    SuDungVoucher {
        int Id PK
        int VoucherId FK
        int DonHangId FK, UK
        decimal SoTienGiam "19,0"
        datetime2(3) HetHanGiuLuot
        datetime2(3) DaChotLuc "NULL"
        nvarchar(32) TrangThai
    }

    GiaoDichThanhToan {
        int Id PK
        int DonHangId FK "NULL"
        int YeuCauThueId FK "NULL"
        nvarchar(128) MaDonNhaCungCap UK "NULL"
        nvarchar(128) MaGiaoDich UK "NULL"
        decimal SoTienYeuCau "19,0"
        decimal SoTienThucThu "19,0 · NULL"
        nvarchar(32) PhuongThuc
        nvarchar(32) TrangThai
        datetime2(3) NgayThanhToan "NULL"
        nvarchar(500) ChungTu "NULL"
    }

    WebhookThanhToan {
        int Id PK
        int GiaoDichId FK "NULL"
        nvarchar(128) MaSuKienWebhook UK
        nvarchar(max) NoiDungDaLoc "JSON"
        datetime2(3) ThoiGianNhan
        nvarchar(32) TrangThaiXuLy
    }

    YeuCauHoanTien {
        int Id PK
        int TaiKhoanId FK
        int GiaoDichId FK
        int VeId FK "NULL"
        nvarchar(128) KhoaChongLap UK
        decimal SoTienYeuCau "19,0"
        decimal SoTienDuyet "19,0 · NULL"
        nvarchar(32) LoaiYeuCau
        nvarchar(max) LyDo
        nvarchar(32) TrangThai
        datetime2(3) NgayYeuCau
        int NguoiDuyetId FK "NULL"
        datetime2(3) NgayDuyet "NULL"
    }

    GiaoDichHoanTien {
        int Id PK
        int YeuCauHoanTienId FK
        nvarchar(128) KhoaChongLap UK
        nvarchar(128) MaGiaoDichHoan UK "NULL"
        decimal SoTien "19,0"
        nvarchar(32) TrangThai
        datetime2(3) NgayXuLy "NULL"
        nvarchar(500) ChungTu "NULL"
    }

    HopDong {
        int Id PK
        int BTCId FK
        int SuKienId FK "NULL"
        nvarchar(32) LoaiPhi
        decimal TyLePhi "5,2 · NULL"
        decimal PhiMoiVe "19,0 · NULL"
        nvarchar(max) NoiDung
        datetime2(3) NgayKy
        datetime2(3) HieuLucTu
        datetime2(3) HieuLucDen "NULL"
        nvarchar(32) TrangThai
    }

    DoiSoat {
        int Id PK
        int SuKienId FK
        int PhienBan
        date TuNgay
        date DenNgay
        decimal TongDoanhThu "19,0"
        decimal TongTienHoan "19,0"
        decimal PhiNenTang "19,0"
        decimal TienDieuChinh "19,0"
        decimal TienBTCNhan "19,0"
        nvarchar(max) ChiTietDoiSoat "JSON"
        datetime2(3) HanXacNhan
        int NguoiDuyetId FK "NULL"
        datetime2(3) NgayDuyet "NULL"
        nvarchar(32) TrangThai
    }

    GiaiNgan {
        int Id PK
        int DoiSoatId FK, UK
        nvarchar(128) KhoaChongLap UK
        nvarchar(128) MaGiaoDich UK "NULL"
        decimal SoTien "19,0"
        nvarchar(32) TrangThai
        datetime2(3) NgayGiaiNgan "NULL"
        nvarchar(500) ChungTu "NULL"
    }

    DonHang ||..o| SuDungVoucher : "DonHangId"
    Voucher ||..o{ SuDungVoucher : "VoucherId"
    DonHang |o..o{ GiaoDichThanhToan : "DonHangId"
    YeuCauThue |o..o{ GiaoDichThanhToan : "YeuCauThueId"
    GiaoDichThanhToan |o..o{ WebhookThanhToan : "GiaoDichId"
    GiaoDichThanhToan ||..o{ YeuCauHoanTien : "GiaoDichId"
    TaiKhoan |o..o{ YeuCauHoanTien : "NguoiDuyetId"
    TaiKhoan ||..o{ YeuCauHoanTien : "TaiKhoanId"
    Ve |o..o{ YeuCauHoanTien : "VeId"
    YeuCauHoanTien ||..o{ GiaoDichHoanTien : "YeuCauHoanTienId"
    HoSoBTC ||..o{ HopDong : "BTCId"
    SuKien |o..o{ HopDong : "SuKienId"
    SuKien ||..o{ DoiSoat : "SuKienId"
    TaiKhoan |o..o{ DoiSoat : "NguoiDuyetId"
    DoiSoat ||..o| GiaiNgan : "DoiSoatId"
```

`GiaoDichThanhToan` gắn đúng một trong `DonHang` và `YeuCauThue`. Mỗi bên có thể có nhiều lần thử, với tối đa một giao dịch thành công. Mỗi bản `DoiSoat` có tối đa một `GiaiNgan`.

## 6. Quản trị và hỗ trợ

```mermaid
erDiagram
    direction TB
    ThongBao {
        int Id PK
        int TaiKhoanId FK "NULL"
        int VeMoiId FK "NULL"
        nvarchar(200) TieuDe
        nvarchar(max) NoiDung
        nvarchar(32) LoaiThongBao
        nvarchar(32) KenhGui
        nvarchar(32) TrangThaiGui
        bit DaDoc
        datetime2(3) NgayTao
        datetime2(3) NgayGui "NULL"
    }

    KhieuNai {
        int Id PK
        int TaiKhoanId FK
        int SuKienId FK "NULL"
        int VeId FK "NULL"
        int YeuCauThueId FK "NULL"
        nvarchar(200) TieuDe
        nvarchar(max) NoiDung
        nvarchar(32) TrangThai
        int NguoiXuLyId FK "NULL"
        datetime2(3) NgayTao
        nvarchar(max) KetQuaXuLy "NULL"
    }

    DanhGia {
        int Id PK
        int TaiKhoanId FK
        int SuKienId FK
        int SoSao
        nvarchar(max) BinhLuan "NULL"
        datetime2(3) NgayDanhGia
        nvarchar(32) TrangThai
    }

    Banner {
        int Id PK
        nvarchar(200) TieuDe
        nvarchar(500) HinhAnh
        nvarchar(500) DuongDan "NULL"
        int ThuTu
        nvarchar(32) TrangThai
    }

    CauHinhHeThong {
        int Id PK
        nvarchar(100) Khoa UK
        nvarchar(max) GiaTri
        nvarchar(max) MoTa "NULL"
        datetime2(3) NgayCapNhat
        int NguoiCapNhatId FK
    }

    NhatKyHeThong {
        int Id PK
        int TaiKhoanId FK "NULL"
        nvarchar(100) HanhDong
        nvarchar(100) DoiTuong
        int DoiTuongId "NULL"
        datetime2(3) ThoiGian
        nvarchar(max) DuLieuThayDoi "NULL · JSON"
    }

    ChinhSach {
        int Id PK
        nvarchar(120) TenChinhSach
        nvarchar(max) NoiDung
        int PhienBan
        date NgayHieuLuc
        nvarchar(32) TrangThai
    }

    TaiKhoan |o..o{ ThongBao : "TaiKhoanId"
    VeMoi |o..o{ ThongBao : "VeMoiId"
    SuKien |o..o{ KhieuNai : "SuKienId"
    TaiKhoan |o..o{ KhieuNai : "NguoiXuLyId"
    TaiKhoan ||..o{ KhieuNai : "TaiKhoanId"
    Ve |o..o{ KhieuNai : "VeId"
    YeuCauThue |o..o{ KhieuNai : "YeuCauThueId"
    SuKien ||..o{ DanhGia : "SuKienId"
    TaiKhoan ||..o{ DanhGia : "TaiKhoanId"
    TaiKhoan ||..o{ CauHinhHeThong : "NguoiCapNhatId"
    TaiKhoan |o..o{ NhatKyHeThong : "TaiKhoanId"
```

`ThongBao` có đúng một đích nhận qua `TaiKhoan` hoặc `VeMoi`. `DanhGia` duy nhất theo cặp tài khoản và sự kiện. `NhatKyHeThong.DoiTuongId` là mã đối tượng ghi nhật ký.
