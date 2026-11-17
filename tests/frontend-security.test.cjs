const { test } = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const vm = require("node:vm");

function environment(elements = {}) {
  const storage = new Map();
  const context = vm.createContext({
    URL, URLSearchParams, AbortController, Map, Set, Date, console, setTimeout, clearTimeout,
    URL_GOC_API: "https://qlsk.test",
    location: { origin: "https://qlsk.test", href: "https://qlsk.test/", pathname: "/" },
    sessionStorage: {
      getItem: key => storage.get(key) || null,
      setItem: (key, value) => storage.set(key, String(value)),
      removeItem: key => storage.delete(key), clear: () => storage.clear()
    },
    document: {
      getElementById: id => elements[id] || null,
      addEventListener() {}, querySelectorAll: () => [],
      body: { classList: { contains: () => false } },
      createElement() { return {}; }
    }
  });
  context.window = context;
  function load(name) { vm.runInContext(fs.readFileSync(path.join(__dirname, "../wwwroot/js", name), "utf8"), context); }
  load("api.js");
  return { context, load, storage };
}
function response(status, data, headers = {}) {
  return { ok: status >= 200 && status < 300, status, headers: { get: name => headers[name] || null }, json: async () => data };
}

test("Concurrent identical reads share a request; a later refresh fetches fresh data", async () => {
  const { context: c } = environment();
  let calls = 0, resolve;
  c.fetch = () => { calls++; return calls === 1 ? new Promise(r => { resolve = r; }) : Promise.resolve(response(200, { data: [2] })); };
  const first = c.guiYeuCauApi("/api/events");
  const concurrent = c.guiYeuCauApi("/api/events");
  assert.equal(first, concurrent);
  assert.equal(calls, 1);
  resolve(response(200, { data: [1] }));
  await Promise.all([first, concurrent]);
  const fresh = await c.guiYeuCauApi("/api/events");
  assert.equal(calls, 2);
  assert.equal(fresh.data[0], 2);
});

test("Changing the login session never shares another user's pending read", async () => {
  const { context: c, storage } = environment();
  let calls = 0;
  c.fetch = async () => { calls++; return response(200, { data: [] }); };
  storage.set("sk_token", "user-a");
  const a = c.guiYeuCauApi("/api/orders/me");
  storage.set("sk_token", "user-b");
  const b = c.guiYeuCauApi("/api/orders/me");
  await Promise.all([a, b]);
  assert.equal(calls, 2);
});

test("429/503 honor Retry-After and do not replay a write", async () => {
  for (const status of [429, 503]) {
    const { context: c } = environment();
    let calls = 0;
    c.fetch = async () => { calls++; return response(status, {}, { "Retry-After": "7" }); };
    await assert.rejects(c.guiYeuCauApi("/api/orders", "POST", { items: [] }), e => e.maTrangThaiHttp === status && e.retryAfter === 7);
    await assert.rejects(c.guiYeuCauApi("/api/orders", "POST", { items: [] }), /7/);
    assert.equal(calls, 1);
  }
});

test("Bearer auth is explicit; mutations preserve their idempotency header", async () => {
  const { context: c, storage } = environment();
  storage.set("sk_token", "private-token");
  c.fetch = async (url, options) => {
    assert.equal(options.credentials, "omit");
    assert.equal(options.headers.Authorization, "Bearer private-token");
    assert.equal(options.headers["Idempotency-Key"], "persisted-request-key");
    assert.equal(options.method, "POST");
    return response(200, { data: { id: 1 } });
  };
  await c.guiYeuCauApi("/api/orders", "POST", {}, null, { idempotencyKey: "persisted-request-key" });
});

test("HTML escaping and JSON normalization resist injected markup and prototype keys", () => {
  const { context: c, load } = environment();
  assert.equal(c.maHoaKyTuHtml('<img src=x onerror="alert(1)">'), "&lt;img src=x onerror=&quot;alert(1)&quot;&gt;");
  const payload = JSON.parse('{"__proto__":{"polluted":true},"constructor":{"prototype":{"polluted":true}},"data":{"name":"safe"}}');
  const normalized = c.chuanHoaPhanHoi(payload);
  assert.equal(normalized.polluted, undefined);
  assert.equal(Object.hasOwn(normalized, "constructor"), false);
  load("sukien.js");
  assert.equal(c.chuanHoaDuLieuSuKien(payload).Polluted, undefined);
});

