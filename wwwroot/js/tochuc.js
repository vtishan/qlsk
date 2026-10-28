/* ===== tochuc.js — xac thuc ban to chuc ===== */
"use strict";
/* global apiFetch, toast, thongBao, val, bangHTML, badgeTT, skeleton, isoLocal, esc */

async function guiYeuCau() {
  var tenToChuc = val("tc-ten") || val("tc-cccd") || "";
  var giayTo = val("tc-giayto") || val("tc-truoc") || "";
  if (!tenToChuc || !giayTo) {
    toast("Vui long nhap ten to chuc va giay to chung nhan.", "loi");
    return;
  }
  try {
    var kq = await apiFetch("/api/organizer-verifications", "POST", {
      TenToChuc: tenToChuc,
      GiayTo: giayTo
    });
    thongBao("Gui yeu cau thanh cong. ID: " + ((kq.data && (kq.data.Id || kq.data.Id)) || ""));
    taiYeuCauCuaToi();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function capNhatYeuCau() {
  var tenToChuc = val("tc-ten") || val("tc-cccd") || "";
  var giayTo = val("tc-giayto") || val("tc-truoc") || "";
  if (!tenToChuc || !giayTo) {
    toast("Vui long nhap ten to chuc va giay to chung nhan.", "loi");
    return;
  }
  // Backend khong co PUT organizer-verifications/{id}; gui yeu cau moi (POST).
  try {
    await apiFetch("/api/organizer-verifications", "POST", { TenToChuc: tenToChuc, GiayTo: giayTo });
    thongBao("Gui yeu cau moi thanh cong.");
    taiYeuCauCuaToi();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function taiYeuCauCuaToi() {
  skeleton("ds-yeucau", 3);
  try {
    var kq = await apiFetch("/api/organizer-verifications/me");
    var rows = (kq.data || []).map(function (d) {
      return {
        YeuCauID: d.Id,
        SoCCCD: d.TenToChuc,
        TrangThai: d.TrangThai,
        NgayTao: isoLocal(d.NgayTao),
        LyDoTuChoi: d.LyDo
      };
    });
    var host = document.getElementById("ds-yeucau");
    if (!rows.length) { host.innerHTML = bangHTML([], []); return; }
    host.innerHTML = '<div class="grid g2">' + rows.map(function (r) {
      return '<div class="card hoverable anim-pop"><h3>Yeu cau #' + esc(r.YeuCauID) + '</h3>' +
        '<div class="muted">To chuc: ' + esc(r.SoCCCD) + " &middot; " + esc(r.NgayTao) + "</div>" +
        '<div class="toolbar"><span>' + badgeTT(r.TrangThai) + "</span></div>" +
        (r.LyDoTuChoi ? '<div class="muted">Ly do tu choi: ' + esc(r.LyDoTuChoi) + "</div>" : "") +
        "</div>";
    }).join("") + "</div>";
  } catch (e) { thongBao(e.message, "loi"); }
}

async function duyetYeuCauToChuc(id) {
  try {
    await apiFetch("/api/organizer-verifications/" + id + "/approve", "POST", null);
    thongBao("Da duyet yeu cau #" + id + ".");
    if (typeof taiThongKeToChuc === "function") taiThongKeToChuc();
    taiYeuCauCuaToi();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function tuChoiYeuCauToChuc(id) {
  var lydo = prompt("Nhap ly do tu choi yeu cau #" + id + ":");
  if (lydo === null) return;
  if (!lydo.trim()) { toast("Phai nhap ly do tu choi.", "loi"); return; }
  try {
    await apiFetch("/api/organizer-verifications/" + id + "/reject", "POST", { LyDo: lydo.trim() });
    thongBao("Da tu choi yeu cau #" + id + ".");
    if (typeof taiThongKeToChuc === "function") taiThongKeToChuc();
    taiYeuCauCuaToi();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function taiThongKeToChuc() {
  try {
    var kq = await apiFetch("/api/organizer-verifications");
    var rows = kq.data || [];
    var host = document.getElementById("tk-tochuc");
    if (host) host.innerHTML = bangHTML(
      [["TrangThai", "Trang thai"], ["SoLuong", "So luong"]],
      groupCount(rows, function (r) { return String(r.TrangThai || r.trangThai || "Chua xet"); }));
  } catch (e) { thongBao(e.message, "loi"); }
}

function groupCount(rows, keyFn) {
  var map = {};
  rows.forEach(function (r) {
    var k = keyFn(r);
    map[k] = (map[k] || 0) + 1;
  });
  return Object.keys(map).map(function (k) { return { TrangThai: k, SoLuong: map[k] }; });
}
