import { action, dataTable, element, facts, heading, layout, notice, panel, routeLink, tabLinks } from '../../shared/ui/view.js';
import { field, fieldGrid, fieldset, finishForm, formValues, staticForm } from '../../shared/ui/forms.js';
import { currency, recordPath, timeLabel } from '../../shared/ui/catalog.js';
import { slot } from '../../shared/ui/templates.js';
import { workspace } from '../../shared/ui/workspace.js';
import { downloadBlob, enabled, endpoint, items, read, refreshView, write } from '../../app/api.js';

const privateData = 'Cần tài khoản và dữ liệu được cấp quyền để xem vé và đơn hàng.';
const orderStates = { PENDING_PAYMENT: 'Chờ thanh toán', PAID: 'Đã thanh toán', EXPIRED: 'Hết hạn giữ ghế',
  CANCELLED: 'Đã hủy', PAID_OUT_OF_STOCK: 'Đã thu tiền, ghế không còn', REFUND_PENDING: 'Chờ hoàn tiền', REFUNDED: 'Đã hoàn tiền' };
const ticketStates = { ISSUED: 'Đã phát hành', CHECKED_IN: 'Đã check-in', CANCELLED: 'Đã hủy', REFUNDED: 'Đã hoàn tiền', REVOKED: 'Đã thu hồi' };
const tabs = [['Vé của tôi', 'mine'], ['Lịch sử đơn', 'orders'], ['Vé mời', 'invitations'], ['Đặt vé', 'book']];

function bookingSteps(current) {
  const steps = element('ol', '', 'progress-track ticket-steps');
  for (const [index, label] of ['Chọn sự kiện', 'Chọn suất', 'Chọn khu vực và ghế', 'Tóm tắt và thanh toán'].entries()) {
    const step = element('li', label);
    if (current === index) step.setAttribute('aria-current', 'step');
    steps.append(step);
  }
  return steps;
}

