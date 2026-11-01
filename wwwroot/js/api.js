/* ===== api.js — lop goi API dung chung, toast, spinner, helper ===== */
"use strict";
/* global API_BASE */
var store = {
  get token() { try { return sessionStorage.getItem("sk_token") || ""; } catch (e) { return ""; } },
  get vaiTro() { try { return sessionStorage.getItem("sk_vaitro") || ""; } catch (e) { return ""; } },
  get hoTen() { try { return sessionStorage.getItem("sk_hoten") || ""; } catch (e) { return ""; } },
  get taiKhoanID() { try { return sessionStorage.getItem("sk_tkid") || ""; } catch (e) { return ""; } }
};

var __dangTai = 0;

function noiDungGiaoDien(value) {
  return String(value === null || value === undefined ? "" : value);
}

function chuanHoaPhanHoi(value) {
  if (Array.isArray(value)) return value.map(chuanHoaPhanHoi);
  if (!value || typeof value !== "object") return value;
  var result = {};
  Object.keys(value).forEach(function (key) {
    var item = chuanHoaPhanHoi(value[key]);
    result[key] = item;
    var upper = key.charAt(0).toUpperCase() + key.slice(1);
    if (!(upper in value)) result[upper] = item;
  });
  return result;
}

function thongDiepNguoiDung(message) {
  var text = String(message || "");
  if (/failed to fetch|networkerror|network error|load failed/i.test(text))
    return "Không thể kết nối. Vui lòng kiểm tra kết nối mạng rồi thử lại.";
  if (/jwt|token|json|payload|sql|database|exception|HTTP|backend|frontend|\bAPI\b/i.test(text))
    return "Không thể hoàn tất yêu cầu. Vui lòng tải lại trang rồi thử lại.";
  return noiDungGiaoDien(text);
}
function batSpinner() {
  __dangTai++;
  var s = document.getElementById("spinner");
  if (s) s.classList.add("hien");
}
function tatSpinner() {
  __dangTai = Math.max(0, __dangTai - 1);
  if (__dangTai === 0) {
    var s = document.getElementById("spinner");
    if (s) s.classList.remove("hien");
  }
}

async function apiFetch(path, method, body) {
  method = method || "GET";
  if (!path.startsWith("/api/")) path = "/api" + (path.startsWith("/") ? "" : "/") + path;
  var headers = { "Content-Type": "application/json" };
  if (store.token) headers["Authorization"] = "Bearer " + store.token;
  batSpinner();
  var controller = new AbortController();
  var timeout = setTimeout(function () { controller.abort(); }, 25000);
  try {
    var res = await fetch(API_BASE + path, {
      method: method,
      cache: "no-store",
      headers: headers,
      signal: controller.signal,
      body: body ? JSON.stringify(body) : null
    });
    var data = {};
    try { data = await res.json(); } catch (e) { data = {}; }
    if (res.status === 401 || res.status === 403) {
      var code = (data && data.code) || "";
      var msg = res.status === 401 ? "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại." : "Tài khoản chưa có quyền thực hiện thao tác này.";
      if (path === "/api/auth/login") msg = "Email hoặc mật khẩu chưa đúng. Vui lòng kiểm tra lại.";
      if (res.status === 401 || code === "TOKEN_STALE") {
        sessionStorage.clear();
        if (typeof hienTrang === "function") {
          try { hienTrang("dangnhap"); } catch (e2) {}
        }
        var loi401 = new Error(msg || "Phien het han, vui long dang nhap lai.");
        loi401.ma = res.status;
        throw loi401;
      }
      var loi403 = new Error(msg || "Khong co quyen thuc hien.");
      loi403.ma = res.status;
      throw loi403;
    }
    if (!res.ok) throw new Error(res.status >= 500 ? "Dịch vụ đang bận. Vui lòng thử lại sau." : ((data && data.message) || "Không thể hoàn tất yêu cầu. Vui lòng kiểm tra thông tin và thử lại."));
    return chuanHoaPhanHoi(data);
  } catch (error) {
    if (error.name === "AbortError") throw new Error("Kết nối mất nhiều thời gian. Vui lòng thử lại.");
    error.message = thongDiepNguoiDung(error.message);
    throw error;
  } finally {
    clearTimeout(timeout);
    tatSpinner();
  }
}

