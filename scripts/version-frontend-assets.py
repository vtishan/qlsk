"""Refresh local stylesheet/script URLs whenever their content changes."""
from pathlib import Path
from urllib.parse import unquote, urlsplit
import argparse
import hashlib
import re

root = Path(__file__).resolve().parents[1] / 'wwwroot'
pattern = re.compile(r'(?P<attr>\b(?:src|href))=(?P<quote>["\'])(?P<url>[^"\']+)(?P=quote)')


def version_pages(check=False):
    changed = []
    for page in root.rglob('*.html'):
        original = page.read_text(encoding='utf-8')

        def replace(match):
            url = urlsplit(match['url'])
            if url.scheme or url.netloc or not url.path.endswith(('.js', '.css')):
                return match[0]
            asset = ((root if url.path.startswith('/') else page.parent) / unquote(url.path.lstrip('/'))).resolve()
            if not asset.is_relative_to(root.resolve()) or not asset.is_file():
                raise ValueError(f'Missing local asset: {page}: {url.path}')
            version = hashlib.sha256(asset.read_bytes()).hexdigest()[:12]
            return f'{match["attr"]}={match["quote"]}{url.path}?v={version}{match["quote"]}'

        updated = pattern.sub(replace, original)
        if updated != original:
            changed.append(page.relative_to(root).as_posix())
            if not check:
                page.write_text(updated, encoding='utf-8')
    return changed


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    changed = version_pages(args.check)
    print(f'{len(changed)} pages {"need updated" if args.check else "received updated"} asset versions.')
    raise SystemExit(1 if args.check and changed else 0)
