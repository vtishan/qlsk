import { action, dataTable, element, facts, heading, layout, notice, openDialog, panel, routeLink, tabLinks } from '../../shared/ui/view.js';
import { field, fieldGrid, fieldset, finishForm, formValues, queryForm, staticForm } from '../../shared/ui/forms.js';
import { currency, matches, recordPath, timeLabel } from '../../shared/ui/catalog.js';
import { slot, template } from '../../shared/ui/templates.js';
import { workspace } from '../../shared/ui/workspace.js';
import { downloadBlob, enabled, endpoint, items, read, refreshView, write } from '../../app/api.js';
import { apiPath, sections } from './work-sections.js';
import { allowed, hashWith, readContext, routeQuery, workspaceGroups } from './work-context.js';

const restricted = 'Cần phiên và quyền được cấp bởi máy chủ. Dữ liệu vận hành hiện chưa khả dụng.';
const scopeNeeded = 'Chọn sự kiện hoặc suất diễn được giao để mở dữ liệu theo phạm vi.';
const stageLabels = { organizer: 'Ban tổ chức', staff: 'Nhân sự Quản lý sự kiện',
  manager: 'Quản lý hệ thống', admin: 'Admin' };
const operation = (name, kind) => 'work.' + name + '.' + kind;

function scopePicker(path, context, section, scopeUnavailable) {
  const { events, sessions, event, session } = context;
  const required = section.scope === 'session' || section.scope === 'event';
  const input = field('Sự kiện', {
    name: 'eventId', choices: events.map(record => [String(record.id), record.name]),
    placeholder: required ? 'Chọn sự kiện' : 'Mọi sự kiện trong phạm vi', value: event?.id?.toString() ?? '',
    disabled: !events.length
  });
  const controls = [input];
  if (section.scope === 'session' || section.scopedQuery === 'session') {
    const select = field('Suất diễn', {
      name: 'sessionId', choices: sessions.map(record => [String(record.id), record.name]),
      placeholder: 'Chọn suất diễn', value: session?.id?.toString() ?? '', disabled: !sessions.length
    });
    input.control.addEventListener('change', () => {
      const other = events.find(item => String(item.id) === input.control.value);
      select.control.replaceChildren(new Option('Chọn suất diễn', ''));
      for (const item of other?.sessions ?? []) select.control.add(new Option(item.name, item.id));
      select.control.disabled = !other?.sessions?.length;
    });
    controls.push(select);
  }
  const form = queryForm(controls, path, 'Áp dụng phạm vi');
  if (!events.length) return panel('Phạm vi làm việc', form,
    notice(scopeUnavailable ? restricted : 'Máy chủ chưa trả về sự kiện nào trong phạm vi tài khoản.'));
  return panel('Phạm vi làm việc', form);
}

function previewValue(record, [key, label, format]) {
  const value = record[key];
  if (format === 'money') return currency(value);
  if (format === 'date') return timeLabel(value);
  return value == null ? undefined : String(value);
}

function fieldFor([key, label, kind, optional], context) {
  const choices = kind === 'session' ? context.sessions.map(item => [String(item.id), item.name])
    : Array.isArray(kind) ? kind : items(context.options?.[kind])?.map(item => [String(item.id), item.name]);
  const options = { name: key, required: !optional, maxLength: 200 };
  if (choices || ['category', 'organizer', 'invitationType', 'invitationSeat', 'session', 'venue', 'zone',
    'editableVersion', 'permissionGroup', 'reviewer', 'eligibleEvent', 'confirmedRental'].includes(kind)) {
    return field(label, { ...options, choices: choices ?? [], placeholder: 'Chọn ' + label.toLowerCase(),
      disabled: !choices?.length });
  }
  if (kind === 'textarea') return field(label, { ...options, type: 'textarea', maxLength: 1800, wide: true });
  if (kind === 'number') return field(label, { ...options, type: 'number', min: ['capacity', 'quantity', 'totalLimit', 'perAccountLimit'].includes(key) ? '1' : '0', step: '1' });
  return field(label, { ...options, type: kind ?? 'text' });
}

function configureFields(fields) {
  const byName = name => fields.find(item => item.control.name === name);
  for (const [startName, endName] of [['from', 'to'], ['salesFrom', 'salesTo']]) {
    const start = byName(startName), end = byName(endName);
    if (!start || !end) continue;
    const check = end.check;
    end.check = () => {
      if (!check()) return false;
      const invalid = !end.control.disabled && start.control.value && end.control.value && end.control.value <= start.control.value;
      if (invalid) end.showError('Thời điểm kết thúc phải sau thời điểm bắt đầu.');
      return !invalid;
    };
    start.control.addEventListener('input', () => { if (end.control.hasAttribute('aria-invalid')) end.check(); });
  }
}

function fillValue(control, value) {
  if (value == null) return;
  if (control.type === 'datetime-local') {
    const time = new Date(value);
    if (!Number.isNaN(time.valueOf())) value = new Date(time.valueOf() + 7 * 3600000).toISOString().slice(0, 16);
  }
  control.value = String(value);
}