test("Login return paths reject external, backslash and control-character redirects", () => {
  const { context: c } = environment();
  for (const input of ["//attacker.test", "/\\attacker.test", "/\n/attacker.test", "/\t/attacker.test", "https://attacker.test", "javascript:alert(1)"])
    assert.equal(c.duongDanNoiBo(input), null);
  assert.equal(c.duongDanNoiBo("/pages/don-ve/tao-don.html?event=1#tickets"), "/pages/don-ve/tao-don.html?event=1#tickets");
});

test("Event summaries render venue and minimum price without requesting individual details", () => {
  const { context: c, load } = environment();
  load("sukien.js");
  load("ui.js");
  let requests = 0;
  c.guiYeuCauApi = () => { requests++; throw new Error("No detail fetch is needed"); };
  const events = [{ Id: 1, Ten: "Summary event", MetadataComplete: true, GiaThapNhat: 0, BatDau: "2030-01-01T10:00:00Z",
    LoaiSuKien: { Ten: "Category" }, DatDiaDiem: { DiaDiem: { Ten: "Safe venue" } } }];
  const html = c.giaoDienQuanLySuKien.taoTheSuKien(events[0]);
  assert.match(html, /Safe venue/);
  assert.match(html, /Miễn phí/);
  c.giaoDienQuanLySuKien.capNhatGiaoDienSauTaiSuKien(events, events);
  assert.equal(requests, 0);
});

test("Pagination requests a new page only after an explicit continuation click", async () => {
  const elements = {};
  const host = { after(button) { elements[button.id] = button; } };
  elements.rows = host;
  const { context: c } = environment(elements);
  const paths = [];
  c.fetch = async url => {
    paths.push(new URL(url).searchParams.get("page"));
    return response(200, { data: [{ id: paths.length, ten: "Row" }] }, { "X-Has-More": paths.length === 1 ? "true" : "false" });
  };
  let last;
  const reload = async () => { last = await c.guiYeuCauDanhSach("/api/events", "rows", reload); };
  await reload();
  assert.equal(last.data.length, 1);
  assert.deepEqual(paths, ["1"]);
  await elements["rows-load-more"].onclick();
  assert.deepEqual(paths, ["1", "2"]);
  assert.equal(last.data.length, 2);
  assert.equal(elements["rows-load-more"].hidden, true);
});

test("Statistics use server aggregates instead of counting a bounded list page", async () => {
  const elements = { "tk-dat-tt": {}, "tk-dat-dd": {}, "tk-tochuc": {} };
  const { context: c, load } = environment(elements);
  load("diadiem.js");
  load("tochuc.js");
  const paths = [];
  c.fetch = async url => {
    const pathname = new URL(url).pathname;
    paths.push(pathname);
    if (pathname === "/api/venue-bookings/statistics") return response(200, { data: {
      tongDiaDiem: 1, theoTrangThai: { DaXacNhan: { trangThai: "DaXacNhan", soLuong: 103 } },
      theoDiaDiem: [{ id: 1, tenDiaDiem: "Venue", soLuotDat: 103 }]
    } });
    assert.equal(pathname, "/api/organizer-verifications/statistics");
    return response(200, { data: [{ trangThai: "ChoDuyet", soLuong: 103 }] });
  };
  await c.taiThongKeDatDiaDiem();
  await c.taiThongKeToChuc();
  assert.match(elements["tk-dat-tt"].innerHTML, /103/);
  assert.match(elements["tk-dat-dd"].innerHTML, /103/);
  assert.match(elements["tk-tochuc"].innerHTML, /103/);
  assert.deepEqual(paths, ["/api/venue-bookings/statistics", "/api/organizer-verifications/statistics"]);
});
