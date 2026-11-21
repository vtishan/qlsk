const templates = new Map();
const templateFolders = { event: 'events', venue: 'venues', export: 'reports', 'work-overview': 'work' };

export async function template(name) {
  if (!templates.has(name)) {
    const folder = templateFolders[name] ?? name;
    const url = new URL(`../../features/${folder}/${name}.html`, import.meta.url);
    const response = await fetch(url);
    if (!response.ok) throw new Error('Thiếu giao diện ' + name);
    const node = document.createElement('template');
    node.innerHTML = await response.text();
    templates.set(name, node);
  }
  return templates.get(name).content.cloneNode(true);
}

export function slot(view, name, ...children) {
  view.querySelector('[data-slot="' + name + '"]').replaceChildren(...children);
}
