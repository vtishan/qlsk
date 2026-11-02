/* ===== donve.js — loai ve + don ve + thanh toan + ve ===== */
"use strict";

/* global guiYeuCauApi, hienThongBaoNoi, thongBao, docGiaTriInput, docSoTuInput, taoHtmlBang, taoNhanTrangThai, hienKhungDangTai, maHoaKyTuHtml, dinhDangNgayGioVietNam, dinhDangSoTien, chayHieuUngGiayMau, kiemTraKhoangThoiGianForm, kiemTraSoKhongAmForm */

/* ---- Loai ve (lay theo su kien) ---- */
async function taiLoaiVeTheoSuKien(suKienIdTruyenVao) {
  var suKienId = suKienIdTruyenVao || docSoTuInput("lv-xem-sk");
  if (!suKienId || suKienId <= 0) {
    hienThongBaoNoi("Nhập mã sự kiện để xem hạng vé.", "loi");
    return;
  }
  var idKhungDanhSachLoaiVe = document.getElementById("lv-ten") ? "ds-loaive" : suKienIdTruyenVao ? "ds-loaive-sk" : "ds-loaive";
  var khungDanhSachLoaiVe =
    document.getElementById(idKhungDanhSachLoaiVe) ||
    document.getElementById("ds-loaive-sk") ||
    document.getElementById("ds-loaive");
  if (!khungDanhSachLoaiVe) {
    location.assign("/pages/su-kien/chi-tiet.html?id=" + Number(suKienId) + "#tickets");
    return;
  }
  if (khungDanhSachLoaiVe) hienKhungDangTai(khungDanhSachLoaiVe.id, 3);
  try {
    var phanHoiLoaiVe = await guiYeuCauApi("/api/events/" + suKienId + "/ticket-types");
    var danhSachLoaiVe = phanHoiLoaiVe.data || [];
    var noiDungHtml;
    if (!danhSachLoaiVe.length) {
      noiDungHtml = taoHtmlBang([], []);
    } else {
      noiDungHtml =
        '<div class="grid g3">' +
        danhSachLoaiVe
          .map(function (loaiVe) {
            return (
              '<article class="ticket-tier anim-pop">' +
              '<div class="toolbar" style="margin:0 0 8px">' +
              '<span class="ticket-tier-price"><b>' +
              maHoaKyTuHtml(dinhDangSoTien(loaiVe.Gia)) +
              " VND</b></span></div>" +
              "<h3>" +
              maHoaKyTuHtml(loaiVe.Ten) +
              "</h3>" +
              '<div class="muted">Còn ' +
              maHoaKyTuHtml(Math.max(0, loaiVe.SoLuong - loaiVe.DaBan - loaiVe.DangGiu)) +
              " vé · Tối đa 20 vé/đơn</div>" +
              '<div class="muted">' +
              maHoaKyTuHtml(dinhDangNgayGioVietNam(loaiVe.MoBan)) +
              " &rarr; " +
              maHoaKyTuHtml(dinhDangNgayGioVietNam(loaiVe.DongBan)) +
              "</div>" +
              '<div class="toolbar"><button class="chinh sm" onclick="chonLoaiVe(' +
              loaiVe.Id +
              "," +
              suKienId +
              ')"' +
              (loaiVe.SoLuong - loaiVe.DaBan - loaiVe.DangGiu <= 0 ||
              chuyenNgayDuLieuSangDate(loaiVe.DongBan) < new Date() ||
              chuyenNgayDuLieuSangDate(loaiVe.MoBan) > new Date()
                ? " disabled"
                : "") +
              ">Chọn vé</button></div>" +
              "</article>"
            );
          })
          .join("") +
        "</div>";
    }
    if (khungDanhSachLoaiVe) {
      khungDanhSachLoaiVe.innerHTML = noiDungHtml;
    }
  } catch (loiXuLy) {
    hienLoiTai(khungDanhSachLoaiVe.id, loiXuLy, function () {
      taiLoaiVeTheoSuKien(suKienIdTruyenVao);
    });
  }
}
function chonLoaiVe(loaiVeId, suKienId) {
  var duongDanTaoDon = "/pages/don-ve/tao-don.html?event=" + Number(suKienId) + "&ticket=" + Number(loaiVeId);
  location.assign(phienDangNhap.token ? duongDanTaoDon : "/pages/auth/dang-nhap.html?next=" + encodeURIComponent(duongDanTaoDon));
}

