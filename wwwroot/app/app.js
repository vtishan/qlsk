import { action, heading, layout, notice, panel, routeLink } from '../shared/ui/view.js';
import { template } from '../shared/ui/templates.js';
import { apiCapabilities, endpoint, readFailure, refreshView, resetCapabilities, write } from './api.js';
import { renderHome } from '../features/home/home.js';
import { renderEvents, renderEvent } from '../features/events/events.js';
import { renderCalendar } from '../features/calendar/calendar.js';
import { renderGuide } from '../features/guide/guide.js';
import { renderVenues, renderVenue } from '../features/venues/venues.js';
import { renderAccount } from '../features/account/account.js';
import { renderOrganizer } from '../features/organizer/organizer.js';
import { renderRequests } from '../features/requests/requests.js';
import { renderTickets } from '../features/tickets/tickets.js';
import { renderWork } from '../features/work/work.js';
import { renderExport } from '../features/reports/reports.js';

const main = document.querySelector('#main-content');
const home = document.querySelector('#home-view');
const page = document.querySelector('#page-view');
const navLinks = document.querySelectorAll('.primary-nav a, .header-account');
const header = document.querySelector('.site-header');
const signIn = header.querySelector('.header-account');
const accountMenu = header.querySelector('.nav-account');
const tickets = header.querySelector('.header-tickets');
const logout = header.querySelector('.header-logout');
const renders = {
  home: renderHome, calendar: renderCalendar, guide: renderGuide, events: renderEvents, event: renderEvent,
  venues: renderVenues, venue: renderVenue, account: renderAccount, organizer: renderOrganizer,
  requests: renderRequests, tickets: renderTickets, work: renderWork, export: renderExport
};
let dispose;
let active;
let firstRender = true;
let lastRoute;
let authLayer;
let authTrigger;

function dismissLayer() {
  if (!authLayer) return;
  authLayer.controller.abort();
  authLayer.dialog?.close();
  authLayer.dialog?.remove();
  authLayer = undefined;
}

function updateHeader(api, section) {
  const signedIn = Boolean(api?.authenticated);
  signIn.hidden = signedIn;
  accountMenu.hidden = !signedIn;
  accountMenu.open = false;
  tickets.hidden = !signedIn;
  const work = api?.operations.includes('work.overview');
  header.querySelector('.header-work').hidden = !work;
  header.querySelector('.header-name').textContent = api?.displayName || api?.workspace?.roleLabel || 'Tài khoản';
  const notifications = header.querySelector('.header-notifications');
  notifications.hidden = !signedIn;
  const count = notifications.querySelector('.nav-count');
  count.textContent = api?.unreadNotifications || '';
  count.hidden = !api?.unreadNotifications;
  notifications.setAttribute('aria-label', count.hidden ? 'Thông báo' : 'Thông báo, ' + count.textContent + ' chưa đọc');
  const brandAction = document.querySelector('.footer-brand .button');
  brandAction.href = work ? '#work' : '#organizer';
  brandAction.textContent = work ? 'Vào khu vực vận hành' : 'Đăng ký Ban tổ chức';
  const here = location.hash.slice(1) || 'home';
  signIn.href = here === 'account' || here.startsWith('account/') ? '#account/login' :
    '#account/login?' + new URLSearchParams({ next: here });
  logout.hidden = !api?.operations.includes('auth.logout') || !api.csrfToken;
  const feedback = accountMenu.querySelector('[role="status"]');
  feedback.textContent = '';
  logout.onclick = async () => {
    logout.disabled = true;
    try {
      await write(api, 'auth.logout', endpoint('auth', 'session'), undefined, 'DELETE');
      if (location.hash === '#account') refreshView();
      else {
        resetCapabilities();
        location.hash = 'account';
      }
    } catch (error) {
      feedback.textContent = error.message;
    } finally {
      logout.disabled = false;
    }
  };
  for (const link of navLinks) {
    if (link.hash === '#' + section || (link === signIn && section === 'account')) link.setAttribute('aria-current', 'page');
    else link.removeAttribute('aria-current');
  }
}

