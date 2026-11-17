/* ===== api.js — lop goi API dung chung, toast, spinner, helper ===== */
"use strict";

/* global URL_GOC_API */
var phienDangNhap = {
  get token() {
    try {
      return sessionStorage.getItem("sk_token") || "";
    } catch (loiXuLy) {
      return "";
    }
  },
  get vaiTro() {
    try {
      return sessionStorage.getItem("sk_vaitro") || "";
    } catch (loiXuLy) {
      return "";
    }
  },
  get hoTen() {
    try {
      return sessionStorage.getItem("sk_hoten") || "";
    } catch (loiXuLy) {
      return "";
    }
  },
  get taiKhoanID() {
    try {
      return sessionStorage.getItem("sk_tkid") || "";
    } catch (loiXuLy) {
      return "";
    }
  },
};
var soYeuCauDangTai = 0;
function chuyenGiaTriThanhVanBan(giaTriCanXuLy) {
  return String(giaTriCanXuLy === null || giaTriCanXuLy === undefined ? "" : giaTriCanXuLy);
}
function chuanHoaPhanHoi(giaTriCanXuLy) {
  if (Array.isArray(giaTriCanXuLy)) return giaTriCanXuLy.map(chuanHoaPhanHoi);
  if (!giaTriCanXuLy || typeof giaTriCanXuLy !== "object") return giaTriCanXuLy;
  var phanHoiChuanHoa = {};
  Object.keys(giaTriCanXuLy).forEach(function (tenThuocTinhJson) {
    if (["__proto__", "constructor", "prototype"].indexOf(tenThuocTinhJson) !== -1) return;
    var giaTriChuanHoa = chuanHoaPhanHoi(giaTriCanXuLy[tenThuocTinhJson]);
    phanHoiChuanHoa[tenThuocTinhJson] = giaTriChuanHoa;
    var tenThuocTinhVietHoa = tenThuocTinhJson.charAt(0).toUpperCase() + tenThuocTinhJson.slice(1);
    if (!(tenThuocTinhVietHoa in giaTriCanXuLy)) phanHoiChuanHoa[tenThuocTinhVietHoa] = giaTriChuanHoa;
  });
  return phanHoiChuanHoa;
}
function thongDiepNguoiDung(thongDiepHienThi) {
  var noiDungVanBan = String(thongDiepHienThi || "");
  if (/failed to fetch|networkerror|network error|load failed/i.test(noiDungVanBan))
    return "Không thể kết nối. Vui lòng kiểm tra kết nối mạng rồi thử lại.";
  if (/jwt|token|json|payload|sql|database|exception|HTTP|backend|frontend|\bAPI\b/i.test(noiDungVanBan))
    return "Không thể hoàn tất yêu cầu. Vui lòng tải lại trang rồi thử lại.";
  return chuyenGiaTriThanhVanBan(noiDungVanBan);
}
function hienBieuTuongDangTai() {
  soYeuCauDangTai++;
  var bieuTuongDangTai = document.getElementById("spinner");
  if (bieuTuongDangTai) bieuTuongDangTai.classList.add("hien");
}
function anBieuTuongDangTai() {
  soYeuCauDangTai = Math.max(0, soYeuCauDangTai - 1);
  if (soYeuCauDangTai === 0) {
    var bieuTuongDangTai = document.getElementById("spinner");
    if (bieuTuongDangTai) bieuTuongDangTai.classList.remove("hien");
  }
}
var cacYeuCauDocDangChay = new Map();
var hanThuLaiYeuCau = 0;
function guiYeuCauApi(duongDanYeuCau, phuongThucHttp, noiDungYeuCau, kieuDuLieuPhanHoi, tuyChonThem) {
  var method = String(phuongThucHttp || "GET").toUpperCase();
  var key = phienDangNhap.token + "|" + duongDanYeuCau + "|" + (kieuDuLieuPhanHoi || "json");
  if (method !== "GET") cacYeuCauDocDangChay.clear();
  if (method === "GET" && cacYeuCauDocDangChay.has(key)) return cacYeuCauDocDangChay.get(key);
  var pending = thucHienYeuCauApi(duongDanYeuCau, method, noiDungYeuCau, kieuDuLieuPhanHoi, tuyChonThem);
  if (method === "GET") {
    cacYeuCauDocDangChay.set(key, pending);
    var cleanup = function () {
      if (cacYeuCauDocDangChay.get(key) === pending) cacYeuCauDocDangChay.delete(key);
    };
    pending.then(cleanup, cleanup);
  }
  return pending;
}
async function thucHienYeuCauApi(duongDanYeuCau, phuongThucHttp, noiDungYeuCau, kieuDuLieuPhanHoi, tuyChonThem) {
  if (Date.now() < hanThuLaiYeuCau && duongDanYeuCau !== "/api/auth/logout") {
    var cooling = new Error("Vui lòng đợi " + Math.ceil((hanThuLaiYeuCau - Date.now()) / 1000) + " giây rồi thử lại.");
    cooling.maTrangThaiHttp = 429;
    throw cooling;
  }
  phuongThucHttp = phuongThucHttp || "GET";
  if (!duongDanYeuCau.startsWith("/api/")) duongDanYeuCau = "/api" + (duongDanYeuCau.startsWith("/") ? "" : "/") + duongDanYeuCau;
  var tieuDeYeuCauHttp = {
    "Content-Type": "application/json",
  };
  if (phienDangNhap.token) tieuDeYeuCauHttp["Authorization"] = "Bearer " + phienDangNhap.token;
  if (tuyChonThem && tuyChonThem.idempotencyKey) tieuDeYeuCauHttp["Idempotency-Key"] = tuyChonThem.idempotencyKey;
  hienBieuTuongDangTai();
  var boHuyYeuCau = new AbortController();
  var boDemHanYeuCau = setTimeout(function () {
    boHuyYeuCau.abort();
  }, 25000);
  try {
    var phanHoiHttp = await fetch(URL_GOC_API + duongDanYeuCau, {
      method: phuongThucHttp,
      cache: "no-store",
      credentials: "omit",
      headers: tieuDeYeuCauHttp,
      signal: boHuyYeuCau.signal,
      body: noiDungYeuCau ? JSON.stringify(noiDungYeuCau) : null,
    });
    if (phanHoiHttp.ok && kieuDuLieuPhanHoi === "blob") return await phanHoiHttp.blob();
    var noiDungPhanHoi = {};
    try {
      noiDungPhanHoi = await phanHoiHttp.json();
    } catch (loiXuLy) {
      noiDungPhanHoi = {};
    }
    if (phanHoiHttp.status === 429 || phanHoiHttp.status === 503) {
      var retryHeader = phanHoiHttp.headers.get("Retry-After");
      var retrySeconds = Number(retryHeader) || Math.ceil((Date.parse(retryHeader) - Date.now()) / 1000) || 5;
      retrySeconds = Math.max(1, Math.min(retrySeconds, 300));
      hanThuLaiYeuCau = Date.now() + retrySeconds * 1000;
      var busy = new Error((phanHoiHttp.status === 429 ? "Bạn gửi yêu cầu quá nhanh. " : "Dịch vụ đang bận. ") +
        "Vui lòng thử lại sau " + retrySeconds + " giây.");
      busy.maTrangThaiHttp = phanHoiHttp.status;
      busy.retryAfter = retrySeconds;
      throw busy;
    }
    if (phanHoiHttp.status === 401 || phanHoiHttp.status === 403) {
      var maKetQuaHttp = (noiDungPhanHoi && noiDungPhanHoi.code) || "";
      var thongDiepLoi =
        phanHoiHttp.status === 401
          ? "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại."
          : "Tài khoản chưa có quyền thực hiện thao tác này.";
      if (duongDanYeuCau === "/api/auth/login") thongDiepLoi = "Email hoặc mật khẩu chưa đúng. Vui lòng kiểm tra lại.";
      if (phanHoiHttp.status === 401 || maKetQuaHttp === "TOKEN_STALE") {
        sessionStorage.clear();
        if (typeof hienTrangTheoTen === "function") {
          try {
            hienTrangTheoTen("dangnhap");
          } catch (loiDuPhong) {}
        }
        var loiPhienDangNhapHetHan = new Error(thongDiepLoi || "Phiên đã hết hạn, vui lòng đăng nhập lại.");
        loiPhienDangNhapHetHan.maTrangThaiHttp = phanHoiHttp.status;
        throw loiPhienDangNhapHetHan;
      }
      var loiKhongDuQuyenTruyCap = new Error(thongDiepLoi || "Tài khoản chưa có quyền thực hiện.");
      loiKhongDuQuyenTruyCap.maTrangThaiHttp = phanHoiHttp.status;
      throw loiKhongDuQuyenTruyCap;
    }
    if (!phanHoiHttp.ok)
      throw new Error(
        phanHoiHttp.status >= 500
          ? "Dịch vụ đang bận. Vui lòng thử lại sau."
          : (noiDungPhanHoi && noiDungPhanHoi.message) ||
              "Không thể hoàn tất yêu cầu. Vui lòng kiểm tra thông tin và thử lại.",
      );
    var ketQua = chuanHoaPhanHoi(noiDungPhanHoi);
    ketQua.hasMore = phanHoiHttp.headers.get("X-Has-More") === "true";
    return ketQua;
  } catch (loiXuLy) {
    if (loiXuLy.name === "AbortError") throw new Error("Kết nối mất nhiều thời gian. Vui lòng thử lại.");
    loiXuLy.message = thongDiepNguoiDung(loiXuLy.message);
    throw loiXuLy;
  } finally {
    clearTimeout(boDemHanYeuCau);
    anBieuTuongDangTai();
  }
}

