"use strict";
(function () {
  var roles = { NguoiDung: "Người dùng", BanToChuc: "Ban tổ chức", QuanLySuKien: "Quản lý sự kiện", QuanTriVien: "Quản trị viên" };
  var routes = { dangnhap: "/pages/auth/dang-nhap.html", sukien: "/pages/su-kien/danh-sach.html", donve: "/pages/don-ve/don-cua-toi.html", quantri: "/pages/quan-tri/tong-quan.html" };
  var originalPage = window.hienTrang;
  window.hienTrang = function (id) {
    if (document.getElementById("trang-" + id)) return originalPage(id);
    if (routes[id] && location.pathname !== routes[id]) location.assign(routes[id]);
  };
  var originalPermissions = window.apDungQuyen;
  window.apDungQuyen = function () {
    if (originalPermissions) originalPermissions();
    var role = store.vaiTro;
    var signedIn = !!store.token && !!role;
    document.body.classList.toggle("signed-in", signedIn);
    [["btn-dangnhap", "btn-dangky", "btn-dangxuat"], ["btn-dangnhap-m", "btn-dangky-m", "btn-dangxuat-m"]].forEach(function (ids) {
      ids.forEach(function (id, index) {
        var node = document.getElementById(id);
        if (node) { node.style.removeProperty("display"); node.hidden = index === 2 ? !signedIn : signedIn; }
      });
    });
    document.querySelectorAll(".rolechip").forEach(function (chip) { chip.textContent = roles[role] || "Thành viên"; });
    var allowed = (document.body.getAttribute("data-allow") || "").split(",").filter(Boolean);
    var permitted = signedIn && (!allowed.length || allowed.indexOf(role) !== -1);
    document.querySelectorAll("[data-khoa]").forEach(function (node) {
      node.hidden = permitted;
      var title = node.querySelector("h3");
      if (title && signedIn && !permitted) title.textContent = "Tài khoản của bạn chưa có quyền truy cập mục này";
    });
    document.querySelectorAll("[data-noi-dung-chinh]").forEach(function (node) { node.hidden = !permitted; node.style.removeProperty("display"); });
    document.querySelectorAll(".mainnav a, .menu-mobile a").forEach(function (link) {
      var target = new URL(link.href).pathname;
      var active = target === location.pathname || (target.endsWith("su-kien/danh-sach.html") && location.pathname === "/");
      link.classList.toggle("active", active);
      if (active) link.setAttribute("aria-current", "page"); else link.removeAttribute("aria-current");
    });
  };
  document.addEventListener("DOMContentLoaded", window.apDungQuyen);
})();
