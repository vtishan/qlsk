/* ===== checkin.js — ve + check-in + xuat ve ===== */
"use strict";
/* global apiFetch, toast, thongBao, val, num, bangHTML, badgeTT, skeleton, esc, isoLocal, fmtTien */

async function taiVeTheoDon() {
  var id = num("ve-theodon");
  if (!id || id <= 0) { toast("Nhap don hang de xem ve.", "loi"); return; }
  skeleton("ds-ve-theodon", 3);
  try {
    var kq = await apiFetch("/api/orders/" + id);
    var d = kq.data || kq;
    var rows = (d.Ves || []).map(function (v) {
      return {
        MaVe: v.MaVe,
        MaQR: v.MaQR,
        TrangThai: v.TrangThai,
        GiaThucTe: v.LoaiVe ? v.LoaiVe.Gia : 0,
        VeID: v.Id
      };
    });
    document.getElementById("ds-ve-theodon").innerHTML = bangHTML(
      [["MaVe", "Ma ve"], ["MaQR", "Ma QR"], ["TrangThai", "Trang thai"], ["GiaThucTe", "Gia", "tien"]],
      rows,
      function (d) {
        return '<button class="phu sm" onclick="taiLichSuXuat(' + d.VeID + ')">Lich su xuat</button>';
      });
  } catch (e) { thongBao(e.message, "loi"); }
}

async function traCuuVe() {
  var ma = val("ci-ma"), qr = val("ci-qr");
  if (!ma && !qr) { toast("Nhap ma ve hoac ma QR de tra cuu.", "loi"); return; }
  try {
    var code = ma || qr;
    var kq = await apiFetch("/api/tickets/by-code/" + encodeURIComponent(code));
    var v = kq.data || {};
    var host = document.getElementById("kq-tracuu");
    if (host) {
      host.innerHTML = '<div class="ticket anim-pop"><div class="muted" style="color:#e0e7ff">Ket qua tra cuu</div>' +
        "<h3>" + esc(v.MaVe || ma || qr) + "</h3>" +
        '<div class="muted" style="color:#e0e7ff">QR: ' + esc(v.MaQR || "") + " &middot; Trang thai: " + esc(v.TrangThai || "") + "</div>" +
        '<div class="toolbar"><button class="phu sm" onclick="dienVeCheckin(\'' + escJs(v.MaVe || ma || qr) + '\')">Dien vao check-in</button></div></div>';
    }
    thongBao("Tra cuu ve thanh cong.");
  } catch (e) { thongBao(e.message, "loi"); }
}

function escJs(s) {
  return String(s === null || s === undefined ? "" : s).replace(/\\/g, "\\\\").replace(/'/g, "\\'").replace(/"/g, '\\"');
}

function dienVeCheckin(maVe) {
  if (!maVe) { toast("Khong co ma ve.", "loi"); return; }
  var el = document.getElementById("ci-ve");
  if (el) el.value = maVe;
  toast("Da dien ma ve " + maVe + " vao o check-in.", "info");
}

async function checkIn() {
  var ma = val("ci-ve");
  if (!ma) { toast("Nhap ma ve hop le de check-in.", "loi"); return; }
  try {
    var kq = await apiFetch("/api/check-in", "POST", { Ma: ma });
    thongBao("Check-in thanh cong: " + ((kq.data && kq.data.MaVe) || ("Ve " + ma)));
    var host = document.getElementById("kq-checkin");
    if (host) {
      host.innerHTML = '<div class="card anim-pop"><h3>Check-in thanh cong</h3>' +
        '<div>' + badgeTT((kq.data && kq.data.TrangThai) || "Da check-in") + '</div>' +
        '<div class="muted">Ma ve: ' + esc((kq.data && kq.data.MaVe) || ma) + "</div></div>";
    }
  } catch (e) { thongBao(e.message, "loi"); }
}

async function xuatVe() {
  var id = num("xv-ve");
  var ht = val("xv-ht");
  if (!id || id <= 0) { toast("Nhap ma ve hop le.", "loi"); return; }
  if (!ht) { toast("Chon hinh thuc xuat ve.", "loi"); return; }
  try {
    var endpoint = (ht && String(ht).toLowerCase().indexOf("email") !== -1)
      ? "/api/tickets/" + id + "/email"
      : "/api/tickets/" + id + "/export";
    var kq = await apiFetch(endpoint, "POST", null);
    thongBao("Xuat ve #" + id + " (" + ht + ") thanh cong." + (kq && kq.message ? " " + kq.message : ""));
  } catch (e) { thongBao(e.message, "loi"); }
}

async function guiEmailVe(veID) {
  if (!veID || veID <= 0) { toast("Thieu ma ve.", "loi"); return; }
  try {
    await apiFetch("/api/tickets/" + veID + "/email", "POST", null);
    thongBao("Da gui ve #" + veID + " qua email.");
  } catch (e) { thongBao(e.message, "loi"); }
}

async function xuatVeNhanh(veID) {
  if (!veID || veID <= 0) { toast("Thieu ma ve.", "loi"); return; }
  var htEl = document.getElementById("xv-ht");
  var ht = htEl ? (htEl.value || "").trim() : "";
  if (!ht) { toast("Chon hinh thuc xuat ve.", "loi"); return; }
  try {
    var kq = await apiFetch("/api/tickets/" + veID + "/export", "POST", null);
    thongBao("Xuat ve #" + veID + " thanh cong.");
  } catch (e) { thongBao(e.message, "loi"); }
}

async function taiLichSuXuat(veID) {
  var id = veID || num("xv-lichsu-ve");
  if (!id || id <= 0) { toast("Nhap ma ve de xem lich su xuat.", "loi"); return; }
  try {
    var kq = await apiFetch("/api/tickets/" + id + "/exports");
    var rows = (kq.data || []).map(function (x) { return { HinhThucXuat: x.Kieu, NgayXuat: x.NgayTao, TrangThai: x.ThanhCong ? "Thành công" : "Thất bại" }; });
    var host = document.getElementById("ds-xuatve") || document.getElementById("ds-ve-theodon");
    if (host) {
      host.innerHTML = "<h3>Lich su xuat ve #" + id + "</h3>" + bangHTML(
        [["HinhThucXuat", "Hinh thuc"], ["NgayXuat", "Ngay xuat", "ngay"], ["TrangThai", "Trang thai"]],
        rows);
      host.scrollIntoView({ behavior: "smooth", block: "nearest" });
    }
  } catch (e) { thongBao(e.message, "loi"); }
}
