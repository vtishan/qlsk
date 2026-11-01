"use strict";

function ngayThang(value) { var date = ngayDuLieu(value); return isNaN(date) ? "—" : String(date.getDate()).padStart(2, "0"); }
function thangNam(value) { var date = ngayDuLieu(value); return isNaN(date) ? "" : "T" + (date.getMonth() + 1); }
function chuanHoaDuLieuSuKien(value) {
  if (Array.isArray(value)) return value.map(chuanHoaDuLieuSuKien);
  if (!value || typeof value !== "object") return value;
  var result = {};
  Object.keys(value).forEach(function (key) {
    var upper = key.charAt(0).toUpperCase() + key.slice(1);
    if (key !== upper && upper in value) return;
    result[upper] = chuanHoaDuLieuSuKien(value[key]);
  });
  return result;
}
function trangThaiSuKienHienThi(value) {
  var states = ["ChoDuyet", "DaDuyet", "TuChoi", "DangDienRa", "DaHuy", "DaKetThuc"];
  return typeof value === "number" ? (states[value] || "") : value;
}
function anhSuKienHTML(event, lazy) {
  var fallback = "/assets/images/placeholder-event.svg";
  var source = String(event.HinhAnh || fallback).trim();
  try {
    var url = new URL(source, location.href);
    if (url.protocol !== "http:" && url.protocol !== "https:") source = fallback;
  } catch (error) { source = fallback; }
  return '<img src="' + esc(source) + '" alt="' + esc(event.Ten || "Sự kiện") + '" width="1200" height="675"' +
    (lazy ? ' loading="lazy"' : '') + ' decoding="async" onerror="this.onerror=null;this.src=\'' + fallback + '\'">';
}

var __suKienCache = null;
var __suKienTrang = 1;
var __suKienLoai = "";
var __suKienDangTai = null;
function phamViDanhSachSuKien() {
  var host = document.getElementById("ds-sukien");
  return host && host.getAttribute("data-event-scope") || "public";
}
function chuoiTimKiem(value) { return noiDungGiaoDien(value).normalize("NFD").replace(/[\u0300-\u036f]/g, "").replace(/đ/gi, "d").toLowerCase(); }
function locLoaiSuKien(category) { __suKienLoai = category || ""; __suKienTrang = 1; taiDanhSachSuKien(); }
function chuyenTrangSuKien(page) {
  __suKienTrang = page;
  veDanhSachSuKien();
  var count = document.getElementById("event-count");
  if (count) count.scrollIntoView({ behavior: "smooth", block: "center" });
}
function timSuKienTrangChu(event) {
  event.preventDefault();
  location.assign("/pages/su-kien/danh-sach.html?q=" + encodeURIComponent(val("sk-tukhoa-page")));
}

async function taiDanhSachSuKien(refresh) {
  var host = document.getElementById("ds-sukien");
  if (!host) return;
  var scope = phamViDanhSachSuKien();
  if (scope !== "public" && (!store.token || (scope === "mine" ? store.vaiTro !== "BanToChuc" : ["QuanLySuKien", "QuanTriVien"].indexOf(store.vaiTro) === -1))) return;
  if (refresh === true) __suKienCache = null;
  if (__suKienDangTai) return __suKienDangTai;
  skeleton("ds-sukien", 4);
  __suKienDangTai = (async function () {
    try {
      var result = await apiFetch("/api/events" + (scope === "public" ? "" : "/" + scope));
      __suKienCache = chuanHoaDuLieuSuKien(result.data || []);
      veDanhSachSuKien();
    } catch (error) {
      host.innerHTML = '<div class="thongbao loi"><p>' + esc(error.message) + '</p><button class="phu" onclick="taiDanhSachSuKien()">Thử lại</button></div>';
    } finally { __suKienDangTai = null; }
  })();
  return __suKienDangTai;
}

