import { endpoint, read } from '../../app/api.js';
import { currency } from './catalog.js';
import { queryField } from './forms.js';

export function readFilterOptions(api, signal) {
  return read(api, 'catalog.eventFilterOptions', endpoint('event-filter-options'), { signal });
}

export function cityChoices(cities, current) {
  return [...new Set([...(cities ?? ['Hà Nội', 'Hồ Chí Minh', 'Đà Nẵng']), current].filter(Boolean))]
    .map(city => [city, city === 'Hồ Chí Minh' ? 'TP. Hồ Chí Minh' : city]);
}

function priceLabel(min, max) {
  if (min === '' || max === '') return (min === '' ? 'Đến ' : 'Từ ') + currency(Number(min || max));
  if (min === max) return Number(min) === 0 ? 'Miễn phí' : currency(Number(min));
  return currency(Number(min)) + ' – ' + currency(Number(max));
}

function bounds(value) {
  const match = /^(\d{1,16}(?:\.\d{1,2})?)?-(\d{1,16}(?:\.\d{1,2})?)?$/.exec(value);
  if (!match || (!match[1] && !match[2]) || (match[1] && match[2] && Number(match[1]) > Number(match[2]))) return;
  return [match[1] ?? '', match[2] ?? ''];
}

export function priceFilter(query, bands) {
  const selected = query.get('price') ?? [query.get('minPrice') ?? '', query.get('maxPrice') ?? ''].join('-');
  const range = bounds(selected);
  const choices = (bands ?? []).map(({ min, max }) => [min + '-' + max, priceLabel(String(min), String(max))]);
  if (range && !choices.some(([value]) => value === selected)) choices.push([selected, priceLabel(...range)]);
  const values = new URLSearchParams(range ? { price: selected } : {});
  return queryField(values, 'price', 'Khoảng giá', { choices, placeholder: 'Mọi mức giá' });
}

export function applyPriceBand(params) {
  if (!params.has('price')) return;
  const range = bounds(params.get('price'));
  params.delete('price');
  params.delete('minPrice');
  params.delete('maxPrice');
  if (range?.[0]) params.set('minPrice', range[0]);
  if (range?.[1]) params.set('maxPrice', range[1]);
}