function renderSeatChoice(inventory, eventId, sessionId, query, api, signal) {
  const verified = enabled(api, 'orders.create', true) && inventory?.policyId != null;
  const zones = inventory?.zones;
  const selectedZone = zones?.find(zone => String(zone.id) === query.get('zoneId')) ?? zones?.[0];
  const seats = selectedZone?.seats;
  const selection = new Set();
  const selectionStatus = element('p', '', 'notice');
  const selectionSummary = element('p', 'Chưa chọn ghế.', 'seat-selection-summary');
  selectionStatus.setAttribute('role', 'status');
  const form = staticForm();
  const zoneNav = zones?.length ? tabLinks('Khu vực ghế', zones.map(zone => [zone.name,
    recordPath('tickets/book/seats', eventId, sessionId) + '?' + new URLSearchParams({ zoneId: zone.id })]),
    recordPath('tickets/book/seats', eventId, sessionId) + '?' + new URLSearchParams({ zoneId: selectedZone.id })) : null;
  const legend = element('ul', '', 'seat-legend');
  for (const label of ['Còn trống', 'Đã chọn', 'Đang giữ', 'Đã bán', 'Bị khóa']) legend.append(element('li', label));
  const controls = new Map();
  const statusName = { AVAILABLE: 'Còn trống', HELD: 'Đang giữ', BOOKED: 'Đã bán', BLOCKED: 'Bị khóa' };
  const remaining = Number.isFinite(inventory?.remainingQuota) ? Math.max(0, inventory.remainingQuota) : 0;
  function markSelection(message = '') {
    selectionStatus.textContent = message || (inventory ? 'Đã chọn ' + selection.size + ' ghế. Hạn mức còn lại từ hệ thống: ' + remaining + '.'
      : 'Hệ thống sẽ xác nhận tồn ghế, hạn mức và giá khi giữ chỗ.');
    const chosen = seats?.filter(seat => selection.has(seat.id)) ?? [];
    const total = chosen.reduce((sum, seat) => sum + (seat.price ?? 0), 0);
    selectionSummary.textContent = chosen.length ? 'Ghế: ' + chosen.map(seat => seat.label).join(', ') +
      ' · Tạm tính: ' + currency(total) + ' · Tổng chốt được xác nhận trong đơn.' : 'Chưa chọn ghế.';
    for (const [id, control] of controls) control.setAttribute('aria-pressed', String(selection.has(id)));
  }
  function switchSeats(seat) {
    const group = seat.groupId == null ? [seat] : seats.filter(item => item.groupId === seat.groupId);
    if (group.some(item => item.status !== 'AVAILABLE' || item.ticketTypeId == null)) {
      markSelection('Cụm ghế này có chỗ chưa khả dụng. Hãy chọn cụm khác.');
      return;
    }
    if (selection.has(seat.id)) group.forEach(item => selection.delete(item.id));
    else {
      const count = group.filter(item => !selection.has(item.id)).length;
      if (selection.size + count > remaining) {
        markSelection('Số ghế chọn vượt hạn mức còn lại cho suất này.');
        return;
      }
      group.forEach(item => selection.add(item.id));
    }
    markSelection();
  }
  const map = element('div', '', 'seat-map');
  map.setAttribute('aria-label', 'Sơ đồ ghế ' + (selectedZone?.name ?? 'theo suất diễn'));
  if (seats?.length) {
    for (const seat of seats) {
      const control = action(seat.label, () => switchSeats(seat), {
        disabled: !verified || seat.status !== 'AVAILABLE' || seat.ticketTypeId == null, style: 'secondary seat'
      });
      control.setAttribute('aria-label', 'Ghế ' + seat.label + ' · ' + (seat.typeLabel ?? '') + ' · ' +
        (statusName[seat.status] || 'Chưa khả dụng') + (currency(seat.price) ? ' · ' + currency(seat.price) : ''));
      control.title = [seat.typeLabel, currency(seat.price)].filter(Boolean).join(' · ');
      control.setAttribute('aria-pressed', 'false');
      control.dataset.status = seat.status;
      controls.set(seat.id, control);
      map.append(control);
    }
  } else map.append(notice(seats ? 'Khu vực này không trả về ghế.' : 'Chưa có dữ liệu sơ đồ ghế từ hệ thống.'));
  if (zoneNav) form.append(zoneNav);
  const voucher = field('Mã khuyến mãi (không bắt buộc)', { name: 'voucherCode', maxLength: 60, disabled: !verified });
  form.append(legend, element('p', 'Sân khấu', 'stage'), map, selectionStatus, selectionSummary, voucher.wrapper,
    layout('actions', action('Xóa ghế đã chọn', () => { selection.clear(); markSelection(); }),
      action('Cập nhật tồn ghế', refreshView),
      routeLink('Về sự kiện', recordPath('event', eventId), 'button secondary')));
  if (inventory?.paidCheckoutAvailable === false)
    form.append(notice('Hệ thống đang xác nhận đơn có tổng thanh toán bằng 0 ₫. Đơn cần thu tiền sẽ mở khi kết nối PayOS.'));
  markSelection();
  let lastRequest;
  let requestKey;
  finishForm(form, [voucher], verified ? '' : 'Cần phiên và dịch vụ giữ ghế. Thanh toán mở sau khi hệ thống xác nhận giữ chỗ.',
    inventory?.paidCheckoutAvailable === false ? 'Xác nhận đơn miễn phí' : 'Giữ chỗ và xem đơn', verified ? async () => {
      if (!selection.size) {
        markSelection('Chọn ghế còn trống trước khi giữ chỗ.');
        map.querySelector('button:not(:disabled)')?.focus();
        throw new Error('Chọn ghế còn trống trước khi giữ chỗ.');
      }
      const selected = seats.filter(seat => selection.has(seat.id));
      const request = {
        sessionId: Number(sessionId), seatIds: selected.map(seat => seat.id),
        items: selected.map(seat => ({ ticketTypeId: seat.ticketTypeId, seatId: seat.id })),
        policyId: inventory.policyId, voucherCode: formValues(form).voucherCode.trim() || undefined
      };
      const signature = JSON.stringify(request);
      if (signature !== lastRequest) {
        requestKey = crypto.randomUUID();
        lastRequest = signature;
      }
      const order = await write(api, 'orders.create', endpoint('orders'), { ...request, idempotencyKey: requestKey });
      if (!order?.id) throw new Error('Dịch vụ chưa trả mã đơn đã giữ.');
      if (!signal.aborted) location.hash = recordPath('tickets/book/checkout', order.id);
    } : undefined);
  return panel('Chọn khu vực và ghế', form);
}

