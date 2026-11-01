/* ===== diadiem.js — dia diem + dat dia diem ===== */
"use strict";
/* global apiFetch, toast, thongBao, val, num, bangHTML, badgeTT, skeleton, esc, fmtTien, checkThoiGian, checkSoKhongAm */

var __diaDiemCache = [];
var __lanTaiDiaDiem = 0;
function anhDiaDiemHTML(d) {
  var fallback = "/assets/images/placeholder-venue.svg";
  var source = d.HinhAnh || fallback;
  try { var url = new URL(source, location.href); if (!/^https?:$/.test(url.protocol)) source = fallback; } catch (error) { source = fallback; }
  return '<div class="venue-photo"><img src="' + esc(source) + '" alt="' + esc("Không gian tổ chức sự kiện tại " + d.Ten) +
    '" width="960" height="540" loading="lazy" decoding="async" onerror="this.onerror=null;this.src=\'' + fallback + '\'"></div>';
}
function lamMoiDiaDiem() {
  var field = document.getElementById("dd-tukhoa");
  if (field) field.value = "";
  return taiDiaDiem();
}
function chuanHoaDiaDiem(d) {
  return {
    Id: d.Id ?? d.id,
    Ten: d.Ten ?? d.ten ?? "Địa điểm đang cập nhật",
    HinhAnh: d.HinhAnh ?? d.hinhAnh,
    DiaChi: d.DiaChi ?? d.diaChi ?? "Địa chỉ đang cập nhật",
    SucChua: d.SucChua ?? d.sucChua,
    GiaThue: d.GiaThue ?? d.giaThue,
    HoatDong: d.HoatDong ?? d.hoatDong ?? false
  };
}
async function taiDiaDiem() {
  var host = document.getElementById("ds-diadiem");
  if (!host) return;
  var requestNumber = ++__lanTaiDiaDiem;
  host.setAttribute("aria-busy", "true");
  var count = document.getElementById("venue-result-count");
  if (count) count.textContent = "Đang tải...";
  skeleton("ds-diadiem", 4);
  try {
    var tk = val("dd-tukhoa");
    var admin = !!store.token && store.vaiTro === "QuanTriVien";
    var kq = await apiFetch(admin ? "/api/venues/all" : "/api/venues");
    if (requestNumber !== __lanTaiDiaDiem) return;
    var data = kq.data ?? kq.Data;
    if (!Array.isArray(data)) throw new Error("Chưa thể tải danh sách địa điểm. Vui lòng thử lại.");
    __diaDiemCache = data.map(chuanHoaDiaDiem);
    var keyword = chuoiTimKiem(tk);
    var rows = __diaDiemCache.filter(function (d) { return !keyword || chuoiTimKiem(d.Ten + " " + d.DiaChi).indexOf(keyword) !== -1; });
    if (count) count.textContent = rows.length + " địa điểm";
    if (!rows.length) { host.innerHTML = '<div class="empty"><h3>Chưa tìm thấy địa điểm phù hợp</h3><p>Thử tìm theo tên địa điểm hoặc thành phố khác.</p></div>'; return; }
    host.innerHTML = '<div class="venue-grid">' + rows.map(function (d) {
      var capacity = Number.isFinite(Number(d.SucChua)) && d.SucChua != null ? fmtTien(d.SucChua) + " người" : "Đang cập nhật";
      var price = Number.isFinite(Number(d.GiaThue)) && d.GiaThue != null ? fmtTien(d.GiaThue) + " đ" : "Liên hệ";
      return '<article class="card venue-card">' +
        anhDiaDiemHTML(d) + '<div class="venue-body">' +
        '<div class="venue-heading"><h3>' + esc(d.Ten) + '</h3><span class="badge ' + (d.HoatDong ? 'b-ok' : 'b-muted') + '">' + (d.HoatDong ? "Đang hoạt động" : "Tạm ngừng") + '</span></div>' +
        '<p class="muted venue-address">' + esc(d.DiaChi) + '</p>' +
        '<dl class="venue-facts"><div><dt>Sức chứa</dt><dd>' + esc(capacity) + '</dd></div><div><dt>Giá thuê</dt><dd>' + esc(price) + '</dd></div></dl>' +
        (admin ? '<div class="toolbar" data-quyen="QuanTriVien"><span class="muted small">Mã địa điểm: ' + esc(d.Id) + '</span><button class="phu sm" onclick="suaDiaDiem(' + Number(d.Id) + ')">Sửa</button><button class="phu sm" onclick="xoaDiaDiem(' + Number(d.Id) + ')">Xóa</button><button class="phu sm" onclick="doiTrangThaiDiaDiem(' + Number(d.Id) + ',1)">Bật</button><button class="phu sm" onclick="doiTrangThaiDiaDiem(' + Number(d.Id) + ',0)">Tắt</button></div>' : '') +
        "</div></article>";
    }).join("") + "</div>";
    apDungQuyen();
  } catch (e) {
    if (requestNumber !== __lanTaiDiaDiem) return;
    if (count) count.textContent = "";
    host.innerHTML = '<div class="empty"><h3>Chưa thể tải địa điểm</h3><p>' + esc(e.message) + '</p><button class="phu" onclick="taiDiaDiem()">Thử lại</button></div>';
  } finally { if (requestNumber === __lanTaiDiaDiem) host.setAttribute("aria-busy", "false"); }
}

