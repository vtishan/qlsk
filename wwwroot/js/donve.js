/* ===== donve.js — loai ve + don ve + thanh toan + ve ===== */
"use strict";
/* global apiFetch, toast, thongBao, val, num, bangHTML, badgeTT, skeleton, esc, isoLocal, fmtTien, banConfetti, checkThoiGian, checkSoKhongAm */

/* ---- Loai ve (lay theo su kien) ---- */
async function taiLoaiVeTheoSuKien(suKienID) {
  var id = suKienID || num("lv-xem-sk");
  if (!id || id <= 0) { toast("Nhap su kien de xem hang ve.", "loi"); return; }
  var hostId = suKienID ? "ds-loaive-sk" : "ds-loaive";
  var host = document.getElementById(hostId) || document.getElementById("ds-loaive-sk") || document.getElementById("ds-loaive");
  if (host) skeleton(host.id, 3);
  try {
    var kq = await apiFetch("/api/events/" + id + "/ticket-types");
    var rows = kq.data || [];
    var html;
    if (!rows.length) { html = bangHTML([], []); }
    else {
      html = '<div class="grid g3">' + rows.map(function (d) {
        return '<article class="card hoverable anim-pop">' +
          '<div class="toolbar" style="margin:0 0 8px"><span class="badge b-info">#' + d.Id + "</span>" +
          '<span style="margin-left:auto"><b>' + esc(fmtTien(d.Gia)) + " VND</b></span></div>" +
          "<h3>" + esc(d.Ten) + "</h3>" +
          '<div class="muted">Toi da: ' + esc(d.SoLuong) + " &middot; Moi don: 20</div>" +
          '<div class="muted">' + esc(isoLocal(d.MoBan)) + " &rarr; " + esc(isoLocal(d.DongBan)) + "</div>" +
          '<div class="toolbar"><button class="chinh sm" onclick="chonLoaiVe(' + d.Id + ')">Chon mua</button></div>' +
          "</article>";
      }).join("") + "</div>";
    }
    if (host) {
      host.innerHTML = "<h3>Hang ve cua su kien</h3>" + html;
      host.scrollIntoView({ behavior: "smooth", block: "nearest" });
    }
  } catch (e) { thongBao(e.message, "loi"); }
}

function chonLoaiVe(loaiVeID) {
  var el = document.getElementById("don-loai");
  if (el) el.value = loaiVeID;
  if (typeof hienTrang === "function") hienTrang("donve");
  toast("Da chon hang ve — nhap so luong roi Tao don.", "info");
}

/* ---- Don ve ---- */
async function taoDon() {
  var loai = num("don-loai"), sl = num("don-sl");
  if (!loai || loai <= 0) { toast("Nhap hang ve hop le.", "loi"); return; }
  if (sl === null || isNaN(sl) || sl <= 0) { toast("So luong ve phai > 0.", "loi"); return; }
  try {
    var kq = await apiFetch("/api/orders", "POST", { Items: [{ LoaiVeId: loai, SoLuong: sl }] });
    var don = kq.data || kq;
    var ma = don.MaDonHang || ("DH-" + don.Id);
    thongBao("Tao don thanh cong. Ma don: " + ma);
    banConfetti(90);
    taiDonCuaToi();
  } catch (e) { thongBao(e.message, "loi"); }
}

var __dsDonCache = [];

function _phanLoaiDon(d) {
  var don = String((d && d.TrangThai) || "");
  var tt = String((d && d.TrangThaiThanhToan) || "");
  if (/DaHuy|BiHetHan/i.test(don)) return "huy";
  if (/DaHoanTien|HoanTienMotPhan|YeuCauHoanTien/i.test(don) || /DaHoanTien|HoanTienMotPhan/i.test(tt)) return "refund";
  if (/ThanhCong/i.test(don) || /DaThanhToan/i.test(tt)) return "paid";
  return "cho";
}

function _nhanDon(d) {
  var loai = _phanLoaiDon(d);
  if (loai === "cho") return "Cho thanh toan";
  if (loai === "paid") return "Da thanh toan";
  if (loai === "refund") return "Hoan tien";
  if (loai === "huy") return "Da huy / Het han";
  return String(d.TrangThai || d.TrangThaiThanhToan || "—");
}