function orderSummary(order) {
  return panel('Tóm tắt đơn', facts([['Sự kiện', order?.eventName], ['Suất diễn', order?.sessionName],
    ['Giá vé', currency(order?.subtotal)], ['Giảm giá', currency(order?.discount)],
    ['Phí dịch vụ', currency(order?.fee)], ['Tổng phải trả', currency(order?.total)],
    ['Trạng thái', order?.statusLabel || orderStates[order?.status]], ['Hạn giữ chỗ', timeLabel(order?.expiresAt)]]),
  dataTable('Chi tiết vé', ['Loại vé', 'Khu vực', 'Ghế', 'Giá chốt'], order?.items?.map(line => [
    line.type, line.zone, line.seat, currency(line.price)
  ]), 'Giá chốt và chi tiết ghế cần dữ liệu đơn từ hệ thống.', 'Đơn chưa trả về dòng vé.'));
}

function paymentContent(order, api, signal) {
  if (order?.status === 'PAID') {
    return panel('Đơn đã xác nhận', routeLink('Mở vé của tôi', 'tickets/mine', 'button'),
      routeLink('Về lịch sử đơn', 'tickets/orders', 'button secondary'));
  }
  const time = element('p', '', 'field-hint');
  const form = staticForm();
  const expires = Date.parse(order?.expiresAt);
  const usable = order?.status === 'PENDING_PAYMENT' && Number.isFinite(expires) && expires > Date.now();
  const ready = usable && order?.allowedActions?.includes('pay') && enabled(api, 'orders.checkout', true);
  finishForm(form, [], ready ? '' : 'Chỉ có thể thanh toán đơn đã được xác nhận giữ chỗ và còn hiệu lực.',
    'Thanh toán PayOS', ready ? async () => {
      if (Date.now() >= expires) throw new Error('Thời hạn giữ chỗ đã kết thúc. Kiểm tra lại trạng thái đơn.');
      const response = await write(api, 'orders.checkout', endpoint('orders', order.id, 'checkout'));
      if (signal.aborted) return;
      const url = new URL(response.checkoutUrl);
      if (url.protocol !== 'https:') throw new Error('Liên kết thanh toán không hợp lệ.');
      location.assign(url.href);
    } : undefined);
  let timer;
  if (usable) {
    const tick = () => {
      const seconds = Math.max(0, Math.ceil((expires - Date.now()) / 1000));
      time.textContent = seconds ? 'Thời gian giữ còn lại: ' + Math.floor(seconds / 60) + ':' +
        String(seconds % 60).padStart(2, '0') : 'Hết thời gian giữ theo đơn. Kiểm tra lại trạng thái với hệ thống.';
      if (!seconds) {
        clearInterval(timer);
        form.querySelector('[type="submit"]').disabled = true;
      }
    };
    timer = setInterval(tick, 1000);
    tick();
    signal.addEventListener('abort', () => clearInterval(timer), { once: true });
  }
  const details = panel('Thanh toán', time, form, layout('actions',
    action('Kiểm tra lại trạng thái đơn', async event => {
      const button = event.currentTarget;
      button.disabled = true;
      try {
        if (order?.allowedActions?.includes('reconcile') && enabled(api, 'orders.reconcile', true))
          await write(api, 'orders.reconcile', endpoint('orders', order.id, 'reconcile'));
        if (!signal.aborted) refreshView();
      } catch (error) { time.textContent = error.message; button.disabled = false; }
    }, { disabled: !order }),
    routeLink('Về lịch sử đơn', 'tickets/orders', 'button secondary')));
  if (order?.status === 'PAID_OUT_OF_STOCK') details.prepend(notice('Đơn chưa phát hành vé. Khoản thu cần đối soát; kiểm tra yêu cầu hoàn tiền hoặc liên hệ hỗ trợ trước khi đặt lại.'));
  if (order?.status === 'PAID_OUT_OF_STOCK') details.append(layout('actions',
    routeLink('Theo dõi hoàn tiền', 'requests/refunds', 'button secondary'),
    routeLink('Liên hệ hỗ trợ', 'requests/support/new', 'button secondary')));
  if (order?.allowedActions?.includes('cancel')) {
    const cancel = staticForm();
    const canCancel = enabled(api, 'orders.cancel', true);
    finishForm(cancel, [], canCancel ? '' : 'Dịch vụ hủy đơn chưa khả dụng.', 'Hủy giữ chỗ', canCancel ? async () => {
      await write(api, 'orders.cancel', endpoint('orders', order.id, 'cancel'));
      if (!signal.aborted) refreshView();
    } : undefined);
    details.append(cancel);
  }
  return details;
}

