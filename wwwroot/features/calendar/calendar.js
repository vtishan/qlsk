import { action, element, layout, notice, routeLink } from '../../shared/ui/view.js';
import { recordPath, timeLabel } from '../../shared/ui/catalog.js';
import { queryForm } from '../../shared/ui/forms.js';
import { slot } from '../../shared/ui/templates.js';
import { items, endpoint, read } from '../../app/api.js';
import { dayOfMonth, monthStart, todayInVietnam } from '../../shared/date.js';
import { downloadSession, eventFilters, shareSession } from '../events/events.js';
import { readFilterOptions } from '../../shared/ui/catalog-filters.js';

const weekdays = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'];
const monthLabel = new Intl.DateTimeFormat('vi-VN', { month: 'long', year: 'numeric', timeZone: 'UTC' });
const dayLabel = new Intl.DateTimeFormat('vi-VN', { dateStyle: 'full', timeZone: 'UTC' });
const clock = new Intl.DateTimeFormat('vi-VN', { hour: '2-digit', minute: '2-digit', timeZone: 'Asia/Ho_Chi_Minh' });
const number = new Intl.NumberFormat('vi-VN');
const key = date => date.toISOString().slice(0, 10);
const monthKey = date => key(date).slice(0, 7);
const localMidnight = date => key(date) + 'T00:00:00+07:00';

function agendaItem(target, session) {
  const event = session.event;
  const route = recordPath('event', event.id, session.id);
  const title = element('h3');
  title.append(routeLink(event.title, route));
  const share = action('Chia sẻ', () => shareSession(target, share, route, 'suất diễn'));
  const time = element('time', '', 'agenda-time');
  if (Number.isFinite(Date.parse(session.start))) {
    time.dateTime = session.start;
    time.textContent = clock.format(new Date(session.start));
  }
  const item = element('li', '', 'session-card agenda-card');
  const now = Date.now();
  const status = Date.parse(session.end) <= now ? 'Đã qua' : Date.parse(session.start) <= now ? 'Đang diễn ra' : 'Sắp diễn ra';
  const badge = element('span', status, 'badge');
  if (status === 'Đang diễn ra') badge.classList.add('green');
  item.append(layout('stack', badge, title,
    element('p', [session.name, session.venue?.name].filter(Boolean).join(' · ')),
    element('p', [timeLabel(session.start), timeLabel(session.end)].filter(Boolean).join(' – '), 'muted'),
    layout('actions', share, action('Tải ICS', () => downloadSession(event, session), { disabled: !session.start || !session.end }))));
  return item;
}

function overlaps(session, date) {
  const start = Date.parse(localMidnight(date));
  return Date.parse(session.start) < start + 86400000 && Date.parse(session.end) > start;
}