function esc(v) {
  return noiDungGiaoDien(v)
    .replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;").replace(/'/g, "&#39;");
}
function td(v) { return (v === null || v === undefined) ? "" : v; }
function ngayDuLieu(v) {
  if (typeof v === "string" && /^\d{4}-\d\d-\d\dT\d\d:\d\d/.test(v) && !/(Z|[+-]\d\d:\d\d)$/i.test(v)) v += "Z";
  return new Date(v);
}
function isoLocal(v) { if (!v) return ""; try { return ngayDuLieu(v).toLocaleString("vi-VN"); } catch (e) { return String(v); } }
function fmtTien(v) {
  if (v === null || v === undefined || v === "") return "";
  var n = Number(v);
  if (isNaN(n)) return String(v);
  return n.toLocaleString("vi-VN");
}
function val(id) {
  var el = document.getElementById(id);
  return el ? el.value.trim() : "";
}
function num(id) {
  var s = val(id);
  if (s === "") return null;
  var n = Number(s);
  return isNaN(n) ? NaN : n;
}
function setErr(id, msg) {
  var el = document.getElementById(id);
  if (el) el.classList.toggle("err", !!msg);
  var f = document.getElementById(id + "-err");
  if (f) f.textContent = msg || "";
  return !msg;
}

/* thong bao doi tuong thich: toast + dong tb-chung */
function thongBao(msg, loai, id) {
  msg = thongDiepNguoiDung(msg);
  toast(msg, loai === "loi" || loai === "err" ? "loi" : "ok");
  var el = document.getElementById(id || "tb-chung");
  if (el) {
    el.className = "thongbao " + (loai === "loi" ? "loi" : "ok");
    el.textContent = msg;
    el.style.display = "block";
    clearTimeout(el.__t);
    el.__t = setTimeout(function () { el.style.display = "none"; }, 6000);
  }
}

function toast(msg, loai) {
  msg = thongDiepNguoiDung(msg);
  loai = loai || "ok";
  var wrap = document.getElementById("toast-wrap");
  if (!wrap) { alert(msg); return; }
  var t = document.createElement("div");
  t.className = "toast " + loai;
  var icon = loai === "loi" || loai === "err" ? "⚠" : (loai === "info" ? "ℹ" : "✔");
  t.innerHTML = "<span>" + esc(icon) + "</span><span></span><button class=\"x\" aria-label=\"Dong\">&times;</button>";
  t.children[1].textContent = msg;
  t.querySelector(".x").onclick = function () { t.remove(); };
  wrap.appendChild(t);
  setTimeout(function () {
    t.style.transition = "opacity .4s,transform .4s";
    t.style.opacity = "0";
    t.style.transform = "translateX(40px)";
    setTimeout(function () { t.remove(); }, 420);
  }, 4200);
}

function bangHTML(cot, dong, nutFn) {
  if (!dong || !dong.length)
    return '<div class="empty">' +
      '<svg viewBox="0 0 120 90" fill="none" aria-hidden="true">' +
      '<rect x="14" y="18" width="92" height="58" rx="10" fill="#dbeafe"/>' +
      '<rect x="24" y="30" width="50" height="8" rx="4" fill="#93c5fd"/>' +
      '<rect x="24" y="43" width="72" height="6" rx="3" fill="#bfdbfe"/>' +
      '<rect x="24" y="53" width="60" height="6" rx="3" fill="#bfdbfe"/>' +
      '<circle cx="92" cy="62" r="14" fill="#c4b5fd"/></svg>' +
      '<div><b>Chưa có thông tin</b></div><div class="muted">Thông tin sẽ xuất hiện tại đây khi có cập nhật.</div></div>';
  var h = '<div class="tablescroll" tabindex="0" role="region" aria-label="Bảng thông tin, cuộn ngang để xem thêm"><table><thead><tr>' +
    cot.map(function (c) { return "<th>" + esc(c[1]) + "</th>"; }).join("") +
    (nutFn ? "<th>Thao tác</th>" : "") + "</tr></thead><tbody>";
  dong.forEach(function (d) {
    h += "<tr>" + cot.map(function (c) {
      var v = d[c[0]];
      if (c[2] === "tien") v = fmtTien(v);
      else if (c[2] === "ngay") v = isoLocal(v);
      else v = td(v);
      if (/trangthai|vaitro/i.test(c[0])) v = nhanTrangThai(v);
      return "<td>" + esc(v) + "</td>";
    }).join("");
    if (nutFn) { try { h += "<td>" + nutFn(d) + "</td>"; } catch (e) { h += "<td></td>"; } }
    h += "</tr>";
  });
  return h + "</tbody></table></div>";
}