function formPayload(form) {
  const values = formValues(form);
  for (const [name, value] of Object.entries(values)) {
    const control = form.elements.namedItem(name);
    if (value === '') delete values[name];
    else if (name.endsWith('Id') || control.type === 'number') values[name] = Number(value);
    else if (control.type === 'datetime-local') values[name] = value + ':00+07:00';
  }
  return values;
}

async function configureRentalSelector(form, fields, context, api, signal, record) {
  const selector = fields.find(item => item.control.name === 'rentalId');
  if (!selector || !context.eventId) return;
  const feedback = element('div', '', 'stack field-hint');
  selector.wrapper.append(feedback);
  const op = enabled(api, 'work.sessions.create') ? 'work.sessions.create' : 'work.sessions.updateDraft';
  const response = await read(api, op, endpoint('manage', 'events', context.eventId, 'confirmed-rentals'), { signal });
  if (signal.aborted) return;
  const rentals = items(response);
  const control = selector.control;
  control.replaceChildren(new Option(record ? 'Giữ nguyên liên kết thuê hiện tại' : 'Không gắn hồ sơ thuê', ''));
  if (record) control.add(new Option('Bỏ liên kết hồ sơ thuê', '0'));
  for (const rental of rentals || []) control.add(new Option('#' + rental.id + ' · ' + rental.venue + ' · ' + timeLabel(rental.from) + ' – ' + timeLabel(rental.to), rental.id));
  const existing = record?.formValues?.rentalId ?? record?.rentalId;
  if (existing) {
    if (!rentals?.some(rental => String(rental.id) === String(existing))) control.add(new Option('Hồ sơ thuê đang gắn #' + existing, existing));
    control.value = String(existing);
  }
  control.disabled = false;
  function showSelection(fill) {
    const selected = rentals?.find(rental => String(rental.id) === control.value);
    if (selected) {
      feedback.replaceChildren(facts([['Tổng tiền thuê địa điểm', currency(selected.rentalTotal)], ['Tiền cọc thuê địa điểm', currency(selected.deposit)],
        ['Khung thuê', timeLabel(selected.from) + ' – ' + timeLabel(selected.to)]]),
      element('p', 'Giá bán vé được nhập riêng tại Loại vé và giá bán. Kiểm tra lịch suất nằm trong khung thuê đã xác nhận.'));
      if (fill) {
        const venue = fields.find(item => item.control.name === 'venueId');
        if (venue) venue.control.value = String(selected.venueId);
        for (const key of ['from', 'to']) {
          const item = fields.find(item => item.control.name === key);
          if (item && !item.control.value) fillValue(item.control, selected[key]);
        }
      }
    } else feedback.replaceChildren(element('p', rentals ? 'Liên kết thuê là tùy chọn. Địa điểm, lịch suất, lịch bán và sức chứa vẫn cần được kiểm tra riêng.'
      : 'Chưa tải được hồ sơ thuê đã xác nhận. Không chọn sẽ giữ nguyên liên kết hiện tại khi sửa.'));
  }
  control.addEventListener('change', () => showSelection(true));
  showSelection(false);
}

function createForm(name, context, api, signal) {
  const section = sections[name];
  const [label, schema] = section.create;
  const path = apiPath(section, context);
  const op = operation(name, 'create');
  const form = staticForm();
  const fields = schema.map(entry => fieldFor(entry, context));
  configureFields(fields);
  const available = path && (!section.scope || context[section.scope + 'Id']) && enabled(api, op, true) &&
    !fields.some(item => item.control.required && item.control.disabled && !(name === 'guests' && ['ticketTypeId', 'seatId'].includes(item.control.name)));
  form.append(fieldGrid(fields));
  if (name === 'sessions') configureRentalSelector(form, fields, context, api, signal);
  finishForm(form, fields, available ? '' : 'Cần quyền tạo mới và danh mục nghiệp vụ từ máy chủ.',
    label, available ? async () => {
      const response = await write(api, op, path, { eventId: context.eventId, ...formPayload(form) });
      if (signal.aborted) return;
      if (response?.id) location.hash = hashWith(recordPath('work/' + name, response.id), routeQuery(context));
      else refreshView();
    } : undefined);
  if (name === 'guests' && available) {
    const session = fields.find(item => item.control.name === 'sessionId');
    const type = fields.find(item => item.control.name === 'ticketTypeId');
    const seat = fields.find(item => item.control.name === 'seatId');
    const submit = form.querySelector('[type="submit"]');
    const feedback = element('p', 'Chọn suất diễn để lấy loại vé mời và ghế còn trống.', 'field-hint');
    feedback.setAttribute('role', 'status');
    form.prepend(feedback);
    let seats = [], revision = 0;
    const updateSeats = () => {
      const choices = seats.filter(item => String(item.ticketTypeId) === type.control.value);
      seat.control.replaceChildren(new Option('Chọn ghế', ''), ...choices.map(item => new Option(item.name, item.id)));
      seat.control.disabled = !choices.length;
      submit.disabled = !choices.length;
    };
    const updateOptions = async () => {
      const current = ++revision;
      type.control.replaceChildren(new Option('Chọn loại vé mời', ''));
      type.control.disabled = true;
      seats = []; updateSeats();
      if (!session.control.value) return;
      feedback.textContent = 'Đang lấy ghế còn trống…';
      const response = await read(api, 'work.guests.options', endpoint('events', context.eventId, 'invitations', 'options'),
        { signal, query: new URLSearchParams({ sessionId: session.control.value }) });
      if (signal.aborted || current !== revision) return;
      seats = response?.invitationSeat ?? [];
      const types = response?.invitationType ?? [];
      type.control.replaceChildren(new Option('Chọn loại vé mời', ''), ...types.map(item => new Option(item.name, item.id)));
      type.control.disabled = !types.length;
      if (types.length === 1) type.control.value = String(types[0].id);
      feedback.textContent = !response ? 'Không lấy được ghế. Chọn lại suất để thử lại.' : types.length
        ? 'Ghế được kiểm tra lại khi phát hành. Chỉ chọn ghế lẻ; cụm ghế cần phát hành theo nhóm.'
        : 'Suất chưa khai loại vé mời. Thêm loại vé mời trong bản nháp và gửi duyệt trước khi phát hành.';
      updateSeats();
    };
    session.control.addEventListener('change', updateOptions);
    type.control.addEventListener('change', updateSeats);
    if (context.sessionId || context.sessions.length === 1) session.control.value = String(context.sessionId ?? context.sessions[0].id);
    updateOptions();
  }
  return panel(label, form);
}

