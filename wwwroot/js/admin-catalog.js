"use strict";
var danhMucQuanTri = null;
var veQuanTri = [];
var dangLuuQuanTri = false;

function dienQuanTri(id, value) { document.getElementById(id).value = value == null ? "" : value; }
function ngayNhapQuanTri(value) {
  var date = ngayDuLieu(value);
  return isNaN(date) ? "" : new Date(date.getTime() - date.getTimezoneOffset() * 60000).toISOString().slice(0, 19);
}
function luaChonQuanTri(id, rows, label) {
  document.getElementById(id).innerHTML = '<option value="">Chọn…</option>' + rows.map(function (x) {
    return '<option value="' + Number(x.Id) + '">' + esc(label(x)) + '</option>';
  }).join("");
}
async function taiDanhMucQuanTri() {
  if (store.vaiTro !== "QuanTriVien") return;
  try {
    var result = await apiFetch("/api/admin/catalog");
    danhMucQuanTri = result.data;
    luaChonQuanTri("qt-sk-btc", danhMucQuanTri.ToChuc, function (x) { return x.Ten; });
    luaChonQuanTri("qt-sk-loai", danhMucQuanTri.LoaiSuKien, function (x) { return x.Ten; });
    luaChonQuanTri("qt-tc-tk", danhMucQuanTri.TaiKhoan, function (x) { return x.HoTen + " · " + x.Email; });
    luaChonQuanTri("qt-ve-sk", danhMucQuanTri.SuKien, function (x) { return x.Ten; });
    document.getElementById("qt-ds-sk").innerHTML = bangHTML(
      [["Id", "Mã"], ["Ten", "Sự kiện"], ["TrangThaiTen", "Trạng thái"]],
      danhMucQuanTri.SuKien.map(function (x) { return Object.assign({}, x, { TrangThaiTen: trangThaiSuKienHienThi(x.TrangThai) }); }),
      function (x) { return nutQuanTri("events", x.Id, "suaSuKienQuanTri"); });
    document.getElementById("qt-ds-tc").innerHTML = bangHTML([["Id", "Mã"], ["Ten", "Ban tổ chức"], ["MoTa", "Mô tả"]], danhMucQuanTri.ToChuc,
      function (x) { return nutQuanTri("organizers", x.Id, "suaToChucQuanTri"); });
    document.getElementById("qt-ds-tk").innerHTML = bangHTML([["Id", "Mã"], ["HoTen", "Họ tên"], ["Email", "Email"], ["HoatDong", "Hoạt động"]], danhMucQuanTri.TaiKhoan,
      function (x) { return '<button class="phu sm" onclick="suaTaiKhoanQuanTri(' + Number(x.Id) + ')">Sửa</button>'; });
  } catch (error) { thongBao(error.message, "loi"); }
}
function nutQuanTri(kind, id, edit) {
  return '<button class="phu sm" onclick="' + edit + '(' + Number(id) + ')">Sửa</button> <button class="phu sm" onclick="xoaBanGhiQuanTri(\'' + kind + '\',' + Number(id) + ')">Xóa</button>';
}
function suaSuKienQuanTri(id) {
  var x = danhMucQuanTri.SuKien.find(function (x) { return x.Id === id; });
  if (!x) return;
  [["id", x.Id], ["ten", x.Ten], ["mota", x.MoTa], ["anh", x.HinhAnh], ["btc", x.BanToChucId], ["loai", x.LoaiSuKienId], ["sl", x.SoNguoi], ["bd", ngayNhapQuanTri(x.BatDau)], ["kt", ngayNhapQuanTri(x.KetThuc)]].forEach(function (p) { dienQuanTri("qt-sk-" + p[0], p[1]); });
  document.getElementById("qt-sk-ten").focus();
}
function suaToChucQuanTri(id) {
  var x = danhMucQuanTri.ToChuc.find(function (x) { return x.Id === id; });
  [["id", x.Id], ["ten", x.Ten], ["mota", x.MoTa], ["tk", x.TaiKhoanId]].forEach(function (p) { dienQuanTri("qt-tc-" + p[0], p[1]); });
  document.getElementById("qt-tc-ten").focus();
}
function suaTaiKhoanQuanTri(id) {
  var x = danhMucQuanTri.TaiKhoan.find(function (x) { return x.Id === id; });
  [["id", x.Id], ["ten", x.HoTen], ["email", x.Email], ["mk", ""]].forEach(function (p) { dienQuanTri("qt-tk-" + p[0], p[1]); });
  document.getElementById("qt-tk-ten").focus();
}
async function luuQuanTri(event, kind) {
  event.preventDefault();
  if (dangLuuQuanTri) return;
  dangLuuQuanTri = true;
  var id, body, path;
  try {
    if (kind === "events") {
      id = num("qt-sk-id");
      body = { Ten: val("qt-sk-ten"), MoTa: val("qt-sk-mota"), HinhAnh: val("qt-sk-anh") || null, BanToChucId: num("qt-sk-btc"), LoaiSuKienId: num("qt-sk-loai"), SoNguoi: num("qt-sk-sl"), BatDau: new Date(val("qt-sk-bd")).toISOString(), KetThuc: new Date(val("qt-sk-kt")).toISOString() };
      if (id) {
        var original = danhMucQuanTri.SuKien.find(function (x) { return x.Id === id; });
        if (new Date(val("qt-sk-bd")).getTime() === new Date(ngayNhapQuanTri(original.BatDau)).getTime()) body.BatDau = ngayDuLieu(original.BatDau).toISOString();
        if (new Date(val("qt-sk-kt")).getTime() === new Date(ngayNhapQuanTri(original.KetThuc)).getTime()) body.KetThuc = ngayDuLieu(original.KetThuc).toISOString();
      }
    } else if (kind === "organizers") {
      id = num("qt-tc-id"); body = { Ten: val("qt-tc-ten"), MoTa: val("qt-tc-mota"), TaiKhoanId: num("qt-tc-tk") };
    } else {
      id = num("qt-tk-id"); body = { HoTen: val("qt-tk-ten"), Email: val("qt-tk-email"), MatKhauMoi: val("qt-tk-mk") || null };
    }
    path = kind === "accounts" && !id ? "/api/auth/register" : "/api/admin/catalog/" + kind + (id ? "/" + id : "");
    if (kind === "accounts" && !id) { body.MatKhau = body.MatKhauMoi; delete body.MatKhauMoi; }
    await apiFetch(path, id ? "PUT" : "POST", body);
    event.target.reset();
    thongBao("Đã lưu thay đổi.");
    await taiDanhMucQuanTri();
  } catch (error) { thongBao(error.message, "loi"); }
  finally { dangLuuQuanTri = false; }
}
async function xoaBanGhiQuanTri(kind, id) {
  if (!confirm("Xóa bản ghi này? Chỉ xóa được khi chưa có dữ liệu liên quan.")) return;
  try {
    await apiFetch("/api/admin/catalog/" + kind + "/" + id, "DELETE");
    thongBao("Đã xóa bản ghi.");
    if (kind === "ticket-types") await taiVeQuanTri(); else await taiDanhMucQuanTri();
  } catch (error) { thongBao(error.message, "loi"); }
}
async function taiVeQuanTri() {
  var id = num("qt-ve-sk"); if (!id) return;
  try {
    var result = await apiFetch("/api/events/" + id + "/ticket-types"); veQuanTri = result.data;
    document.getElementById("qt-ds-ve").innerHTML = bangHTML([["Id", "Mã"], ["Ten", "Hạng vé"], ["Gia", "Giá", "tien"], ["SoLuong", "Số lượng"], ["DaBan", "Đã bán"], ["DangGiu", "Đang giữ"]], veQuanTri,
      function (x) { return nutQuanTri("ticket-types", x.Id, "suaVeQuanTri"); });
  } catch (error) { thongBao(error.message, "loi"); }
}
function suaVeQuanTri(id) {
  var x = veQuanTri.find(function (x) { return x.Id === id; });
  [["id", x.Id], ["ten", x.Ten], ["gia", x.Gia], ["sl", x.SoLuong], ["mo", ngayNhapQuanTri(x.MoBan)], ["dong", ngayNhapQuanTri(x.DongBan)]].forEach(function (p) { dienQuanTri("qt-ve-" + p[0], p[1]); });
}
async function luuVeQuanTri(event) {
  event.preventDefault(); if (dangLuuQuanTri) return; dangLuuQuanTri = true;
  try {
    var id = num("qt-ve-id"), sk = num("qt-ve-sk");
    await apiFetch(id ? "/api/ticket-types/" + id : "/api/events/" + sk + "/ticket-types", id ? "PUT" : "POST", { Ten: val("qt-ve-ten"), Gia: num("qt-ve-gia"), SoLuong: num("qt-ve-sl"), MoBan: new Date(val("qt-ve-mo")).toISOString(), DongBan: new Date(val("qt-ve-dong")).toISOString() });
    thongBao("Đã lưu hạng vé."); dienQuanTri("qt-ve-id", ""); await taiVeQuanTri();
  } catch (error) { thongBao(error.message, "loi"); }
  finally { dangLuuQuanTri = false; }
}
document.addEventListener("DOMContentLoaded", taiDanhMucQuanTri);