async function book(path, query, api, signal) {
  const [stage, first, second] = path;
  if (!stage) {
    return [0, panel('Chọn sự kiện',
      element('p', 'Mở sự kiện đã công bố để chọn suất diễn và kiểm tra thời gian bán.'),
      routeLink('Tìm sự kiện', 'events', 'button'), routeLink('Xem lịch diễn', 'calendar', 'button secondary'))];
  }
  if (stage === 'sessions' && first && !second) {
    const event = await read(api, 'catalog.event', endpoint('events', first), { signal });
    const rows = event?.sessions?.map(session => [session.name, timeLabel(session.start), session.venue?.name,
      session.bookable ? routeLink('Chọn ghế', recordPath('tickets/book/seats', event.id, session.id), 'button') : 'Không mở đặt vé']);
    return [1, panel(event?.title ?? 'Chọn suất diễn', dataTable('Suất diễn', ['Suất', 'Bắt đầu', 'Địa điểm', 'Đặt vé'],
      rows, 'Danh sách suất diễn chưa khả dụng.', 'Sự kiện chưa công bố suất diễn.'),
    routeLink('Xem sự kiện', recordPath('event', first), 'button secondary'))];
  }
  if (stage === 'seats' && first && second) {
    const zone = query.get('zoneId');
    const inventory = await read(api, 'orders.inventory', endpoint('sessions', second, 'inventory'),
      { signal, query: zone ? new URLSearchParams({ zoneId: zone }) : undefined });
    return [2, layout('ticket-layout', renderSeatChoice(inventory, first, second, query, api, signal),
      layout('stack', panel('Suất đã chọn', facts([['Sự kiện', inventory?.eventName],
        ['Suất diễn', inventory?.sessionName], ['Bắt đầu', timeLabel(inventory?.start)]]),
      routeLink('Đổi suất', recordPath('tickets/book/sessions', first), 'button secondary')),
      panel('Giữ chỗ', element('p', 'Tối đa 10 vé đã mua và đang giữ cho mỗi tài khoản trong cùng suất. Thời hạn giữ là 10 phút theo xác nhận của hệ thống.'))))];
  }
  if (stage === 'checkout' && first && !second) {
    const order = await read(api, 'orders.detail', endpoint('orders', first), { signal });
    return [3, layout('ticket-layout', orderSummary(order), paymentContent(order, api, signal))];
  }
  return [];
}

