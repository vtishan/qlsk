const { test: khaiBaoKiemThu } = require("node:test");
const kiemTraKetQua = require("node:assert/strict");
const heThongTep = require("node:fs");
const mayAoJavaScript = require("node:vm");
const duongDanTep = require("node:path");
function taoMoiTruongKiemThuFrontend(danhSachTepJavaScript, cacPhanTuDom = {}) {
  const nguCanhKiemThu = mayAoJavaScript.createContext({
    console,
    URL,
    Date,
    Map,
    Set,
    Number,
    String,
    Object,
    Array,
    location: {
      href: "http://localhost/",
      pathname: "/",
      hash: "",
    },
    document: {
      getElementById: (phanTuDomId) => cacPhanTuDom[phanTuDomId] || null,
      addEventListener() {},
      querySelectorAll: () => [],
    },
    phienDangNhap: {
      token: "test-only",
      vaiTro: "QuanTriVien",
      taiKhoanID: "1",
    },
    hienKhungDangTai() {},
    thongBao(thongDiepHienThi, loaiThongBao) {
      if (loaiThongBao === "loi") throw new Error(thongDiepHienThi);
    },
    chuyenGiaTriThanhVanBan: (giaTriCanXuLy) => String(giaTriCanXuLy ?? ""),
    maHoaKyTuHtml: (giaTriCanXuLy) => String(giaTriCanXuLy ?? ""),
    dinhDangNgayGioVietNam: (giaTriCanXuLy) => giaTriCanXuLy,
    taoNhanTrangThai: (giaTriCanXuLy) => giaTriCanXuLy,
    taoHtmlBang: () => "",
    docGiaTriInput: (phanTuDomId) => String(cacPhanTuDom[phanTuDomId]?.value ?? "").trim(),
    docSoTuInput: (phanTuDomId) => Number(cacPhanTuDom[phanTuDomId]?.value) || null,
    chuyenNgayDuLieuSangDate: (giaTriCanXuLy) => new Date(/Z$/.test(giaTriCanXuLy) ? giaTriCanXuLy : giaTriCanXuLy + "Z"),
  });
  nguCanhKiemThu.window = nguCanhKiemThu;
  for (const tenTepJavaScript of danhSachTepJavaScript)
    mayAoJavaScript.runInContext(heThongTep.readFileSync(duongDanTep.join(__dirname, "../wwwroot/js", tenTepJavaScript), "utf8"), nguCanhKiemThu);
  return nguCanhKiemThu;
}
khaiBaoKiemThu("Reloading events reads new backend data instead of a permanent frontend cache", async () => {
  const khungHienThiHtml = {
    innerHTML: "",
    getAttribute: () => null,
    classList: {
      remove() {},
      add() {},
    },
  };
  const nguCanhKiemThu = taoMoiTruongKiemThuFrontend(["sukien.js"], {
    "ds-sukien": khungHienThiHtml,
  });
  let yeuCauDaGui = 0;
  nguCanhKiemThu.guiYeuCauApi = async () => ({
    data: [
      {
        Id: 1,
        Ten: ++yeuCauDaGui === 1 ? "Tên trước" : "Tên đã sửa",
        TrangThai: 1,
        BatDau: "2026-12-01T00:00:00Z",
      },
    ],
  });
  await nguCanhKiemThu.taiDanhSachSuKien();
  kiemTraKetQua.match(khungHienThiHtml.innerHTML, /Tên trước/);
  await nguCanhKiemThu.taiDanhSachSuKien();
  kiemTraKetQua.match(khungHienThiHtml.innerHTML, /Tên đã sửa/);
  kiemTraKetQua.equal(yeuCauDaGui, 2);
});
khaiBaoKiemThu("Venue images use the stored path for arbitrary IDs and a placeholder for empty data", () => {
  const nguCanhKiemThu = taoMoiTruongKiemThuFrontend(["diadiem.js"]);
  kiemTraKetQua.match(
    nguCanhKiemThu.anhDiaDiemHTML({
      Id: 987,
      Ten: "Địa điểm thật",
      HinhAnh: "/uploads/venue.jpg",
    }),
    /src="\/uploads\/venue.jpg"/,
  );
  kiemTraKetQua.match(
    nguCanhKiemThu.anhDiaDiemHTML({
      Id: 1,
      Ten: "Không có ảnh",
    }),
    /src="\/assets\/images\/placeholder-venue.svg"/,
  );
});
khaiBaoKiemThu("Category navigation renders names supplied by the backend", async () => {
  const khungHienThiHtml = {
    children: [],
    replaceChildren() {
      this.children = [];
    },
    appendChild(phanTuCon) {
      this.children.push(phanTuCon);
    },
  };
  const nguCanhKiemThu = taoMoiTruongKiemThuFrontend(["sukien.js"]);
  nguCanhKiemThu.document.querySelectorAll = (boChonPhanTu) => (boChonPhanTu === "[data-event-categories]" ? [khungHienThiHtml] : []);
  nguCanhKiemThu.document.createElement = () => ({});
  nguCanhKiemThu.guiYeuCauApi = async (urlYeuCau) => {
    kiemTraKetQua.equal(urlYeuCau, "/api/event-types");
    return {
      data: [
        {
          Id: 82,
          Ten: "Chủ đề mới trong database",
        },
      ],
    };
  };
  await nguCanhKiemThu.taiLoaiSuKien();
  kiemTraKetQua.deepEqual(
    khungHienThiHtml.children.map((phanTuCon) => phanTuCon.textContent),
    ["Tất cả", "Chủ đề mới trong database"],
  );
});
khaiBaoKiemThu("Editing a booked event preserves UTC milliseconds when the date field is unchanged", async () => {
  const cacPhanTuDom = Object.fromEntries(
    ["id", "ten", "mota", "anh", "btc", "loai", "sl", "bd", "kt"].map((tenDoiTuong) => [
      "qt-sk-" + tenDoiTuong,
      {
        value: "",
      },
    ]),
  );
  Object.assign(cacPhanTuDom["qt-sk-id"], {
    value: "7",
  });
  const nguCanhKiemThu = taoMoiTruongKiemThuFrontend(["admin-catalog.js"], cacPhanTuDom);
  const suKienTruocThayDoi = {
    Id: 7,
    Ten: "Sự kiện",
    BanToChucId: 3,
    LoaiSuKienId: 9,
    SoNguoi: 100,
    BatDau: "2026-12-01T10:30:00.123",
    KetThuc: "2026-12-01T12:30:00.123",
  };
  nguCanhKiemThu.danhMucQuanTri = {
    SuKien: [suKienTruocThayDoi],
  };
  cacPhanTuDom["qt-sk-bd"].value = nguCanhKiemThu.ngayNhapQuanTri(suKienTruocThayDoi.BatDau).replace(/:00$/, "");
  cacPhanTuDom["qt-sk-kt"].value = nguCanhKiemThu.ngayNhapQuanTri(suKienTruocThayDoi.KetThuc).replace(/:00$/, "");
  let daGuiYeuCauLuu = false;
  nguCanhKiemThu.guiYeuCauApi = async (urlYeuCau, phuongThucHttp, noiDungYeuCau) => {
    kiemTraKetQua.equal(urlYeuCau, "/api/admin/catalog/events/7");
    kiemTraKetQua.equal(phuongThucHttp, "PUT");
    kiemTraKetQua.equal(noiDungYeuCau.BatDau, "2026-12-01T10:30:00.123Z");
    kiemTraKetQua.equal(noiDungYeuCau.KetThuc, "2026-12-01T12:30:00.123Z");
    daGuiYeuCauLuu = true;
  };
  nguCanhKiemThu.taiDanhMucQuanTri = async () => {};
  await nguCanhKiemThu.luuSuKienQuanTri({
    preventDefault() {},
    target: {
      reset() {},
    },
  });
  kiemTraKetQua.ok(daGuiYeuCauLuu);
});
khaiBaoKiemThu("Each catalog form keeps its own create/update request contract", async () => {
  for (const loaiDanhMuc of ["events", "organizers", "accounts"]) {
    for (const banGhiKiemThuId of ["", "7"]) {
      const tienToTruongNhapLieu = {
        events: "sk",
        organizers: "tc",
        accounts: "tk",
      }[loaiDanhMuc];
      const cacGiaTriHienThi = {
        id: banGhiKiemThuId,
        ten: "Tên",
        mota: "Mô tả",
        anh: "",
        btc: "3",
        loai: "9",
        sl: "100",
        bd: "2026-12-01T10:00",
        kt: "2026-12-01T12:00",
        tk: "2",
        email: "new@test.local",
        mk: "Test@123456",
      };
      const cacPhanTuDom = Object.fromEntries(
        Object.entries(cacGiaTriHienThi).map(([tenThuocTinhJson, giaTriCanXuLy]) => [
          "qt-" + tienToTruongNhapLieu + "-" + tenThuocTinhJson,
          {
            value: giaTriCanXuLy,
          },
        ]),
      );
      const nguCanhKiemThu = taoMoiTruongKiemThuFrontend(["admin-catalog.js"], cacPhanTuDom);
      nguCanhKiemThu.danhMucQuanTri = {
        SuKien: [
          {
            Id: 7,
            BatDau: "2026-12-01T10:00:00Z",
            KetThuc: "2026-12-01T12:00:00Z",
          },
        ],
      };
      let soLanGuiLuuDuLieu = 0;
      nguCanhKiemThu.guiYeuCauApi = async (urlYeuCau, phuongThucHttp, noiDungYeuCau) => {
        kiemTraKetQua.equal(
          urlYeuCau,
          loaiDanhMuc === "accounts" && !banGhiKiemThuId
            ? "/api/auth/register"
            : "/api/admin/catalog/" + loaiDanhMuc + (banGhiKiemThuId ? "/" + banGhiKiemThuId : ""),
        );
        kiemTraKetQua.equal(phuongThucHttp, banGhiKiemThuId ? "PUT" : "POST");
        if (loaiDanhMuc === "accounts") {
          kiemTraKetQua.equal(noiDungYeuCau[banGhiKiemThuId ? "MatKhauMoi" : "MatKhau"], cacGiaTriHienThi.mk);
          kiemTraKetQua.equal(Object.hasOwn(noiDungYeuCau, banGhiKiemThuId ? "MatKhau" : "MatKhauMoi"), false);
        } else if (loaiDanhMuc === "organizers") kiemTraKetQua.equal(noiDungYeuCau.TaiKhoanId, 2);
        else kiemTraKetQua.equal(noiDungYeuCau.BanToChucId, 3);
        soLanGuiLuuDuLieu++;
      };
      nguCanhKiemThu.taiDanhMucQuanTri = async () => {};
      const tenHamXuLyThaoTac = {
        events: "luuSuKienQuanTri",
        organizers: "luuToChucQuanTri",
        accounts: "luuTaiKhoanQuanTri",
      }[loaiDanhMuc];
      await nguCanhKiemThu[tenHamXuLyThaoTac]({
        preventDefault() {},
        target: {
          reset() {},
        },
      });
      kiemTraKetQua.equal(soLanGuiLuuDuLieu, 1);
    }
  }
});
khaiBaoKiemThu("Refund form sends a payment ID and unchanged refund body", async () => {
  const nguCanhKiemThu = taoMoiTruongKiemThuFrontend(["donve.js"], {
    "ht-thanh-toan-id": {
      value: "42",
    },
    "ht-tien": {
      value: "100000",
    },
    "ht-magd": {
      value: "REF-42",
    },
  });
  let daGuiYeuCauLuu = false;
  nguCanhKiemThu.guiYeuCauApi = async (urlYeuCau, phuongThucHttp, noiDungYeuCau) => {
    kiemTraKetQua.equal(urlYeuCau, "/api/payments/42/refund");
    kiemTraKetQua.equal(phuongThucHttp, "POST");
    kiemTraKetQua.equal(noiDungYeuCau.SoTien, 100000);
    kiemTraKetQua.equal(noiDungYeuCau.MaHoanTien, "REF-42");
    daGuiYeuCauLuu = true;
  };
  nguCanhKiemThu.taiDonVeCuaTaiKhoan = () => {};
  await nguCanhKiemThu.guiYeuCauHoanTien();
  kiemTraKetQua.ok(daGuiYeuCauLuu);
});
khaiBaoKiemThu("Viewing issued tickets only reads the order and navigates", async () => {
  const nguCanhKiemThu = taoMoiTruongKiemThuFrontend(["donve.js"]);
  const yeuCauDaGui = [];
  nguCanhKiemThu.guiYeuCauApi = async (urlYeuCau, phuongThucHttp) => {
    yeuCauDaGui.push({
      url: urlYeuCau,
      method: phuongThucHttp,
    });
    return {
      data: {
        Id: 27,
      },
    };
  };
  nguCanhKiemThu.location.assign = (urlYeuCau) => {
    kiemTraKetQua.equal(urlYeuCau, "/pages/ve/ve-theo-don.html?id=27");
  };
  await nguCanhKiemThu.moTrangVeTheoDon(27);
  kiemTraKetQua.deepEqual(yeuCauDaGui, [
    {
      url: "/api/orders/27",
      method: undefined,
    },
  ]);
});
khaiBaoKiemThu("Explicit drawer decisions preserve handlers, refresh, and duplicate-click protection", async () => {
  for (const [thaoTacCanThucThi, tenHamXuLyThaoTac, canTaiLaiDanhSach] of [
    ["duyetSuKienTrongHopChiTiet", "duyetSuKien", false],
    ["tuChoiSuKienTrongHopChiTiet", "tuChoiSuKien", false],
    ["duyetYeuCauXacThucTrongHopChiTiet", "duyetYeuCauToChuc", true],
    ["tuChoiYeuCauXacThucTrongHopChiTiet", "tuChoiYeuCauToChuc", true],
  ]) {
    const cacNutThaoTac = [
      {
        disabled: false,
      },
      {
        disabled: false,
      },
    ];
    let soLanDongHop = 0,
      soLanTaiLai = 0,
      soLanGoiHandler = 0,
      hoanTatThaoTacCho;
    const hopChiTiet = {
      open: true,
      querySelectorAll: () => cacNutThaoTac,
      close() {
        soLanDongHop++;
        this.open = false;
      },
    };
    const nguCanhKiemThu = taoMoiTruongKiemThuFrontend(["ui.js"], {
      "ui-drawer": hopChiTiet,
    });
    nguCanhKiemThu.document.querySelector = () => null;
    nguCanhKiemThu[tenHamXuLyThaoTac] = (banGhiKiemThuId) => {
      kiemTraKetQua.equal(banGhiKiemThuId, 7);
      soLanGoiHandler++;
      return new Promise((baoHoanTatThaoTac) => {
        hoanTatThaoTacCho = baoHoanTatThaoTac;
      });
    };
    nguCanhKiemThu.hienDanhSachDuyetToChuc = async () => {
      soLanTaiLai++;
    };
    const thaoTacDangCho = nguCanhKiemThu.giaoDienQuanLySuKien[thaoTacCanThucThi](7);
    kiemTraKetQua.ok(cacNutThaoTac.every((nutThaoTac) => nutThaoTac.disabled));
    await nguCanhKiemThu.giaoDienQuanLySuKien[thaoTacCanThucThi](7);
    kiemTraKetQua.equal(soLanGoiHandler, 1);
    hoanTatThaoTacCho(true);
    await thaoTacDangCho;
    kiemTraKetQua.equal(soLanDongHop, 1);
    kiemTraKetQua.equal(soLanTaiLai, canTaiLaiDanhSach ? 1 : 0);
    kiemTraKetQua.ok(cacNutThaoTac.every((nutThaoTac) => !nutThaoTac.disabled));
  }
});
khaiBaoKiemThu("Named activity actions keep the existing PUT payload for venues and categories", async () => {
  for (const [tenTepJavaScript, tenBoNhoDem, banGhiKiemThu, tienToTruongNhapLieu, duongDanYeuCau] of [
    [
      "sukien.js",
      "__loaiSuKienCache",
      {
        Id: 5,
        Ten: "Chủ đề",
        MoTa: "Mô tả",
      },
      "LoaiSuKien",
      "/api/event-types/5",
    ],
    [
      "diadiem.js",
      "__diaDiemCache",
      {
        Id: 5,
        Ten: "Địa điểm",
        DiaChi: "Địa chỉ",
        SucChua: 100,
        GiaThue: 200000,
        HinhAnh: "/venue.jpg",
      },
      "DiaDiem",
      "/api/venues/5",
    ],
  ]) {
    const nguCanhKiemThu = taoMoiTruongKiemThuFrontend([tenTepJavaScript]);
    nguCanhKiemThu[tenBoNhoDem] = [banGhiKiemThu];
    nguCanhKiemThu.taiLoaiSuKien = nguCanhKiemThu.taiDanhSachDiaDiem = () => {};
    const cacTrangThaiHienThi = [];
    nguCanhKiemThu.guiYeuCauApi = async (urlYeuCau, phuongThucHttp, noiDungYeuCau) => {
      kiemTraKetQua.equal(urlYeuCau, duongDanYeuCau);
      kiemTraKetQua.equal(phuongThucHttp, "PUT");
      kiemTraKetQua.equal(noiDungYeuCau.Ten, banGhiKiemThu.Ten);
      if (tienToTruongNhapLieu === "DiaDiem") {
        kiemTraKetQua.equal(noiDungYeuCau.HinhAnh, banGhiKiemThu.HinhAnh);
        kiemTraKetQua.equal(noiDungYeuCau.SucChua, banGhiKiemThu.SucChua);
      }
      cacTrangThaiHienThi.push(noiDungYeuCau.HoatDong);
    };
    await nguCanhKiemThu["bat" + tienToTruongNhapLieu](5);
    await nguCanhKiemThu["tat" + tienToTruongNhapLieu](5);
    kiemTraKetQua.deepEqual(cacTrangThaiHienThi, [true, false]);
  }
});
