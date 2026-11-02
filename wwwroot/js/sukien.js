"use strict";

function chuanHoaDuLieuSuKien(giaTriCanXuLy) {
  if (Array.isArray(giaTriCanXuLy)) return giaTriCanXuLy.map(chuanHoaDuLieuSuKien);
  if (!giaTriCanXuLy || typeof giaTriCanXuLy !== "object") return giaTriCanXuLy;
  var suKienChuanHoa = {};
  Object.keys(giaTriCanXuLy).forEach(function (tenThuocTinhJson) {
    var tenThuocTinhVietHoa = tenThuocTinhJson.charAt(0).toUpperCase() + tenThuocTinhJson.slice(1);
    if (tenThuocTinhJson !== tenThuocTinhVietHoa && tenThuocTinhVietHoa in giaTriCanXuLy) return;
    suKienChuanHoa[tenThuocTinhVietHoa] = chuanHoaDuLieuSuKien(giaTriCanXuLy[tenThuocTinhJson]);
  });
  return suKienChuanHoa;
}
function trangThaiSuKienHienThi(giaTriCanXuLy) {
  var cacTrangThaiHienThi = ["ChoDuyet", "DaDuyet", "TuChoi", "DangDienRa", "DaHuy", "DaKetThuc"];
  return typeof giaTriCanXuLy === "number" ? cacTrangThaiHienThi[giaTriCanXuLy] || "" : giaTriCanXuLy;
}
function anhSuKienHTML(suKien, canTaiKhiMoTrang) {
  var giaTriDuPhong = "/assets/images/placeholder-event.svg";
  var duLieuNguon = String(suKien.HinhAnh || giaTriDuPhong).trim();
  try {
    var urlYeuCau = new URL(duLieuNguon, location.href);
    if (urlYeuCau.protocol !== "http:" && urlYeuCau.protocol !== "https:") duLieuNguon = giaTriDuPhong;
  } catch (loiXuLy) {
    duLieuNguon = giaTriDuPhong;
  }
  return (
    '<img src="' +
    maHoaKyTuHtml(duLieuNguon) +
    '" alt="' +
    maHoaKyTuHtml(suKien.Ten || "Sự kiện") +
    '" width="1200" height="675"' +
    (canTaiKhiMoTrang ? ' loading="lazy"' : "") +
    ' decoding="async" onerror="this.onerror=null;this.src=\'' +
    giaTriDuPhong +
    "'\">"
  );
}
var __suKienCache = null;
var __suKienTrang = 1;
var __suKienLoai = "";
var __suKienDangTai = null;
function phamViDanhSachSuKien() {
  var khungDanhSachSuKien = document.getElementById("ds-sukien");
  return (khungDanhSachSuKien && khungDanhSachSuKien.getAttribute("data-event-scope")) || "public";
}
function chuoiTimKiem(giaTriCanXuLy) {
  return chuyenGiaTriThanhVanBan(giaTriCanXuLy)
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/đ/gi, "d")
    .toLowerCase();
}
function locLoaiSuKien(tenLoaiSuKien) {
  __suKienLoai = tenLoaiSuKien || "";
  __suKienTrang = 1;
  if (Array.isArray(__suKienCache)) hienThiDanhSachSuKien();
  else taiDanhSachSuKien();
}
function chuyenTrangSuKien(soTrangHienTai) {
  __suKienTrang = soTrangHienTai;
  hienThiDanhSachSuKien();
  var khungSoLuongKetQua = document.getElementById("event-count");
  if (khungSoLuongKetQua)
    khungSoLuongKetQua.scrollIntoView({
      behavior: "smooth",
      block: "center",
    });
}
function timSuKienTrangChu(suKienDom) {
  suKienDom.preventDefault();
  location.assign("/pages/su-kien/danh-sach.html?q=" + encodeURIComponent(docGiaTriInput("sk-tukhoa-page")));
}
async function taiDanhSachSuKien(canTaiLaiDanhSach) {
  var khungDanhSachSuKien = document.getElementById("ds-sukien");
  if (!khungDanhSachSuKien) return;
  var phamViHienThi = phamViDanhSachSuKien();
  if (
    phamViHienThi !== "public" &&
    (!phienDangNhap.token ||
      (phamViHienThi === "mine"
        ? phienDangNhap.vaiTro !== "BanToChuc"
        : ["QuanLySuKien", "QuanTriVien"].indexOf(phienDangNhap.vaiTro) === -1))
  )
    return;
  if (canTaiLaiDanhSach === true) __suKienCache = null;
  if (__suKienDangTai) return __suKienDangTai;
  hienKhungDangTai("ds-sukien", 4);
  __suKienDangTai = (async function () {
    try {
      var phanHoiSuKien = await guiYeuCauApi("/api/events" + (phamViHienThi === "public" ? "" : "/" + phamViHienThi));
      __suKienCache = chuanHoaDuLieuSuKien(phanHoiSuKien.data || []);
      if (typeof giaoDienQuanLySuKien !== "undefined") giaoDienQuanLySuKien.ghiNhanDaTaiSuKien();
      hienThiDanhSachSuKien();
    } catch (loiXuLy) {
      __suKienCache = null;
      khungDanhSachSuKien.innerHTML =
        '<div class="thongbao loi"><p>' +
        maHoaKyTuHtml(loiXuLy.message) +
        '</p><button class="phu" onclick="taiDanhSachSuKien()">Thử lại</button></div>';
    } finally {
      __suKienDangTai = null;
    }
  })();
  return __suKienDangTai;
}
function hienThiDanhSachSuKien() {
  var khungDanhSachSuKien = document.getElementById("ds-sukien");
  if (!khungDanhSachSuKien || !Array.isArray(__suKienCache)) return;
  var phamViHienThi = phamViDanhSachSuKien();
  var tuKhoaTimKiem = chuoiTimKiem(docGiaTriInput("sk-tukhoa-page") || docGiaTriInput("sk-tukhoa"));
  var danhSachSuKien = typeof giaoDienQuanLySuKien !== "undefined" ? __suKienCache.map(giaoDienQuanLySuKien.hopNhatChiTietSuKien) : __suKienCache;
  var suKienSauBoLoc = danhSachSuKien.filter(function (suKien) {
    if (phamViHienThi === "manage" && trangThaiSuKienHienThi(suKien.TrangThai) !== "ChoDuyet") return false;
    var tenLoaiSuKien = suKien.LoaiSuKien && suKien.LoaiSuKien.Ten;
    if (__suKienLoai && tenLoaiSuKien !== __suKienLoai) return false;
    return (
      !tuKhoaTimKiem ||
      chuoiTimKiem(
        [suKien.Ten, tenLoaiSuKien, suKien.BanToChuc && suKien.BanToChuc.Ten, suKien.MoTa].join(" "),
      ).indexOf(tuKhoaTimKiem) !== -1
    );
  });
  if (typeof giaoDienQuanLySuKien !== "undefined") suKienSauBoLoc = giaoDienQuanLySuKien.locDanhSachSuKien(suKienSauBoLoc);
  var soSuKienMoiTrang =
    Number(khungDanhSachSuKien.getAttribute("data-page-size")) ||
    (document.getElementById("event-pagination") ? 12 : Math.max(1, suKienSauBoLoc.length));
  var tongSoTrang = Math.max(1, Math.ceil(suKienSauBoLoc.length / soSuKienMoiTrang));
  __suKienTrang = Math.min(Math.max(1, __suKienTrang), tongSoTrang);
  var chiSoSuKienDauTrang = (__suKienTrang - 1) * soSuKienMoiTrang;
  khungDanhSachSuKien.classList.remove("grid", "g3");
  if (phamViHienThi === "public") khungDanhSachSuKien.classList.add("ev-grid");
  else khungDanhSachSuKien.classList.remove("ev-grid");
  var khungSoLuongKetQua = document.getElementById("event-count");
  if (khungSoLuongKetQua)
    khungSoLuongKetQua.textContent = suKienSauBoLoc.length
      ? document.getElementById("event-pagination")
        ? suKienSauBoLoc.length +
          " sự kiện · Hiển thị " +
          (chiSoSuKienDauTrang + 1) +
          "–" +
          Math.min(chiSoSuKienDauTrang + soSuKienMoiTrang, suKienSauBoLoc.length)
        : suKienSauBoLoc.length + " sự kiện đang chờ bạn khám phá"
      : "Không tìm thấy sự kiện phù hợp";
  var dangKiemTraThanhToan =
    !suKienSauBoLoc.length &&
    typeof giaoDienQuanLySuKien !== "undefined" &&
    giaoDienQuanLySuKien.ghiNhanDangTaiThongTinBoSung() &&
    (docGiaTriInput("filter-price") || docGiaTriInput("filter-location"));
  if (dangKiemTraThanhToan) {
    hienKhungDangTai("ds-sukien", 3);
    if (khungSoLuongKetQua) khungSoLuongKetQua.textContent = "Đang kiểm tra thông tin phù hợp với bộ lọc…";
  } else
    khungDanhSachSuKien.innerHTML = suKienSauBoLoc.length
      ? typeof giaoDienQuanLySuKien !== "undefined"
        ? phamViHienThi === "public"
          ? suKienSauBoLoc
              .slice(chiSoSuKienDauTrang, chiSoSuKienDauTrang + soSuKienMoiTrang)
              .map(giaoDienQuanLySuKien.taoTheSuKien)
              .join("")
          : giaoDienQuanLySuKien.taoBangSuKien(suKienSauBoLoc.slice(chiSoSuKienDauTrang, chiSoSuKienDauTrang + soSuKienMoiTrang), phamViHienThi)
        : suKienSauBoLoc
            .slice(chiSoSuKienDauTrang, chiSoSuKienDauTrang + soSuKienMoiTrang)
            .map(function (suKien) {
              var lienKetDieuHuong = "/pages/su-kien/chi-tiet.html?id=" + Number(suKien.Id);
              var trangThaiSuKien = trangThaiSuKienHienThi(suKien.TrangThai);
              var cacThaoTacChoPhep = '<a class="phu btn sm" href="' + lienKetDieuHuong + '">Chi tiết</a>';
              if (phamViHienThi === "public")
                cacThaoTacChoPhep =
                  '<a class="chinh btn sm" href="' +
                  lienKetDieuHuong +
                  '#tickets">' +
                  (trangThaiSuKien === "DaKetThuc" ? "Xem sự kiện" : "Xem vé") +
                  "</a>" +
                  cacThaoTacChoPhep;
              if (
                phamViHienThi === "manage" &&
                Number(suKien.BanToChuc && suKien.BanToChuc.TaiKhoanId) !== Number(phienDangNhap.taiKhoanID)
              ) {
                cacThaoTacChoPhep +=
                  '<button class="chinh sm" onclick="duyetSuKien(' +
                  Number(suKien.Id) +
                  ')">Duyệt</button><button class="phu sm" onclick="tuChoiSuKien(' +
                  Number(suKien.Id) +
                  ')">Từ chối</button>';
              }
              if (phamViHienThi === "mine" && ["DaHuy", "DaKetThuc"].indexOf(trangThaiSuKien) === -1)
                cacThaoTacChoPhep +=
                  '<button class="phu sm" onclick="huySuKien(' +
                  Number(suKien.Id) +
                  ')">Hủy sự kiện</button>';
              return (
                '<article class="ev-card"><a class="ev-cover" href="' +
                lienKetDieuHuong +
                '">' +
                anhSuKienHTML(suKien, true) +
                '</a><div class="ev-body">' +
                '<span class="badge b-info">' +
                maHoaKyTuHtml((suKien.LoaiSuKien && suKien.LoaiSuKien.Ten) || "Sự kiện") +
                "</span>" +
                (phamViHienThi !== "public" ? taoNhanTrangThai(trangThaiSuKien) : "") +
                '<h3 class="ev-title"><a href="' +
                lienKetDieuHuong +
                '">' +
                maHoaKyTuHtml(suKien.Ten) +
                "</a></h3>" +
                '<div class="ev-meta">' +
                maHoaKyTuHtml(dinhDangNgayGioVietNam(suKien.BatDau)) +
                "</div>" +
                '<div class="ev-meta">' +
                maHoaKyTuHtml((suKien.BanToChuc && suKien.BanToChuc.Ten) || "") +
                "</div>" +
                '<div class="ev-actions">' +
                cacThaoTacChoPhep +
                "</div></div></article>"
              );
            })
            .join("")
      : phamViHienThi === "public"
        ? '<div class="empty"><h3>Chưa có sự kiện phù hợp</h3><p>Thử một từ khóa khác hoặc chọn chủ đề khác.</p><button class="phu" onclick="xoaBoLocSuKien()">Xem tất cả sự kiện</button></div>'
        : '<div class="empty"><h3>' +
          (phamViHienThi === "manage" ? "Không có sự kiện chờ duyệt" : "Bạn chưa có sự kiện nào") +
          "</h3><p>" +
          (phamViHienThi === "manage"
            ? "Các sự kiện cần xử lý sẽ xuất hiện tại đây."
            : "Chọn Tạo sự kiện để bắt đầu.") +
          "</p></div>";
  var khungPhanTrang = document.getElementById("event-pagination");
  if (khungPhanTrang)
    khungPhanTrang.innerHTML =
      tongSoTrang > 1
        ? '<button class="phu" onclick="chuyenTrangSuKien(' +
          (__suKienTrang - 1) +
          ')"' +
          (__suKienTrang === 1 ? " disabled" : "") +
          ">← Trước</button><span>Trang " +
          __suKienTrang +
          " / " +
          tongSoTrang +
          '</span><button class="phu" onclick="chuyenTrangSuKien(' +
          (__suKienTrang + 1) +
          ')"' +
          (__suKienTrang === tongSoTrang ? " disabled" : "") +
          ">Sau →</button>"
        : "";
  if (typeof giaoDienQuanLySuKien !== "undefined")
    giaoDienQuanLySuKien.capNhatGiaoDienSauTaiSuKien(__suKienCache || [], suKienSauBoLoc.slice(chiSoSuKienDauTrang, chiSoSuKienDauTrang + soSuKienMoiTrang));
}
function xoaBoLocSuKien() {
  ["sk-tukhoa", "sk-tukhoa-hero", "sk-tukhoa-page", "filter-date", "filter-location", "filter-price"].forEach(
    function (idOBoLoc) {
      var oBoLoc = document.getElementById(idOBoLoc);
      if (oBoLoc) oBoLoc.value = "";
    },
  );
  document.querySelectorAll("button[data-category]").forEach(function (nutLocLoaiSuKien) {
    nutLocLoaiSuKien.classList.toggle("active", !nutLocLoaiSuKien.getAttribute("data-category"));
  });
  locLoaiSuKien("");
}
async function xemChiTietSuKien(suKienId) {
  suKienId = Number(suKienId);
  if (!Number.isInteger(suKienId) || suKienId < 1) return;
  var khungChiTietSuKien = document.getElementById("sk-chitiet");
  if (!khungChiTietSuKien) {
    location.assign("/pages/su-kien/chi-tiet.html?id=" + suKienId);
    return;
  }
  hienKhungDangTai("sk-chitiet", 5);
  try {
    var phanHoiSuKien = await guiYeuCauApi("/api/events/" + suKienId);
    var suKien = chuanHoaDuLieuSuKien(phanHoiSuKien.data || {});
    var diaDiem = suKien.DatDiaDiem && suKien.DatDiaDiem.DiaDiem;
    document.title = chuyenGiaTriThanhVanBan(suKien.Ten) + " — QLSK";
    if (typeof giaoDienQuanLySuKien !== "undefined") khungChiTietSuKien.innerHTML = giaoDienQuanLySuKien.taoHtmlChiTietSuKien(suKien);
    else {
      khungChiTietSuKien.innerHTML =
        '<div class="ev-detail-cover">' +
        anhSuKienHTML(suKien, false) +
        '</div><span class="badge b-info">' +
        maHoaKyTuHtml((suKien.LoaiSuKien && suKien.LoaiSuKien.Ten) || "Sự kiện") +
        "</span><h1>" +
        maHoaKyTuHtml(suKien.Ten) +
        "</h1>" +
        '<div class="toolbar">' +
        taoNhanTrangThai(trangThaiSuKienHienThi(suKien.TrangThai)) +
        "</div>" +
        "<p><b>Thời gian:</b> " +
        maHoaKyTuHtml(dinhDangNgayGioVietNam(suKien.BatDau)) +
        " – " +
        maHoaKyTuHtml(dinhDangNgayGioVietNam(suKien.KetThuc)) +
        "</p>" +
        (diaDiem
          ? "<p><b>Địa điểm:</b> " + maHoaKyTuHtml(diaDiem.Ten) + "<br>" + maHoaKyTuHtml(diaDiem.DiaChi) + "</p>"
          : "") +
        (suKien.BanToChuc ? "<p><b>Ban tổ chức:</b> " + maHoaKyTuHtml(suKien.BanToChuc.Ten) + "</p>" : "") +
        '<h2>Về sự kiện</h2><p class="event-description">' +
        maHoaKyTuHtml(suKien.MoTa || "Thông tin sự kiện đang được cập nhật.") +
        "</p>";
    }
    await taiLoaiVeTheoSuKien(suKienId);
    if (location.hash === "#tickets")
      document.getElementById("tickets").scrollIntoView({
        block: "start",
      });
  } catch (loiXuLy) {
    khungChiTietSuKien.innerHTML =
      '<div class="empty"><h1>Chưa thể tải sự kiện</h1><p>' +
      maHoaKyTuHtml(loiXuLy.message) +
      '</p><a class="phu btn" href="danh-sach.html">Quay lại danh sách</a></div>';
  }
}
document.addEventListener("DOMContentLoaded", function () {
  if (location.pathname.endsWith("/su-kien/chi-tiet.html"))
    xemChiTietSuKien(new URLSearchParams(location.search).get("id"));
});
async function taoSuKien() {
  var tenHienThi = docGiaTriInput("sk-ten");
  var loaiSuKienId = docSoTuInput("sk-loai");
  var batDau = docGiaTriInput("sk-bd"),
    ketThuc = docGiaTriInput("sk-kt");
  var soLuong = docSoTuInput("sk-sl");
  if (!tenHienThi) {
    hienThongBaoNoi("Vui lòng nhập tên sự kiện.", "loi");
    return;
  }
  if (!loaiSuKienId || loaiSuKienId <= 0) {
    hienThongBaoNoi("Vui lòng chọn loại sự kiện.", "loi");
    return;
  }
  if (!kiemTraKhoangThoiGianForm(batDau, ketThuc)) return;
  if (soLuong === null || isNaN(soLuong) || soLuong <= 0) {
    hienThongBaoNoi("Số người dự kiến phải lớn hơn 0.", "loi");
    return;
  }
  var nutTaoSuKien = document.querySelector("[data-wizard-submit]");
  if (nutTaoSuKien && nutTaoSuKien.disabled) return;
  if (nutTaoSuKien) nutTaoSuKien.disabled = true;
  try {
    var phanHoiTaoSuKien = await guiYeuCauApi("/api/events", "POST", {
      Ten: tenHienThi,
      MoTa: docGiaTriInput("sk-mota") || null,
      BatDau: batDau || null,
      KetThuc: ketThuc || null,
      SoNguoi: soLuong,
      LoaiSuKienId: loaiSuKienId,
    });
    thongBao("Đã tạo sự kiện và gửi chờ duyệt.");
    location.assign("/pages/ban-to-chuc/su-kien-cua-toi.html");
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  } finally {
    if (nutTaoSuKien) nutTaoSuKien.disabled = false;
  }
}
async function duyetSuKien(suKienId) {
  try {
    await guiYeuCauApi("/api/events/" + suKienId + "/approve", "POST", null);
    thongBao("Đã duyệt sự kiện.");
    await taiDanhSachSuKien(true);
    return true;
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
    return false;
  }
}
async function tuChoiSuKien(suKienId) {
  var lyDoTuChoi = prompt("Nhập lý do từ chối sự kiện:");
  if (lyDoTuChoi === null) return;
  if (!lyDoTuChoi.trim()) {
    hienThongBaoNoi("Vui lòng nhập lý do từ chối.", "loi");
    return;
  }
  try {
    await guiYeuCauApi("/api/events/" + suKienId + "/reject", "POST", {
      LyDo: lyDoTuChoi.trim(),
    });
    thongBao("Đã từ chối sự kiện.");
    await taiDanhSachSuKien(true);
    return true;
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
    return false;
  }
}
async function huySuKien(suKienId) {
  if (!confirm("Bạn có chắc muốn hủy sự kiện này?")) return;
  try {
    await guiYeuCauApi("/api/events/" + suKienId + "/cancel", "POST", null);
    thongBao("Đã hủy sự kiện.");
    await taiDanhSachSuKien(true);
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  }
}
var __loaiSuKienCache = [];
async function taiLoaiSuKien() {
  hienKhungDangTai("ds-loai", 3);
  try {
    var laDanhMucQuanTri = !!document.getElementById("ds-loai") && phienDangNhap.vaiTro === "QuanTriVien";
    var phanHoiLoaiSuKien = await guiYeuCauApi("/api/event-types" + (laDanhMucQuanTri ? "/all" : ""));
    __loaiSuKienCache = phanHoiLoaiSuKien.data || [];
    if (typeof giaoDienQuanLySuKien !== "undefined") giaoDienQuanLySuKien.hienLoaiSuKienTrangChu(__loaiSuKienCache);
    document.querySelectorAll("[data-event-categories]").forEach(function (khungLocLoaiSuKien) {
      khungLocLoaiSuKien.replaceChildren();
      [
        {
          Ten: "",
          Label: "Tất cả",
        },
      ]
        .concat(__loaiSuKienCache)
        .forEach(function (loaiSuKien) {
          var nutLocLoaiSuKien = document.createElement("button");
          nutLocLoaiSuKien.className = "catchip" + (loaiSuKien.Ten === __suKienLoai ? " active" : "");
          nutLocLoaiSuKien.textContent = loaiSuKien.Label || loaiSuKien.Ten;
          nutLocLoaiSuKien.onclick = function () {
            khungLocLoaiSuKien.querySelectorAll("button").forEach(function (phanTuDom) {
              phanTuDom.classList.remove("active");
            });
            nutLocLoaiSuKien.classList.add("active");
            locLoaiSuKien(loaiSuKien.Ten);
          };
          khungLocLoaiSuKien.appendChild(nutLocLoaiSuKien);
        });
    });
    var oChonLoaiSuKien = document.getElementById("sk-loai");
    if (oChonLoaiSuKien) {
      oChonLoaiSuKien.innerHTML =
        '<option value="">Chọn loại sự kiện</option>' +
        (phanHoiLoaiSuKien.data || [])
          .map(function (loaiSuKien) {
            return '<option value="' + loaiSuKien.Id + '">' + maHoaKyTuHtml(loaiSuKien.Ten) + "</option>";
          })
          .join("");
    }
    var khungDanhSachLoaiSuKien = document.getElementById("ds-loai");
    if (khungDanhSachLoaiSuKien) {
      khungDanhSachLoaiSuKien.innerHTML = taoHtmlBang(
        [
          ["Ten", "Tên loại"],
          ["MoTa", "Mô tả"],
          ["HoatDong", "Hoạt động"],
        ],
        (phanHoiLoaiSuKien.data || []).map(function (loaiSuKien) {
          return {
            Ten: loaiSuKien.Ten,
            MoTa: loaiSuKien.MoTa,
            HoatDong: loaiSuKien.HoatDong ? "Đang hoạt động" : "Đã tắt",
            Id: loaiSuKien.Id,
          };
        }),
        phienDangNhap.vaiTro === "QuanTriVien"
          ? function (loaiSuKien) {
              return (
                '<button class="phu sm" onclick="dienFormSuaLoaiSuKien(' +
                loaiSuKien.Id +
                ')">Sửa</button> <button class="phu sm" onclick="xoaLoaiSuKien(' +
                loaiSuKien.Id +
                ')">Xóa</button> <button class="phu sm" onclick="batLoaiSuKien(' +
                loaiSuKien.Id +
                ')">Bat</button> ' +
                '<button class="phu sm" onclick="tatLoaiSuKien(' +
                loaiSuKien.Id +
                ')">Tat</button>'
              );
            }
          : null,
      );
    }
  } catch (loiXuLy) {
    hienLoiTai("ds-loai", loiXuLy, taiLoaiSuKien);
    var oChonDuLieu = document.getElementById("sk-loai");
    if (oChonDuLieu) oChonDuLieu.innerHTML = '<option value="">Chưa tải được loại sự kiện</option>';
    document.querySelectorAll("[data-event-categories], [data-home-categories]").forEach(function (khungDanhSachLoaiSuKien) {
      khungDanhSachLoaiSuKien.innerHTML = '<button class="phu sm" type="button">Tải lại chủ đề</button>';
      khungDanhSachLoaiSuKien.querySelector("button").onclick = taiLoaiSuKien;
    });
    thongBao(loiXuLy.message, "loi");
  }
}
function dienFormSuaLoaiSuKien(loaiSuKienId) {
  var loaiSuKien = __loaiSuKienCache.find(function (loaiSuKien) {
    return loaiSuKien.Id === loaiSuKienId;
  });
  [
    ["id", loaiSuKien.Id],
    ["ten", loaiSuKien.Ten],
    ["mota", loaiSuKien.MoTa],
  ].forEach(function (capTruongGiaTri) {
    document.getElementById("lsk-" + capTruongGiaTri[0]).value = capTruongGiaTri[1] || "";
  });
}
async function xoaLoaiSuKien(loaiSuKienId) {
  if (!confirm("Xóa loại chưa được sự kiện nào sử dụng?")) return;
  try {
    await guiYeuCauApi("/api/admin/catalog/event-types/" + loaiSuKienId, "DELETE");
    await taiLoaiSuKien();
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  }
}
async function themLoaiSuKien() {
  var tenHienThi = docGiaTriInput("lsk-ten");
  if (!tenHienThi) {
    hienThongBaoNoi("Nhập tên loại sự kiện.", "loi");
    return;
  }
  try {
    await guiYeuCauApi("/api/event-types", "POST", {
      Ten: tenHienThi,
      MoTa: docGiaTriInput("lsk-mota") || null,
    });
    thongBao("Thêm loại sự kiện thành công.");
    taiLoaiSuKien();
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  }
}
async function capNhatLoaiSuKien() {
  var loaiSuKienId = docSoTuInput("lsk-id");
  if (!loaiSuKienId || loaiSuKienId <= 0) {
    hienThongBaoNoi("Nhập mã loại sự kiện cần cập nhật.", "loi");
    return;
  }
  var tenHienThi = docGiaTriInput("lsk-ten");
  if (!tenHienThi) {
    hienThongBaoNoi("Nhập tên loại sự kiện.", "loi");
    return;
  }
  try {
    await guiYeuCauApi("/api/event-types/" + loaiSuKienId, "PUT", {
      Ten: tenHienThi,
      MoTa: docGiaTriInput("lsk-mota") || null,
    });
    thongBao("Cập nhật loại sự kiện thành công.");
    taiLoaiSuKien();
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  }
}
function batLoaiSuKien(loaiSuKienId) {
  return capNhatHoatDongLoaiSuKien(loaiSuKienId, true);
}
function tatLoaiSuKien(loaiSuKienId) {
  return capNhatHoatDongLoaiSuKien(loaiSuKienId, false);
}
async function capNhatHoatDongLoaiSuKien(loaiSuKienId, hoatDong) {
  var loaiSuKienCanSua = __loaiSuKienCache.find(function (loaiSuKien) {
    return Number(loaiSuKien.Id) === Number(loaiSuKienId);
  });
  if (!loaiSuKienCanSua) return;
  try {
    await guiYeuCauApi("/api/event-types/" + loaiSuKienId, "PUT", {
      Ten: loaiSuKienCanSua.Ten,
      MoTa: loaiSuKienCanSua.MoTa || "",
      HoatDong: hoatDong,
    });
    thongBao("Đã cập nhật trạng thái loại sự kiện.");
    taiLoaiSuKien();
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  }
}