function duongDanNoiBo(duongDan) {
  if (!duongDan || !duongDan.startsWith("/") || /[\\\x00-\x20]/.test(duongDan)) return null;
  try {
    var target = new URL(duongDan, location.origin);
    return target.origin === location.origin ? target.pathname + target.search + target.hash : null;
  } catch (error) { return null; }
}

var cacDanhSachPhanTrang = new Map();
async function guiYeuCauDanhSach(duongDan, idKhung, hamTaiLai) {
  var key = phienDangNhap.token + "|" + duongDan;
  var state = cacDanhSachPhanTrang.get(key);
  var append = state && state.next;
  if (!append) state = { page: 0, data: null, next: false };
  state.next = false;
  cacDanhSachPhanTrang.set(key, state);
  if (cacDanhSachPhanTrang.size > 20) cacDanhSachPhanTrang.delete(cacDanhSachPhanTrang.keys().next().value);
  var url = new URL(duongDan, location.origin);
  url.searchParams.set("page", state.page + 1);
  if (!url.searchParams.has("size")) url.searchParams.set("size", "100");
  var result;
  try { result = await guiYeuCauApi(url.pathname + url.search); }
  catch (error) { state.next = !!append; throw error; }
  state.page++;
  function merge(previous, incoming) {
    if (Array.isArray(incoming)) {
      var rows = (previous || []).concat(incoming);
      var ids = new Set();
      return rows.filter(function (row) {
        var id = row && (row.Id || row.id);
        if (!id) return true;
        if (ids.has(id)) return false;
        ids.add(id);
        return true;
      });
    }
    if (!incoming || typeof incoming !== "object") return incoming;
    var merged = {};
    Object.keys(incoming || {}).forEach(function (property) {
      merged[property] = merge(previous && previous[property], incoming[property]);
    });
    return merged;
  }
  state.data = append ? merge(state.data, result.data) : result.data;
  result.data = result.Data = state.data;
  var host = document.getElementById(idKhung);
  var button = document.getElementById(idKhung + "-load-more");
  if (host && result.hasMore) {
    if (!button) {
      button = document.createElement("button");
      button.id = idKhung + "-load-more";
      button.type = "button";
      button.className = "phu";
      button.textContent = "Tải thêm";
      host.after(button);
    }
    button.hidden = false;
    button.disabled = false;
    button.onclick = async function () {
      if (button.disabled) return;
      button.disabled = true;
      state.next = true;
      try { await hamTaiLai(); } finally { button.disabled = false; }
    };
  } else if (button) button.hidden = true;
  return result;
}
function maHoaKyTuHtml(giaTri) {
  return chuyenGiaTriThanhVanBan(giaTri)
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#39;");
}
function giaTriHoacChuoiRong(giaTri) {
  return giaTri === null || giaTri === undefined ? "" : giaTri;
}
function chuyenNgayDuLieuSangDate(giaTri) {
  if (
    typeof giaTri === "string" &&
    /^\d{4}-\d\d-\d\dT\d\d:\d\d/.test(giaTri) &&
    !/(Z|[+-]\d\d:\d\d)$/i.test(giaTri)
  )
    giaTri += "Z";
  return new Date(giaTri);
}
function dinhDangNgayGioVietNam(giaTri) {
  if (!giaTri) return "";
  try {
    return chuyenNgayDuLieuSangDate(giaTri).toLocaleString("vi-VN");
  } catch (loiXuLy) {
    return String(giaTri);
  }
}
function dinhDangSoTien(giaTri) {
  if (giaTri === null || giaTri === undefined || giaTri === "") return "";
  var soTienHienThi = Number(giaTri);
  if (isNaN(soTienHienThi)) return String(giaTri);
  return soTienHienThi.toLocaleString("vi-VN");
}
function docGiaTriInput(phanTuDomId) {
  var oNhapLieu = document.getElementById(phanTuDomId);
  return oNhapLieu ? oNhapLieu.value.trim() : "";
}
function docSoTuInput(phanTuDomId) {
  var chuoiGiaTri = docGiaTriInput(phanTuDomId);
  if (chuoiGiaTri === "") return null;
  var giaTriSo = Number(chuoiGiaTri);
  return isNaN(giaTriSo) ? NaN : giaTriSo;
}
function hienLoiNhapLieu(phanTuDomId, thongDiep) {
  var oNhapLieu = document.getElementById(phanTuDomId);
  if (oNhapLieu) {
    oNhapLieu.classList.toggle("err", !!thongDiep);
    oNhapLieu.setAttribute("aria-invalid", String(!!thongDiep));
  }
  var khungLoi = document.getElementById(phanTuDomId + "-err");
  if (khungLoi) khungLoi.textContent = thongDiep || "";
  return !thongDiep;
}

