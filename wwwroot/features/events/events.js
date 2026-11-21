import { action, dataTable, element, facts, heading, layout, notice, openDialog, routeLink } from '../../shared/ui/view.js';
import { field, fieldGrid, finishForm, formValues, queryField, queryForm, staticForm } from '../../shared/ui/forms.js';
import { slot } from '../../shared/ui/templates.js';
import { enabled, endpoint, items, read, refreshView, write } from '../../app/api.js';
import { catalogResults, categories, categoryImage, currency, photo, recordPath, timeLabel } from '../../shared/ui/catalog.js';
import { applyPriceBand, cityChoices, priceFilter, readFilterOptions } from '../../shared/ui/catalog-filters.js';

import { eventCard } from './event-card.js';

const unavailable = 'Danh mục sự kiện công khai chưa khả dụng.';
const sessionUnavailable = 'Dữ liệu suất diễn chưa khả dụng.';

export function eventFilters(query, includeKeyword = true, options) {
  const fields = [
    queryField(query, 'query', 'Từ khóa', { type: 'search', maxLength: 120, placeholder: 'Tên, mô tả hoặc địa điểm' }),
    queryField(query, 'cat', 'Danh mục', {
      choices: [...new Set([...(options?.categories ?? categories), query.get('cat')].filter(Boolean))], placeholder: 'Mọi danh mục'
    }),
    queryField(query, 'city', 'Thành phố', { choices: cityChoices(options?.cities, query.get('city')), placeholder: 'Mọi thành phố' })
  ];
  return includeKeyword ? fields : fields.slice(1);
}

export async function renderEvents(target, segments, query, { view, api, signal }) {
  if (segments.length) return;
  const params = new URLSearchParams(query);
  applyPriceBand(params);
  for (const [key, name] of [['query', 'q'], ['cat', 'category'], ['stock', 'availability']]) {
    if (params.has(key)) {
      params.set(name, key === 'stock' ? 'available' : params.get(key));
      params.delete(key);
    }
  }
  if (params.get('upcoming') === 'on') params.set('upcoming', 'true');
  params.delete('page');
  const [options, response] = await Promise.all([
    readFilterOptions(api, signal),
    read(api, 'catalog.events', endpoint('events'), { query: params, signal })
  ]);
  const primary = [...eventFilters(query, true, options), priceFilter(query, options?.priceBands)];
  const venues = (options?.venues ?? []).map(venue => [String(venue.id), venue.name]);
  const selectedVenue = query.get('venueId');
  if (selectedVenue && !venues.some(([id]) => id === selectedVenue)) venues.push([selectedVenue, 'Địa điểm #' + selectedVenue]);
  const extra = [
    queryField(query, 'venueId', 'Địa điểm', { choices: venues, placeholder: 'Mọi địa điểm' }),
    queryField(query, 'date', 'Ngày diễn', { type: 'date' }),
    queryField(query, 'session', 'Suất diễn', { type: 'search' }),
    queryField(query, 'organizer', 'Ban tổ chức', { type: 'search' }),
    queryField(query, 'stock', 'Chỉ xem suất còn vé', { type: 'checkbox' }),
    queryField(query, 'upcoming', 'Chỉ xem sự kiện sắp diễn ra', { type: 'checkbox' })
  ];
  const advanced = element('details', '', 'filter-more');
  if (query.has('venue')) extra.push(queryField(query, 'venue', 'Tên địa điểm', { type: 'search' }));
  advanced.open = extra.some(item => query.has(item.control.name));
  advanced.append(element('summary', 'Lọc thêm'), layout('form-grid', ...extra.map(item => item.wrapper)));
  const form = queryForm(primary, 'events', 'Lọc sự kiện');
  form.classList.add('public-filters', 'event-filters');
  const actions = form.lastElementChild;
  const clear = actions.lastElementChild;
  const active = [...query.keys()].some(key => key !== 'cursor' && key !== 'page');
  if (!active) clear.hidden = true;
  form.append(advanced, clear);
  const resultHeading = view.querySelector('[data-filter-results]');
  if (query.size) resultHeading.setAttribute('data-route-focus', '');
  const records = items(response);
  if (records) resultHeading.textContent = records.length +
    (response.nextCursor || query.has('cursor') ? ' sự kiện trên trang này' : ' sự kiện');
  slot(view, 'filters', form);
  slot(view, 'results', catalogResults(records, 'events', query, row => eventCard(row, api), unavailable,
    'Không có sự kiện phù hợp.', response?.nextCursor, false));
  const title = heading('Khám phá sự kiện', 'Lọc theo chủ đề, địa điểm, lịch diễn và giá vé.');
  title.append(routeLink('Xem lịch theo tháng', 'calendar', 'button secondary compact'));
  title.classList.add('heading-with-action');
  target.append(title, view);
}

