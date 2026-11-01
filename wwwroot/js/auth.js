"use strict";

async function dangKy() {
  var hoten = val("dk-hoten"), email = val("dk-email");
  var mk = document.getElementById("dk-mk") ? document.getElementById("dk-mk").value : "";
  var lai = document.getElementById("dk-nhaplai") ? document.getElementById("dk-nhaplai").value : "";
  if (!hoten || !email || !mk) { toast("Vui lòng nhập đầy đủ họ tên, email và mật khẩu.", "loi"); return; }
  if (!checkMatKhau(mk, lai, "dk-nhaplai")) return;
  var button = document.getElementById("btn-dangky-form");
  if (button && button.disabled) return;
  if (button) button.disabled = true;
  try {
    await apiFetch("/api/auth/register", "POST", {
      HoTen: hoten,
      Email: email,
      MatKhau: mk
    });
    thongBao("Đăng ký thành công. Vui lòng đăng nhập.");
    hienTrang("dangnhap");
  } catch (e) { thongBao(e.message, "loi"); }
  finally { if (button) button.disabled = false; }
}

async function dangNhap() {
  try {
    var kq = await apiFetch("/api/auth/login", "POST", {
      Email: val("dn-email"),
      MatKhau: document.getElementById("dn-mk") ? document.getElementById("dn-mk").value : ""
    });
    var tk = kq.data || kq;
    var taiKhoan = tk.taiKhoan || tk;
    sessionStorage.setItem("sk_token", tk.token || taiKhoan.token || "");
    sessionStorage.setItem("sk_vaitro", taiKhoan.vaiTro || taiKhoan.TenVaiTro || "");
    sessionStorage.setItem("sk_hoten", taiKhoan.hoTen || taiKhoan.HoTen || "");
    sessionStorage.setItem("sk_tkid", taiKhoan.id || taiKhoan.TaiKhoanID || "");
    thongBao("Đăng nhập thành công: " + (taiKhoan.hoTen || ""));
    if (typeof taiDanhSachSuKien === "function") taiDanhSachSuKien();
    var next = new URLSearchParams(location.search).get("next");
    if (next && next.startsWith("/") && !next.startsWith("//") && !next.includes("\\")) location.assign(next);
    else if (store.vaiTro === "BanToChuc") location.assign("/pages/ban-to-chuc/su-kien-cua-toi.html");
    else if (["QuanLySuKien", "QuanTriVien"].indexOf(store.vaiTro) !== -1) location.assign("/pages/quan-tri/tong-quan.html");
    else hienTrang("sukien");
  } catch (e) { thongBao(e.message, "loi"); }
}

function dangXuat() {
  try { apiFetch("/api/auth/logout", "POST").catch(function () {}); } catch (e) {}
  try { sessionStorage.clear(); } catch (e2) {}
  if (typeof apDungQuyen === "function") apDungQuyen();
  hienTrang("dangnhap");
}

async function doiMatKhau() {
  var cu = document.getElementById("dmk-cu") ? document.getElementById("dmk-cu").value : "";
  var moi = document.getElementById("dmk-moi") ? document.getElementById("dmk-moi").value : "";
  var lai = document.getElementById("dmk-nhaplai") ? document.getElementById("dmk-nhaplai").value : "";
  if (!cu || !moi) { toast("Vui lòng nhập mật khẩu cũ và mật khẩu mới.", "loi"); return; }
  if (!checkMatKhau(moi, lai, "dmk-nhaplai")) return;
  try {
    await apiFetch("/api/auth/change-password", "PUT", { MatKhauCu: cu, MatKhauMoi: moi });
    thongBao("Đã đổi mật khẩu. Vui lòng đăng nhập lại.");
    dangXuat();
  } catch (e) { thongBao(e.message, "loi"); }
}

