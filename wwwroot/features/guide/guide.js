import { element, heading, notice, panel, routeLink } from '../../shared/ui/view.js';
import { layout, tabLinks } from '../../shared/ui/view.js';
import { endpoint, items, read } from '../../app/api.js';

const topics = [
  ['Tôi có bao lâu để thanh toán vé?', 'Ghế được giữ trong 10 phút theo hạn hiển thị trên đơn. Mỗi tài khoản tối đa 10 vé đã mua và đang giữ trong cùng một suất. Kiểm tra trạng thái đơn trước khi thanh toán lại.', 'tickets/orders', 'Mở lịch sử đơn'],
  ['Khi nào và làm sao để hoàn vé?', 'Nếu sự kiện cho phép, chọn vé chưa check-in và gửi yêu cầu trước ít nhất 72 giờ. Số tiền đề nghị dựa trên giá vé sau giảm giá. Quản lý xét điều khoản đã chốt; đã duyệt chưa có nghĩa đã chuyển tiền.', 'requests/refunds', 'Theo dõi hoàn tiền'],
  ['Tôi phải cọc bao nhiêu khi thuê?', 'Cọc 30% trong 24 giờ sau khi được duyệt giữ lịch. Đọc chính sách thuê đã công bố và điều khoản được chốt trong hồ sơ để biết điều kiện hủy và hoàn cọc.', 'requests/rentals', 'Theo dõi yêu cầu thuê'],
  ['Tìm và xem lịch sự kiện', 'Mở lịch theo giờ Việt Nam, chọn ngày rồi mở suất diễn để xem địa điểm và giá vé.', 'calendar', 'Mở lịch'],
  ['Đặt vé và quản lý vé', 'Đăng nhập, chọn suất và ghế, rồi kiểm tra đơn và hạn giữ chỗ trước khi thanh toán. Vé đã phát hành nằm trong mục Vé của tôi.', 'tickets', 'Mở Vé của tôi'],
  ['Thuê địa điểm', 'Chọn địa điểm và khoảng giờ theo lịch công khai. Sau khi gửi yêu cầu, theo dõi hạn cọc và xác nhận thuê trong tài khoản.', 'venues', 'Mở Địa điểm'],
  ['Hoàn tiền và hỗ trợ', 'Tự hủy vé cần còn ít nhất 72 giờ, sự kiện cho phép và vé chưa check-in. Tiền đề nghị hoàn dựa trên giá đã chốt sau giảm giá; nhân viên kiểm tra điều khoản trước khi duyệt. Trạng thái đã duyệt chưa có nghĩa đã chuyển tiền.', 'requests/refunds', 'Theo dõi hoàn tiền'],
  ['Đăng ký làm ban tổ chức', 'Ban tổ chức cần tài khoản và hồ sơ được xét duyệt trước khi có quyền quản lý sự kiện.', 'organizer', 'Mở Ban tổ chức']
];

const articles = {
  about: ['Về QLSK', 'QLSK giúp bạn khám phá sự kiện, tra cứu lịch diễn theo giờ Việt Nam, quản lý vé điện tử và gửi yêu cầu thuê địa điểm. Thông tin sự kiện, giá và điều kiện được công bố tại từng sự kiện.', 'events', 'Khám phá sự kiện'],
  privacy: ['Quyền riêng tư', 'Chỉ cung cấp thông tin cần thiết khi đăng ký, mua vé hoặc gửi yêu cầu. Vé, đơn hàng và nội dung hỗ trợ gắn với tài khoản của bạn; không chia sẻ mật khẩu, mã xác minh hoặc mã QR vé. Liên hệ hỗ trợ trong tài khoản để yêu cầu xử lý dữ liệu cá nhân.', 'requests/support/new', 'Liên hệ hỗ trợ'],
  rules: ['Điều khoản sử dụng', 'Đọc điều khoản đã công bố trước khi đăng ký, đặt vé hoặc thuê địa điểm. Kiểm tra sự kiện, suất diễn, ghế và tổng chốt trước khi xác nhận. Chính sách hoàn vé và thuê địa điểm áp dụng theo phiên bản điều khoản đã chốt trong giao dịch.', 'guide', 'Xem chính sách công bố'],
  tickets: ['Hướng dẫn đặt và sử dụng vé', 'Đăng nhập, mở sự kiện, chọn suất và khu vực ghế. Cụm ghế cần được chọn đầy đủ. Kiểm tra tổng chốt và trạng thái đơn; mở Vé của tôi để xem QR hoặc tải PDF khi vé đã phát hành. Nếu cổng thanh toán chưa khả dụng, hệ thống chỉ xác nhận đơn miễn phí. Không chuyển tiền qua liên kết không được hệ thống cung cấp.', 'tickets/mine', 'Mở Vé của tôi'],
  faq: ['Câu hỏi thường gặp', 'Vé nằm ở đâu? Mở Vé của tôi sau khi đơn đã xác nhận. Có thể hoàn vé không? Kiểm tra điều kiện tối thiểu 72 giờ và chính sách sự kiện trong mục Hoàn tiền. Lịch địa điểm trống có được bảo đảm không? Lịch chỉ được xác nhận sau khi yêu cầu thuê được duyệt và hoàn tất điều kiện đặt cọc.', 'requests/support/new', 'Gửi câu hỏi hỗ trợ']
};

export async function renderGuide(target, segments, query, { view, api, signal }) {
  if (segments.length > 1 || (segments.length && !Object.hasOwn(articles, segments[0]))) return;
  const published = items(await read(api, 'catalog.policies', endpoint('policies'), { signal }));
  const seen = new Set();
  const policies = published?.filter(policy => {
    if (seen.has(policy.title)) return false;
    seen.add(policy.title);
    return true;
  });
  const selected = segments[0];
  const links = [['Tổng quan', 'guide'], ['Đặt vé', 'guide/tickets'], ['Quy định', 'guide/rules'], ['Câu hỏi thường gặp', 'guide/faq'], ['Quyền riêng tư', 'guide/privacy']];
  const main = layout('stack');
  const [title, text, route, label] = articles[selected] ?? articles.about;
  main.append(panel(title, element('p', text), routeLink(label, route, 'button secondary panel-action')));
  if (!selected || selected === 'about') {
    main.append(panel('Cách đặt vé', element('p', articles.tickets[1]), routeLink('Xem hướng dẫn đặt vé', 'guide/tickets')));
    main.append(panel('Quy định', element('p', articles.rules[1]), routeLink('Xem quy định và chính sách', 'guide/rules')));
  }
  const list = [];
  for (const [title, description, route, label] of topics) {
    const topic = element('details', '', 'guide-topic');
    topic.append(element('summary', title), element('p', description), routeLink(label, route, 'button secondary panel-action'));
    list.push(topic);
  }
  const questions = panel('Câu hỏi thường gặp', ...list);
  main.append(panel('Chính sách công bố', ...(policies?.length
    ? policies.map(policy => {
      const detail = element('details', '', 'guide-topic');
      detail.open = selected === 'rules';
      detail.append(element('summary', policy.title), element('p', policy.content));
      return detail;
    })
    : [notice(policies ? 'Chưa có chính sách được công bố.' : 'Nguồn chính sách công bố chưa khả dụng.')])));
  target.append(heading(selected === 'rules' ? 'Quy định và chính sách' : selected ? title : 'Hướng dẫn sử dụng QLSK'),
    layout('stack guide-page', tabLinks('Nội dung hướng dẫn', links, selected ? 'guide/' + selected : 'guide'),
      layout('guide-layout', main, questions)));
}