/* ---- Don ve ---- */
async function taoDonDatVe() {
  var loaiVeId = docSoTuInput("don-loai"),
    soLuong = docSoTuInput("don-sl");
  if (!loaiVeId || loaiVeId <= 0) {
    hienThongBaoNoi("Chọn hạng vé hợp lệ.", "loi");
    return;
  }
  if (!Number.isInteger(soLuong) || soLuong < 1 || soLuong > 20) {
    hienThongBaoNoi("Vui lòng chọn từ 1 đến 20 vé.", "loi");
    return;
  }
  if (taoDonDatVe.dangGui) return;
  var nutTaoDon = document.querySelector("button[onclick=\"taoDonDatVe()\"]");
  if (nutTaoDon && nutTaoDon.disabled) return;
  taoDonDatVe.dangGui = true;
  if (nutTaoDon) nutTaoDon.disabled = true;
  try {
    var phanHoiTaoDon = await guiYeuCauApi("/api/orders", "POST", {
      Items: [
        {
          LoaiVeId: loaiVeId,
          SoLuong: soLuong,
        },
      ],
    });
    var donVe = phanHoiTaoDon.data || phanHoiTaoDon;
    var maDonHang = donVe.MaDonHang || "DH-" + donVe.Id;
    thongBao("Tạo đơn thành công. Mã đơn: " + maDonHang);
    chayHieuUngGiayMau(90);
    location.assign("/pages/don-ve/chi-tiet-don.html?id=" + Number(donVe.Id));
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  } finally {
    taoDonDatVe.dangGui = false;
    if (nutTaoDon) nutTaoDon.disabled = false;
  }
}
var __dsDonCache = [];
function nhanTrangThaiDon(giaTriTrangThai) {
  return typeof giaTriTrangThai === "number"
    ? ["ChoThanhToan", "ThanhCong", "HetHan", "DaHuy", "YeuCauHoanTien", "DaHoanTien"][giaTriTrangThai] || ""
    : giaTriTrangThai;
}
function phanLoaiTrangThaiDonVe(donVe) {
  var trangThaiDon = String((donVe && donVe.TrangThai) || "");
  var trangThaiThanhToan = String((donVe && donVe.TrangThaiThanhToan) || "");
  if (/DaHuy|HetHan/i.test(trangThaiDon) || [2, 3].indexOf(donVe.TrangThai) !== -1) return "huy";
  if ([4, 5].indexOf(donVe.TrangThai) !== -1) return "refund";
  if (donVe.TrangThai === 1) return "paid";
  if (
    /DaHoanTien|HoanTienMotPhan|YeuCauHoanTien/i.test(trangThaiDon) ||
    /DaHoanTien|HoanTienMotPhan/i.test(trangThaiThanhToan)
  )
    return "refund";
  if (/ThanhCong/i.test(trangThaiDon) || /DaThanhToan/i.test(trangThaiThanhToan)) return "paid";
  return "cho";
}
function taoNhanTrangThaiDonVe(donVe) {
  var loaiThongBao = phanLoaiTrangThaiDonVe(donVe);
  if (loaiThongBao === "cho") return "Chờ thanh toán";
  if (loaiThongBao === "paid") return "Đã thanh toán";
  if (loaiThongBao === "refund") return "Hoàn tiền";
  if (loaiThongBao === "huy") return "Đã hủy / Hết hạn";
  return String(donVe.TrangThai || donVe.TrangThaiThanhToan || "—");
}
function capNhatThongKeDon(danhSachDonVe) {
  var tongSoDon = danhSachDonVe.length,
    soDonChoThanhToan = 0,
    soDonDaThanhToan = 0,
    soDonHoanTien = 0;
  danhSachDonVe.forEach(function (donVe) {
    var nhomTrangThaiDon = phanLoaiTrangThaiDonVe(donVe);
    if (nhomTrangThaiDon === "cho") soDonChoThanhToan++;
    else if (nhomTrangThaiDon === "paid") soDonDaThanhToan++;
    else if (nhomTrangThaiDon === "refund" || nhomTrangThaiDon === "huy") soDonHoanTien++;
  });
  var hienThiChiSoDon = function (idKhungChiSo, giaTriChiSo) {
    var khungChiSo = document.getElementById(idKhungChiSo);
    if (khungChiSo) khungChiSo.textContent = giaTriChiSo;
  };
  hienThiChiSoDon("qs-tong", tongSoDon);
  hienThiChiSoDon("qs-chothanhtoan", soDonChoThanhToan);
  hienThiChiSoDon("qs-dathanhtoan", soDonDaThanhToan);
  hienThiChiSoDon("qs-hoantien", soDonHoanTien);
}
function taoTheDonVe(donVe) {
  var loaiThongBao = phanLoaiTrangThaiDonVe(donVe);
  var lopTrangThai =
    loaiThongBao === "cho"
      ? "is-pending"
      : loaiThongBao === "paid"
        ? "is-paid"
        : loaiThongBao === "refund"
          ? "is-refund"
          : "is-cancelled";
  var lopBuocDatVe = "done",
    lopBuocThanhToan = "now",
    lopBuocNhanVe = "off",
    nhanBuocThanhToan = "Thanh toán",
    nhanBuocNhanVe = "Nhận vé";
  if (loaiThongBao === "paid" || loaiThongBao === "refund") {
    lopBuocDatVe = "done";
    lopBuocThanhToan = "done";
    lopBuocNhanVe = "done";
  } else if (loaiThongBao === "huy") {
    lopBuocDatVe = "done";
    lopBuocThanhToan = "off";
    lopBuocNhanVe = "off";
    nhanBuocThanhToan = "Đã đóng";
    nhanBuocNhanVe = "Không có vé";
  } else if (loaiThongBao === "cho") {
    lopBuocDatVe = "done";
    lopBuocThanhToan = "now";
    lopBuocNhanVe = "off";
  }
  var hetHan = "";
  try {
    if (loaiThongBao === "cho" && donVe.HetHan) {
      hetHan =
        '<div class="muted small">Giữ chỗ đến: <b>' +
        maHoaKyTuHtml(dinhDangNgayGioVietNam(donVe.HetHan)) +
        "</b></div>";
    }
  } catch (loiXuLy) {}
  var ngayDat = "";
  try {
    if (donVe.NgayTao) ngayDat = maHoaKyTuHtml(dinhDangNgayGioVietNam(donVe.NgayTao));
  } catch (loiXuLy) {}
  var tongTien = Number(donVe.TongTien || 0);
  var donDangChoThanhToan = loaiThongBao === "cho";
  var donDaThanhToan = loaiThongBao === "paid" || loaiThongBao === "refund";
  var noiDungHtml =
    '<article class="card hoverable anim-pop don-card ' +
    lopTrangThai +
    '">' +
    '<div class="don-top"><span class="don-code">' +
    maHoaKyTuHtml(donVe.MaDonHang || "DH-" + donVe.Id) +
    "</span>" +
    '<span class="don-badges">' +
    taoNhanTrangThai(nhanTrangThaiDon(donVe.TrangThai)) +
    "</span></div>" +
    '<div class="muted small">Đơn #' +
    maHoaKyTuHtml(donVe.Id) +
    (ngayDat ? " &middot; Đặt lúc " + ngayDat : "") +
    " &middot; " +
    maHoaKyTuHtml(taoNhanTrangThaiDonVe(donVe)) +
    "</div>" +
    hetHan +
    '<div class="don-price-row"><span class="muted small">Tổng thanh toán</span><span class="don-price">' +
    maHoaKyTuHtml(dinhDangSoTien(tongTien)) +
    "đ</span></div>" +
    '<div class="don-steps">' +
    '<div class="don-step ' +
    lopBuocDatVe +
    '"><div class="dot">1</div>Đặt đơn</div>' +
    '<div class="don-step ' +
    lopBuocThanhToan +
    '"><div class="dot">2</div>' +
    nhanBuocThanhToan +
    "</div>" +
    '<div class="don-step ' +
    lopBuocNhanVe +
    '"><div class="dot">3</div>' +
    nhanBuocNhanVe +
    "</div></div>" +
    '<div class="don-actions">' +
    '<button class="phu sm" onclick="xemChiTietDon(' +
    donVe.Id +
    ')">Chi tiết</button> ';
  if (donDangChoThanhToan) {
    noiDungHtml +=
      '<button class="chinh sm" onclick="thanhToanDon(' +
      donVe.Id +
      "," +
      tongTien +
      ')">Thanh toán</button> ' +
      "<button class=\"phu sm btn-huy\" onclick=\"huyDonDatVe(" +
      donVe.Id +
      ')">Hủy đơn</button>';
  }
  if (donDaThanhToan) {
    noiDungHtml += '<button class="chinh sm" onclick="moTrangVeTheoDon(' + donVe.Id + ')">Xem vé</button>';
  }
  if (!donDangChoThanhToan && !donDaThanhToan) {
    noiDungHtml += '<button class="phu sm" onclick="xemChiTietDon(' + donVe.Id + ')">Xem lại</button>';
  }
  noiDungHtml += "</div></article>";
  return noiDungHtml;
}
function hienThiDanhSachDon() {
  var khungDanhSachDonVe = document.getElementById("ds-don");
  if (!khungDanhSachDonVe) return;
  var danhSachDonVe = __dsDonCache || [];
  var tuKhoaTimKiem = "";
  try {
    tuKhoaTimKiem = (document.getElementById("loc-tukhoa").value || "").trim().toLowerCase();
  } catch (loiXuLy) {}
  var boLocTrangThai = "all";
  try {
    boLocTrangThai = document.getElementById("loc-trangthai").value || "all";
  } catch (loiXuLy) {}
  var donVeDaLoc = danhSachDonVe.filter(function (donVe) {
    if (boLocTrangThai !== "all" && phanLoaiTrangThaiDonVe(donVe) !== boLocTrangThai) return false;
    if (tuKhoaTimKiem) {
      var chuoiTimDon = String((donVe.MaDonHang || "") + " " + (donVe.Id || "")).toLowerCase();
      if (chuoiTimDon.indexOf(tuKhoaTimKiem) === -1) return false;
    }
    return true;
  });
  if (!danhSachDonVe.length) {
    khungDanhSachDonVe.innerHTML =
      '<div class="card empty-don"><div class="big">🎫</div><h3 style="margin:0 0 6px">Chưa có đơn nào</h3><p class="muted" style="margin:0 0 12px">Bạn chưa đặt đơn nào. Tạo đơn đầu tiên chỉ mất 30 giây.</p><a class="chinh btn sm" href="tao-don.html">+ Tạo đơn mới</a></div>';
    return;
  }
  if (!donVeDaLoc.length) {
    khungDanhSachDonVe.innerHTML =
      '<div class="card empty-don"><div class="big">🔎</div><h3 style="margin:0 0 6px">Không khớp bộ lọc</h3><p class="muted" style="margin:0">Thử đổi từ khóa hoặc chọn “Tất cả trạng thái”.</p></div>';
    return;
  }
  khungDanhSachDonVe.innerHTML =
    '<div class="don-grid">' +
    donVeDaLoc.map(taoTheDonVe).join("") +
    "</div>" +
    '<p class="muted small" style="margin:10px 2px 0">Hiển thị ' +
    donVeDaLoc.length +
    " / " +
    danhSachDonVe.length +
    " đơn.</p>";
}
function ganSuKienBoLocDonVe() {
  var oTimDonVe = document.getElementById("loc-tukhoa");
  var oLocTrangThai = document.getElementById("loc-trangthai");
  if (oTimDonVe && !oTimDonVe.__gan) {
    oTimDonVe.__gan = 1;
    oTimDonVe.addEventListener("input", function () {
      hienThiDanhSachDon();
    });
  }
  if (oLocTrangThai && !oLocTrangThai.__gan) {
    oLocTrangThai.__gan = 1;
    oLocTrangThai.addEventListener("change", function () {
      hienThiDanhSachDon();
    });
  }
}
async function taiDonVeCuaTaiKhoan() {
  hienKhungDangTai("ds-don", 3);
  try {
    var phanHoiDonVe = await guiYeuCauApi("/api/orders/me");
    var danhSachDonVe = phanHoiDonVe.data || [];
    __dsDonCache = danhSachDonVe;
    var khungChiTietDonVe = document.getElementById("ds-chitiet");
    if (khungChiTietDonVe) khungChiTietDonVe.innerHTML = "";
    var khungDanhSachVe = document.getElementById("ds-ve");
    if (khungDanhSachVe) khungDanhSachVe.innerHTML = "";
    capNhatThongKeDon(danhSachDonVe);
    ganSuKienBoLocDonVe();
    hienThiDanhSachDon();
  } catch (loiXuLy) {
    hienLoiTai("ds-don", loiXuLy, taiDonVeCuaTaiKhoan);
  }
}
async function xemChiTietDon(donVeId) {
  donVeId = donVeId || docSoTuInput("don-chitiet-id");
  if (!donVeId || donVeId <= 0) {
    hienThongBaoNoi("Nhập mã đơn hàng để xem chi tiết.", "loi");
    return;
  }
  var resultHost = document.getElementById("ds-chitiet") || document.getElementById("ds-don");
  if (!resultHost) {
    location.assign("/pages/don-ve/chi-tiet-don.html?id=" + Number(donVeId));
    return;
  }
  hienKhungDangTai(resultHost.id, 3);
  try {
    var phanHoiDonVe = await guiYeuCauApi("/api/orders/" + donVeId);
    var donVe = phanHoiDonVe.data || phanHoiDonVe;
    var chiTietDonVe = donVe.ChiTietDonVes || donVe.ChiTiets || [];
    var khungChiTietDonVe = document.getElementById("ds-chitiet") || document.getElementById("ds-don");
    var tongThanhTien = 0;
    try {
      chiTietDonVe.forEach(function (chiTietDonVe) {
        tongThanhTien += Number(chiTietDonVe.ThanhTien || 0);
      });
    } catch (loiXuLy) {}
    var tenSuKien = "";
    try {
      if (donVe.SuKien) tenSuKien = String(donVe.SuKien.Ten);
    } catch (loiDuPhong) {}
    var noiDungHtml =
      '<div class="card don-detail anim-pop"><h3>Chi tiết đơn #' +
      maHoaKyTuHtml(donVeId) +
      "</h3>" +
      (tenSuKien
        ? '<p class="muted" style="margin:4px 0 0">Sự kiện: <b>' + maHoaKyTuHtml(tenSuKien) + "</b></p>"
        : "") +
      taoHtmlBang(
        [
          ["TenLoaiVe", "Hạng vé"],
          ["SoLuong", "SL"],
          ["DonGia", "Đơn giá", "tien"],
          ["ThanhTien", "Thành tiền", "tien"],
        ],
        chiTietDonVe.map(function (chiTietDonVe) {
          return {
            TenLoaiVe: chiTietDonVe.LoaiVe && chiTietDonVe.LoaiVe.Ten ? chiTietDonVe.LoaiVe.Ten : "",
            SoLuong: chiTietDonVe.SoLuong,
            DonGia: chiTietDonVe.DonGia,
            ThanhTien: chiTietDonVe.ThanhTien,
          };
        }),
      ) +
      '<div class="don-sum"><span>Tổng cộng (' +
      chiTietDonVe.length +
      ' dòng vé)</span><span class="tien">' +
      maHoaKyTuHtml(dinhDangSoTien(tongThanhTien)) +
      "đ</span></div>" +
      '<div class="don-actions"><a class="phu btn sm" href="../thanh-toan/thanh-toan.html?id=' +
      Number(donVeId) +
      '">Sang thanh toán</a> ' +
      '<a class="phu btn sm" href="../ve/ve-theo-don.html?id=' +
      Number(donVeId) +
      '">Xem vé theo đơn</a></div></div>';
    khungChiTietDonVe.innerHTML = noiDungHtml;
    khungChiTietDonVe.scrollIntoView({
      behavior: "smooth",
      block: "nearest",
    });
  } catch (loiXuLy) {
    hienLoiTai(resultHost.id, loiXuLy, function () {
      xemChiTietDon(donVeId);
    });
  }
}
async function huyDonDatVe(donVeId) {
  if (!donVeId || donVeId <= 0) {
    hienThongBaoNoi("Vui lòng chọn đơn hàng.", "loi");
    return;
  }
  try {
    await guiYeuCauApi("/api/orders/" + donVeId + "/cancel", "POST", null);
    thongBao("Đã hủy đơn hàng.");
    taiDonVeCuaTaiKhoan();
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  }
}
async function taiDanhSachGiaoDich() {
  try {
    var donVeId = docSoTuInput("don-gd-id");
    if (!donVeId || donVeId <= 0) {
      hienThongBaoNoi("Nhập mã đơn hàng hợp lệ.", "loi");
      return;
    }
    hienKhungDangTai("ds-giaodich", 3);
    var phanHoiDonVe = await guiYeuCauApi("/api/orders/" + donVeId);
    var donVe = phanHoiDonVe.data || phanHoiDonVe;
    var hangGiaoDich = (donVe.ThanhToans || []).map(function (taoLienKetThanhToanTuForm) {
      return {
        MaGiaoDich: taoLienKetThanhToanTuForm.MaGiaoDich,
        SoTien: taoLienKetThanhToanTuForm.SoTien,
        TrangThai:
          typeof taoLienKetThanhToanTuForm.TrangThai === "number"
            ? ["DangXuLy", "ThanhCong", "ThatBai", "NghiVan"][taoLienKetThanhToanTuForm.TrangThai]
            : taoLienKetThanhToanTuForm.TrangThai,
      };
    });
    document.getElementById("ds-giaodich").innerHTML = taoHtmlBang(
      [
        ["MaGiaoDich", "Mã giao dịch"],
        ["SoTien", "Số tiền", "tien"],
        ["TrangThai", "Trạng thái"],
      ],
      hangGiaoDich,
    );
  } catch (loiXuLy) {
    hienLoiTai("ds-giaodich", loiXuLy, taiDanhSachGiaoDich);
  }
}