async function settlementCreateForm(api, signal) {
  const operation = 'work.settlements.create';
  const response = await read(api, operation, endpoint('manage', 'settlements', 'eligible-events'), { signal });
  const events = items(response);
  const event = field('Sự kiện đã hoàn tất', { name: 'eventId', required: true,
    choices: events?.map(item => [item.label, item.value]) ?? [], placeholder: 'Chọn sự kiện', disabled: !events?.length });
  const from = field('Ngày thu từ (giờ Việt Nam)', { name: 'from', type: 'date', required: true });
  const to = field('Ngày thu đến (giờ Việt Nam)', { name: 'to', type: 'date', required: true,
    validate: value => !from.control.value || value >= from.control.value ? '' : 'Ngày kết thúc không được trước ngày bắt đầu.' });
  const form = staticForm();
  form.append(notice('Hệ thống dùng khoản thu và khoản hoàn đã xác minh, cùng phí hợp đồng đã chốt tại từng đơn. Phí dịch vụ được tách khỏi doanh thu vé; điều chỉnh bổ sung bằng 0. Kỳ đã lập không được trùng hoặc chồng lấn.'),
    fieldGrid([event, from, to]));
  const ready = enabled(api, operation, true) && Boolean(events?.length);
  finishForm(form, [event, from, to], ready ? '' : events ? 'Chưa có sự kiện hoàn tất và hết thời hạn chờ khóa sổ.' : restricted,
    'Lập kỳ đối soát', ready ? async () => {
      const result = await write(api, operation, endpoint('manage', 'settlements'), { ...formValues(form), eventId: Number(event.control.value) });
      if (!signal.aborted) location.hash = 'work/settlements/' + result.id;
    } : undefined);
  return panel('Lập kỳ đối soát', form);
}

function decisionForm(target, name, context, api, record, signal, decision) {
  const { key, label, method = 'POST', path, tail, choices, idempotent } = decision;
  let endpointPath;
  if (path) {
    const parts = path(record);
    if (parts) endpointPath = endpoint(...parts);
  } else endpointPath = apiPath(sections[name], context, record.id, ...(tail ? [tail] : []));
  const op = operation(name, key);
  const operationReady = endpointPath && enabled(api, op, true);
  const trigger = action(label, () => {
    const form = staticForm();
    const requestKey = idempotent ? crypto.randomUUID() : undefined;
    const reason = field('Lý do / ghi chú', { name: 'reason', type: 'textarea', required: true, maxLength: 1000, wide: true });
    const fields = choices ? [field('Quyết định', { name: 'action', choices, placeholder: 'Chọn quyết định', required: true })] : [];
    for (const definition of decision.fields ?? []) {
      const item = fieldFor(definition, { ...context, options: { ...context.options, ...record.options } });
      const value = record.formValues?.[definition[0]] ?? record[definition[0]];
      fillValue(item.control, value);
      fields.push(item);
    }
    fields.push(reason);
    configureFields(fields);
    const canSubmit = operationReady && !fields.some(item => item.control.required && item.control.disabled);
    form.append(facts([['Đối tượng', record.name || record.label || record.title],
      ['Trạng thái hiện tại', record.statusLabel || record.status]]), fieldGrid(fields));
    if (name === 'sessions' && key === 'updateDraft') configureRentalSelector(form, fields, context, api, signal, record);
    finishForm(form, fields, canSubmit ? '' : 'Quyền xác nhận thao tác chưa khả dụng.', label, canSubmit ? async () => {
      const payload = { action: key, ...formPayload(form), expectedState: record.status, expectedVersion: record.version,
        idempotencyKey: requestKey, key: name === 'settings' ? record.id : undefined };
      await write(api, op, endpointPath, method === 'DELETE' ? undefined : payload, method);
      if (!signal.aborted) refreshView();
    } : undefined);
    openDialog(target, trigger, label, form);
  });
  trigger.disabled = !operationReady;
  return trigger;
}

