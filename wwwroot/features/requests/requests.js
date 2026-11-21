import { dataTable, element, facts, heading, layout, notice, panel, routeLink } from '../../shared/ui/view.js';
import { field, fieldGrid, fieldset, finishForm, formValues, staticForm } from '../../shared/ui/forms.js';
import { currency, recordPath, timeLabel } from '../../shared/ui/catalog.js';
import { slot } from '../../shared/ui/templates.js';
import { workspace } from '../../shared/ui/workspace.js';
import { enabled, endpoint, items, read, refreshView, write } from '../../app/api.js';

const groups = {
  rentals: {
    title: 'Thuê địa điểm', resource: 'venue-rentals', newLabel: 'Tạo yêu cầu thuê',
    phases: [['REQUESTED', 'Đã gửi'], ['APPROVED_HOLD', 'Duyệt giữ lịch'], ['AWAITING_DEPOSIT', 'Chờ đặt cọc'], ['CONFIRMED', 'Xác nhận thuê']],
    labels: { REJECTED: 'Từ chối', EXPIRED: 'Quá hạn cọc', CANCELLED: 'Đã hủy' },
    unavailable: 'Danh sách yêu cầu thuê cần phiên đăng nhập và dữ liệu từ dịch vụ địa điểm.'
  },
  refunds: {
    title: 'Hoàn tiền', resource: 'refund-requests', newLabel: 'Tạo yêu cầu hoàn tiền',
    phases: [['REQUESTED', 'Chờ xét duyệt'], ['APPROVED', 'Đã duyệt'], ['MANUAL_REVIEW', 'Xét ngoại lệ'],
      ['PROCESSING', 'Đang chuyển tiền'], ['COMPLETED', 'Đã hoàn tiền']],
    labels: { FAILED: 'Chuyển tiền thất bại', REJECTED: 'Từ chối' },
    unavailable: 'Cần dữ liệu vé, giao dịch thực thu và quyền truy cập để xem yêu cầu hoàn tiền.'
  },
  support: {
    title: 'Hỗ trợ', resource: 'support-cases', newLabel: 'Gửi yêu cầu hỗ trợ',
    phases: [['OPEN', 'Đã mở'], ['IN_PROGRESS', 'Đang xử lý'], ['RESOLVED', 'Đã giải quyết'], ['CLOSED', 'Đã đóng']],
    labels: {}, unavailable: 'Cần đăng nhập để xem yêu cầu hỗ trợ và phản hồi.'
  }
};

function stateLabel(group, code) {
  return group.labels[code] ?? group.phases.find(([key]) => key === code)?.[1] ?? code;
}

function progress(group, state) {
  const steps = element('ol', '', 'progress-track');
  const phases = group === groups.refunds
    ? [['REQUESTED|MANUAL_REVIEW', 'Chờ xét điều kiện hoàn'], ['APPROVED|AUTO_APPROVED', 'Đã duyệt'],
      ['PROCESSING', 'Đang chuyển tiền'], ['COMPLETED', 'Đã hoàn tiền']]
    : group.phases;
  for (const [code, label] of phases) {
    const step = element('li', label);
    if (code.split('|').includes(state)) step.setAttribute('aria-current', 'step');
    steps.append(step);
  }
  return steps;
}

const vietnamDate = value => Date.parse(value + ':00+07:00');
const phone = value => /^\+?[\d ().-]+$/.test(value) && value.replace(/\D/g, '').length >= 9
  ? '' : 'Nhập số điện thoại hợp lệ.';

