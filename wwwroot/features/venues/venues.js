import { action, dataTable, element, heading, layout, notice, routeLink } from '../../shared/ui/view.js';
import { catalogCard, currency, photo, recordPath, catalogResults, timeLabel } from '../../shared/ui/catalog.js';
import { queryField, queryForm } from '../../shared/ui/forms.js';
import { slot } from '../../shared/ui/templates.js';
import { endpoint, items, read } from '../../app/api.js';
import { dayOfMonth, todayInVietnam } from '../../shared/date.js';
import { cityChoices, readFilterOptions } from '../../shared/ui/catalog-filters.js';

const unavailable = 'Danh mục địa điểm chưa khả dụng.';
const number = new Intl.NumberFormat('vi-VN');
const dayLabel = new Intl.DateTimeFormat('vi-VN', { weekday: 'short', day: 'numeric', month: 'numeric', timeZone: 'UTC' });
const dateKey = date => date.toISOString().slice(0, 10);
const midnight = date => date + 'T00:00:00+07:00';
const venuePhotos = [
  ['cung van hoa huu nghi', 'cung-van-hoa-huu-nghi', 'thalling55', 'by/2.0', 'Cung_văn_hóa_Hữu_nghị_Việt-Xô.jpg'],
  ['cong vien bien dong', 'cong-vien-bien-dong', 'Leanh2001', 'by-sa/4.0', 'Công_viên_Biển_Đông_Đà_Nẵng.jpg'],
  ['trung tam hoi nghi quoc gia', 'trung-tam-hoi-nghi-quoc-gia', 'Nguyen QuocTrung', 'by-sa/4.0', 'Outside_Vietnam_National_Convention_Center.jpg'],
  ['nha thi dau quan khu 7', 'nha-thi-dau-quan-khu-7', 'Magicknight94', null, 'Khan_gia_xep_hang_cho_Nick_Vujicic_tai_QK7.jpg']
];
function venuePhoto(venue) {
  if (venue?.image) return { image: venue.image };
  const name = venue?.name?.normalize('NFD').replace(/\p{Diacritic}/gu, '').toLowerCase().replaceAll('đ', 'd');
  const known = venuePhotos.find(([key]) => key === name);
  if (known) return { image: 'assets/images/' + known[1] + '.webp', credit: known };
  const illustration = [
    [/nha hat/, 'theatre'], [/hoi nghi/, 'conference'], [/factory/, 'art'], [/indika/, 'dj'],
    [/studio/, 'dance'], [/cong vien/, 'music'], [/nha thi dau/, 'concert']
  ].find(([pattern]) => pattern.test(name || ''))?.[1] || 'venue';
  return { illustration: 'assets/images/' + illustration + '.webp' };
}
const shiftDay = (date, offset) => dayOfMonth(date, date.getUTCDate() + offset);

function selectDay(value) {
  if (!/^[1-9]\d{3}-\d{2}-\d{2}$/.test(value)) return undefined;
  const date = new Date(value + 'T00:00:00Z');
  if (Number.isFinite(date.getTime()) && date.getUTCFullYear() < 9999 && dateKey(date) === value) return date;
}

function forDay(ranges, date) {
  const start = Date.parse(midnight(dateKey(date)));
  const end = start + 86400000;
  return ranges?.filter(range => Date.parse(range.from) < end && Date.parse(range.to) > start);
}

function venueCard(venue, date) {
  const path = recordPath('venue', venue.id);
  return catalogCard({
    title: venue.name, ...venuePhoto(venue), route: date ? path + '?' + new URLSearchParams({ date }) : path, label: venue.city,
    details: [venue.address, venue.capacity != null && venue.capacity + ' khách', venue.amenities],
    price: Number.isFinite(venue.rate) ? currency(venue.rate) + ' / khung' : ''
  });
}