/* thong bao doi tuong thich: toast + dong tb-chung */
function thongBao(thongDiep, loaiThongBao, phanTuDomId) {
  thongDiep = thongDiepNguoiDung(thongDiep);
  hienThongBaoNoi(thongDiep, loaiThongBao === "loi" || loaiThongBao === "err" ? "loi" : "ok");
  var khungThongBao = document.getElementById(phanTuDomId || "tb-chung");
  if (khungThongBao) {
    khungThongBao.className = "thongbao " + (loaiThongBao === "loi" ? "loi" : "ok");
    khungThongBao.textContent = thongDiep;
    khungThongBao.style.display = "block";
    clearTimeout(khungThongBao.__t);
    khungThongBao.__t = setTimeout(function () {
      khungThongBao.style.display = "none";
    }, 6000);
  }
}
function hienThongBaoNoi(thongDiep, loaiThongBao) {
  thongDiep = thongDiepNguoiDung(thongDiep);
  loaiThongBao = loaiThongBao || "ok";
  var khungBaoNgoai = document.getElementById("toast-wrap");
  if (!khungBaoNgoai) {
    alert(thongDiep);
    return;
  }
  var khungToast = document.createElement("div");
  khungToast.className = "toast " + loaiThongBao;
  var kyHieuThongBao = loaiThongBao === "loi" || loaiThongBao === "err" ? "⚠" : loaiThongBao === "info" ? "ℹ" : "✔";
  khungToast.innerHTML =
    "<span>" + maHoaKyTuHtml(kyHieuThongBao) + '</span><span></span><button class="x" aria-label="Dong">&times;</button>';
  khungToast.children[1].textContent = thongDiep;
  khungToast.querySelector(".x").onclick = function () {
    khungToast.remove();
  };
  khungBaoNgoai.appendChild(khungToast);
  setTimeout(function () {
    khungToast.style.transition = "opacity .4s,transform .4s";
    khungToast.style.opacity = "0";
    khungToast.style.transform = "translateX(40px)";
    setTimeout(function () {
      khungToast.remove();
    }, 420);
  }, 4200);
}
function taoHtmlBang(cauHinhCacCot, danhSachHang, taoNutThaoTac) {
  if (!danhSachHang || !danhSachHang.length)
    return (
      '<div class="empty">' +
      '<svg viewBox="0 0 120 90" fill="none" aria-hidden="true">' +
      '<rect x="14" y="18" width="92" height="58" rx="10" fill="#dbeafe"/>' +
      '<rect x="24" y="30" width="50" height="8" rx="4" fill="#93c5fd"/>' +
      '<rect x="24" y="43" width="72" height="6" rx="3" fill="#bfdbfe"/>' +
      '<rect x="24" y="53" width="60" height="6" rx="3" fill="#bfdbfe"/>' +
      '<circle cx="92" cy="62" r="14" fill="#b2ddff"/></svg>' +
      '<div><b>Chưa có thông tin</b></div><div class="muted">Thông tin sẽ xuất hiện tại đây khi có cập nhật.</div></div>'
    );
  var noiDungBang =
    '<div class="tablescroll" tabindex="0" role="region" aria-label="Bảng thông tin, cuộn ngang để xem thêm"><table><thead><tr>' +
    cauHinhCacCot
      .map(function (cauHinhCot) {
        return "<th>" + maHoaKyTuHtml(cauHinhCot[1]) + "</th>";
      })
      .join("") +
    (taoNutThaoTac ? "<th>Thao tác</th>" : "") +
    "</tr></thead><tbody>";
  danhSachHang.forEach(function (hangDuLieu) {
    noiDungBang +=
      "<tr>" +
      cauHinhCacCot
        .map(function (cauHinhCot) {
          var giaTriO = hangDuLieu[cauHinhCot[0]];
          if (cauHinhCot[2] === "tien") giaTriO = dinhDangSoTien(giaTriO);
          else if (cauHinhCot[2] === "ngay") giaTriO = dinhDangNgayGioVietNam(giaTriO);
          else giaTriO = giaTriHoacChuoiRong(giaTriO);
          if (/trangthai|vaitro/i.test(cauHinhCot[0])) giaTriO = nhanTrangThai(giaTriO);
          return "<td>" + maHoaKyTuHtml(giaTriO) + "</td>";
        })
        .join("");
    if (taoNutThaoTac) {
      try {
        noiDungBang += "<td>" + taoNutThaoTac(hangDuLieu) + "</td>";
      } catch (loiXuLy) {
        noiDungBang += "<td></td>";
      }
    }
    noiDungBang += "</tr>";
  });
  return noiDungBang + "</tbody></table></div>";
}
function nhanTrangThai(giaTri) {
  var cacNhanHienThi = {
    ChoDuyet: "Chờ duyệt",
    DaDuyet: "Đã duyệt",
    TuChoi: "Từ chối",
    DangDienRa: "Đang diễn ra",
    DaHuy: "Đã hủy",
    DaKetThuc: "Đã kết thúc",
    ChoThanhToan: "Chờ thanh toán",
    ThanhCong: "Thành công",
    ThatBai: "Thất bại",
    HetHan: "Hết hạn",
    DaXacNhan: "Đã xác nhận",
    ChoXacNhan: "Chờ xác nhận",
    HoanThanh: "Hoàn thành",
    HoanTien: "Hoàn tiền",
    YeuCauHoanTien: "Yêu cầu hoàn tiền",
    DaHoanTien: "Đã hoàn tiền",
    DangXuLy: "Đang xử lý",
    NghiVan: "Cần kiểm tra",
    ChuaSuDung: "Chưa sử dụng",
    DaSuDung: "Đã sử dụng",
    DaCheckIn: "Đã check-in",
    NguoiDung: "Người dùng",
    BanToChuc: "Ban tổ chức",
    QuanLySuKien: "Quản lý sự kiện",
    QuanTriVien: "Quản trị viên",
    "Hoat dong": "Hoạt động",
    "Dang hoat dong": "Đang hoạt động",
    "Da tat": "Đã tắt",
    "Da khoa": "Đã khóa",
  };
  var noiDungVanBan = String(giaTri === null || giaTri === undefined ? "" : giaTri);
  return cacNhanHienThi[noiDungVanBan] || noiDungVanBan || "—";
}
function taoNhanTrangThai(giaTri) {
  var trangThai = String(giaTri === null || giaTri === undefined ? "" : giaTri);
  var lopTrangThai = "b-muted";
  if (/thanhcong|hoatdong|daduyet|daxacnhan|daxuat|damua|dung|ok/i.test(trangThai.replace(/\s/g, "")))
    lopTrangThai = "b-ok";
  else if (/thatbai|khoa|tuchoi|dahuy|het|that/i.test(trangThai.toLowerCase().replace(/\s/g, "")))
    lopTrangThai = "b-err";
  else if (/choduyet|cho|moiban|dangcho|pending/i.test(trangThai.toLowerCase().replace(/\s/g, "")))
    lopTrangThai = "b-warn";
  else if (trangThai) lopTrangThai = "b-info";
  return '<span class="badge ' + lopTrangThai + '">' + maHoaKyTuHtml(nhanTrangThai(trangThai)) + "</span>";
}
function hienKhungDangTai(idKhungHienThi, soKhoiDangTai) {
  var khungDangTai = document.getElementById(idKhungHienThi);
  if (!khungDangTai) return;
  soKhoiDangTai = soKhoiDangTai || 3;
  var noiDungKhung =
    '<div class="skel-wrap" role="status" aria-label="Đang tải dữ liệu"><span class="visually-hidden">Đang tải dữ liệu…</span>';
  for (var chiSoKhoi = 0; chiSoKhoi < soKhoiDangTai; chiSoKhoi++)
    noiDungKhung += '<div class="shimmer-line" style="width:' + (92 - chiSoKhoi * 9) + '%"></div>';
  noiDungKhung += "</div>";
  khungDangTai.innerHTML = noiDungKhung;
}

