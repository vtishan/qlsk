"""Download and crop local illustrative venue photos. Run: py -3 scripts/generate-venue-images.py"""

from concurrent.futures import ThreadPoolExecutor
from io import BytesIO
from pathlib import Path
from urllib.request import Request, urlopen

from PIL import Image, ImageOps


ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / "wwwroot/assets/images/venues"
# Unsplash photo identifiers; these are illustrations, not photos of the sample records.
PHOTOS = [
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
CAPTIONS = [
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
PHOTO_PAGES = {
    5: ("ål nik", "https://unsplash.com/photos/white-chair-lot-J5XqX-qvEZE"),
    7: ("Aditya Sethia", "https://unsplash.com/photos/an-empty-conference-room-with-a-projector-screen-g1YKUSb11KY"),
    9: ("Sergio Butko", "https://unsplash.com/photos/an-outdoor-ceremony-set-up-with-chairs-and-rugs-f-AFdkhAk0I"),
}


def download(item):
    number, photo_id = item
    target = OUTPUT / f"venue-{number:03d}.jpg"
    if target.exists():
        with Image.open(target) as photo:
            photo.verify()
        return
    url = f"https://images.unsplash.com/photo-{photo_id}?auto=format&fit=crop&w=1200&q=85"
    request = Request(url, headers={"User-Agent": "QLSK-VenueAssets/1.0"})
    with urlopen(request, timeout=45) as response:
        data = response.read()
    with Image.open(BytesIO(data)) as photo:
        cover = ImageOps.fit(ImageOps.exif_transpose(photo).convert("RGB"), (960, 540),
                             method=Image.Resampling.LANCZOS)
        cover.save(target, quality=85, optimize=True, progressive=True)
    print(f"Saved {target.name}: {target.stat().st_size:,} bytes", flush=True)


def main():
    OUTPUT.mkdir(parents=True, exist_ok=True)
    with ThreadPoolExecutor(max_workers=4) as pool:
        list(pool.map(download, enumerate(PHOTOS, start=1)))
    rows = "\n".join(
        f"| `venue-{number:03d}.jpg` | {CAPTIONS[number - 1]} | https://images.unsplash.com/photo-{photo_id} | "
        + (f"[{PHOTO_PAGES[number][0]}]({PHOTO_PAGES[number][1]})" if number in PHOTO_PAGES else "Unsplash") + " |"
        for number, photo_id in enumerate(PHOTOS, start=1)
    )
    (OUTPUT / "SOURCES.md").write_text(
        "# Ảnh minh họa địa điểm\n\n"
        "10 ảnh JPG riêng, kích thước 960 × 540 (16:9), được lưu và phục vụ trực tiếp "
        "từ `/assets/images/venues/`. Không lưu dữ liệu ảnh trong database và không "
        "phụ thuộc vào máy chủ ảnh bên ngoài khi mở trang.\n\n"
        "Đây là ảnh minh họa không gian tổ chức sự kiện, không phải ảnh "
        "chụp được xác minh của các địa điểm mang tên tương ứng. Ảnh được gắn cố định "
        "theo mã địa điểm từ 1 đến 10; mã khác dùng ảnh dự phòng.\n\n"
        "Nguồn: Unsplash, sử dụng theo [Unsplash License](https://unsplash.com/license). "
        "Đã tải và kiểm tra trực quan ngày 31/10/2026.\n\n"
        "| File | Không gian | URL ảnh nguồn | Tác giả / trang ảnh |\n| --- | --- | --- | --- |\n" + rows + "\n\n"
        "Tạo lại bằng `py -3 scripts/generate-venue-images.py` (cần Pillow).\n",
        encoding="utf-8",
    )


if __name__ == "__main__":
    main()