export async function renderVenues(target, segments, query, { view, api, signal }) {
  if (segments.length) return;
  const params = new URLSearchParams(query);
  params.delete('page');
  const [response, options] = await Promise.all([
    read(api, 'catalog.venues', endpoint('venues'), { query: params, signal }),
    readFilterOptions(api, signal)
  ]);
  const rates = [...new Set((options?.venues ?? items(response) ?? []).map(venue => venue.rate).filter(Number.isFinite))];
  // Existing bookmarked filters remain selectable even when catalog options change.
  const currentPrice = query.get('maxPrice');
  if (currentPrice && Number.isFinite(Number(currentPrice)) && Number(currentPrice) >= 0 && !rates.includes(Number(currentPrice)))
    rates.push(Number(currentPrice));
  rates.sort((a, b) => a - b);
  const fields = [
    queryField(query, 'q', 'Tên hoặc địa chỉ', { type: 'search' }),
    queryField(query, 'city', 'Thành phố', { choices: cityChoices(options?.cities, query.get('city')), placeholder: 'Tất cả' }),
    queryField(query, 'minCapacity', 'Sức chứa tối thiểu', { type: 'number', min: '1' }),
    queryField(query, 'maxPrice', 'Giá thuê tối đa', {
      choices: [['', 'Mọi mức giá'], ...rates.map(rate => [String(rate), currency(rate)])]
    }),
    queryField(query, 'amenities', 'Tiện ích', { type: 'search' }),
    queryField(query, 'date', 'Ngày cần thuê', { type: 'date' })
  ];
  const filters = queryForm(fields.slice(0, 4), 'venues', 'Tìm địa điểm');
  filters.classList.add('public-filters');
  const extra = element('details', '', 'filter-more');
  extra.open = fields.slice(4).some(item => query.has(item.control.name));
  extra.append(element('summary', 'Lọc thêm'), layout('form-grid', ...fields.slice(4).map(item => item.wrapper)));
  filters.insertBefore(extra, filters.lastChild);
  slot(view, 'filters', filters);
  const results = view.querySelector('[data-filter-results]');
  if (query.size) results.setAttribute('data-route-focus', '');
  const date = selectDay(query.get('date'));
  slot(view, 'results', catalogResults(items(response), 'venues', query, venue => venueCard(venue, date && dateKey(date)), unavailable,
    'Không có địa điểm phù hợp.', response?.nextCursor));
  target.append(heading('Địa điểm tổ chức', 'Tìm không gian theo sức chứa, lịch trống và tiện ích.'), view);
}