/* A failed read must replace its loading state and leave a working retry action. */
function hienLoiTai(idKhungHienThi, loiXuLy, hamThuLai) {
  var khungHienThiLoi = document.getElementById(idKhungHienThi);
  if (!khungHienThiLoi) return;
  khungHienThiLoi.innerHTML =
    '<div class="empty load-error" role="status"><h3>Chưa thể tải thông tin</h3><p>' +
    maHoaKyTuHtml(loiXuLy.message || loiXuLy) +
    "</p>" +
    (hamThuLai ? '<button class="phu" type="button">Thử lại</button>' : "") +
    "</div>";
  var nutThuLai = khungHienThiLoi.querySelector("button");
  if (nutThuLai) nutThuLai.onclick = hamThuLai;
}

/* Confetti CSS/SVG thuan — khong anh ngoai, chay offline */
function chayHieuUngGiayMau(soManhGiay) {
  soManhGiay = soManhGiay || 80;
  var mau = ["#5b21f2", "#2563eb", "#06b6d4", "#ec4899", "#f59e0b", "#22c55e"];
  for (var chiSoManh = 0; chiSoManh < soManhGiay; chiSoManh++) {
    (function () {
      var manhGiay = document.createElement("div");
      manhGiay.className = "confetti";
      var chieuRong = 6 + Math.random() * 7;
      manhGiay.style.left = Math.random() * 100 + "vw";
      manhGiay.style.width = chieuRong + "px";
      manhGiay.style.height = chieuRong * (Math.random() > 0.5 ? 1 : 0.45) + "px";
      manhGiay.style.background = mau[Math.floor(Math.random() * mau.length)];
      manhGiay.style.borderRadius = Math.random() > 0.5 ? "50%" : "2px";
      manhGiay.style.animationDuration = 2.4 + Math.random() * 2.2 + "s";
      document.body.appendChild(manhGiay);
      setTimeout(function () {
        manhGiay.remove();
      }, 5200);
    })();
  }
}

