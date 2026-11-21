import { element, layout, notice, routeLink } from './view.js';

export const categories = ['Âm nhạc', 'Nghệ thuật', 'Sân khấu', 'Hội thảo', 'Workshop'];
const categoryImages = { 'Âm nhạc': 'music', 'Nghệ thuật': 'art', 'Sân khấu': 'theatre', 'Hội thảo': 'conference', Workshop: 'workshop' };
export const categoryImage = category => 'assets/images/' + (categoryImages[category] ?? 'hero') + '.webp';
const money = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' });
const dateTime = new Intl.DateTimeFormat('vi-VN', {
  dateStyle: 'medium', timeStyle: 'short', timeZone: 'Asia/Ho_Chi_Minh'
});

export const currency = value => Number.isFinite(value) ? money.format(value) : undefined;
export const timeLabel = value => Number.isFinite(Date.parse(value)) ? dateTime.format(new Date(value)) : undefined;

function fold(value) {
  return String(value ?? '').normalize('NFD').replace(/\p{Diacritic}/gu, '').toLowerCase().replaceAll('đ', 'd');
}

export function matches(value, query) {
  return fold(value).includes(fold(query));
}

export function recordPath(area, ...ids) {
  return [area, ...ids.map(id => encodeURIComponent(id))].join('/');
}

export function photo(source, alt = '') {
  if (!source) {
    const placeholder = element('div', 'QLSK', 'catalog-photo catalog-photo-placeholder');
    placeholder.setAttribute('aria-label', 'Chưa có ảnh công bố');
    return placeholder;
  }
  const image = element('img', '', 'catalog-photo');
  image.src = source;
  image.alt = alt;
  image.loading = 'lazy';
  image.addEventListener('error', () => image.replaceWith(photo()), { once: true });
  return image;
}

export function catalogCard({ title, image, route, label, details, price, illustration }) {
  const body = layout('catalog-card-body stack');
  const name = element('h3');
  name.append(routeLink(title, route));
  if (label) body.append(element('p', label, 'catalog-label'));
  body.append(name);
  for (const detail of details.filter(Boolean)) body.append(element('p', detail, 'muted'));
  if (price) body.append(element('strong', price, 'catalog-price'));
  const card = element('article', '', 'catalog-card');
  const cover = routeLink('', route, 'catalog-card-cover');
  cover.tabIndex = -1;
  cover.setAttribute('aria-hidden', 'true');
  cover.append(photo(image || illustration || categoryImage(label)));
  if (!image) cover.append(element('span', route.startsWith('venue/') ? 'Ảnh minh họa địa điểm' : 'Ảnh minh họa sự kiện', 'catalog-photo-label'));
  card.append(cover, body);
  return card;
}

export function catalogResults(records, path, query, card, unavailable, empty, nextCursor, showCount = true) {
  if (!records) return notice(unavailable);
  if (!records.length && !nextCursor) return notice(empty);
  if (nextCursor || query.has('cursor')) {
    const pagination = element('nav', '', 'pagination actions');
    pagination.setAttribute('aria-label', 'Phân trang kết quả');
    const params = new URLSearchParams(query);
    if (params.has('cursor')) {
      params.delete('cursor');
      pagination.append(routeLink('Trang đầu', path + (params.size ? '?' + params : ''), 'button secondary'));
    }
    if (nextCursor) {
      params.set('cursor', nextCursor);
      pagination.append(routeLink('Trang sau', path + '?' + params, 'button secondary'));
    }
    return layout('stack', layout('catalog-grid', ...records.map(card)), pagination);
  }

  const size = 8;
  const count = Math.ceil(records.length / size);
  const index = Math.min(count, Math.max(1, Number.parseInt(query.get('page'), 10) || 1));
  const result = layout('stack', ...(showCount ? [element('p', records.length + ' kết quả', 'muted')] : []),
    layout('catalog-grid', ...records.slice((index - 1) * size, index * size).map(card)));
  if (count > 1) {
    const pagination = element('nav', '', 'pagination actions');
    pagination.setAttribute('aria-label', 'Phân trang kết quả');
    function link(label, page) {
      const params = new URLSearchParams(query);
      params.set('page', page);
      return routeLink(label, path + '?' + params, 'button secondary');
    }
    if (index > 1) pagination.append(link('Trang trước', index - 1));
    pagination.append(element('span', 'Trang ' + index + ' / ' + count));
    if (index < count) pagination.append(link('Trang sau', index + 1));
    result.append(pagination);
  }
  return result;
}
