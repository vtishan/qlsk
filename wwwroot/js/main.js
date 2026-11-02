/* ===== main.js — dieu huong trang + phan quyen + khoi tao ===== */
"use strict";

/* global phienDangNhap, taiDanhSachSuKien, taiLoaiSuKien */
var __trangHienTai = "dangnhap";
function hienTrangTheoTen(tenTrang) {
  __trangHienTai = tenTrang;
  var tongSoTrang = document.querySelectorAll(".page, main section, .shell section");
  tongSoTrang.forEach(function (khungTrang) {
    khungTrang.classList.remove("hien");
  });
  var khungTrang = document.getElementById("trang-" + tenTrang);
  if (khungTrang) khungTrang.classList.add("hien");
  document.querySelectorAll(".mainnav button[data-trang]").forEach(function (nutDieuHuong) {
    nutDieuHuong.classList.toggle("active", nutDieuHuong.getAttribute("data-trang") === tenTrang);
  });
  apDungQuyen();
  if (tenTrang === "quantri" && typeof hienDanhSachDuyetToChuc === "function") {
    try {
      hienDanhSachDuyetToChuc();
    } catch (loiXuLy) {}
  }
  window.scrollTo({
    top: 0,
    behavior: "smooth",
  });
}
function apDungQuyen() {
  var vaiTro = "";
  try {
    vaiTro = sessionStorage.getItem("sk_vaitro") || "";
  } catch (loiXuLy) {}
  var hoTen = "";
  try {
    hoTen = sessionStorage.getItem("sk_hoten") || "";
  } catch (loiXuLy) {}
  hoTen = chuyenGiaTriThanhVanBan(hoTen);
  var khungTaiKhoan = document.getElementById("nd-user");
  if (khungTaiKhoan) {
    if (hoTen) {
      khungTaiKhoan.innerHTML = "";
      var nhanTaiKhoan = document.createElement("b");
      nhanTaiKhoan.textContent = hoTen;
      khungTaiKhoan.appendChild(nhanTaiKhoan);
      if (vaiTro) {
        var nhanVaiTro = document.createElement("span");
        nhanVaiTro.className = "rolechip";
        nhanVaiTro.textContent = "Thành viên";
        khungTaiKhoan.appendChild(nhanVaiTro);
      }
    } else {
      khungTaiKhoan.textContent = "";
    }
  }
  var nutDangXuatMayTinh = document.getElementById("btn-dangxuat");
  if (nutDangXuatMayTinh) nutDangXuatMayTinh.style.display = vaiTro ? "" : "none";
  var nutDangXuatDiDong = document.getElementById("btn-dangxuat-m");
  if (nutDangXuatDiDong) nutDangXuatDiDong.style.display = vaiTro ? "" : "none";
  var anhDaiDien = document.getElementById("avatar-chu");
  if (anhDaiDien) {
    var tenHienThi = (hoTen || "").trim();
    if (tenHienThi) {
      var cacPhanVanBan = tenHienThi.split(/\s+/);
      var chuCaiDau =
        cacPhanVanBan.length > 1 ? cacPhanVanBan[0].charAt(0) + cacPhanVanBan[cacPhanVanBan.length - 1].charAt(0) : tenHienThi.slice(0, 2);
      anhDaiDien.textContent = chuCaiDau.toUpperCase();
    } else {
      anhDaiDien.textContent = "SK";
    }
  }
  /* P0: an nav han che khi chua dang nhap — chi hien khi co vai tro va nam trong danh sach */
  document.querySelectorAll("[data-quyen]").forEach(function (phanTuPhanQuyen) {
    var vaiTroChoPhep = (phanTuPhanQuyen.getAttribute("data-quyen") || "")
      .split(",")
      .map(function (vaiTroChoPhep) {
        return vaiTroChoPhep.trim();
      });
    var coQuyenTruyCap = !!vaiTro && vaiTroChoPhep.indexOf(vaiTro) !== -1;
    phanTuPhanQuyen.style.display = coQuyenTruyCap ? "" : "none";
  });
}
function khoiTaoTuongTacTrang() {
  ganEnterDangNhap();
  dongBoOTimKiem();
  khoiTaoMenuMobile();
  ganBoLocChuDe();
}
function ganEnterDangNhap() {
  var oMatKhauDangNhap = document.getElementById("dn-mk");
  if (oMatKhauDangNhap && !oMatKhauDangNhap.form)
    oMatKhauDangNhap.addEventListener("keydown", function (suKienDom) {
      if (suKienDom.key === "Enter" && !suKienDom.isComposing && typeof dangNhapTaiKhoan === "function") {
        suKienDom.preventDefault();
        dangNhapTaiKhoan();
      }
    });
}
function dongBoOTimKiem() {
  /* Dong bo 3 o tim kiem: header, hero, trang su kien */
  var danhSachPhanTuId = ["sk-tukhoa", "sk-tukhoa-hero", "sk-tukhoa-page"];
  danhSachPhanTuId.forEach(function (idOTimKiemThayDoi) {
    var oTimKiem = document.getElementById(idOTimKiemThayDoi);
    if (!oTimKiem) return;
    oTimKiem.addEventListener("input", function () {
      danhSachPhanTuId.forEach(function (idOTimKiem) {
        if (idOTimKiem === idOTimKiemThayDoi) return;
        var oTimKiemKhac = document.getElementById(idOTimKiem);
        if (oTimKiemKhac && oTimKiemKhac.value !== oTimKiem.value) oTimKiemKhac.value = oTimKiem.value;
      });
    });
    oTimKiem.addEventListener("keydown", function (suKienDom) {
      if (
        suKienDom.key === "Enter" &&
        !suKienDom.isComposing &&
        !oTimKiem.form &&
        typeof taiDanhSachSuKien === "function"
      ) {
        suKienDom.preventDefault();
        taiDanhSachSuKien();
      }
    });
  });
}
function khoiTaoMenuMobile() {
  /* Menu co gon mobile */
  var nutMenu = document.getElementById("btn-menu");
  var menuDieuHuong = document.getElementById("menu-mobile");
  if (nutMenu && menuDieuHuong) {
    var capNhat = function () {
      if (window.innerWidth > 1150) dongMenuDieuHuong();
    };
    function dongMenuDieuHuong() {
      menuDieuHuong.hidden = true;
      nutMenu.setAttribute("aria-expanded", "false");
      nutMenu.setAttribute("aria-label", "Mở menu");
    }
    nutMenu.onclick = function () {
      menuDieuHuong.hidden = !menuDieuHuong.hidden;
      nutMenu.setAttribute("aria-expanded", String(!menuDieuHuong.hidden));
      nutMenu.setAttribute("aria-label", menuDieuHuong.hidden ? "Mở menu" : "Đóng menu");
    };
    menuDieuHuong.addEventListener("click", function (suKienDom) {
      if (suKienDom.target.closest("a")) dongMenuDieuHuong();
    });
    document.addEventListener("keydown", function (suKienDom) {
      if (suKienDom.key === "Escape" && !menuDieuHuong.hidden) {
        dongMenuDieuHuong();
        nutMenu.focus();
      }
    });
    document.addEventListener("click", function (suKienDom) {
      if (!menuDieuHuong.hidden && !menuDieuHuong.contains(suKienDom.target) && !nutMenu.contains(suKienDom.target)) dongMenuDieuHuong();
    });
    window.addEventListener("resize", capNhat);
    capNhat();
  }
}
function ganBoLocChuDe() {
  /* Tags nhanh hero -> tim kiem */
  Array.prototype.forEach.call(document.querySelectorAll("button[data-category]"), function (nutChuDe) {
    nutChuDe.addEventListener("click", function () {
      Array.prototype.forEach.call(document.querySelectorAll(".hero-tags .catchip"), function (nutChuDeKhac) {
        nutChuDeKhac.classList.remove("active");
      });
      nutChuDe.classList.add("active");
      if (typeof locLoaiSuKien === "function") locLoaiSuKien(nutChuDe.getAttribute("data-category"));
    });
  });
}