export async function renderVenue(target, segments, query, { view, api, signal }) {
  if (segments.length > 1) return;
  const [id] = segments;
  const today = todayInVietnam();
  const selected = selectDay(query.get('date'));
  const weekStart = selected && (selected < today || selected >= shiftDay(today, 7)) ? selected : today;
  const path = id ? recordPath('venue', id) : 'venue';
  const [venue, availability, events, policies] = id ? await Promise.all([
    read(api, 'catalog.venue', endpoint('venues', id), { signal, missingRecord: true }),
    read(api, 'catalog.venueAvailability', endpoint('venues', id, 'availability'), { signal,
      query: new URLSearchParams({ from: midnight(dateKey(weekStart)), to: midnight(dateKey(shiftDay(weekStart, 7))) }) }),
    read(api, 'catalog.events', endpoint('events'), { signal, query: new URLSearchParams({ venueId: id, upcoming: 'true' }) }),
    read(api, 'catalog.policies', endpoint('policies'), { signal, query: new URLSearchParams({ type: 'venue-rental' }) })
  ]) : [];

  const venuePicture = venuePhoto(venue);
  const figure = photo(venuePicture.image || venuePicture.illustration, venuePicture.image ? venue?.name : '');
  figure.loading = 'eager';
  const caption = element('figcaption');
  if (venuePicture.credit) {
    const [, , author, license, source] = venuePicture.credit;
    const credit = element('a', author);
    credit.href = 'https://commons.wikimedia.org/wiki/File:' + encodeURIComponent(source);
    caption.append('Ảnh ', credit, ' · ');
    if (license) {
      const link = element('a', 'CC ' + license.replace('/', ' ').toUpperCase());
      link.href = 'https://creativecommons.org/licenses/' + license + '/';
      caption.append(link);
    } else caption.append('Phạm vi công cộng');
    caption.append(' · Đã thu nhỏ và chuyển WebP');
  } else if (!venuePicture.image) caption.textContent = 'Ảnh minh họa';
  slot(view, 'photo', figure, ...(caption.childNodes.length ? [caption] : []));
  const title = view.querySelector('#venue-title');
  const scheduleTitle = view.querySelector('#venue-availability');
  const rentalActions = [routeLink('Theo dõi yêu cầu', 'requests/rentals', 'button secondary')];
  const policy = items(policies)?.[0];
  if (policy?.content) {
    const disclosure = element('details', '', 'venue-policy');
    disclosure.append(element('summary', policy.title || 'Điều khoản thuê hiện hành'), element('p', policy.content));
    rentalActions.push(disclosure);
  }
  const coverActions = [action('Xem lịch địa điểm', () => scheduleTitle.focus())];
  if (venue) {
    title.textContent = venue.name;
    slot(view, 'address', venue.address || 'Địa chỉ chưa công bố');
    if (venue.address) {
      const directions = element('a', 'Chỉ đường', 'button secondary');
      directions.href = 'https://www.google.com/maps/dir/?' + new URLSearchParams({ api: '1', destination: venue.name + ', ' + venue.address });
      directions.target = '_blank';
      directions.rel = 'noopener noreferrer';
      coverActions.push(directions);
    }
    const stats = [
      ['Sức chứa', venue.capacity == null ? 'Chưa công bố' : number.format(venue.capacity) + ' khách'],
      ['Giá thuê tham khảo', currency(venue.rate) ?? 'Chưa công bố'],
      ['Tiện ích', venue.amenities || 'Chưa công bố']
    ];
    slot(view, 'highlights', ...stats.map(([label, value]) => {
      const stat = element('div', '', 'panel');
      stat.append(element('dt', label), element('dd', value));
      return stat;
    }));
    const rentPath = 'requests/rentals/new?' + new URLSearchParams({
      venueId: venue.id, ...(selected && selected >= today ? { date: dateKey(selected) } : {})
    });
    const rent = api?.authenticated ? rentPath : 'account/login?' + new URLSearchParams({ next: rentPath });
    coverActions.unshift(routeLink('Yêu cầu thuê', rent, 'button lime'));
    rentalActions.unshift(routeLink('Mở biểu mẫu thuê', rent, 'button'));
  } else {
    slot(view, 'status', notice(venue === null ? 'Không tìm thấy địa điểm công khai.' : unavailable));
  }
  slot(view, 'cover-actions', ...coverActions);
  slot(view, 'rental-actions', ...rentalActions);
  slot(view, 'zones', dataTable('Khu vực được công bố', ['Khu vực', 'Sức chứa', 'Loại chỗ'],
    venue?.zones?.map(zone => [zone.name, zone.capacity?.toString(), zone.seating]), unavailable, 'Địa điểm chưa công bố khu vực.'));

  if (selected) {
    scheduleTitle.textContent = 'Lịch ' + dayLabel.format(selected);
    scheduleTitle.setAttribute('data-route-focus', '');
  }
  const datePicker = element('details', '', 'venue-date-picker');
  datePicker.open = Boolean(selected);
  datePicker.append(element('summary', 'Kiểm tra khung giờ khác', 'button secondary'),
    queryForm([queryField(query, 'date', 'Ngày muốn kiểm tra', { type: 'date', max: '9998-12-31' })], path, 'Kiểm tra lịch'));
  slot(view, 'date', datePicker);
  const ranges = items(availability);
  if (venue) slot(view, 'days', ...Array.from({ length: 7 }, (_, index) => {
    const date = shiftDay(weekStart, index);
    const day = routeLink(dayLabel.format(date), path + '?' + new URLSearchParams({ date: dateKey(date) }), 'venue-day');
    if (date.getTime() === selected?.getTime()) day.setAttribute('aria-current', 'date');
    const busy = forDay(ranges, date);
    if (busy) day.append(element('small', busy.length ? number.format(busy.length) + ' khung bận' : 'Chưa có khung bận'));
    const cell = element('li');
    cell.append(day);
    return cell;
  }));
  const shown = selected ? forDay(ranges, selected) : ranges;
  const busyDetails = element('details', '', 'venue-busy-details');
  busyDetails.open = Boolean(selected);
  busyDetails.append(element('summary', 'Chi tiết khung giờ đã bận'), dataTable('Khung giờ đã bận', ['Bắt đầu', 'Kết thúc', 'Tình trạng'],
    shown?.map(range => [timeLabel(range.from), timeLabel(range.to), range.status ?? 'Bận']),
    'Lịch địa điểm chưa khả dụng.', 'Chưa có khung bận trong khoảng đã chọn. Lịch được xác nhận khi xét yêu cầu thuê.'));
  slot(view, 'availability', busyDetails);
  const upcoming = items(events);
  if (upcoming?.length) {
    const cards = upcoming.slice(0, 4).map(event => catalogCard({
      title: event.title, image: event.image, route: recordPath('event', event.id), label: event.category,
      details: [timeLabel(event.nextStart)], price: currency(event.minPrice)
    }));
    const more = routeLink('Xem thêm sự kiện tại đây', 'events?' + new URLSearchParams({ venueId: id, upcoming: 'on' }), 'button secondary');
    slot(view, 'events', layout('stack', layout('catalog-grid', ...cards),
      ...(upcoming.length > cards.length || events.nextCursor ? [layout('actions', more)] : [])));
  } else {
    slot(view, 'events', notice(upcoming ? 'Không có sự kiện sắp diễn ra tại đây.' : 'Danh sách sự kiện tại địa điểm chưa khả dụng.'));
  }
  target.append(view);
}
