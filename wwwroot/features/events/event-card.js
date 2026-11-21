import { action, element } from '../../shared/ui/view.js';
import { catalogCard, currency, recordPath, timeLabel } from '../../shared/ui/catalog.js';
import { endpoint, write } from '../../app/api.js';

export function eventCard(event, api) {
  const session = event.sessions?.[0];
  const from = currency(event.minPrice);
  const card = catalogCard({
    title: event.title, image: event.image, route: recordPath('event', event.id), label: event.category,
    details: [timeLabel(event.nextStart ?? session?.start), event.venueName ?? session?.venue?.name], price: event.minPrice === 0 ? 'Miễn phí' : from ? 'Từ ' + from : 'Xem sự kiện'
  });
  const favorite = action('', async () => {
    if (!api?.authenticated) {
      location.hash = 'account/login?' + new URLSearchParams({ next: recordPath('event', event.id) });
      return;
    }
    favorite.disabled = true;
    const saved = favorite.getAttribute('aria-pressed') === 'true';
    try {
      await write(api, saved ? 'account.removeFavorite' : 'account.addFavorite', endpoint('me','favorites',event.id), undefined, saved ? 'DELETE' : 'PUT');
      label(!saved);
      feedback.textContent = '';
    } catch (error) { feedback.textContent = error.message; }
    finally { favorite.disabled = false; }
  });
  function label(saved) {
    favorite.textContent = saved ? '♥' : '♡';
    favorite.setAttribute('aria-label', (saved ? 'Bỏ yêu thích ' : 'Lưu yêu thích ') + event.title);
    favorite.setAttribute('aria-pressed', String(saved));
  }
  const feedback = element('p', '', 'field-error');
  feedback.setAttribute('role', 'status');
  favorite.className = 'favorite';
  label(event.favorite);
  card.append(favorite, feedback);
  return card;
}
