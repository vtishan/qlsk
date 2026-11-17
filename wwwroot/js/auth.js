"use strict";

function khoiTaoNutHienMatKhau() {
  document.querySelectorAll('input[type="password"]').forEach(function (oMatKhau) {
    if (oMatKhau.closest(".password-field")) return;
    var nhanMatKhau = oMatKhau.labels && oMatKhau.labels[0];
    var tenTruong = nhanMatKhau ? nhanMatKhau.textContent.trim().toLocaleLowerCase("vi") : "mật khẩu";
    var khungMatKhau = document.createElement("span");
    khungMatKhau.className = "password-field";
    oMatKhau.before(khungMatKhau);
    khungMatKhau.appendChild(oMatKhau);
    var nutHienMatKhau = document.createElement("button");
    nutHienMatKhau.type = "button";
    nutHienMatKhau.className = "password-toggle";
    nutHienMatKhau.setAttribute("aria-controls", oMatKhau.id);
    function capNhatTrangThaiMatKhau(hienMatKhau) {
      oMatKhau.type = hienMatKhau ? "text" : "password";
      var nhanThaoTac = (hienMatKhau ? "Ẩn " : "Hiện ") + tenTruong;
      nutHienMatKhau.setAttribute("aria-label", nhanThaoTac);
      nutHienMatKhau.title = nhanThaoTac;
      nutHienMatKhau.innerHTML =
        '<svg class="icon" aria-hidden="true" viewBox="0 0 24 24"><path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12Z"/><circle cx="12" cy="12" r="3"/>' +
        (hienMatKhau ? '<path d="m3 3 18 18"/>' : "") + "</svg>";
    }
    nutHienMatKhau.addEventListener("click", function () {
      capNhatTrangThaiMatKhau(oMatKhau.type === "password");
    });
    khungMatKhau.appendChild(nutHienMatKhau);
    capNhatTrangThaiMatKhau(false);
    if (oMatKhau.form) oMatKhau.form.addEventListener("reset", function () {
      capNhatTrangThaiMatKhau(false);
    });
    window.addEventListener("pageshow", function () {
      capNhatTrangThaiMatKhau(false);
    });
  });
  [["dk-mk", "dk-nhaplai"], ["dmk-moi", "dmk-nhaplai"]].forEach(function (capMatKhau) {
    var oMatKhau = document.getElementById(capMatKhau[0]);
    var oNhapLai = document.getElementById(capMatKhau[1]);
    if (!oMatKhau || !oNhapLai || oNhapLai.dataset.passwordValidation) return;
    oNhapLai.dataset.passwordValidation = "true";
    function capNhatLoiNhapLai() {
      if (oNhapLai.getAttribute("aria-invalid") === "true" && oMatKhau.value === oNhapLai.value)
        hienLoiNhapLieu(oNhapLai.id, "");
    }
    oMatKhau.addEventListener("input", capNhatLoiNhapLai);
    oNhapLai.addEventListener("input", capNhatLoiNhapLai);
  });
}
document.addEventListener("DOMContentLoaded", khoiTaoNutHienMatKhau);