function publicationAction(target, record, api, signal) {
  const op = 'work.events.publish';
  const path = endpoint('manage', 'events', record.id, 'publication');
  const trigger = action('Xem trước và công khai', async () => {
    trigger.disabled = true;
    const body = layout('stack', notice('Đang tải bản xem trước đã được duyệt…'));
    const dialog = openDialog(target, trigger, 'Xem trước và công khai sự kiện', body);
    try {
      const preview = await read(api, op, path, { signal });
      if (signal.aborted || !dialog.isConnected) return;
      if (!preview) { body.replaceChildren(notice('Bản xem trước chưa khả dụng. Đóng hộp thoại và thử lại.')); return; }
      const content = panel(preview.name || 'Nội dung sự kiện', facts([
        ['Danh mục', preview.category], ['Mô tả', preview.description], ['Chương trình', preview.program],
        ['Nghệ sĩ / diễn giả', preview.participants], ['Quy định tham dự', preview.rules],
        ['Điều khoản vé', preview.policy], ['Tự hủy vé', preview.allowRefund ? 'Cho phép theo điều khoản' : 'Không áp dụng']
      ]));
      if (preview.image) {
        const url = new URL(preview.image, location.origin);
        if (['https:', 'http:'].includes(url.protocol)) {
          const image = element('img');
          image.src = url.href;
          image.alt = 'Ảnh bìa sự kiện ' + (preview.name || '');
          image.style.maxWidth = '100%';
          image.style.maxHeight = '240px';
          image.style.objectFit = 'contain';
          content.append(image);
        }
      }
      const schedule = (preview.sessions || []).map(session => panel(session.name || 'Suất diễn',
        facts([['Địa điểm', session.venue], ['Bắt đầu', timeLabel(session.from)], ['Kết thúc', timeLabel(session.to)],
          ['Sức chứa', session.capacity?.toString()],
          ...(session.rentalId ? [['Hồ sơ thuê', '#' + session.rentalId], ['Tổng tiền thuê địa điểm', currency(session.rentalTotal)],
            ['Tiền cọc thuê địa điểm', currency(session.deposit)]] : [['Hồ sơ thuê', 'Không gắn hồ sơ thuê cho suất này']])]),
        element('p', 'Giá bán vé theo suất và khu vực dưới đây là khoản riêng với tiền thuê địa điểm và tiền cọc.', 'field-hint'),
        dataTable('Giá vé · ' + (session.name || 'Suất diễn'), ['Hạng vé', 'Khu vực', 'Giá bán / vé', 'Số lượng', 'Mở bán', 'Đóng bán', 'Quyền lợi'],
          session.prices?.map(price => [price.invitation ? price.name + ' · Vé mời, không bán công khai' : price.name,
            price.zone, currency(price.price), price.quantity?.toString(), timeLabel(price.from), timeLabel(price.to), price.benefits]),
          'Giá vé chưa khả dụng.', 'Suất chưa có loại vé được công bố.')));
      const form = staticForm();
      const reason = field('Lý do / ghi chú quyết định', { name: 'reason', type: 'textarea', required: true, maxLength: 1000 });
      const confirm = field('Tôi đã kiểm tra nội dung, lịch suất và giá bán vé trước khi xác nhận lựa chọn', { name: 'confirm', type: 'checkbox', required: true });
      form.append(fieldGrid([reason, confirm]));
      finishForm(form, [reason, confirm], '', 'Xác nhận công khai', async () => {
        await write(api, op, path, { versionId: preview.versionId, reason: reason.control.value.trim() });
        if (signal.aborted) return;
        dialog.close();
        refreshView();
      });
      if (record.allowedActions?.includes('reopen') && enabled(api, 'work.events.reopen', true)) {
        const feedback = element('p', '', 'field-hint');
        feedback.setAttribute('role', 'status');
        const reopen = action('Đưa về bản nháp để chỉnh sửa', async () => {
          if (![reason, confirm].map(item => item.check()).every(Boolean)) return;
          if (!window.confirm('Đưa sự kiện về bản nháp? Giá vé và lịch suất được giữ lại, nhưng bạn phải gửi xét duyệt lại trước khi công khai.')) return;
          reopen.disabled = true;
          try {
            await write(api, 'work.events.reopen', endpoint('manage', 'events', record.id, 'publication', 'reopen'),
              { versionId: preview.versionId, reason: reason.control.value.trim() });
            if (!signal.aborted) { dialog.close(); refreshView(); }
          } catch (error) { feedback.textContent = error.message; reopen.disabled = false; }
        });
        form.append(element('p', 'Cần sửa nội dung hoặc giá? Đưa về bản nháp sẽ giữ lại lịch suất và giá vé để chỉnh sửa, rồi gửi xét duyệt lại.', 'field-hint'),
          layout('actions', reopen), feedback);
      }
      body.replaceChildren(notice('Đây là nội dung đã được duyệt. Kiểm tra từng suất và giá vé rồi xác nhận để sự kiện xuất hiện công khai.'), content,
        ...schedule, form);
    } catch (error) {
      if (!signal.aborted && dialog.isConnected) body.replaceChildren(notice(error.message));
    } finally { trigger.disabled = false; }
  });
  trigger.disabled = !enabled(api, op, true);
  return trigger;
}