function capNhatThongKeDon(rows) {
  var tong = rows.length, cho = 0, paid = 0, refund = 0;
  rows.forEach(function (d) {
    var l = _phanLoaiDon(d);
    if (l === "cho") cho++;
    else if (l === "paid") paid++;
    else if (l === "refund" || l === "huy") refund++;
  });
  var g = function (id, v) { var el = document.getElementById(id); if (el) el.textContent = v; };
  g("qs-tong", tong);
  g("qs-chothanhtoan", cho);
  g("qs-dathanhtoan", paid);
  g("qs-hoantien", refund);
}

function locNhanhDon(loai) {
  var sel = document.getElementById("loc-trangthai");
  if (sel) sel.value = loai || "all";
  try {
    Array.prototype.forEach.call(document.querySelectorAll(".hero-tags .catchip"), function (c, i) {
      var t = (c.textContent || "").toLowerCase();
      var active = (i === 0 && (loai === "all" || !loai)) ||
        (t.indexOf("chờ") !== -1 && loai === "cho") ||
        (t.indexOf("đã thanh") !== -1 && loai === "paid");
      c.classList.toggle("active", !!active);
    });
  } catch (e) {}
  veDonList();
}

function _cardDon(d) {
  var loai = _phanLoaiDon(d);
  var cls = loai === "cho" ? "is-pending" : loai === "paid" ? "is-paid" : loai === "refund" ? "is-refund" : "is-cancelled";
  var s1 = "done", s2 = "now", s3 = "off", t2 = "Thanh toán", t3 = "Nhận vé";
  if (loai === "paid" || loai === "refund") { s1 = "done"; s2 = "done"; s3 = "done"; }
  else if (loai === "huy") { s1 = "done"; s2 = "off"; s3 = "off"; t2 = "Đã đóng"; t3 = "Không có vé"; }
  else if (loai === "cho") { s1 = "done"; s2 = "now"; s3 = "off"; }
  var hetHan = "";
  try {
    if (loai === "cho" && d.HetHan) {
      hetHan = '<div class="muted small">Giữ chỗ đến: <b>' + esc(isoLocal(d.HetHan)) + "</b></div>";
    }
  } catch (e) {}
  var ngayDat = "";
  try { if (d.NgayTao) ngayDat = esc(isoLocal(d.NgayTao)); } catch (e) {}
  var tong = Number(d.TongTien || 0);
  var isCho = loai === "cho";
  var isPaid = loai === "paid" || loai === "refund";
  var h = '<article class="card hoverable anim-pop don-card ' + cls + '">' +
    '<div class="don-top"><span class="don-code">' + esc(d.MaDonHang || ("DH-" + d.Id)) + "</span>" +
    '<span class="don-badges">' + badgeTT(d.TrangThai) + " " + badgeTT(d.TrangThaiThanhToan) + "</span></div>" +
    '<div class="muted small">Đơn #' + esc(d.Id) + (ngayDat ? " &middot; Đặt lúc " + ngayDat : "") + " &middot; " + esc(_nhanDon(d)) + "</div>" +
    hetHan +
    '<div class="don-price-row"><span class="muted small">Tổng thanh toán</span><span class="don-price">' + esc(fmtTien(tong)) + "đ</span></div>" +
    '<div class="don-steps">' +
    '<div class="don-step ' + s1 + '"><div class="dot">1</div>Đặt đơn</div>' +
    '<div class="don-step ' + s2 + '"><div class="dot">2</div>' + t2 + "</div>" +
    '<div class="don-step ' + s3 + '"><div class="dot">3</div>' + t3 + "</div></div>" +
    '<div class="don-actions">' +
    '<button class="phu sm" onclick="xemChiTietDon(' + d.Id + ')">Chi tiết</button> ';
  if (isCho) {
    h += '<button class="chinh sm" onclick="thanhToanDon(' + d.Id + ',' + tong + ')">Thanh toán</button> ' +
      '<button class="phu sm btn-huy" onclick="huyDon(' + d.Id + ')">Hủy đơn</button>';
  }
  if (isPaid) {
    h += '<button class="chinh sm" onclick="phatHanhVe(' + d.Id + ')">Phát hành vé</button>';
  }
  if (!isCho && !isPaid) {
    h += '<button class="phu sm" onclick="xemChiTietDon(' + d.Id + ')">Xem lại</button>';
  }
  h += "</div></article>";
  return h;
}

