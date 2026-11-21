import { element, layout, routeLink } from './view.js';

const accountAreas = [
  ['Vé của tôi', 'tickets/mine'], ['Đơn hàng', 'tickets/orders'], ['Hoàn tiền', 'requests/refunds'], ['Thuê địa điểm', 'requests/rentals'],
  ['Hồ sơ cá nhân', 'account/profile'], ['Sự kiện yêu thích', 'account/favorites'], ['Thông báo', 'account/notifications'],
  ['Yêu cầu hỗ trợ', 'requests/support'], ['Hồ sơ Ban tổ chức', 'organizer'],
  ['Vé mời', 'tickets/invitations'], ['Đặt vé', 'tickets/book']
];

export function accountNav(current) {
  const nav = element('nav', '', 'account-nav');
  nav.setAttribute('aria-label', 'Tài khoản');
  const primary = layout('account-nav-main');
  const more = element('details', '', 'nav-account account-more');
  const summary = element('summary', 'Thêm', 'button secondary compact');
  const menu = layout('nav-popover');
  accountAreas.forEach(([name, route], index) => {
    const link = routeLink(name, route);
    if (current === route || current.startsWith(route + '/')) {
      link.setAttribute('aria-current', 'page');
      if (index >= 4) summary.classList.add('active');
    }
    (index < 4 ? primary : menu).append(link);
  });
  more.append(summary, menu);
  nav.append(primary, more);
  return nav;
}

export function workspace(area, content, groups = [], roleLabel) {
  if (area !== 'work' && area !== 'export') {
    const route = location.hash.slice(1).split('?')[0];
    const current = ({ tickets: 'tickets/mine', requests: 'requests/rentals', account: 'account/profile' })[route] || route;
    return layout('stack customer-workspace', accountNav(current), content);
  }
  const nav = element('nav', '', 'workspace-nav');
  nav.setAttribute('aria-label', 'Menu tài khoản và vận hành');
  nav.append(element('p', roleLabel || 'Khu vực vận hành', 'workspace-title'));
  const topLinks = [['Tổng quan', 'work'], ...(area === 'export' ? [['Báo cáo', 'export']] : [])];
  for (const [label, route] of topLinks) {
    const link = routeLink(label, route);
    if (area === route && !groups.some(([, , current]) => current && current !== 'work/overview')) link.setAttribute('aria-current', 'page');
    nav.append(link);
  }
  for (const [label, entries, current] of groups) {
    const group = element('details', '', 'workspace-group');
    group.open = true;
    const links = element('div', '', 'workspace-links');
    group.append(element('summary', label), links);
    for (const [name, route] of entries) {
      const link = routeLink(name, route);
      if (route.split('?')[0] === current?.split('?')[0]) {
        link.setAttribute('aria-current', 'page');
        group.open = true;
      }
      links.append(link);
    }
    nav.append(group);
  }
  nav.append(routeLink('Tài khoản của tôi', 'account', 'workspace-account'));
  const selector = element('select');
  selector.id = 'workspace-section';
  const label = element('label', 'Khu vực làm việc');
  label.htmlFor = selector.id;
  selector.add(new Option('Tổng quan', 'work'));
  if (area === 'export') selector.add(new Option('Báo cáo', 'export'));
  for (const [title, entries, current] of groups) {
    const group = element('optgroup');
    group.label = title;
    for (const [name, route] of entries) {
      const option = new Option(name, route);
      option.selected = route.split('?')[0] === current?.split('?')[0];
      group.append(option);
    }
    selector.append(group);
  }
  if (area === 'export') selector.value = 'export';
  selector.addEventListener('change', () => { location.hash = selector.value; });
  const mobile = layout('workspace-mobile', label, selector);
  return layout('workspace workspace-operational', nav, layout('workspace-content stack', mobile, content));
}
