/* ===== main.js — dieu huong trang + phan quyen + khoi tao ===== */
"use strict";
/* global store, taiDanhSachSuKien, taiLoaiSuKien */

var __trangHienTai = "dangnhap";

function hienTrang(id) {
  __trangHienTai = id;
  var pages = document.querySelectorAll(".page, main section, .shell section");
  pages.forEach(function (s) { s.classList.remove("hien"); });
  var el = document.getElementById("trang-" + id);
  if (el) el.classList.add("hien");
  document.querySelectorAll(".mainnav button[data-trang]").forEach(function (b) {
    b.classList.toggle("active", b.getAttribute("data-trang") === id);
  });
  apDungQuyen();
  if (id === "quantri" && typeof renderDuyetToChuc === "function") {
    try { renderDuyetToChuc(); } catch (e) {}
  }
  window.scrollTo({ top: 0, behavior: "smooth" });
}

function apDungQuyen() {
  var vt = "";
  try { vt = sessionStorage.getItem("sk_vaitro") || ""; } catch (e) {}
  var hoten = "";
  try { hoten = sessionStorage.getItem("sk_hoten") || ""; } catch (e) {}
  hoten = noiDungGiaoDien(hoten);
  var nd = document.getElementById("nd-user");
  if (nd) {
    if (hoten) {
      nd.innerHTML = "";
      var b = document.createElement("b");
      b.textContent = hoten;
      nd.appendChild(b);
      if (vt) {
        var chip = document.createElement("span");
        chip.className = "rolechip";
        chip.textContent = "Thành viên";
        nd.appendChild(chip);
      }
    } else {
      nd.textContent = "";
    }
  }
  var btnOut = document.getElementById("btn-dangxuat");
  if (btnOut) btnOut.style.display = vt ? "" : "none";
  var btnOutM = document.getElementById("btn-dangxuat-m");
  if (btnOutM) btnOutM.style.display = vt ? "" : "none";
  var av = document.getElementById("avatar-chu");
  if (av) {
    var ten = (hoten || "").trim();
    if (ten) {
      var parts = ten.split(/\s+/);
      var init = parts.length > 1
        ? (parts[0].charAt(0) + parts[parts.length - 1].charAt(0))
        : ten.slice(0, 2);
      av.textContent = init.toUpperCase();
    } else {
      av.textContent = "SK";
    }
  }
  /* P0: an nav han che khi chua dang nhap — chi hien khi co vai tro va nam trong danh sach */
  document.querySelectorAll("[data-quyen]").forEach(function (el) {
    var ds = (el.getAttribute("data-quyen") || "").split(",").map(function (s) { return s.trim(); });
    var ok = !!vt && ds.indexOf(vt) !== -1;
    el.style.display = ok ? "" : "none";
  });
}

function ganEnterSubmit() {
  var dn = document.getElementById("dn-mk");
  if (dn) dn.addEventListener("keydown", function (e) {
    if (e.key === "Enter" && typeof dangNhap === "function") dangNhap();
  });
  /* Dong bo 3 o tim kiem: header, hero, trang su kien */
  var ids = ["sk-tukhoa", "sk-tukhoa-hero", "sk-tukhoa-page"];
  ids.forEach(function (id) {
    var el = document.getElementById(id);
    if (!el) return;
    el.addEventListener("input", function () {
      ids.forEach(function (o) {
        if (o === id) return;
        var t = document.getElementById(o);
        if (t && t.value !== el.value) t.value = el.value;
      });
    });
    el.addEventListener("keydown", function (e) {
      if (e.key === "Enter" && typeof taiDanhSachSuKien === "function") taiDanhSachSuKien();
    });
  });
  /* Menu co gon mobile */
  var btn = document.getElementById("btn-menu");
  var menu = document.getElementById("menu-mobile");
  if (btn && menu) {
    var capNhat = function () {
      if (window.innerWidth > 1150) dongMenu();
    };
    function dongMenu() {
      menu.hidden = true;
      btn.setAttribute("aria-expanded", "false");
      btn.setAttribute("aria-label", "Mở menu");
    }
    btn.onclick = function () {
      menu.hidden = !menu.hidden;
      btn.setAttribute("aria-expanded", String(!menu.hidden));
      btn.setAttribute("aria-label", menu.hidden ? "Mở menu" : "Đóng menu");
    };
    menu.addEventListener("click", function (event) { if (event.target.closest("a")) dongMenu(); });
    document.addEventListener("keydown", function (event) {
      if (event.key === "Escape" && !menu.hidden) { dongMenu(); btn.focus(); }
    });
    document.addEventListener("click", function (event) {
      if (!menu.hidden && !menu.contains(event.target) && !btn.contains(event.target)) dongMenu();
    });
    window.addEventListener("resize", capNhat);
    capNhat();
  }
  /* Tags nhanh hero -> tim kiem */
  Array.prototype.forEach.call(document.querySelectorAll("button[data-category]"), function (c) {
    c.addEventListener("click", function () {
      Array.prototype.forEach.call(document.querySelectorAll(".hero-tags .catchip"), function (x) { x.classList.remove("active"); });
      c.classList.add("active");
      if (typeof locLoaiSuKien === "function") locLoaiSuKien(c.getAttribute("data-category"));
    });
  });
}

