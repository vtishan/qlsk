"use strict";
function hienThongKe(id, cols, rows) {
  var host = document.getElementById(id); if (host) host.innerHTML = bangHTML(cols, rows);
}
async function taiThongKe() {
  try {
    var result = await apiFetch("/api/admin/dashboard"), d = result.data;
    hienThongKe("tk-taikhoan", [["vaiTro", "Vai trò"], ["soLuong", "Số lượng"]], d.theoVaiTro);
    hienThongKe("tk-sukien", [["trangThai", "Trạng thái"], ["soLuong", "Số lượng"]], d.theoTrangThaiSuKien);
    hienThongKe("tk-ve", [["trangThai", "Trạng thái vé"], ["soLuong", "Số lượng"]], d.theoTrangThaiVe);
    hienThongKeTaiKhoan(d);
    hienThongKeThanhToan(d);
    if (document.getElementById("tk-tochuc")) await taiThongKeToChuc();
    if (document.getElementById("tk-dat-tt") || document.getElementById("tk-dat-dd")) await taiThongKeDatDiaDiem();
    await taiThongKeCheckIn();
  } catch (error) { thongBao(error.message, "loi"); }
}
function hienThongKeTaiKhoan(d) {
  hienThongKe("tk-taikhoan-tt", [["TrangThai", "Trạng thái"], ["SoLuong", "Số lượng"]], [
    { TrangThai: "Hoạt động", SoLuong: d.taiKhoanHoatDong },
    { TrangThai: "Đã khóa", SoLuong: d.taiKhoan - d.taiKhoanHoatDong }
  ]);
}
async function taiThongKeTaiKhoanMoRong() {
  try { var r = await apiFetch("/api/admin/dashboard"); hienThongKeTaiKhoan(r.data); }
  catch (error) { thongBao(error.message, "loi"); }
}
function hienThongKeThanhToan(d) {
  hienThongKe("tk-thanhtoan", [["trangThai", "Trạng thái"], ["soLuong", "Số giao dịch"], ["soTien", "Số tiền", "tien"], ["soTienHoan", "Đã hoàn", "tien"]], d.theoTrangThaiThanhToan);
}
async function taiSuKienCuaTaiKhoan() {
  var id = num("qtk-sukien-id") || num("qtk-id");
  if (!id) { toast("Nhập mã tài khoản.", "loi"); return; }
  try {
    var r = await apiFetch("/api/admin/catalog/accounts/" + id + "/activity");
    hienThongKe("ds-sukien-cua-toi", [["Ten", "Sự kiện"], ["TrangThaiTen", "Trạng thái"]], r.data.SuKien.map(function (e) { return { Ten: e.Ten, TrangThaiTen: trangThaiSuKienHienThi(e.TrangThai) }; }));
  } catch (error) { thongBao(error.message, "loi"); }
}
async function taiDonVeCuaTaiKhoan() {
  var id = num("qtk-donve-id") || num("qtk-id");
  if (!id) { toast("Nhập mã tài khoản.", "loi"); return; }
  try {
    var r = await apiFetch("/api/admin/catalog/accounts/" + id + "/activity");
    hienThongKe("ds-don-cua-toi", [["Id", "Mã đơn"], ["TongTien", "Tổng tiền", "tien"], ["TrangThai", "Trạng thái"]], r.data.DonVe);
  } catch (error) { thongBao(error.message, "loi"); }
}
async function taiThongKeCheckIn() {
  var id = num("tk-checkin-sk"), host = document.getElementById("tk-checkin");
  if (!host || !id) return;
  try {
    var r = await apiFetch("/api/admin/dashboard/events/" + id + "/check-in");
    hienThongKe("tk-checkin", [["trangThai", "Trạng thái vé"], ["soLuong", "Số lượng"]], r.data);
  } catch (error) { thongBao(error.message, "loi"); }
}
