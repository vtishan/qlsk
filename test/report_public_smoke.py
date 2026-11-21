"""Read-only report checks. Usage: python test/report_public_smoke.py --base URL."""
import argparse
import json
from datetime import datetime, timedelta, timezone
from pathlib import Path
from urllib.parse import urlencode
from urllib.request import urlopen

parser = argparse.ArgumentParser()
parser.add_argument('--base', default='http://127.0.0.1:5834')
parser.add_argument('--output', default='docs/bao-cao/bang-chung/public-smoke.json')
args = parser.parse_args()
results = []
state = {}


def get(path, **query):
    url = args.base.rstrip('/') + '/api/' + path
    if query:
        url += '?' + urlencode(query)
    with urlopen(url, timeout=20) as response:
        assert response.status == 200
        assert 'application/json' in response.headers['Content-Type']
        return json.load(response)


def case(code, mapped, description, action):
    try:
        detail = action()
        results.append(dict(id=code, mapped=mapped, description=description,
                            passed=True, observation=detail))
    except Exception as error:
        results.append(dict(id=code, mapped=mapped, description=description,
                            passed=False, error=str(error)))


def capabilities():
    data = get('capabilities')
    assert data['authenticated'] is False
    assert 'catalog.events' in data['operations']
    return 'Guest session; catalog.events available; token omitted from evidence.'


def events():
    data = get('events')['items']
    assert data and all(x['id'] > 0 and x['title'] for x in data)
    state['event'] = data[0]
    return {'count_first_page': len(data), 'selected_id': data[0]['id']}


def search():
    selected = state['event']
    data = get('events', q=selected['title'])['items']
    assert any(x['id'] == selected['id'] for x in data)
    assert all(selected['title'].lower() in x['title'].lower() for x in data)
    return {'matching_count': len(data)}


def prices():
    data = get('events', minPrice=100000, maxPrice=100000)['items']
    assert data and all(x['minPrice'] == 100000 for x in data)
    return {'matching_count': len(data), 'min_price': 100000}


def detail():
    data = get('events/' + str(state['event']['id']))
    assert data['title'] == state['event']['title'] and data['sessions']
    assert all(s['venue']['id'] > 0 and s['start'] and s['end'] for s in data['sessions'])
    state['detail'] = data
    return {'id': data['id'], 'sessions': len(data['sessions'])}


def calendar():
    session = state['detail']['sessions'][0]
    start = datetime.fromisoformat(session['start'])
    data = get('sessions', **{'from': (start - timedelta(days=1)).isoformat(),
                             'to': (start + timedelta(days=1)).isoformat()})['items']
    assert any(x['id'] == session['id'] for x in data)
    return {'selected_session': session['id'], 'count_first_page': len(data)}


def venues():
    data = get('venues', maxPrice=1000000)['items']
    assert data and all(0 <= x['rate'] <= 1000000 and x['capacity'] > 0 for x in data)
    state['venue'] = data[0]
    return {'count_first_page': len(data), 'selected_id': data[0]['id']}


def venue_detail():
    data = get('venues/' + str(state['venue']['id']))
    assert data['name'] == state['venue']['name'] and data['zones']
    return {'id': data['id'], 'zones': len(data['zones'])}


def options():
    data = get('event-filter-options')
    assert data['cities'] and data['venues'] and data['categories']
    assert all(x['min'] <= x['max'] for x in data['priceBands'])
    return {key: len(value) for key, value in data.items()}


def policies():
    data = get('policies')['items']
    assert data and all(x['title'] and x['content'] for x in data)
    return {'published_policy_count_first_page': len(data)}


for index, (mapped, description, action) in enumerate([
    ('K-06', 'Public guest capabilities', capabilities),
    ('K-01', 'Public event listing', events),
    ('K-01', 'Search by existing event title', search),
    ('K-01', 'Filter events by exact price', prices),
    ('K-02', 'Event detail and sessions', detail),
    ('K-03', 'Calendar includes selected session', calendar),
    ('K-04', 'Venue listing and rental price filter', venues),
    ('K-04', 'Venue detail and seating zones', venue_detail),
    ('K-01', 'Filter options are populated', options),
    ('K-05', 'Published public policy content', policies),
], 1):
    case(f'BC-{index:02}', mapped, description, action)

report = dict(executed_at=datetime.now(timezone.utc).isoformat(), base=args.base,
              method='GET only, unauthenticated, Python standard library',
              passed=sum(x['passed'] for x in results), total=len(results), cases=results)
Path(args.output).parent.mkdir(parents=True, exist_ok=True)
Path(args.output).write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps(report, ensure_ascii=True, indent=2))
raise SystemExit(0 if report['passed'] == report['total'] else 1)
