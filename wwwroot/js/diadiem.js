/* ===== diadiem.js — dia diem + dat dia diem ===== */
"use strict";

/* global guiYeuCauApi, hienThongBaoNoi, thongBao, docGiaTriInput, docSoTuInput, taoHtmlBang, taoNhanTrangThai, hienKhungDangTai, maHoaKyTuHtml, dinhDangSoTien, kiemTraKhoangThoiGianForm, kiemTraSoKhongAmForm */
var __diaDiemCache = [];
var __lanTaiDiaDiem = 0;
function anhDiaDiemHTML(diaDiem) {
  var giaTriDuPhong = "/assets/images/placeholder-venue.svg";
  var duLieuNguon = diaDiem.HinhAnh || giaTriDuPhong;
  try {
    var urlYeuCau = new URL(duLieuNguon, location.href);
    if (!/^https?:$/.test(urlYeuCau.protocol)) duLieuNguon = giaTriDuPhong;
  } catch (loiXuLy) {
    duLieuNguon = giaTriDuPhong;
  }
  return (
    '<div class="venue-photo"><img src="' +
    maHoaKyTuHtml(duLieuNguon) +
    '" alt="' +
    maHoaKyTuHtml("Không gian tổ chức sự kiện tại " + diaDiem.Ten) +
    '" width="960" height="540" loading="lazy" decoding="async" onerror="this.onerror=null;this.src=\'' +
    giaTriDuPhong +
    "'\"></div>"
  );
}
function lamMoiDiaDiem() {
  var oTimDiaDiem = document.getElementById("dd-tukhoa");
  if (oTimDiaDiem) oTimDiaDiem.value = "";
  return taiDanhSachDiaDiem();
}
function chuanHoaDiaDiem(diaDiem) {
  return {
    Id: diaDiem.Id ?? diaDiem.id,
    Ten: diaDiem.Ten ?? diaDiem.ten ?? "Địa điểm đang cập nhật",
    HinhAnh: diaDiem.HinhAnh ?? diaDiem.hinhAnh,
    DiaChi: diaDiem.DiaChi ?? diaDiem.diaChi ?? "Địa chỉ đang cập nhật",
    SucChua: diaDiem.SucChua ?? diaDiem.sucChua,
    GiaThue: diaDiem.GiaThue ?? diaDiem.giaThue,
    HoatDong: diaDiem.HoatDong ?? diaDiem.hoatDong ?? false,
  };
}
async function taiDanhSachDiaDiem() {
  var khungDanhSachDiaDiem = document.getElementById("ds-diadiem");
  if (!khungDanhSachDiaDiem) return;
  var requestNumber = ++__lanTaiDiaDiem;
  khungDanhSachDiaDiem.setAttribute("aria-busy", "true");
  var khungSoLuongKetQua = document.getElementById("venue-result-count");
  if (khungSoLuongKetQua) khungSoLuongKetQua.textContent = "Đang tải...";
  hienKhungDangTai("ds-diadiem", 4);
  try {
    var tuKhoaTimKiem = docGiaTriInput("dd-tukhoa");
    var laQuanTriVien = !!phienDangNhap.token && phienDangNhap.vaiTro === "QuanTriVien";
    var phanHoiDiaDiem = await guiYeuCauApi(laQuanTriVien ? "/api/venues/all" : "/api/venues");
    if (requestNumber !== __lanTaiDiaDiem) return;
    var duLieuDiaDiem = phanHoiDiaDiem.data ?? phanHoiDiaDiem.Data;
    if (!Array.isArray(duLieuDiaDiem)) throw new Error("Chưa thể tải danh sách địa điểm. Vui lòng thử lại.");
    __diaDiemCache = duLieuDiaDiem.map(chuanHoaDiaDiem);
    var tuKhoaTimKiemChuanHoa = chuoiTimKiem(tuKhoaTimKiem);
    var danhSachDiaDiem = __diaDiemCache.filter(function (diaDiem) {
      return !tuKhoaTimKiemChuanHoa || chuoiTimKiem(diaDiem.Ten + " " + diaDiem.DiaChi).indexOf(tuKhoaTimKiemChuanHoa) !== -1;
    });
    if (khungSoLuongKetQua) khungSoLuongKetQua.textContent = danhSachDiaDiem.length + " địa điểm";
    if (!danhSachDiaDiem.length) {
      khungDanhSachDiaDiem.innerHTML =
        '<div class="empty"><h3>Chưa tìm thấy địa điểm phù hợp</h3><p>Thử tìm theo tên địa điểm hoặc thành phố khác.</p></div>';
      return;
    }
    khungDanhSachDiaDiem.innerHTML =
      '<div class="venue-grid">' +
      danhSachDiaDiem
        .map(function (diaDiem) {
          var sucChuaDiaDiem =
            Number.isFinite(Number(diaDiem.SucChua)) && diaDiem.SucChua != null
              ? dinhDangSoTien(diaDiem.SucChua) + " người"
              : "Đang cập nhật";
          var giaVe =
            Number.isFinite(Number(diaDiem.GiaThue)) && diaDiem.GiaThue != null
              ? dinhDangSoTien(diaDiem.GiaThue) + " đ"
              : "Liên hệ";
          return (
            '<article class="card venue-card">' +
            anhDiaDiemHTML(diaDiem) +
            '<div class="venue-body">' +
            '<div class="venue-heading"><h3>' +
            maHoaKyTuHtml(diaDiem.Ten) +
            '</h3><span class="badge ' +
            (diaDiem.HoatDong ? "b-ok" : "b-muted") +
            '">' +
            (diaDiem.HoatDong ? "Đang hoạt động" : "Tạm ngừng") +
            "</span></div>" +
            '<p class="muted venue-address">' +
            maHoaKyTuHtml(diaDiem.DiaChi) +
            "</p>" +
            '<dl class="venue-facts"><div><dt>Sức chứa</dt><dd>' +
            maHoaKyTuHtml(sucChuaDiaDiem) +
            "</dd></div><div><dt>Giá thuê</dt><dd>" +
            maHoaKyTuHtml(giaVe) +
            "</dd></div></dl>" +
            (laQuanTriVien
              ? '<div class="toolbar" data-quyen="QuanTriVien"><span class="muted small">Mã địa điểm: ' +
                maHoaKyTuHtml(diaDiem.Id) +
                '</span><button class="phu sm" onclick="dienFormSuaDiaDiem(' +
                Number(diaDiem.Id) +
                ')">Sửa</button><button class="phu sm" onclick="xoaDiaDiem(' +
                Number(diaDiem.Id) +
                ')">Xóa</button><button class="phu sm" onclick="batDiaDiem(' +
                Number(diaDiem.Id) +
                ')">Bật</button><button class="phu sm" onclick="tatDiaDiem(' +
                Number(diaDiem.Id) +
                ')">Tắt</button></div>'
              : "") +
            "</div></article>"
          );
        })
        .join("") +
      "</div>";
    apDungQuyen();
  } catch (loiXuLy) {
    if (requestNumber !== __lanTaiDiaDiem) return;
    if (khungSoLuongKetQua) khungSoLuongKetQua.textContent = "";
    khungDanhSachDiaDiem.innerHTML =
      '<div class="empty"><h3>Chưa thể tải địa điểm</h3><p>' +
      maHoaKyTuHtml(loiXuLy.message) +
      "</p><button class=\"phu\" onclick=\"taiDanhSachDiaDiem()\">Thử lại</button></div>";
  } finally {
    if (requestNumber === __lanTaiDiaDiem) khungDanhSachDiaDiem.setAttribute("aria-busy", "false");
  }
}
async function themDiaDiem() {
  var tenHienThi = docGiaTriInput("dd-ten"),
    diaChi = docGiaTriInput("dd-dc");
  var sucChua = docSoTuInput("dd-sc"),
    giaVe = docSoTuInput("dd-gia");
  if (!tenHienThi || !diaChi) {
    hienThongBaoNoi("Vui lòng nhập tên địa điểm và địa chỉ.", "loi");
    return;
  }
  if (sucChua === null || isNaN(sucChua) || sucChua <= 0) {
    hienThongBaoNoi("Sức chứa phải lớn hơn 0.", "loi");
    return;
  }
  if (!kiemTraSoKhongAmForm(docGiaTriInput("dd-gia"), "Giá thuê")) return;
  try {
    await guiYeuCauApi("/api/venues", "POST", {
      Ten: tenHienThi,
      DiaChi: diaChi,
      SucChua: sucChua,
      GiaThue: giaVe,
      HinhAnh: docGiaTriInput("dd-anh") || null,
      HoatDong: true,
    });
    thongBao("Đã thêm địa điểm.");
    taiDanhSachDiaDiem();
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  }
}
function dienFormSuaDiaDiem(diaDiemId) {
  if (!document.getElementById("dd-id")) {
    location.assign("/pages/quan-tri/dia-diem.html");
    return;
  }
  var diaDiem = __diaDiemCache.find(function (diaDiem) {
    return diaDiem.Id === diaDiemId;
  });
  [
    ["id", diaDiem.Id],
    ["ten", diaDiem.Ten],
    ["dc", diaDiem.DiaChi],
    ["sc", diaDiem.SucChua],
    ["gia", diaDiem.GiaThue],
    ["anh", diaDiem.HinhAnh],
  ].forEach(function (capTruongGiaTri) {
    var oThongTinDiaDiem = document.getElementById("dd-" + capTruongGiaTri[0]);
    if (oThongTinDiaDiem) oThongTinDiaDiem.value = capTruongGiaTri[1] == null ? "" : capTruongGiaTri[1];
  });
  document.getElementById("dd-ten").focus();
}
async function xoaDiaDiem(diaDiemId) {
  if (!confirm("Xóa địa điểm chưa có lịch đặt?")) return;
  try {
    await guiYeuCauApi("/api/admin/catalog/venues/" + diaDiemId, "DELETE");
    await taiDanhSachDiaDiem();
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  }
}
async function capNhatDiaDiem() {
  var diaDiemId = docSoTuInput("dd-id");
  if (!diaDiemId || diaDiemId <= 0) {
    hienThongBaoNoi("Nhập mã địa điểm cần cập nhật.", "loi");
    return;
  }
  var tenHienThi = docGiaTriInput("dd-ten"),
    diaChi = docGiaTriInput("dd-dc");
  if (!tenHienThi || !diaChi) {
    hienThongBaoNoi("Vui lòng nhập tên địa điểm và địa chỉ.", "loi");
    return;
  }
  var sucChua = docSoTuInput("dd-sc");
  if (sucChua === null || isNaN(sucChua) || sucChua <= 0) {
    hienThongBaoNoi("Sức chứa phải lớn hơn 0.", "loi");
    return;
  }
  if (!kiemTraSoKhongAmForm(docGiaTriInput("dd-gia"), "Giá thuê")) return;
  try {
    await guiYeuCauApi("/api/venues/" + diaDiemId, "PUT", {
      Ten: tenHienThi,
      DiaChi: diaChi,
      SucChua: sucChua,
      GiaThue: docSoTuInput("dd-gia"),
      HinhAnh: docGiaTriInput("dd-anh") || null,
      HoatDong: true,
    });
    thongBao("Đã cập nhật địa điểm.");
    taiDanhSachDiaDiem();
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  }
}
function batDiaDiem(diaDiemId) {
  return capNhatHoatDongDiaDiem(diaDiemId, true);
}
function tatDiaDiem(diaDiemId) {
  return capNhatHoatDongDiaDiem(diaDiemId, false);
}
async function capNhatHoatDongDiaDiem(diaDiemId, hoatDong) {
  var diaDiemCanSua = __diaDiemCache.find(function (diaDiem) {
    return Number(diaDiem.Id) === Number(diaDiemId);
  });
  if (!diaDiemCanSua) return;
  try {
    await guiYeuCauApi("/api/venues/" + diaDiemId, "PUT", {
      Ten: diaDiemCanSua.Ten,
      DiaChi: diaDiemCanSua.DiaChi,
      SucChua: diaDiemCanSua.SucChua,
      GiaThue: diaDiemCanSua.GiaThue,
      HinhAnh: diaDiemCanSua.HinhAnh,
      HoatDong: hoatDong,
    });
    thongBao("Đã cập nhật trạng thái địa điểm.");
    taiDanhSachDiaDiem();
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  }
}
async function guiPhieuDatDiaDiem() {
  var suKienId = docSoTuInput("dat-sk"),
    diaDiemId = docSoTuInput("dat-dd");
  var batDau = docGiaTriInput("dat-bd"),
    ketThuc = docGiaTriInput("dat-kt");
  var tienCoc = docSoTuInput("dat-tc");
  if (!suKienId || suKienId <= 0 || !diaDiemId || diaDiemId <= 0) {
    hienThongBaoNoi("Nhập mã sự kiện và địa điểm hợp lệ.", "loi");
    return;
  }
  if (!kiemTraKhoangThoiGianForm(batDau, ketThuc)) return;
  if (tienCoc === null || isNaN(tienCoc) || tienCoc < 0) {
    hienThongBaoNoi("Tiền cọc phải là số không âm.", "loi");
    return;
  }
  var nutDatDiaDiem = document.querySelector('[data-action="datDiaDiem"]');
  if (nutDatDiaDiem && nutDatDiaDiem.disabled) return;
  if (nutDatDiaDiem) nutDatDiaDiem.disabled = true;
  try {
    await guiYeuCauApi("/api/venue-bookings", "POST", {
      DiaDiemId: diaDiemId,
      SuKienId: suKienId,
      BatDau: batDau || null,
      KetThuc: ketThuc || null,
      TienCoc: tienCoc || 0,
    });
    thongBao("Đặt địa điểm thành công.");
    taiDatDiaDiem();
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  } finally {
    if (nutDatDiaDiem) nutDatDiaDiem.disabled = false;
  }
}
async function taiDatDiaDiem() {
  var khungDanhSachDatDiaDiem = document.getElementById("ds-dat");
  if (!khungDanhSachDatDiaDiem) return;
  if (["QuanLySuKien", "QuanTriVien"].indexOf(phienDangNhap.vaiTro) === -1) {
    khungDanhSachDatDiaDiem.innerHTML =
      '<div class="empty"><h3>Lịch đặt được theo dõi bởi bộ phận quản lý</h3><p>Sau khi gửi yêu cầu, ban tổ chức có thể kiểm tra thông tin địa điểm trong chi tiết sự kiện. Danh sách lịch đặt toàn hệ thống dành cho quản lý và quản trị viên.</p><a class="phu" href="/pages/su-kien/danh-sach.html">Khám phá sự kiện</a></div>';
    return;
  }
  hienKhungDangTai("ds-dat", 3);
  var cacMocThoiGian = document.getElementById("booking-timeline");
  if (cacMocThoiGian) cacMocThoiGian.innerHTML = "";
  try {
    var phanHoiDatDiaDiem = await guiYeuCauApi("/api/venue-bookings");
    if (typeof giaoDienQuanLySuKien !== "undefined") {
      giaoDienQuanLySuKien.hienDanhSachDatDiaDiem(phanHoiDatDiaDiem.data || []);
      return;
    }
    document.getElementById("ds-dat").innerHTML = taoHtmlBang(
      [
        ["TenSuKien", "Sự kiện"],
        ["TenDiaDiem", "Địa điểm"],
        ["TrangThai", "Trạng thái"],
        ["TongTien", "Tổng tiền", "tien"],
        ["TienCoc", "Cọc", "tien"],
      ],
      (phanHoiDatDiaDiem.data || []).map(function (guiPhieuDatDiaDiem) {
        return {
          Id: guiPhieuDatDiaDiem.Id,
          TenSuKien: guiPhieuDatDiaDiem.SuKien && guiPhieuDatDiaDiem.SuKien.Ten ? guiPhieuDatDiaDiem.SuKien.Ten : "",
          TenDiaDiem: guiPhieuDatDiaDiem.DiaDiem && guiPhieuDatDiaDiem.DiaDiem.Ten ? guiPhieuDatDiaDiem.DiaDiem.Ten : "",
          TrangThai: guiPhieuDatDiaDiem.TrangThai,
          TongTien: guiPhieuDatDiaDiem.TongTien,
          TienCoc: guiPhieuDatDiaDiem.TienCoc,
        };
      }),
      function (guiPhieuDatDiaDiem) {
        return (
          "<button class=\"phu sm\" onclick=\"xacNhanDatDiaDiem(" +
          guiPhieuDatDiaDiem.Id +
          ')">Xac nhan</button> ' +
          "<button class=\"phu sm\" onclick=\"tuChoiDatDiaDiem(" +
          guiPhieuDatDiaDiem.Id +
          ')">Tu choi</button> ' +
          "<button class=\"phu sm\" onclick=\"huyDatDiaDiem(" +
          guiPhieuDatDiaDiem.Id +
          ')">Huy</button>'
        );
      },
    );
  } catch (loiXuLy) {
    hienLoiTai("ds-dat", loiXuLy, taiDatDiaDiem);
  }
}
async function xacNhanDatDiaDiem(datDiaDiemId) {
  try {
    await guiYeuCauApi("/api/venue-bookings/" + datDiaDiemId + "/approve", "POST", null);
    thongBao("Đã xác nhận đặt địa điểm #" + datDiaDiemId + ".");
    taiDatDiaDiem();
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  }
}
async function tuChoiDatDiaDiem(datDiaDiemId) {
  try {
    await guiYeuCauApi("/api/venue-bookings/" + datDiaDiemId + "/reject", "POST", null);
    thongBao("Đã từ chối đặt địa điểm #" + datDiaDiemId + ".");
    taiDatDiaDiem();
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  }
}
async function huyDatDiaDiem(datDiaDiemId) {
  try {
    await guiYeuCauApi("/api/venue-bookings/" + datDiaDiemId + "/cancel", "POST", null);
    thongBao("Đã hủy đặt địa điểm #" + datDiaDiemId + ".");
    taiDatDiaDiem();
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  }
}
async function taiThongKeDatDiaDiem() {
  hienKhungDangTai("tk-dat-tt", 3);
  hienKhungDangTai("tk-dat-dd", 3);
  try {
    var phanHoiDatDiaDiem = await guiYeuCauApi("/api/venue-bookings");
    var khungTheoTrangThai = document.getElementById("tk-dat-tt"),
      khungTheoDiaDiem = document.getElementById("tk-dat-dd");
    var danhSachDatDiaDiem = phanHoiDatDiaDiem.data || [];
    if (khungTheoTrangThai)
      khungTheoTrangThai.innerHTML = taoHtmlBang(
        [
          ["TrangThai", "Trạng thái"],
          ["SoLuong", "Số lượng"],
        ],
        demSoLuongTheoNhom(danhSachDatDiaDiem, function (guiPhieuDatDiaDiem) {
          return (
            ["ChoXacNhan", "DaXacNhan", "TuChoi", "DaHuy", "HoanThanh"][guiPhieuDatDiaDiem.TrangThai] ||
            guiPhieuDatDiaDiem.TrangThai
          );
        }),
      );
    var thongKeTheoDiaDiem = new Map();
    danhSachDatDiaDiem.forEach(function (guiPhieuDatDiaDiem) {
      if (!thongKeTheoDiaDiem.has(guiPhieuDatDiaDiem.DiaDiemId))
        thongKeTheoDiaDiem.set(guiPhieuDatDiaDiem.DiaDiemId, {
          TenDiaDiem: guiPhieuDatDiaDiem.DiaDiem ? guiPhieuDatDiaDiem.DiaDiem.Ten : String(guiPhieuDatDiaDiem.DiaDiemId),
          SoLuotDat: 0,
        });
      thongKeTheoDiaDiem.get(guiPhieuDatDiaDiem.DiaDiemId).SoLuotDat++;
    });
    document.querySelectorAll('[data-metric="venues"]').forEach(function (phanTuDom) {
      phanTuDom.textContent = dinhDangSoTien(thongKeTheoDiaDiem.size);
    });
    if (khungTheoDiaDiem)
      khungTheoDiaDiem.innerHTML = taoHtmlBang(
        [
          ["TenDiaDiem", "Địa điểm"],
          ["SoLuotDat", "Lượt đặt"],
        ],
        Array.from(thongKeTheoDiaDiem.values()),
      );
  } catch (loiXuLy) {
    ["tk-dat-tt", "tk-dat-dd"].forEach(function (idKhungThongKe) {
      hienLoiTai(idKhungThongKe, loiXuLy, taiThongKeDatDiaDiem);
    });
    thongBao(loiXuLy.message, "loi");
  }
}