function veDonList() {
  var host = document.getElementById("ds-don");
  if (!host) return;
  var rows = __dsDonCache || [];
  var tk = "";
  try { tk = (document.getElementById("loc-tukhoa").value || "").trim().toLowerCase(); } catch (e) {}
  var st = "all";
  try { st = document.getElementById("loc-trangthai").value || "all"; } catch (e) {}
  var loc = rows.filter(function (d) {
    if (st !== "all" && _phanLoaiDon(d) !== st) return false;
    if (tk) {
      var gop = String((d.MaDonHang || "") + " " + (d.Id || "")).toLowerCase();
      if (gop.indexOf(tk) === -1) return false;
    }
    return true;
  });
  if (!rows.length) {
    host.innerHTML = '<div class="card empty-don"><div class="big">🎫</div><h3 style="margin:0 0 6px">Chưa có đơn nào</h3><p class="muted" style="margin:0 0 12px">Bạn chưa đặt đơn nào. Tạo đơn đầu tiên chỉ mất 30 giây.</p><a class="chinh btn sm" href="tao-don.html">+ Tạo đơn mới</a></div>';
    return;
  }
  if (!loc.length) {
    host.innerHTML = '<div class="card empty-don"><div class="big">🔎</div><h3 style="margin:0 0 6px">Không khớp bộ lọc</h3><p class="muted" style="margin:0">Thử đổi từ khóa hoặc chọn “Tất cả trạng thái”.</p></div>';
    return;
  }
  host.innerHTML = '<div class="don-grid">' + loc.map(_cardDon).join("") + "</div>" +
    '<p class="muted small" style="margin:10px 2px 0">Hiển thị ' + loc.length + " / " + rows.length + " đơn.</p>";
}

function _ganBoLocDon() {
  var tk = document.getElementById("loc-tukhoa");
  var st = document.getElementById("loc-trangthai");
  if (tk && !tk.__gan) { tk.__gan = 1; tk.addEventListener("input", function () { veDonList(); }); }
  if (st && !st.__gan) { st.__gan = 1; st.addEventListener("change", function () { veDonList(); }); }
}

