"""Capture public report screens using installed Playwright and Microsoft Edge."""
import json
from datetime import datetime, timezone
from pathlib import Path
from playwright.sync_api import sync_playwright

output = Path('docs/bao-cao/anh')
output.mkdir(parents=True, exist_ok=True)
evidence = []
with sync_playwright() as p:
    browser = p.chromium.launch(channel='msedge', headless=True)
    context = browser.new_context(viewport={'width': 1440, 'height': 960},
                                  locale='vi-VN', timezone_id='Asia/Ho_Chi_Minh')
    context.route('**/*', lambda route: route.continue_() if route.request.method == 'GET' else route.abort())
    page = context.new_page()
    for route, filename in [('home', 'trang-chu.png'), ('event/9', 'chi-tiet-su-kien.png'),
                             ('venues', 'danh-sach-dia-diem.png')]:
        errors = []
        page.on('pageerror', lambda error: errors.append(str(error)))
        response = page.goto('http://127.0.0.1:5834/#' + route, wait_until='networkidle')
        page.locator('main h1').first.wait_for()
        page.screenshot(path=str(output / filename), full_page=True)
        page.screenshot(path=str(output / filename.replace('.png', '-khung-nhin.png')), full_page=False)
        evidence.append(dict(route=route, image=filename, title=page.locator('main h1').first.inner_text(),
                             screenshot_at=datetime.now(timezone.utc).isoformat(),
                             width=1440, viewport_height=960, full_page=True, page_errors=errors))
    browser.close()
Path('docs/bao-cao/bang-chung/captures.json').write_text(
    json.dumps(evidence, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps(evidence, ensure_ascii=True, indent=2))
