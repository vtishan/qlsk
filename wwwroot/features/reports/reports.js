import { action, dataTable, element, layout, notice, openDialog } from '../../shared/ui/view.js';
import { field, fieldset, queryField, queryForm, staticForm } from '../../shared/ui/forms.js';
import { slot } from '../../shared/ui/templates.js';
import { workspace } from '../../shared/ui/workspace.js';
import { downloadBlob, enabled, endpoint, read } from '../../app/api.js';
import { allowed, readContext, workspaceGroups } from '../work/work-context.js';
import { sections } from '../work/work-sections.js';

const tooBroad = 'Chưa lấy được dữ liệu báo cáo. Kiểm tra phạm vi và thu hẹp khoảng ngày nếu cần.';
const needsAccess = 'Dữ liệu báo cáo cần phiên đăng nhập và quyền do máy chủ cấp.';
const number = new Intl.NumberFormat('vi-VN');
const scopeByType = { orders: 'event', checkins: 'session', settlements: 'optional', payouts: 'optional' };
const formats = [
  ['xlsx', 'Excel (.xlsx)', 'Bảng tính', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'],
  ['docx', 'Word (.docx)', 'Tài liệu có bảng', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document'],
  ['pdf', 'In hoặc lưu PDF', 'Mở hộp in của trình duyệt']
];

function formatDialog(target, trigger, api, report, params, signal) {
  const canExport = report?.allowedActions?.includes('export') && enabled(api, 'reports.export');
  const form = staticForm();
  const choices = fieldset('Định dạng');
  choices.classList.add('report-formats');
  for (const [value, label, description] of formats) {
    const option = field(label, { name: 'format', type: 'radio', value, checked: value === 'xlsx',
      disabled: value === 'pdf' && canExport && !enabled(api, 'reports.print') });
    option.wrapper.querySelector('label').append(element('small', description));
    choices.append(option.wrapper);
  }
  const submit = action('Xuất báo cáo', null, { type: 'submit', style: '', disabled: !canExport });
  const feedback = element('p', '', 'field-hint');
  feedback.setAttribute('role', 'status');
  if (report?.rows?.length) form.append(element('p', report.title + ' · ' + number.format(report.rows.length) + ' dòng', 'muted'));
  form.append(choices);
  if (!canExport) form.append(notice(report?.rows?.length === 0
    ? 'Chọn phạm vi có dữ liệu để xuất báo cáo.' : 'Cần dữ liệu báo cáo và quyền xuất do máy chủ cấp.'));
  form.append(layout('actions', submit), feedback);
  form.addEventListener('submit', async () => {
    if (!canExport || submit.disabled) return;
    const [format, , , mime] = formats.find(([value]) => value === new FormData(form).get('format')) ?? formats[0];
    if (format === 'pdf') {
      if (enabled(api, 'reports.print')) feedback.textContent = printReport(report)
        ? 'Đã mở cửa sổ in.' : 'Cho phép mở cửa sổ in rồi thử lại.';
      return;
    }
    submit.disabled = true;
    feedback.textContent = 'Đang tạo tệp…';
    const query = new URLSearchParams(params);
    query.set('format', format);
    const blob = await read(api, 'reports.export', endpoint('reports', 'export'), { signal, query, download: true });
    if (signal.aborted || !form.isConnected) return;
    if (blob?.type === mime) {
      downloadBlob(blob, 'qlsk-bao-cao.' + format);
      feedback.textContent = 'Đã tải báo cáo.';
    } else feedback.textContent = 'Tệp báo cáo chưa khả dụng.';
    submit.disabled = false;
  });
  openDialog(target, trigger, 'Xuất báo cáo', form);
}

function printReport(report) {
  const preview = window.open('', '_blank', 'popup');
  if (!preview) return false;
  preview.opener = null;
  preview.document.title = report.title + ' · QLSK';
  preview.document.documentElement.lang = 'vi';
  const style = preview.document.createElement('style');
  style.textContent = 'body{font:12px system-ui;margin:24px}table{border-collapse:collapse;width:100%}td,th{border:1px solid #999;padding:7px;text-align:left}thead{display:table-header-group}tr{break-inside:avoid}';
  preview.document.head.append(style);
  const results = reportTable(report);
  preview.document.body.append(results.querySelector('table'));
  preview.focus();
  preview.print();
  return true;
}

function reportTable(report) {
  const rows = report.rows.map(row => row.map((value, index) =>
    report.columns[index].includes('₫') && /^-?\d+$/.test(value) ? number.format(BigInt(value)) : value));
  return dataTable(report.title, report.columns, rows, tooBroad, 'Không có dữ liệu phù hợp với bộ lọc.');
}

function reportForm(params, choices, context, activeType) {
  const type = queryField(params, 'type', 'Loại báo cáo', { choices, placeholder: 'Chọn loại báo cáo',
    disabled: !choices.length, required: !!choices.length });
  const event = queryField(params, 'eventId', 'Sự kiện', { placeholder: 'Chọn sự kiện',
    choices: context.events.map(item => [String(item.id), item.name]) });
  const session = queryField(params, 'sessionId', 'Suất diễn', { choices: [] });
  const dateOptions = { type: 'date', min: '1970-01-01', max: '9998-12-31', validateOnBlur: false };
  const from = queryField(params, 'from', 'Từ ngày', dateOptions);
  const to = queryField(params, 'to', 'Đến ngày', { ...dateOptions, validate: value =>
    !value || !from.control.value || value >= from.control.value ? '' : 'Ngày kết thúc phải từ ngày bắt đầu trở đi.' });
  const status = queryField(params, 'status', 'Trạng thái', { choices: [] });

  function updateSessions(preferred) {
    const records = context.events.find(item => String(item.id) === event.control.value)?.sessions ?? [];
    const required = type.control.value === 'checkins';
    session.control.replaceChildren(new Option(required ? 'Chọn suất diễn' : 'Mọi suất diễn', ''));
    for (const item of records) session.control.add(new Option(item.name, item.id));
    session.control.value = preferred || (required && records.length === 1 ? String(records[0].id) : '');
    if (session.control.selectedIndex === -1) session.control.selectedIndex = 0;
    session.control.disabled = event.control.disabled || !records.length;
    session.control.required = required;
  }

  function updateType(selectedStatus) {
    const key = type.control.value;
    const scope = scopeByType[key];
    if (!event.control.value && scope && context.events.length === 1) event.control.value = String(context.events[0].id);
    event.control.required = scope === 'event' || scope === 'session';
    event.control.disabled = !scope || !context.events.length;
    event.wrapper.hidden = !scope;
    session.wrapper.hidden = !scope || scope === 'optional';
    updateSessions(key === activeType ? params.get('sessionId') : '');
    if (scope === 'optional') session.control.disabled = true;
    let states = [];
    if (['orders', 'applications', 'rentals', 'refunds', 'settlements', 'payouts'].includes(key)) states = sections[key].states;
    if (key === 'rentals') states = states.filter(([value]) => value !== 'APPROVED_HOLD');
    if (key === 'refunds') states = states.filter(([value]) => value !== 'AUTO_APPROVED').concat([['APPROVED', 'Đã duyệt']]);
    status.control.replaceChildren(new Option('Mọi trạng thái', ''));
    for (const [value, name] of states) status.control.add(new Option(name, value));
    status.control.value = Array.from(status.control.options).some(option => option.value === selectedStatus) ? selectedStatus : '';
    status.wrapper.hidden = status.control.disabled = !states.length;
  }

  if (activeType) type.control.value = activeType;
  updateType(params.get('status'));
  type.control.addEventListener('change', () => updateType(''));
  event.control.addEventListener('change', () => updateSessions(''));
  from.control.addEventListener('input', () => {
    if (to.control.hasAttribute('aria-invalid')) to.check();
  });
  const fields = [type, event, session, from, to, status];
  const form = queryForm(fields, 'export', 'Lọc báo cáo');
  form.noValidate = true;
  form.addEventListener('reset', () => queueMicrotask(() => {
    type.control.value = activeType || '';
    event.control.value = params.get('eventId') || '';
    updateType('');
    for (const item of fields) item.showError('');
  }));
  form.addEventListener('submit', submit => {
    const invalid = fields.filter(control => !control.check());
    if (!invalid.length) return;
    submit.preventDefault();
    submit.stopImmediatePropagation();
    invalid[0].control.focus();
  }, { capture: true });
  if (!choices.length) form.querySelector('[type="submit"]').disabled = true;
  return form;
}

export async function renderExport(target, segments, query, { view, api, signal }) {
  if (segments.length) return;
  const scopes = await read(api, 'work.scopes', endpoint('manage', 'scopes'), { signal });
  const types = [['checkins', 'Check-in và lượt tham dự'], ['orders', 'Đơn hàng và vé'], ['refunds', 'Hoàn tiền'],
    ['settlements', 'Doanh thu và đối soát'], ['payouts', 'Giải ngân']];
  for (const [name, section] of Object.entries(sections)) {
    if (section.columns && !types.some(([key]) => key === name)) types.push([name, section.title]);
  }
  const choices = types.filter(([key]) => api?.reportTypes?.includes(key) ?? (!api && allowed(api, key)));
  const explicitType = query.get('type');
  const activeType = explicitType || api?.reportTypes?.[0];
  const validType = choices.some(([key]) => key === activeType);
  const scope = scopeByType[activeType];
  const context = readContext(query, scopes, { scope });
  const params = new URLSearchParams();
  if (validType) params.set('type', activeType);
  for (const name of ['from', 'to', 'status']) if (query.get(name)) params.set(name, query.get(name));
  for (const name of ['eventId', 'sessionId']) if (scope && context[name]) params.set(name, context[name]);
  const missingScope = scope === 'event' && !context.eventId ? 'Chọn sự kiện cần báo cáo.'
    : scope === 'session' && !context.sessionId ? 'Chọn sự kiện và suất diễn cần báo cáo.' : '';
  const message = !enabled(api, 'reports.preview') ? needsAccess : !validType ? 'Loại báo cáo chưa khả dụng với tài khoản này.'
    : missingScope || '';
  const report = message ? undefined : await read(api, 'reports.preview', endpoint('reports'), { query: params, signal });
  const summary = Array.isArray(report?.rows) ? number.format(report.rows.length) + ' dòng theo bộ lọc' : '';
  const trigger = action('Chọn định dạng xuất', () => formatDialog(target, trigger, api, report, params, signal),
    { style: report?.allowedActions?.includes('export') ? '' : 'secondary' });
  trigger.setAttribute('aria-haspopup', 'dialog');

  if (query.size && !missingScope) view.querySelector('#report-preview-title').setAttribute('data-route-focus', '');
  slot(view, 'filters', reportForm(params, choices, context, validType ? activeType : ''));
  slot(view, 'summary', summary);
  slot(view, 'actions', trigger);
  slot(view, 'preview', report ? reportTable(report) : notice(message || tooBroad));
  target.append(workspace('export', view, workspaceGroups(api, context), api?.workspace?.roleLabel));
}
