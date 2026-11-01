"""Build local photo-based event covers. Run: py tests/QLSK.LocalData/scripts/generate-event-images.py

Requires Pillow (py -m pip install Pillow). Source photos are downloaded once;
subsequent runs reuse wwwroot/assets/images/events/sources/.
"""

from __future__ import annotations

import argparse
import concurrent.futures
import json
import os
from pathlib import Path
import urllib.request

from PIL import Image, ImageDraw, ImageFont, ImageOps


ROOT = Path(__file__).resolve().parents[3]
OUTPUT = ROOT / "wwwroot/assets/images/events"
SOURCES = [
    ("music", "1492684223066-81342ee5ff30", "Âm nhạc", "#FFBC69"),
    ("technology", "1519389950473-47ba0277781c", "Công nghệ", "#78D9FF"),
    ("sports", "1461896836934-ffe607ba8211", "Thể thao", "#B7EF79"),
    ("art", "1579783902614-a3fb3927b6a5", "Nghệ thuật", "#D3B5FF"),
    ("food", "1414235077428-338989a2e8c0", "Ẩm thực", "#FFC48E"),
    ("education", "1524178232363-1fb2b075b655", "Giáo dục", "#A7D5FF"),
    ("business", "1515187029135-18ee286d815b", "Kinh doanh", "#A2E6D5"),
    ("nature", "1441974231531-c6227db76b6e", "Môi trường", "#B3E7A6"),
    ("family", "1511632765486-a01980e01a18", "Gia đình", "#FFBED8"),
    ("travel", "1488646953014-85cb44e25828", "Du lịch", "#FFE09D"),
]


def source_url(photo_id: str) -> str:
    return f"https://images.unsplash.com/photo-{photo_id}?auto=format&fit=crop&w=1400&q=85"


def download_source(source: tuple[str, str, str, str]) -> None:
    name, photo_id, _, _ = source
    path = OUTPUT / "sources" / f"{name}.jpg"
    if path.exists():
        with Image.open(path) as image:
            image.verify()
        return
    request = urllib.request.Request(source_url(photo_id), headers={"User-Agent": "QLSK-LocalAssets/1.0"})
    with urllib.request.urlopen(request, timeout=45) as response:
        data = response.read()
    path.write_bytes(data)
    with Image.open(path) as image:
        image.verify()
    print(f"Downloaded {name}: {len(data):,} bytes", flush=True)


def font(size: int, bold: bool = False) -> ImageFont.FreeTypeFont:
    candidates = [
        Path(os.environ.get("WINDIR", "C:/Windows")) / "Fonts" / ("arialbd.ttf" if bold else "arial.ttf"),
        Path("/usr/share/fonts/truetype/dejavu") / ("DejaVuSans-Bold.ttf" if bold else "DejaVuSans.ttf"),
    ]
    for path in candidates:
        if path.exists():
            return ImageFont.truetype(str(path), size)
    raise FileNotFoundError("Install Arial or DejaVu Sans to render Vietnamese cover text.")


def wrap_text(draw: ImageDraw.ImageDraw, value: str, text_font: ImageFont.FreeTypeFont, width: int) -> list[str]:
    lines: list[str] = []
    line = ""
    for word in value.split():
        trial = f"{line} {word}".strip()
        if line and draw.textlength(trial, font=text_font) > width:
            lines.append(line)
            line = word
        else:
            line = trial
    if line:
        lines.append(line)
    return lines


def theme_for(category: str, category_index: int) -> tuple[str, str, str, str]:
    lowered = category.lower()
    words = [
        (0, ["nhạc", "music"]), (1, ["công nghệ", "technology"]),
        (2, ["thể thao", "sport"]), (3, ["nghệ thuật", "triển lãm", "art"]),
        (4, ["ẩm thực", "food"]), (5, ["giáo dục", "học", "kỹ năng", "education"]),
        (6, ["kinh doanh", "khởi nghiệp", "doanh nghiệp", "business"]),
        (7, ["môi trường", "thiện nguyện", "cộng đồng", "nature"]),
        (8, ["gia đình", "thiếu nhi", "family"]), (9, ["du lịch", "travel"]),
    ]
    for index, keywords in words:
        if any(word in lowered for word in keywords):
            return SOURCES[index]
    return SOURCES[category_index % len(SOURCES)]