function veDanhSachSuKien() {
  var host = document.getElementById("ds-sukien");
  if (!host) return;
  var scope = phamViDanhSachSuKien();
  var keyword = chuoiTimKiem(val("sk-tukhoa-page") || val("sk-tukhoa"));
  var filtered = (__suKienCache || []).filter(function (item) {
    if (scope === "manage" && trangThaiSuKienHienThi(item.TrangThai) !== "ChoDuyet") return false;
    var category = item.LoaiSuKien && item.LoaiSuKien.Ten;
    if (__suKienLoai && category !== __suKienLoai) return false;
    return !keyword || chuoiTimKiem([item.Ten, category, item.BanToChuc && item.BanToChuc.Ten, item.MoTa].join(" ")).indexOf(keyword) !== -1;
  });
  var size = Number(host.getAttribute("data-page-size")) || (document.getElementById("event-pagination") ? 12 : Math.max(1, filtered.length));
  var pages = Math.max(1, Math.ceil(filtered.length / size));
  __suKienTrang = Math.min(Math.max(1, __suKienTrang), pages);
  var start = (__suKienTrang - 1) * size;
  host.classList.remove("grid", "g3");
  host.classList.add("ev-grid");
  var count = document.getElementById("event-count");
  if (count) count.textContent = filtered.length ? (document.getElementById("event-pagination") ? filtered.length + " sự kiện · Hiển thị " + (start + 1) + "–" + Math.min(start + size, filtered.length) : filtered.length + " sự kiện đang chờ bạn khám phá") : "Không tìm thấy sự kiện phù hợp";
  host.innerHTML = filtered.length ? filtered.slice(start, start + size).map(function (item) {
    var link = "/pages/su-kien/chi-tiet.html?id=" + Number(item.Id);
    var state = trangThaiSuKienHienThi(item.TrangThai);
    var actions = '<a class="phu btn sm" href="' + link + '">Chi tiết</a>';
    if (scope === "public") actions = '<a class="chinh btn sm" href="' + link + '#tickets">' + (state === "DaKetThuc" ? "Xem sự kiện" : "Xem vé") + '</a>' + actions;
    if (scope === "manage" && Number(item.BanToChuc && item.BanToChuc.TaiKhoanId) !== Number(store.taiKhoanID)) {
      actions += '<button class="chinh sm" onclick="duyetSuKien(' + Number(item.Id) + ')">Duyệt</button><button class="phu sm" onclick="tuChoiSuKien(' + Number(item.Id) + ')">Từ chối</button>';
    }
    if (scope === "mine" && ["DaHuy", "DaKetThuc"].indexOf(state) === -1) actions += '<button class="phu sm" onclick="huySuKien(' + Number(item.Id) + ')">Hủy sự kiện</button>';
    return '<article class="ev-card"><a class="ev-cover" href="' + link + '">' + anhSuKienHTML(item, true) + '</a><div class="ev-body">' +
      '<span class="badge b-info">' + esc(item.LoaiSuKien && item.LoaiSuKien.Ten || "Sự kiện") + '</span>' +
      (scope !== "public" ? badgeTT(state) : '') +
      '<h3 class="ev-title"><a href="' + link + '">' + esc(item.Ten) + '</a></h3>' +
      '<div class="ev-meta">' + esc(isoLocal(item.BatDau)) + '</div>' +
      '<div class="ev-meta">' + esc(item.BanToChuc && item.BanToChuc.Ten || "") + '</div>' +
      '<div class="ev-actions">' + actions + '</div></div></article>';
  }).join("") : (scope === "public" ? '<div class="empty"><h3>Chưa có sự kiện phù hợp</h3><p>Thử một từ khóa khác hoặc chọn chủ đề khác.</p><button class="phu" onclick="xoaBoLocSuKien()">Xem tất cả sự kiện</button></div>' : '<div class="empty"><h3>' + (scope === "manage" ? 'Không có sự kiện chờ duyệt' : 'Bạn chưa có sự kiện nào') + '</h3><p>' + (scope === "manage" ? 'Các sự kiện cần xử lý sẽ xuất hiện tại đây.' : 'Chọn Tạo sự kiện để bắt đầu.') + '</p></div>');
  var pager = document.getElementById("event-pagination");
  if (pager) pager.innerHTML = pages > 1 ? '<button class="phu" onclick="chuyenTrangSuKien(' + (__suKienTrang - 1) + ')"' + (__suKienTrang === 1 ? ' disabled' : '') + '>← Trước</button><span>Trang ' + __suKienTrang + ' / ' + pages + '</span><button class="phu" onclick="chuyenTrangSuKien(' + (__suKienTrang + 1) + ')"' + (__suKienTrang === pages ? ' disabled' : '') + '>Sau →</button>' : "";
}
function xoaBoLocSuKien() {
  ["sk-tukhoa", "sk-tukhoa-page"].forEach(function (id) { var input = document.getElementById(id); if (input) input.value = ""; });
  document.querySelectorAll("button[data-category]").forEach(function (button) { button.classList.toggle("active", !button.getAttribute("data-category")); });
  locLoaiSuKien("");
}

