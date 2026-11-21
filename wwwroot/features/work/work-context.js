import { items } from '../../app/api.js';
import { sections, workGroups } from './work-sections.js';

export const allowed = (api, name) => api?.workspace?.sections?.includes(name) === true;

export function hashWith(path, query) {
  return path + (query.size ? '?' + query : '');
}

export function routeQuery(context) {
  const query = new URLSearchParams();
  for (const key of ['eventId', 'sessionId', 'resource']) {
    if (context[key]) query.set(key, context[key]);
  }
  return query;
}

export function readContext(query, scopes, section) {
  const events = items(scopes) ?? [];
  const event = events.find(item => String(item.id) === query.get('eventId')) ??
    (!query.has('eventId') && section?.scope && events.length === 1 ? events[0] : undefined);
  const sessions = event?.sessions ?? [];
  const session = sessions.find(item => String(item.id) === query.get('sessionId')) ??
    (!query.has('sessionId') && section?.scope === 'session' && sessions.length === 1 ? sessions[0] : undefined);
  const resource = section?.resources?.find(([key]) => key === query.get('resource'))?.[0] ?? section?.resources?.[0][0];
  return { events, event, sessions, session, eventId: event?.id, sessionId: session?.id, resource, options: { ...scopes?.options, ...event?.options, ...session?.options } };
}

export function workspaceGroups(api, context, active) {
  const role = api?.workspace?.role;
  const order = role === 'admin' ? ['Quản trị', 'Xét duyệt', 'Tài chính', 'Sự kiện', 'Tại cổng']
    : role === 'manager' ? ['Xét duyệt', 'Tài chính', 'Quản trị', 'Sự kiện', 'Tại cổng']
    : role === 'staff' ? ['Tại cổng', 'Sự kiện', 'Xét duyệt', 'Tài chính', 'Quản trị'] : workGroups.map(([label]) => label);
  return [...workGroups].sort(([a], [b]) => order.indexOf(a) - order.indexOf(b)).map(([label, group]) => [label,
    group.filter(name => allowed(api, name)).map(name => {
      const section = sections[name];
      const query = section.scope || section.scopedQuery ? routeQuery({ eventId: context.eventId, sessionId: context.sessionId })
        : new URLSearchParams();
      return [section.title, hashWith('work/' + name, query)];
    }), active]).filter(([, links]) => links.length);
}