/* ---- Quan tri tai khoan (giuu ten ham cu de HTML cu van chay) ---- */
async function taiTaiKhoan() {
  var skeletonFn = (typeof skeleton === "function") ? skeleton : null;
  if (skeletonFn) skeletonFn("ds-taikhoan", 4);
  try {
    var tuKhoa = val("qtk-tukhoa") || null;
    var qs = "/api/accounts?page=1&size=50" + (tuKhoa ? "&q=" + encodeURIComponent(tuKhoa) : "");
    var kq = await apiFetch(qs);
    document.getElementById("ds-taikhoan").innerHTML = bangHTML(
      [["HoTen", "Ho ten"], ["Email", "Email"], ["VaiTro", "Vai trò"], ["TrangThai", "Hoat dong"]],
      (kq.data || []).map(function (d) {
        d.TrangThai = d.HoatDong ? "Dang hoat dong" : "Da khoa";
        return d;
      }),
      function (d) {
        return '<button class="phu" onclick="khoaTaiKhoan(' + d.Id + ')">Khoa</button> ' +
          '<button class="phu" onclick="moKhoaTaiKhoan(' + d.Id + ')">Mo</button> ' +
          '<button class="phu" onclick="doiVaiTroTaiKhoan(' + d.Id + ')">Doi vai tro</button>';
      });
  } catch (e) { thongBao(e.message, "loi"); }
}
async function khoaTaiKhoan(id) {
  try { await apiFetch("/api/accounts/" + id + "/lock", "POST"); thongBao("Da khoa tai khoan."); taiTaiKhoan(); }
  catch (e) { thongBao(e.message, "loi"); }
}
async function moKhoaTaiKhoan(id) {
  try { await apiFetch("/api/accounts/" + id + "/unlock", "POST"); thongBao("Da mo khoa."); taiTaiKhoan(); }
  catch (e) { thongBao(e.message, "loi"); }
}
async function doiVaiTroTaiKhoan(id) {
  var lua = prompt("Chon vai tro moi:\n1 = Khach hang\n2 = Ban to chuc\n3 = Quan ly\n4 = Quan tri", "1");
  if (lua === null) return;
  var banDo = { "khach hang": 1, "khachhang": 1, "1": 1, "ban to chuc": 2, "bantchuc": 2, "2": 2, "quan ly": 3, "quanly": 3, "3": 3, "quan tri": 4, "quantri": 4, "4": 4 };
  var n = banDo[String(lua).trim().toLowerCase()];
  if (!n) { toast("Lua chon khong hop le. Chon 1-4.", "loi"); return; }
  try {
    await apiFetch("/api/accounts/" + id + "/role", "PUT", { VaiTroId: n });
    thongBao("Doi vai tro thanh cong."); taiTaiKhoan();
  } catch (e) { thongBao(e.message, "loi"); }
}
async function capNhatTaiKhoan() {
  var hoten = val("qtk-hoten");
  if (!hoten) { toast("Vui lòng nhập họ tên.", "loi"); return; }
  var button = document.getElementById("btn-luu-hoso");
  if (button && button.disabled) return;
  if (button) button.disabled = true;
  try {
    await apiFetch("/api/accounts/me", "PUT", { HoTen: hoten });
    sessionStorage.setItem("sk_hoten", hoten);
    apDungQuyen();
    thongBao("Đã lưu thông tin tài khoản.");
  } catch (e) { thongBao(e.message, "loi"); }
  finally { if (button) button.disabled = false; }
}

async function taiHoSoCaNhan() {
  if (!store.token || !document.getElementById("btn-luu-hoso")) return;
  var button = document.getElementById("btn-luu-hoso");
  if (button) button.disabled = true;
  try {
    var result = await apiFetch("/api/accounts/me");
    var profile = result.data || {};
    [["qtk-id", profile.Id], ["qtk-hoten", profile.HoTen], ["qtk-email", profile.Email]].forEach(function (field) {
      var input = document.getElementById(field[0]);
      if (input) input.value = field[1] || "";
    });
    sessionStorage.setItem("sk_hoten", profile.HoTen || "");
    apDungQuyen();
  } catch (e) { thongBao(e.message, "loi"); }
  finally { if (button) button.disabled = false; }
}
document.addEventListener("DOMContentLoaded", taiHoSoCaNhan);
