import { element, layout, notice, routeLink } from '../../shared/ui/view.js';
import { photo, recordPath } from '../../shared/ui/catalog.js';
import { endpoint, items, read } from '../../app/api.js';
import { eventCard } from '../events/event-card.js';
import { readFilterOptions, priceFilter, applyPriceBand, cityChoices } from '../../shared/ui/catalog-filters.js';

export async function renderHome(target, segments, query, { view, api, signal }) {
  const search = new URLSearchParams({ upcoming: 'true', limit: '4' });
  for (const [name, key] of [['query', 'q'], ['city', 'city'], ['cat', 'category'], ['maxPrice', 'maxPrice']]) {
    if (query.get(name)) search.set(key, query.get(name));
  }
  if (query.has('price')) search.set('price', query.get('price'));
  applyPriceBand(search);
  const [banners, featured, upcoming, categoryList, options] = await Promise.all([
    read(api, 'catalog.banners', endpoint('banners'), { signal }),
    read(api, 'catalog.events', endpoint('events'), { signal, query: new URLSearchParams({ featured: 'true', limit: '4' }) }),
    read(api, 'catalog.events', endpoint('events'), { signal, query: search }),
    read(api, 'catalog.categories', endpoint('event-categories'), { signal }),
    readFilterOptions(api, signal)
  ]);
  const banner = items(banners)?.[0];
  if (banner?.title) {
    const title = view.querySelector('#home-title');
    const [first, second] = banner.title.split('|');
    title.replaceChildren(first);
    if (second) title.append(element('br'), element('span', second));
    if (banner.text) title.nextElementSibling.textContent = banner.text;
  }
  if (banner?.image) {
    const picture = photo(banner.image);
    picture.className = 'hero-banner-image';
    picture.loading = 'eager';
    view.querySelector('.hero').prepend(picture);
  }
  const replaceListing = (selector, response, empty) => {
    const records = items(response);
    if (!records) return;
    const section = view.querySelector(selector);
    section.querySelector('.notice').replaceWith(records.length
      ? layout('catalog-grid', ...records.slice(0, 4).map(row => eventCard(row, api))) : notice(empty));
  };
  const pick = items(featured)?.[0];
  if (pick) {
    const featuredLink = view.querySelector('[data-featured-link]');
    featuredLink.href = '#' + recordPath('event', pick.id);
    featuredLink.querySelector('strong').textContent = pick.title;
    featuredLink.hidden = false;
  }
  const organizerLink = view.querySelector('[data-organizer-link]');
  if (api?.operations.includes('work.overview')) {
    organizerLink.href = '#work';
    organizerLink.textContent = 'Vào khu vực vận hành';
  }
  replaceListing('[aria-labelledby="upcoming-title"]', upcoming, 'Không có sự kiện sắp diễn ra.');
  const upcomingSection = view.querySelector('[aria-labelledby="upcoming-title"]');
  const count = element('p', items(upcoming) ? items(upcoming).length + (upcoming?.nextCursor ? '+ sự kiện phù hợp' : ' sự kiện phù hợp') : 'Chưa tải được danh sách sự kiện.', 'listing-count');
  count.setAttribute('role', 'status');
  upcomingSection.querySelector('.section-heading').after(count);
  const more = new URLSearchParams(query);
  applyPriceBand(more);
  more.set('upcoming', 'on');
  upcomingSection.querySelector('a[href*="upcoming"]').href = '#events?' + more;
  const categories = items(categoryList);
  if (categories) {
    const themes = view.querySelector('.theme-list');
    const covers = { 'Âm nhạc': 'music', 'Nghệ thuật': 'art', 'Sân khấu': 'theatre', 'Hội thảo': 'conference', 'Workshop': 'workshop' };
    const other = categories.map(({ name }) => {
      const item = element('li');
      const link = routeLink('', 'events?' + new URLSearchParams({ cat: name }));
      const descriptions = { 'Âm nhạc': 'Lễ hội, concert và nhạc sống', 'Nghệ thuật': 'Triển lãm và nghệ thuật đương đại', 'Sân khấu': 'Kịch và biểu diễn trực tiếp', 'Hội thảo': 'Gặp gỡ và học hỏi từ diễn giả', 'Workshop': 'Trải nghiệm thực hành' };
      link.append(photo('assets/images/' + (Object.hasOwn(covers, name) ? covers[name] : 'venue') + '.webp'), element('strong', name));
      if (descriptions[name]) link.append(element('small', descriptions[name]));
      item.append(link);
      return item;
    });
    themes.replaceChildren(...other);
    if (!other.length) themes.replaceWith(notice('Chưa có danh mục công khai.'));
  } else view.querySelector('.theme-list').replaceWith(notice('Danh mục sở thích chưa khả dụng.'));
  const form = view.querySelector('#home-search');
  const prices = priceFilter(query, options?.priceBands).control;
  prices.className = 'home-filter';
  prices.setAttribute('aria-label', 'Khoảng giá vé');
  form.elements.price.replaceWith(prices);
  const cities = cityChoices(options?.cities, query.get('city'));
  form.elements.city.replaceChildren(new Option('Mọi thành phố', ''), ...cities.map(([value, label]) => new Option(label, value)));
  const cityLinks = cities.slice(0, 3).map(([value, label]) => {
    const link = element('a', label);
    link.dataset.city = value;
    return link;
  });
  view.querySelector('.quick-cities').replaceChildren(...cityLinks);
  for (const name of ['query', 'city', 'price']) form.elements[name].value = query.get(name) ?? '';
  for (const chip of view.querySelectorAll('[data-city]')) {
    const next = new URLSearchParams(query);
    next.set('city', chip.dataset.city);
    chip.href = '#home/discover?' + next;
    chip.classList.toggle('active', query.get('city') === chip.dataset.city);
    if (query.get('city') === chip.dataset.city) chip.setAttribute('aria-current', 'true');
  }
  if (query.size) view.querySelector('.quick-cities').append(routeLink('Xóa lọc', 'home/discover'));
  form.addEventListener('submit', event => {
    event.preventDefault();
    const filters = new URLSearchParams();
    for (const [name, value] of new FormData(form)) if (value.trim()) filters.set(name, value.trim());
    location.hash = '#home/discover' + (filters.size ? '?' + filters : '');
  });
  target.append(view);
}