export function shareSession(target, trigger, route, label) {
  const url = new URL(location.href);
  url.hash = route;
  const link = field('Liên kết ' + label, { type: 'url', value: url.href, readOnly: true });
  const status = element('p', '', 'field-hint');
  status.setAttribute('role', 'status');
  const copy = action('Sao chép liên kết', async () => {
    try {
      await navigator.clipboard.writeText(url.href);
      status.textContent = 'Đã sao chép liên kết.';
    } catch {
      link.control.focus();
      link.control.select();
      status.textContent = 'Liên kết đã được chọn. Dùng lệnh sao chép của thiết bị.';
    }
  });
  openDialog(target, trigger, 'Chia sẻ ' + label, layout('stack', link.wrapper, copy, status));
}

export function downloadSession(event, session) {
  const icsText = value => String(value ?? '').replaceAll('\\', '\\\\').replace(/\r\n|\r|\n/g, '\\n')
    .replaceAll(';', '\\;').replaceAll(',', '\\,');
  const stamp = value => new Date(value).toISOString().replace(/[-:]|\.\d{3}/g, '');
  const url = new URL(location.href);
  url.hash = recordPath('event', event.id, session.id);
  const lines = [
    'BEGIN:VCALENDAR', 'VERSION:2.0', 'PRODID:-//QLSK//Lich su kien//VI', 'BEGIN:VEVENT',
    'UID:' + encodeURIComponent(event.id) + '/' + encodeURIComponent(session.id) + '@' + location.host,
    'DTSTAMP:' + stamp(Date.now()), 'DTSTART:' + stamp(session.start), 'DTEND:' + stamp(session.end),
    'SUMMARY:' + icsText(event.title + ' – ' + session.name), 'LOCATION:' + icsText(session.venue?.name),
    'URL:' + url.href, 'END:VEVENT', 'END:VCALENDAR'
  ];
  const encoder = new TextEncoder();
  const folded = lines.map(line => {
    let output = '';
    let bytes = 0;
    for (const char of line) {
      const length = encoder.encode(char).length;
      if (bytes + length > 75) {
        output += '\r\n ';
        bytes = 1;
      }
      output += char;
      bytes += length;
    }
    return output;
  });
  const objectUrl = URL.createObjectURL(new Blob([folded.join('\r\n') + '\r\n'], { type: 'text/calendar' }));
  const link = element('a');
  link.href = objectUrl;
  link.download = 'qlsk-suat-dien.ics';
  link.click();
  setTimeout(() => URL.revokeObjectURL(objectUrl), 1000);
}

function sessionDetail(target, event, session, selected) {
  const venue = session.venue;
  const route = recordPath('event', event.id, session.id);
  const name = element('h3', session.name);
  if (String(session.id) === selected) {
    name.tabIndex = -1;
    name.setAttribute('data-route-focus', '');
  }
  const share = action('Chia sẻ suất diễn', () => shareSession(target, share, route, 'suất diễn'));
  return layout('session-card stack', name, facts([
    ['Bắt đầu', timeLabel(session.start)], ['Kết thúc', timeLabel(session.end)],
    ['Địa điểm', venue?.id ? routeLink(venue.name, recordPath('venue', venue.id)) : venue?.name],
    ['Địa chỉ', venue?.address]
  ]), dataTable('Giá vé và thời gian bán', ['Loại vé', 'Giá vé', 'Mở bán', 'Đóng bán'],
    session.prices?.map(price => [price.name, currency(price.amount), timeLabel(price.from), timeLabel(price.to)]),
    'Giá vé chưa khả dụng.', 'Suất diễn chưa công bố loại vé.'),
  layout('actions', ...(session.bookable ? [routeLink('Chọn khu vực và ghế',
    recordPath('tickets/book/seats', event.id, session.id), 'button')] : []), share, action('Tải ICS', () => downloadSession(event, session), { disabled: !session.start || !session.end })));
}

