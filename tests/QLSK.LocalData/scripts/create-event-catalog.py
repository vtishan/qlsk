"""Rebuild the local Vietnamese event catalog (no database writes)."""

import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
TOPICS = [
    ("Âm nhạc", "Thưởng thức các tiết mục biểu diễn trực tiếp và giao lưu cùng nghệ sĩ trong không gian âm nhạc gần gũi.", [
        "Đêm nhạc acoustic bên sông", "Giai điệu thanh xuân", "Jazz dưới ánh đèn", "Rock kết nối đam mê", "Hòa nhạc mùa gió mới",
        "Indie và những câu chuyện", "Đêm nhạc phim yêu thích", "Piano cuối tuần", "Guitar mộc giữa phố", "Sắc màu âm nhạc Việt",
    ]),
    ("Công nghệ", "Khám phá sản phẩm mới, trao đổi kinh nghiệm với diễn giả và thực hành qua các phiên trình diễn công nghệ.", [
        "Ngày hội trí tuệ nhân tạo", "Lập trình web từ ý tưởng", "Kết nối cộng đồng dữ liệu", "An toàn thông tin cho mọi người", "Trải nghiệm robot sáng tạo",
        "Điện toán đám mây thực chiến", "Thiết kế ứng dụng di động", "Công nghệ xanh cho đô thị", "Khởi đầu cùng mã nguồn mở", "Ngày hội sản phẩm số",
    ]),
    ("Thể thao", "Vận động cùng cộng đồng, nhận hướng dẫn từ huấn luyện viên và tham gia các hoạt động phù hợp nhiều cấp độ.", [
        "Chạy bộ đón bình minh", "Ngày hội cầu lông phong trào", "Yoga cân bằng cuộc sống", "Đạp xe khám phá thành phố", "Bóng rổ kết nối tuổi trẻ",
        "Thử thách thể lực cuối tuần", "Ngày hội bóng đá cộng đồng", "Nhịp bước vì sức khỏe", "Khám phá bộ môn pickleball", "Giao lưu võ thuật Việt",
    ]),
    ("Nghệ thuật", "Gặp gỡ người làm sáng tạo, tham quan tác phẩm và tự tay trải nghiệm các hoạt động nghệ thuật có hướng dẫn.", [
        "Triển lãm sắc màu phố thị", "Vẽ màu nước cho người mới", "Nhiếp ảnh kể chuyện đời thường", "Nặn gốm bằng đôi tay", "Nghệ thuật in tranh thủ công",
        "Phác họa góc phố thân quen", "Chạm vào nghệ thuật đương đại", "Thư pháp và nét chữ Việt", "Thiết kế poster sáng tạo", "Không gian điêu khắc trẻ",
    ]),
    ("Ẩm thực", "Thưởng thức hương vị đa dạng, tìm hiểu nguyên liệu và tham gia phần thực hành cùng những người yêu ẩm thực.", [
        "Ngày hội món ngon ba miền", "Workshop bánh mì thủ công", "Hành trình cà phê Việt", "Bếp xanh cuối tuần", "Khám phá nghệ thuật pha trà",
        "Lớp làm bánh ngọt đầu tiên", "Ẩm thực đường phố Việt Nam", "Gia vị và câu chuyện quê nhà", "Trải nghiệm món chay sáng tạo", "Bữa sáng cùng đầu bếp",
    ]),
    ("Kinh doanh", "Kết nối người làm kinh doanh, chia sẻ tình huống thực tế và xây dựng kế hoạch hành động sau chương trình.", [
        "Kết nối doanh nghiệp địa phương", "Khởi nghiệp từ bài toán nhỏ", "Xây dựng thương hiệu bền vững", "Quản lý tài chính cửa hàng", "Kỹ năng bán hàng tử tế",
        "Câu chuyện nhà sáng lập", "Thương mại điện tử thực hành", "Tiếp thị nội dung sáng tạo", "Lãnh đạo đội nhóm hiệu quả", "Ngày hội ý tưởng kinh doanh",
    ]),
    ("Giáo dục", "Học qua hoạt động thực hành, trao đổi với người hướng dẫn và mang về tài liệu để tiếp tục tự học.", [
        "Kỹ năng thuyết trình tự tin", "Học ngoại ngữ qua trải nghiệm", "Ngày hội định hướng nghề nghiệp", "Đọc sách và tư duy phản biện", "Quản lý thời gian hiệu quả",
        "Phương pháp học tập chủ động", "Viết sáng tạo mỗi ngày", "Khoa học vui cho gia đình", "Tư duy thiết kế trong học tập", "Kỹ năng làm việc nhóm",
    ]),
    ("Cộng đồng", "Gặp gỡ những người cùng quan tâm, chia sẻ kinh nghiệm và cùng thực hiện hoạt động tích cực cho cộng đồng.", [
        "Đổi sách trao tri thức", "Ngày xanh khu phố", "Gian hàng sẻ chia", "Kết nối tình nguyện viên", "Sống xanh từ việc nhỏ",
        "Gặp gỡ câu lạc bộ đọc sách", "Ngày hội tái chế sáng tạo", "Chăm sóc vườn cộng đồng", "Câu chuyện người truyền cảm hứng", "Kết nối hàng xóm thân thiện",
    ]),
    ("Du lịch", "Tìm cảm hứng cho chuyến đi, khám phá văn hóa địa phương và học cách chuẩn bị hành trình có trách nhiệm.", [
        "Hành trình khám phá miền Tây", "Kể chuyện những cung đường", "Du lịch chậm và trải nghiệm", "Khám phá di sản Việt Nam", "Chuẩn bị chuyến đi đầu tiên",
        "Văn hóa bản địa qua ống kính", "Cắm trại cùng bạn bè", "Ngày hội du lịch xanh", "Trekking cho người mới", "Ẩm thực trên những hành trình",
    ]),
    ("Gia đình", "Dành thời gian chất lượng bên người thân với các hoạt động sáng tạo, vui chơi và kết nối nhiều thế hệ.", [
        "Ngày hội gia đình sáng tạo", "Cùng con làm đồ thủ công", "Kể chuyện dưới tán cây", "Trò chơi dân gian cuối tuần", "Vườn nhỏ của bé",
        "Vẽ tranh cùng ba mẹ", "Khám phá khoa học cùng con", "Bếp vui cả nhà", "Ngày hội tuổi thơ", "Kết nối những thế hệ",
    ]),
]

events = []
for category, description, titles in TOPICS:
    for title in titles:
        number = len(events) + 1
        events.append({
            "number": number,
            "title": title,
            "category": category,
            "description": description + " Chương trình gồm đón khách, hoạt động chính và phần giao lưu tổng kết. Vui lòng đến trước 30 phút để check-in.",
            "image": f"/assets/images/events/event-{number:03d}.jpg",
        })

destination = ROOT / "tests/QLSK.LocalData/catalog/events.json"
destination.write_text(json.dumps({"events": events}, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(f"Created {len(events)} local events in {destination}")