async function xemChiTietSuKien(id) {
  id = Number(id);
  if (!Number.isInteger(id) || id < 1) return;
  var host = document.getElementById("sk-chitiet");
  if (!host) { location.assign("/pages/su-kien/chi-tiet.html?id=" + id); return; }
  skeleton("sk-chitiet", 5);
  try {
    var result = await apiFetch("/api/events/" + id);
    var item = chuanHoaDuLieuSuKien(result.data || {});
    var venue = item.DatDiaDiem && item.DatDiaDiem.DiaDiem;
    document.title = noiDungGiaoDien(item.Ten) + " — SựKiện Hub";
    host.innerHTML = '<div class="ev-detail-cover">' + anhSuKienHTML(item, false) + '</div><span class="badge b-info">' + esc(item.LoaiSuKien && item.LoaiSuKien.Ten || "Sự kiện") + '</span><h1>' + esc(item.Ten) + '</h1>' +
      '<div class="toolbar">' + badgeTT(trangThaiSuKienHienThi(item.TrangThai)) + '</div>' +
      '<p><b>Thời gian:</b> ' + esc(isoLocal(item.BatDau)) + ' – ' + esc(isoLocal(item.KetThuc)) + '</p>' +
      (venue ? '<p><b>Địa điểm:</b> ' + esc(venue.Ten) + '<br>' + esc(venue.DiaChi) + '</p>' : '') +
      (item.BanToChuc ? '<p><b>Ban tổ chức:</b> ' + esc(item.BanToChuc.Ten) + '</p>' : '') +
      '<h2>Về sự kiện</h2><p class="event-description">' + esc(item.MoTa || "Thông tin sự kiện đang được cập nhật.") + '</p>';
    await taiLoaiVeTheoSuKien(id);
    if (location.hash === "#tickets") document.getElementById("tickets").scrollIntoView({ block: "start" });
  } catch (error) {
    host.innerHTML = '<div class="empty"><h1>Chưa thể tải sự kiện</h1><p>' + esc(error.message) + '</p><a class="phu btn" href="danh-sach.html">Quay lại danh sách</a></div>';
  }
}
document.addEventListener("DOMContentLoaded", function () {
  if (location.pathname.endsWith("/su-kien/chi-tiet.html")) xemChiTietSuKien(new URLSearchParams(location.search).get("id"));
});

async function taoSuKien() {
  var ten = val("sk-ten");
  var loai = num("sk-loai");
  var bd = val("sk-bd"), kt = val("sk-kt");
  var sl = num("sk-sl");
  if (!ten) { toast("Vui lòng nhập tên sự kiện.", "loi"); return; }
  if (!loai || loai <= 0) { toast("Vui lòng chọn loại sự kiện.", "loi"); return; }
  if (!checkThoiGian(bd, kt)) return;
  if (sl === null || isNaN(sl) || sl <= 0) { toast("Số người dự kiến phải lớn hơn 0.", "loi"); return; }
  try {
    var kq = await apiFetch("/api/events", "POST", {
      Ten: ten,
      MoTa: val("sk-mota") || null,
      BatDau: bd || null,
      KetThuc: kt || null,
      SoNguoi: sl,
      LoaiSuKienId: loai
    });
    thongBao("Đã tạo sự kiện và gửi chờ duyệt.");
    location.assign("/pages/ban-to-chuc/su-kien-cua-toi.html");
  } catch (e) { thongBao(e.message, "loi"); }
}

async function duyetSuKien(id) {
  try {
    await apiFetch("/api/events/" + id + "/approve", "POST", null);
    thongBao("Đã duyệt sự kiện.");
    await taiDanhSachSuKien(true);
  } catch (e) { thongBao(e.message, "loi"); }
}

async function tuChoiSuKien(id) {
  var lydo = prompt("Nhập lý do từ chối sự kiện:");
  if (lydo === null) return;
  if (!lydo.trim()) { toast("Vui lòng nhập lý do từ chối.", "loi"); return; }
  try {
    await apiFetch("/api/events/" + id + "/reject", "POST", { LyDo: lydo.trim() });
    thongBao("Đã từ chối sự kiện.");
    await taiDanhSachSuKien(true);
  } catch (e) { thongBao(e.message, "loi"); }
}

