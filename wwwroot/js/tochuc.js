/* ===== tochuc.js — xac thuc ban to chuc ===== */
"use strict";

/* global guiYeuCauApi, hienThongBaoNoi, thongBao, docGiaTriInput, taoHtmlBang, taoNhanTrangThai, hienKhungDangTai, dinhDangNgayGioVietNam, maHoaKyTuHtml */
async function guiYeuCauXacThucToChuc() {
  var tenToChuc = docGiaTriInput("tc-ten");
  var giayTo = docGiaTriInput("tc-giayto");
  if (!tenToChuc || !giayTo) {
    hienThongBaoNoi("Vui lòng nhập tên tổ chức và đường dẫn giấy tờ xác minh.", "loi");
    return;
  }
  try {
    var documentUrl = new URL(giayTo);
    if (documentUrl.protocol !== "https:" && documentUrl.protocol !== "http:") throw new Error();
  } catch (loiXuLy) {
    hienThongBaoNoi("Vui lòng nhập đường dẫn giấy tờ hợp lệ.", "loi");
    return;
  }
  var nutGuiYeuCauXacThuc = document.getElementById("btn-gui-xacthuc");
  if (nutGuiYeuCauXacThuc && nutGuiYeuCauXacThuc.disabled) return;
  if (nutGuiYeuCauXacThuc) nutGuiYeuCauXacThuc.disabled = true;
  try {
    var phanHoiYeuCau = await guiYeuCauApi("/api/organizer-verifications", "POST", {
      TenToChuc: tenToChuc,
      GiayTo: giayTo,
    });
    thongBao("Đã gửi hồ sơ xác thực tổ chức.");
    location.assign("/pages/to-chuc/yeu-cau-cua-toi.html");
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  } finally {
    if (nutGuiYeuCauXacThuc) nutGuiYeuCauXacThuc.disabled = false;
  }
}
async function taiYeuCauXacThucCuaToi() {
  var khungYeuCauXacThuc = document.getElementById("ds-yeucau");
  if (!khungYeuCauXacThuc || !phienDangNhap.token) return;
  hienKhungDangTai("ds-yeucau", 3);
  try {
    var phanHoiYeuCau = await guiYeuCauApi("/api/organizer-verifications/me");
    var danhSachYeuCau = phanHoiYeuCau.data
      ? Array.isArray(phanHoiYeuCau.data)
        ? phanHoiYeuCau.data
        : [phanHoiYeuCau.data]
      : [];
    var cacDongYeuCauXacThuc = danhSachYeuCau.map(function (yeuCauXacThuc) {
      return {
        YeuCauID: yeuCauXacThuc.Id,
        TenToChuc: yeuCauXacThuc.TenToChuc,
        TrangThai:
          typeof yeuCauXacThuc.TrangThai === "number"
            ? ["ChoDuyet", "DaDuyet", "TuChoi"][yeuCauXacThuc.TrangThai]
            : yeuCauXacThuc.TrangThai,
        NgayTao: dinhDangNgayGioVietNam(yeuCauXacThuc.NgayTao),
        LyDoTuChoi: yeuCauXacThuc.LyDo,
      };
    });
    if (!cacDongYeuCauXacThuc.length) {
      khungYeuCauXacThuc.innerHTML =
        '<div class="empty"><h3>Bạn không có hồ sơ đang chờ xử lý</h3><p>Nếu hồ sơ đã được duyệt, hãy đăng nhập lại để sử dụng quyền ban tổ chức.</p><a class="phu btn" href="gui-xac-thuc.html">Gửi hồ sơ</a></div>';
      return;
    }
    khungYeuCauXacThuc.innerHTML =
      '<div class="grid g2">' +
      cacDongYeuCauXacThuc
        .map(function (yeuCauXacThuc) {
          return (
            '<div class="card hoverable anim-pop"><h3>Hồ sơ ' +
            maHoaKyTuHtml(yeuCauXacThuc.TenToChuc) +
            "</h3>" +
            '<div class="muted">Ngày gửi: ' +
            maHoaKyTuHtml(yeuCauXacThuc.NgayTao) +
            "</div>" +
            '<div class="toolbar"><span>' +
            taoNhanTrangThai(yeuCauXacThuc.TrangThai) +
            "</span></div>" +
            (yeuCauXacThuc.LyDoTuChoi
              ? '<div class="muted">Lý do từ chối: ' + maHoaKyTuHtml(yeuCauXacThuc.LyDoTuChoi) + "</div>"
              : "") +
            "</div>"
          );
        })
        .join("") +
      "</div>";
  } catch (loiXuLy) {
    hienLoiTai("ds-yeucau", loiXuLy, taiYeuCauXacThucCuaToi);
  }
}
document.addEventListener("DOMContentLoaded", function () {
  if (document.getElementById("ds-yeucau")) taiYeuCauXacThucCuaToi();
});
async function duyetYeuCauToChuc(yeuCauXacThucId) {
  try {
    await guiYeuCauApi("/api/organizer-verifications/" + yeuCauXacThucId + "/approve", "POST", null);
    thongBao("Đã duyệt yêu cầu #" + yeuCauXacThucId + ".");
    if (typeof taiThongKeToChuc === "function") taiThongKeToChuc();
    taiYeuCauXacThucCuaToi();
    return true;
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
    return false;
  }
}
async function tuChoiYeuCauToChuc(yeuCauXacThucId) {
  var lyDoTuChoi = prompt("Nhập lý do từ chối yêu cầu #" + yeuCauXacThucId + ":");
  if (lyDoTuChoi === null) return;
  if (!lyDoTuChoi.trim()) {
    hienThongBaoNoi("Vui lòng nhập lý do từ chối.", "loi");
    return;
  }
  try {
    await guiYeuCauApi("/api/organizer-verifications/" + yeuCauXacThucId + "/reject", "POST", {
      LyDo: lyDoTuChoi.trim(),
    });
    thongBao("Đã từ chối yêu cầu #" + yeuCauXacThucId + ".");
    if (typeof taiThongKeToChuc === "function") taiThongKeToChuc();
    taiYeuCauXacThucCuaToi();
    return true;
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
    return false;
  }
}
async function taiThongKeToChuc() {
  hienKhungDangTai("tk-tochuc", 3);
  try {
    var phanHoiYeuCau = await guiYeuCauApi("/api/organizer-verifications");
    hienThongKeToChuc(phanHoiYeuCau.data || []);
  } catch (loiXuLy) {
    hienLoiTai("tk-tochuc", loiXuLy, taiThongKeToChuc);
    thongBao(loiXuLy.message, "loi");
  }
}
function hienThongKeToChuc(danhSachYeuCauXacThuc) {
  var khungThongKeXacThuc = document.getElementById("tk-tochuc");
  if (khungThongKeXacThuc)
    khungThongKeXacThuc.innerHTML = taoHtmlBang(
      [
        ["TrangThai", "Trạng thái"],
        ["SoLuong", "Số lượng"],
      ],
      demSoLuongTheoNhom(danhSachYeuCauXacThuc, function (yeuCauXacThuc) {
        return ["ChoDuyet", "DaDuyet", "TuChoi"][yeuCauXacThuc.TrangThai] || yeuCauXacThuc.TrangThai;
      }),
    );
}
function demSoLuongTheoNhom(cacBanGhiThongKe, layKhoaNhom) {
  var soLuongTheoNhom = {};
  cacBanGhiThongKe.forEach(function (banGhiThongKe) {
    var khoaNhom = layKhoaNhom(banGhiThongKe);
    soLuongTheoNhom[khoaNhom] = (soLuongTheoNhom[khoaNhom] || 0) + 1;
  });
  return Object.keys(soLuongTheoNhom).map(function (khoaNhom) {
    return {
      TrangThai: khoaNhom,
      SoLuong: soLuongTheoNhom[khoaNhom],
    };
  });
}
