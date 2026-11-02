"use strict";

function hienThongKe(idKhungThongKe, cacCotThongKe, cacDongThongKe) {
  var khungThongKe = document.getElementById(idKhungThongKe);
  if (khungThongKe) khungThongKe.innerHTML = taoHtmlBang(cacCotThongKe, cacDongThongKe);
}
async function taiThongKe() {
  var cacIdKhungThongKe = [
    "payment-chart",
    "event-status-chart",
    "ticket-chart",
    "tk-taikhoan",
    "tk-sukien",
    "tk-ve",
    "tk-taikhoan-tt",
    "tk-thanhtoan",
  ];
  cacIdKhungThongKe.forEach(function (idKhungThongKe) {
    hienKhungDangTai(idKhungThongKe, 3);
  });
  try {
    var phanHoiThongKe = await guiYeuCauApi("/api/admin/dashboard"),
      thongKe = phanHoiThongKe.data;
    hienThongKe(
      "tk-taikhoan",
      [
        ["vaiTro", "Vai trò"],
        ["soLuong", "Số lượng"],
      ],
      thongKe.theoVaiTro,
    );
    hienThongKe(
      "tk-sukien",
      [
        ["trangThai", "Trạng thái"],
        ["soLuong", "Số lượng"],
      ],
      thongKe.theoTrangThaiSuKien,
    );
    hienThongKe(
      "tk-ve",
      [
        ["trangThai", "Trạng thái vé"],
        ["soLuong", "Số lượng"],
      ],
      thongKe.theoTrangThaiVe,
    );
    if (typeof giaoDienQuanLySuKien !== "undefined") giaoDienQuanLySuKien.hienTongQuanQuanTri(thongKe);
    hienThongKeTaiKhoan(thongKe);
    hienThongKeThanhToan(thongKe);
    if (document.getElementById("tk-tochuc")) await taiThongKeToChuc();
    if (document.getElementById("tk-dat-tt") || document.getElementById("tk-dat-dd"))
      await taiThongKeDatDiaDiem();
    await taiThongKeCheckIn();
  } catch (loiXuLy) {
    if (typeof giaoDienQuanLySuKien !== "undefined") giaoDienQuanLySuKien.hienLoiTaiTongQuan();
    cacIdKhungThongKe.forEach(function (idKhungThongKe) {
      hienLoiTai(idKhungThongKe, loiXuLy, taiThongKe);
    });
    thongBao(loiXuLy.message, "loi");
  }
}
function hienThongKeTaiKhoan(thongKe) {
  hienThongKe(
    "tk-taikhoan-tt",
    [
      ["TrangThai", "Trạng thái"],
      ["SoLuong", "Số lượng"],
    ],
    [
      {
        TrangThai: "Hoạt động",
        SoLuong: thongKe.taiKhoanHoatDong,
      },
      {
        TrangThai: "Đã khóa",
        SoLuong: thongKe.taiKhoan - thongKe.taiKhoanHoatDong,
      },
    ],
  );
}
function hienThongKeThanhToan(thongKe) {
  hienThongKe(
    "tk-thanhtoan",
    [
      ["trangThai", "Trạng thái"],
      ["soLuong", "Số giao dịch"],
      ["soTien", "Số tiền", "tien"],
      ["soTienHoan", "Đã hoàn", "tien"],
    ],
    thongKe.theoTrangThaiThanhToan,
  );
}
async function taiSuKienCuaTaiKhoan() {
  var taiKhoanId = docSoTuInput("qtk-sukien-id") || docSoTuInput("qtk-id");
  if (!taiKhoanId) {
    hienThongBaoNoi("Nhập mã tài khoản.", "loi");
    return;
  }
  hienKhungDangTai("ds-sukien-cua-toi", 3);
  try {
    var phanHoiHoatDong = await guiYeuCauApi("/api/admin/catalog/accounts/" + taiKhoanId + "/activity");
    hienThongKe(
      "ds-sukien-cua-toi",
      [
        ["Ten", "Sự kiện"],
        ["TrangThaiTen", "Trạng thái"],
      ],
      phanHoiHoatDong.data.SuKien.map(function (suKien) {
        return {
          Ten: suKien.Ten,
          TrangThaiTen: trangThaiSuKienHienThi(suKien.TrangThai),
        };
      }),
    );
  } catch (loiXuLy) {
    hienLoiTai("ds-sukien-cua-toi", loiXuLy, taiSuKienCuaTaiKhoan);
  }
}
async function taiDonVeCuaTaiKhoan() {
  var taiKhoanId = docSoTuInput("qtk-donve-id") || docSoTuInput("qtk-id");
  if (!taiKhoanId) {
    hienThongBaoNoi("Nhập mã tài khoản.", "loi");
    return;
  }
  hienKhungDangTai("ds-don-cua-toi", 3);
  try {
    var phanHoiHoatDong = await guiYeuCauApi("/api/admin/catalog/accounts/" + taiKhoanId + "/activity");
    hienThongKe(
      "ds-don-cua-toi",
      [
        ["Id", "Mã đơn"],
        ["TongTien", "Tổng tiền", "tien"],
        ["TrangThai", "Trạng thái"],
      ],
      phanHoiHoatDong.data.DonVe.map(function (donVe) {
        return Object.assign({}, donVe, {
          TrangThai: nhanTrangThaiDon(donVe.TrangThai),
        });
      }),
    );
  } catch (loiXuLy) {
    hienLoiTai("ds-don-cua-toi", loiXuLy, taiDonVeCuaTaiKhoan);
  }
}
async function taiThongKeCheckIn() {
  var suKienId = docSoTuInput("tk-checkin-sk"),
    khungThongKeCheckIn = document.getElementById("tk-checkin");
  if (!khungThongKeCheckIn || !suKienId) return;
  hienKhungDangTai("tk-checkin", 3);
  try {
    var phanHoiThongKeCheckIn = await guiYeuCauApi("/api/admin/dashboard/events/" + suKienId + "/check-in");
    hienThongKe(
      "tk-checkin",
      [
        ["trangThai", "Trạng thái vé"],
        ["soLuong", "Số lượng"],
      ],
      phanHoiThongKeCheckIn.data,
    );
  } catch (loiXuLy) {
    hienLoiTai("tk-checkin", loiXuLy, taiThongKeCheckIn);
  }
}