def render_cover(event: dict, category_index: int) -> None:
    number = int(event["number"])
    title, category = event["title"], event["category"]
    source_name, _, _, accent = theme_for(category, category_index)
    with Image.open(OUTPUT / "sources" / f"{source_name}.jpg") as photo:
        cover = ImageOps.fit(photo.convert("RGB"), (1200, 675), method=Image.Resampling.LANCZOS,
                             centering=(0.35 + ((number - 1) % 5) * 0.075, 0.45)).convert("RGBA")
    gradient = Image.new("RGBA", cover.size)
    painter = ImageDraw.Draw(gradient)
    for y in range(675):
        # Keep the photograph visible while giving text a consistent dark field.
        bottom = int(235 * max(0, (y - 110) / 565) ** 0.8)
        top = int(85 * max(0, (150 - y) / 150))
        painter.line((0, y, 1200, y), fill=(7, 16, 31, max(bottom, top)))
    cover = Image.alpha_composite(cover, gradient)
    draw = ImageDraw.Draw(cover)
    draw.rounded_rectangle((48, 42, 89, 83), radius=12, fill=accent)
    draw.text((59, 48), "S", font=font(26, True), fill="#102033")
    draw.text((104, 47), "SỰ KIỆN HUB", font=font(25, True), fill="white")
    sample = f"SỰ KIỆN  /  {number:03d}"
    sample_font = font(18, True)
    width = int(draw.textlength(sample, font=sample_font)) + 36
    draw.rounded_rectangle((1152 - width, 43, 1152, 82), radius=19, fill=(10, 20, 34, 200))
    draw.text((1170 - width, 51), sample, font=sample_font, fill="white")

    title_size = 55
    title_font = font(title_size, True)
    lines = wrap_text(draw, title, title_font, 1080)
    while len(lines) > 3 and title_size > 36:
        title_size -= 2
        title_font = font(title_size, True)
        lines = wrap_text(draw, title, title_font, 1080)
    line_height = title_size + 13
    title_y = 565 - len(lines) * line_height
    category_font = font(20, True)
    category_width = int(draw.textlength(category.upper(), font=category_font)) + 34
    draw.rounded_rectangle((48, title_y - 65, 48 + category_width, title_y - 23), radius=8, fill=accent)
    draw.text((65, title_y - 57), category.upper(), font=category_font, fill="#102033")
    for index, line in enumerate(lines):
        draw.text((48, title_y + index * line_height), line, font=title_font, fill="white", stroke_width=0)
    draw.line((48, 596, 1152, 596), fill=(255, 255, 255, 80), width=1)
    draw.text((48, 617), "Khám phá  •  Kết nối  •  Trải nghiệm", font=font(21), fill="#E0E7EE")
    draw.text((1010, 615), f"SK / {number:03d}", font=font(22, True), fill=accent)
    cover.convert("RGB").save(OUTPUT / f"event-{number:03d}.jpg", quality=84, optimize=True, progressive=True)


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--download-only", action="store_true")
    parser.add_argument("--catalog", type=Path, default=ROOT / "tests/QLSK.LocalData/catalog/events.json")
    args = parser.parse_args()
    (OUTPUT / "sources").mkdir(parents=True, exist_ok=True)
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
        list(pool.map(download_source, SOURCES))
    if args.download_only:
        return
    catalog = json.loads(args.catalog.read_text(encoding="utf-8-sig"))
    events = catalog["events"] if isinstance(catalog, dict) else catalog
    categories = list(dict.fromkeys(event["category"] for event in events))
    numbers = [int(event["number"]) for event in events]
    if sorted(numbers) != list(range(1, 101)):
        raise ValueError("Catalog must contain exactly event numbers 1 through 100.")
    for event in events:
        render_cover(event, categories.index(event["category"]))
    print(f"Created {len(events)} JPEG covers in {OUTPUT}", flush=True)


if __name__ == "__main__":
    main()