async function themDiaDiem() {
  var ten = val("dd-ten"), dc = val("dd-dc");
  var sc = num("dd-sc"), gia = num("dd-gia");
  if (!ten || !dc) { toast("Vui lòng nhập tên địa điểm và địa chỉ.", "loi"); return; }
  if (sc === null || isNaN(sc) || sc <= 0) { toast("Sức chứa phải lớn hơn 0.", "loi"); return; }
  if (!checkSoKhongAm(val("dd-gia"), "Giá thuê")) return;
  try {
    await apiFetch("/api/venues", "POST", {
      Ten: ten, DiaChi: dc, SucChua: sc, GiaThue: gia, HinhAnh: val("dd-anh") || null, HoatDong: true
    });
    thongBao("Đã thêm địa điểm.");
    taiDiaDiem();
  } catch (e) { thongBao(e.message, "loi"); }
}

function suaDiaDiem(id) {
  if (!document.getElementById("dd-id")) { location.assign("/pages/quan-tri/dia-diem.html"); return; }
  var item = __diaDiemCache.find(function (x) { return x.Id === id; });
  [["id", item.Id], ["ten", item.Ten], ["dc", item.DiaChi], ["sc", item.SucChua], ["gia", item.GiaThue], ["anh", item.HinhAnh]].forEach(function (p) {
    var field = document.getElementById("dd-" + p[0]); if (field) field.value = p[1] == null ? "" : p[1];
  });
  document.getElementById("dd-ten").focus();
}
async function xoaDiaDiem(id) {
  if (!confirm("Xóa địa điểm chưa có lịch đặt?")) return;
  try { await apiFetch("/api/admin/catalog/venues/" + id, "DELETE"); await taiDiaDiem(); }
  catch (error) { thongBao(error.message, "loi"); }
}

