/* ===== quantri.js — thong ke tong hop + tai khoan mo rong ===== */
"use strict";
/* global apiFetch, thongBao, toast, val, num, bangHTML, badgeTT, skeleton, esc */

async function taiThongKe() {
  skeleton("tk-taikhoan", 3);
  skeleton("tk-sukien", 3);
  skeleton("tk-ve", 3);
  try {
    var dash = await apiFetch("/api/admin/dashboard");
    var d = dash.data || dash;
    renderTk("tk-taikhoan", [
      ["vaiTro", "Vai tro"], ["soLuong", "So luong"]
    ], d.theoVaiTro || []);
    document.getElementById("tk-sukien").innerHTML = bangHTML(
      [["trangThai", "Trang thai"], ["soLuong", "So luong"]],
      (d.theoTrangThaiSuKien || []).map(function (g) {
        return { trangThai: g.trangThai || g.TrangThai || "Khac", soLuong: g.soLuong || g.SoLuong || 0 };
      })
    );
    document.getElementById("tk-ve").innerHTML = bangHTML(
      [["TrangThai", "Trang thai"], ["SoLuongVe", "So luong"]],
      [
        { TrangThai: "Ve da check in", SoLuongVe: d.veDaCheckIn || 0 },
        { TrangThai: "Don thanh cong", SoLuongVe: d.donThanhCong || 0 },
        { TrangThai: "Hoan tien", SoLuongVe: d.hoanTien || 0 }
      ]
    );
    /* mo rong: thong ke con lai chay nen, khong lam hong khoi chinh */
    try { await taiThongKeTaiKhoanMoRong(); } catch (e) {}
    try {
      if (typeof taiThongKeToChuc === "function") await taiThongKeToChuc();
    } catch (e) {}
    try {
      if (typeof taiThongKeDatDiaDiem === "function") await taiThongKeDatDiaDiem();
    } catch (e) {}
    try {
      if (typeof taiThongKeThanhToan === "function") await taiThongKeThanhToan();
    } catch (e) {}
    try { await taiThongKeCheckIn(); } catch (e) {}
  } catch (e) { thongBao(e.message, "loi"); }
}

function renderTk(hostId, cols, rows) {
  var host = document.getElementById(hostId);
  if (!host) return;
  host.innerHTML = bangHTML(cols, (rows || []).map(function (r, i) {
    var obj = {};
    obj[cols[0][0]] = i + 1;
    obj[cols[1][0]] = r[cols[1][1]] || r[cols[1][0]] || 0;
    return obj;
  }));
}

async function taiThongKeTaiKhoanMoRong() {
  try {
    var kq = await apiFetch("/api/accounts");
    var rows = kq.data || [];
    var host = document.getElementById("tk-taikhoan-tt");
    if (host) host.innerHTML = bangHTML([["TrangThai", "Trang thai"], ["SoLuong", "So luong"]],
      [
        { TrangThai: "Hoat dong", SoLuong: rows.filter(function (x) { return x.HoatDong; }).length },
        { TrangThai: "Da khoa", SoLuong: rows.filter(function (x) { return !x.HoatDong; }).length }
      ]);
  } catch (e) { /* bo qua khi thieu quyen */ }
}

async function taiSuKienCuaTaiKhoan() {
  var id = num("qtk-sukien-id") || num("qtk-id");
  if (!id || id <= 0) { toast("Nhap tai khoan de xem su kien.", "loi"); return; }
  try {
    var kq = await apiFetch("/api/events/mine");
    var rows = (kq.data || []).filter(function (e) { return e.BanToChucId === id; });
    document.getElementById("ds-sukien-cua-toi").innerHTML = bangHTML(
      [["Ten", "Su kien"], ["TrangThai", "Trang thai"]],
      rows.map(function (e) { return { Ten: e.Ten, TrangThai: e.TrangThai }; })
    );
  } catch (e) { thongBao(e.message, "loi"); }
}

async function taiDonVeCuaTaiKhoan() {
  var id = num("qtk-donve-id") || num("qtk-id");
  if (!id || id <= 0) { toast("Nhap tai khoan de xem don ve.", "loi"); return; }
  try {
    var kq = await apiFetch("/api/orders/me");
    var rows = kq.data || [];
    document.getElementById("ds-don-cua-toi").innerHTML = bangHTML(
      [["MaDonHang", "Ma don"], ["TongTien", "Tong TT", "tien"], ["TrangThai", "Don"]],
      rows.map(function (d) { return { MaDonHang: d.MaDonHang || ("DH-" + d.Id), TongTien: d.TongTien, TrangThai: d.TrangThai }; })
    );
  } catch (e) { thongBao(e.message, "loi"); }
}

async function taiThongKeCheckIn() {
  var id = num("tk-checkin-sk");
  var host = document.getElementById("tk-checkin");
  if (!host) return;
  if (!id || id <= 0) {
    host.innerHTML = '<p class="muted">Nhap su kien roi bam "Tai check-in" de xem thong ke diem danh.</p>';
    return;
  }
  try {
    var kq = await apiFetch("/api/events/" + id);
    var e = kq.data || kq;
    host.innerHTML = bangHTML([["TrangThai", "Trang thai"], ["SoLuong", "So luong"]],
      [
        { TrangThai: "Su kien", SoLuong: e.SoNguoi || 0 },
        { TrangThai: "Trang thai hien tai", SoLuong: e.TrangThai ? 1 : 0 }
      ]);
  } catch (e) { thongBao(e.message, "loi"); }
}
