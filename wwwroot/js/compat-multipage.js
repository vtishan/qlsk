/* Compat da-trang cho JS cu (KHONG sua 9 file JS cu).
   - hienTrang("sukien"/"dangnhap"/"donve"/"quantri") -> chuyen location.href khi khong co section tuong ung.
   - Chan redirect lan dau khi main.js tu goi hienTrang("dangnhap") luc tai trang.
   - Ho tro .mainnav a[data-trang] (cu chi ho tro button). */
"use strict";
(function () {
  window.__MPA_SUSPEND = true;
  window.addEventListener("load", function () {
    setTimeout(function () { window.__MPA_SUSPEND = false; }, 600);
  });
  setTimeout(function () { window.__MPA_SUSPEND = false; }, 2500);

  function siteRoot() {
    try {
      var p = location.pathname.replace(/\\/g, "/");
      var i = p.indexOf("/pages/");
      if (i !== -1) return p.slice(0, i);
      var j = p.lastIndexOf("/");
      return j > 0 ? p.slice(0, j) : "";
    } catch (e) { return ""; }
  }
  var MAP = {
    dangnhap: "/pages/auth/dang-nhap.html",
    sukien: "/pages/su-kien/danh-sach.html",
    donve: "/pages/don-ve/don-cua-toi.html",
    quantri: "/pages/quan-tri/tong-quan.html"
  };
  function goFile(rel) {
    var r = siteRoot();
    var url = r + rel;
    if (location.pathname.replace(/\\/g, "/") !== url) location.href = url;
  }
  if (typeof hienTrang === "function") {
    var __origHienTrang = hienTrang;
    window.hienTrang = function (id) {
      var sec = null;
      try { sec = document.getElementById("trang-" + id); } catch (e) {}
      if (sec) { return __origHienTrang(id); }
      try { __origHienTrang(id); } catch (e) {}
      if (window.__MPA_SUSPEND) return;
      if (MAP[id]) goFile(MAP[id]);
      // active cho <a>
      try {
        document.querySelectorAll(".mainnav a[data-trang],.menu-mobile a[data-trang]").forEach(function (a) {
          a.classList.toggle("active", a.getAttribute("data-trang") === id);
        });
      } catch (e) {}
    };
  }
  // mo rong apDungQuyen cho the <a> + header 2 trang thai + lock-card
  if (typeof apDungQuyen === "function") {
    var __origQ = apDungQuyen;
    var TEN_VT = {
      NguoiDung: "Người dùng", BanToChuc: "Ban tổ chức",
      QuanLySuKien: "Quản lý", QuanTriVien: "Quản trị viên"
    };
    window.apDungQuyen = function () {
      __origQ();
      try {
        var vt = ""; try { vt = sessionStorage.getItem("sk_vaitro") || ""; } catch (e) {}
        var daDangNhap = !!vt;
        var id = (typeof __trangHienTai !== "undefined") ? __trangHienTai : "";
        document.querySelectorAll(".mainnav a[data-trang],.menu-mobile a[data-trang]").forEach(function (a) {
          if (a.getAttribute("data-trang") === id) a.classList.add("active");
          else a.classList.remove("active");
        });
        // an hien khoi data-quyen (giong cu, ap dung moi the)
        document.querySelectorAll("[data-quyen]").forEach(function (el) {
          var ds = (el.getAttribute("data-quyen") || "").split(",").map(function (s) { return s.trim(); });
          var ok = !!vt && ds.indexOf(vt) !== -1;
          el.style.display = ok ? "" : "none";
        });
        // Ten vai tro tieng Viet tren header
        try {
          document.querySelectorAll(".rolechip").forEach(function (chip) {
            var raw = (chip.textContent || "").trim();
            if (TEN_VT[raw]) chip.textContent = TEN_VT[raw];
          });
        } catch (e2) {}
        // Header 2 trang thai: chua dang nhap -> hien Dang nhap/Dang ky, an Dang xuat
        [["btn-dangnhap", "btn-dangky", "btn-dangxuat"], ["btn-dangnhap-m", "btn-dangky-m", "btn-dangxuat-m"]].forEach(function (bo) {
          var nutVao = document.getElementById(bo[0]);
          var nutKy = document.getElementById(bo[1]);
          var nutRa = document.getElementById(bo[2]);
          if (nutVao) nutVao.style.display = daDangNhap ? "none" : "";
          if (nutKy) nutKy.style.display = daDangNhap ? "none" : "";
          if (nutRa) nutRa.style.display = daDangNhap ? "" : "none";
        });
        try {
          var chao = document.getElementById("nd-user");
          if (chao && !daDangNhap && !chao.querySelector("b")) chao.textContent = "Chào bạn, hãy đăng nhập";
        } catch (e3) {}
        // Lock-card: chua dang nhap -> hien khoa, an noi dung chinh
        try {
          document.querySelectorAll("[data-khoa]").forEach(function (k) {
            k.hidden = daDangNhap;
          });
          document.querySelectorAll("[data-noi-dung-chinh]").forEach(function (n) {
            if (!daDangNhap) n.style.display = "none";
            else n.style.display = "";
          });
        } catch (e4) {}
      } catch (e) {}
    };
    try { document.addEventListener("DOMContentLoaded", function () { try { window.apDungQuyen(); } catch (e) {} }); } catch (e5) {}
  }
  // Guard vai tro tuy chon: <body data-allow="QuanLySuKien,QuanTriVien">
  document.addEventListener("DOMContentLoaded", function () {
    try {
      var allow = (document.body.getAttribute("data-allow") || "").split(",").map(function (s) { return s.trim(); }).filter(Boolean);
      if (!allow.length) return;
      var vt = ""; try { vt = sessionStorage.getItem("sk_vaitro") || ""; } catch (e) {}
      if (!vt || allow.indexOf(vt) === -1) {
        var w = document.getElementById("tb-chung");
        if (w) { w.className = "thongbao loi"; w.style.display = "block"; w.textContent = "Trang nay yeu cau quyen: " + allow.join(", ") + ". Vui long dang nhap dung vai tro."; }
      }
    } catch (e) {}
  });
})();