async function capNhatDiaDiem() {
  var id = num("dd-id");
  if (!id || id <= 0) { toast("Nhập mã địa điểm cần cập nhật.", "loi"); return; }
  var ten = val("dd-ten"), dc = val("dd-dc");
  if (!ten || !dc) { toast("Vui lòng nhập tên địa điểm và địa chỉ.", "loi"); return; }
  var sc = num("dd-sc");
  if (sc === null || isNaN(sc) || sc <= 0) { toast("Sức chứa phải lớn hơn 0.", "loi"); return; }
  if (!checkSoKhongAm(val("dd-gia"), "Giá thuê")) return;
  try {
    await apiFetch("/api/venues/" + id, "PUT", {
      Ten: ten, DiaChi: dc, SucChua: sc, GiaThue: num("dd-gia"), HinhAnh: val("dd-anh") || null, HoatDong: true
    });
    thongBao("Đã cập nhật địa điểm.");
    taiDiaDiem();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function doiTrangThaiDiaDiem(id, trangThai) {
  var venue = __diaDiemCache.find(function (item) { return Number(item.Id) === Number(id); });
  if (!venue) return;
  try {
    await apiFetch("/api/venues/" + id, "PUT", { Ten: venue.Ten, DiaChi: venue.DiaChi, SucChua: venue.SucChua, GiaThue: venue.GiaThue, HinhAnh: venue.HinhAnh, HoatDong: !!trangThai });
    thongBao("Đã cập nhật trạng thái địa điểm.");
    taiDiaDiem();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function datDiaDiem() {
  var sk = num("dat-sk"), dd = num("dat-dd");
  var bd = val("dat-bd"), kt = val("dat-kt");
  var tc = num("dat-tc");
  if (!sk || sk <= 0 || !dd || dd <= 0) { toast("Nhap su kien va dia diem hop le.", "loi"); return; }
  if (!checkThoiGian(bd, kt)) return;
  if (tc === null || isNaN(tc) || tc < 0) { toast("Tien coc phải là số không âm.", "loi"); return; }
  try {
    await apiFetch("/api/venue-bookings", "POST", {
      DiaDiemId: dd, SuKienId: sk, BatDau: bd || null, KetThuc: kt || null, TienCoc: tc || 0
    });
    thongBao("Dat dia diem thanh cong.");
    taiDatDiaDiem();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function taiDatDiaDiem() {
  skeleton("ds-dat", 3);
  try {
    var kq = await apiFetch("/api/venue-bookings");
    document.getElementById("ds-dat").innerHTML = bangHTML(
      [["TenSuKien", "Su kien"], ["TenDiaDiem", "Dia diem"], ["TrangThai", "Trang thai"],
       ["TongTien", "Tong tien", "tien"], ["TienCoc", "Coc", "tien"]],
      (kq.data || []).map(function (d) {
        return {
          Id: d.Id,
          TenSuKien: (d.SuKien && d.SuKien.Ten) ? d.SuKien.Ten : "",
          TenDiaDiem: (d.DiaDiem && d.DiaDiem.Ten) ? d.DiaDiem.Ten : "",
          TrangThai: d.TrangThai,
          TongTien: d.TongTien,
          TienCoc: d.TienCoc
        };
      }),
      function (d) {
        return '<button class="phu sm" onclick="xacNhanDat(' + d.Id + ')">Xac nhan</button> ' +
          '<button class="phu sm" onclick="tuChoiDat(' + d.Id + ')">Tu choi</button> ' +
          '<button class="phu sm" onclick="huyDat(' + d.Id + ')">Huy</button>';
      });
  } catch (e) { thongBao(e.message, "loi"); }
}

async function xacNhanDat(id) {
  try {
    await apiFetch("/api/venue-bookings/" + id + "/approve", "POST", null);
    thongBao("Da xac nhan dat dia diem #" + id + ".");
    taiDatDiaDiem();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function tuChoiDat(id) {
  try {
    await apiFetch("/api/venue-bookings/" + id + "/reject", "POST", null);
    thongBao("Da tu choi dat dia diem #" + id + ".");
    taiDatDiaDiem();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function huyDat(id) {
  try {
    await apiFetch("/api/venue-bookings/" + id + "/cancel", "POST", null);
    thongBao("Da huy dat dia diem #" + id + ".");
    taiDatDiaDiem();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function taiThongKeDatDiaDiem() {
  try {
    var kq = await apiFetch("/api/venue-bookings");
    var h1 = document.getElementById("tk-dat-tt"), h2 = document.getElementById("tk-dat-dd");
    var rows = kq.data || [];
    if (h1) h1.innerHTML = bangHTML([["TrangThai", "Trạng thái"], ["SoLuong", "Số lượng"]], groupCount(rows, function (x) { return ["ChoXacNhan", "DaXacNhan", "TuChoi", "DaHuy", "HoanThanh"][x.TrangThai] || x.TrangThai; }));
    var venues = new Map();
    rows.forEach(function (x) {
      if (!venues.has(x.DiaDiemId)) venues.set(x.DiaDiemId, { TenDiaDiem: x.DiaDiem ? x.DiaDiem.Ten : String(x.DiaDiemId), SoLuotDat: 0 });
      venues.get(x.DiaDiemId).SoLuotDat++;
    });
    if (h2) h2.innerHTML = bangHTML([["TenDiaDiem", "Địa điểm"], ["SoLuotDat", "Lượt đặt"]], Array.from(venues.values()));
  } catch (e) { thongBao(e.message, "loi"); }
}