/* Khu vuc duyet/tu choi yeu cau to chuc trong trang Quan tri (P1) — goi API thuc. */
async function hienDanhSachDuyetToChuc() {
  var khungYeuCauXacThuc = document.getElementById("ds-duyet-tochuc");
  if (!khungYeuCauXacThuc) return;
  hienKhungDangTai("ds-duyet-tochuc", 3);
  try {
    var phanHoiYeuCau = await guiYeuCauApi("/api/organizer-verifications");
    var yeuCauChoDuyet = (phanHoiYeuCau.data || []).filter(function (yeuCauXacThuc) {
      return yeuCauXacThuc.TrangThai === 0 || yeuCauXacThuc.TrangThai === "ChoDuyet";
    });
    if (typeof hienThongKeToChuc === "function") hienThongKeToChuc(phanHoiYeuCau.data || []);
    if (typeof giaoDienQuanLySuKien !== "undefined") {
      khungYeuCauXacThuc.innerHTML = giaoDienQuanLySuKien.hienYeuCauXacThuc(yeuCauChoDuyet);
      return;
    }
    khungYeuCauXacThuc.innerHTML =
      '<div class="muted">Co ' +
      yeuCauChoDuyet.length +
      " yeu cau dang chờ duyet.</div>" +
      '<div class="toolbar">' +
      "<button class=\"chinh sm\" onclick=\"taiDanhSachDuyetToChuc()\">Tai lai danh sach</button> " +
      '<button class="phu sm" onclick="taiThongKeToChuc()">Thong ke</button></div>' +
      '<p class="muted" style="margin:0">Chọn mã yeu cau va bam duyet/tu choi.</p>' +
      '<div class="row"><input id="dtc-id" type="number" placeholder="Mã yeu cau">' +
      '<input id="dtc-ten" placeholder="Ten to chuc (khi duyet)">' +
      '<input id="dtc-lydo" placeholder="Ly do (khi tu choi)"></div>' +
      '<div class="toolbar" style="margin-top:6px"><button class="chinh sm" onclick="duyetToChucNhanh()">Duyet yeu cau</button> ' +
      '<button class="phu sm" onclick="tuChoiToChucNhanh()">Tu choi yeu cau</button></div>';
  } catch (loiXuLy) {
    hienLoiTai("ds-duyet-tochuc", loiXuLy, hienDanhSachDuyetToChuc);
  }
}
async function taiDanhSachDuyetToChuc() {
  hienDanhSachDuyetToChuc();
}
async function duyetToChucNhanh() {
  var yeuCauXacThucId = docSoTuInput("dtc-id");
  if (!yeuCauXacThucId || yeuCauXacThucId <= 0) {
    hienThongBaoNoi("Nhập mã yeu cau.", "loi");
    return;
  }
  try {
    await guiYeuCauApi("/api/organizer-verifications/" + yeuCauXacThucId + "/approve", "POST", null);
    thongBao("Đã duyệt yêu cầu.");
    hienDanhSachDuyetToChuc();
    if (typeof taiThongKeToChuc === "function") taiThongKeToChuc();
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  }
}
async function tuChoiToChucNhanh() {
  var yeuCauXacThucId = docSoTuInput("dtc-id");
  var lyDoTuChoi = docGiaTriInput("dtc-lydo");
  if (!yeuCauXacThucId || yeuCauXacThucId <= 0) {
    hienThongBaoNoi("Nhập mã yeu cau.", "loi");
    return;
  }
  if (!lyDoTuChoi) {
    hienThongBaoNoi("Nhập lý do từ chối.", "loi");
    return;
  }
  try {
    await guiYeuCauApi("/api/organizer-verifications/" + yeuCauXacThucId + "/reject", "POST", {
      LyDo: lyDoTuChoi,
    });
    thongBao("Đã từ chối yêu cầu.");
    hienDanhSachDuyetToChuc();
    if (typeof taiThongKeToChuc === "function") taiThongKeToChuc();
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  }
}
window.addEventListener("DOMContentLoaded", function () {
  apDungQuyen();
  khoiTaoTuongTacTrang();
  var thamSoUrl = new URLSearchParams(location.search);
  if (thamSoUrl.has("q")) {
    ["sk-tukhoa", "sk-tukhoa-page"].forEach(function (idTruongNhap) {
      var oTruongNhap = document.getElementById(idTruongNhap);
      if (oTruongNhap) oTruongNhap.value = thamSoUrl.get("q");
    });
  }
  /* Tu dong nap su kien cong khai tren cac trang co danh sach. */
  try {
    if (document.getElementById("ds-sukien") && typeof taiDanhSachSuKien === "function") {
      taiDanhSachSuKien();
    }
  } catch (loiXuLy) {}
  if (typeof taiLoaiSuKien === "function") {
    /* preload loai su kien cho select tao su kien (can token) */
    try {
      if (
        document.querySelector("[data-event-categories]") ||
        (sessionStorage.getItem("sk_token") &&
          (document.getElementById("sk-loai") || document.getElementById("ds-loai")))
      )
        taiLoaiSuKien();
    } catch (loiXuLy) {}
  }
});
