"use strict";

(function () {
  var nhanHienThiTheoVaiTro = {
    NguoiDung: "Người dùng",
    BanToChuc: "Ban tổ chức",
    QuanLySuKien: "Quản lý sự kiện",
    QuanTriVien: "Quản trị viên",
  };
  var duongDanTheoTrang = {
    dangnhap: "/pages/auth/dang-nhap.html",
    sukien: "/pages/su-kien/danh-sach.html",
    donve: "/pages/don-ve/don-cua-toi.html",
    quantri: "/pages/quan-tri/tong-quan.html",
  };
  var hamHienTrangGoc = window.hienTrangTheoTen;
  window.hienTrangTheoTen = function (tenTrangDich) {
    if (document.getElementById("trang-" + tenTrangDich)) return hamHienTrangGoc(tenTrangDich);
    if (duongDanTheoTrang[tenTrangDich] && location.pathname !== duongDanTheoTrang[tenTrangDich]) location.assign(duongDanTheoTrang[tenTrangDich]);
  };
  var hamApDungQuyenGoc = window.apDungQuyen;
  window.apDungQuyen = function () {
    if (hamApDungQuyenGoc) hamApDungQuyenGoc();
    var vaiTroTaiKhoan = phienDangNhap.vaiTro;
    var daDangNhap = !!phienDangNhap.token && !!vaiTroTaiKhoan;
    document.body.classList.toggle("signed-in", daDangNhap);
    [
      ["btn-dangnhap", "btn-dangky", "btn-dangxuat"],
      ["btn-dangnhap-m", "btn-dangky-m", "btn-dangxuat-m"],
    ].forEach(function (danhSachPhanTuId) {
      danhSachPhanTuId.forEach(function (phanTuDomId, chiSoPhanTu) {
        var phanTuDom = document.getElementById(phanTuDomId);
        if (phanTuDom) {
          phanTuDom.style.removeProperty("display");
          phanTuDom.hidden = chiSoPhanTu === 2 ? !daDangNhap : daDangNhap;
        }
      });
    });
    document.querySelectorAll(".rolechip").forEach(function (nhanVaiTro) {
      nhanVaiTro.textContent = nhanHienThiTheoVaiTro[vaiTroTaiKhoan] || "Thành viên";
    });
    var danhSachVaiTroChoPhep = (document.body.getAttribute("data-allow") || "").split(",").filter(Boolean);
    var coQuyenTruyCap = daDangNhap && (!danhSachVaiTroChoPhep.length || danhSachVaiTroChoPhep.indexOf(vaiTroTaiKhoan) !== -1);
    document.querySelectorAll("[data-khoa]").forEach(function (phanTuDom) {
      phanTuDom.hidden = coQuyenTruyCap;
      var tieuDeTrang = phanTuDom.querySelector("h3");
      if (tieuDeTrang && daDangNhap && !coQuyenTruyCap)
        tieuDeTrang.textContent = "Tài khoản của bạn chưa có quyền truy cập mục này";
    });
    document.querySelectorAll("[data-noi-dung-chinh]").forEach(function (phanTuDom) {
      phanTuDom.hidden = !coQuyenTruyCap;
      phanTuDom.style.removeProperty("display");
    });
    document.querySelectorAll(".mainnav a, .menu-mobile a").forEach(function (lienKetDieuHuong) {
      var duongDanTrangDich = new URL(lienKetDieuHuong.href).pathname;
      var dangHoatDong = duongDanTrangDich === location.pathname || (duongDanTrangDich === "/index.html" && location.pathname === "/");
      lienKetDieuHuong.classList.toggle("active", dangHoatDong);
      if (dangHoatDong) lienKetDieuHuong.setAttribute("aria-current", "page");
      else lienKetDieuHuong.removeAttribute("aria-current");
    });
  };
  document.addEventListener("DOMContentLoaded", window.apDungQuyen);
})();
