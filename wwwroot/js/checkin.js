/* ===== checkin.js — ve + check-in + xuat ve ===== */
"use strict";

/* global guiYeuCauApi, hienThongBaoNoi, thongBao, docGiaTriInput, docSoTuInput, taoHtmlBang, taoNhanTrangThai, hienKhungDangTai, maHoaKyTuHtml, dinhDangNgayGioVietNam, dinhDangSoTien */
async function taiVeTheoDon() {
  var donVeId = docSoTuInput("ve-theodon");
  if (!donVeId || donVeId <= 0) {
    hienThongBaoNoi("Nhập mã đơn hàng để xem vé.", "loi");
    return;
  }
  hienKhungDangTai("ds-ve-theodon", 3);
  try {
    var phanHoiDonVe = await guiYeuCauApi("/api/orders/" + donVeId);
    var donVe = phanHoiDonVe.data || phanHoiDonVe;
    if (typeof giaoDienQuanLySuKien !== "undefined") {
      document.getElementById("ds-ve-theodon").innerHTML = giaoDienQuanLySuKien.hienVeDienTu(donVe);
      return;
    }
    var danhSachVeHienThi = (donVe.Ves || []).map(function (veDaPhatHanh) {
      return {
        MaVe: veDaPhatHanh.MaVe,
        MaQR: veDaPhatHanh.MaQR,
        TrangThai: veDaPhatHanh.TrangThai,
        GiaThucTe: veDaPhatHanh.LoaiVe ? veDaPhatHanh.LoaiVe.Gia : 0,
        VeID: veDaPhatHanh.Id,
      };
    });
    document.getElementById("ds-ve-theodon").innerHTML = taoHtmlBang(
      [
        ["MaVe", "Mã vé"],
        ["MaQR", "Mã QR"],
        ["TrangThai", "Trạng thái"],
        ["GiaThucTe", "Gia", "tien"],
      ],
      danhSachVeHienThi,
      function (hangVe) {
        return "<button class=\"phu sm\" onclick=\"taiLichSuXuatVe(" + hangVe.VeID + ')">Lich su xuat</button>';
      },
    );
  } catch (loiXuLy) {
    hienLoiTai("ds-ve-theodon", loiXuLy, taiVeTheoDon);
  }
}
async function traCuuVe() {
  var maVe = docGiaTriInput("ci-ma"),
    noiDungQrTraCuu = docGiaTriInput("ci-qr");
  if (!maVe && !noiDungQrTraCuu) {
    hienThongBaoNoi("Nhập mã vé hoặc mã QR để tra cứu.", "loi");
    return;
  }
  hienKhungDangTai("kq-tracuu", 2);
  try {
    var maTraCuu = maVe || noiDungQrTraCuu;
    var phanHoiTraCuu = await guiYeuCauApi("/api/tickets/by-code/" + encodeURIComponent(maTraCuu));
    var veDaPhatHanh = phanHoiTraCuu.data || {};
    var khungKetQuaTraCuu = document.getElementById("kq-tracuu");
    if (khungKetQuaTraCuu) {
      khungKetQuaTraCuu.innerHTML =
        '<div class="ticket anim-pop"><div class="muted">Kết quả tra cứu</div>' +
        "<h3>" +
        maHoaKyTuHtml(veDaPhatHanh.MaVe || maVe || noiDungQrTraCuu) +
        "</h3>" +
        '<div class="toolbar">' +
        taoNhanTrangThai(
          typeof veDaPhatHanh.TrangThai === "number"
            ? ["ChuaSuDung", "DaCheckIn", "DaHuy", "DaHoanTien"][veDaPhatHanh.TrangThai]
            : veDaPhatHanh.TrangThai,
        ) +
        "</div>" +
        '<div class="toolbar"><button class="phu sm" type="button">Điền vào check-in</button></div></div>';
      khungKetQuaTraCuu.querySelector("button").onclick = function () {
        dienVeCheckin(veDaPhatHanh.MaVe || maVe || noiDungQrTraCuu);
      };
    }
    thongBao("Tra cứu vé thành công.");
  } catch (loiXuLy) {
    hienLoiTai("kq-tracuu", loiXuLy, traCuuVe);
  }
}
function dienVeCheckin(maVe) {
  if (!maVe) {
    hienThongBaoNoi("Chưa có mã vé.", "loi");
    return;
  }
  var oMaCheckIn = document.getElementById("ci-ve");
  if (oMaCheckIn) {
    oMaCheckIn.value = maVe;
    oMaCheckIn.focus();
    oMaCheckIn.select();
  }
  hienThongBaoNoi("Đã điền mã vé " + maVe + " vào ô check-in.", "info");
}
async function ghiNhanCheckInVe() {
  var maVeHoacQr = docGiaTriInput("ci-ve");
  if (!maVeHoacQr) {
    hienThongBaoNoi("Nhập mã vé hợp lệ để check-in.", "loi");
    return;
  }
  if (ghiNhanCheckInVe.dangGui) return;
  ghiNhanCheckInVe.dangGui = true;
  var nutCheckIn = document.querySelector("button[onclick=\"ghiNhanCheckInVe()\"]");
  if (nutCheckIn) {
    nutCheckIn.disabled = true;
    nutCheckIn.setAttribute("aria-busy", "true");
  }
  try {
    var phanHoiCheckIn = await guiYeuCauApi("/api/check-in", "POST", {
      Ma: maVeHoacQr,
    });
    thongBao("Check-in thành công: " + ((phanHoiCheckIn.data && phanHoiCheckIn.data.MaVe) || "Ve " + maVeHoacQr));
    var khungKetQuaCheckIn = document.getElementById("kq-checkin");
    if (khungKetQuaCheckIn) {
      khungKetQuaCheckIn.innerHTML =
        '<div class="checkin-result anim-pop"><h3>Check-in thành công</h3>' +
        "<div>" +
        taoNhanTrangThai("DaCheckIn") +
        "</div>" +
        "<div>Mã vé: " +
        maHoaKyTuHtml((phanHoiCheckIn.data && phanHoiCheckIn.data.MaVe) || maVeHoacQr) +
        "</div></div>";
    }
  } catch (loiXuLy) {
    var khungKetQuaCheckIn = document.getElementById("kq-checkin");
    if (khungKetQuaCheckIn)
      khungKetQuaCheckIn.innerHTML =
        '<div class="checkin-result error"><h3>Chưa thể check-in</h3><p>' +
        maHoaKyTuHtml(loiXuLy.message) +
        "</p></div>";
    thongBao(loiXuLy.message, "loi");
  } finally {
    ghiNhanCheckInVe.dangGui = false;
    if (nutCheckIn) {
      nutCheckIn.disabled = false;
      nutCheckIn.removeAttribute("aria-busy");
    }
    var oMaCheckIn = document.getElementById("ci-ve");
    if (oMaCheckIn) {
      oMaCheckIn.focus();
      oMaCheckIn.select();
    }
  }
}
async function taiTepVe(veId) {
  var tepPdfVe = await guiYeuCauApi("/api/tickets/" + veId + "/export", "POST", null, "blob");
  var urlTepPdfVe = URL.createObjectURL(tepPdfVe),
    lienKetTaiVe = document.createElement("a");
  lienKetTaiVe.href = urlTepPdfVe;
  lienKetTaiVe.download = "ve-" + veId + ".pdf";
  document.body.appendChild(lienKetTaiVe);
  lienKetTaiVe.click();
  lienKetTaiVe.remove();
  setTimeout(function () {
    URL.revokeObjectURL(urlTepPdfVe);
  }, 1000);
}
async function xuatVeTheoHinhThuc() {
  var veId = docSoTuInput("xv-ve");
  var hinhThucXuat = docGiaTriInput("xv-ht");
  if (!veId || veId <= 0) {
    hienThongBaoNoi("Nhập mã vé hợp lệ.", "loi");
    return;
  }
  if (!hinhThucXuat) {
    hienThongBaoNoi("Chọn hình thức xuất vé.", "loi");
    return;
  }
  if (xuatVeTheoHinhThuc.dangGui) return;
  xuatVeTheoHinhThuc.dangGui = true;
  var nutXuatVe = document.querySelector("button[onclick=\"xuatVeTheoHinhThuc()\"], [data-action=\"xuatVe\"]");
  if (nutXuatVe) nutXuatVe.disabled = true;
  try {
    if (String(hinhThucXuat).toLowerCase().indexOf("email") !== -1) await guiEmailVe(veId);
    else {
      await taiTepVe(veId);
      thongBao("Đã tải vé PDF #" + veId + ".");
    }
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  } finally {
    xuatVeTheoHinhThuc.dangGui = false;
    if (nutXuatVe) nutXuatVe.disabled = false;
  }
}
async function guiEmailVe(veDaPhatHanhId) {
  if (!veDaPhatHanhId || veDaPhatHanhId <= 0) {
    hienThongBaoNoi("Vui lòng nhập mã vé.", "loi");
    return;
  }
  try {
    await guiYeuCauApi("/api/tickets/" + veDaPhatHanhId + "/email", "POST", null);
    thongBao("Đã gửi vé #" + veDaPhatHanhId + " qua email.");
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  }
}
async function taiLichSuXuatVe(veDaPhatHanhId) {
  var veId = veDaPhatHanhId || docSoTuInput("xv-lichsu-ve");
  if (!veId || veId <= 0) {
    hienThongBaoNoi("Nhập mã vé để xem lịch sử xuất.", "loi");
    return;
  }
  var khungLichSuXuatVe = document.getElementById("ds-xuatve");
  if (!khungLichSuXuatVe && typeof giaoDienQuanLySuKien !== "undefined") {
    giaoDienQuanLySuKien.moLichSuXuatVe(veId);
    return;
  }
  hienKhungDangTai("ds-xuatve", 3);
  try {
    var phanHoiLichSuXuat = await guiYeuCauApi("/api/tickets/" + veId + "/exports");
    var danhSachLanXuatVe = (phanHoiLichSuXuat.data || []).map(function (lanXuatVe) {
      return {
        HinhThucXuat: lanXuatVe.Kieu,
        NgayXuat: lanXuatVe.NgayTao,
        TrangThai: lanXuatVe.ThanhCong ? "Thành công" : "Thất bại",
      };
    });
    var khungLichSuXuatVe = document.getElementById("ds-xuatve") || document.getElementById("ds-ve-theodon");
    if (khungLichSuXuatVe) {
      khungLichSuXuatVe.innerHTML =
        "<h3>Lich su xuat ve #" +
        veId +
        "</h3>" +
        taoHtmlBang(
          [
            ["HinhThucXuat", "Hình thức"],
            ["NgayXuat", "Ngày xuất", "ngay"],
            ["TrangThai", "Trạng thái"],
          ],
          danhSachLanXuatVe,
        );
      khungLichSuXuatVe.scrollIntoView({
        behavior: "smooth",
        block: "nearest",
      });
    }
  } catch (loiXuLy) {
    hienLoiTai("ds-xuatve", loiXuLy, function () {
      taiLichSuXuatVe(veId);
    });
  }
}