async function huySuKien(id) {
  if (!confirm("Bạn có chắc muốn hủy sự kiện này?")) return;
  try {
    await apiFetch("/api/events/" + id + "/cancel", "POST", null);
    thongBao("Đã hủy sự kiện.");
    await taiDanhSachSuKien(true);
  } catch (e) { thongBao(e.message, "loi"); }
}

var __loaiSuKienCache = [];
async function taiLoaiSuKien() {
  skeleton("ds-loai", 3);
  try {
    var isAdminList = !!document.getElementById("ds-loai") && store.vaiTro === "QuanTriVien";
    var kq = await apiFetch("/api/event-types" + (isAdminList ? "/all" : ""));
    __loaiSuKienCache = kq.data || [];
    document.querySelectorAll("[data-event-categories]").forEach(function (host) {
      host.replaceChildren();
      [{ Ten: "", Label: "Tất cả" }].concat(__loaiSuKienCache).forEach(function (item) {
        var button = document.createElement("button");
        button.className = "catchip" + (item.Ten === __suKienLoai ? " active" : "");
        button.textContent = item.Label || item.Ten;
        button.onclick = function () {
          host.querySelectorAll("button").forEach(function (node) { node.classList.remove("active"); });
          button.classList.add("active"); locLoaiSuKien(item.Ten);
        };
        host.appendChild(button);
      });
    });
    var sel = document.getElementById("sk-loai");
    if (sel) {
      sel.innerHTML = (kq.data || []).map(function (l) {
        return '<option value="' + l.Id + '">' + esc(l.Ten) + "</option>";
      }).join("");
    }
    var host = document.getElementById("ds-loai");
    if (host) {
      host.innerHTML = bangHTML(
        [["Ten", "Ten loai"], ["MoTa", "Mo ta"], ["HoatDong", "Hoat dong"]],
        (kq.data || []).map(function (d) {
          return { Ten: d.Ten, MoTa: d.MoTa, HoatDong: d.HoatDong ? "Dang hoat dong" : "Da tat", Id: d.Id };
        }),
        function (d) {
          return '<button class="phu sm" onclick="suaLoaiSuKien(' + d.Id + ')">Sửa</button> <button class="phu sm" onclick="xoaLoaiSuKien(' + d.Id + ')">Xóa</button> <button class="phu sm" onclick="doiTrangThaiLoai(' + d.Id + ",1)\">Bat</button> " +
            '<button class="phu sm" onclick="doiTrangThaiLoai(' + d.Id + ",0)\">Tat</button>";
        });
    }
  } catch (e) { thongBao(e.message, "loi"); }
}

function suaLoaiSuKien(id) {
  var item = __loaiSuKienCache.find(function (x) { return x.Id === id; });
  [["id", item.Id], ["ten", item.Ten], ["mota", item.MoTa]].forEach(function (p) { document.getElementById("lsk-" + p[0]).value = p[1] || ""; });
}
async function xoaLoaiSuKien(id) {
  if (!confirm("Xóa loại chưa được sự kiện nào sử dụng?")) return;
  try { await apiFetch("/api/admin/catalog/event-types/" + id, "DELETE"); await taiLoaiSuKien(); }
  catch (error) { thongBao(error.message, "loi"); }
}

async function themLoaiSuKien() {
  var ten = val("lsk-ten");
  if (!ten) { toast("Nhap ten loai su kien.", "loi"); return; }
  try {
    await apiFetch("/api/event-types", "POST", { Ten: ten, MoTa: val("lsk-mota") || null });
    thongBao("Them loai su kien thanh cong.");
    taiLoaiSuKien();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function capNhatLoaiSuKien() {
  var id = num("lsk-id");
  if (!id || id <= 0) { toast("Nhập mã loai can cap nhat.", "loi"); return; }
  var ten = val("lsk-ten");
  if (!ten) { toast("Nhap ten loai su kien.", "loi"); return; }
  try {
    await apiFetch("/api/event-types/" + id, "PUT", { Ten: ten, MoTa: val("lsk-mota") || null });
    thongBao("Cap nhat loai thanh cong.");
    taiLoaiSuKien();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function doiTrangThaiLoai(id, trangThai) {
  var category = __loaiSuKienCache.find(function (item) { return Number(item.Id) === Number(id); });
  if (!category) return;
  try {
    await apiFetch("/api/event-types/" + id, "PUT", { Ten: category.Ten, MoTa: category.MoTa || "", HoatDong: !!trangThai });
    thongBao("Đã cập nhật trạng thái loại sự kiện.");
    taiLoaiSuKien();
  } catch (e) { thongBao(e.message, "loi"); }
}