/* ---- Thanh toan ---- */
async function thanhToanDon(donVeId) {
  if (!donVeId || donVeId <= 0) {
    hienThongBaoNoi("Vui lòng chọn đơn hàng.", "loi");
    return;
  }
  if (thanhToanDon.dangGui) return;
  thanhToanDon.dangGui = true;
  var nutThanhToan = document.getElementById("payment-submit");
  if (nutThanhToan) {
    nutThanhToan.disabled = true;
    nutThanhToan.setAttribute("aria-busy", "true");
  }
  try {
    var phanHoiThanhToan = await guiYeuCauApi("/api/payments", "POST", {
      DonVeId: donVeId,
    });
    var taoLienKetThanhToanTuForm = phanHoiThanhToan.data || phanHoiThanhToan;
    var urlYeuCau = taoLienKetThanhToanTuForm.checkoutUrl || "";
    if (urlYeuCau) {
      var khungTomTatThanhToan = new URL(urlYeuCau, location.origin);
      if (khungTomTatThanhToan.protocol !== "https:" && khungTomTatThanhToan.origin !== location.origin)
        throw new Error("Chưa thể mở trang thanh toán. Vui lòng thử lại sau.");
      location.assign(khungTomTatThanhToan.href);
    } else {
      thongBao("Đặt vé miễn phí thành công. Vé của bạn đã sẵn sàng.");
      if (!document.getElementById("ds-don")) location.assign("/pages/don-ve/don-cua-toi.html");
    }
    if (document.getElementById("ds-don")) taiDonVeCuaTaiKhoan();
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  } finally {
    thanhToanDon.dangGui = false;
    if (nutThanhToan) {
      nutThanhToan.disabled = Number(donVeId) !== docSoTuInput("tt-don");
      nutThanhToan.removeAttribute("aria-busy");
    }
  }
}
async function taoLienKetThanhToanTuForm() {
  var donVeId = docSoTuInput("tt-don");
  if (!donVeId || donVeId <= 0) {
    hienThongBaoNoi("Nhập mã đơn hàng hợp lệ.", "loi");
    return;
  }
  await thanhToanDon(donVeId);
}
async function guiYeuCauHoanTien() {
  var thanhToanId = docSoTuInput("ht-thanh-toan-id"),
    soTienHoan = docSoTuInput("ht-tien"),
    maHoanTien = docGiaTriInput("ht-magd");
  if (!thanhToanId || thanhToanId <= 0) {
    hienThongBaoNoi("Nhập mã thanh toán hợp lệ.", "loi");
    return;
  }
  if (soTienHoan === null || isNaN(soTienHoan) || soTienHoan <= 0) {
    hienThongBaoNoi("Số tiền hoàn phải lớn hơn 0.", "loi");
    return;
  }
  if (!maHoanTien) {
    hienThongBaoNoi("Nhập mã giao dịch hoàn tiền.", "loi");
    return;
  }
  try {
    await guiYeuCauApi("/api/payments/" + thanhToanId + "/refund", "POST", {
      SoTien: soTienHoan,
      MaHoanTien: maHoanTien,
    });
    thongBao("Ghi nhận hoàn tiền thành công.");
    taiDonVeCuaTaiKhoan();
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  }
}
async function taiThongKeThanhToan() {
  try {
    var phanHoiThongKe = await guiYeuCauApi("/api/admin/dashboard");
    hienThongKeThanhToan(phanHoiThongKe.data);
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  }
}