async function requestForm(kind, query, api, signal) {
  const group = groups[kind];
  const operation = 'requests.' + kind + '.create';
  const form = staticForm();
  const required = (label, name, options = {}) => field(label, { name, required: true, ...options });
  let fields;
  let available;
  let payload = () => formValues(form);
  if (kind === 'rentals') {
    const [listing, policies, selected] = await Promise.all([
      read(api, 'catalog.venues', endpoint('venues'), { signal }),
      read(api, 'catalog.policies', endpoint('policies'), { signal, query: new URLSearchParams({ type: 'venue-rental' }) }),
      query.get('venueId') ? read(api, 'catalog.venue', endpoint('venues', query.get('venueId')), { signal }) : undefined
    ]);
    const policyId = items(policies)?.[0]?.id;
    const venues = (items(listing) || []).filter(item => item.id !== selected?.id);
    if (selected) venues.unshift(selected);
    const venue = required('Địa điểm thuê', 'venueId', {
      choices: venues.map(place => [String(place.id), place.name]), placeholder: 'Chọn địa điểm',
      value: selected?.id?.toString() ?? '', disabled: !venues.length
    });
    const date = query.get('date');
    const suggestedHour = /^\d{4}-\d{2}-\d{2}$/.test(date) && Date.parse(date + 'T09:00+07:00') > Date.now()
      ? date + 'T09:00' : '';
    const from = required('Bắt đầu', 'from', { type: 'datetime-local', value: suggestedHour,
      hint: suggestedHour ? 'Ngày lấy từ lịch địa điểm. Kiểm tra lại giờ bắt đầu.' : undefined,
      validate: value => vietnamDate(value) > Date.now() ? '' : 'Chọn giờ bắt đầu trong tương lai (giờ Việt Nam).' });
    const to = required('Kết thúc', 'to', { type: 'datetime-local', validate: value =>
      !from.control.value || vietnamDate(value) > vietnamDate(from.control.value) ? '' : 'Giờ kết thúc phải sau giờ bắt đầu.' });
    const event = [
      required('Tên sự kiện', 'eventName', { maxLength: 180 }),
      required('Loại hình sự kiện', 'eventType', { choices: ['Hội thảo', 'Tiệc', 'Triển lãm', 'Workshop', 'Giải trí', 'Khác'],
        placeholder: 'Chọn loại hình' }),
      required('Mục đích tổ chức', 'reason', { type: 'textarea', maxLength: 500, wide: true }),
      required('Kịch bản và hoạt động chính', 'program', { type: 'textarea', maxLength: 1500, wide: true }),
      field('Thiết bị cần thuê thêm', { name: 'equipment', type: 'textarea', maxLength: 700 }),
      field('Lưu ý đặc biệt', { name: 'specialNotes', type: 'textarea', maxLength: 700 }),
      required('Họ tên người phụ trách', 'representative', { maxLength: 120 }),
      required('Số điện thoại trực tiếp', 'phone', { type: 'tel', maxLength: 20, validate: phone })
    ];
    const slotFields = [venue, from, to, required('Số người dự kiến', 'capacity', { type: 'number', min: '1', step: '1' })];
    fields = [...event, ...slotFields];
    available = enabled(api, operation, true) && venues.length && policyId;
    form.append(fieldset('Thông tin sự kiện', fieldGrid(event)), fieldset('Khung giờ thuê · Giờ Việt Nam', fieldGrid(slotFields)),
      notice('Tiền thuê, cọc 30% trong 24 giờ và khoản hoàn cọc do hệ thống áp dụng theo điều khoản đã chốt.'),
      routeLink('Xem chính sách thuê', 'guide'));
    payload = () => {
      const data = formValues(form);
      return { ...data, venueId: Number(data.venueId), from: data.from + ':00+07:00', to: data.to + ':00+07:00',
        capacity: Number(data.capacity), policyId };
    };
  } else if (kind === 'refunds') {
    const eligible = items(await read(api, 'tickets.eligibleRefunds', endpoint('me', 'refundable-tickets'), { signal }));
    const selector = required('Giao dịch thanh toán', 'transactionId', {
      choices: [...new Map(eligible?.map(ticket => [String(ticket.transactionId), ticket.orderLabel || 'Đơn ' + ticket.orderId]))],
      placeholder: 'Chọn giao dịch', disabled: !eligible?.length
    });
    const preferred = eligible?.find(ticket => String(ticket.id) === query.get('ticketId'));
    if (preferred) selector.control.value = String(preferred.transactionId);
    const selected = new Set();
    const selectionError = element('p', '', 'field-error');
    selectionError.setAttribute('role', 'status');
    const choices = fieldset('Vé đủ điều kiện');
    const renderChoices = () => {
      selected.clear();
      choices.replaceChildren(element('legend', 'Vé đủ điều kiện'));
      for (const ticket of eligible?.filter(item => String(item.transactionId) === selector.control.value) ?? []) {
        const checked = String(ticket.id) === query.get('ticketId');
        if (checked) selected.add(ticket.id);
        const choice = field([ticket.label, ticket.sessionName, ticket.seat || ticket.zone].filter(Boolean).join(' · '),
          { type: 'checkbox', checked });
        choice.control.addEventListener('change', () => {
          selectionError.textContent = '';
          if (choice.control.checked) selected.add(ticket.id);
          else selected.delete(ticket.id);
        });
        choices.append(choice.wrapper);
      }
      if (!eligible?.length) choices.append(notice(eligible ? 'Không có vé đủ điều kiện theo dữ liệu trả về.'
        : 'Danh sách vé đủ điều kiện chưa khả dụng.'));
    };
    selector.control.addEventListener('change', renderChoices);
    renderChoices();
    const other = field('Mô tả thêm', { name: 'description', type: 'textarea', wide: true, maxLength: 500 });
    const reason = required('Lý do hoàn', 'reason', { choices: ['Thay đổi kế hoạch', 'Không thể tham dự', 'Đặt nhầm suất hoặc ghế',
      'Sự kiện thay đổi lịch', 'Khác'], placeholder: 'Chọn lý do',
      validate: value => value !== 'Khác' || other.control.value.trim() ? '' : 'Nhập mô tả khi chọn lý do Khác.' });
    fields = [selector, reason, other];
    available = enabled(api, operation, true) && eligible?.length;
    form.append(selector.wrapper, choices, selectionError, fieldGrid([reason, other]),
      notice('Chọn nhiều vé sẽ tạo yêu cầu riêng cho từng vé. Số tiền đề nghị dựa trên giá vé sau giảm giá, chưa gồm phí dịch vụ. Nhân viên kiểm tra điều khoản đã chốt trước khi duyệt; chỉ ghi nhận đã hoàn sau khi đối soát chuyển tiền.'));
    payload = () => {
      if (!selected.size) {
        selectionError.textContent = 'Chọn ít nhất một vé trong giao dịch.';
        choices.querySelector('input')?.focus();
        throw new Error('Chọn ít nhất một vé trong giao dịch.');
      }
      const data = formValues(form);
      return { ...data, transactionId: Number(data.transactionId), ticketIds: [...selected] };
    };
  } else {
    const eventId = query.get('eventId');
    const ticketId = query.get('ticketId');
    const event = eventId ? await read(api, 'catalog.event', endpoint('events', eventId), { signal }) : undefined;
    const ticket = ticketId ? await read(api, 'tickets.detail', endpoint('tickets', ticketId), { signal }) : undefined;
    fields = [required('Chủ đề', 'subject', { maxLength: 160, wide: true }),
      required('Nội dung', 'description', { type: 'textarea', wide: true, maxLength: 1200 })];
    form.append(facts([['Phạm vi', ticket?.label || event?.title || 'Hỗ trợ chung']]), fieldGrid(fields));
    available = enabled(api, operation, true);
    payload = () => ({ ...formValues(form), eventId: event?.id, ticketId: ticket?.id });
  }

  let lastPayload;
  let requestKey;
  finishForm(form, fields, available ? '' : 'Dịch vụ gửi yêu cầu và các dữ liệu xác thực liên quan chưa khả dụng.',
    group.newLabel, available ? async () => {
      const data = payload();
      if (kind === 'rentals') {
        const value = JSON.stringify(data);
        if (value !== lastPayload) { requestKey = crypto.randomUUID(); lastPayload = value; }
        data.requestKey = requestKey;
      }
      const result = await write(api, operation, endpoint(group.resource), data);
      if (!signal.aborted) location.hash = result?.id ? recordPath('requests/' + kind, result.id) : 'requests/' + kind;
    } : undefined);
  return panel(group.newLabel, form);
}

