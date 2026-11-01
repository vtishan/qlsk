const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const vm = require('node:vm');
const path = require('node:path');

function runtime(files, nodes = {}) {
  const context = vm.createContext({
    console, URL, Date, Map, Set, Number, String, Object, Array,
    location: { href: 'http://localhost/', pathname: '/', hash: '' },
    document: { getElementById: id => nodes[id] || null, addEventListener() {}, querySelectorAll: () => [] },
    store: { token: 'test-only', vaiTro: 'QuanTriVien', taiKhoanID: '1' },
    skeleton() {}, thongBao(message, type) { if (type === 'loi') throw new Error(message); },
    noiDungGiaoDien: value => String(value ?? ''), esc: value => String(value ?? ''),
    isoLocal: value => value, badgeTT: value => value, bangHTML: () => '',
    val: id => String(nodes[id]?.value ?? '').trim(), num: id => Number(nodes[id]?.value) || null,
    ngayDuLieu: value => new Date(/Z$/.test(value) ? value : value + 'Z')
  });
  for (const file of files) vm.runInContext(fs.readFileSync(path.join(__dirname, '../wwwroot/js', file), 'utf8'), context);
  return context;
}

test('Reloading events reads new backend data instead of a permanent frontend cache', async () => {
  const host = { innerHTML: '', getAttribute: () => null, classList: { remove() {}, add() {} } };
  const context = runtime(['sukien.js'], { 'ds-sukien': host });
  let requests = 0;
  context.apiFetch = async () => ({ data: [{ Id: 1, Ten: ++requests === 1 ? 'Tên trước' : 'Tên đã sửa', TrangThai: 1, BatDau: '2026-12-01T00:00:00Z' }] });
  await context.taiDanhSachSuKien(); assert.match(host.innerHTML, /Tên trước/);
  await context.taiDanhSachSuKien(); assert.match(host.innerHTML, /Tên đã sửa/); assert.equal(requests, 2);
});

test('Venue images use the stored path for arbitrary IDs and a placeholder for empty data', () => {
  const context = runtime(['diadiem.js']);
  assert.match(context.anhDiaDiemHTML({ Id: 987, Ten: 'Địa điểm thật', HinhAnh: '/uploads/venue.jpg' }), /src="\/uploads\/venue.jpg"/);
  assert.match(context.anhDiaDiemHTML({ Id: 1, Ten: 'Không có ảnh' }), /src="\/assets\/images\/placeholder-venue.svg"/);
});

test('Category navigation renders names supplied by the backend', async () => {
  const host = { children: [], replaceChildren() { this.children = []; }, appendChild(child) { this.children.push(child); } };
  const context = runtime(['sukien.js']);
  context.document.querySelectorAll = selector => selector === '[data-event-categories]' ? [host] : [];
  context.document.createElement = () => ({});
  context.apiFetch = async url => { assert.equal(url, '/api/event-types'); return { data: [{ Id: 82, Ten: 'Chủ đề mới trong database' }] }; };
  await context.taiLoaiSuKien();
  assert.deepEqual(host.children.map(child => child.textContent), ['Tất cả', 'Chủ đề mới trong database']);
});

test('Editing a booked event preserves UTC milliseconds when the date field is unchanged', async () => {
  const nodes = Object.fromEntries(['id','ten','mota','anh','btc','loai','sl','bd','kt'].map(name => ['qt-sk-' + name, { value: '' }]));
  Object.assign(nodes['qt-sk-id'], { value: '7' });
  const context = runtime(['admin-catalog.js'], nodes);
  const original = { Id: 7, Ten: 'Sự kiện', BanToChucId: 3, LoaiSuKienId: 9, SoNguoi: 100, BatDau: '2026-12-01T10:30:00.123', KetThuc: '2026-12-01T12:30:00.123' };
  context.danhMucQuanTri = { SuKien: [original] };
  nodes['qt-sk-bd'].value = context.ngayNhapQuanTri(original.BatDau).replace(/:00$/, '');
  nodes['qt-sk-kt'].value = context.ngayNhapQuanTri(original.KetThuc).replace(/:00$/, '');
  let saved = false;
  context.apiFetch = async (url, method, body) => {
    assert.equal(url, '/api/admin/catalog/events/7'); assert.equal(method, 'PUT');
    assert.equal(body.BatDau, '2026-12-01T10:30:00.123Z'); assert.equal(body.KetThuc, '2026-12-01T12:30:00.123Z'); saved = true;
  };
  context.taiDanhMucQuanTri = async () => {};
  await context.luuQuanTri({ preventDefault() {}, target: { reset() {} } }, 'events');
  assert.ok(saved);
});