/* ---- Mo trang ve theo don ---- */
async function moTrangVeTheoDon(donVeId) {
  if (!donVeId || donVeId <= 0) {
    hienThongBaoNoi("Vui lòng chọn đơn hàng.", "loi");
    return;
  }
  try {
    // Ve tu phat khi thanh toan thanh cong (webhook); khong co API phat lai theo don.
    var phanHoiDonVe = await guiYeuCauApi("/api/orders/" + donVeId);
    var donVe = phanHoiDonVe.data || phanHoiDonVe;
    location.assign("/pages/ve/ve-theo-don.html?id=" + Number(donVe.Id || donVeId));
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  }
}

/* ---- Loai ve: them/cap nhat theo su kien (BanToChuc) ---- */
async function themLoaiVe() {
  var suKienId = docSoTuInput("lv-sk"),
    tenHienThi = docGiaTriInput("lv-ten"),
    giaVe = docSoTuInput("lv-gia");
  var soLuong = docSoTuInput("lv-slmax"),
    thoiGianMoBan = docGiaTriInput("lv-mo"),
    thoiGianDongBan = docGiaTriInput("lv-dong");
  if (!suKienId || suKienId <= 0) {
    hienThongBaoNoi("Nhập mã sự kiện hợp lệ.", "loi");
    return;
  }
  if (!tenHienThi) {
    hienThongBaoNoi("Nhập tên hạng vé.", "loi");
    return;
  }
  if (giaVe === null || isNaN(giaVe) || giaVe < 0) {
    hienThongBaoNoi("Giá vé phải là số không âm.", "loi");
    return;
  }
  if (soLuong === null || isNaN(soLuong) || soLuong <= 0) {
    hienThongBaoNoi("Số lượng phải lớn hơn 0.", "loi");
    return;
  }
  if (!thoiGianMoBan || !thoiGianDongBan) {
    hienThongBaoNoi("Chọn thời gian mở bán và đóng bán.", "loi");
    return;
  }
  try {
    await guiYeuCauApi("/api/events/" + suKienId + "/ticket-types", "POST", {
      Ten: tenHienThi,
      Gia: giaVe,
      SoLuong: soLuong,
      MoBan: thoiGianMoBan || null,
      DongBan: thoiGianDongBan || null,
    });
    thongBao("Thêm hạng vé thành công.");
    taiLoaiVeTheoSuKien(suKienId);
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  }
}
async function capNhatLoaiVe() {
  var loaiVeId = docSoTuInput("lv-id"),
    tenHienThi = docGiaTriInput("lv-ten"),
    giaVe = docSoTuInput("lv-gia");
  var soLuong = docSoTuInput("lv-slmax"),
    thoiGianMoBan = docGiaTriInput("lv-mo"),
    thoiGianDongBan = docGiaTriInput("lv-dong");
  if (!loaiVeId || loaiVeId <= 0) {
    hienThongBaoNoi("Nhập mã hạng vé cần cập nhật.", "loi");
    return;
  }
  if (!tenHienThi) {
    hienThongBaoNoi("Nhập tên hạng vé.", "loi");
    return;
  }
  if (giaVe === null || isNaN(giaVe) || giaVe < 0) {
    hienThongBaoNoi("Giá vé phải là số không âm.", "loi");
    return;
  }
  if (soLuong === null || isNaN(soLuong) || soLuong <= 0) {
    hienThongBaoNoi("Số lượng phải lớn hơn 0.", "loi");
    return;
  }
  if (!thoiGianMoBan || !thoiGianDongBan) {
    hienThongBaoNoi("Chọn thời gian mở bán và đóng bán.", "loi");
    return;
  }
  try {
    await guiYeuCauApi("/api/ticket-types/" + loaiVeId, "PUT", {
      Ten: tenHienThi,
      Gia: giaVe,
      SoLuong: soLuong,
      MoBan: thoiGianMoBan || null,
      DongBan: thoiGianDongBan || null,
    });
    thongBao("Cập nhật hạng vé thành công.");
    taiLoaiVeTheoSuKien(docSoTuInput("lv-sk"));
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  }
}
document.addEventListener("DOMContentLoaded", async function () {
  var thamSoUrl = new URLSearchParams(location.search);
  var donVeId = Number(thamSoUrl.get("id"));
  if (donVeId > 0 && document.getElementById("tt-don")) document.getElementById("tt-don").value = donVeId;
  if (donVeId > 0 && location.pathname.endsWith("/don-ve/chi-tiet-don.html") && phienDangNhap.token)
    xemChiTietDon(donVeId);
  var oChonSuKien = document.getElementById("don-sukien");
  if (!oChonSuKien || !phienDangNhap.token) return;
  var oChonLoaiVe = document.getElementById("don-loai");
  var oSoLuongVe = document.getElementById("don-sl");
  var danhSachLoaiVe = [],
    lanTaiLoaiVe = 0;
  function capNhatTongTienDatVe() {
    var loaiVeDangChon = danhSachLoaiVe.find(function (loaiVe) {
      return loaiVe.Id === Number(oChonLoaiVe.value);
    });
    var nutTaoDon = document.querySelector("button[onclick=\"taoDonDatVe()\"]");
    if (nutTaoDon) nutTaoDon.disabled = !loaiVeDangChon || !!taoDonDatVe.dangGui;
    var khungTongTien = document.getElementById("don-total");
    if (khungTongTien)
      khungTongTien.textContent = loaiVeDangChon
        ? "Tổng tiền: " + dinhDangSoTien(loaiVeDangChon.Gia * Number(oSoLuongVe.value || 0)) + " ₫"
        : "—";
    if (typeof giaoDienQuanLySuKien !== "undefined") giaoDienQuanLySuKien.capNhatTomTatThanhToan();
  }
  async function taiLoaiVeDangMoBan() {
    var lanTaiHienTai = ++lanTaiLoaiVe;
    oChonLoaiVe.innerHTML = '<option value="">Đang tải hạng vé…</option>';
    danhSachLoaiVe = [];
    capNhatTongTienDatVe();
    if (!oChonSuKien.value) {
      oChonLoaiVe.innerHTML = '<option value="">Chọn sự kiện trước</option>';
      return;
    }
    try {
      var phanHoiLoaiVe = await guiYeuCauApi("/api/events/" + Number(oChonSuKien.value) + "/ticket-types");
      if (lanTaiHienTai !== lanTaiLoaiVe) return;
      danhSachLoaiVe = (phanHoiLoaiVe.data || []).filter(function (loaiVe) {
        return (
          loaiVe.SoLuong > loaiVe.DaBan + loaiVe.DangGiu &&
          chuyenNgayDuLieuSangDate(loaiVe.MoBan) <= new Date() &&
          chuyenNgayDuLieuSangDate(loaiVe.DongBan) > new Date()
        );
      });
      oChonLoaiVe.innerHTML = danhSachLoaiVe.length
        ? danhSachLoaiVe
            .map(function (loaiVe) {
              return (
                '<option value="' +
                Number(loaiVe.Id) +
                '">' +
                maHoaKyTuHtml(loaiVe.Ten) +
                " · " +
                (loaiVe.Gia ? maHoaKyTuHtml(dinhDangSoTien(loaiVe.Gia)) + " ₫" : "Miễn phí") +
                "</option>"
              );
            })
            .join("")
        : '<option value="">Chưa có hạng vé đang mở bán</option>';
      if (
        danhSachLoaiVe.some(function (loaiVe) {
          return loaiVe.Id === Number(thamSoUrl.get("ticket"));
        })
      )
        oChonLoaiVe.value = thamSoUrl.get("ticket");
      capNhatTongTienDatVe();
    } catch (loiXuLy) {
      if (lanTaiHienTai !== lanTaiLoaiVe) return;
      oChonLoaiVe.innerHTML = '<option value="">Chưa tải được hạng vé</option>';
      thongBao(loiXuLy.message, "loi");
    }
  }
  capNhatTongTienDatVe();
  oChonSuKien.addEventListener("change", taiLoaiVeDangMoBan);
  oChonLoaiVe.addEventListener("change", capNhatTongTienDatVe);
  oSoLuongVe.addEventListener("input", capNhatTongTienDatVe);
  try {
    var phanHoiSuKien = await guiYeuCauApi("/api/events");
    var suKienDaDuyet = (phanHoiSuKien.data || []).filter(function (suKien) {
      return suKien.TrangThai === 1 || suKien.TrangThai === "DaDuyet";
    });
    oChonSuKien.innerHTML =
      '<option value="">Chọn sự kiện</option>' +
      suKienDaDuyet
        .map(function (suKien) {
          return '<option value="' + Number(suKien.Id) + '">' + maHoaKyTuHtml(suKien.Ten) + "</option>";
        })
        .join("");
    if (typeof giaoDienQuanLySuKien !== "undefined") giaoDienQuanLySuKien.capNhatTomTatThanhToan();
    if (
      suKienDaDuyet.some(function (suKien) {
        return suKien.Id === Number(thamSoUrl.get("event"));
      })
    ) {
      oChonSuKien.value = thamSoUrl.get("event");
      await taiLoaiVeDangMoBan();
    }
  } catch (loiXuLy) {
    oChonSuKien.innerHTML = '<option value="">Chưa thể tải sự kiện. Vui lòng tải lại trang.</option>';
    thongBao(loiXuLy.message, "loi");
  }
});