function detailActions(kind, record, api, signal) {
  const actions = layout('stack');
  if (kind !== 'rentals' || !record) return actions;
  if (record.allowedActions?.includes('deposit')) {
    const form = staticForm();
    const ready = enabled(api, 'requests.rentals.deposit', true);
    finishForm(form, [], ready ? '' : 'Dịch vụ thanh toán cọc chưa khả dụng.', 'Mở thanh toán cọc', ready ? async () => {
      const payment = await write(api, 'requests.rentals.deposit', endpoint('venue-rentals', record.id, 'deposit'));
      if (signal.aborted) return;
      const url = new URL(payment.checkoutUrl);
      if (url.protocol !== 'https:' || !(url.hostname === 'pay.payos.vn' || url.hostname.endsWith('.payos.vn'))) throw new Error('Liên kết thanh toán không hợp lệ.');
      location.assign(url.href);
    } : undefined);
    actions.append(form);
  }
  if (record.allowedActions?.includes('reconcileDeposit')) {
    const form = staticForm();
    form.append(notice('Kiểm tra trực tiếp với PayOS sau khi chuyển tiền. Lịch thuê chỉ được xác nhận khi khoản cọc đúng số tiền và còn thời hạn.'));
    const ready = enabled(api, 'requests.rentals.deposit', true);
    finishForm(form, [], ready ? '' : 'Dịch vụ đối soát cọc chưa khả dụng.', 'Kiểm tra thanh toán cọc', ready ? async () => {
      await write(api, 'requests.rentals.deposit', endpoint('venue-rentals', record.id, 'deposit', 'reconciliation'));
      if (!signal.aborted) refreshView();
    } : undefined);
    actions.append(form);
  }
  if (record.allowedActions?.includes('cancel')) {
    const form = staticForm();
    const reason = field('Lý do hủy thuê', { type: 'textarea', name: 'reason', required: true, maxLength: 500 });
    form.append(reason.wrapper);
    const ready = enabled(api, 'requests.rentals.cancel', true);
    finishForm(form, [reason], ready ? '' : 'Dịch vụ hủy thuê chưa khả dụng.', 'Gửi yêu cầu hủy', ready ? async () => {
      await write(api, 'requests.rentals.cancel', endpoint('venue-rentals', record.id, 'cancellations'), formValues(form));
      if (!signal.aborted) refreshView();
    } : undefined);
    const cancellation = element('details', '', 'stack guide-topic');
    cancellation.append(element('summary', 'Hủy thuê'), form);
    actions.append(cancellation);
  }
  return actions;
}