async function dangKyTaiKhoan() {
  var hoTen = docGiaTriInput("dk-hoten"),
    email = docGiaTriInput("dk-email");
  var matKhau = document.getElementById("dk-mk") ? document.getElementById("dk-mk").value : "";
  var matKhauNhapLai = document.getElementById("dk-nhaplai")
    ? document.getElementById("dk-nhaplai").value
    : "";
  if (!hoTen || !email || !matKhau) {
    hienThongBaoNoi("Vui lòng nhập đầy đủ họ tên, email và mật khẩu.", "loi");
    return;
  }
  if (!kiemTraMatKhauForm(matKhau, matKhauNhapLai, "dk-nhaplai")) return;
  var nutDangKy = document.getElementById("btn-dangky-form");
  if (nutDangKy && nutDangKy.disabled) return;
  if (nutDangKy) nutDangKy.disabled = true;
  try {
    await guiYeuCauApi("/api/auth/register", "POST", {
      HoTen: hoTen,
      Email: email,
      MatKhau: matKhau,
    });
    thongBao("Đăng ký thành công. Vui lòng đăng nhập.");
    hienTrangTheoTen("dangnhap");
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  } finally {
    if (nutDangKy) nutDangKy.disabled = false;
  }
}
async function dangNhapTaiKhoan() {
  var nutDangNhap = document.getElementById("btn-dangnhap-form");
  if (dangNhapTaiKhoan.dangGui) return;
  dangNhapTaiKhoan.dangGui = true;
  if (nutDangNhap) {
    nutDangNhap.disabled = true;
    nutDangNhap.setAttribute("aria-busy", "true");
  }
  try {
    var ketQuaDangNhap = await guiYeuCauApi("/api/auth/login", "POST", {
      Email: docGiaTriInput("dn-email"),
      MatKhau: document.getElementById("dn-mk") ? document.getElementById("dn-mk").value : "",
    });
    var phanHoiDangNhap = ketQuaDangNhap.data || ketQuaDangNhap;
    var taiKhoan = phanHoiDangNhap.taiKhoan || phanHoiDangNhap;
    sessionStorage.setItem("sk_token", phanHoiDangNhap.token || taiKhoan.token || "");
    sessionStorage.setItem("sk_vaitro", taiKhoan.vaiTro || taiKhoan.TenVaiTro || "");
    sessionStorage.setItem("sk_hoten", taiKhoan.hoTen || taiKhoan.HoTen || "");
    sessionStorage.setItem("sk_tkid", taiKhoan.id || taiKhoan.TaiKhoanID || "");
    thongBao("Đăng nhập thành công: " + (taiKhoan.hoTen || ""));
    if (typeof taiDanhSachSuKien === "function") taiDanhSachSuKien();
    var duongDanSauDangNhap = duongDanNoiBo(new URLSearchParams(location.search).get("next"));
    if (duongDanSauDangNhap) location.assign(duongDanSauDangNhap);
    else if (phienDangNhap.vaiTro === "BanToChuc") location.assign("/pages/ban-to-chuc/su-kien-cua-toi.html");
    else if (["QuanLySuKien", "QuanTriVien"].indexOf(phienDangNhap.vaiTro) !== -1)
      location.assign("/pages/quan-tri/tong-quan.html");
    else hienTrangTheoTen("sukien");
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  } finally {
    dangNhapTaiKhoan.dangGui = false;
    if (nutDangNhap) {
      nutDangNhap.disabled = false;
      nutDangNhap.removeAttribute("aria-busy");
    }
  }
}
function dangXuatTaiKhoan() {
  try {
    guiYeuCauApi("/api/auth/logout", "POST").catch(function () {});
  } catch (loiXuLy) {}
  try {
    sessionStorage.clear();
  } catch (loiDuPhong) {}
  cacDanhSachPhanTrang.clear();
  cacYeuCauDocDangChay.clear();
  if (typeof apDungQuyen === "function") apDungQuyen();
  hienTrangTheoTen("dangnhap");
}
async function doiMatKhauTaiKhoan() {
  if (doiMatKhauTaiKhoan.dangGui) return;
  var matKhauCu = document.getElementById("dmk-cu") ? document.getElementById("dmk-cu").value : "";
  var matKhauMoi = document.getElementById("dmk-moi") ? document.getElementById("dmk-moi").value : "";
  var matKhauNhapLai = document.getElementById("dmk-nhaplai")
    ? document.getElementById("dmk-nhaplai").value
    : "";
  if (!matKhauCu || !matKhauMoi) {
    hienThongBaoNoi("Vui lòng nhập mật khẩu cũ và mật khẩu mới.", "loi");
    return;
  }
  if (!kiemTraMatKhauForm(matKhauMoi, matKhauNhapLai, "dmk-nhaplai")) return;
  var nutDoiMatKhau = document.querySelector('[data-action="doiMatKhau"]');
  doiMatKhauTaiKhoan.dangGui = true;
  if (nutDoiMatKhau) {
    nutDoiMatKhau.disabled = true;
    nutDoiMatKhau.setAttribute("aria-busy", "true");
  }
  try {
    await guiYeuCauApi("/api/auth/change-password", "PUT", {
      MatKhauCu: matKhauCu,
      MatKhauMoi: matKhauMoi,
    });
    thongBao("Đã đổi mật khẩu. Vui lòng đăng nhập lại.");
    dangXuatTaiKhoan();
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  } finally {
    doiMatKhauTaiKhoan.dangGui = false;
    if (nutDoiMatKhau) {
      nutDoiMatKhau.disabled = false;
      nutDoiMatKhau.removeAttribute("aria-busy");
    }
  }
}