function dataDetail(target, name, context, record, api, signal) {
  const section = sections[name];
  const path = 'work/' + name;
  const title = record?.name || record?.label || record?.title || 'Chi tiết ' + section.title.toLowerCase();
  const quick = record && section.links?.filter(([key]) => allowed(api, key)).map(([key, label]) => routeLink(label,
    hashWith('work/' + key, routeQuery({ eventId: record.id })), 'button secondary')) || [];
  const actions = record ? (section.decisions ?? []).filter(item => record.allowedActions?.includes(item.key))
    .map(item => decisionForm(target, name, context, api, record, signal, item)) : [];
  if (name === 'events' && record?.status === 'APPROVED_PRIVATE' && record.allowedActions?.includes('publish'))
    actions.unshift(publicationAction(target, record, api, signal));
  if (name === 'events' && record && ['APPROVED', 'ACTIVE', 'POSTPONED', 'CANCELLED', 'COMPLETED'].includes(record.status))
    quick.unshift(routeLink('Mở trang sự kiện công khai', recordPath('event', record.id), 'button secondary'));
  const columns = section.columns.map(column => [column[1], record ? previewValue(record, column) : undefined]);
  const extra = (record?.fields ?? []).map(field => [field.label, field.value]);
  const sectionsList = [routeLink('Về danh sách', hashWith(path, routeQuery(context)), 'button secondary')];
  if (record) {
    sectionsList.push(facts([...columns, ['Trạng thái', record.statusLabel || section.states?.find(([key]) => record.status === key)?.[1] || record.status], ...extra]));
    if (record.description) sectionsList.push(element('p', record.description));
    if (name === 'rentals' && record.allowedActions?.includes('reconcileDeposit') && enabled(api, 'requests.rentals.deposit', true)) {
      const feedback = element('p', '', 'field-hint');
      feedback.setAttribute('role', 'status');
      const reconcile = action('Đối chiếu tiền cọc với PayOS', async () => {
        reconcile.disabled = true;
        feedback.textContent = 'Đang đối chiếu giao dịch…';
        try {
          await write(api, 'requests.rentals.deposit', endpoint('venue-rentals', record.id, 'deposit', 'reconciliation'), {});
          if (!signal.aborted) refreshView();
        } catch (error) {
          feedback.textContent = error.message;
          reconcile.disabled = false;
        }
      });
      sectionsList.push(layout('actions', reconcile), feedback);
    }
    if (name === 'guests' && record.allowedActions?.includes('pdf') && enabled(api, 'work.guests.pdf')) {
      const feedback = element('p', '', 'field-hint');
      feedback.setAttribute('role', 'status');
      const download = action('Tải vé mời PDF để gửi khách', async () => {
        download.disabled = true;
        const blob = await read(api, 'work.guests.pdf', endpoint('events', context.eventId, 'invitations', record.id, 'pdf'), { signal, download: true });
        if (signal.aborted) return;
        if (blob?.type === 'application/pdf') downloadBlob(blob, 'qlsk-ve-moi-' + record.id + '.pdf');
        else feedback.textContent = 'Vé không còn khả dụng để tải. Tải lại trang để kiểm tra trạng thái.';
        download.disabled = false;
      });
      sectionsList.push(layout('actions', download), element('p', 'Gửi tệp PDF cho khách qua kênh liên hệ đã khai. Khách có tài khoản đã xác minh liên hệ sẽ thấy vé trong Vé của tôi.', 'field-hint'), feedback);
    }
    if (name === 'guests' && record.allowedActions?.includes('link') && enabled(api, 'work.guests.link', true)) {
      const feedback = element('p', '', 'field-hint');
      feedback.setAttribute('role', 'status');
      const createLink = action('Tạo / thay liên kết nhận vé', async () => {
        createLink.disabled = true;
        try {
          const response = await write(api, 'work.guests.link', endpoint('events', context.eventId, 'invitations', record.id, 'links'), {});
          if (signal.aborted) return;
          const url = new URL('/#' + response.route, location.origin).href;
          const link = field('Liên kết nhận vé mời', { value: url, readOnly: true });
          const copied = element('p', '', 'field-hint');
          copied.setAttribute('role', 'status');
          const copy = action('Sao chép liên kết', async () => {
            try { await navigator.clipboard.writeText(url); copied.textContent = 'Đã sao chép liên kết.'; }
            catch { link.control.select(); copied.textContent = 'Chọn và sao chép liên kết trong ô trên.'; }
          });
          const download = action('Tải lời mời để gửi', () => downloadBlob(new Blob([
            'Lời mời tham dự sự kiện QLSK\nKhách mời: ' + record.name + '\nMở liên kết: ' + url +
            '\nCó hiệu lực đến: ' + timeLabel(response.expiresAt) + '\nĐăng nhập bằng liên hệ đã xác minh để nhận vé.\n'
          ], { type: 'text/plain;charset=utf-8' }), 'qlsk-loi-moi-' + record.id + '.txt'));
          openDialog(target, createLink, 'Liên kết nhận vé mời', layout('stack', link.wrapper,
            element('p', 'Hết hạn: ' + timeLabel(response.expiresAt) + '. Liên kết trước đã hết hiệu lực. Chỉ gửi liên kết cho đúng người nhận; hệ thống chưa gửi email.'),
            layout('actions', copy, download), copied));
        } catch (error) { feedback.textContent = error.message; }
        finally { createLink.disabled = false; }
      });
      sectionsList.push(layout('actions', createLink), element('p', 'Tạo liên kết mới sẽ thay thế liên kết cũ. BTC tự gửi lời mời cho khách; liên kết không tự gửi email.', 'field-hint'), feedback);
    }
    if (name === 'applications' && record.documents) {
      const feedback = element('p', '', 'field-hint');
      feedback.setAttribute('role', 'status');
      sectionsList.push(dataTable('Giấy tờ nộp kèm', ['Tài liệu', 'Thao tác'], record.documents.map(file => [file.label,
        action('Mở tệp được cấp quyền', async () => {
          const blob = await read(api, 'work.applications.document', endpoint('organizer-applications', record.id, 'documents', file.id),
            { signal, download: true });
          if (signal.aborted) return;
          if (blob && ['application/pdf', 'image/png', 'image/jpeg'].includes(blob.type)) downloadBlob(blob, file.name);
          else feedback.textContent = 'Tệp chưa khả dụng.';
        }, { disabled: !enabled(api, 'work.applications.document') })
      ]), restricted, 'Hồ sơ không có tài liệu được trả về.'), feedback);
    }
  } else sectionsList.push(notice(record === null ? 'Bản ghi không khả dụng.' : restricted), facts(columns));
  sectionsList.push(layout('actions', ...actions, ...quick),
    dataTable('Lịch sử xử lý', ['Thời điểm', 'Người thực hiện', 'Nội dung'],
      record?.history?.map(item => [timeLabel(item.at), item.actorName, item.note]),
      'Lịch sử bản ghi chưa khả dụng.', 'Bản ghi chưa có cập nhật.'));
  return panel(title, ...sectionsList);
}

