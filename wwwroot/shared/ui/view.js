export function element(tag, text = '', className = '') {
  const node = document.createElement(tag);
  node.textContent = text;
  if (className) node.className = className;
  return node;
}

export function heading(title, description) {
  const header = element('header', '', 'page-heading');
  const titleNode = element('h1', title);
  titleNode.tabIndex = -1;
  header.append(titleNode);
  if (description) header.append(element('p', description));
  return header;
}

export function notice(message) {
  return element('p', message, 'notice');
}

export function routeLink(label, route, className) {
  const link = element('a', label, className);
  link.href = '#' + route;
  return link;
}

export function action(label, handler, { type = 'button', disabled = false, style = 'secondary' } = {}) {
  const button = element('button', label, ('button ' + style).trim());
  button.type = type;
  button.disabled = disabled;
  if (handler) button.addEventListener('click', handler);
  return button;
}

export function panel(title, ...children) {
  const section = element('section', '', 'panel stack');
  section.append(element('h2', title), ...children);
  return section;
}

export function layout(className, ...children) {
  const container = element('div', '', className);
  container.append(...children);
  return container;
}

export function tabLinks(label, options, current) {
  const nav = element('nav', '', 'tabs');
  nav.setAttribute('aria-label', label);
  for (const [name, route] of options) {
    const link = routeLink(name, route);
    if (route === current) link.setAttribute('aria-current', 'page');
    nav.append(link);
  }
  return nav;
}

export function facts(entries) {
  const list = element('dl', '', 'facts');
  for (const [name, value] of entries) {
    const detail = element('dd');
    detail.append(value ?? 'Chưa khả dụng');
    list.append(element('dt', name), detail);
  }
  return list;
}

export function dataTable(title, headers, rows, unavailable, empty) {
  const table = element('table');
  table.append(element('caption', title));
  const head = table.createTHead().insertRow();
  for (const name of headers) {
    const column = element('th', name);
    column.scope = 'col';
    head.append(column);
  }
  const body = table.createTBody();
  if (!rows?.length) {
    const cell = body.insertRow().insertCell();
    cell.colSpan = headers.length;
    cell.append(notice(rows ? empty : unavailable));
  } else {
    for (const values of rows) {
      const row = body.insertRow();
      for (const value of values) row.insertCell().append(value ?? 'Chưa khả dụng');
    }
  }
  const scroll = layout('table-scroll', table);
  scroll.tabIndex = 0;
  scroll.setAttribute('role', 'region');
  scroll.setAttribute('aria-label', title);
  return scroll;
}

let dialogCount = 0;

export function openDialog(target, trigger, title, content, onClose) {
  const dialog = element('dialog', '', 'dialog');
  const titleNode = element('h2', title);
  titleNode.id = 'dialog-title-' + ++dialogCount;
  dialog.setAttribute('aria-labelledby', titleNode.id);
  const closeForm = element('form');
  closeForm.method = 'dialog';
  const close = action('Đóng', null, { type: 'submit' });
  close.autofocus = true;
  closeForm.append(close);
  dialog.append(layout('dialog-heading', titleNode, closeForm), content);
  dialog.addEventListener('close', () => {
    dialog.remove();
    onClose?.();
    if (trigger.isConnected) trigger.focus();
  }, { once: true });
  target.append(dialog);
  dialog.showModal();
  return dialog;
}
