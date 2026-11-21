// Only the deployment can advertise ready operations. Missing capability or denied reads never act as empty data.
let capabilities;
let capabilityExpiry = 0;
const failedReads = new WeakMap();
export const readFailure = signal => failedReads.get(signal);
export const endpoint = (...segments) => '/api/' + segments.map(part => encodeURIComponent(part)).join('/');

export function resetCapabilities() {
  capabilities = undefined;
}

export function refreshView() {
  resetCapabilities();
  window.dispatchEvent(new Event('qlsk:refresh'));
}

export function apiCapabilities() {
  if (Date.now() >= capabilityExpiry) {
    capabilityExpiry = Date.now() + 30000;
    capabilities = undefined;
  }
  capabilities ??= fetch('/api/capabilities', { credentials: 'same-origin', cache: 'no-store', signal: AbortSignal.timeout(6000), headers: { Accept: 'application/json' } })
    .then(response => response.ok ? response.json() : null)
    .then(data => data?.version === 1 && Array.isArray(data.operations) ? data : null)
    .catch(() => null);
  return capabilities;
}

export function enabled(api, operation, write = false) {
  return Boolean(api?.operations.includes(operation) && (!write || api.csrfToken));
}

async function call(path, { method = 'GET', query, body, signal, csrf, download = false, missingRecord = false } = {}) {
  const suffix = query?.size ? '?' + query : '';
  const headers = { Accept: download ? '*/*' : 'application/json' };
  if (csrf) headers['X-CSRF-Token'] = csrf;
  const json = body !== undefined && !(body instanceof FormData);
  if (json) headers['Content-Type'] = 'application/json';
  const response = await fetch(path + suffix, {
    method, headers, signal, credentials: 'same-origin', cache: 'no-store',
    body: json ? JSON.stringify(body) : body
  });
  if (!response.ok) {
    if (missingRecord && response.status === 404) return null;
    const detail = await response.json().catch(() => undefined);
    const error = new Error(detail?.message || ({ 400: 'Vui lòng kiểm tra lại dữ liệu đã nhập.', 401: 'Vui lòng đăng nhập để tiếp tục.', 403: 'Tài khoản cần quyền phù hợp.',
      409: 'Dữ liệu đã thay đổi. Tải lại trước khi thử lại.', 422: 'Kiểm tra lại thông tin và điều kiện nghiệp vụ.',
      429: 'Đã gửi quá nhiều yêu cầu. Vui lòng thử lại sau.' })[response.status] || 'Dịch vụ chưa phản hồi thành công.');
    error.status = response.status;
    error.fields = detail?.errors;
    if (response.status === 401 || response.status === 403) resetCapabilities();
    throw error;
  }
  if (download) return response.blob();
  if (response.status === 204) return true;
  const contentType = response.headers.get('content-type') ?? '';
  if (contentType.includes('application/json')) return response.json();
  if (response.status === 202 && method !== 'GET') return true;
  throw new Error('Phản hồi dịch vụ không đúng định dạng.');
}

export async function read(api, operation, path, options) {
  if (!enabled(api, operation) || !path) return undefined;
  try {
    return await call(path, options);
  } catch (error) {
    if (options?.signal && !options.signal.aborted && !failedReads.has(options.signal)) {
      failedReads.set(options.signal, error);
    }
    return undefined;
  }
}

export function write(api, operation, path, body, method = 'POST') {
  if (!enabled(api, operation, true)) throw new Error('Thao tác này chưa khả dụng.');
  return call(path, { method, body, csrf: api.csrfToken });
}

export const items = response => Array.isArray(response) ? response : Array.isArray(response?.items) ? response.items : undefined;

export function downloadBlob(file, filename) {
  const url = URL.createObjectURL(file);
  const link = document.createElement('a');
  link.href = url;
  link.download = filename;
  link.click();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}