/* ---- Quan tri tai khoan (giuu ten ham cu de HTML cu van chay) ---- */
async function taiDanhSachTaiKhoan() {
  var hienKhungDangTai = typeof hienKhungDangTai === "function" ? hienKhungDangTai : null;
  if (hienKhungDangTai) hienKhungDangTai("ds-taikhoan", 4);
  try {
    var tuKhoaTimKiem = docGiaTriInput("qtk-tukhoa") || null;
    var duongDanTruyVan = "/api/accounts?page=1&size=50" + (tuKhoaTimKiem ? "&q=" + encodeURIComponent(tuKhoaTimKiem) : "");
    var phanHoiTaiKhoan = await guiYeuCauDanhSach(duongDanTruyVan, "ds-taikhoan", taiDanhSachTaiKhoan);
    if (typeof giaoDienQuanLySuKien !== "undefined") giaoDienQuanLySuKien.hienDanhSachTaiKhoan(phanHoiTaiKhoan.data || []);
    else {
      document.getElementById("ds-taikhoan").innerHTML = taoHtmlBang(
        [
          ["HoTen", "Họ tên"],
          ["Email", "Email"],
          ["VaiTro", "Vai trò"],
          ["TrangThai", "Hoạt động"],
        ],
        (phanHoiTaiKhoan.data || []).map(function (taiKhoan) {
          taiKhoan.TrangThai = taiKhoan.HoatDong ? "Đang hoạt động" : "Đã khóa";
          return taiKhoan;
        }),
        function (taiKhoan) {
          return (
            '<button class="phu" onclick="khoaTaiKhoan(' +
            taiKhoan.Id +
            ')">Khoa</button> ' +
            '<button class="phu" onclick="moKhoaTaiKhoan(' +
            taiKhoan.Id +
            ')">Mo</button> ' +
            '<button class="phu" onclick="doiVaiTroTaiKhoan(' +
            taiKhoan.Id +
            ')">Doi vai tro</button>'
          );
        },
      );
    }
  } catch (loiXuLy) {
    hienLoiTai("ds-taikhoan", loiXuLy, taiDanhSachTaiKhoan);
  }
}
async function khoaTaiKhoan(taiKhoanId) {
  try {
    await guiYeuCauApi("/api/accounts/" + taiKhoanId + "/lock", "POST");
    thongBao("Đã khóa tài khoản.");
    taiDanhSachTaiKhoan();
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  }
}
async function moKhoaTaiKhoan(taiKhoanId) {
  try {
    await guiYeuCauApi("/api/accounts/" + taiKhoanId + "/unlock", "POST");
    thongBao("Đã mở khóa tài khoản.");
    taiDanhSachTaiKhoan();
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  }
}
async function doiVaiTroTaiKhoan(taiKhoanId) {
  var luaChonVaiTro = prompt(
    "Chọn vai trò mới:\n1 = Người dùng\n2 = Ban tổ chức\n3 = Quản lý sự kiện\n4 = Quản trị viên",
    "1",
  );
  if (luaChonVaiTro === null) return;
  var vaiTroIdTheoLuaChon = {
    "khach hang": 1,
    khachhang: 1,
    1: 1,
    "ban to chuc": 2,
    bantchuc: 2,
    2: 2,
    "quan ly": 3,
    quanly: 3,
    3: 3,
    "quan tri": 4,
    quantri: 4,
    4: 4,
  };
  var vaiTroId = vaiTroIdTheoLuaChon[String(luaChonVaiTro).trim().toLowerCase()];
  if (!vaiTroId) {
    hienThongBaoNoi("Lựa chọn không hợp lệ. Chọn từ 1 đến 4.", "loi");
    return;
  }
  try {
    await guiYeuCauApi("/api/accounts/" + taiKhoanId + "/role", "PUT", {
      VaiTroId: vaiTroId,
    });
    thongBao("Đổi vai trò thành công.");
    taiDanhSachTaiKhoan();
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  }
}
async function capNhatTaiKhoan() {
  var hoTen = docGiaTriInput("qtk-hoten");
  if (!hoTen) {
    hienThongBaoNoi("Vui lòng nhập họ tên.", "loi");
    return;
  }
  var nutLuuHoSo = document.getElementById("btn-luu-hoso");
  if (nutLuuHoSo && nutLuuHoSo.disabled) return;
  if (nutLuuHoSo) nutLuuHoSo.disabled = true;
  try {
    await guiYeuCauApi("/api/accounts/me", "PUT", {
      HoTen: hoTen,
    });
    sessionStorage.setItem("sk_hoten", hoTen);
    apDungQuyen();
    if (typeof giaoDienQuanLySuKien !== "undefined") giaoDienQuanLySuKien.hienHoSoTaiKhoan();
    thongBao("Đã lưu thông tin tài khoản.");
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  } finally {
    if (nutLuuHoSo) nutLuuHoSo.disabled = false;
  }
}
async function taiHoSoCaNhan() {
  if (!phienDangNhap.token || !document.getElementById("btn-luu-hoso")) return;
  var nutLuuHoSo = document.getElementById("btn-luu-hoso");
  if (nutLuuHoSo) nutLuuHoSo.disabled = true;
  try {
    var phanHoiHoSo = await guiYeuCauApi("/api/accounts/me");
    var hoSoTaiKhoan = phanHoiHoSo.data || {};
    [
      ["qtk-id", hoSoTaiKhoan.Id],
      ["qtk-hoten", hoSoTaiKhoan.HoTen],
      ["qtk-email", hoSoTaiKhoan.Email],
    ].forEach(function (capTruongGiaTri) {
      var oThongTinTaiKhoan = document.getElementById(capTruongGiaTri[0]);
      if (oThongTinTaiKhoan) oThongTinTaiKhoan.value = capTruongGiaTri[1] || "";
    });
    sessionStorage.setItem("sk_hoten", hoSoTaiKhoan.HoTen || "");
    apDungQuyen();
    if (typeof giaoDienQuanLySuKien !== "undefined") giaoDienQuanLySuKien.hienHoSoTaiKhoan();
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  } finally {
    if (nutLuuHoSo) nutLuuHoSo.disabled = false;
  }
}
document.addEventListener("DOMContentLoaded", taiHoSoCaNhan);
