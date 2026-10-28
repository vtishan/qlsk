/* ===== sukien.js — loai su kien + su kien ===== */
"use strict";
/* global apiFetch, toast, thongBao, val, num, bangHTML, badgeTT, skeleton, esc, isoLocal, fmtTien, checkThoiGian, apDungQuyen */

function ngayThang(v) {
  try {
    var d = new Date(v);
    if (isNaN(d)) return "--";
    return ("0" + d.getDate()).slice(-2);
  } catch (e) { return "--"; }
}
function thangNam(v) {
  try {
    var d = new Date(v);
    if (isNaN(d)) return "";
    return "T" + (d.getMonth() + 1);
  } catch (e) { return ""; }
}

async function taiDanhSachSuKien() {
  skeleton("ds-sukien", 4);
  try {
    var tk = val("sk-tukhoa");
    var qs = "/api/events" + (tk ? "?q=" + encodeURIComponent(tk) : "");
    var kq = await apiFetch(qs);
    var rows = (kq.data || []).map(function (d) {
      return {
        TenSuKien: d.Ten, TenLoai: (d.LoaiSuKien && d.LoaiSuKien.Ten) ? d.LoaiSuKien.Ten : "Su kien",
        TrangThai: d.TrangThai,
        BatDau: isoLocal(d.BatDau), KetThuc: isoLocal(d.KetThuc),
        SuKienID: d.Id, _raw: d
      };
    });
    var host = document.getElementById("ds-sukien");
    if (!rows.length) { host.innerHTML = bangHTML([], []); return; }
    var h = '<div class="ev-grid">';
    rows.forEach(function (r) {
      var d = r._raw;
      h += '<article class="ev-card anim-pop">' +
        '<div class="ev-cover ev-cover-' + (d.SuKienID % 6) + '">' +
        '<span class="ev-date"><b>' + esc(ngayThang(d.BatDau)) + '</b><span>' + esc(thangNam(d.BatDau)) + '</span></span>' +
        '<span class="badge b-info ev-cat">' + esc(r.TenLoai || "Su kien") + '</span>' +
        '<span class="ev-status">' + badgeTT(r.TrangThai) + '</span></div>' +
        '<div class="ev-body">' +
        '<h3 class="ev-title">' + esc(r.TenSuKien) + '</h3>' +
        '<div class="ev-meta">' + esc(r.BatDau) + " &rarr; " + esc(r.KetThuc) + "</div>" +
        '<div class="ev-meta muted">Ma hien thi su kien: ' + d.Id + "</div>" +
        '<div class="ev-actions">' +
        '<button class="btn chinh sm" onclick="xemChiTietSuKien(' + d.Id + ')">Chi tiet</button>' +
        '<span data-quyen="QuanLySuKien,QuanTriVien"><button class="btn phu sm" onclick="duyetSuKien(' + d.Id + ')">Duyet</button> ' +
        '<button class="btn phu sm" onclick="tuChoiSuKien(' + d.Id + ')">Tu choi</button></span>' +
        '<button class="btn phu sm" onclick="taiLoaiVeTheoSuKien(' + d.Id + ')">Mua ve</button>' +
        "</div></div></article>";
    });
    host.innerHTML = h + "</div>";
    if (typeof apDungQuyen === "function") apDungQuyen();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function xemChiTietSuKien(id) {
  try {
    var kq = await apiFetch("/api/events/" + id);
    var d = kq.data || {};
    var host = document.getElementById("sk-chitiet");
    if (host) {
      host.innerHTML = '<div class="card anim-pop"><h3>' + esc(d.Ten || ("Su kien #" + id)) + "</h3>" +
        '<div class="muted">' + esc((d.LoaiSuKien && d.LoaiSuKien.Ten) ? d.LoaiSuKien.Ten : "") + " &middot; " + esc(isoLocal(d.BatDau)) + " &rarr; " + esc(isoLocal(d.KetThuc)) + "</div>" +
        "<p>" + esc(d.MoTa || "Chua co mo ta.") + "</p>" +
        '<div class="toolbar"><span>' + badgeTT(d.TrangThai) + "</span>" +
        '<button class="phu sm" onclick="taiLoaiVeTheoSuKien(' + id + ')">Xem loai ve</button></div></div>';
      host.scrollIntoView({ behavior: "smooth", block: "nearest" });
    }
  } catch (e) { thongBao(e.message, "loi"); }
}

async function taoSuKien() {
  var ten = val("sk-ten");
  var loai = num("sk-loai");
  var bd = val("sk-bd"), kt = val("sk-kt");
  var sl = num("sk-sl");
  if (!ten) { toast("Nhap ten su kien.", "loi"); return; }
  if (!loai || loai <= 0) { toast("Chon loai su kien.", "loi"); return; }
  if (!checkThoiGian(bd, kt)) return;
  if (sl === null || isNaN(sl) || sl <= 0) { toast("So nguoi du kien phai > 0.", "loi"); return; }
  try {
    var kq = await apiFetch("/api/events", "POST", {
      Ten: ten,
      MoTa: val("sk-mota") || null,
      BatDau: bd || null,
      KetThuc: kt || null,
      SoNguoi: sl,
      LoaiSuKienId: loai
    });
    thongBao("Tao su kien thanh cong, cho duyet.");
    taiDanhSachSuKien();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function duyetSuKien(id) {
  try {
    await apiFetch("/api/events/" + id + "/approve", "POST", null);
    thongBao("Da duyet su kien.");
    taiDanhSachSuKien();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function tuChoiSuKien(id) {
  var lydo = prompt("Nhap ly do tu choi su kien:");
  if (lydo === null) return;
  if (!lydo.trim()) { toast("Phai nhap ly do tu choi.", "loi"); return; }
  try {
    await apiFetch("/api/events/" + id + "/reject", "POST", { LyDo: lydo.trim() });
    thongBao("Da tu choi su kien.");
    taiDanhSachSuKien();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function huySuKien(id) {
  if (!confirm("Xac nhan huy su kien #" + id + "?")) return;
  try {
    await apiFetch("/api/events/" + id + "/cancel", "POST", null);
    thongBao("Da huy su kien.");
    taiDanhSachSuKien();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function taiLoaiSuKien() {
  skeleton("ds-loai", 3);
  try {
    var kq = await apiFetch("/api/event-types");
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
          return '<button class="phu sm" onclick="doiTrangThaiLoai(' + d.Id + ",1)\">Bat</button> " +
            '<button class="phu sm" onclick="doiTrangThaiLoai(' + d.Id + ",0)\">Tat</button>";
        });
    }
  } catch (e) { thongBao(e.message, "loi"); }
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
  if (!id || id <= 0) { toast("Nhap ID loai can cap nhat.", "loi"); return; }
  var ten = val("lsk-ten");
  if (!ten) { toast("Nhap ten loai su kien.", "loi"); return; }
  try {
    await apiFetch("/api/event-types/" + id, "PUT", { Ten: ten, MoTa: val("lsk-mota") || null });
    thongBao("Cap nhat loai thanh cong.");
    taiLoaiSuKien();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function doiTrangThaiLoai(id, trangThai) {
  try {
    await apiFetch("/api/event-types/" + id, "PUT", { Ten: val("lsk-ten") || "", MoTa: val("lsk-mota") || null });
    thongBao("Cap nhat loai thanh cong.");
    taiLoaiSuKien();
  } catch (e) { thongBao(e.message, "loi"); }
}