/* Khu vuc duyet/tu choi yeu cau to chuc trong trang Quan tri (P1) — goi API thuc. */
async function renderDuyetToChuc() {
  var host = document.getElementById("ds-duyet-tochuc");
  if (!host) return;
  try {
    var kq = await apiFetch("/api/organizer-verifications");
    var rows = (kq.data || []).filter(function (r) { return (r.TrangThai === 0 || r.TrangThai === "ChoDuyet"); });
    host.innerHTML = '<div class="muted">Co ' + rows.length + " yeu cau dang chờ duyet.</div>" +
      '<div class="toolbar">' +
      '<button class="chinh sm" onclick="taiDuyetToChuc()">Tai lai danh sach</button> ' +
      '<button class="phu sm" onclick="taiThongKeToChuc()">Thong ke</button></div>' +
      '<p class="muted" style="margin:0">Chọn mã yeu cau va bam duyet/tu choi.</p>' +
      '<div class="row"><input id="dtc-id" type="number" placeholder="Mã yeu cau">' +
      '<input id="dtc-ten" placeholder="Ten to chuc (khi duyet)">' +
      '<input id="dtc-lydo" placeholder="Ly do (khi tu choi)"></div>' +
      '<div class="toolbar" style="margin-top:6px"><button class="chinh sm" onclick="duyetToChucNhanh()">Duyet yeu cau</button> ' +
      '<button class="phu sm" onclick="tuChoiToChucNhanh()">Tu choi yeu cau</button></div>';
  } catch (e) {
    host.innerHTML = '<div class="row"><input id="dtc-id" type="number" placeholder="Mã yeu cau">' +
      '<input id="dtc-ten" placeholder="Ten to chuc (khi duyet)">' +
      '<input id="dtc-lydo" placeholder="Ly do (khi tu choi)"></div>' +
      '<div class="toolbar"><button class="chinh sm" onclick="duyetToChucNhanh()">Duyet yeu cau</button> ' +
      '<button class="phu sm" onclick="tuChoiToChucNhanh()">Tu choi yeu cau</button></div>';
  }
}

async function taiDuyetToChuc() {
  renderDuyetToChuc();
}

async function duyetToChucNhanh() {
  var id = num("dtc-id");
  if (!id || id <= 0) { toast("Nhập mã yeu cau.", "loi"); return; }
  try {
    await apiFetch("/api/organizer-verifications/" + id + "/approve", "POST", null);
    thongBao("Da duyet yeu cau.");
    renderDuyetToChuc();
    if (typeof taiThongKeToChuc === "function") taiThongKeToChuc();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function tuChoiToChucNhanh() {
  var id = num("dtc-id");
  var lydo = val("dtc-lydo");
  if (!id || id <= 0) { toast("Nhập mã yeu cau.", "loi"); return; }
  if (!lydo) { toast("Nhap ly do tu choi.", "loi"); return; }
  try {
    await apiFetch("/api/organizer-verifications/" + id + "/reject", "POST", { LyDo: lydo });
    thongBao("Da tu choi yeu cau.");
    renderDuyetToChuc();
    if (typeof taiThongKeToChuc === "function") taiThongKeToChuc();
  } catch (e) { thongBao(e.message, "loi"); }
}

window.addEventListener("DOMContentLoaded", function () {
  apDungQuyen();
  ganEnterSubmit();
  var params = new URLSearchParams(location.search);
  if (params.has("q")) {
    ["sk-tukhoa", "sk-tukhoa-page"].forEach(function (id) { var field = document.getElementById(id); if (field) field.value = params.get("q"); });
  }
  /* Tu dong nap su kien cong khai tren cac trang co danh sach. */
  try {
    if (document.getElementById("ds-sukien") && typeof taiDanhSachSuKien === "function") {
      taiDanhSachSuKien();
    }
  } catch (e) {}
  if (typeof taiLoaiSuKien === "function") {
    /* preload loai su kien cho select tao su kien (can token) */
    try {
      if (document.querySelector("[data-event-categories]") || (sessionStorage.getItem("sk_token") && (document.getElementById("sk-loai") || document.getElementById("ds-loai")))) taiLoaiSuKien();
    } catch (e) {}
  }
});
