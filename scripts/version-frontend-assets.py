"""Refresh local stylesheet/script URLs whenever their content changes."""
from pathlib import Path
from urllib.parse import unquote, urlsplit
import argparse
import hashlib
import re

thu_muc_web = Path(__file__).resolve().parents[1] / 'wwwroot'
mau_url_tai_nguyen = re.compile(r'(?P<attr>\b(?:src|href))=(?P<quote>["\'])(?P<url>[^"\']+)(?P=quote)')


def cap_nhat_phien_ban_tai_nguyen_html(chi_kiem_tra=False):
    cac_trang_da_thay_doi = []
    for tep_html in thu_muc_web.rglob('*.html'):
        noi_dung_html_goc = tep_html.read_text(encoding='utf-8')

        def thay_url_tai_nguyen(ket_qua_khop_url):
            thanh_phan_url = urlsplit(ket_qua_khop_url['url'])
            if thanh_phan_url.scheme or thanh_phan_url.netloc or not thanh_phan_url.path.endswith(('.js', '.css')):
                return ket_qua_khop_url[0]
            tep_tai_nguyen = ((thu_muc_web if thanh_phan_url.path.startswith('/') else tep_html.parent) / unquote(thanh_phan_url.path.lstrip('/'))).resolve()
            if not tep_tai_nguyen.is_relative_to(thu_muc_web.resolve()) or not tep_tai_nguyen.is_file():
                raise ValueError(f'Missing local asset: {tep_html}: {thanh_phan_url.path}')
            ma_phien_ban_tai_nguyen = hashlib.sha256(tep_tai_nguyen.read_bytes()).hexdigest()[:12]
            return f'{ket_qua_khop_url["attr"]}={ket_qua_khop_url["quote"]}{thanh_phan_url.path}?v={ma_phien_ban_tai_nguyen}{ket_qua_khop_url["quote"]}'

        noi_dung_html_moi = mau_url_tai_nguyen.sub(thay_url_tai_nguyen, noi_dung_html_goc)
        if noi_dung_html_moi != noi_dung_html_goc:
            cac_trang_da_thay_doi.append(tep_html.relative_to(thu_muc_web).as_posix())
            if not chi_kiem_tra:
                tep_html.write_text(noi_dung_html_moi, encoding='utf-8')
    return cac_trang_da_thay_doi


if __name__ == '__main__':
    bo_doc_tham_so = argparse.ArgumentParser()
    bo_doc_tham_so.add_argument('--check', action='store_true')
    tham_so_dong_lenh = bo_doc_tham_so.parse_args()
    cac_trang_da_thay_doi = cap_nhat_phien_ban_tai_nguyen_html(tham_so_dong_lenh.check)
    print(f'{len(cac_trang_da_thay_doi)} pages {"need updated" if tham_so_dong_lenh.check else "received updated"} asset versions.')
    raise SystemExit(1 if tham_so_dong_lenh.check and cac_trang_da_thay_doi else 0)