function searchList(name, context, response, query) {
  const section = sections[name];
  const path = 'work/' + name;
  const records = items(response);
  const status = field('Trạng thái', { choices: section.states ?? [], placeholder: 'Mọi trạng thái', value: query.get('status') ?? '' });
  const search = field('Tìm trong trang này', { type: 'search', placeholder: 'Tìm trong trang này' });
  const filter = staticForm();
  filter.classList.add('work-list-tools');
  search.wrapper.classList.add('work-list-search');
  status.wrapper.classList.add('work-list-status');
  const result = layout('stack');
  const count = element('p', '', 'field-hint');
  count.setAttribute('role', 'status');
  const route = routeQuery(context);
  const showStatus = !section.columns.some(([key]) => key === 'status' || key === 'statusLabel') &&
    (section.states || records?.some(record => record.status || record.statusLabel));
  const rowsFor = list => list?.map(record => {
    const columns = section.columns.map(column => previewValue(record, column));
    if (record.id != null && section.detail !== false) columns[0] = routeLink(columns[0] || 'Xem chi tiết',
      hashWith(recordPath(path, record.id), route));
    return showStatus ? [...columns, record.statusLabel || section.states?.find(([key]) => record.status === key)?.[1] || record.status] : columns;
  });
  const headers = [...section.columns.map(([, label]) => label), ...(showStatus ? ['Trạng thái'] : [])];
  function update() {
    const current = records?.filter(item => matches(section.columns.map(column => previewValue(item, column)).join(' '), search.control.value));
    count.textContent = current ? new Intl.NumberFormat('vi-VN').format(current.length) + ' bản ghi trong trang này' : '';
    result.replaceChildren(dataTable(section.title, headers, rowsFor(current), restricted, 'Không có bản ghi phù hợp.'));
  }
  search.control.addEventListener('input', update);
  filter.append(search.wrapper, ...(section.states ? [status.wrapper] : []), count);
  if (section.states) status.control.addEventListener('change', () => {
    const next = new URLSearchParams(route);
    if (status.control.value) next.set('status', status.control.value);
    location.hash = hashWith(path, next);
  });
  update();

  const controls = [];
  for (const [state, label] of section.shortcuts ?? []) {
    const params = new URLSearchParams(route);
    params.set(state === 'overdue' ? 'overdue' : 'status', state === 'overdue' ? 'true' : state);
    controls.push(routeLink(label, hashWith(path, params), 'button secondary'));
  }
  if (query.has('status') || query.has('overdue')) controls.push(routeLink('Tất cả', hashWith(path, route), 'button secondary'));
  const bottom = layout('actions');
  if (query.has('cursor')) {
    const params = new URLSearchParams(query);
    params.delete('cursor');
    bottom.append(routeLink('Trang đầu', hashWith(path, params), 'button secondary'));
  }
  if (response?.nextCursor) {
    const params = new URLSearchParams(query);
    params.set('cursor', response.nextCursor);
    bottom.append(routeLink('Trang sau', hashWith(path, params), 'button secondary'));
  }
  const listing = layout('stack work-list', ...(controls.length ? [layout('actions', ...controls)] : []), filter,
    layout('panel work-list-results', result, bottom));
  return listing;
}

