"""Download and crop local illustrative venue photos. Run: py -3 scripts/generate-venue-images.py"""

from concurrent.futures import ThreadPoolExecutor
from io import BytesIO
from pathlib import Path
from urllib.request import Request, urlopen

from PIL import Image, ImageOps


THU_MUC_DU_AN = Path(__file__).resolve().parents[1]
THU_MUC_ANH_DIA_DIEM = THU_MUC_DU_AN / "wwwroot/assets/images/venues"
# Unsplash photo identifiers; these are illustrations, not photos of the sample records.
MA_ANH_UNSPLASH = [
    "1519167758481-83f550bb49b3",
    "1511795409834-ef04bbd61622",
    "1505373877841-8d25f7d46678",
    "1511578314322-379afb476865",
    "1505845753232-f74a87b62db6",
    "1517502884422-41eaead166d4",
    "1724315069543-52ffaa592bcc",
    "1497366811353-6870744d04b2",
    "1677677403344-029c7fcd7300",
    "1464366400600-7168b8af9bc3",
]
CHU_THICH_ANH = [
    "Sảnh tiệc với bàn tròn",
    "Bàn tiệc trang trí hoa",
    "Hội trường có sân khấu trình chiếu",
    "Phòng hội nghị với bàn tiệc",
    "Phòng hội thảo với các hàng ghế",
    "Phòng họp nhìn ra thành phố",
    "Hội trường có màn chiếu",
    "Phòng họp sáng với cửa kính",
    "Không gian tổ chức ngoài trời",
    "Sảnh tiệc với bàn trang trí",
]
TRANG_NGUON_ANH = {
    5: ("ål nik", "https://unsplash.com/photos/white-chair-lot-J5XqX-qvEZE"),
    7: ("Aditya Sethia", "https://unsplash.com/photos/an-empty-conference-room-with-a-projector-screen-g1YKUSb11KY"),
    9: ("Sergio Butko", "https://unsplash.com/photos/an-outdoor-ceremony-set-up-with-chairs-and-rugs-f-AFdkhAk0I"),
}


def tai_va_cat_anh_dia_diem(thong_tin_anh):
    so_thu_tu_anh, ma_anh_unsplash = thong_tin_anh
    tep_anh_dich = THU_MUC_ANH_DIA_DIEM / f"venue-{so_thu_tu_anh:03d}.jpg"
    if tep_anh_dich.exists():
        with Image.open(tep_anh_dich) as anh_goc:
            anh_goc.verify()
        return
    url_anh_unsplash = f"https://images.unsplash.com/photo-{ma_anh_unsplash}?auto=format&fit=crop&w=1200&q=85"
    yeu_cau_tai_anh = Request(url_anh_unsplash, headers={"User-Agent": "QLSK-VenueAssets/1.0"})
    with urlopen(yeu_cau_tai_anh, timeout=45) as phan_hoi_tai_anh:
        du_lieu_anh = phan_hoi_tai_anh.read()
    with Image.open(BytesIO(du_lieu_anh)) as anh_goc:
        anh_da_cat = ImageOps.fit(ImageOps.exif_transpose(anh_goc).convert("RGB"), (960, 540),
                             method=Image.Resampling.LANCZOS)
        anh_da_cat.save(tep_anh_dich, quality=85, optimize=True, progressive=True)
    print(f"Saved {tep_anh_dich.name}: {tep_anh_dich.stat().st_size:,} bytes", flush=True)


def tao_bo_anh_dia_diem():
    THU_MUC_ANH_DIA_DIEM.mkdir(parents=True, exist_ok=True)
    with ThreadPoolExecutor(max_workers=4) as nhom_tac_vu_tai_anh:
        list(nhom_tac_vu_tai_anh.map(tai_va_cat_anh_dia_diem, enumerate(MA_ANH_UNSPLASH, start=1)))
    cac_dong_nguon_anh = "\n".join(
        f"| `venue-{so_thu_tu_anh:03d}.jpg` | {CHU_THICH_ANH[so_thu_tu_anh - 1]} | https://images.unsplash.com/photo-{ma_anh_unsplash} | "
        + (f"[{TRANG_NGUON_ANH[so_thu_tu_anh][0]}]({TRANG_NGUON_ANH[so_thu_tu_anh][1]})" if so_thu_tu_anh in TRANG_NGUON_ANH else "Unsplash") + " |"
        for so_thu_tu_anh, ma_anh_unsplash in enumerate(MA_ANH_UNSPLASH, start=1)
    )
    (THU_MUC_ANH_DIA_DIEM / "SOURCES.md").write_text(
        "# Ảnh minh họa địa điểm\n\n"
        "10 ảnh JPG riêng, kích thước 960 × 540 (16:9), được lưu và phục vụ trực tiếp "
        "từ `/assets/images/venues/`. Không lưu dữ liệu ảnh trong database và không "
        "phụ thuộc vào máy chủ ảnh bên ngoài khi mở trang.\n\n"
        "Đây là ảnh minh họa không gian tổ chức sự kiện, không phải ảnh "
        "chụp được xác minh của các địa điểm mang tên tương ứng. Ảnh được gắn cố định "
        "theo mã địa điểm từ 1 đến 10; mã khác dùng ảnh dự phòng.\n\n"
        "Nguồn: Unsplash, sử dụng theo [Unsplash License](https://unsplash.com/license). "
        "Đã tải và kiểm tra trực quan ngày 31/10/2026.\n\n"
        "| File | Không gian | URL ảnh nguồn | Tác giả / trang ảnh |\n| --- | --- | --- | --- |\n" + cac_dong_nguon_anh + "\n\n"
        "Tạo lại bằng `py -3 scripts/generate-venue-images.py` (cần Pillow).\n",
        encoding="utf-8",
    )


if __name__ == "__main__":
    tao_bo_anh_dia_diem()