async function personalTickets(id, api, signal) {
  if (!id) {
    const records = items(await read(api, 'tickets.mine', endpoint('me', 'tickets'), { signal }));
    const content = layout('stack personal-ticket-list');
    if (!records?.length) return panel('Vé điện tử', notice(records ? 'Bạn chưa có vé.' : privateData),
      routeLink('Tìm sự kiện', 'events', 'button secondary'));
    function card(ticket) {
      const receipt = element('article', '', 'personal-ticket');
      const title = element('h3');
      title.append(routeLink(ticket.eventName || ticket.label, recordPath('tickets/mine', ticket.id)));
      const state = element('span', ticket.statusLabel || ticketStates[ticket.status] || ticket.status, 'personal-ticket-state');
      const copy = layout('stack personal-ticket-copy', layout('personal-ticket-heading', title, state),
        element('p', [ticket.sessionName, ticket.venueName].filter(Boolean).join(' · '), 'muted'),
        element('p', [ticket.type, ticket.zone, ticket.seat && 'Ghế ' + ticket.seat].filter(Boolean).join(' · ')));
      const code = element('code', ticket.code || ticket.label);
      copy.append(code);
      const feedback = element('p', '', 'field-hint');
      feedback.setAttribute('role', 'status');
      const preview = layout('stack personal-ticket-preview');
      preview.hidden = true;
      let detail;
      let pending;
      async function load() {
        if (!detail) {
          pending ??= read(api, 'tickets.detail', endpoint('tickets', ticket.id), { signal, missingRecord: true });
          detail = await pending;
          if (!detail) { pending = undefined; throw new Error('Vé không khả dụng. Mở chi tiết để kiểm tra trạng thái.'); }
          if (detail.code) code.textContent = detail.code;
        }
        return detail;
      }
      async function showQr() {
        const value = await load();
        const url = value.qrImageUrl ? new URL(value.qrImageUrl, location.origin) : null;
        if (!url || url.origin !== location.origin) throw new Error('Mã QR chỉ hiện khi hệ thống cấp vé hợp lệ.');
        if (!preview.querySelector('img')) {
          const image = element('img');
          image.alt = 'Mã QR vé ' + (value.code || ticket.id);
          image.src = url.href;
          preview.append(image, element('p', 'Xuất trình QR đúng suất diễn và ghế trên vé.', 'muted'));
        }
        preview.hidden = false;
        return preview.querySelector('img');
      }
      function control(label, handler) {
        const button = action(label, async () => {
          button.disabled = true;
          feedback.textContent = '';
          try { await handler(); }
          catch (error) { if (!signal.aborted) feedback.textContent = error.message; }
          finally { button.disabled = false; }
        });
        return button;
      }
      const actions = layout('actions personal-ticket-actions');
      if (ticket.status === 'ISSUED') {
        actions.append(control('Xem vé & QR', showQr), control('Tải PDF vé', async () => {
          const value = await load();
          if (!value.allowedActions?.includes('pdf') || !enabled(api, 'tickets.pdf')) throw new Error('Tệp PDF chưa khả dụng cho vé này.');
          const file = await read(api, 'tickets.pdf', endpoint('tickets', ticket.id, 'pdf'), { signal, download: true });
          if (signal.aborted) return;
          if (file?.type !== 'application/pdf') throw new Error('Tệp PDF chưa khả dụng.');
          downloadBlob(file, 'qlsk-ve-' + ticket.id + '.pdf');
        }), control('In vé', async () => {
          const image = await showQr();
          if (!image.complete) await new Promise((resolve, reject) => {
            image.addEventListener('load', resolve, { once: true });
            image.addEventListener('error', () => reject(new Error('Không tải được QR để in.')), { once: true });
          });
          if (!image.naturalWidth) throw new Error('Không tải được QR để in.');
          if (signal.aborted) return;
          receipt.classList.add('ticket-print-target');
          document.body.classList.add('printing-ticket');
          try { window.print(); }
          finally { receipt.classList.remove('ticket-print-target'); document.body.classList.remove('printing-ticket'); }
        }));
      }
      actions.append(routeLink('Chi tiết', recordPath('tickets/mine', ticket.id), 'button secondary'));
      copy.append(actions, feedback);
      receipt.append(copy, preview);
      return receipt;
    }
    const current = records.filter(ticket => ticket.status === 'ISSUED');
    const history = records.filter(ticket => ticket.status !== 'ISSUED');
    content.append(panel('Vé hiện tại', current.length ? layout('stack', ...current.map(card)) : notice('Bạn chưa có vé đang hiệu lực.')));
    if (history.length) {
      const past = element('details', '', 'personal-ticket-history');
      past.open = !current.length;
      past.append(element('summary', 'Lịch sử vé (' + history.length + ')'), layout('stack', ...history.map(card)));
      content.append(past);
    }
    content.append(routeLink('Tìm sự kiện', 'events', 'button secondary'));
    return content;
  }
  const ticket = await read(api, 'tickets.detail', endpoint('tickets', id), { signal, missingRecord: true });
  const ticketNotice = element('p', '', 'field-hint');
  ticketNotice.setAttribute('role', 'status');
  const pdf = action('Tải PDF vé', async () => {
    const file = await read(api, 'tickets.pdf', endpoint('tickets', id, 'pdf'), { signal, download: true });
    if (signal.aborted) return;
    if (file?.type === 'application/pdf') downloadBlob(file, 'qlsk-ve.pdf');
    else ticketNotice.textContent = 'Tệp PDF chưa khả dụng.';
  }, { disabled: !ticket?.allowedActions?.includes('pdf') || !enabled(api, 'tickets.pdf') });
  const qr = element('div', '', 'ticket-qr');
  const qrUrl = ticket?.qrImageUrl ? new URL(ticket.qrImageUrl, location.origin) : null;
  if (qrUrl?.origin === location.origin) {
    const image = element('img');
    image.alt = 'Mã QR vé để xuất trình tại cổng';
    image.src = qrUrl.href;
    qr.append(image);
  } else qr.append(notice('Mã QR chỉ hiện khi hệ thống cấp vé hợp lệ.'));
  return panel('Chi tiết vé',
    ...(ticket ? [] : [notice(ticket === null ? 'Vé không khả dụng.' : privateData)]),
    facts([['Sự kiện', ticket?.eventName], ['Suất diễn', ticket?.sessionName], ['Địa điểm', ticket?.venueName],
      ['Loại vé', ticket?.type], ['Khu vực', ticket?.zone], ['Ghế', ticket?.seat],
      ['Trạng thái', ticket?.statusLabel || ticketStates[ticket?.status]]]),
    qr, layout('actions', pdf, routeLink('Về vé của tôi', 'tickets/mine', 'button secondary'),
      ...(ticket?.allowedActions?.includes('refund') ? [routeLink('Yêu cầu hoàn tiền', 'requests/refunds/new?' + new URLSearchParams({ ticketId: id }), 'button secondary')] : []),
      ...(ticket ? [routeLink('Liên hệ hỗ trợ', 'requests/support/new?' + new URLSearchParams({ ticketId: id }), 'button secondary')] : [])),
    ticketNotice);
}