function checkIn(name, context, api, signal) {
  const section = sections[name];
  const code = field(section.checkIn === 'QR' ? 'Mã QR từ đầu đọc' : 'Mã đơn, email hoặc số điện thoại', {
    name: 'code', required: true, maxLength: 240
  });
  const form = staticForm();
  const result = layout('stack');
  result.setAttribute('aria-live', 'polite');
  const path = context.sessionId ? endpoint('sessions', context.sessionId, 'check-ins') : undefined;
  const resource = context.sessionId ? endpoint('sessions', context.sessionId, 'attendees', 'lookup') : undefined;
  const ready = Boolean(path && enabled(api, operation(name, 'confirm'), true));
  const canLookup = Boolean(resource && enabled(api, operation(name, 'lookup'), true));
  form.append(code.wrapper);
  function refreshResult(response) {
    result.replaceChildren(facts([['Kết quả', response.resultLabel], ['Vé', response.ticketLabel],
      ['Check-in lúc', timeLabel(response.checkedInAt)], ['Lý do', response.reason]]));
  }
  if (section.checkIn === 'QR') {
    finishForm(form, [code], ready ? '' : 'Cần chọn suất và có quyền check-in QR.',
      'Xác nhận và quét tiếp', ready ? async () => {
        const response = await write(api, operation(name, 'confirm'), path, { code: code.control.value, method: 'QR' });
        if (signal.aborted) return;
        refreshResult(response);
        form.reset();
        code.control.focus();
      } : undefined);
  } else {
    const supplied = layout('stack');
    const search = action('Tra cứu vé', async () => {
      if (!code.check()) return;
      search.disabled = true;
      supplied.replaceChildren();
      try {
        const records = items(await write(api, operation(name, 'lookup'), resource, { query: code.control.value }));
        if (signal.aborted) return;
        supplied.replaceChildren(dataTable('Đối chiếu người tham dự', ['Người tham dự', 'Vé', 'Ghế', 'Xác nhận'],
          records?.map(record => {
            const confirm = action('Check-in vé này', async () => {
              confirm.disabled = true;
              try {
                const response = await write(api, operation(name, 'confirm'), path, { ticketId: record.ticketId, method: 'MANUAL' });
                if (signal.aborted) return;
                refreshResult(response);
              } catch (error) {
                result.replaceChildren(notice(error.message));
                confirm.disabled = false;
              }
            }, { disabled: !ready || !record.allowedActions?.includes('check_in') });
            return [record.label, record.ticketLabel, record.seat, confirm];
          }), restricted, 'Không tìm thấy người tham dự khớp dữ liệu.'));
      } catch (error) {
        supplied.replaceChildren(notice(error.message));
      } finally {
        search.disabled = !canLookup;
      }
    }, { type: 'submit', style: '', disabled: !canLookup });
    form.addEventListener('submit', event => {
      event.preventDefault();
      if (event.submitter !== search && !search.disabled) search.click();
    });
    search.addEventListener('click', event => event.preventDefault());
    form.append(search, ...(!canLookup ? [notice('Cần quyền tra cứu vé trong suất đã chọn.')] : []), supplied);
  }
  return panel(section.title, element('p', section.description), form, result);
}

