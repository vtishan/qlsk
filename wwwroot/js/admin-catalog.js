"use strict";

var danhMucQuanTri = null;
var danhSachLoaiVeQuanTri = [];
var dangLuuQuanTri = false;
function dienGiaTriQuanTri(idTruongNhap, giaTriCanXuLy) {
  document.getElementById(idTruongNhap).value = giaTriCanXuLy == null ? "" : giaTriCanXuLy;
}
function ngayNhapQuanTri(giaTriCanXuLy) {
  var ngayDuocChuyenDoi = chuyenNgayDuLieuSangDate(giaTriCanXuLy);
  return isNaN(ngayDuocChuyenDoi)
    ? ""
    : new Date(ngayDuocChuyenDoi.getTime() - ngayDuocChuyenDoi.getTimezoneOffset() * 60000).toISOString().slice(0, 19);
}
function luaChonQuanTri(idOChon, danhSachLuaChon, layNhanLuaChon) {
  document.getElementById(idOChon).innerHTML =
    '<option value="">Chọn…</option>' +
    danhSachLuaChon
      .map(function (luaChon) {
        return '<option value="' + Number(luaChon.Id) + '">' + maHoaKyTuHtml(layNhanLuaChon(luaChon)) + "</option>";
      })
      .join("");
}
async function taiDanhMucQuanTri() {
  if (phienDangNhap.vaiTro !== "QuanTriVien") return;
  ["qt-ds-sk", "qt-ds-tc", "qt-ds-tk"].forEach(function (idKhungDanhMuc) {
    hienKhungDangTai(idKhungDanhMuc, 3);
  });
  try {
    var phanHoiDanhMuc = await guiYeuCauApi("/api/admin/catalog");
    danhMucQuanTri = phanHoiDanhMuc.data;
    luaChonQuanTri("qt-sk-btc", danhMucQuanTri.ToChuc, function (suKien) {
      return suKien.Ten;
    });
    luaChonQuanTri("qt-sk-loai", danhMucQuanTri.LoaiSuKien, function (banToChuc) {
      return banToChuc.Ten;
    });
    luaChonQuanTri("qt-tc-tk", danhMucQuanTri.TaiKhoan, function (taiKhoan) {
      return taiKhoan.HoTen + " · " + taiKhoan.Email;
    });
    luaChonQuanTri("qt-ve-sk", danhMucQuanTri.SuKien, function (loaiSuKien) {
      return loaiSuKien.Ten;
    });
    document.getElementById("qt-ds-sk").innerHTML = taoHtmlBang(
      [
        ["Id", "Mã"],
        ["Ten", "Sự kiện"],
        ["TrangThaiTen", "Trạng thái"],
      ],
      danhMucQuanTri.SuKien.map(function (suKien) {
        return Object.assign({}, suKien, {
          TrangThaiTen: trangThaiSuKienHienThi(suKien.TrangThai),
        });
      }),
      function (suKien) {
        return nutQuanTri("events", suKien.Id, "dienFormSuaSuKienQuanTri");
      },
    );
    document.getElementById("qt-ds-tc").innerHTML = taoHtmlBang(
      [
        ["Id", "Mã"],
        ["Ten", "Ban tổ chức"],
        ["MoTa", "Mô tả"],
      ],
      danhMucQuanTri.ToChuc,
      function (banToChuc) {
        return nutQuanTri("organizers", banToChuc.Id, "dienFormSuaToChucQuanTri");
      },
    );
    document.getElementById("qt-ds-tk").innerHTML = taoHtmlBang(
      [
        ["Id", "Mã"],
        ["HoTen", "Họ tên"],
        ["Email", "Email"],
        ["HoatDong", "Hoạt động"],
      ],
      danhMucQuanTri.TaiKhoan.map(function (taiKhoan) {
        return Object.assign({}, taiKhoan, {
          HoatDong: taiKhoan.HoatDong ? "Đang hoạt động" : "Đã khóa",
        });
      }),
      function (taiKhoan) {
        return (
          '<button class="phu sm" onclick="dienFormSuaTaiKhoanQuanTri(' +
          Number(taiKhoan.Id) +
          ')">Sửa</button>'
        );
      },
    );
  } catch (loiXuLy) {
    ["qt-ds-sk", "qt-ds-tc", "qt-ds-tk"].forEach(function (idKhungDanhMuc) {
      hienLoiTai(idKhungDanhMuc, loiXuLy, taiDanhMucQuanTri);
    });
    thongBao(loiXuLy.message, "loi");
  }
}
function nutQuanTri(loaiDanhMuc, banGhiId, tenHamDienFormSua) {
  return (
    '<button class="phu sm" onclick="' +
    tenHamDienFormSua +
    "(" +
    Number(banGhiId) +
    ')">Sửa</button> <button class="phu sm" onclick="xoaBanGhiQuanTri(\'' +
    loaiDanhMuc +
    "'," +
    Number(banGhiId) +
    ')">Xóa</button>'
  );
}
function dienFormSuaSuKienQuanTri(suKienId) {
  var suKien = danhMucQuanTri.SuKien.find(function (suKien) {
    return suKien.Id === suKienId;
  });
  if (!suKien) return;
  [
    ["id", suKien.Id],
    ["ten", suKien.Ten],
    ["mota", suKien.MoTa],
    ["anh", suKien.HinhAnh],
    ["btc", suKien.BanToChucId],
    ["loai", suKien.LoaiSuKienId],
    ["sl", suKien.SoNguoi],
    ["bd", ngayNhapQuanTri(suKien.BatDau)],
    ["kt", ngayNhapQuanTri(suKien.KetThuc)],
  ].forEach(function (capTruongGiaTri) {
    dienGiaTriQuanTri("qt-sk-" + capTruongGiaTri[0], capTruongGiaTri[1]);
  });
  document.getElementById("qt-sk-ten").focus();
}
function dienFormSuaToChucQuanTri(banToChucId) {
  var banToChuc = danhMucQuanTri.ToChuc.find(function (banToChuc) {
    return banToChuc.Id === banToChucId;
  });
  [
    ["id", banToChuc.Id],
    ["ten", banToChuc.Ten],
    ["mota", banToChuc.MoTa],
    ["tk", banToChuc.TaiKhoanId],
  ].forEach(function (capTruongGiaTri) {
    dienGiaTriQuanTri("qt-tc-" + capTruongGiaTri[0], capTruongGiaTri[1]);
  });
  document.getElementById("qt-tc-ten").focus();
}
function dienFormSuaTaiKhoanQuanTri(taiKhoanId) {
  var taiKhoan = danhMucQuanTri.TaiKhoan.find(function (taiKhoan) {
    return taiKhoan.Id === taiKhoanId;
  });
  [
    ["id", taiKhoan.Id],
    ["ten", taiKhoan.HoTen],
    ["email", taiKhoan.Email],
    ["mk", ""],
  ].forEach(function (capTruongGiaTri) {
    dienGiaTriQuanTri("qt-tk-" + capTruongGiaTri[0], capTruongGiaTri[1]);
  });
  document.getElementById("qt-tk-ten").focus();
}
async function guiFormQuanTri(suKienDom, taoRequest) {
  suKienDom.preventDefault();
  if (dangLuuQuanTri) return;
  dangLuuQuanTri = true;
  try {
    var cauHinhYeuCauQuanTri = taoRequest();
    await guiYeuCauApi(cauHinhYeuCauQuanTri.path, cauHinhYeuCauQuanTri.method, cauHinhYeuCauQuanTri.body);
    suKienDom.target.reset();
    thongBao("Đã lưu thay đổi.");
    await taiDanhMucQuanTri();
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  } finally {
    dangLuuQuanTri = false;
  }
}
function luuSuKienQuanTri(suKienDom) {
  return guiFormQuanTri(suKienDom, function () {
    var suKienId = docSoTuInput("qt-sk-id");
    var duLieuSuKien = {
      Ten: docGiaTriInput("qt-sk-ten"),
      MoTa: docGiaTriInput("qt-sk-mota"),
      HinhAnh: docGiaTriInput("qt-sk-anh") || null,
      BanToChucId: docSoTuInput("qt-sk-btc"),
      LoaiSuKienId: docSoTuInput("qt-sk-loai"),
      SoNguoi: docSoTuInput("qt-sk-sl"),
      BatDau: new Date(docGiaTriInput("qt-sk-bd")).toISOString(),
      KetThuc: new Date(docGiaTriInput("qt-sk-kt")).toISOString(),
    };
    if (suKienId) {
      var suKienTruocCapNhat = danhMucQuanTri.SuKien.find(function (suKien) {
        return suKien.Id === suKienId;
      });
      if (
        new Date(docGiaTriInput("qt-sk-bd")).getTime() ===
        new Date(ngayNhapQuanTri(suKienTruocCapNhat.BatDau)).getTime()
      )
        duLieuSuKien.BatDau = chuyenNgayDuLieuSangDate(suKienTruocCapNhat.BatDau).toISOString();
      if (
        new Date(docGiaTriInput("qt-sk-kt")).getTime() ===
        new Date(ngayNhapQuanTri(suKienTruocCapNhat.KetThuc)).getTime()
      )
        duLieuSuKien.KetThuc = chuyenNgayDuLieuSangDate(suKienTruocCapNhat.KetThuc).toISOString();
    }
    return {
      path: "/api/admin/catalog/events" + (suKienId ? "/" + suKienId : ""),
      method: suKienId ? "PUT" : "POST",
      body: duLieuSuKien,
    };
  });
}
function luuToChucQuanTri(suKienDom) {
  return guiFormQuanTri(suKienDom, function () {
    var banToChucId = docSoTuInput("qt-tc-id");
    return {
      path: "/api/admin/catalog/organizers" + (banToChucId ? "/" + banToChucId : ""),
      method: banToChucId ? "PUT" : "POST",
      body: {
        Ten: docGiaTriInput("qt-tc-ten"),
        MoTa: docGiaTriInput("qt-tc-mota"),
        TaiKhoanId: docSoTuInput("qt-tc-tk"),
      },
    };
  });
}
function luuTaiKhoanQuanTri(suKienDom) {
  return guiFormQuanTri(suKienDom, function () {
    var taiKhoanId = docSoTuInput("qt-tk-id");
    var duLieuTaiKhoan = {
      HoTen: docGiaTriInput("qt-tk-ten"),
      Email: docGiaTriInput("qt-tk-email"),
      MatKhauMoi: docGiaTriInput("qt-tk-mk") || null,
    };
    if (!taiKhoanId) {
      duLieuTaiKhoan.MatKhau = duLieuTaiKhoan.MatKhauMoi;
      delete duLieuTaiKhoan.MatKhauMoi;
    }
    return {
      path: taiKhoanId ? "/api/admin/catalog/accounts/" + taiKhoanId : "/api/auth/register",
      method: taiKhoanId ? "PUT" : "POST",
      body: duLieuTaiKhoan,
    };
  });
}
async function xoaBanGhiQuanTri(loaiDanhMuc, banGhiId) {
  if (!confirm("Xóa bản ghi này? Chỉ xóa được khi chưa có dữ liệu liên quan.")) return;
  try {
    await guiYeuCauApi("/api/admin/catalog/" + loaiDanhMuc + "/" + banGhiId, "DELETE");
    thongBao("Đã xóa bản ghi.");
    if (loaiDanhMuc === "ticket-types") await taiLoaiVeQuanTri();
    else await taiDanhMucQuanTri();
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  }
}
async function taiLoaiVeQuanTri() {
  var suKienId = docSoTuInput("qt-ve-sk");
  danhSachLoaiVeQuanTri = [];
  if (!suKienId) {
    document.getElementById("qt-ds-ve").innerHTML =
      '<div class="empty"><p>Chọn sự kiện để xem các hạng vé.</p></div>';
    return;
  }
  hienKhungDangTai("qt-ds-ve", 3);
  try {
    var phanHoiLoaiVe = await guiYeuCauApi("/api/events/" + suKienId + "/ticket-types");
    danhSachLoaiVeQuanTri = phanHoiLoaiVe.data;
    document.getElementById("qt-ds-ve").innerHTML = taoHtmlBang(
      [
        ["Id", "Mã"],
        ["Ten", "Hạng vé"],
        ["Gia", "Giá", "tien"],
        ["SoLuong", "Số lượng"],
        ["DaBan", "Đã bán"],
        ["DangGiu", "Đang giữ"],
      ],
      danhSachLoaiVeQuanTri,
      function (loaiVe) {
        return nutQuanTri("ticket-types", loaiVe.Id, "dienFormSuaLoaiVeQuanTri");
      },
    );
  } catch (loiXuLy) {
    hienLoiTai("qt-ds-ve", loiXuLy, taiLoaiVeQuanTri);
    thongBao(loiXuLy.message, "loi");
  }
}
function dienFormSuaLoaiVeQuanTri(loaiVeId) {
  var loaiVe = danhSachLoaiVeQuanTri.find(function (loaiVe) {
    return loaiVe.Id === loaiVeId;
  });
  [
    ["id", loaiVe.Id],
    ["ten", loaiVe.Ten],
    ["gia", loaiVe.Gia],
    ["sl", loaiVe.SoLuong],
    ["mo", ngayNhapQuanTri(loaiVe.MoBan)],
    ["dong", ngayNhapQuanTri(loaiVe.DongBan)],
  ].forEach(function (capTruongGiaTri) {
    dienGiaTriQuanTri("qt-ve-" + capTruongGiaTri[0], capTruongGiaTri[1]);
  });
}
async function luuLoaiVeQuanTri(suKienDom) {
  suKienDom.preventDefault();
  if (dangLuuQuanTri) return;
  dangLuuQuanTri = true;
  try {
    var loaiVeId = docSoTuInput("qt-ve-id"),
      suKienId = docSoTuInput("qt-ve-sk");
    await guiYeuCauApi(
      loaiVeId ? "/api/ticket-types/" + loaiVeId : "/api/events/" + suKienId + "/ticket-types",
      loaiVeId ? "PUT" : "POST",
      {
        Ten: docGiaTriInput("qt-ve-ten"),
        Gia: docSoTuInput("qt-ve-gia"),
        SoLuong: docSoTuInput("qt-ve-sl"),
        MoBan: new Date(docGiaTriInput("qt-ve-mo")).toISOString(),
        DongBan: new Date(docGiaTriInput("qt-ve-dong")).toISOString(),
      },
    );
    thongBao("Đã lưu hạng vé.");
    dienGiaTriQuanTri("qt-ve-id", "");
    await taiLoaiVeQuanTri();
  } catch (loiXuLy) {
    thongBao(loiXuLy.message, "loi");
  } finally {
    dangLuuQuanTri = false;
  }
}
document.addEventListener("DOMContentLoaded", taiDanhMucQuanTri);