function parseRoute() {
  const raw = location.hash.slice(1);
  const separator = raw.indexOf('?');
  const path = separator === -1 ? raw : raw.slice(0, separator);
  const query = new URLSearchParams(separator === -1 ? '' : raw.slice(separator + 1));
  try {
    const [route, ...segments] = (path === 'main-content' ? '' : path).split('/').map(decodeURIComponent);
    const aliases = { tickets: ['tickets','mine'], orders: ['tickets','orders'], refunds: ['requests','refunds'],
      rentals: ['requests','rentals'], support: ['requests','support'], application: ['organizer','status'] };
    if (route === 'account' && Object.hasOwn(aliases, segments[0])) {
      const [section, tab] = aliases[segments[0]];
      return { route: section, segments: [tab, ...segments.slice(1)], query };
    }
    return { route: route || 'home', segments, query };
  } catch {
    return { route: 'unknown', segments: [], query };
  }
}

function cleanup() {
  active?.abort();
  dispose?.();
  dispose = undefined;
  for (const dialog of main.querySelectorAll('dialog[open]')) dialog.close();
}

async function render(moveFocus = false) {
  const { route, segments, query } = parseRoute();
  const fragment = location.hash.slice(1) || 'home';
  const overlay = route === 'account' && ['login','register','forgot','reset'].includes(segments[0]) &&
    lastRoute && (query.get('next') === lastRoute || (!query.has('next') && lastRoute === 'account'));
  const previousLayer = authLayer;
  const restoreFocus = authLayer && fragment === lastRoute ? authTrigger : null;
  dismissLayer();
  if (previousLayer?.dialog && fragment === lastRoute) {
    const api = await apiCapabilities();
    if ((location.hash.slice(1) || 'home') !== fragment) return;
    if (api?.authenticated === previousLayer.authenticated) {
      if (authTrigger?.isConnected) authTrigger.focus({ preventScroll: true });
      return;
    }
  }
  if (overlay) {
    const controller = new AbortController();
    authLayer = { controller };
    const { signal } = controller;
    const [view, api] = await Promise.all([template('account'), apiCapabilities()]);
    if (signal.aborted) return;
    authLayer.authenticated = api?.authenticated;
    const wrapper = document.createElement('div');
    await renderAccount(wrapper, segments, query, { view, api, signal });
    if (signal.aborted) return;
    const dialog = wrapper.querySelector('dialog');
    if (dialog) {
      authLayer.dialog = dialog;
      main.append(dialog);
      dialog.showModal();
      return;
    }
  }
  const discover = route === 'home' && segments.join('/') === 'discover';
  const isHome = route === 'home' && (!segments.length || discover);
  const lastHomeQuery = lastRoute?.split('?')[1] ?? '';
  if (isHome && !home.hidden && home.querySelector('h1') && !firstRender && !restoreFocus && query.toString() === lastHomeQuery) {
    if (discover) {
      const section = home.querySelector('#home-discover-title');
      section.focus({ preventScroll: true });
      section.scrollIntoView();
    } else if (moveFocus) {
      home.querySelector('h1').focus({ preventScroll: true });
      window.scrollTo(0, 0);
    }
    return;
  }

  cleanup();
  active = new AbortController();
  const { signal } = active;
  const target = document.createElement('div');
  const mount = isHome ? home : page;
  home.hidden = !isHome;
  page.hidden = isHome;
  home.replaceChildren();
  page.replaceChildren();
  const loading = notice('Đang mở trang…');
  loading.setAttribute('role', 'status');
  mount.append(loading);

  const section = route === 'event' ? 'events' : route === 'venue' ? 'venues' : route;
  header.classList.toggle('home-header', isHome);
  main.classList.toggle('work-main', route === 'work' || route === 'export');

  try {
    const renderPage = Object.hasOwn(renders, route) && renders[route];
    if (renderPage && (route !== 'home' || isHome)) {
      const [view, api] = await Promise.all([template(route), apiCapabilities()]);
      if (signal.aborted) return;
      updateHeader(api, section);
      let cleanupView;
      const privatePage = ['work', 'export', 'requests', 'organizer', 'tickets'].includes(route);
      if (api && !api.authenticated && privatePage) {
        const labels = { work: 'khu vực vận hành', export: 'báo cáo', requests: 'yêu cầu', tickets: 'vé và đơn hàng', organizer: 'hồ sơ Ban tổ chức' };
        const destination = new URLSearchParams({ next: fragment });
        target.append(heading('Đăng nhập', 'Đăng nhập để tiếp tục đến ' + labels[route] + '.'),
          layout('panel actions', routeLink('Đăng nhập', 'account/login?' + destination, 'button'),
            routeLink('Tạo tài khoản', 'account/register?' + destination, 'button secondary')));
      } else {
        cleanupView = await renderPage(target, segments, query, { view, api, signal });
      }
      if (signal.aborted) {
        cleanupView?.();
        return;
      }
      dispose = cleanupView;
      const failure = readFailure(signal);
      if (!api || failure) {
        const feedback = notice(failure?.message || 'Chưa kết nối được dịch vụ QLSK. Kiểm tra kết nối mạng hoặc cấu hình SQL Server.');
        feedback.classList.add('service-notice');
        feedback.setAttribute('role', 'status');
        feedback.append(' ', failure?.status === 401
          ? routeLink('Đăng nhập lại', 'account/login?' + new URLSearchParams({ next: fragment }), 'button secondary compact')
          : action('Thử lại', refreshView, { style: 'secondary compact' }));
        target.append(feedback);
      }
    }
    if (!target.querySelector('h1')) {
      target.append(heading('Không tìm thấy trang'), panel('Liên kết không hợp lệ',
        notice('Liên kết này không dẫn đến một mục trong QLSK.'), routeLink('Về trang chủ', 'home', 'button secondary')));
    }
  } catch (error) {
    if (signal.aborted) return;
    console.error(error);
    target.replaceChildren(heading('Trang chưa khả dụng'), panel('Tải giao diện',
      notice('Không mở được màn hình này.'), routeLink('Về trang chủ', 'home', 'button secondary')));
  }

  mount.replaceChildren(target);
  lastRoute = fragment;
  const shortcuts = document.querySelector('.quick-actions');
  shortcuts.hidden = !['home','event','events','calendar','venue','venues','guide','account'].includes(route) ||
    Boolean(target.querySelector('dialog[data-route-dialog]'));
  shortcuts.querySelector('.quick-support').href = '#requests/support/new' + (route === 'event' && /^\d+$/.test(segments[0]) ? '?eventId=' + segments[0] : '');
  const goToSeats = route === 'event' && target.querySelector('a[href^="#tickets/book/"]');
  const book = shortcuts.querySelector('.quick-book');
  book.hidden = !goToSeats;
  if (goToSeats) book.href = goToSeats.href;
  firstRender = false;
  const title = target.querySelector('h1');
  document.title = isHome ? 'QLSK' : title.textContent + ' · QLSK';
  const routeDialog = target.querySelector('dialog[data-route-dialog]');
  if (routeDialog) {
    routeDialog.showModal();
    return;
  }
  if (restoreFocus && !signIn.hidden) {
    const matching = [...document.links].find(link => link.getAttribute('href') === restoreFocus.getAttribute('href') && link.textContent === restoreFocus.textContent);
    (matching || signIn).focus({ preventScroll: true });
    return;
  }
  const focus = (discover && target.querySelector('#home-discover-title')) || target.querySelector('[data-route-focus]') || title;
  if (discover || moveFocus) {
    focus.focus({ preventScroll: true });
    if (focus === title) window.scrollTo(0, 0);
    else focus.scrollIntoView({ block: 'nearest' });
  }
}

document.addEventListener('click', event => {
  const link = event.target.closest('a[href*="account/login"], a[href*="account/register"]');
  if (link && !link.closest('dialog')) authTrigger = link;
  for (const menu of document.querySelectorAll('.nav-account[open]')) {
    if (!menu.contains(event.target)) menu.open = false;
  }
});
document.addEventListener('keydown', event => {
  if (event.key !== 'Escape') return;
  const menu = document.activeElement?.closest('.nav-account[open]');
  if (menu) { menu.open = false; menu.querySelector('summary').focus(); }
});

for (const link of document.querySelectorAll('a[href="#main-content"]')) {
  link.addEventListener('click', event => {
    event.preventDefault();
    main.focus();
    main.scrollIntoView();
  });
}
window.addEventListener('hashchange', () => render(true));
window.addEventListener('qlsk:refresh', () => {
  firstRender = true;
  render(true);
});
window.addEventListener('pagehide', () => {
  cleanup();
  for (const form of page.querySelectorAll('form')) form.reset();
});
window.addEventListener('pageshow', event => {
  if (event.persisted) {
    firstRender = true;
    render();
  }
});
render();