async function workOverview(api, context, signal) {
  const dashboard = await read(api, 'work.overview', endpoint('admin', 'dashboard'), { signal });
  const overview = await template('work-overview');
  const shortcuts = [
    ['events', 'Quản lý sự kiện'], ['scan', 'Mở cổng check-in'], ['attendees', 'Tra cứu người tham dự'],
    ['applications', 'Duyệt hồ sơ BTC', { status: 'PENDING' }], ['reviews', 'Duyệt sự kiện'],
    ['rentals', 'Duyệt thuê', { status: 'REQUESTED' }], ['support', 'Xử lý hỗ trợ', { status: 'OPEN' }],
    ['refunds', 'Xét hoàn tiền', { status: 'MANUAL_REVIEW' }], ['settlements', 'Đối soát'],
    ['accounts', 'Phân quyền']
  ].filter(([key]) => allowed(api, key)).map(([key, label, filters]) => {
    const params = sections[key].scope ? routeQuery(context) : new URLSearchParams();
    for (const [name, value] of Object.entries(filters ?? {})) params.set(name, value);
    return routeLink(label, hashWith('work/' + key, params), 'button secondary');
  });
  if (enabled(api, 'reports.preview')) shortcuts.push(routeLink('Mở báo cáo', 'export', 'button secondary'));
  slot(overview, 'shortcuts', ...(shortcuts.length ? shortcuts : [notice(restricted)]));
  const metrics = dashboard?.metrics;
  if (metrics?.length) {
    const list = element('dl', '', 'work-metrics');
    const number = new Intl.NumberFormat('vi-VN');
    for (const item of metrics) {
      list.append(layout('work-metric', element('dt', item.label), element('dd', number.format(item.value))));
    }
    slot(overview, 'queue', list);
  } else {
    slot(overview, 'queue', notice(metrics ? 'Không có số liệu hàng đợi trong phạm vi hiện tại.' : 'Số liệu hàng đợi chưa khả dụng.'));
  }
  const activity = dashboard?.activity;
  slot(overview, 'activity', activity?.length ? layout('work-activity-list', ...activity.map(record =>
    layout('work-activity-row', element('strong', record.label), element('p', record.statusLabel, 'muted'),
      element('small', timeLabel(record.updatedAt), 'muted'))))
    : notice(activity ? 'Chưa có hoạt động được trả về.' : restricted));
  return overview;
}

export async function renderWork(target, segments, query, { view, api, signal }) {
  const [name = 'overview', id] = segments;
  if (segments.length > 2 || (name === 'overview' ? id : !Object.hasOwn(sections, name))) return;
  const section = sections[name];
  const canAccess = !section || allowed(api, name);
  const scopes = canAccess ? await read(api, 'work.scopes', endpoint('manage', 'scopes'), { signal }) : undefined;
  const context = readContext(query, scopes, section);
  const path = 'work/' + name;
  if (canAccess && (section?.scope || section?.scopedQuery)) slot(view, 'scope', scopePicker(path, context, section, !scopes));
  const active = hashWith(path, routeQuery(section?.resources ? context : { eventId: context.eventId, sessionId: context.sessionId }));
  let content;
  if (!section) content = await workOverview(api, context, signal);
  else if (!canAccess) content = panel('Truy cập vận hành', notice('Tài khoản chưa được cấp quyền mở mục này.'),
    routeLink('Về tổng quan', 'work', 'button secondary'));
  else {
    const actions = [];
    if (section.create && id !== 'new' && (!api || enabled(api, operation(name, 'create'), true))) actions.push(routeLink(section.create[0],
      hashWith(path + '/new', routeQuery(context)), 'button'));
    if (section.columns && (!api?.reportTypes || api.reportTypes.includes(name))) actions.push(routeLink('Báo cáo và định dạng xuất', 'export?' + new URLSearchParams({
      type: name, ...Object.fromEntries(routeQuery(context)), ...(query.get('status') ? { status: query.get('status') } : {})
    }), 'button secondary'));
    if (section.resources) actions.push(tabLinks('Loại nội dung', section.resources.map(([key, label]) => [label, path + '?' +
      new URLSearchParams({ resource: key })]), path + '?' + new URLSearchParams({ resource: context.resource })));
    slot(view, 'actions', ...actions);
    if (name === 'scan' || name === 'manual') content = checkIn(name, context, api, signal);
    else if (id === 'new' && section.create) content = name === 'settlements' ? await settlementCreateForm(api, signal) : createForm(name, context, api, signal);
    else if (section.scope && !context[section.scope + 'Id']) content = panel(section.title, notice(scopeNeeded));
    else if (id) {
      const detail = section === sections.settings ? apiPath(section, context) : apiPath(section, context, id);
      const result = await read(api, operation(name, 'detail'), detail, { signal, missingRecord: true,
        query: section === sections.settings ? new URLSearchParams({ key: id }) : undefined });
      content = dataDetail(target, name, context, result, api, signal);
    } else {
      const params = new URLSearchParams({ ...section.query, ...Object.fromEntries(routeQuery(context)) });
      for (const key of ['status', 'overdue', 'cursor']) if (query.has(key)) params.set(key, query.get(key));
      const response = await read(api, operation(name, 'list'), apiPath(section, context), { query: params, signal });
      content = searchList(name, context, response, query);
    }
  }
  slot(view, 'content', content);
  const role = stageLabels[api?.workspace?.role] || api?.workspace?.roleLabel;
  const title = heading(section?.title ?? 'Tổng quan', section?.description);
  title.classList.add('work-heading');
  title.prepend(element('p', 'Vận hành / ' + role, 'work-breadcrumb'));
  target.append(workspace('work', layout('stack', title, view), workspaceGroups(api, context, active), role));
}