export async function renderCalendar(target, segments, query, { view, api, signal }) {
  if (segments.length > 2 || (segments.length && !/^[1-9]\d{3}-(0[1-9]|1[0-2])$/.test(segments[0]))) return;
  const today = todayInVietnam();
  const month = segments.length ? new Date(segments[0] + '-01T00:00:00Z') : monthStart(today);
  const monthPath = 'calendar/' + monthKey(month);
  const next = monthStart(month, 1);
  const monthLength = dayOfMonth(next, 0).getUTCDate();
  const dates = Array.from({ length: monthLength }, (_, index) => dayOfMonth(month, index + 1));
  const selected = dates.find(date => key(date) === segments[1]);
  if (segments.length === 2 && !selected) return;
  const path = selected ? monthPath + '/' + key(selected) : monthPath;
  const cursor = query.get('cursor');
  const filters = new URLSearchParams({ from: localMidnight(month), to: localMidnight(next) });
  for (const [name, apiName] of [['city', 'city'], ['cat', 'category']]) {
    if (query.get(name)) filters.set(apiName, query.get(name));
  }
  const load = filters => read(api, 'catalog.sessions', endpoint('sessions'), { query: filters, signal });
  const [response, filterOptions] = await Promise.all([load(filters), readFilterOptions(api, signal)]);
  const sessions = items(response);
  const fullMonth = sessions && !response.nextCursor;
  let agenda = response;
  if (cursor || (selected && response?.nextCursor)) {
    const params = new URLSearchParams(filters);
    if (selected) {
      params.set('from', localMidnight(selected));
      params.set('to', localMidnight(dayOfMonth(selected, selected.getUTCDate() + 1)));
    }
    if (cursor) params.set('cursor', cursor);
    agenda = await load(params);
  }
  const options = new URLSearchParams(query);
  options.delete('cursor');
  const suffix = options.size ? '?' + options : '';
  const records = items(agenda)?.filter(session => !selected || overlaps(session, selected));
  records?.sort((left, right) => Date.parse(left.start) - Date.parse(right.start) || left.id - right.id);
  const monthTitle = view.querySelector('#calendar-month');
  const agendaTitle = view.querySelector('#calendar-agenda');
  monthTitle.textContent = monthLabel.format(month);
  agendaTitle.textContent = selected ? dayLabel.format(selected) : 'Lịch trong tháng';
  if (segments.length || query.size) (selected || cursor ? agendaTitle : monthTitle).setAttribute('data-route-focus', '');

  const filterForm = queryForm(eventFilters(query, false, filterOptions).reverse(), path, 'Áp dụng bộ lọc');
  filterForm.querySelector('.actions button:last-child').remove();
  slot(view, 'filters', filterForm);
  slot(view, 'months',
    routeLink('Tháng trước', 'calendar/' + monthKey(monthStart(month, -1)) + suffix, 'button secondary'),
    routeLink('Tháng sau', 'calendar/' + monthKey(next) + suffix, 'button secondary'));
  slot(view, 'today', routeLink('Tháng này', 'calendar' + suffix, 'button secondary compact'));

  const cells = weekdays.map(label => element('span', label, 'calendar-weekday'));
  const offset = (month.getUTCDay() + 6) % 7;
  for (let index = 0; index < offset; index++) cells.push(element('span'));
  for (const cell of cells) cell.setAttribute('aria-hidden', 'true');
  for (const date of dates) {
    const active = date === selected;
    const count = fullMonth ? sessions.filter(session => overlaps(session, date)).length : undefined;
    const control = routeLink(String(date.getUTCDate()), monthPath + '/' + key(date) + suffix,
      'calendar-day' + (active ? ' selected' : ''));
    control.setAttribute('aria-controls', 'calendar-agenda');
    control.setAttribute('aria-label', dayLabel.format(date) + (active ? ', đang chọn' : '') +
      (count == null ? '' : ', ' + number.format(count) + ' suất'));
    if (key(date) === key(today)) control.setAttribute('aria-current', 'date');
    if (count) {
      const badge = element('small', number.format(count));
      badge.setAttribute('aria-hidden', 'true');
      control.append(badge);
    }
    cells.push(control);
  }
  slot(view, 'days', ...cells);
  if (selected) slot(view, 'clear', routeLink('Xem cả tháng', monthPath + suffix, 'button secondary'));
  if (records?.length) {
    slot(view, 'count', number.format(records.length) + (agenda.nextCursor || cursor ? ' suất trên trang' : ' suất diễn'));
    const list = element('ol', '', 'agenda-list');
    list.append(...records.map(session => agendaItem(target, session)));
    slot(view, 'sessions', list);
  } else {
    slot(view, 'sessions', notice(records
      ? (selected ? 'Không có suất diễn phù hợp trong ngày này.' : 'Không có suất diễn phù hợp trong khoảng đã chọn.')
      : 'Dữ liệu suất diễn chưa khả dụng.'));
  }
  if (cursor || agenda?.nextCursor) {
    const links = [];
    if (cursor) links.push(routeLink('Trang đầu', path + suffix, 'button secondary'));
    if (agenda?.nextCursor) {
      options.set('cursor', agenda.nextCursor);
      links.push(routeLink('Trang sau', path + '?' + options, 'button secondary'));
    }
    slot(view, 'pagination', ...links);
  }
  target.append(view);
}