/* Validate dung chung theo hop dong backend */
function kiemTraMatKhauForm(matKhau, nhapLai, idNhapLai) {
  if (!matKhau || matKhau.length < 8) {
    hienThongBaoNoi("Mật khẩu cần tối thiểu 8 ký tự.", "loi");
    return false;
  }
  if (nhapLai !== undefined && matKhau !== nhapLai) {
    if (idNhapLai) hienLoiNhapLieu(idNhapLai, "Mật khẩu nhập lại không khớp.");
    hienThongBaoNoi("Mật khẩu nhập lại không khớp.", "loi");
    return false;
  }
  if (idNhapLai) hienLoiNhapLieu(idNhapLai, "");
  return true;
}
function kiemTraKhoangThoiGianForm(batDau, ketThuc) {
  if (!batDau || !ketThuc) {
    hienThongBaoNoi("Vui lòng chọn thời gian bắt đầu và kết thúc.", "loi");
    return false;
  }
  var thoiDiemBatDau = new Date(batDau),
    thoiDiemKetThuc = new Date(ketThuc);
  if (isNaN(thoiDiemBatDau) || isNaN(thoiDiemKetThuc) || thoiDiemKetThuc <= thoiDiemBatDau) {
    hienThongBaoNoi("Thời gian kết thúc phải sau thời gian bắt đầu.", "loi");
    return false;
  }
  return true;
}
function kiemTraSoKhongAmForm(giaTri, tenHienThi) {
  if (
    giaTri === null ||
    giaTri === undefined ||
    giaTri === "" ||
    isNaN(Number(giaTri)) ||
    Number(giaTri) < 0
  ) {
    hienThongBaoNoi((tenHienThi || "Giá trị") + " phải là số không âm.", "loi");
    return false;
  }
  return true;
}
