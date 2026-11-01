/* ===== tochuc.js — xac thuc ban to chuc ===== */
"use strict";
/* global apiFetch, toast, thongBao, val, bangHTML, badgeTT, skeleton, isoLocal, esc */

async function guiYeuCau() {
  var tenToChuc = val("tc-ten");
  var giayTo = val("tc-giayto");
  if (!tenToChuc || !giayTo) {
    toast("Vui lòng nhập tên tổ chức và đường dẫn giấy tờ xác minh.", "loi");
    return;
  }
  try {
    var documentUrl = new URL(giayTo);
    if (documentUrl.protocol !== "https:" && documentUrl.protocol !== "http:") throw new Error();
  } catch (e) { toast("Vui lòng nhập đường dẫn giấy tờ hợp lệ.", "loi"); return; }
  var button = document.getElementById("btn-gui-xacthuc");
  if (button && button.disabled) return;
  if (button) button.disabled = true;
  try {
    var kq = await apiFetch("/api/organizer-verifications", "POST", {
      TenToChuc: tenToChuc,
      GiayTo: giayTo
    });
    thongBao("Đã gửi hồ sơ xác thực tổ chức.");
    location.assign("/pages/to-chuc/yeu-cau-cua-toi.html");
  } catch (e) { thongBao(e.message, "loi"); }
  finally { if (button) button.disabled = false; }
}

async function capNhatYeuCau() {
  var tenToChuc = val("tc-ten") || val("tc-cccd") || "";
  var giayTo = val("tc-giayto") || val("tc-truoc") || "";
  if (!tenToChuc || !giayTo) {
    toast("Vui long nhap ten to chuc va giay to chung nhan.", "loi");
    return;
  }
  // Backend khong co PUT organizer-verifications/{id}; gui yeu cau moi (POST).
  try {
    await apiFetch("/api/organizer-verifications", "POST", { TenToChuc: tenToChuc, GiayTo: giayTo });
    thongBao("Gui yeu cau moi thanh cong.");
    taiYeuCauCuaToi();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function taiYeuCauCuaToi() {
  var host = document.getElementById("ds-yeucau");
  if (!host || !store.token) return;
  skeleton("ds-yeucau", 3);
  try {
    var kq = await apiFetch("/api/organizer-verifications/me");
    var data = kq.data ? (Array.isArray(kq.data) ? kq.data : [kq.data]) : [];
    var rows = data.map(function (d) {
      return {
        YeuCauID: d.Id,
        SoCCCD: d.TenToChuc,
        TrangThai: typeof d.TrangThai === "number" ? ["ChoDuyet", "DaDuyet", "TuChoi"][d.TrangThai] : d.TrangThai,
        NgayTao: isoLocal(d.NgayTao),
        LyDoTuChoi: d.LyDo
      };
    });
    if (!rows.length) { host.innerHTML = '<div class="empty"><h3>Bạn không có hồ sơ đang chờ xử lý</h3><p>Nếu hồ sơ đã được duyệt, hãy đăng nhập lại để sử dụng quyền ban tổ chức.</p><a class="phu btn" href="gui-xac-thuc.html">Gửi hồ sơ</a></div>'; return; }
    host.innerHTML = '<div class="grid g2">' + rows.map(function (r) {
      return '<div class="card hoverable anim-pop"><h3>Hồ sơ ' + esc(r.SoCCCD) + '</h3>' +
        '<div class="muted">Ngày gửi: ' + esc(r.NgayTao) + "</div>" +
        '<div class="toolbar"><span>' + badgeTT(r.TrangThai) + "</span></div>" +
        (r.LyDoTuChoi ? '<div class="muted">Lý do từ chối: ' + esc(r.LyDoTuChoi) + "</div>" : "") +
        "</div>";
    }).join("") + "</div>";
  } catch (e) { thongBao(e.message, "loi"); }
}
document.addEventListener("DOMContentLoaded", function () {
  if (document.getElementById("ds-yeucau")) taiYeuCauCuaToi();
});

async function duyetYeuCauToChuc(id) {
  try {
    await apiFetch("/api/organizer-verifications/" + id + "/approve", "POST", null);
    thongBao("Da duyet yeu cau #" + id + ".");
    if (typeof taiThongKeToChuc === "function") taiThongKeToChuc();
    taiYeuCauCuaToi();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function tuChoiYeuCauToChuc(id) {
  var lydo = prompt("Nhap ly do tu choi yeu cau #" + id + ":");
  if (lydo === null) return;
  if (!lydo.trim()) { toast("Vui lòng nhập lý do từ chối.", "loi"); return; }
  try {
    await apiFetch("/api/organizer-verifications/" + id + "/reject", "POST", { LyDo: lydo.trim() });
    thongBao("Da tu choi yeu cau #" + id + ".");
    if (typeof taiThongKeToChuc === "function") taiThongKeToChuc();
    taiYeuCauCuaToi();
  } catch (e) { thongBao(e.message, "loi"); }
}

async function taiThongKeToChuc() {
  try {
    var kq = await apiFetch("/api/organizer-verifications");
    var rows = kq.data || [];
    var host = document.getElementById("tk-tochuc");
    if (host) host.innerHTML = bangHTML(
      [["TrangThai", "Trang thai"], ["SoLuong", "So luong"]],
      groupCount(rows, function (r) { return ["ChoDuyet", "DaDuyet", "TuChoi"][r.TrangThai] || r.TrangThai; }));
  } catch (e) { thongBao(e.message, "loi"); }
}

function groupCount(rows, keyFn) {
  var map = {};
  rows.forEach(function (r) {
    var k = keyFn(r);
    map[k] = (map[k] || 0) + 1;
  });
  return Object.keys(map).map(function (k) { return { TrangThai: k, SoLuong: map[k] }; });
}