async function orders(api, signal) {
  const records = items(await read(api, 'orders.list', endpoint('orders'), { signal }));
  return panel('Lịch sử đơn hàng', dataTable('Đơn đã đặt', ['Đơn', 'Sự kiện', 'Thời điểm', 'Tổng chốt', 'Trạng thái'],
    records?.map(order => [routeLink(order.label, recordPath('tickets/book/checkout', order.id)),
      order.eventName, timeLabel(order.createdAt), currency(order.total), order.statusLabel || orderStates[order.status]]),
    privateData, 'Bạn chưa có đơn hàng.'));
}

function invitations(api, signal, query) {
  const form = staticForm();
  const token = field('Mã hoặc liên kết nhận vé mời', { type: 'password', name: 'token', required: true, maxLength: 1000,
    value: query.get('token') || query.get('code') || '', autocomplete: 'off' });
  const result = layout('stack');
  form.append(token.wrapper);
  const ready = enabled(api, 'invitations.resolve', true);
  finishForm(form, [token], ready ? '' : 'Dịch vụ xác minh vé mời chưa khả dụng.', 'Mở vé mời', ready ? async () => {
    result.replaceChildren();
    let value = token.control.value.trim();
    if (value.includes('#') || value.includes('?')) {
      try {
        const link = new URL(value, location.origin);
        const fragment = link.hash.includes('?') ? link.hash.slice(link.hash.indexOf('?') + 1) : link.search.slice(1);
        const params = new URLSearchParams(fragment);
        value = params.get('token') || params.get('code') || value;
      } catch { /* Server validates an invalid pasted token. */ }
    }
    const ticket = await write(api, 'invitations.resolve', endpoint('invitations', 'resolve'), { token: value });
    if (signal.aborted) return;
    const claim = action('Liên kết vào tài khoản', async () => {
      claim.disabled = true;
      try {
        const received = await write(api, 'invitations.claim', endpoint('invitations', 'claim'), { token: value });
        if (!signal.aborted) {
          token.control.value = '';
          location.replace('#tickets/mine/' + received.id);
        }
      } catch (error) {
        result.append(notice(error.message));
        claim.disabled = false;
      }
    }, { disabled: !enabled(api, 'invitations.claim', true) || !ticket?.allowedActions?.includes('claim') });
    const nodes = [facts([['Sự kiện', ticket.eventName], ['Suất diễn', ticket.sessionName],
      ['Ghế', ticket.seat], ['Trạng thái', ticket.statusLabel]])];
    if (ticket.qrImageUrl) {
      const url = new URL(ticket.qrImageUrl, location.origin);
      if (url.origin === location.origin) {
        const image = element('img', '', 'ticket-qr');
        image.alt = 'Mã QR vé mời';
        image.src = url.href;
        nodes.push(image);
      }
    }
    if (ticket.ticketPath) nodes.push(routeLink('Mở vé trong tài khoản', ticket.ticketPath, 'button'));
    else if (ticket.allowedActions?.includes('claim')) nodes.push(claim);
    else nodes.push(notice('Để nhận vé, đăng nhập bằng tài khoản có email hoặc số điện thoại trùng lời mời và đã xác minh. Nếu cần sửa liên hệ, hãy liên hệ Ban tổ chức.'),
      routeLink('Kiểm tra liên hệ tài khoản', 'account/profile', 'button secondary'));
    result.append(...nodes);
    form.reset();
  } : undefined);
  return panel('Vé mời', element('p', 'Mở liên kết do Ban tổ chức gửi, rồi nhận vé bằng tài khoản có liên hệ đã xác minh. Sau khi nhận, QR nằm trong Vé của tôi. Mỗi liên kết có hạn sử dụng và có thể được Ban tổ chức thay thế.'),
    form, result);
}

export async function renderTickets(target, segments, query, { view, api, signal }) {
  const [mode = 'mine', ...path] = segments;
  if (!tabs.some(([, key]) => mode === key) || (mode === 'book' ? path.length > 3 : path.length > 1) ||
      (['orders', 'invitations'].includes(mode) && path.length)) return;
  if (!api?.authenticated) view.prepend(panel('Đăng nhập để tiếp tục',
    element('p', 'Dùng tài khoản đặt vé để giữ ghế, thanh toán hoặc xem vé và đơn hàng.'),
    routeLink('Đăng nhập', 'account/login?' + new URLSearchParams({ next: location.hash.slice(1) || 'tickets/mine' }), 'button')));
  if (mode === 'book') {
    const [step, content] = await book(path, query, api, signal);
    if (!content) return;
    slot(view, 'steps', bookingSteps(step));
    slot(view, 'content', content);
  } else {
    const content = mode === 'mine' ? await personalTickets(path[0], api, signal)
      : mode === 'orders' ? await orders(api, signal) : invitations(api, signal, query);
    slot(view, 'content', content);
  }
  target.append(heading(tabs.find(([, key]) => mode === key)[0]), workspace('tickets', layout('stack', view)));
}