async function taiDonCuaToi() {
  skeleton("ds-don", 3);
  try {
    var kq = await apiFetch("/api/orders/me");
    var rows = kq.data || [];
    __dsDonCache = rows;
    var ctHost = document.getElementById("ds-chitiet");
    if (ctHost) ctHost.innerHTML = "";
    var veHost = document.getElementById("ds-ve");
    if (veHost) veHost.innerHTML = "";
    capNhatThongKeDon(rows);
    _ganBoLocDon();
    veDonList();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function xemChiTietDon(id) {
  id = id || num("don-chitiet-id");
  if (!id || id <= 0) { toast("Nhap don hang de xem chi tiet.", "loi"); return; }
  try {
    var kq = await apiFetch("/api/orders/" + id);
    var d = kq.data || kq;
    var rows = d.ChiTietDonVes || d.ChiTiets || [];
    var host = document.getElementById("ds-chitiet") || document.getElementById("ds-don");
    var tong = 0;
    try {
      rows.forEach(function (r) { tong += Number(r.ThanhTien || 0); });
    } catch (e) {}
    var tenSK = "";
    try { if (d.SuKien) tenSK = String(d.SuKien.Ten); } catch (e2) {}
    var h = '<div class="card don-detail anim-pop"><h3>Chi tiết đơn #' + esc(id) + "</h3>" +
      (tenSK ? '<p class="muted" style="margin:4px 0 0">Sự kiện: <b>' + esc(tenSK) + "</b></p>" : "") +
      bangHTML(
        [["TenLoaiVe", "Hạng vé"], ["SoLuong", "SL"], ["DonGia", "Đơn giá", "tien"], ["ThanhTien", "Thành tiền", "tien"]],
        rows.map(function (r) {
          return {
            TenLoaiVe: (r.LoaiVe && r.LoaiVe.Ten) ? r.LoaiVe.Ten : "",
            SoLuong: r.SoLuong,
            DonGia: r.DonGia,
            ThanhTien: r.ThanhTien
          };
        })) +
      '<div class="don-sum"><span>Tổng cộng (' + rows.length + ' dòng vé)</span><span class="tien">' + esc(fmtTien(tong)) + "đ</span></div>" +
      '<div class="don-actions"><a class="phu btn sm" href="../thanh-toan/thanh-toan.html">Sang thanh toán</a> ' +
      '<a class="phu btn sm" href="../ve/ve-theo-don.html">Xem vé theo đơn</a></div></div>';
    host.innerHTML = h;
    host.scrollIntoView({ behavior: "smooth", block: "nearest" });
  } catch (e) { thongBao(e.message, "loi"); }
}

async function huyDon(id) {
  if (!id || id <= 0) { toast("Thieu don hang.", "loi"); return; }
  try {
    await apiFetch("/api/orders/" + id + "/cancel", "POST", null);
    thongBao("Da huy don hang.");
    taiDonCuaToi();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function taiGiaoDich() {
  try {
    var id = num("don-gd-id");
    var kq = await apiFetch("/api/orders/" + id);
    var d = kq.data || kq;
    var rows = (d.ThanhToans || []).map(function (p) {
      return {
        MaGiaoDich: p.MaGiaoDich,
        PhuongThucThanhToan: "Tien mat",
        SoTien: p.SoTien,
        TrangThai: p.TrangThai
      };
    });
    document.getElementById("ds-giaodich").innerHTML = bangHTML(
      [["MaGiaoDich", "Ma GD"], ["PhuongThucThanhToan", "Phuong thuc"], ["SoTien", "So tien", "tien"], ["TrangThai", "Trang thai"]],
      rows);
  } catch (e) { thongBao(e.message, "loi"); }
}

/* ---- Thanh toan ---- */
async function thanhToanDon(donID) {
  if (!donID || donID <= 0) { toast("Thieu don hang.", "loi"); return; }
  try {
    var kq = await apiFetch("/api/payments", "POST", { DonVeId: donID });
    var p = kq.data || kq;
    var url = p.checkoutUrl || "";
    thongBao("Tao link thanh toán thành công.");
    if (url) {
      window.open(url, "_blank");
    } else {
      thongBao("Don da thanh toan (tong tien 0) - ve da duoc phat hanh.");
    }
    taiDonCuaToi();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function thanhToan() {
  var don = num("tt-don");
  if (!don || don <= 0) { toast("Nhap don hang hop le.", "loi"); return; }
  try {
    var kq = await apiFetch("/api/payments", "POST", { DonVeId: don });
    var p = kq.data || kq;
    var url = p.checkoutUrl || "";
    thongBao("Ghi nhan thanh toan thanh cong.");
    if (url) {
      if (url.startsWith("/")) {
        location.href = API_BASE + url;
      } else {
        window.open(url, "_blank");
      }
    }
    taiDonCuaToi();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function hoanTien() {
  var don = num("ht-don"), tien = num("ht-tien"), magd = val("ht-magd");
  if (!don || don <= 0) { toast("Nhap don hang hop le.", "loi"); return; }
  if (tien === null || isNaN(tien) || tien <= 0) { toast("So tien hoan phai > 0.", "loi"); return; }
  if (!magd) { toast("Nhap ma giao dich hoan tien.", "loi"); return; }
  try {
    await apiFetch("/api/payments/" + don + "/refund", "POST", { SoTien: tien, MaHoanTien: magd });
    thongBao("Ghi nhan hoan tien thanh cong.");
    taiDonCuaToi();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function taiThanhToanTheoDon() {
  var id = num("tt-theodon");
  if (!id || id <= 0) { toast("Nhap don hang de xem thanh toan.", "loi"); return; }
  try {
    var kq = await apiFetch("/api/orders/" + id);
    var d = kq.data || kq;
    var rows = (d.ThanhToans || []).map(function (p) {
      return {
        MaGiaoDich: p.MaGiaoDich,
        PhuongThucThanhToan: "Tien mat",
        SoTien: p.SoTien,
        TrangThai: p.TrangThai
      };
    });
    document.getElementById("ds-thanhtoan").innerHTML = bangHTML(
      [["MaGiaoDich", "Ma GD"], ["PhuongThucThanhToan", "Phuong thuc"], ["SoTien", "So tien", "tien"], ["TrangThai", "Trang thai"]],
      rows);
  } catch (e) { thongBao(e.message, "loi"); }
}

async function taiThongKeThanhToan() {
  try {
    var kq = await apiFetch("/api/orders/me");
    var rows = kq.data || [];
    var thanhCong = rows.filter(function (o) { return _phanLoaiDon(o) === "paid" || _phanLoaiDon(o) === "refund"; }).length;
    var tong = rows.reduce(function (s, o) { return s + Number(o.TongTien || 0); }, 0);
    var host = document.getElementById("tk-thanhtoan");
    if (host) host.innerHTML = bangHTML(
      [["MaGiaoDich", "Ma GD"], ["SoTien", "So tien", "tien"], ["TrangThai", "Trang thai"]],
      rows.map(function (o) {
        return { MaGiaoDich: o.MaDonHang || ("DH-" + o.Id), SoTien: o.TongTien, TrangThai: o.TrangThai };
      }).slice(0, 10));
  } catch (e) { thongBao(e.message, "loi"); }
}

/* ---- Phat hanh ve theo don ---- */
async function phatHanhVe(donID) {
  if (!donID || donID <= 0) { toast("Thieu don hang.", "loi"); return; }
  try {
    // Ve tu phat khi thanh toan thanh cong (webhook); khong co API phat lai theo don.
    var kq = await apiFetch("/api/orders/" + donID);
    var d = kq.data || kq;
    thongBao("Don #" + esc(donID) + " co " + (d.Ves ? d.Ves.length : 0) + " ve. Xem chi tiet tai trang Ve theo don.");
    taiDonCuaToi();
    banConfetti(100);
  } catch (e) { thongBao(e.message, "loi"); }
}

/* ---- Loai ve: them/cap nhat theo su kien (BanToChuc) ---- */
async function themLoaiVe() {
  var sk = num("lv-sk"), ten = val("lv-ten"), gia = num("lv-gia");
  var sl = num("lv-slmax"), mo = val("lv-mo"), dong = val("lv-dong");
  if (!sk || sk <= 0) { toast("Nhap ma su kien hop le.", "loi"); return; }
  if (!ten) { toast("Nhap ten hang ve.", "loi"); return; }
  if (gia === null || isNaN(gia) || gia < 0) { toast("Gia ve phai la so khong am.", "loi"); return; }
  if (sl === null || isNaN(sl) || sl <= 0) { toast("So luong phai > 0.", "loi"); return; }
  if (!mo || !dong) { toast("Chon thoi gian mo ban va dong ban.", "loi"); return; }
  try {
    await apiFetch("/api/events/" + sk + "/ticket-types", "POST", {
      Ten: ten, Gia: gia, SoLuong: sl, MoBan: mo || null, DongBan: dong || null
    });
    thongBao("Them loai ve thanh cong.");
    taiLoaiVeTheoSuKien(sk);
  } catch (e) { thongBao(e.message, "loi"); }
}

async function capNhatLoaiVe() {
  var id = num("lv-id"), ten = val("lv-ten"), gia = num("lv-gia");
  var sl = num("lv-slmax"), mo = val("lv-mo"), dong = val("lv-dong");
  if (!id || id <= 0) { toast("Nhap ID loai ve can cap nhat.", "loi"); return; }
  if (!ten) { toast("Nhap ten hang ve.", "loi"); return; }
  if (gia === null || isNaN(gia) || gia < 0) { toast("Gia ve phai la so khong am.", "loi"); return; }
  if (sl === null || isNaN(sl) || sl <= 0) { toast("So luong phai > 0.", "loi"); return; }
  if (!mo || !dong) { toast("Chon thoi gian mo ban va dong ban.", "loi"); return; }
  try {
    await apiFetch("/api/ticket-types/" + id, "PUT", {
      Ten: ten, Gia: gia, SoLuong: sl, MoBan: mo || null, DongBan: dong || null
    });
    thongBao("Cap nhat loai ve thanh cong.");
    taiLoaiVeTheoSuKien(num("lv-sk"));
  } catch (e) { thongBao(e.message, "loi"); }
}