function requestDetail(kind, record, api, signal) {
  const group = groups[kind];
  const context = kind === 'rentals' ? [
    ['Địa điểm', record?.venueName], ['Bắt đầu', timeLabel(record?.from)], ['Kết thúc', timeLabel(record?.to)],
    ['Tiền thuê', currency(record?.total)], ['Tiền cọc', currency(record?.depositAmount)], ['Hạn cọc', timeLabel(record?.depositDueAt)]
  ] : kind === 'refunds' ? [['Đơn hàng', record?.orderLabel], ['Số tiền hoàn', currency(record?.refundAmount)],
    ['Tình trạng chuyển tiền', record?.transferStatusLabel]] : [['Sự kiện / vé liên quan', record?.contextLabel], ['Phản hồi', record?.response]];
  return panel('Chi tiết ' + group.title.toLowerCase(),
    routeLink('Về danh sách', 'requests/' + kind, 'button secondary'),
    ...(record ? [element('h3', record.title)] : [notice(record === null ? 'Yêu cầu không khả dụng.' : group.unavailable)]),
    facts([['Trạng thái', record?.statusLabel || stateLabel(group, record?.status)], ['Ngày gửi', timeLabel(record?.createdAt)],
      ...context, ...(record?.fields?.map(item => [item.label, item.value]) ?? []),
      ['Lý do / nội dung', record?.description], ['Kết quả xử lý', record?.resolution]]),
    detailActions(kind, record, api, signal), element('h3', 'Tiến trình xử lý'), progress(group, record?.status),
    dataTable('Lịch sử cập nhật', ['Thời điểm', 'Trạng thái', 'Nội dung'],
      record?.history?.map(item => [timeLabel(item.at), item.statusLabel ?? stateLabel(group, item.status), item.note]),
      'Lịch sử xử lý chưa khả dụng.', 'Yêu cầu chưa có cập nhật.'));
}

export async function renderRequests(target, segments, query, { view, api, signal }) {
  const [kind = 'rentals', id] = segments;
  if (segments.length > 2 || !Object.hasOwn(groups, kind)) return;
  if (!api?.authenticated) view.prepend(panel('Đăng nhập để gửi và theo dõi yêu cầu',
    element('p', 'Yêu cầu thuê, hoàn tiền và nội dung hỗ trợ được bảo vệ theo tài khoản.'),
    routeLink('Đăng nhập', 'account/login?' + new URLSearchParams({ next: location.hash.slice(1) || 'requests' }), 'button')));
  const group = groups[kind];
  const path = 'requests/' + kind;
  let content;
  if (id === 'new') content = await requestForm(kind, query, api, signal);
  else if (id) content = requestDetail(kind, await read(api, 'requests.' + kind + '.detail',
    endpoint(group.resource, id), { signal, missingRecord: true }), api, signal);
  else {
    const response = await read(api, 'requests.' + kind + '.list', endpoint(group.resource),
      { signal, query: query.has('cursor') ? new URLSearchParams({ cursor: query.get('cursor') }) : undefined });
    const rows = items(response)?.map(item => [routeLink(item.title || 'Xem yêu cầu', recordPath(path, item.id)),
      timeLabel(item.createdAt), item.statusLabel || stateLabel(group, item.status), item.summary]);
    slot(view, 'actions', routeLink(group.newLabel, path + '/new', 'button'));
    content = panel('Danh sách ' + group.title.toLowerCase(),
      dataTable('Yêu cầu đã gửi', ['Yêu cầu', 'Thời điểm', 'Trạng thái', 'Nội dung'], rows, group.unavailable, 'Bạn chưa gửi yêu cầu.'));
    if (query.has('cursor') || response?.nextCursor) content.append(layout('actions',
      ...(query.has('cursor') ? [routeLink('Trang đầu', path, 'button secondary')] : []),
      ...(response?.nextCursor ? [routeLink('Trang sau', path + '?' + new URLSearchParams({ cursor: response.nextCursor }), 'button secondary')] : [])));
  }
  slot(view, 'content', content);
  target.append(heading(group.title), workspace('requests', layout('stack', view)));
}
