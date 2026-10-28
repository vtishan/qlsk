/* ===== diadiem.js — dia diem + dat dia diem ===== */
"use strict";
/* global apiFetch, toast, thongBao, val, num, bangHTML, badgeTT, skeleton, esc, fmtTien, checkThoiGian, checkSoKhongAm */

async function taiDiaDiem() {
  skeleton("ds-diadiem", 4);
  try {
    var qs = "/api/venues";
    var tk = val("dd-tukhoa");
    if (tk) qs += "?q=" + encodeURIComponent(tk);
    var kq = await apiFetch(qs);
    var rows = (kq.data || []).map(function (d) {
      return {
        Id: d.Id, Ten: d.Ten, DiaChi: d.DiaChi, SucChua: d.SucChua,
        GiaThue: d.GiaThue, HoatDong: d.HoatDong, TrangThai: d.HoatDong ? "Hoat dong" : "Da tat"
      };
    });
    var host = document.getElementById("ds-diadiem");
    if (!rows.length) { host.innerHTML = bangHTML([], []); return; }
    host.innerHTML = '<div class="grid g3">' + rows.map(function (d) {
      return '<article class="card hoverable anim-pop">' +
        '<div class="toolbar" style="margin:0 0 8px"><span class="badge b-info">#' + d.Id + "</span>" +
        '<span style="margin-left:auto">' + badgeTT(d.TrangThai) + "</span></div>" +
        "<h3>" + esc(d.Ten) + "</h3>" +
        '<div class="muted">' + esc(d.DiaChi || "") + "</div>" +
        '<div class="muted">Suc chua: <b>' + esc(d.SucChua) + "</b> &middot; Gia thue: <b>" + esc(fmtTien(d.GiaThue)) + "</b></div>" +
        '<div class="toolbar"><button class="phu sm" onclick="doiTrangThaiDiaDiem(' + d.Id + ",1)\">Bat</button> " +
        '<button class="phu sm" onclick="doiTrangThaiDiaDiem(' + d.Id + ",0)\">Tat</button></div>" +
        "</article>";
    }).join("") + "</div>";
  } catch (e) { thongBao(e.message, "loi"); }
}

async function themDiaDiem() {
  var ten = val("dd-ten"), dc = val("dd-dc");
  var sc = num("dd-sc"), gia = num("dd-gia");
  if (!ten || !dc) { toast("Nhap ten dia diem va dia chi.", "loi"); return; }
  if (sc === null || isNaN(sc) || sc <= 0) { toast("Suc chua phai > 0.", "loi"); return; }
  if (!checkSoKhongAm(val("dd-gia"), "Gia thue")) return;
  try {
    await apiFetch("/api/venues", "POST", {
      Ten: ten, DiaChi: dc, SucChua: sc, GiaThue: gia, HoatDong: true
    });
    thongBao("Them dia diem thanh cong.");
    taiDiaDiem();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function capNhatDiaDiem() {
  var id = num("dd-id");
  if (!id || id <= 0) { toast("Nhap ID dia diem can cap nhat.", "loi"); return; }
  var ten = val("dd-ten"), dc = val("dd-dc");
  if (!ten || !dc) { toast("Nhap ten dia diem va dia chi.", "loi"); return; }
  var sc = num("dd-sc");
  if (sc === null || isNaN(sc) || sc <= 0) { toast("Suc chua phai > 0.", "loi"); return; }
  if (!checkSoKhongAm(val("dd-gia"), "Gia thue")) return;
  try {
    await apiFetch("/api/venues/" + id, "PUT", {
      Ten: ten, DiaChi: dc, SucChua: sc, GiaThue: num("dd-gia"), HoatDong: true
    });
    thongBao("Cap nhat dia diem thanh cong.");
    taiDiaDiem();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function doiTrangThaiDiaDiem(id, trangThai) {
  try {
    await apiFetch("/api/venues/" + id, "PUT", { Ten: val("dd-ten") || "", DiaChi: val("dd-dc") || "", SucChua: num("dd-sc") || 0, GiaThue: num("dd-gia") || 0, HoatDong: !!trangThai });
    thongBao("Cap nhat dia diem thanh cong.");
    taiDiaDiem();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function datDiaDiem() {
  var sk = num("dat-sk"), dd = num("dat-dd");
  var bd = val("dat-bd"), kt = val("dat-kt");
  var tc = num("dat-tc");
  if (!sk || sk <= 0 || !dd || dd <= 0) { toast("Nhap su kien va dia diem hop le.", "loi"); return; }
  if (!checkThoiGian(bd, kt)) return;
  if (tc === null || isNaN(tc) || tc < 0) { toast("Tien coc phai la so khong am.", "loi"); return; }
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
    if (h1) h1.innerHTML = '<div class="muted">Thong ke dat dia diem lay tu danh sach dat.</div>';
    if (h2) h2.innerHTML = bangHTML(
      [["TenDiaDiem", "Dia diem"], ["SoLuotDat", "Luot dat"]],
      (kq.data || []).filter(function (x) { return x.DiaDiem; }).map(function (x) {
        return { TenDiaDiem: x.DiaDiem.TenDiaDiem, SoLuotDat: 1 };
      }));
  } catch (e) { thongBao(e.message, "loi"); }
}