function nhanTrangThai(v) {
  var labels = { ChoDuyet: "Chờ duyệt", DaDuyet: "Đã duyệt", TuChoi: "Từ chối", DangDienRa: "Đang diễn ra", DaHuy: "Đã hủy", DaKetThuc: "Đã kết thúc", ChoThanhToan: "Chờ thanh toán", ThanhCong: "Thành công", ThatBai: "Thất bại", HetHan: "Hết hạn", DaXacNhan: "Đã xác nhận", ChoXacNhan: "Chờ xác nhận", HoanThanh: "Hoàn thành", HoanTien: "Hoàn tiền", YeuCauHoanTien: "Yêu cầu hoàn tiền", DaHoanTien: "Đã hoàn tiền", DangXuLy: "Đang xử lý", NghiVan: "Cần kiểm tra", ChuaSuDung: "Chưa sử dụng", DaSuDung: "Đã sử dụng", DaCheckIn: "Đã check-in", NguoiDung: "Người dùng", BanToChuc: "Ban tổ chức", QuanLySuKien: "Quản lý sự kiện", QuanTriVien: "Quản trị viên", "Hoat dong": "Hoạt động", "Dang hoat dong": "Đang hoạt động", "Da tat": "Đã tắt", "Da khoa": "Đã khóa" };
  var text = String(v === null || v === undefined ? "" : v);
  return labels[text] || text || "—";
}
function badgeTT(v) {
  var s = String(v === null || v === undefined ? "" : v);
  var cls = "b-muted";
  if (/thanhcong|hoatdong|daduyet|daxacnhan|daxuat|damua|dung|ok/i.test(s.replace(/\s/g, ""))) cls = "b-ok";
  else if (/thatbai|khoa|tuchoi|dahuy|het|that/i.test(s.toLowerCase().replace(/\s/g, ""))) cls = "b-err";
  else if (/choduyet|cho|moiban|dangcho|pending/i.test(s.toLowerCase().replace(/\s/g, ""))) cls = "b-warn";
  else if (s) cls = "b-info";
  return '<span class="badge ' + cls + '">' + esc(nhanTrangThai(s)) + "</span>";
}

function skeleton(elId, rows) {
  var el = document.getElementById(elId);
  if (!el) return;
  rows = rows || 3;
  var h = '<div class="skel-wrap">';
  for (var i = 0; i < rows; i++) h += '<div class="shimmer-line" style="width:' + (92 - i * 9) + '%"></div>';
  h += "</div>";
  el.innerHTML = h;
}

/* Confetti CSS/SVG thuan — khong anh ngoai, chay offline */
function banConfetti(n) {
  n = n || 80;
  var mau = ["#5b21f2", "#2563eb", "#06b6d4", "#ec4899", "#f59e0b", "#22c55e"];
  for (var i = 0; i < n; i++) {
    (function () {
      var c = document.createElement("div");
      c.className = "confetti";
      var w = 6 + Math.random() * 7;
      c.style.left = (Math.random() * 100) + "vw";
      c.style.width = w + "px";
      c.style.height = (w * (Math.random() > 0.5 ? 1 : 0.45)) + "px";
      c.style.background = mau[Math.floor(Math.random() * mau.length)];
      c.style.borderRadius = Math.random() > 0.5 ? "50%" : "2px";
      c.style.animationDuration = (2.4 + Math.random() * 2.2) + "s";
      document.body.appendChild(c);
      setTimeout(function () { c.remove(); }, 5200);
    })();
  }
}

/* Validate dung chung theo hop dong backend */
function checkMatKhau(mk, nhapLai, idNhapLai) {
  if (!mk || mk.length < 8) { toast("Mật khẩu cần tối thiểu 8 ký tự.", "loi"); return false; }
  if (nhapLai !== undefined && mk !== nhapLai) {
    if (idNhapLai) setErr(idNhapLai, "Mật khẩu nhập lại không khớp.");
    toast("Mật khẩu nhập lại không khớp.", "loi");
    return false;
  }
  if (idNhapLai) setErr(idNhapLai, "");
  return true;
}
function checkThoiGian(bd, kt) {
  if (!bd || !kt) { toast("Vui lòng chọn thời gian bắt đầu và kết thúc.", "loi"); return false; }
  var a = new Date(bd), b = new Date(kt);
  if (isNaN(a) || isNaN(b) || b <= a) { toast("Thời gian kết thúc phải sau thời gian bắt đầu.", "loi"); return false; }
  return true;
}
function checkSoKhongAm(v, ten) {
  if (v === null || v === undefined || v === "" || isNaN(Number(v)) || Number(v) < 0) {
    toast((ten || "Giá trị") + " phải là số không âm.", "loi");
    return false;
  }
  return true;
}