export async function renderEvent(target, segments, query, { view, api, signal }) {
  if (segments.length > 2) return;
  const [id] = segments;
  const [event, reviews] = id ? await Promise.all([
    read(api, 'catalog.event', endpoint('events', id), { signal, missingRecord: true }),
    read(api, 'catalog.reviews', endpoint('events', id, 'reviews'), { signal })
  ]) : [];
  const sessions = event?.sessions;
  const list = sessions?.length ? sessions.map(session => sessionDetail(target, event, session, segments[1]))
    : [notice(sessions ? 'Sự kiện chưa công bố suất diễn.' : sessionUnavailable),
      layout('actions', action('Chia sẻ suất diễn', null, { disabled: true }), action('Tải ICS', null, { disabled: true }))];
  const about = event ? [
    ...(event.description ? [element('p', event.description, 'event-prose')] : []),
    ...(event.program ? [element('h3', 'Chương trình'), element('p', event.program, 'event-prose')] : []),
    facts([['Ban tổ chức', event.organizer], ['Danh mục', event.category], ['Nghệ sĩ / diễn giả', event.participants]])
  ] : [notice(event === null ? 'Không tìm thấy sự kiện công khai.' : unavailable)];
  const policy = [event?.rules, event?.policy].filter(Boolean).map(text => element('p', text, 'event-prose'));
  const feedback = view.querySelector('[data-slot="feedback"]');
  const favorite = action(event?.favorite ? 'Bỏ yêu thích' : 'Lưu yêu thích', async () => {
    favorite.disabled = true;
    const operation = event.favorite ? 'account.removeFavorite' : 'account.addFavorite';
    try {
      await write(api, operation, endpoint('me', 'favorites', event.id), undefined, event.favorite ? 'DELETE' : 'PUT');
      if (!signal.aborted) refreshView();
    } catch (error) {
      feedback.textContent = error.message;
      favorite.disabled = false;
    }
  }, { disabled: !event || !enabled(api, event.favorite ? 'account.removeFavorite' : 'account.addFavorite', true) });
  const share = action('Chia sẻ sự kiện', () => shareSession(target, share, recordPath('event', event.id), 'sự kiện'), { disabled: !event });
  const booking = event?.sessions?.some(session => session.bookable)
    ? [routeLink('Chọn suất để đặt vé', recordPath('tickets/book/sessions', event.id), 'button lime')] : [];
  if (event?.image) view.querySelector('[data-slot="image"]').replaceWith(photo(event.image));
  else if (event) view.querySelector('[data-slot="image"]').replaceWith(photo(categoryImage(event.category), 'Ảnh minh họa theo chủ đề sự kiện'));
  if (event) {
    view.querySelector('#event-title').textContent = event.title;
    slot(view, 'meta', [event.category, event.organizer].filter(Boolean).join(' · '));
    if (favorite.disabled && !api?.authenticated) about.push(routeLink('Đăng nhập để lưu yêu thích', 'account/login?' +
      new URLSearchParams({ next: recordPath('event', event.id) })));
  }
  slot(view, 'actions', ...booking, share, favorite);
  slot(view, 'sessions', ...list);
  slot(view, 'about', ...about);
  slot(view, 'policy', ...(policy.length ? policy : [notice('Chính sách theo sự kiện chưa khả dụng.')]));
  const review = layout('stack', dataTable('Đánh giá công khai', ['Điểm', 'Nội dung'],
    items(reviews)?.map(review => [String(review.rating), review.comment]),
    'Đánh giá chưa khả dụng.', 'Chưa có đánh giá đã được công bố.'));
  if (event?.allowedActions?.includes('review')) {
    const form = staticForm();
    const rating = field('Điểm đánh giá', { name: 'rating', type: 'number', min: '1', max: '5', step: '1', required: true });
    const comment = field('Nhận xét', { name: 'comment', type: 'textarea', required: true, maxLength: 800 });
    const ready = enabled(api, 'catalog.review', true);
    form.append(fieldGrid([rating, comment]));
    finishForm(form, [rating, comment], ready ? '' : 'Dịch vụ nhận đánh giá chưa khả dụng.', 'Gửi đánh giá', ready ? async () => {
      const data = formValues(form);
      await write(api, 'catalog.review', endpoint('events', event.id, 'reviews'), { ...data, rating: Number(data.rating) });
      if (!signal.aborted) refreshView();
    } : undefined);
    review.append(form);
  }
  slot(view, 'reviews', review);
  if (event && !signal.aborted && enabled(api, 'account.recordView', true)) {
    try {
      await write(api, 'account.recordView', endpoint('me', 'view-history', event.id), undefined, 'PUT');
    } catch (error) {
      feedback.textContent = 'Chưa lưu được sự kiện vào lịch sử xem. ' + error.message;
    }
  }
  target.append(view);
}
