/* QLSK · presentation only. Existing service functions own all writes and permissions. */
"use strict";

window.giaoDienQuanLySuKien = (function () {
  var chiTietSuKienTheoId = new Map(),
    suKienDangTaiChiTiet = new Set(),
    suKienTaiChiTietThatBai = new Set(),
    hangDoiTaiChiTiet = [],
    soLuongLuotTaiSongSong = 0,
    phienTaiDanhSach = 0;
  var danhSachTaiKhoan = [],
    danhSachDatDiaDiem = [],
    danhSachYeuCauXacThuc = [],
    duLieuTongQuan = null;
  var boLocTrangThai = "",
    buocTaoSuKienHienTai = 0,
    phanTuCoTieuDiemTruocDo = null,
    lanTaiHopChiTiet = 0;
  function timPhanTuTheoId(phanTuDomId) {
    return document.getElementById(phanTuDomId);
  }
  function taoHtmlBieuTuong(tenBieuTuong) {
    return '<svg class="icon" aria-hidden="true"><use href="/assets/icons/ui.svg#' + tenBieuTuong + '"></use></svg>';
  }
  function dinhDangTienDong(soTienHienThi) {
    return dinhDangSoTien(soTienHienThi) + " ₫";
  }
  function capNhatVanBan(phanTuDomId, noiDungVanBanMoi) {
    var phanTuHienThi = timPhanTuTheoId(phanTuDomId);
    if (phanTuHienThi && phanTuHienThi.textContent !== String(noiDungVanBanMoi)) phanTuHienThi.textContent = noiDungVanBanMoi;
  }
  function capNhatHtml(phanTuDomId, noiDungHtmlMoi) {
    var phanTuHienThi = timPhanTuTheoId(phanTuDomId);
    // Other renderers may replace this host with a skeleton or an error.
    if (
      phanTuHienThi &&
      (phanTuHienThi.__uiHTML !== noiDungHtmlMoi || phanTuHienThi.__uiFirstChild !== phanTuHienThi.firstChild)
    ) {
      phanTuHienThi.__uiHTML = noiDungHtmlMoi;
      phanTuHienThi.innerHTML = noiDungHtmlMoi;
      phanTuHienThi.__uiFirstChild = phanTuHienThi.firstChild;
    }
  }
  function layChuCaiDauTen(tenHienThi) {
    return String(tenHienThi || "SK")
      .trim()
      .split(/\s+/)
      .map(function (tuTrongTen) {
        return tuTrongTen[0];
      })
      .filter(Boolean)
      .slice(-2)
      .join("")
      .toUpperCase();
  }
  function hopNhatChiTietSuKien(suKien) {
    return Object.assign({}, suKien, chiTietSuKienTheoId.get(Number(suKien.Id)) || {});
  }
  function layDiaDiemSuKien(suKien) {
    return (suKien.DatDiaDiem && suKien.DatDiaDiem.DiaDiem) || {};
  }
  function layGiaVeThapNhat(suKien) {
    var danhSachLoaiVe = suKien.LoaiVes || [];
    var danhSachGiaVe = danhSachLoaiVe
      .map(function (loaiVe) {
        return loaiVe.Gia == null ? NaN : Number(loaiVe.Gia);
      })
      .filter(function (giaVe) {
        return Number.isFinite(giaVe) && giaVe >= 0;
      });
    return danhSachGiaVe.length ? Math.min.apply(null, danhSachGiaVe) : null;
  }
  function layTrangThaiSuKien(suKien) {
    return trangThaiSuKienHienThi(suKien.TrangThai);
  }
  function dinhDangNgaySuKien(thoiGianSuKien) {
    var ngaySuKien = chuyenNgayDuLieuSangDate(thoiGianSuKien);
    return isNaN(ngaySuKien)
      ? "Thời gian đang cập nhật"
      : ngaySuKien.toLocaleDateString("vi-VN", {
          day: "2-digit",
          month: "short",
          year: "numeric",
        });
  }
  function docSuKienDaLuu() {
    try {
      var suKienDaLuu = JSON.parse(localStorage.getItem("qlsk_saved_events") || "[]");
      return Array.isArray(suKienDaLuu) ? suKienDaLuu : [];
    } catch (loiXuLy) {
      return [];
    }
  }
  function taoTheSuKien(suKien) {
    suKien = hopNhatChiTietSuKien(suKien);
    var suKienId = Number(suKien.Id),
      duongDanChiTietSuKien = "/pages/su-kien/chi-tiet.html?id=" + suKienId,
      diaDiem = layDiaDiemSuKien(suKien),
      giaVeThapNhat = layGiaVeThapNhat(suKien);
    return (
      '<article class="ev-card"><button class="ev-save" type="button" data-save-event="' +
      suKienId +
      '" aria-label="Lưu sự kiện ' +
      maHoaKyTuHtml(suKien.Ten) +
      '" aria-pressed="' +
      (docSuKienDaLuu().indexOf(suKienId) !== -1) +
      '">' +
      taoHtmlBieuTuong("heart") +
      '</button><a class="ev-cover" href="' +
      duongDanChiTietSuKien +
      '">' +
      anhSuKienHTML(suKien, true) +
      '<span class="ev-category">' +
      maHoaKyTuHtml((suKien.LoaiSuKien && suKien.LoaiSuKien.Ten) || "Sự kiện") +
      '</span></a><div class="ev-body"><div class="ev-date-line">' +
      maHoaKyTuHtml(dinhDangNgaySuKien(suKien.BatDau)) +
      '</div><h3 class="ev-title"><a href="' +
      duongDanChiTietSuKien +
      '">' +
      maHoaKyTuHtml(suKien.Ten) +
      '</a></h3><div class="ev-meta">' +
      taoHtmlBieuTuong("pin") +
      maHoaKyTuHtml(diaDiem.Ten || "Địa điểm đang cập nhật") +
      '</div><div class="ev-meta">' +
      taoHtmlBieuTuong("briefcase") +
      maHoaKyTuHtml((suKien.BanToChuc && suKien.BanToChuc.Ten) || "Ban tổ chức") +
      '</div><a class="ev-price" href="' +
      duongDanChiTietSuKien +
      '#tickets">' +
      (giaVeThapNhat === null
        ? "<small>Xem các hạng vé</small>"
        : giaVeThapNhat === 0
          ? "Miễn phí"
          : "<span><small>Từ </small>" + maHoaKyTuHtml(dinhDangTienDong(giaVeThapNhat)) + "</span>") +
      taoHtmlBieuTuong("arrow-up-right") +
      "</a></div></article>"
    );
  }
  function locDanhSachSuKien(danhSachSuKien) {
    var ngayTrongTuan = docGiaTriInput("filter-date"),
      locationKeyword = chuoiTimKiem(docGiaTriInput("filter-location")),
      priceRange = docGiaTriInput("filter-price");
    // Fetch metadata for the whole candidate set, including rows not on this page.
    if (locationKeyword || priceRange) xepHangTaiChiTietSuKien(danhSachSuKien);
    var thoiDiemHienTai = new Date(),
      thoiDiemKetThuc = new Date(thoiDiemHienTai);
    thoiDiemKetThuc.setDate(thoiDiemKetThuc.getDate() + 30);
    var suKienDaLoc = danhSachSuKien.filter(function (suKienTomTat) {
      var suKien = hopNhatChiTietSuKien(suKienTomTat),
        thoiDiemBatDau = chuyenNgayDuLieuSangDate(suKien.BatDau),
        giaVeThapNhat = layGiaVeThapNhat(suKien),
        diaDiem = layDiaDiemSuKien(suKien);
      if (boLocTrangThai && layTrangThaiSuKien(suKien) !== boLocTrangThai) return false;
      if (ngayTrongTuan === "today" && thoiDiemBatDau.toDateString() !== thoiDiemHienTai.toDateString()) return false;
      if (ngayTrongTuan === "month" && (thoiDiemBatDau < thoiDiemHienTai || thoiDiemBatDau > thoiDiemKetThuc)) return false;
      if (ngayTrongTuan === "weekend" && !suKienDienRaCuoiTuan(suKien)) return false;
      if (
        locationKeyword &&
        chuoiTimKiem((diaDiem.Ten || "") + " " + (diaDiem.DiaChi || "")).indexOf(locationKeyword) === -1
      )
        return false;
      if (priceRange === "free" && giaVeThapNhat !== 0) return false;
      if (priceRange === "under500" && (giaVeThapNhat === null || giaVeThapNhat >= 500000)) return false;
      if (priceRange === "over500" && (giaVeThapNhat === null || giaVeThapNhat < 500000)) return false;
      return true;
    });
    var thuTuSapXep = docGiaTriInput("event-sort");
    if (thuTuSapXep === "name")
      suKienDaLoc.sort(function (suKienTruoc, suKienSau) {
        return String(suKienTruoc.Ten).localeCompare(String(suKienSau.Ten), "vi");
      });
    else if (thuTuSapXep === "newest")
      suKienDaLoc.sort(function (suKienTruoc, suKienSau) {
        return Number(suKienSau.Id) - Number(suKienTruoc.Id);
      });
    else if (thuTuSapXep === "upcoming")
      suKienDaLoc.sort(function (suKienTruoc, suKienSau) {
        return chuyenNgayDuLieuSangDate(suKienTruoc.BatDau) - chuyenNgayDuLieuSangDate(suKienSau.BatDau);
      });
    return suKienDaLoc;
  }
  function taoBangSuKien(danhSachSuKien, phamViHienThi) {
    var nhanCacCotBang =
      phamViHienThi === "mine"
        ? ["Sự kiện", "Thời gian", "Địa điểm", "Trạng thái", "Thao tác"]
        : ["Sự kiện", "Ban tổ chức", "Thời gian", "Trạng thái", "Thao tác"];
    var noiDungBang =
      '<div class="responsive-records"><div class="tablescroll" tabindex="0" role="region" aria-label="Danh sách sự kiện"><table><thead><tr>' +
      nhanCacCotBang
        .map(function (nhanCot) {
          return "<th>" + nhanCot + "</th>";
        })
        .join("") +
      "</tr></thead><tbody>";
    danhSachSuKien.forEach(function (suKienTomTat) {
      var suKien = hopNhatChiTietSuKien(suKienTomTat),
        suKienId = Number(suKien.Id),
        diaDiem = layDiaDiemSuKien(suKien),
        trangThaiSuKien = layTrangThaiSuKien(suKien);
      var thaoTacCanThucThi =
        "<button class=\"phu sm\" onclick=\"giaoDienQuanLySuKien.moChiTietSuKien(" +
        suKienId +
        ')">Xem chi tiết ' +
        taoHtmlBieuTuong("arrow-up-right") +
        "</button>";
      if (phamViHienThi === "mine") {
        thaoTacCanThucThi =
          '<details class="row-actions"><summary aria-label="Thao tác sự kiện ' +
          maHoaKyTuHtml(suKien.Ten) +
          '">⋯</summary><div class="action-menu"><a href="/pages/su-kien/chi-tiet.html?id=' +
          suKienId +
          '">Xem sự kiện</a><a href="/pages/ve/loai-ve.html?event=' +
          suKienId +
          '">Quản lý hạng vé</a>' +
          (["DaHuy", "DaKetThuc"].indexOf(trangThaiSuKien) === -1
            ? '<button type="button" class="danger-action" onclick="huySuKien(' +
              suKienId +
              ')">Hủy sự kiện</button>'
            : "") +
          "</div></details>";
      }
      var cacGiaTriHienThi = [
        '<div class="table-event">' +
          anhSuKienHTML(suKien, true) +
          "<span><strong>" +
          maHoaKyTuHtml(suKien.Ten) +
          "</strong><small>" +
          maHoaKyTuHtml((suKien.LoaiSuKien && suKien.LoaiSuKien.Ten) || "Sự kiện") +
          "</small></span></div>",
        phamViHienThi === "mine"
          ? maHoaKyTuHtml(dinhDangNgayGioVietNam(suKien.BatDau))
          : maHoaKyTuHtml((suKien.BanToChuc && suKien.BanToChuc.Ten) || "—"),
        phamViHienThi === "mine"
          ? maHoaKyTuHtml(diaDiem.Ten || "Chưa cập nhật")
          : maHoaKyTuHtml(dinhDangNgayGioVietNam(suKien.BatDau)),
        taoNhanTrangThai(trangThaiSuKien),
        thaoTacCanThucThi,
      ];
      noiDungBang +=
        "<tr>" +
        cacGiaTriHienThi
          .map(function (noiDungOBang, chiSoCot) {
            return '<td data-label="' + nhanCacCotBang[chiSoCot] + '">' + noiDungOBang + "</td>";
          })
          .join("") +
        "</tr>";
    });
    return noiDungBang + "</tbody></table></div></div>";
  }
  function hienChiSoThongKe(tenChiSoThongKe, giaTriChiSoThongKe) {
    document.querySelectorAll('[data-metric="' + tenChiSoThongKe + '"]').forEach(function (phanTuHienThi) {
      phanTuHienThi.textContent = giaTriChiSoThongKe;
    });
  }
  function suKienDienRaCuoiTuan(suKien) {
    var ngaySuKien = chuyenNgayDuLieuSangDate(suKien.BatDau),
      thoiDiemHienTai = new Date(),
      dauTuanHienTai = new Date(thoiDiemHienTai);
    dauTuanHienTai.setDate(thoiDiemHienTai.getDate() - ((thoiDiemHienTai.getDay() + 6) % 7));
    dauTuanHienTai.setHours(0, 0, 0, 0);
    var dauTuanKeTiep = new Date(dauTuanHienTai);
    dauTuanKeTiep.setDate(dauTuanKeTiep.getDate() + 7);
    return ngaySuKien >= thoiDiemHienTai && ngaySuKien < dauTuanKeTiep && (ngaySuKien.getDay() === 0 || ngaySuKien.getDay() === 6);
  }
  function capNhatGiaoDienSauTaiSuKien(tatCaSuKien, suKienDangHienThi) {
    if (document.body.classList.contains("workspace-page")) {
      var suKienTrongPhamVi =
        phamViDanhSachSuKien() === "manage"
          ? tatCaSuKien.filter(function (suKien) {
              return layTrangThaiSuKien(suKien) === "ChoDuyet";
            })
          : tatCaSuKien;
      hienChiSoThongKe("events", suKienTrongPhamVi.length);
      hienChiSoThongKe(
        "pending",
        suKienTrongPhamVi.filter(function (suKien) {
          return layTrangThaiSuKien(suKien) === "ChoDuyet";
        }).length,
      );
      hienChiSoThongKe(
        "approved",
        suKienTrongPhamVi.filter(function (suKien) {
          return layTrangThaiSuKien(suKien) === "DaDuyet";
        }).length,
      );
      hienChiSoThongKe(
        "upcoming",
        suKienTrongPhamVi.filter(function (suKien) {
          return chuyenNgayDuLieuSangDate(suKien.BatDau) > new Date();
        }).length,
      );
      hienChiSoThongKe(
        "organizers",
        new Set(
          suKienTrongPhamVi
            .map(function (suKien) {
              return suKien.BanToChucId || (suKien.BanToChuc && suKien.BanToChuc.Id);
            })
            .filter(Boolean),
        ).size,
      );
    }
    if (document.body.classList.contains("home-page")) hienCacNhomSuKienTrangChu(tatCaSuKien);
    xepHangTaiChiTietSuKien(suKienDangHienThi);
    capNhatVanBan(
      "event-metadata-status",
      soLuongLuotTaiSongSong || hangDoiTaiChiTiet.length
        ? "Đang bổ sung địa điểm và giá vé…"
        : suKienTaiChiTietThatBai.size
          ? "Một số thông tin chưa tải được. Làm mới danh sách để thử lại."
          : "",
    );
    var trangThaiHienThi = timPhanTuTheoId("event-metadata-status");
    if (trangThaiHienThi && suKienTaiChiTietThatBai.size && !soLuongLuotTaiSongSong && !hangDoiTaiChiTiet.length) {
      var hamThuLai = document.createElement("button");
      hamThuLai.className = "text-button";
      hamThuLai.type = "button";
      hamThuLai.textContent = "Thử lại";
      hamThuLai.onclick = function () {
        suKienTaiChiTietThatBai.clear();
        xepHangTaiChiTietSuKien(__suKienCache || []);
        hienThiDanhSachSuKien();
      };
      trangThaiHienThi.appendChild(hamThuLai);
    }
  }
  function ghiNhanDaTaiSuKien() {
    // List data is authoritative on every refresh; do not reuse stale detail data.
    phienTaiDanhSach++;
    chiTietSuKienTheoId.clear();
    suKienDangTaiChiTiet.clear();
    suKienTaiChiTietThatBai.clear();
    hangDoiTaiChiTiet = [];
  }
  function ghiNhanDangTaiThongTinBoSung() {
    return !!(soLuongLuotTaiSongSong || hangDoiTaiChiTiet.length);
  }
  function xepHangTaiChiTietSuKien(danhSachSuKien) {
    danhSachSuKien.forEach(function (suKien) {
      var suKienId = Number(suKien.Id);
      if (suKienId > 0 && !chiTietSuKienTheoId.has(suKienId) && !suKienDangTaiChiTiet.has(suKienId) && !suKienTaiChiTietThatBai.has(suKienId)) {
        suKienDangTaiChiTiet.add(suKienId);
        hangDoiTaiChiTiet.push({
          id: suKienId,
          generation: phienTaiDanhSach,
        });
      }
    });
    taiChiTietSuKienTrongHang();
  }
  function taiChiTietSuKienTrongHang() {
    while (soLuongLuotTaiSongSong < 4 && hangDoiTaiChiTiet.length) {
      (function (yeuCauTaiChiTiet) {
        var suKienId = yeuCauTaiChiTiet.id;
        soLuongLuotTaiSongSong++;
        guiYeuCauApi("/api/events/" + suKienId)
          .then(function (phanHoiChiTiet) {
            if (yeuCauTaiChiTiet.generation === phienTaiDanhSach)
              chiTietSuKienTheoId.set(suKienId, chuanHoaDuLieuSuKien(phanHoiChiTiet.data || {}));
          })
          .catch(function () {
            if (yeuCauTaiChiTiet.generation === phienTaiDanhSach) suKienTaiChiTietThatBai.add(suKienId);
          })
          .finally(function () {
            if (yeuCauTaiChiTiet.generation === phienTaiDanhSach) suKienDangTaiChiTiet.delete(suKienId);
            soLuongLuotTaiSongSong--;
            taiChiTietSuKienTrongHang();
            if (!soLuongLuotTaiSongSong && !hangDoiTaiChiTiet.length && Array.isArray(__suKienCache) && timPhanTuTheoId("ds-sukien"))
              hienThiDanhSachSuKien();
          });
      })(hangDoiTaiChiTiet.shift());
    }
  }
  function hienCacNhomSuKienTrangChu(danhSachSuKien) {
    var suKienDuChiTiet = danhSachSuKien.map(hopNhatChiTietSuKien),
      danhSachSuKienSapDienRa = suKienDuChiTiet
        .filter(function (suKien) {
          return chuyenNgayDuLieuSangDate(suKien.BatDau) >= new Date();
        })
        .sort(function (suKienTruoc, suKienSau) {
          return chuyenNgayDuLieuSangDate(suKienTruoc.BatDau) - chuyenNgayDuLieuSangDate(suKienSau.BatDau);
        });
    var suKienTheoNhom = {
      upcoming: danhSachSuKienSapDienRa.slice(0, 4),
      weekend: danhSachSuKienSapDienRa.filter(suKienDienRaCuoiTuan).slice(0, 4),
      free: danhSachSuKienSapDienRa
        .filter(function (suKien) {
          return layGiaVeThapNhat(suKien) === 0;
        })
        .slice(0, 4),
    };
    Object.keys(suKienTheoNhom).forEach(function (tenNhomSuKien) {
      var khungNhomSuKienTrangChu = document.querySelector('[data-home-feed="' + tenNhomSuKien + '"]');
      if (khungNhomSuKienTrangChu) {
        khungNhomSuKienTrangChu.hidden = !suKienTheoNhom[tenNhomSuKien].length;
        var luoiSuKien = khungNhomSuKienTrangChu.querySelector(".ev-grid"),
          noiDungHopChiTiet = suKienTheoNhom[tenNhomSuKien].map(taoTheSuKien).join("");
        if (luoiSuKien.__uiHTML !== noiDungHopChiTiet) {
          luoiSuKien.innerHTML = noiDungHopChiTiet;
          luoiSuKien.__uiHTML = noiDungHopChiTiet;
        }
      }
    });
    var noiDungNoiBat = danhSachSuKienSapDienRa[0];
    if (noiDungNoiBat && timPhanTuTheoId("home-highlight")) {
      var khungNoiBat = timPhanTuTheoId("home-highlight");
      khungNoiBat.href = "/pages/su-kien/chi-tiet.html?id=" + Number(noiDungNoiBat.Id);
      var duongDanHinhAnh = khungNoiBat.querySelector(".spotlight-image"),
        noiDungHtml = anhSuKienHTML(noiDungNoiBat, false);
      if (duongDanHinhAnh.__uiHTML !== noiDungHtml) {
        duongDanHinhAnh.innerHTML = noiDungHtml;
        duongDanHinhAnh.__uiHTML = noiDungHtml;
      }
      khungNoiBat.querySelector("strong").textContent = noiDungNoiBat.Ten;
      khungNoiBat.querySelector(".spotlight-content .eyebrow").textContent = dinhDangNgaySuKien(noiDungNoiBat.BatDau);
      khungNoiBat.hidden = false;
    } else if (timPhanTuTheoId("home-highlight")) timPhanTuTheoId("home-highlight").hidden = true;
    var soSuKienTheoToChuc = new Map();
    danhSachSuKien.forEach(function (suKien) {
      var tenBanToChuc = suKien.BanToChuc && suKien.BanToChuc.Ten;
      if (tenBanToChuc) soSuKienTheoToChuc.set(tenBanToChuc, (soSuKienTheoToChuc.get(tenBanToChuc) || 0) + 1);
    });
    var khungToChucTrangChu = document.querySelector("[data-home-organizers]");
    if (khungToChucTrangChu) {
      khungToChucTrangChu.hidden = !soSuKienTheoToChuc.size;
      khungToChucTrangChu.querySelector(".organizer-grid").innerHTML = Array.from(soSuKienTheoToChuc)
        .sort(function (toChucTruoc, toChucSau) {
          return toChucSau[1] - toChucTruoc[1];
        })
        .slice(0, 4)
        .map(function (capTenToChucVaSoSuKien) {
          return (
            '<a class="organizer-tile" href="/pages/su-kien/danh-sach.html?q=' +
            encodeURIComponent(capTenToChucVaSoSuKien[0]) +
            '"><span>' +
            maHoaKyTuHtml(layChuCaiDauTen(capTenToChucVaSoSuKien[0])) +
            "</span><div><b>" +
            maHoaKyTuHtml(capTenToChucVaSoSuKien[0]) +
            "</b><small>" +
            capTenToChucVaSoSuKien[1] +
            " sự kiện trên QLSK</small></div></a>"
          );
        })
        .join("");
    }
    // A free or weekend event can occur beyond the first eight list rows.
    xepHangTaiChiTietSuKien(danhSachSuKienSapDienRa);
  }
  function taoHtmlChiTietSuKien(suKien) {
    chiTietSuKienTheoId.set(Number(suKien.Id), suKien);
    var diaDiem = layDiaDiemSuKien(suKien),
      giaVeThapNhat = layGiaVeThapNhat(suKien);
    return (
      '<div class="event-detail-cover">' +
      anhSuKienHTML(suKien, false) +
      '</div><div class="event-detail-grid"><article class="event-info"><span class="badge b-info">' +
      maHoaKyTuHtml((suKien.LoaiSuKien && suKien.LoaiSuKien.Ten) || "Sự kiện") +
      "</span><h1>" +
      maHoaKyTuHtml(suKien.Ten) +
      '</h1><div class="event-facts"><div class="event-fact">' +
      taoHtmlBieuTuong("calendar") +
      "<div><small>Thời gian diễn ra</small><b>" +
      maHoaKyTuHtml(dinhDangNgayGioVietNam(suKien.BatDau)) +
      "</b><p>Kết thúc: " +
      maHoaKyTuHtml(dinhDangNgayGioVietNam(suKien.KetThuc)) +
      '</p></div></div><div class="event-fact" id="event-venue">' +
      taoHtmlBieuTuong("pin") +
      "<div><small>Địa điểm</small><b>" +
      maHoaKyTuHtml(diaDiem.Ten || "Đang cập nhật") +
      "</b><p>" +
      maHoaKyTuHtml(diaDiem.DiaChi || "Thông tin địa điểm sẽ được ban tổ chức bổ sung.") +
      '</p></div></div><div class="event-fact">' +
      taoHtmlBieuTuong("briefcase") +
      "<div><small>Ban tổ chức</small><b>" +
      maHoaKyTuHtml((suKien.BanToChuc && suKien.BanToChuc.Ten) || "Đang cập nhật") +
      '</b></div></div></div><nav class="event-content-tabs" aria-label="Nội dung sự kiện"><a href="#event-about">Giới thiệu</a><a href="#tickets">Vé tham dự</a><a href="#event-venue">Địa điểm</a></nav><h2 id="event-about">Về sự kiện</h2><p class="event-description">' +
      maHoaKyTuHtml(suKien.MoTa || "Thông tin sự kiện đang được cập nhật.") +
      '</p></article><aside class="booking-card"><p class="eyebrow">TRẢI NGHIỆM TIẾP THEO CỦA BẠN</p><div class="booking-price">' +
      (giaVeThapNhat === null
        ? "Chọn hạng vé"
        : giaVeThapNhat === 0
          ? "Miễn phí"
          : "<small>Từ </small>" + maHoaKyTuHtml(dinhDangTienDong(giaVeThapNhat))) +
      "</div>" +
      taoNhanTrangThai(layTrangThaiSuKien(suKien)) +
      '<a class="chinh full-width" href="#tickets">Đặt vé ngay ' +
      taoHtmlBieuTuong("arrow-right") +
      '</a><p class="secure-note">' +
      taoHtmlBieuTuong("shield") +
      ' Đặt vé qua tài khoản QLSK<br/>Nhận vé điện tử sau xác nhận thanh toán</p><div class="info-note">' +
      taoHtmlBieuTuong("calendar") +
      "<p>Hãy kiểm tra thời gian và địa điểm trước khi đặt vé.</p></div></aside></div>"
    );
  }
  function khoiPhucTieuDiemSauDongChiTiet() {
    var phanTuDich =
      phanTuCoTieuDiemTruocDo && document.contains(phanTuCoTieuDiemTruocDo)
        ? phanTuCoTieuDiemTruocDo
        : document.querySelector(".results-toolbar button, main h1");
    if (phanTuDich) {
      if (phanTuDich.tagName === "H1") phanTuDich.tabIndex = -1;
      phanTuDich.focus({
        preventScroll: true,
      });
    }
  }
  function dongHopChiTiet() {
    lanTaiHopChiTiet++;
    var khungChiTiet = timPhanTuTheoId("ui-drawer");
    if (khungChiTiet && khungChiTiet.open) khungChiTiet.close();
    khoiPhucTieuDiemSauDongChiTiet();
  }
  function moHopChiTiet(noiDungHopChiTiet) {
    var khungChiTiet = timPhanTuTheoId("ui-drawer");
    if (!khungChiTiet) return;
    if (!khungChiTiet.open) phanTuCoTieuDiemTruocDo = document.activeElement;
    capNhatHtml("ui-drawer-content", noiDungHopChiTiet);
    if (!khungChiTiet.open) khungChiTiet.showModal();
    khungChiTiet.scrollTop = 0;
  }
  async function moChiTietSuKien(suKienId) {
    var lanTaiChiTietHienTai = ++lanTaiHopChiTiet,
      viewGeneration = phienTaiDanhSach;
    moHopChiTiet('<h2 id="ui-drawer-title">Chi tiết sự kiện</h2><div id="drawer-event-loading"></div>');
    hienKhungDangTai("drawer-event-loading", 4);
    try {
      var phanHoiSuKien = await guiYeuCauApi("/api/events/" + Number(suKienId)),
        suKien = chuanHoaDuLieuSuKien(phanHoiSuKien.data || {}),
        diaDiem = layDiaDiemSuKien(suKien);
      if (lanTaiChiTietHienTai !== lanTaiHopChiTiet || !timPhanTuTheoId("ui-drawer").open) return;
      if (viewGeneration === phienTaiDanhSach) chiTietSuKienTheoId.set(Number(suKienId), suKien);
      var banToChucSuKien = Number(suKien.BanToChuc && suKien.BanToChuc.TaiKhoanId) === Number(phienDangNhap.taiKhoanID);
      var canApprove =
        ["QuanLySuKien", "QuanTriVien"].indexOf(phienDangNhap.vaiTro) !== -1 &&
        !banToChucSuKien &&
        layTrangThaiSuKien(suKien) === "ChoDuyet";
      var cacTruongNhapLieu = [
        ["Ban tổ chức", suKien.BanToChuc && suKien.BanToChuc.Ten],
        ["Bắt đầu", dinhDangNgayGioVietNam(suKien.BatDau)],
        ["Kết thúc", dinhDangNgayGioVietNam(suKien.KetThuc)],
        ["Địa điểm", diaDiem.Ten || "Chưa cập nhật"],
        ["Địa chỉ", diaDiem.DiaChi || "Chưa cập nhật"],
        ["Quy mô", suKien.SoNguoi ? dinhDangSoTien(suKien.SoNguoi) + " người" : "Chưa cập nhật"],
      ];
      capNhatHtml(
        "ui-drawer-content",
        '<div class="drawer-cover">' +
          anhSuKienHTML(suKien, false) +
          '</div><h2 id="ui-drawer-title">' +
          maHoaKyTuHtml(suKien.Ten) +
          "</h2>" +
          taoNhanTrangThai(layTrangThaiSuKien(suKien)) +
          '<dl class="detail-list">' +
          cacTruongNhapLieu
            .map(function (capThongTin) {
              return (
                "<div><dt>" + capThongTin[0] + "</dt><dd>" + maHoaKyTuHtml(capThongTin[1] || "—") + "</dd></div>"
              );
            })
            .join("") +
          '</dl><div class="drawer-section"><h3>Giới thiệu</h3><p>' +
          maHoaKyTuHtml(suKien.MoTa || "Chưa có mô tả.") +
          '</p></div><div class="drawer-section"><h3>Hạng vé</h3>' +
          (suKien.LoaiVes && suKien.LoaiVes.length
            ? suKien.LoaiVes.map(function (loaiVe) {
                return (
                  '<div class="summary-lines"><div><dt>' +
                  maHoaKyTuHtml(loaiVe.Ten) +
                  "</dt><dd>" +
                  maHoaKyTuHtml(dinhDangTienDong(loaiVe.Gia)) +
                  " · " +
                  maHoaKyTuHtml(loaiVe.SoLuong) +
                  " vé</dd></div></div>"
                );
              }).join("")
            : '<p class="muted">Chưa thiết lập hạng vé.</p>') +
          "</div>" +
          (canApprove
            ? "<div class=\"drawer-actions\"><button class=\"phu danger-action\" onclick=\"giaoDienQuanLySuKien.tuChoiSuKienTrongHopChiTiet(" +
              Number(suKienId) +
              ")\">Từ chối</button><button class=\"chinh\" onclick=\"giaoDienQuanLySuKien.duyetSuKienTrongHopChiTiet(" +
              Number(suKienId) +
              ')">Duyệt sự kiện ' +
              taoHtmlBieuTuong("check") +
              "</button></div>"
            : ""),
      );
    } catch (loiXuLy) {
      if (lanTaiChiTietHienTai === lanTaiHopChiTiet) {
        capNhatHtml(
          "ui-drawer-content",
          '<h2 id="ui-drawer-title">Chưa thể tải sự kiện</h2><p>' +
            maHoaKyTuHtml(loiXuLy.message) +
            "</p><button class=\"phu\" onclick=\"giaoDienQuanLySuKien.moChiTietSuKien(" +
            Number(suKienId) +
            ')">Thử lại</button>',
        );
      }
    }
  }
  function duyetSuKienTrongHopChiTiet(suKienId) {
    return thucThiThaoTacHopChiTiet(function () {
      return duyetSuKien(suKienId);
    });
  }
  function tuChoiSuKienTrongHopChiTiet(suKienId) {
    return thucThiThaoTacHopChiTiet(function () {
      return tuChoiSuKien(suKienId);
    });
  }
  async function thucThiThaoTacHopChiTiet(thaoTacCanThucThi) {
    var cacNutThaoTac = timPhanTuTheoId("ui-drawer").querySelectorAll(".drawer-actions button");
    if (
      Array.from(cacNutThaoTac).some(function (nutThaoTac) {
        return nutThaoTac.disabled;
      })
    )
      return;
    cacNutThaoTac.forEach(function (nutThaoTac) {
      nutThaoTac.disabled = true;
    });
    try {
      if (await thaoTacCanThucThi()) dongHopChiTiet();
    } finally {
      cacNutThaoTac.forEach(function (nutThaoTac) {
        nutThaoTac.disabled = false;
      });
    }
  }
  function hienYeuCauXacThuc(yeuCauXacThucMoiTai) {
    danhSachYeuCauXacThuc = yeuCauXacThucMoiTai;
    return (
      '<div class="results-toolbar"><p>' +
      yeuCauXacThucMoiTai.length +
      " hồ sơ đang chờ xét duyệt</p><button class=\"phu sm\" onclick=\"hienDanhSachDuyetToChuc()\">" +
      taoHtmlBieuTuong("refresh") +
      "Làm mới</button></div>" +
      taoHtmlBang(
        [
          ["TenToChuc", "Tên đơn vị"],
          ["HoTen", "Người đại diện"],
          ["Email", "Email"],
          ["NgayTao", "Ngày gửi", "ngay"],
        ],
        yeuCauXacThucMoiTai.map(function (yeuCauXacThuc) {
          return Object.assign({}, yeuCauXacThuc, {
            HoTen: (yeuCauXacThuc.TaiKhoan && yeuCauXacThuc.TaiKhoan.HoTen) || "—",
            Email: (yeuCauXacThuc.TaiKhoan && yeuCauXacThuc.TaiKhoan.Email) || "—",
          });
        }),
        function (yeuCauXacThuc) {
          return (
            "<button class=\"phu sm\" onclick=\"giaoDienQuanLySuKien.moChiTietYeuCauXacThuc(" +
            Number(yeuCauXacThuc.Id) +
            ')">Xem hồ sơ ' +
            taoHtmlBieuTuong("arrow-up-right") +
            "</button>"
          );
        },
      ) +
      '<div hidden><input id="dtc-id"/><input id="dtc-ten"/><input id="dtc-lydo"/></div>'
    );
  }
  function moChiTietYeuCauXacThuc(yeuCauXacThucId) {
    lanTaiHopChiTiet++;
    var yeuCauXacThuc = danhSachYeuCauXacThuc.find(function (yeuCauXacThuc) {
      return Number(yeuCauXacThuc.Id) === Number(yeuCauXacThucId);
    });
    if (!yeuCauXacThuc) return;
    var noiDungDaMaHoaHtml = "";
    try {
      var urlGiayTo = new URL(yeuCauXacThuc.GiayTo);
      if (/^https?:$/.test(urlGiayTo.protocol)) noiDungDaMaHoaHtml = urlGiayTo.href;
    } catch (loiXuLy) {}
    moHopChiTiet(
      '<h2 id="ui-drawer-title">' +
        maHoaKyTuHtml(yeuCauXacThuc.TenToChuc) +
        '</h2><span class="badge b-warn">Chờ xét duyệt</span><dl class="detail-list"><div><dt>Người đại diện</dt><dd>' +
        maHoaKyTuHtml((yeuCauXacThuc.TaiKhoan && yeuCauXacThuc.TaiKhoan.HoTen) || "Chưa có thông tin") +
        "</dd></div><div><dt>Email</dt><dd>" +
        maHoaKyTuHtml((yeuCauXacThuc.TaiKhoan && yeuCauXacThuc.TaiKhoan.Email) || "Chưa có thông tin") +
        "</dd></div><div><dt>Ngày gửi</dt><dd>" +
        maHoaKyTuHtml(dinhDangNgayGioVietNam(yeuCauXacThuc.NgayTao)) +
        '</dd></div></dl><div class="drawer-section"><h3>Giấy tờ xác minh</h3>' +
        (noiDungDaMaHoaHtml
          ? '<a class="phu" target="_blank" rel="noopener noreferrer" href="' +
            maHoaKyTuHtml(noiDungDaMaHoaHtml) +
            '">' +
            taoHtmlBieuTuong("external") +
            "Mở tài liệu xác minh</a>"
          : '<p class="muted">Đường dẫn tài liệu không khả dụng.</p>') +
        '</div><div class="info-note">' +
        taoHtmlBieuTuong("shield") +
        "<p>Kiểm tra tên đơn vị và hiệu lực giấy tờ trước khi phê duyệt. Khi từ chối, hệ thống sẽ yêu cầu nhập lý do.</p></div><div class=\"drawer-actions\"><button class=\"phu danger-action\" onclick=\"giaoDienQuanLySuKien.tuChoiYeuCauXacThucTrongHopChiTiet(" +
        Number(yeuCauXacThucId) +
        ")\">Từ chối</button><button class=\"chinh\" onclick=\"giaoDienQuanLySuKien.duyetYeuCauXacThucTrongHopChiTiet(" +
        Number(yeuCauXacThucId) +
        ')">Duyệt tổ chức ' +
        taoHtmlBieuTuong("check") +
        "</button></div>",
    );
  }
  function duyetYeuCauXacThucTrongHopChiTiet(yeuCauXacThucId) {
    return thucThiThaoTacYeuCauXacThuc(function () {
      return duyetYeuCauToChuc(yeuCauXacThucId);
    });
  }
  function tuChoiYeuCauXacThucTrongHopChiTiet(yeuCauXacThucId) {
    return thucThiThaoTacYeuCauXacThuc(function () {
      return tuChoiYeuCauToChuc(yeuCauXacThucId);
    });
  }
  function thucThiThaoTacYeuCauXacThuc(thaoTacCanThucThi) {
    return thucThiThaoTacHopChiTiet(async function () {
      var xuLyThanhCong = await thaoTacCanThucThi();
      if (xuLyThanhCong) await hienDanhSachDuyetToChuc();
      return xuLyThanhCong;
    });
  }
  function hienDanhSachTaiKhoan(taiKhoanMoiTai) {
    danhSachTaiKhoan = taiKhoanMoiTai;
    capNhatDanhSachTaiKhoan();
  }
  function capNhatDanhSachTaiKhoan() {
    var vaiTroTaiKhoan = docGiaTriInput("account-role-filter"),
      dangHoatDong = docGiaTriInput("account-status-filter");
    var taiKhoanDaLoc = danhSachTaiKhoan.filter(function (taiKhoan) {
      return (!vaiTroTaiKhoan || taiKhoan.VaiTro === vaiTroTaiKhoan) && (!dangHoatDong || String(!!taiKhoan.HoatDong) === dangHoatDong);
    });
    var cacNhanHienThi = ["Người dùng", "Email", "Vai trò", "Trạng thái", "Thao tác"];
    capNhatHtml(
      "ds-taikhoan",
      taiKhoanDaLoc.length
        ? '<div class="responsive-records"><div class="tablescroll" tabindex="0" role="region" aria-label="Người dùng"><table><thead><tr>' +
            cacNhanHienThi
              .map(function (nhanCot) {
                return "<th>" + nhanCot + "</th>";
              })
              .join("") +
            "</tr></thead><tbody>" +
            taiKhoanDaLoc
              .map(function (taiKhoan) {
                var cacGiaTriHienThi = [
                  '<div class="table-person"><span class="avatar">' +
                    maHoaKyTuHtml(layChuCaiDauTen(taiKhoan.HoTen)) +
                    "</span><span><b>" +
                    maHoaKyTuHtml(taiKhoan.HoTen) +
                    "</b><small>#" +
                    Number(taiKhoan.Id) +
                    "</small></span></div>",
                  maHoaKyTuHtml(taiKhoan.Email),
                  taoNhanTrangThai(taiKhoan.VaiTro),
                  '<span class="badge ' +
                    (taiKhoan.HoatDong ? "b-ok" : "b-err") +
                    '">' +
                    (taiKhoan.HoatDong ? "Đang hoạt động" : "Đã khóa") +
                    "</span>",
                  '<details class="row-actions"><summary aria-label="Thao tác với ' +
                    maHoaKyTuHtml(taiKhoan.HoTen) +
                    '">⋯</summary><div class="action-menu"><button onclick="khoaTaiKhoan(' +
                    Number(taiKhoan.Id) +
                    ')" class="danger-action">Khóa tài khoản</button><button onclick="moKhoaTaiKhoan(' +
                    Number(taiKhoan.Id) +
                    ')">Mở khóa</button><button onclick="doiVaiTroTaiKhoan(' +
                    Number(taiKhoan.Id) +
                    ')">Đổi vai trò</button></div></details>',
                ];
                return (
                  "<tr>" +
                  cacGiaTriHienThi
                    .map(function (giaTriO, chiSoCot) {
                      return '<td data-label="' + cacNhanHienThi[chiSoCot] + '">' + giaTriO + "</td>";
                    })
                    .join("") +
                  "</tr>"
                );
              })
              .join("") +
            "</tbody></table></div></div>"
        : taoHtmlBang([], []),
    );
  }
  function hienBieuDoCot(phanTuDomId, cacNhomSoLieuBieuDo, giaTriLaSoTien) {
    if (!cacNhomSoLieuBieuDo.length) {
      capNhatHtml(phanTuDomId, '<div class="empty"><p>Chưa có dữ liệu trong phạm vi này.</p></div>');
      return;
    }
    var giaTriLonNhat = Math.max.apply(
      null,
      cacNhomSoLieuBieuDo
        .map(function (cotThongKe) {
          return Number(cotThongKe.value) || 0;
        })
        .concat([1]),
    );
    capNhatHtml(
      phanTuDomId,
      '<div class="bar-chart">' +
        cacNhomSoLieuBieuDo
          .map(function (cotThongKe) {
            return (
              '<div class="bar-item"><div><span>' +
              maHoaKyTuHtml(nhanTrangThai(cotThongKe.label)) +
              "</span><strong>" +
              maHoaKyTuHtml(giaTriLaSoTien ? dinhDangTienDong(cotThongKe.value) : dinhDangSoTien(cotThongKe.value)) +
              '</strong></div><div class="bar-track"><i style="width:' +
              Math.max(0, (Number(cotThongKe.value) / giaTriLonNhat) * 100) +
              '%"></i></div></div>'
            );
          })
          .join("") +
        "</div>",
    );
  }
  function hienBieuDoVong(phanTuDomId, cacNhomSoLieuBieuDo) {
    var tongGiaTriBieuDo = cacNhomSoLieuBieuDo.reduce(function (tongSoLuong, nhomThongKe) {
        return tongSoLuong + Number(nhomThongKe.soLuong || nhomThongKe.SoLuong || 0);
      }, 0),
      cacMauBieuDo = ["#1570ef", "#53b1fd", "#12b76a", "#f79009", "#98a2b3", "#f04438"],
      gocBieuDo = 0;
    var cacMocMauBieuDo = cacNhomSoLieuBieuDo.map(function (nhomThongKe, chiSoNhom) {
      var gocBatDauNhom = gocBieuDo;
      gocBieuDo += tongGiaTriBieuDo ? (Number(nhomThongKe.soLuong || nhomThongKe.SoLuong || 0) / tongGiaTriBieuDo) * 100 : 0;
      return cacMauBieuDo[chiSoNhom % cacMauBieuDo.length] + " " + gocBatDauNhom + "% " + gocBieuDo + "%";
    });
    capNhatHtml(
      phanTuDomId,
      '<div class="donut-wrap"><div class="donut" style="background:conic-gradient(' +
        (tongGiaTriBieuDo ? cacMocMauBieuDo.join(",") : "#eaecf0 0% 100%") +
        ')"><strong>' +
        dinhDangSoTien(tongGiaTriBieuDo) +
        '<small>sự kiện</small></strong></div><div class="chart-legend">' +
        cacNhomSoLieuBieuDo
          .map(function (nhomThongKe, chiSoNhom) {
            return (
              '<div><i style="background:' +
              cacMauBieuDo[chiSoNhom % cacMauBieuDo.length] +
              '"></i><span>' +
              maHoaKyTuHtml(nhanTrangThai(nhomThongKe.trangThai || nhomThongKe.TrangThai)) +
              "</span><b>" +
              maHoaKyTuHtml(nhomThongKe.soLuong || nhomThongKe.SoLuong || 0) +
              "</b></div>"
            );
          })
          .join("") +
        "</div></div>",
    );
  }
  function hienLoiTaiTongQuan() {
    duLieuTongQuan = null;
    document.querySelectorAll("[data-metric]").forEach(function (khungChiSo) {
      khungChiSo.textContent = "—";
    });
  }
  function hienTongQuanQuanTri(thongKe) {
    duLieuTongQuan = thongKe;
    ["taiKhoan", "suKien", "ve", "suKienChoDuyet"].forEach(function (tenChiSo) {
      hienChiSoThongKe(tenChiSo, dinhDangSoTien(thongKe[tenChiSo]));
    });
    hienChiSoThongKe("doanhThu", dinhDangTienDong(thongKe.doanhThu));
    hienChiSoThongKe("pendingOrders", dinhDangSoTien(Number(thongKe.don) - Number(thongKe.donThanhCong)));
    var thongKeBanToChuc = (thongKe.theoVaiTro || []).find(function (nhomThongKe) {
      return nhomThongKe.vaiTro === "BanToChuc";
    });
    hienChiSoThongKe("organizers", thongKeBanToChuc ? dinhDangSoTien(thongKeBanToChuc.soLuong) : "0");
    hienBieuDoCot(
      "payment-chart",
      (thongKe.theoTrangThaiThanhToan || []).map(function (nhomThongKe) {
        return {
          label: nhomThongKe.trangThai,
          value: nhomThongKe.soTien,
        };
      }),
      true,
    );
    hienBieuDoCot(
      "ticket-chart",
      (thongKe.theoTrangThaiVe || []).map(function (nhomThongKe) {
        return {
          label: nhomThongKe.trangThai,
          value: nhomThongKe.soLuong,
        };
      }),
    );
    hienBieuDoVong("event-status-chart", thongKe.theoTrangThaiSuKien || []);
  }
  function hienDanhSachDatDiaDiem(datDiaDiemMoiTai) {
    danhSachDatDiaDiem = datDiaDiemMoiTai;
    capNhatDanhSachDatDiaDiem();
  }
  function layTrangThaiDatDiaDiem(guiPhieuDatDiaDiem) {
    return typeof guiPhieuDatDiaDiem.TrangThai === "number"
      ? ["ChoXacNhan", "DaXacNhan", "TuChoi", "DaHuy", "HoanThanh"][guiPhieuDatDiaDiem.TrangThai]
      : guiPhieuDatDiaDiem.TrangThai;
  }
  function capNhatDanhSachDatDiaDiem() {
    var ngayTrongTuan = docGiaTriInput("booking-date-filter"),
      tuKhoaTimKiem = chuoiTimKiem(docGiaTriInput("booking-venue-filter")),
      trangThaiHienThi = docGiaTriInput("booking-status-filter");
    var cacTrangThaiHienThi = ["ChoXacNhan", "DaXacNhan", "TuChoi", "DaHuy", "HoanThanh"];
    var thoiDiemBatDauLoc = ngayTrongTuan ? new Date(ngayTrongTuan + "T00:00:00") : null,
      thoiDiemKetThucLoc = ngayTrongTuan ? new Date(ngayTrongTuan + "T00:00:00") : null;
    if (thoiDiemKetThucLoc) thoiDiemKetThucLoc.setDate(thoiDiemKetThucLoc.getDate() + 1);
    var datDiaDiemDaLoc = danhSachDatDiaDiem.filter(function (guiPhieuDatDiaDiem) {
      return (
        (!ngayTrongTuan || (chuyenNgayDuLieuSangDate(guiPhieuDatDiaDiem.BatDau) < thoiDiemKetThucLoc && chuyenNgayDuLieuSangDate(guiPhieuDatDiaDiem.KetThuc) > thoiDiemBatDauLoc)) &&
        (!tuKhoaTimKiem || chuoiTimKiem((guiPhieuDatDiaDiem.DiaDiem && guiPhieuDatDiaDiem.DiaDiem.Ten) || "").includes(tuKhoaTimKiem)) &&
        (!trangThaiHienThi || layTrangThaiDatDiaDiem(guiPhieuDatDiaDiem) === cacTrangThaiHienThi[Number(trangThaiHienThi)])
      );
    });
    capNhatHtml(
      "ds-dat",
      taoHtmlBang(
        [
          ["TenSuKien", "Sự kiện"],
          ["TenDiaDiem", "Địa điểm"],
          ["BatDau", "Bắt đầu", "ngay"],
          ["KetThuc", "Kết thúc", "ngay"],
          ["TrangThai", "Trạng thái"],
          ["TienCoc", "Tiền cọc", "tien"],
        ],
        datDiaDiemDaLoc.map(function (guiPhieuDatDiaDiem) {
          return Object.assign({}, guiPhieuDatDiaDiem, {
            TenSuKien: (guiPhieuDatDiaDiem.SuKien && guiPhieuDatDiaDiem.SuKien.Ten) || "—",
            TenDiaDiem: (guiPhieuDatDiaDiem.DiaDiem && guiPhieuDatDiaDiem.DiaDiem.Ten) || "—",
            TrangThai: layTrangThaiDatDiaDiem(guiPhieuDatDiaDiem),
          });
        }),
        function (guiPhieuDatDiaDiem) {
          var coQuyenQuanLy = ["QuanLySuKien", "QuanTriVien"].includes(phienDangNhap.vaiTro),
            trangThaiDat = layTrangThaiDatDiaDiem(guiPhieuDatDiaDiem),
            cacThaoTacChoPhep = "";
          if (coQuyenQuanLy && trangThaiDat === "ChoXacNhan")
            cacThaoTacChoPhep +=
              "<button onclick=\"xacNhanDatDiaDiem(" +
              Number(guiPhieuDatDiaDiem.Id) +
              ")\">Xác nhận</button><button onclick=\"tuChoiDatDiaDiem(" +
              Number(guiPhieuDatDiaDiem.Id) +
              ')">Từ chối</button>';
          if (phienDangNhap.vaiTro === "BanToChuc" && ["ChoXacNhan", "DaXacNhan"].includes(trangThaiDat))
            cacThaoTacChoPhep +=
              "<button class=\"danger-action\" onclick=\"huyDatDiaDiem(" +
              Number(guiPhieuDatDiaDiem.Id) +
              ')">Hủy phiếu đặt</button>';
          return cacThaoTacChoPhep
            ? '<details class="row-actions"><summary aria-label="Thao tác phiếu đặt">⋯</summary><div class="action-menu">' +
                cacThaoTacChoPhep +
                "</div></details>"
            : "—";
        },
      ),
    );
    var phieuDatTheoNgayDiaDiem = new Map();
    datDiaDiemDaLoc.forEach(function (guiPhieuDatDiaDiem) {
      var khoaNgayDiaDiem =
        String(
          guiPhieuDatDiaDiem.DiaDiemId ||
            (guiPhieuDatDiaDiem.DiaDiem && guiPhieuDatDiaDiem.DiaDiem.Id) ||
            (guiPhieuDatDiaDiem.DiaDiem && guiPhieuDatDiaDiem.DiaDiem.Ten),
        ) +
        "|" +
        chuyenNgayDuLieuSangDate(guiPhieuDatDiaDiem.BatDau).toLocaleDateString("vi-VN");
      if (!phieuDatTheoNgayDiaDiem.has(khoaNgayDiaDiem)) phieuDatTheoNgayDiaDiem.set(khoaNgayDiaDiem, []);
      phieuDatTheoNgayDiaDiem.get(khoaNgayDiaDiem).push(guiPhieuDatDiaDiem);
    });
    var laPhieuDatConHieuLuc = function (guiPhieuDatDiaDiem) {
      return ["ChoXacNhan", "DaXacNhan"].includes(layTrangThaiDatDiaDiem(guiPhieuDatDiaDiem));
    };
    capNhatHtml(
      "booking-timeline",
      datDiaDiemDaLoc.length
        ? Array.from(phieuDatTheoNgayDiaDiem.values())
            .map(function (cacPhieuDatCungNgayDiaDiem) {
              cacPhieuDatCungNgayDiaDiem.sort(function (datDiaDiemTruoc, datDiaDiemSau) {
                return chuyenNgayDuLieuSangDate(datDiaDiemTruoc.BatDau) - chuyenNgayDuLieuSangDate(datDiaDiemSau.BatDau);
              });
              var phieuDatDauTien = cacPhieuDatCungNgayDiaDiem[0];
              return (
                '<div class="timeline-row"><div class="timeline-label"><b>' +
                maHoaKyTuHtml((phieuDatDauTien.DiaDiem && phieuDatDauTien.DiaDiem.Ten) || "Địa điểm") +
                "</b><small>" +
                maHoaKyTuHtml(chuyenNgayDuLieuSangDate(phieuDatDauTien.BatDau).toLocaleDateString("vi-VN")) +
                '</small></div><div class="timeline-slots">' +
                cacPhieuDatCungNgayDiaDiem
                  .map(function (guiPhieuDatDiaDiem) {
                    var diaDiemId = guiPhieuDatDiaDiem.DiaDiemId || (guiPhieuDatDiaDiem.DiaDiem && guiPhieuDatDiaDiem.DiaDiem.Id);
                    var coTrungLich =
                      laPhieuDatConHieuLuc(guiPhieuDatDiaDiem) &&
                      danhSachDatDiaDiem.some(function (phieuDatKhac) {
                        return (
                          phieuDatKhac.Id !== guiPhieuDatDiaDiem.Id &&
                          (diaDiemId
                            ? diaDiemId ===
                              (phieuDatKhac.DiaDiemId || (phieuDatKhac.DiaDiem && phieuDatKhac.DiaDiem.Id))
                            : guiPhieuDatDiaDiem.DiaDiem &&
                              phieuDatKhac.DiaDiem &&
                              guiPhieuDatDiaDiem.DiaDiem.Ten === phieuDatKhac.DiaDiem.Ten) &&
                          laPhieuDatConHieuLuc(phieuDatKhac) &&
                          chuyenNgayDuLieuSangDate(guiPhieuDatDiaDiem.BatDau) < chuyenNgayDuLieuSangDate(phieuDatKhac.KetThuc) &&
                          chuyenNgayDuLieuSangDate(guiPhieuDatDiaDiem.KetThuc) > chuyenNgayDuLieuSangDate(phieuDatKhac.BatDau)
                        );
                      });
                    return (
                      '<div class="timeline-slot ' +
                      (coTrungLich ? "timeline-conflict" : "") +
                      '"><b>' +
                      maHoaKyTuHtml(dinhDangNgayGioVietNam(guiPhieuDatDiaDiem.BatDau)) +
                      " – " +
                      maHoaKyTuHtml(dinhDangNgayGioVietNam(guiPhieuDatDiaDiem.KetThuc)) +
                      "</b><span>" +
                      maHoaKyTuHtml((guiPhieuDatDiaDiem.SuKien && guiPhieuDatDiaDiem.SuKien.Ten) || "Sự kiện") +
                      "</span><small>" +
                      maHoaKyTuHtml(
                        coTrungLich
                          ? "Trùng khung giờ · Cần đối chiếu"
                          : nhanTrangThai(layTrangThaiDatDiaDiem(guiPhieuDatDiaDiem)),
                      ) +
                      "</small></div>"
                    );
                  })
                  .join("") +
                "</div></div>"
              );
            })
            .join("")
        : '<p class="timeline-empty">Không có lịch đặt phù hợp với bộ lọc.</p>',
    );
  }
  function hienVeDienTu(donVe) {
    var danhSachVe = donVe.Ves || [];
    if (!danhSachVe.length)
      return (
        '<div class="empty">' +
        taoHtmlBieuTuong("ticket") +
        "<h3>Chưa có vé được phát hành</h3><p>Vé sẽ sẵn sàng khi thanh toán của đơn được xác nhận.</p></div>"
      );
    return (
      '<div class="digital-ticket-grid">' +
      danhSachVe
        .map(function (veDaPhatHanh) {
          var suKien = (veDaPhatHanh.LoaiVe && veDaPhatHanh.LoaiVe.SuKien) || donVe.SuKien || {},
            trangThaiVe =
              typeof veDaPhatHanh.TrangThai === "number"
                ? ["ChuaSuDung", "DaCheckIn", "DaHuy", "DaHoanTien"][veDaPhatHanh.TrangThai]
                : veDaPhatHanh.TrangThai;
          var noiDungQrHtml = "";
          try {
            if (!veDaPhatHanh.MaQR && !veDaPhatHanh.MaVe) throw new Error("missing code");
            var boTaoMaQr = qrcode(0, "M");
            boTaoMaQr.addData(String(veDaPhatHanh.MaQR || veDaPhatHanh.MaVe));
            boTaoMaQr.make();
            noiDungQrHtml = boTaoMaQr.createSvgTag({
              cellSize: 4,
              margin: 16,
              scalable: true,
              alt: "Mã QR vé " + veDaPhatHanh.MaVe,
            });
          } catch (loiTaoQr) {
            noiDungQrHtml = '<p class="muted">Mã QR chưa khả dụng. Vui lòng tra cứu bằng mã vé.</p>';
          }
          return (
            '<article class="digital-ticket"><div class="ticket-banner"><small>QLSK / DIGITAL TICKET</small><h3>' +
            maHoaKyTuHtml(suKien.Ten || (veDaPhatHanh.LoaiVe && veDaPhatHanh.LoaiVe.Ten) || "Vé sự kiện") +
            '</h3></div><div class="ticket-main"><div class="ticket-data"><div><small>HẠNG VÉ</small><b>' +
            maHoaKyTuHtml((veDaPhatHanh.LoaiVe && veDaPhatHanh.LoaiVe.Ten) || "—") +
            "</b></div><div><small>TRẠNG THÁI</small>" +
            taoNhanTrangThai(trangThaiVe) +
            "</div><div><small>THỜI GIAN</small><b>" +
            maHoaKyTuHtml(suKien.BatDau ? dinhDangNgayGioVietNam(suKien.BatDau) : "Xem chi tiết sự kiện") +
            "</b></div><div><small>ĐƠN HÀNG</small><b>" +
            maHoaKyTuHtml(donVe.MaDonHang || "#" + donVe.Id) +
            '</b></div></div><div class="ticket-qr">' +
            noiDungQrHtml +
            '<div class="ticket-code">' +
            maHoaKyTuHtml(veDaPhatHanh.MaVe) +
            '</div><p class="muted">Xuất trình mã này khi check-in. Không chia sẻ mã vé công khai.</p></div></div><div class="ticket-footer"><a class="phu sm" href="/pages/ve/xuat-ve.html?ticket=' +
            Number(veDaPhatHanh.Id) +
            '">' +
            taoHtmlBieuTuong("download") +
            'Xuất vé</a><button class="phu sm" onclick="guiEmailVe(' +
            Number(veDaPhatHanh.Id) +
            ")\">Gửi email</button><button class=\"phu sm\" onclick=\"taiLichSuXuatVe(" +
            Number(veDaPhatHanh.Id) +
            ')">Lịch sử</button></div></article>'
          );
        })
        .join("") +
      "</div>"
    );
  }
  async function moLichSuXuatVe(veId) {
    var lanTaiChiTietHienTai = ++lanTaiHopChiTiet;
    moHopChiTiet(
      '<h2 id="ui-drawer-title">Lịch sử xuất vé #' + Number(veId) + '</h2><div id="ticket-history"></div>',
    );
    hienKhungDangTai("ticket-history", 3);
    try {
      var phanHoiLichSuXuat = await guiYeuCauApi("/api/tickets/" + Number(veId) + "/exports");
      if (lanTaiChiTietHienTai !== lanTaiHopChiTiet) return;
      capNhatHtml(
        "ticket-history",
        taoHtmlBang(
          [
            ["Kieu", "Hình thức"],
            ["NgayTao", "Ngày xuất", "ngay"],
            ["KetQua", "Kết quả"],
          ],
          (phanHoiLichSuXuat.data || []).map(function (lanXuatVe) {
            return Object.assign({}, lanXuatVe, {
              KetQua: lanXuatVe.ThanhCong ? "Thành công" : "Thất bại",
            });
          }),
        ),
      );
    } catch (loiXuLy) {
      if (lanTaiChiTietHienTai === lanTaiHopChiTiet)
        hienLoiTai("ticket-history", loiXuLy, function () {
          moLichSuXuatVe(veId);
        });
    }
  }
  function hienLoaiSuKienTrangChu(danhSachLoaiSuKien) {
    var khungLoaiSuKienTrangChu = document.querySelector("[data-home-categories]");
    if (!khungLoaiSuKienTrangChu) return;
    khungLoaiSuKienTrangChu.innerHTML = danhSachLoaiSuKien
      .map(function (loaiSuKien, chiSoLoai) {
        return (
          '<a class="category-item" href="/pages/su-kien/danh-sach.html?category=' +
          encodeURIComponent(loaiSuKien.Ten) +
          '"><span>' +
          taoHtmlBieuTuong(["music", "mic", "cpu", "ball", "art", "book", "layers"][chiSoLoai % 7]) +
          "</span><b>" +
          maHoaKyTuHtml(loaiSuKien.Ten) +
          "</b></a>"
        );
      })
      .join("");
  }
  async function hienDiaDiemTrangChu() {
    if (!timPhanTuTheoId("home-venues")) return;
    hienKhungDangTai("home-venues", 3);
    try {
      var phanHoiDiaDiem = await guiYeuCauApi("/api/venues");
      var danhSachDiaDiem = (phanHoiDiaDiem.data || []).slice(0, 3);
      capNhatHtml(
        "home-venues",
        danhSachDiaDiem.length
          ? danhSachDiaDiem
              .map(function (diaDiem) {
                return (
                  '<a class="home-venue" href="/pages/dia-diem/danh-sach.html?q=' +
                  encodeURIComponent(diaDiem.Ten) +
                  '">' +
                  anhDiaDiemHTML(diaDiem) +
                  "<div><h3>" +
                  maHoaKyTuHtml(diaDiem.Ten) +
                  "</h3><p>" +
                  maHoaKyTuHtml(diaDiem.DiaChi || "Địa chỉ đang cập nhật") +
                  '</p><span class="text-link">Khám phá không gian ' +
                  taoHtmlBieuTuong("arrow-up-right") +
                  "</span></div></a>"
                );
              })
              .join("")
          : '<div class="empty"><p>Địa điểm mới sẽ được cập nhật tại đây.</p></div>',
      );
    } catch (loiXuLy) {
      hienLoiTai("home-venues", loiXuLy, hienDiaDiemTrangChu);
    }
  }
  var paymentRequest = 0;
  async function taiDonVeCanThanhToan() {
    var donVeId = docSoTuInput("tt-don");
    if (!donVeId || donVeId <= 0) {
      hienThongBaoNoi("Nhập mã đơn vé hợp lệ.", "loi");
      return;
    }
    hienKhungDangTai("payment-order-summary", 3);
    var lanTaiHienTai = ++paymentRequest,
      nutThanhToan = timPhanTuTheoId("payment-submit");
    if (nutThanhToan) nutThanhToan.disabled = true;
    capNhatVanBan("payment-total", "—");
    try {
      var phanHoiDonVe = await guiYeuCauApi("/api/orders/" + donVeId),
        donVe = phanHoiDonVe.data || phanHoiDonVe;
      if (lanTaiHienTai !== paymentRequest || donVeId !== docSoTuInput("tt-don")) return;
      capNhatHtml(
        "payment-order-summary",
        '<div class="payment-order-meta"><b>' +
          maHoaKyTuHtml(donVe.MaDonHang || "#" + donVeId) +
          "</b>" +
          taoNhanTrangThai(nhanTrangThaiDon(donVe.TrangThai)) +
          "</div>" +
          taoHtmlBang(
            [
              ["Ten", "Hạng vé"],
              ["SoLuong", "Số lượng"],
              ["ThanhTien", "Thành tiền", "tien"],
            ],
            (donVe.ChiTietDonVes || donVe.ChiTiets || []).map(function (chiTietDonVe) {
              return {
                Ten: (chiTietDonVe.LoaiVe && chiTietDonVe.LoaiVe.Ten) || "Vé sự kiện",
                SoLuong: chiTietDonVe.SoLuong,
                ThanhTien: chiTietDonVe.ThanhTien,
              };
            }),
          ),
      );
      capNhatVanBan("payment-total", dinhDangTienDong(donVe.TongTien));
      capNhatVanBan(
        "payment-submit",
        Number(donVe.TongTien) === 0 ? "Xác nhận vé miễn phí" : "Thanh toán " + dinhDangTienDong(donVe.TongTien),
      );
      if (nutThanhToan) nutThanhToan.disabled = nhanTrangThaiDon(donVe.TrangThai) !== "ChoThanhToan";
    } catch (loiXuLy) {
      if (lanTaiHienTai === paymentRequest) {
        hienLoiTai("payment-order-summary", loiXuLy, taiDonVeCanThanhToan);
        capNhatVanBan("payment-total", "—");
        capNhatVanBan("payment-submit", "Tiếp tục với PayOS");
      }
    }
  }
  function hienTomTatSuKien() {
    var danhSachPhanTuId = ["sk-ten", "sk-loai", "sk-bd", "sk-kt", "sk-sl", "sk-mota"],
      cacNhanHienThi = ["Tên sự kiện", "Chủ đề", "Bắt đầu", "Kết thúc", "Số người dự kiến", "Mô tả"];
    capNhatHtml(
      "wizard-review",
      danhSachPhanTuId
        .map(function (phanTuDomId, chiSoTruong) {
          var oNhapLieu = timPhanTuTheoId(phanTuDomId),
            giaTriHienThi =
              oNhapLieu && oNhapLieu.tagName === "SELECT"
                ? oNhapLieu.options[oNhapLieu.selectedIndex] &&
                  oNhapLieu.options[oNhapLieu.selectedIndex].text
                : docGiaTriInput(phanTuDomId);
          if ((phanTuDomId === "sk-bd" || phanTuDomId === "sk-kt") && giaTriHienThi)
            giaTriHienThi = new Date(giaTriHienThi).toLocaleString("vi-VN");
          return (
            "<div><dt>" +
            cacNhanHienThi[chiSoTruong] +
            "</dt><dd>" +
            maHoaKyTuHtml(giaTriHienThi || "Chưa nhập") +
            "</dd></div>"
          );
        })
        .join(""),
    );
  }
  function kiemTraBuocTaoSuKien(chiSoBuocTaoSuKien) {
    var danhSachPhanTuId =
      chiSoBuocTaoSuKien === 0 ? ["sk-ten", "sk-loai"] : chiSoBuocTaoSuKien === 1 ? ["sk-bd", "sk-kt"] : chiSoBuocTaoSuKien === 2 ? ["sk-sl"] : [];
    for (var chiSoTruong = 0; chiSoTruong < danhSachPhanTuId.length; chiSoTruong++) {
      var oNhapLieu = timPhanTuTheoId(danhSachPhanTuId[chiSoTruong]);
      if (
        !oNhapLieu ||
        !oNhapLieu.value.trim() ||
        (oNhapLieu.type === "number" && Number(oNhapLieu.value) < 1)
      ) {
        oNhapLieu && oNhapLieu.focus();
        hienThongBaoNoi("Vui lòng hoàn tất các thông tin của bước này.", "loi");
        return false;
      }
    }
    return chiSoBuocTaoSuKien !== 1 || kiemTraKhoangThoiGianForm(docGiaTriInput("sk-bd"), docGiaTriInput("sk-kt"));
  }
  function chuyenBuocTaoSuKien(buocTaoSuKienDich) {
    if (buocTaoSuKienDich > buocTaoSuKienHienTai && !kiemTraBuocTaoSuKien(buocTaoSuKienHienTai)) return;
    buocTaoSuKienHienTai = Math.min(4, Math.max(0, buocTaoSuKienDich));
    document.querySelectorAll("[data-wizard-step]").forEach(function (phanTuBuoc) {
      phanTuBuoc.hidden = Number(phanTuBuoc.dataset.wizardStep) !== buocTaoSuKienHienTai;
    });
    document.querySelectorAll("[data-wizard-goto]").forEach(function (phanTuBuoc) {
      var chiSoBuoc = Number(phanTuBuoc.dataset.wizardGoto);
      phanTuBuoc.classList.toggle("active", chiSoBuoc === buocTaoSuKienHienTai);
      phanTuBuoc.classList.toggle("done", chiSoBuoc < buocTaoSuKienHienTai);
      phanTuBuoc.setAttribute("aria-current", chiSoBuoc === buocTaoSuKienHienTai ? "step" : "false");
    });
    document.querySelector("[data-wizard-prev]").hidden = buocTaoSuKienHienTai === 0;
    document.querySelector("[data-wizard-next]").hidden = buocTaoSuKienHienTai === 4;
    document.querySelector("[data-wizard-submit]").hidden = buocTaoSuKienHienTai !== 4;
    if (buocTaoSuKienHienTai === 4) hienTomTatSuKien();
    var tieuDeBuoc = document.querySelector('[data-wizard-step="' + buocTaoSuKienHienTai + '"] h2');
    if (tieuDeBuoc) {
      tieuDeBuoc.tabIndex = -1;
      tieuDeBuoc.focus({
        preventScroll: true,
      });
    }
  }
  function xoaBoLocGiaoDien() {
    ["filter-date", "filter-location", "filter-price", "event-sort"].forEach(function (phanTuDomId) {
      var oBoLoc = timPhanTuTheoId(phanTuDomId);
      if (oBoLoc) oBoLoc.value = phanTuDomId === "event-sort" ? "upcoming" : "";
    });
    boLocTrangThai = "";
    xoaBoLocSuKien();
  }
  function boSungGiaoDienBang(phanTuGocCanBoSungBang) {
    if (!phanTuGocCanBoSungBang || !phanTuGocCanBoSungBang.querySelectorAll) return;
    phanTuGocCanBoSungBang.querySelectorAll("table:not([data-ui-decorated])").forEach(function (bangDuLieu) {
      bangDuLieu.dataset.uiDecorated = "true";
      var cacNhanHienThi = Array.from(bangDuLieu.querySelectorAll("thead th")).map(function (oBang) {
        return oBang.textContent;
      });
      bangDuLieu.querySelectorAll("tbody tr").forEach(function (hangBang) {
        Array.from(hangBang.cells).forEach(function (oBangDuLieu, chiSoCot) {
          oBangDuLieu.dataset.label = cacNhanHienThi[chiSoCot] || "";
          var nhanHienThi = cacNhanHienThi[chiSoCot] || "",
            noiDungO = oBangDuLieu.textContent.trim();
          if (/trạng thái|vai trò|hoạt động/i.test(nhanHienThi) && !oBangDuLieu.querySelector(".badge")) {
            var lopTrangThai = /khóa|từ chối|hủy|thất bại/i.test(noiDungO)
              ? "b-err"
              : /chờ|xử lý/i.test(noiDungO)
                ? "b-warn"
                : /thành công|hoạt động|đã duyệt|hoàn thành/i.test(noiDungO)
                  ? "b-ok"
                  : "b-info";
            oBangDuLieu.innerHTML =
              '<span class="badge ' + lopTrangThai + '">' + maHoaKyTuHtml(noiDungO) + "</span>";
          }
          if (
            /thao tác/i.test(nhanHienThi) &&
            oBangDuLieu.children.length > 2 &&
            !oBangDuLieu.querySelector("details")
          ) {
            var cacPhanTuDom = Array.from(oBangDuLieu.childNodes),
              menuThaoTac = document.createElement("details");
            menuThaoTac.className = "row-actions";
            menuThaoTac.innerHTML =
              '<summary aria-label="Mở thao tác">⋯</summary><div class="action-menu"></div>';
            cacPhanTuDom.forEach(function (phanTuGiaoDien) {
              menuThaoTac.lastChild.appendChild(phanTuGiaoDien);
            });
            oBangDuLieu.appendChild(menuThaoTac);
          }
        });
      });
      if (!bangDuLieu.closest(".analytics-grid"))
        bangDuLieu.closest(".tablescroll") &&
          bangDuLieu.closest(".tablescroll").parentElement.classList.add("responsive-records");
    });
    phanTuGocCanBoSungBang.querySelectorAll(".kpi .ic:not([data-iconized])").forEach(function (phanTuGiaoDien, chiSoCot) {
      phanTuGiaoDien.dataset.iconized = "true";
      phanTuGiaoDien.innerHTML = taoHtmlBieuTuong(["ticket", "clock", "check", "refresh"][chiSoCot % 4]);
    });
  }
  function hienHoSoTaiKhoan() {
    document.querySelectorAll("[data-profile-name]").forEach(function (nhanHoSo) {
      var noiDungHoSo = phienDangNhap.hoTen || "Tài khoản của bạn";
      if (nhanHoSo.textContent !== noiDungHoSo) nhanHoSo.textContent = noiDungHoSo;
    });
    document.querySelectorAll("[data-profile-initials]").forEach(function (nhanHoSo) {
      var noiDungHoSo = layChuCaiDauTen(phienDangNhap.hoTen);
      if (nhanHoSo.textContent !== noiDungHoSo) nhanHoSo.textContent = noiDungHoSo;
    });
    var email = timPhanTuTheoId("qtk-email");
    if (email && email.value)
      document.querySelectorAll("[data-profile-email]").forEach(function (nhanHoSo) {
        if (nhanHoSo.textContent !== email.value) nhanHoSo.textContent = email.value;
      });
  }
  function xuatBaoCaoCsv() {
    if (!duLieuTongQuan) {
      hienThongBaoNoi("Vui lòng tải dữ liệu trước khi xuất báo cáo.", "info");
      return;
    }
    var cacDongCsv = [
      ["Chỉ số", "Giá trị"],
      ["Phạm vi", "Toàn bộ thời gian"],
      ["Người dùng", duLieuTongQuan.taiKhoan],
      ["Sự kiện", duLieuTongQuan.suKien],
      ["Vé phát hành", duLieuTongQuan.ve],
      ["Doanh thu thuần", duLieuTongQuan.doanhThu],
      ["Hoàn tiền", duLieuTongQuan.hoanTien],
      ["Đơn hàng", duLieuTongQuan.don],
    ];
    var noiDungCsv =
      "\uFEFF" +
      cacDongCsv
        .map(function (hangBaoCao) {
          return hangBaoCao
            .map(function (giaTriO) {
              var chuoiGiaTri = String(giaTriO == null ? "" : giaTriO);
              if (/^[=+@-]/.test(chuoiGiaTri)) chuoiGiaTri = "'" + chuoiGiaTri;
              return '"' + chuoiGiaTri.replace(/"/g, '""') + '"';
            })
            .join(",");
        })
        .join("\r\n");
    var lienKetTai = document.createElement("a"),
      urlYeuCau = URL.createObjectURL(
        new Blob([noiDungCsv], {
          type: "text/csv;charset=utf-8",
        }),
      );
    lienKetTai.href = urlYeuCau;
    lienKetTai.download = "QLSK-bao-cao-tong-hop.csv";
    lienKetTai.click();
    setTimeout(function () {
      URL.revokeObjectURL(urlYeuCau);
    }, 1000);
  }
  function capNhatTomTatThanhToan() {
    var oChonSuKien = timPhanTuTheoId("don-sukien"),
      oChonLoaiVe = timPhanTuTheoId("don-loai");
    if (!oChonSuKien || !oChonLoaiVe) return;
    var eventName = oChonSuKien.options[oChonSuKien.selectedIndex] && oChonSuKien.options[oChonSuKien.selectedIndex].text;
    var ticketName = oChonLoaiVe.options[oChonLoaiVe.selectedIndex] && oChonLoaiVe.options[oChonLoaiVe.selectedIndex].text;
    capNhatHtml(
      "checkout-event-summary",
      taoHtmlBieuTuong("calendar") +
        "<div><strong>" +
        maHoaKyTuHtml(oChonSuKien.value ? eventName : "Chọn một sự kiện để bắt đầu.") +
        "</strong><small>Vé điện tử · QLSK</small></div>",
    );
    document.querySelectorAll("[data-checkout-ticket]").forEach(function (nhanTomTat) {
      var giaTriTomTat = oChonLoaiVe.value ? ticketName : "—";
      if (nhanTomTat.textContent !== giaTriTomTat) nhanTomTat.textContent = giaTriTomTat;
    });
    document.querySelectorAll("[data-checkout-quantity]").forEach(function (nhanTomTat) {
      var giaTriTomTat = docGiaTriInput("don-sl") || "1";
      if (nhanTomTat.textContent !== giaTriTomTat) nhanTomTat.textContent = giaTriTomTat;
    });
  }
  function khoiTaoTabDanhMuc() {
    var cacNutTab = Array.from(document.querySelectorAll("[data-catalog-tab]"));
    function kichHoatTabDanhMuc(nutTabDangChon, canChuyenTieuDiem) {
      cacNutTab.forEach(function (nutTab) {
        var laTabDangChon = nutTab === nutTabDangChon;
        nutTab.classList.toggle("active", laTabDangChon);
        nutTab.setAttribute("aria-selected", String(laTabDangChon));
        nutTab.tabIndex = laTabDangChon ? 0 : -1;
      });
      document.querySelectorAll("[data-catalog-panel]").forEach(function (khungTab) {
        khungTab.hidden = khungTab.dataset.catalogPanel !== nutTabDangChon.dataset.catalogTab;
      });
      if (canChuyenTieuDiem) nutTabDangChon.focus();
    }
    cacNutTab.forEach(function (nutTab, chiSoTab) {
      nutTab.onclick = function () {
        kichHoatTabDanhMuc(nutTab, false);
      };
      nutTab.addEventListener("keydown", function (suKienDom) {
        var chiSoTabKeTiep =
          suKienDom.key === "ArrowRight"
            ? (chiSoTab + 1) % cacNutTab.length
            : suKienDom.key === "ArrowLeft"
              ? (chiSoTab + cacNutTab.length - 1) % cacNutTab.length
              : suKienDom.key === "Home"
                ? 0
                : suKienDom.key === "End"
                  ? cacNutTab.length - 1
                  : -1;
        if (chiSoTabKeTiep >= 0) {
          suKienDom.preventDefault();
          kichHoatTabDanhMuc(cacNutTab[chiSoTabKeTiep], true);
        }
      });
    });
    if (cacNutTab.length) kichHoatTabDanhMuc(cacNutTab[0], false);
  }
  function khoiTaoGiaoDien() {
    var searchParams = new URLSearchParams(location.search),
      loaiSuKien = searchParams.get("category");
    if (loaiSuKien && typeof locLoaiSuKien === "function") locLoaiSuKien(loaiSuKien);
    var venueQuery = timPhanTuTheoId("dd-tukhoa");
    if (venueQuery && searchParams.has("q")) venueQuery.value = searchParams.get("q");
    ["filter-date", "filter-price", "filter-location"].forEach(function (phanTuDomId) {
      var phanTuGiaoDien = timPhanTuTheoId(phanTuDomId),
        tenThamSoUrl = phanTuDomId.slice(7);
      if (phanTuGiaoDien && searchParams.has(tenThamSoUrl)) phanTuGiaoDien.value = searchParams.get(tenThamSoUrl);
    });
    var veDaPhatHanhId = Number(searchParams.get("ticket"));
    if (veDaPhatHanhId > 0 && timPhanTuTheoId("xv-ve")) {
      timPhanTuTheoId("xv-ve").value = veDaPhatHanhId;
      if (timPhanTuTheoId("xv-lichsu-ve")) timPhanTuTheoId("xv-lichsu-ve").value = veDaPhatHanhId;
    }
    var suKienId = Number(searchParams.get("event"));
    if (suKienId > 0 && timPhanTuTheoId("lv-sk")) {
      timPhanTuTheoId("lv-sk").value = suKienId;
      timPhanTuTheoId("lv-xem-sk").value = suKienId;
      taiLoaiVeTheoSuKien();
    }
    var donVeId = Number(searchParams.get("id"));
    if (donVeId > 0 && timPhanTuTheoId("ve-theodon")) {
      timPhanTuTheoId("ve-theodon").value = donVeId;
      if (phienDangNhap.token) taiVeTheoDon();
    }
    if (timPhanTuTheoId("tt-don")) {
      var paymentButton = timPhanTuTheoId("payment-submit");
      if (paymentButton) paymentButton.disabled = true;
      timPhanTuTheoId("tt-don").addEventListener("input", function () {
        paymentRequest++;
        if (paymentButton) paymentButton.disabled = true;
        capNhatVanBan("payment-total", "—");
        capNhatHtml(
          "payment-order-summary",
          '<div class="empty"><p>Chọn Xem đơn để kiểm tra thông tin trước khi thanh toán.</p></div>',
        );
      });
      if (donVeId > 0 && phienDangNhap.token) taiDonVeCanThanhToan();
    }
    if (timPhanTuTheoId("event-wizard-form")) {
      timPhanTuTheoId("event-wizard-form").addEventListener("submit", function (suKienDom) {
        suKienDom.preventDefault();
        if (buocTaoSuKienHienTai < 4) chuyenBuocTaoSuKien(buocTaoSuKienHienTai + 1);
      });
      document.querySelectorAll("[data-wizard-goto]").forEach(function (nutThaoTac) {
        nutThaoTac.addEventListener("click", function () {
          var buocTaoSuKienDich = Number(nutThaoTac.dataset.wizardGoto);
          for (var chiSo = 0; chiSo < buocTaoSuKienDich; chiSo++) {
            if (!kiemTraBuocTaoSuKien(chiSo)) return;
          }
          chuyenBuocTaoSuKien(buocTaoSuKienDich);
        });
      });
      document.querySelector("[data-wizard-next]").onclick = function () {
        chuyenBuocTaoSuKien(buocTaoSuKienHienTai + 1);
      };
      document.querySelector("[data-wizard-prev]").onclick = function () {
        chuyenBuocTaoSuKien(buocTaoSuKienHienTai - 1);
      };
    }
    var khungTabTrangThaiSuKien = document.querySelector("[data-event-status-tabs]");
    if (khungTabTrangThaiSuKien) {
      khungTabTrangThaiSuKien.innerHTML = [
        ["", "Tất cả"],
        ["ChoDuyet", "Chờ duyệt"],
        ["DaDuyet", "Đã duyệt"],
        ["DangDienRa", "Đang diễn ra"],
        ["DaKetThuc", "Kết thúc"],
        ["TuChoi", "Từ chối"],
        ["DaHuy", "Đã hủy"],
      ]
        .map(function (capTrangThai) {
          return (
            '<button type="button" data-status="' +
            capTrangThai[0] +
            '" class="' +
            (!capTrangThai[0] ? "active" : "") +
            '">' +
            capTrangThai[1] +
            "</button>"
          );
        })
        .join("");
      khungTabTrangThaiSuKien.addEventListener("click", function (suKienDom) {
        var nutThaoTac = suKienDom.target.closest("[data-status]");
        if (nutThaoTac) {
          boLocTrangThai = nutThaoTac.dataset.status;
          khungTabTrangThaiSuKien.querySelectorAll("button").forEach(function (phanTuGiaoDien) {
            phanTuGiaoDien.classList.toggle("active", phanTuGiaoDien === nutThaoTac);
          });
          __suKienTrang = 1;
          hienThiDanhSachSuKien();
        }
      });
    }
    ["filter-date", "filter-location", "filter-price", "event-sort"].forEach(function (phanTuDomId) {
      var phanTuGiaoDien = timPhanTuTheoId(phanTuDomId);
      if (phanTuGiaoDien)
        phanTuGiaoDien.addEventListener(phanTuDomId === "filter-location" ? "input" : "change", function () {
          __suKienTrang = 1;
          if (["filter-location", "filter-price"].indexOf(phanTuDomId) !== -1) xepHangTaiChiTietSuKien(__suKienCache || []);
          hienThiDanhSachSuKien();
        });
    });
    var eventSearch = timPhanTuTheoId("workspace-event-search");
    if (eventSearch)
      eventSearch.addEventListener("input", function () {
        var hamTuongThichTrangCu = timPhanTuTheoId("sk-tukhoa-page");
        if (hamTuongThichTrangCu) hamTuongThichTrangCu.value = eventSearch.value;
        __suKienTrang = 1;
        hienThiDanhSachSuKien();
      });
    ["account-role-filter", "account-status-filter"].forEach(function (phanTuDomId) {
      var phanTuGiaoDien = timPhanTuTheoId(phanTuDomId);
      if (phanTuGiaoDien) phanTuGiaoDien.onchange = capNhatDanhSachTaiKhoan;
    });
    ["booking-date-filter", "booking-venue-filter", "booking-status-filter"].forEach(function (phanTuDomId) {
      var phanTuGiaoDien = timPhanTuTheoId(phanTuDomId);
      if (phanTuGiaoDien)
        phanTuGiaoDien.addEventListener(phanTuDomId === "booking-venue-filter" ? "input" : "change", capNhatDanhSachDatDiaDiem);
    });
    document.querySelectorAll("[data-quantity]").forEach(function (nutThaoTac) {
      nutThaoTac.onclick = function () {
        var phanTuGiaoDien = timPhanTuTheoId("don-sl");
        phanTuGiaoDien.value = Math.max(
          1,
          Math.min(20, Number(phanTuGiaoDien.value || 1) + Number(nutThaoTac.dataset.quantity)),
        );
        phanTuGiaoDien.dispatchEvent(
          new Event("input", {
            bubbles: true,
          }),
        );
        capNhatTomTatThanhToan();
      };
    });
    ["don-sukien", "don-loai", "don-sl"].forEach(function (phanTuDomId) {
      var phanTuGiaoDien = timPhanTuTheoId(phanTuDomId);
      if (phanTuGiaoDien) {
        phanTuGiaoDien.addEventListener("change", capNhatTomTatThanhToan);
        phanTuGiaoDien.addEventListener("input", capNhatTomTatThanhToan);
      }
    });
    var sidebarToggle = document.querySelector("[data-sidebar-toggle]"),
      lopNenThanhDieuHuong = document.querySelector(".sidebar-scrim"),
      thanhDieuHuong = timPhanTuTheoId("workspace-sidebar");
    function capNhatTrangThaiThanhDieuHuong(canMoThanhDieuHuong) {
      document.body.classList.toggle("sidebar-mobile-open", canMoThanhDieuHuong);
      if (lopNenThanhDieuHuong) lopNenThanhDieuHuong.hidden = !canMoThanhDieuHuong;
      if (sidebarToggle) {
        sidebarToggle.setAttribute("aria-expanded", String(canMoThanhDieuHuong));
        sidebarToggle.setAttribute("aria-label", canMoThanhDieuHuong ? "Đóng điều hướng" : "Mở điều hướng");
      }
      if (timPhanTuTheoId("noi-dung")) timPhanTuTheoId("noi-dung").inert = canMoThanhDieuHuong;
      if (window.innerWidth < 1024 && thanhDieuHuong) thanhDieuHuong.inert = !canMoThanhDieuHuong;
    }
    function dongThanhDieuHuong() {
      var hopChiTietDaMo = document.body.classList.contains("sidebar-mobile-open");
      capNhatTrangThaiThanhDieuHuong(false);
      if (hopChiTietDaMo && sidebarToggle) sidebarToggle.focus();
    }
    if (sidebarToggle)
      sidebarToggle.onclick = function () {
        if (window.innerWidth < 1024) {
          var canMoThanhDieuHuong = !document.body.classList.contains("sidebar-mobile-open");
          capNhatTrangThaiThanhDieuHuong(canMoThanhDieuHuong);
          if (canMoThanhDieuHuong) {
            var phanTuDauTien = Array.from(thanhDieuHuong.querySelectorAll("a,button")).find(function (phanTuCoTheFocus) {
              return phanTuCoTheFocus.getClientRects().length;
            });
            if (phanTuDauTien) phanTuDauTien.focus();
          }
        } else {
          var thanhDieuHuongDaThuGon = document.body.classList.toggle("sidebar-collapsed");
          sidebarToggle.setAttribute("aria-expanded", String(!thanhDieuHuongDaThuGon));
          sidebarToggle.setAttribute(
            "aria-label",
            thanhDieuHuongDaThuGon ? "Mở rộng thanh điều hướng" : "Thu gọn thanh điều hướng",
          );
          document.querySelectorAll(".workspace-nav-link").forEach(function (phanTuCoTheFocus) {
            phanTuCoTheFocus.title = phanTuCoTheFocus.textContent.trim();
          });
        }
      };
    if (lopNenThanhDieuHuong) lopNenThanhDieuHuong.onclick = dongThanhDieuHuong;
    function capNhatThanhDieuHuongTheoManHinh() {
      if (!thanhDieuHuong) return;
      if (window.innerWidth >= 1024) {
        document.body.classList.remove("sidebar-mobile-open");
        thanhDieuHuong.inert = false;
        if (lopNenThanhDieuHuong) lopNenThanhDieuHuong.hidden = true;
        if (timPhanTuTheoId("noi-dung")) timPhanTuTheoId("noi-dung").inert = false;
        if (sidebarToggle)
          sidebarToggle.setAttribute(
            "aria-expanded",
            String(!document.body.classList.contains("sidebar-collapsed")),
          );
      } else capNhatTrangThaiThanhDieuHuong(document.body.classList.contains("sidebar-mobile-open"));
    }
    window.addEventListener("resize", capNhatThanhDieuHuongTheoManHinh);
    capNhatThanhDieuHuongTheoManHinh();
    document.addEventListener("keydown", function (suKienDom) {
      if (suKienDom.key !== "Tab" || !document.body.classList.contains("sidebar-mobile-open")) return;
      var cacPhanTuNhanTieuDiem = Array.from(thanhDieuHuong.querySelectorAll("a,button")).filter(function (phanTuCoTheFocus) {
        return phanTuCoTheFocus.getClientRects().length;
      });
      if (!cacPhanTuNhanTieuDiem.length) return;
      var phanTuDauTien = cacPhanTuNhanTieuDiem[0],
        phanTuCuoiCung = cacPhanTuNhanTieuDiem[cacPhanTuNhanTieuDiem.length - 1];
      if (
        suKienDom.shiftKey &&
        (document.activeElement === phanTuDauTien || !thanhDieuHuong.contains(document.activeElement))
      ) {
        suKienDom.preventDefault();
        phanTuCuoiCung.focus();
      } else if (
        !suKienDom.shiftKey &&
        (document.activeElement === phanTuCuoiCung || !thanhDieuHuong.contains(document.activeElement))
      ) {
        suKienDom.preventDefault();
        phanTuDauTien.focus();
      }
    });
    document.querySelectorAll("[data-drawer-close]").forEach(function (nutThaoTac) {
      nutThaoTac.onclick = dongHopChiTiet;
    });
    var hopChiTiet = timPhanTuTheoId("ui-drawer");
    if (hopChiTiet) {
      hopChiTiet.addEventListener("click", function (suKienDom) {
        if (suKienDom.target === hopChiTiet && suKienDom.clientX < hopChiTiet.getBoundingClientRect().left)
          dongHopChiTiet();
      });
      hopChiTiet.addEventListener("close", function () {
        lanTaiHopChiTiet++;
        khoiPhucTieuDiemSauDongChiTiet();
      });
    }
    document.addEventListener("keydown", function (suKienDom) {
      if (suKienDom.key === "Escape") dongThanhDieuHuong();
      if (suKienDom.key === "Enter" && !suKienDom.isComposing && suKienDom.target.id === "ci-ve") {
        suKienDom.preventDefault();
        ghiNhanCheckInVe();
      }
    });
    document.addEventListener("click", function (suKienDom) {
      var nutLuuSuKien = suKienDom.target.closest("[data-save-event]");
      if (nutLuuSuKien) {
        var suKienId = Number(nutLuuSuKien.dataset.saveEvent),
          cacSuKienDaLuu = docSuKienDaLuu(),
          chiSo = cacSuKienDaLuu.indexOf(suKienId);
        if (chiSo === -1) cacSuKienDaLuu.push(suKienId);
        else cacSuKienDaLuu.splice(chiSo, 1);
        try {
          localStorage.setItem("qlsk_saved_events", JSON.stringify(cacSuKienDaLuu));
        } catch (loiLuuTrinhDuyet) {}
        document.querySelectorAll('[data-save-event="' + suKienId + '"]').forEach(function (nutThaoTac) {
          nutThaoTac.setAttribute("aria-pressed", String(chiSo === -1));
        });
        hienThongBaoNoi(chiSo === -1 ? "Đã lưu sự kiện trên trình duyệt này." : "Đã bỏ lưu sự kiện.", "info");
      }
      if (!suKienDom.target.closest(".row-actions"))
        document.querySelectorAll(".row-actions[open]").forEach(function (menuThaoTac) {
          menuThaoTac.open = false;
        });
    });
    document.querySelectorAll("[data-notice]").forEach(function (nutThaoTac) {
      nutThaoTac.onclick = function () {
        moHopChiTiet(
          '<h2 id="ui-drawer-title">Thông báo của bạn</h2><div class="empty">' +
            taoHtmlBieuTuong("bell") +
            '<h3>Theo dõi những cập nhật quan trọng</h3><p>Xem trạng thái đơn vé, giao dịch và hồ sơ tổ chức tại các mục bên dưới.</p></div><div class="settings-links"><a href="/pages/don-ve/don-cua-toi.html">Đơn vé của tôi ' +
            taoHtmlBieuTuong("arrow-right") +
            '</a><a href="/pages/to-chuc/yeu-cau-cua-toi.html">Hồ sơ tổ chức ' +
            taoHtmlBieuTuong("arrow-right") +
            "</a></div>",
        );
      };
    });
    document.querySelectorAll("[data-load-payment-order]").forEach(function (nutThaoTac) {
      nutThaoTac.onclick = taiDonVeCanThanhToan;
    });
    document.querySelectorAll("[data-export-report]").forEach(function (nutThaoTac) {
      nutThaoTac.onclick = xuatBaoCaoCsv;
    });
    khoiTaoTabDanhMuc();
    hienDiaDiemTrangChu();
    boSungGiaoDienBang(document);
    hienHoSoTaiKhoan();
    capNhatTomTatThanhToan();
    // Only inspect changed hosts. The observer never rerenders data or rewrites
    // its own checkout/profile content, so it cannot create a feedback loop.
    var cacKhungChoCapNhat = new Set(),
      daHenCapNhat = false;
    if (timPhanTuTheoId("noi-dung"))
      new MutationObserver(function (cacThayDoiDom) {
        cacThayDoiDom.forEach(function (thayDoiDom) {
          if (thayDoiDom.addedNodes.length && thayDoiDom.target.nodeType === 1)
            cacKhungChoCapNhat.add(thayDoiDom.target);
        });
        if (daHenCapNhat || !cacKhungChoCapNhat.size) return;
        daHenCapNhat = true;
        requestAnimationFrame(function () {
          daHenCapNhat = false;
          var cacKhungDom = Array.from(cacKhungChoCapNhat);
          cacKhungChoCapNhat.clear();
          cacKhungDom.forEach(boSungGiaoDienBang);
        });
      }).observe(timPhanTuTheoId("noi-dung"), {
        childList: true,
        subtree: true,
      });
    var coQuyenTruyCap =
      phienDangNhap.token &&
      (!document.body.dataset.allow || document.body.dataset.allow.split(",").indexOf(phienDangNhap.vaiTro) !== -1);
    if (timPhanTuTheoId("ds-diadiem") && (!document.body.hasAttribute("data-allow") || coQuyenTruyCap))
      taiDanhSachDiaDiem();
    if (coQuyenTruyCap) {
      if (timPhanTuTheoId("ds-dat")) taiDatDiaDiem();
      if (timPhanTuTheoId("payment-chart") && phienDangNhap.vaiTro === "QuanTriVien") taiThongKe();
      if (timPhanTuTheoId("ds-taikhoan")) taiDanhSachTaiKhoan();
      if (timPhanTuTheoId("ds-duyet-tochuc")) hienDanhSachDuyetToChuc();
      if (document.body.dataset.uiPage === "pages/don-ve/don-cua-toi.html") taiDonVeCuaTaiKhoan();
    }
    document.querySelectorAll("[data-quyen]").forEach(function (phanTuGiaoDien) {
      if (!phanTuGiaoDien.style.display && phienDangNhap.vaiTro === "") phanTuGiaoDien.style.display = "none";
    });
    if (timPhanTuTheoId("ds-sukien") && Array.isArray(__suKienCache)) hienThiDanhSachSuKien();
  }
  document.addEventListener("DOMContentLoaded", khoiTaoGiaoDien);
  return {
    taoHtmlBieuTuong: taoHtmlBieuTuong,
    hopNhatChiTietSuKien: hopNhatChiTietSuKien,
    taoTheSuKien: taoTheSuKien,
    taoBangSuKien: taoBangSuKien,
    locDanhSachSuKien: locDanhSachSuKien,
    capNhatGiaoDienSauTaiSuKien: capNhatGiaoDienSauTaiSuKien,
    ghiNhanDaTaiSuKien: ghiNhanDaTaiSuKien,
    ghiNhanDangTaiThongTinBoSung: ghiNhanDangTaiThongTinBoSung,
    taoHtmlChiTietSuKien: taoHtmlChiTietSuKien,
    moChiTietSuKien: moChiTietSuKien,
    duyetSuKienTrongHopChiTiet: duyetSuKienTrongHopChiTiet,
    tuChoiSuKienTrongHopChiTiet: tuChoiSuKienTrongHopChiTiet,
    hienYeuCauXacThuc: hienYeuCauXacThuc,
    moChiTietYeuCauXacThuc: moChiTietYeuCauXacThuc,
    duyetYeuCauXacThucTrongHopChiTiet: duyetYeuCauXacThucTrongHopChiTiet,
    tuChoiYeuCauXacThucTrongHopChiTiet: tuChoiYeuCauXacThucTrongHopChiTiet,
    hienDanhSachTaiKhoan: hienDanhSachTaiKhoan,
    hienTongQuanQuanTri: hienTongQuanQuanTri,
    hienDanhSachDatDiaDiem: hienDanhSachDatDiaDiem,
    hienVeDienTu: hienVeDienTu,
    moLichSuXuatVe: moLichSuXuatVe,
    hienLoaiSuKienTrangChu: hienLoaiSuKienTrangChu,
    hienLoiTaiTongQuan: hienLoiTaiTongQuan,
    xoaBoLocGiaoDien: xoaBoLocGiaoDien,
    dongHopChiTiet: dongHopChiTiet,
    hienHoSoTaiKhoan: hienHoSoTaiKhoan,
    capNhatTomTatThanhToan: capNhatTomTatThanhToan,
    taiDonVeCanThanhToan: taiDonVeCanThanhToan,
  };
})();
