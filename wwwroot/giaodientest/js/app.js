'use strict';

// Biểu tượng và poster từ mẫu VÉNUE ban đầu.
const paths={search:'<circle cx="11" cy="11" r="7"/><path d="m20 20-4-4"/>',arrow:'<path d="M5 12h14m-7-7 7 7-7 7"/>',chevron:'<path d="m9 18 6-6-6-6"/>',calendar:'<rect x="3" y="5" width="18" height="16" rx="2"/><path d="M7 3v4m10-4v4M3 10h18"/>',pin:'<path d="M20 10c0 5-8 11-8 11S4 15 4 10a8 8 0 1 1 16 0Z"/><circle cx="12" cy="10" r="2"/>',ticket:'<path d="M3 8V5h18v3a3 3 0 0 0 0 6v5H3v-5a3 3 0 0 0 0-6Z"/><path d="M13 5v14" stroke-dasharray="2 2"/>',heart:'<path d="M20.8 4.6a5.5 5.5 0 0 0-7.8 0L12 5.7l-1.1-1.1a5.5 5.5 0 0 0-7.8 7.8L12 21l8.8-8.6a5.5 5.5 0 0 0 0-7.8Z"/>',user:'<circle cx="12" cy="8" r="4"/><path d="M4 21a8 8 0 0 1 16 0"/>',menu:'<path d="M3 6h18M3 12h18M3 18h18"/>',close:'<path d="M5 5l14 14M19 5 5 19"/>',check:'<path d="m4 12 5 5L20 6"/>',shield:'<path d="M12 2 4 5v6c0 5 4 9 8 11 4-2 8-6 8-11V5l-8-3Z"/><path d="m9 12 2 2 4-4"/>',clock:'<circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 2"/>',spark:'<path d="m12 2 2.4 7.6L22 12l-7.6 2.4L12 22l-2.4-7.6L2 12l7.6-2.4L12 2Z"/>',users:'<circle cx="9" cy="8" r="3"/><path d="M2 20a7 7 0 0 1 14 0M17 5a3 3 0 0 1 0 6m2 9h3a6 6 0 0 0-4-5.6"/>',chart:'<path d="M3 20V4m0 16h18M7 15l4-5 4 2 5-7"/>',grid:'<rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/>',settings:'<circle cx="12" cy="12" r="3"/><path d="M12 2v2m0 16v2M2 12h2m16 0h2M5 5l1.5 1.5m11 11L19 19M19 5l-1.5 1.5m-11 11L5 19"/>',wallet:'<rect x="3" y="5" width="18" height="15" rx="2"/><path d="M3 9h18m-5 5h2"/>',scan:'<path d="M8 3H5a2 2 0 0 0-2 2v3m13-5h3a2 2 0 0 1 2 2v3M3 16v3a2 2 0 0 0 2 2h3m13-5v3a2 2 0 0 1-2 2h-3M3 12h18"/>',bell:'<path d="M18 8a6 6 0 0 0-12 0c0 8-3 8-3 10h18c0-2-3-2-3-10m-8 13h4"/>',back:'<path d="m15 18-6-6 6-6"/>',plus:'<path d="M12 4v16M4 12h16"/>',edit:'<path d="m4 20 4-.9L20 7a2 2 0 0 0-3-3L5 16l-1 4Z"/>',logout:'<path d="M10 4H5a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h5m5-4 4-4-4-4m4 4H9"/>',info:'<circle cx="12" cy="12" r="10"/><path d="M12 11v6m0-10v.1"/>',download:'<path d="M12 3v12m-4-4 4 4 4-4M4 17v4h16v-4"/>',building:'<rect x="4" y="3" width="16" height="18" rx="2"/><path d="M9 7h2m4 0h1M9 11h2m4 0h1M9 15h2m4 0h1"/>'};
Object.assign(paths,{
  phone:'<path d="M22 17v3a2 2 0 0 1-2 2A19 19 0 0 1 2 4a2 2 0 0 1 2-2h3l2 5-3 2a14 14 0 0 0 7 7l2-3 5 2Z"/>',
  chat:'<path d="M20 15a3 3 0 0 1-3 3H8l-5 3V6a3 3 0 0 1 3-3h11a3 3 0 0 1 3 3Z"/><path d="M7 9h9m-9 4h5"/>',
  list:'<path d="M9 6h12M9 12h12M9 18h12M3 6h1M3 12h1M3 18h1"/>'
});
const ic=(name,size=20)=>`<span class="icon" style="width:${size}px;height:${size}px"><svg aria-hidden="true" viewBox="0 0 24 24">${paths[name]||paths.spark}</svg></span>`;
const images={
  festival:'../assets/images/hero.webp', art:'../assets/images/art.webp', dj:'../assets/images/dj.webp', theatre:'../assets/images/theatre.webp',
  music:'../assets/images/music.webp', conference:'../assets/images/conference.webp', dance:'../assets/images/dance.webp', concert:'../assets/images/concert.webp'
};
const events=[{id:1,title:'NEON WAVE FESTIVAL 2026',cat:'Âm nhạc',date:'12 TH12, 2026',time:'17:00',venue:'Cung Văn hóa Hữu nghị',city:'Hà Nội',price:690000,tag:'BÁN CHẠY',image:images.festival,description:'Một đêm hội âm nhạc điện tử bùng nổ, nơi ánh sáng, âm nhạc và những cộng đồng đam mê gặp nhau. Trải nghiệm các sân khấu đa giác quan, hệ thống âm thanh đỉnh cao và những khoảnh khắc không thể quên.'},{id:2,title:'THE ART OF STILLNESS',cat:'Nghệ thuật',date:'19 TH12, 2026',time:'09:00',venue:'The Factory Contemporary',city:'TP. Hồ Chí Minh',price:250000,tag:'ĐANG MỞ BÁN',image:images.art,description:'Triển lãm nghệ thuật đương đại đưa bạn qua một hành trình của hình khối, ánh sáng và những khoảng lặng đầy cảm hứng.'},{id:3,title:'AFTER HOURS: UNDERGROUND',cat:'Âm nhạc',date:'26 TH12, 2026',time:'21:00',venue:'Indika Saigon',city:'TP. Hồ Chí Minh',price:390000,tag:'SẮP HẾT VÉ',image:images.dj,description:'Một đêm dành riêng cho những nhịp điệu underground, nghệ sĩ độc lập và văn hóa club nguyên bản.'},{id:4,title:'STORIES ON STAGE',cat:'Sân khấu',date:'05 TH01, 2027',time:'19:30',venue:'Nhà hát Tuổi trẻ',city:'Hà Nội',price:320000,tag:'MỚI',image:images.theatre,description:'Một sân khấu nhỏ, những câu chuyện lớn. Đêm diễn mang lại trải nghiệm đầy cảm xúc và kết nối.'},{id:5,title:'SUNSET SOUNDS LIVE',cat:'Âm nhạc',date:'10 TH01, 2027',time:'16:00',venue:'Công viên Biển Đông',city:'Đà Nẵng',price:490000,tag:'PHỔ BIẾN',image:images.music,description:'Âm nhạc sống bên bờ biển khi hoàng hôn buông xuống, với nghệ sĩ trẻ và không gian đầy cảm hứng.'},{id:6,title:'FUTURE FORWARD SUMMIT',cat:'Hội thảo',date:'17 TH01, 2027',time:'08:30',venue:'Trung tâm Hội nghị Quốc gia',city:'Hà Nội',price:850000,tag:'EARLY BIRD',image:images.conference,description:'Gặp gỡ những nhà sáng tạo, lãnh đạo và người tiên phong đang định hình tương lai công nghệ.'},{id:7,title:'MOVEMENT IN MOTION',cat:'Workshop',date:'24 TH01, 2027',time:'14:00',venue:'Soul Studio',city:'TP. Hồ Chí Minh',price:220000,tag:'GIỚI HẠN',image:images.dance,description:'Workshop kết nối cơ thể và tâm trí qua nghệ thuật chuyển động đương đại.'},{id:8,title:'ECHOES OF THE NIGHT',cat:'Âm nhạc',date:'31 TH01, 2027',time:'19:00',venue:'Nhà thi đấu Quân khu 7',city:'TP. Hồ Chí Minh',price:590000,tag:'HOT',image:images.concert,description:'Đêm nhạc hội tụ những thanh âm đáng nhớ và nhiều màn trình diễn đặc biệt.'}];

// Dữ liệu mẫu và quy tắc nghiệp vụ

const root = document.querySelector('#root');
const modal = document.querySelector('#dialog');
const snackbar = document.querySelector('#snackbar');
const storeKey = 'venue-qlsk-demo-v1';
const minute = 60_000;
const hour = 60 * minute;
const day = 24 * hour;
const roles = {
  guest:'Khách', user:'Người dùng', organizer:'Ban tổ chức', staff:'Quản lý sự kiện',
  manager:'Quản lý hệ thống', admin:'Admin'
};
const permissionNames = {
  'checkin.scan':'Quét QR', 'checkin.manual':'Check-in thủ công', 'attendees.view':'Xem khách tham dự',
  'orders.view':'Xem đơn và vé', 'reports.view':'Xem báo cáo', 'guests.manage':'Quản lý khách mời',
  'content.propose':'Đề xuất nội dung'
};
const managerPermissions = {
  applications:'Duyệt hồ sơ BTC', reviews:'Duyệt sự kiện', rentals:'Duyệt thuê địa điểm',
  refunds:'Xử lý hoàn tiền', payments:'Theo dõi thanh toán', support:'Xử lý hỗ trợ',
  settlements:'Duyệt đối soát', monitoring:'Giám sát dịch vụ'
};
const status = {
  DRAFT:['Bản nháp'], PENDING:['Chờ duyệt','warn'], NEEDS_INFO:['Cần bổ sung','warn'],
  APPROVED:['Đã duyệt','ok'], REJECTED:['Từ chối','bad'], ACTIVE:['Đang công khai','ok'],
  PENDING_UPDATE:['Chờ duyệt thay đổi','warn'], SUSPENDED:['Đình chỉ','bad'], CANCELLED:['Đã hủy','bad'],
  POSTPONED:['Đã hoãn','warn'], COMPLETED:['Hoàn tất','ok'], EXPIRED:['Hết hạn','bad'],
  PENDING_PAYMENT:['Chờ thanh toán','warn'], PAID:['Đã thanh toán','ok'], FAILED:['Thất bại','bad'],
  PAID_OUT_OF_STOCK:['Đã thu tiền · Hết chỗ','bad'], ISSUED:['Đã phát hành','ok'], CHECKED_IN:['Đã check-in','ok'],
  REVOKED:['Đã thu hồi','bad'], REFUNDED:['Đã hoàn tiền','ok'], REFUND_PENDING:['Chờ hoàn tiền','warn'],
  REQUESTED:['Đã gửi yêu cầu','warn'], AWAITING_DEPOSIT:['Chờ đặt cọc','warn'], DEPOSIT_REVIEW:['Chờ đối soát cọc','info'],
  CONFIRMED:['Đã xác nhận','ok'], CANCELLATION_REQUESTED:['Chờ hủy thuê','warn'], ESCALATED:['Chuyển Admin','warn'],
  AUTO_APPROVED:['Duyệt tự động','ok'], MANUAL_REVIEW:['Chờ xét duyệt','warn'], PROCESSING:['Đang chuyển','info'],
  ORGANIZER_CONFIRMED:['BTC đã xác nhận','ok'], MANAGER_APPROVED:['Quản lý đã duyệt','ok'],
  PAYOUT_PENDING:['Chờ chi trả','warn'], DISPUTED:['Cần đối chiếu','warn'], OPEN:['Đang mở','info'],
  RESOLVED:['Đã xử lý','ok'], SENT:['Đã gửi mô phỏng','info'], CLAIMED:['Đã liên kết','ok'],
  HIDDEN:['Đã ẩn','bad'], DISABLED:['Đã tắt','bad']
};
const esc = value => String(value ?? '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
const money = amount => new Intl.NumberFormat('vi-VN',{style:'currency',currency:'VND'}).format(amount || 0);
const formatter = new Intl.DateTimeFormat('vi-VN',{timeZone:'Asia/Ho_Chi_Minh',dateStyle:'medium',timeStyle:'short'});
const dateOnly = new Intl.DateTimeFormat('sv-SE',{timeZone:'Asia/Ho_Chi_Minh',year:'numeric',month:'2-digit',day:'2-digit'});
const fmt = epoch => epoch ? formatter.format(epoch) : '—';
const dayKey = epoch => dateOnly.format(epoch);
const localInput = epoch => {
  const local = new Date(epoch + 7 * hour).toISOString();
  return local.slice(0,16);
};
const parseLocal = value => new Date(value + (value.length === 10 ? 'T00:00:00+07:00' : '+07:00')).getTime();
const uid = (prefix='') => prefix + (crypto.randomUUID?.() || Array.from(crypto.getRandomValues(new Uint8Array(16)), x => x.toString(16).padStart(2,'0')).join('')).replaceAll('-','').toUpperCase();
const row = (title, detail='', buttons='') => '<div class="row"><div><strong>' + esc(title) + '</strong>' + (detail ? '<small>' + esc(detail) + '</small>' : '') + '</div>' + (buttons ? '<div class="cluster">'+buttons+'</div>' : '') + '</div>';
const tag = value => {const [label,color=''] = status[value] || [value]; return '<span class="badge '+color+'" data-status="'+esc(value)+'">'+esc(label)+'</span>';};
const empty = message => '<div class="empty">'+esc(message)+'</div>';
const notice = (message, problem=false) => '<p class="notice'+(problem?' problem':'')+'">'+esc(message)+'</p>';
const opt = (value,label=value,selected='') => '<option value="'+esc(value)+'"'+(String(value)===String(selected)?' selected':'')+'>'+esc(label)+'</option>';
const attr = (name,value) => ' data-'+name+'="'+esc(value)+'"';
const button = (label,action,id='',kind='',extra='') => '<button type="button" class="btn '+kind+'"'+attr('action',action)+attr('id',id)+extra+'>'+esc(label)+'</button>';
const link = (label,path,kind='') => '<a class="'+kind+'" href="#'+esc(path)+'">'+esc(label)+'</a>';
const field = (label,input,wide=false) => '<label class="field'+(wide?' span':'')+'"><span>'+esc(label)+'</span>'+input+'</label>';
const input = (name,value='',type='text',options='') => '<input name="'+name+'" type="'+type+'" value="'+esc(value)+'" '+options+'>';
const select = (name,options,extra='') => '<select name="'+name+'" '+extra+'>'+options+'</select>';
const area = (name,value='',extra='') => '<textarea name="'+name+'" '+extra+'>'+esc(value)+'</textarea>';
const check = (name,label,checked=false,value='yes',required=false) => '<label class="check"><input type="checkbox" name="'+name+'" value="'+esc(value)+'"'+(checked?' checked':'')+(required?' required':'')+'> '+esc(label)+'</label>';
const material = name => Array.from(name?.files || []).filter(file => file.name).map(file => file.name);
const clone = value => structuredClone(value);
const lookup = (records,id) => records.find(record => record.id === id);
const normalized = value => String(value ?? '').trim().toLocaleLowerCase('vi').normalize('NFD').replace(/\p{Diacritic}/gu,'').replaceAll('đ','d');
const matches = (value,needle) => normalized(value).includes(normalized(needle));
const lines = value => String(value ?? '').split(/[\n,;]+/).map(part => part.trim()).filter(Boolean);
const demoAccounts = [
  {id:'member', name:'Minh Anh', email:'minhanh@example.test', phone:'0900000001', role:'user'},
  {id:'buyer2', name:'Ngọc Hà', email:'ngocha@example.test', phone:'0900000002', role:'user'},
  {id:'studio', name:'Studio North', email:'studio@example.test', phone:'0900000003', role:'organizer'},
  {id:'gate', name:'Hoàng Minh', email:'cong@example.test', phone:'0900000004', role:'staff'},
  {id:'ops', name:'Thu Linh', email:'vanhanh@example.test', phone:'0900000005', role:'manager', scope:Object.keys(managerPermissions)},
  {id:'admin', name:'Anh Tuấn', email:'admin@example.test', phone:'0900000006', role:'admin'}
];
const defaultPolicy = {
  serviceRate:3, depositPercent:30, depositHours:24, refundHours:72, settlementDays:3,
  settlementHours:48, postponementDays:7, holdMinutes:10, maxTickets:10,
  rentalEarlyDays:14, rentalEarlyPercent:80, rentalMidDays:7, rentalMidPercent:50,
  holiday:'', authVerify:false, history:true, passwordLength:8, sessionHours:12,
  payoutLimit:500000000, promotionBudget:50000000
};
function seatRows(rows, seats, type) {
  return rows.flatMap((letter,index) => Array.from({length:seats},(_,n) => ({
    code:letter+(n+1), type, group:index===0 && n<2 ? letter+'-đôi' : index===1 && n<3 ? letter+'-cụm' : ''
  })));
}
function startingData() {
  const now = Date.now();
  const base = parseLocal(dayKey(now)) + 18*hour;
  const zones = [
    {name:'VIP', capacity:12, seats:seatRows(['A','B'],6,'VIP')},
    {name:'Standard', capacity:24, seats:seatRows(['C','D','E'],8,'Standard')}
  ];
  const extra = [
    ['Trung tâm Hội nghị Quốc gia','Hà Nội',3500], ['Nhà hát Tuổi trẻ','Hà Nội',1000],
    ['Riverside Hall','TP. Hồ Chí Minh',2000]
  ];
  const locations = [...events.map((e,i) => [e.venue,e.city,[1500,550,400,1000,3000,3500,300,2200][i]]), ...extra]
    .filter((entry,index,all) => all.findIndex(other => other[0]===entry[0])===index)
    .map(([name,city,capacity],i) => ({
      id:'v'+(i+1),name,city,capacity,address:city==='Hà Nội'?'Khu trung tâm, Hà Nội':city,
      rate: [24000000,11000000,9000000,18000000,30000000,60000000,7000000,35000000,38000000][i % 9],
      amenities:'Âm thanh, Ánh sáng, Bãi đỗ xe', blocks:[], zones:clone(zones), state:'ACTIVE'
    }));
  const makeSession = (id, venueId, start, price, name) => ({
    id, venueId, start, end:start+3*hour, from:now-7*day, to:start-15*minute, name,
    zones:clone(zones), open:true, types:[
      {name:'Standard',price,quantity:24,benefits:'Vào cổng và khu vực ghế Standard',from:null,to:null,enabled:true},
      {name:'VIP',price:price+350000,quantity:12,benefits:'Khu vực ưu tiên và lối vào riêng',from:null,to:null,enabled:true},
      {name:'Invitation',price:0,quantity:8,benefits:'Dành cho khách được mời',from:null,to:null,enabled:true}
    ]
  });
  const listings = events.map((event,i) => {
    const {title,cat,description,price} = event;
    const venue = locations.find(venue => venue.name===event.venue);
    const start = base+(20+i*4)*day;
    return {
      id:String(event.id), owner:i===2?'other':'studio', state:'ACTIVE', version:1,
      live:{title,cat,description,program:'Đón khách trước giờ bắt đầu 30 phút.',rules:'Vui lòng mang vé và đến đúng khu vực ghi trên vé.',
        participants:['Nhiều nghệ sĩ điện tử','Nhóm nghệ sĩ đương đại','Các DJ độc lập','Đoàn sân khấu trẻ','Các nghệ sĩ nhạc sống','Diễn giả công nghệ','Nhóm hướng dẫn chuyển động','Các ban nhạc khách mời'][i],
        terms:'Vé có hiệu lực cho suất và khu vực đã chọn.',
        imageKey:Object.keys(images)[i], refund:true, visibleStock:true, venueRequired:false, contact:'studio@example.test',
        sessions:[makeSession('s'+event.id,venue.id,start,price,'Suất chính'),...(i===0?[makeSession('s1b',locations.at(-1).id,start+day,price,'Ngày thứ hai')]:[])]},
      draft:null, versions:[], request:null, notices:[]
    };
  });
  const finished = {
    id:'past',owner:'studio',state:'COMPLETED',version:1,versions:[],notices:[],draft:null,request:null,
    live:{...clone(listings[0].live),title:'AFTERGLOW LIVE',imageKey:'concert',
      sessions:[{...makeSession('spast','v1',base-9*day,300000,'Đêm diễn'),from:base-18*day,to:base-9*day-15*minute}]}
  };
  const reviewTicket = {id:'VN-SAMPLE-PAST',eventId:'past',sessionId:'spast',orderId:'DEMO-PAST',user:'member',seat:'C1',zone:'Standard',type:'Standard',amount:300000,state:'CHECKED_IN',checkedAt:base-9*day};
  return {
    accounts:clone(demoAccounts), events:[...listings,finished], venues:locations, policy:{...defaultPolicy},
    applications:[], rentals:[], reviews:[], support:[], audit:[], scans:[], notifications:[], communications:[],
    favorites:{}, history:{}, resets:[], sessionVersions:{}, flags:{},
    orders:[{id:'DEMO-PAST',user:'member',eventId:'past',sessionId:'spast',state:'PAID',seats:[{code:'C1',zone:'Standard',type:'Standard',price:300000}],created:base-12*day,paidAt:base-12*day,subtotal:300000,discount:0,fee:9000,total:309000,platformFee:24000,contractId:'c1',txn:'TX-DEMO-PAST',refund:true,refundHours:72}],
    tickets:[reviewTicket,{id:'VN-DEMO-QR',eventId:'1',sessionId:'s1',seat:'C2',zone:'Standard',type:'Standard',state:'ISSUED',user:'buyer2'},
      {id:'VN-DEMO-MOI',eventId:'1',sessionId:'s1',seat:'C3',zone:'Standard',type:'Invitation',state:'ISSUED',token:'venue-demo-moi',inviteUntil:base+20*day,contact:'khach@example.test',channel:'Email'}],
    grants:[{id:'grant1',user:'gate',eventId:'1',sessionId:'',permissions:Object.keys(permissionNames),active:true}],
    vouchers:[{id:'promo1',code:'VENUE15',owner:'studio',kind:'percent',value:15,cap:150000,min:200000,from:now-7*day,to:now+45*day,limit:30,perUser:2,state:'ACTIVE',eventId:'',sessionId:'',zone:'',sponsor:'BTC'}],
    contracts:[{id:'c1',owner:'studio',name:'Hợp đồng Studio North',kind:'percent',value:8,from:now-120*day,to:now+365*day,basis:'Doanh thu vé sau giảm',notes:'Mẫu hợp đồng VÉNUE'}],
    refunds:[], settlements:[], payouts:[], transactions:[{id:'TX-DEMO-PAST',orderId:'DEMO-PAST',state:'PAID',amount:309000,time:base-12*day}],
    categories:[...new Set(events.map(event=>event.cat))],
    banners:[{id:'hero',title:'Khám phá sự kiện|đáng nhớ.',text:'Khám phá sự kiện, đặt vé và tìm địa điểm tổ chức tại Việt Nam.',eventId:'1',state:'ACTIVE'}],
    content:{about:'VÉNUE là giao diện tìm sự kiện, đặt vé và thuê không gian tổ chức.', rules:'Kiểm tra suất diễn, giờ mở cửa và nội quy sự kiện trước khi tham dự.', guide:'Tìm sự kiện, chọn suất diễn và ghế, sau đó mở vé trong mục Vé của tôi.'},
    settings:{domain:'venue.example.test', smtp:'Chưa cấu hình', sms:'Chưa cấu hình', payos:'Mô phỏng', https:true, twoFactor:false, protection:'Chưa cấu hình', version:1},
    configHistory:[], alerts:[], backups:[]
  };
}
let memory;
let memoryOnly=false;
function load() {
  if (memoryOnly&&memory) return clone(memory);
  try {
    const stored = localStorage.getItem(storeKey);
    if (stored) return JSON.parse(stored);
  } catch {}
  return memory ? clone(memory) : startingData();
}
function save() {
  memory = clone(db);
  try {
    localStorage.setItem(storeKey,JSON.stringify(db));
    memoryOnly=false;
  } catch {
    if (!memoryOnly) notifyUi('Không thể lưu vào bộ nhớ trình duyệt. Dữ liệu mới chỉ giữ trong tab này.');
    memoryOnly=true;
  }
}
let db = load();
let auth;
try {auth=JSON.parse(sessionStorage.getItem(storeKey+'-auth'));} catch {}
let route = [];
let currentModal = null;
let homeFilters = {};
const rentalSelection = new Set();
let eventPage = 1;
let eventFilters = {};
let venueFilters = {};
let selection = {eventId:'', sessionId:'', zone:'', seats:[]};
let staffScope = {eventId:'1',sessionId:'s1'};
let staffResult = '';
let staffLookup = '';
let editorId = '';
let editorSection = 'general';
let venueEditorId = '';
let toastTimer;
let actionRunning = false;
function notifyUi(message) {
  snackbar.textContent=message;
  clearTimeout(toastTimer);
  toastTimer=setTimeout(()=>{snackbar.textContent='';},4200);
}
function setAuth(account) {
  if (auth?.id!==account?.id) selection={eventId:'',sessionId:'',zone:'',seats:[]};
  auth = account ? {id:account.id,until:Date.now()+db.policy.sessionHours*hour,version:db.sessionVersions[account.id]||0} : null;
  try {sessionStorage.setItem(storeKey+'-auth',JSON.stringify(auth));} catch {}
}
function account() {
  const person=auth && lookup(db.accounts,auth.id);
  return person && !person.locked && auth.until>Date.now() && auth.version===(db.sessionVersions[person.id]||0) ? person : null;
}
function audit(action, subject, detail='') {
  db.audit.unshift({id:uid(),time:Date.now(),actor:account()?.id || 'worker',action,subject,detail});
  db.audit=db.audit.slice(0,600);
}
function message(user,title,detail='',channel='Trong ứng dụng') {
  if (!user) return;
  db.notifications.unshift({id:uid(),user,title,detail,channel,time:Date.now(),read:false});
}
function sendMock(channel,target,subject,body) {
  db.communications.unshift({id:uid(),channel,target,subject,body,time:Date.now(),state:'SENT'});
}
function sessionOf(event,id,source='live') {
  return event?.[source]?.sessions.find(s=>s.id===id);
}
function eventTitle(id) {
  const event=lookup(db.events,id);
  return event?.live?.title || event?.draft?.title || 'Sự kiện';
}
function venueName(id) {return lookup(db.venues,id)?.name || 'Địa điểm';}
function publicEvent(event) {return event?.live && ['ACTIVE','POSTPONED','CANCELLED','COMPLETED'].includes(event.state);}
function sellable(event, ses, now=Date.now()) {
  return event?.state==='ACTIVE' && ses?.open && now>=ses.from && now<ses.to && now<ses.start;
}
function saleType(ses,name,now=Date.now()) {
  return ses?.types.find(type=>type.name===name && type.enabled && (!type.from || now>=type.from) && (!type.to || now<type.to));
}
const validTicket = ticket => ['ISSUED','CHECKED_IN'].includes(ticket.state);
const heldOrder = (order,now=Date.now()) => order.state==='PENDING_PAYMENT' && order.expires>now;
function inventory(sessionId,exclude='',now=Date.now()) {
  const map = new Map();
  db.tickets.filter(t=>t.sessionId===sessionId && validTicket(t)).forEach(t=>map.set(t.seat,{state:t.type==='Invitation'?'invite':'sold',zone:t.zone,type:t.type}));
  db.orders.filter(o=>o.sessionId===sessionId && o.id!==exclude && heldOrder(o,now)).forEach(o=>o.seats.forEach(s=>map.set(s.code,{state:'hold',zone:s.zone,type:s.type})));
  return map;
}
function quantityUsed(user,sessionId,exclude,now=Date.now()) {
  return db.tickets.filter(t=>t.user===user && t.sessionId===sessionId && t.type!=='Invitation' && validTicket(t)).length +
    db.orders.filter(o=>o.id!==exclude && o.user===user && o.sessionId===sessionId && heldOrder(o,now)).reduce((sum,o)=>sum+o.seats.length,0);
}
function inventoryError(ses,choices,exclude='',now=Date.now()) {
  if (!ses || !choices.length) return 'Hãy chọn ghế.';
  const bag=inventory(ses.id,exclude,now);
  const clashes=choices.filter(seat=>bag.has(seat.code)||!ses.zones.find(z=>z.name===seat.zone)?.seats.some(item=>item.code===seat.code&&!item.blocked));
  if (clashes.length) return 'Ghế không còn khả dụng: '+clashes.map(s=>s.code).join(', ')+'.';
  const occupied=[...bag.values(),...choices];
  for (const zone of ses.zones) {
    if (occupied.filter(seat=>seat.zone===zone.name).length>zone.capacity) return 'Khu '+zone.name+' đã đạt sức chứa.';
  }
  for (const type of ses.types) {
    if (occupied.filter(seat=>seat.type===type.name).length>type.quantity) return 'Loại vé '+type.name+' đã hết số lượng.';
  }
  return '';
}
function sessionFree(ses) {
  const bag=inventory(ses.id);
  const occupied=[...bag.values()];
  const availableByType=new Map(ses.types.map(type=>[type.name,Math.max(0,type.quantity-occupied.filter(s=>s.type===type.name).length)]));
  let total=0;
  for (const zone of ses.zones) {
    let slots=Math.max(0,zone.capacity-occupied.filter(seat=>seat.zone===zone.name).length);
    for (const seat of zone.seats) {
      const remaining=availableByType.get(seat.type)||0;
      if (!slots || !remaining || seat.blocked || bag.has(seat.code) || !saleType(ses,seat.type)) continue;
      slots--;total++;
      availableByType.set(seat.type,remaining-1);
    }
  }
  return total;
}
function contractFor(owner,now=Date.now()) {return db.contracts.find(c=>c.owner===owner && c.from<=now && c.to>now);}
function financeSnapshot(order,event) {
  const contract=order.contractSnapshot===undefined ? contractFor(event.owner) : order.contractSnapshot;
  order.contractId=contract?.id||'';
  order.contractSnapshot=contract ? clone(contract) : null;
  order.platformFee=contract?.kind==='fixed' ? contract.value*order.seats.length : Math.round((order.subtotal-order.discount)*(contract?.value||0)/100);
}
function voucherError(voucher,order,now=Date.now()) {
  if (!voucher || voucher.state!=='ACTIVE' || now<voucher.from || now>=voucher.to) return 'Mã không hợp lệ hoặc đã hết hiệu lực.';
  if ((voucher.eventId && voucher.eventId!==order.eventId) || (voucher.sessionId && voucher.sessionId!==order.sessionId) || (voucher.zone && order.seats.some(s=>s.zone!==voucher.zone))) return 'Mã không áp dụng cho sự kiện, suất hoặc khu vực đã chọn.';
  if (voucher.owner!=='admin' && voucher.owner!==lookup(db.events,order.eventId)?.owner) return 'Mã thuộc một Ban tổ chức khác.';
  if (order.subtotal<voucher.min) return 'Đơn cần đạt tối thiểu '+money(voucher.min)+'.';
  const reserved=db.orders.filter(o=>o.id!==order.id && o.voucherId===voucher.id && (o.voucherUsed || heldOrder(o,now)));
  if (reserved.length>=voucher.limit || reserved.filter(o=>o.user===order.user).length>=voucher.perUser) return 'Mã đã hết lượt áp dụng.';
  if (reserved.reduce((sum,o)=>sum+o.discount,0)+voucherDiscount(voucher,order)>db.policy.promotionBudget) return 'Chiến dịch đã hết ngân sách ưu đãi.';
  return '';
}
function voucherDiscount(voucher,order) {
  const amount=voucher.kind==='percent' ? Math.min(Math.round(order.subtotal*voucher.value/100),voucher.cap||Infinity) : voucher.value;
  return Math.min(amount,order.subtotal);
}
function updateTotals(order,voucher) {
  order.voucherId=voucher?.id || '';
  order.discount=voucher ? voucherDiscount(voucher,order) : 0;
  order.total=order.subtotal-order.discount+order.fee;
}
function settledRefundAmount(orderId) {
  return db.refunds.filter(r=>r.state==='COMPLETED').reduce((sum,r)=>sum+refundForOrder(r,orderId),0);
}
function queueRefund(data) {
  const refund={id:uid('RF-').slice(0,15),created:Date.now(),state:'MANUAL_REVIEW',...data};
  db.refunds.unshift(refund);
  message(refund.user,'Yêu cầu hoàn tiền '+refund.id,money(refund.amount)+' · '+(status[refund.state]?.[0]||refund.state));
  audit('Tạo yêu cầu hoàn',refund.id,refund.reason);
  return refund;
}
function confirmPayment(order,scenario,channel='PayOS') {
  if (!order || !['PENDING_PAYMENT','EXPIRED','PAID','PAID_OUT_OF_STOCK'].includes(order.state)) return 'Đơn không thể nhận thanh toán.';
  const txn='TX-'+order.id;
  if (db.transactions.some(t=>t.id===txn)) {
    audit('Phản hồi thanh toán trùng',order.id);
    return 'Giao dịch đã ghi nhận. Số vé giữ nguyên.';
  }
  if (scenario==='failed' || scenario==='review') {
    db.transactions.unshift({id:uid('ATT-').slice(0,16),orderId:order.id,state:scenario==='failed'?'FAILED':'MANUAL_REVIEW',amount:order.total,time:Date.now(),channel});
    if (scenario==='failed') {
      order.state='CANCELLED';
      message(order.user,'Giao dịch thất bại',order.id+' · Ghế và lượt ưu đãi đã được giải phóng.');
    }
    audit('Đối soát thanh toán',order.id,scenario);
    return scenario==='failed'?'Thanh toán thất bại. Ghế đã được giải phóng.':'Giao dịch đã vào hàng đợi đối soát.';
  }
  const now=Date.now();
  const event=lookup(db.events,order.eventId);
  const ses=sessionOf(event,order.sessionId);
  const late=order.state==='EXPIRED'||order.expires<=now||scenario.startsWith('late');
  if (late) order.state='EXPIRED';
  if (scenario==='late-conflict' && ses && !inventoryError(ses,[order.seats[0]]) &&
    quantityUsed('buyer2',ses.id,'',now)<db.policy.maxTickets) {
    const seat=order.seats[0];
    db.tickets.unshift({id:uid('VN-').slice(0,18),user:'buyer2',eventId:order.eventId,sessionId:ses.id,seat:seat.code,zone:seat.zone,type:seat.type,state:'ISSUED'});
  }
  const issueError=!ses || !['ACTIVE','POSTPONED'].includes(event.state)||ses.start<=now ||
    inventoryError(ses,order.seats,order.id,now) ||
    quantityUsed(order.user,ses.id,order.id,now)+order.seats.length>db.policy.maxTickets ||
    (late && order.voucherId && voucherError(lookup(db.vouchers,order.voucherId),order,now));
  order.txn=txn;
  order.channel=channel;
  order.paidAt=now;
  order.state=issueError?'PAID_OUT_OF_STOCK':'PAID';
  db.transactions.unshift({id:txn,orderId:order.id,state:order.state,amount:order.total,time:now,late,channel});
  if (issueError) {
    queueRefund({orderId:order.id,eventId:order.eventId,user:order.user,reason:'Thanh toán đến khi đơn không thể phát hành vé',amount:order.total,priority:true,state:'AUTO_APPROVED'});
    message(order.user,'Đã thu tiền, cần hoàn toàn bộ',order.id+' · '+money(order.total));
  } else {
    order.voucherUsed=!!order.voucherId;
    for (const seat of order.seats) {
      db.tickets.unshift({id:uid('VN-').slice(0,18),eventId:order.eventId,sessionId:order.sessionId,orderId:order.id,user:order.user,seat:seat.code,zone:seat.zone,type:seat.type,amount:seat.price,state:'ISSUED'});
    }
    message(order.user,'Vé đã phát hành',eventTitle(order.eventId)+' · '+order.id);
    const email=lookup(db.accounts,order.user)?.email;
    if (email) sendMock('Email',email,'Vé '+order.id,'Mở Vé của tôi để xem QR và lưu PDF.');
  }
  audit('Xác nhận thanh toán',order.id,order.state);
  return issueError?'Đơn đã vào luồng ưu tiên hoàn 100%.':'Đã phát hành vé.';
}
function expire(now=Date.now()) {
  let changed=false;
  for (const order of db.orders) {
    if (order.state!=='PENDING_PAYMENT'||order.expires>now) continue;
    order.state='EXPIRED';
    changed=true;
    message(order.user,'Hết hạn giữ ghế',order.id+' · Ghế và lượt voucher đã được trả.');
    audit('Giải phóng giữ ghế',order.id);
  }
  for (const rental of db.rentals) {
    if (rental.state!=='AWAITING_DEPOSIT'||rental.deadline>now) continue;
    rental.state='EXPIRED';
    changed=true;
    message(rental.user,'Hết hạn đặt cọc',venueName(rental.venueId)+' · Khung giờ đã được giải phóng.');
    audit('Giải phóng lịch thuê',rental.id);
  }
  return changed;
}
const rentalBlocking = rental => ['DEPOSIT_REVIEW','CONFIRMED','CANCELLATION_REQUESTED','ESCALATED'].includes(rental.state) ||
  (rental.state==='AWAITING_DEPOSIT' && rental.deadline>Date.now());
function rentalConflict(venue,start,end,ignore='') {
  if (!venue) return false;
  const linkedEvent=ignore?lookup(db.rentals,ignore)?.eventId:'';
  return venue.blocks.some(block=>start<block.end&&end>block.start) ||
    db.rentals.some(r=>r.id!==ignore&&r.venueId===venue.id&&rentalBlocking(r)&&start<r.end&&end>r.start) ||
    eventVenueConflict(venue.id,start,end,linkedEvent);
}
function eventVenueConflict(venueId,start,end,ignoreEvent='') {
  return db.events.some(event=>event.id!==ignoreEvent&&event.state!=='CANCELLED'&&
    event.live?.sessions.some(s=>s.venueId===venueId&&start<s.end&&end>s.start));
}
function refundable(ticket) {
  const event=lookup(db.events,ticket.eventId);
  const ses=sessionOf(event,ticket.sessionId);
  const order=lookup(db.orders,ticket.orderId);
  if (!order || !ses || ticket.state!=='ISSUED') return '';
  if (event.state==='POSTPONED' && event.refundUntil>Date.now()) return 'postponed';
  if (order.refund && ['ACTIVE','POSTPONED'].includes(event.state) && ses.start-Date.now()>=(order.refundHours||72)*hour) return 'voluntary';
  return '';
}
function amountForTicket(order,ticket,full) {
  const index=order.seats.findIndex(s=>s.code===ticket.seat);
  if (index<0) return 0;
  const orderValue=full ? order.total : order.subtotal;
  const previous=order.seats.slice(0,index).reduce((sum,s)=>sum+s.price,0);
  return order.subtotal ? Math.round(orderValue*(previous+order.seats[index].price)/order.subtotal) - Math.round(orderValue*previous/order.subtotal) :
    Math.floor(orderValue/order.seats.length)+(index<orderValue%order.seats.length?1:0);
}
function canStaff(permission,eventId,sessionId='') {
  return account()?.role==='staff' && db.grants.some(grant=>grant.user===account().id && grant.active &&
    grant.eventId===eventId && (!sessionId || !grant.sessionId || grant.sessionId===sessionId) && grant.permissions.includes(permission));
}
function canManage(tab) {
  const user=account();
  return user?.role==='admin'||(user?.role==='manager'&&user.scope?.includes(tab));
}
function ownerEvent(event) {return account()?.role==='organizer' && event?.owner===account().id;}
function canOperate(permission,eventId,sessionId) {
  return ownerEvent(lookup(db.events,eventId))||canStaff(permission,eventId,sessionId);
}
function needsVerified(user) {return db.policy.authVerify&&user?.role==='user'&&!user.verified;}
function assert(condition,message) {if (!condition) throw new Error(message);}
function faq() {
  const p=db.policy;
  return [
    ['Tôi có bao lâu để thanh toán vé?','Ghế và lượt voucher được giữ '+p.holdMinutes+' phút. Mỗi tài khoản được đặt tối đa '+p.maxTickets+' vé trên một suất.'],
    ['Khi nào và làm sao để hoàn vé?', 'Nếu sự kiện cho phép, bạn có thể chọn nhiều vé chưa check-in trong mục Vé của tôi và gửi cùng một lý do trước ít nhất '+p.refundHours+' giờ. Nếu sự kiện bị hủy, bạn được hoàn toàn bộ số tiền đã trả.'],
    ['Tôi phải cọc bao nhiêu khi thuê?', 'Cọc '+p.depositPercent+'% trong '+p.depositHours+' giờ sau khi được duyệt giữ lịch. Hủy trước ít nhất '+p.rentalEarlyDays+' ngày hoàn '+p.rentalEarlyPercent+'% cọc, từ '+p.rentalMidDays+' ngày đến dưới '+p.rentalEarlyDays+' ngày hoàn '+p.rentalMidPercent+'%, và dưới '+p.rentalMidDays+' ngày hoàn 0%.']
  ];
}
function workDeadline(time,days) {
  const holidays=lines(db.policy.holiday);
  let date=time;
  while(days>0) {
    date+=day;
    const weekday=new Date(date+7*hour).getUTCDay();
    if (weekday!==0 && weekday!==6 && !holidays.includes(dayKey(date))) days--;
  }
  return date;
}
function currencyNumber(form,key) {
  const value=Number(form.get(key)||0);
  assert(Number.isFinite(value)&&value>=0,'Giá trị tiền hoặc số lượng không hợp lệ.');
  return value;
}
function requireFuture(start,end) {
  assert(Number.isFinite(start)&&Number.isFinite(end)&&start<end,'Khung thời gian không hợp lệ.');
}
function qrMarkup(value,label) {
  if (typeof qrcode!=='function') return '<code>'+esc(value)+'</code>';
  const qr=qrcode(0,'M');
  qr.addData(value);
  qr.make();
  const margin=4;
  const count=qr.getModuleCount();
  const canvas=document.createElement('canvas');
  canvas.width=canvas.height=(count+2*margin)*5;
  const ctx=canvas.getContext('2d');
  ctx.fillStyle='#fff';
  ctx.fillRect(0,0,canvas.width,canvas.height);
  ctx.fillStyle='#151129';
  for(let y=0;y<count;y++) for(let x=0;x<count;x++) if(qr.isDark(y,x)) ctx.fillRect((x+margin)*5,(y+margin)*5,5,5);
  return '<img class="qr" src="'+canvas.toDataURL()+'" alt="'+esc(label)+'">';
}


// Khám phá, đặt vé và tài khoản

function pageTitle(title,description='',actions='') {
  return `<div class="title-block split"><div><h1>${esc(title)}</h1>${description?'<p class="muted">'+esc(description)+'</p>':''}</div>${actions}</div>`;
}
function panel(title,body) {
  return `<section class="panel">${title?'<h3>'+esc(title)+'</h3>':''}${body}</section>`;
}
function isUploadedCover(cover) {
  return typeof cover==='string'&&/^data:image\/webp;base64,[a-z0-9+/=]+$/i.test(cover);
}
function imageFor(key,cover) {
  if (isUploadedCover(cover)) return cover;
  return Object.hasOwn(images,key)?images[key]:images.festival;
}
function organizerName(event) {return lookup(db.accounts,event.owner)?.name || 'Indika Events';}
function navLink(label,path,active) {
  return `<a href="#${esc(path)}"${active?' aria-current="page"':''}>${esc(label)}</a>`;
}
function header() {
  const user=account();
  const unread=db.notifications.filter(n=>n.user===user?.id&&!n.read).length;
  const [page]=route;
  return `<header class="site-header${page==='home'?' home-header':''}"><div class="wrap nav">
    <a href="#home" class="logo" aria-label="Vénue, trang chủ"><span class="logo-mark" aria-hidden="true">✳</span>vénue</a>
    <nav class="nav-links" aria-label="Menu chính">
      ${navLink('Trang chủ','home',page==='home')}${navLink('Sự kiện','events',page==='events'||page==='event')}${navLink('Lịch','calendar',page==='calendar')}
      ${navLink('Địa điểm','venues',page==='venues'||page==='venue')}${navLink('Hướng dẫn','guide',page==='guide')}
      ${user&&user.role!=='user'?navLink('Vận hành','work',page==='work'):''}
    </nav>
    <div class="nav-actions">
      ${button('Vai trò mẫu','demo','','ghost compact demo-button')}
      ${user?navLink('Vé của tôi','account/tickets',page==='account'&&route[1]==='tickets'):''}
      ${user?`<a class="nav-notifications" href="#account/notifications" aria-label="Thông báo${unread?', '+unread+' chưa đọc':''}">${ic('bell',19)}${unread?' <small>'+unread+'</small>':''}</a>`:''}
      ${user?`<details class="nav-account"><summary class="btn secondary compact">${ic('user',16)} <span>${esc(user.name)}</span></summary><div class="nav-popover">
        ${link('Hồ sơ cá nhân','account/profile')}${link('Sự kiện yêu thích','account/favorites')}
        ${link('Yêu cầu hỗ trợ','account/support')}${button('Đăng xuất','logout','','ghost compact')}
      </div></details>`:button('Đăng nhập','login','','compact')}
    </div></div></header>`;
}
function footer() {
  const user=account();
  return `<footer class="footer"><div class="wrap">
    <div class="footer-grid">
      <div class="footer-brand">
        <a href="#home" class="logo" aria-label="Vénue, trang chủ"><span class="logo-mark" aria-hidden="true">✳</span>vénue</a>
        <p>Tìm sự kiện bạn yêu thích, đặt vé và chọn không gian để tổ chức sự kiện của riêng bạn.</p>
        ${user&&user.role!=='user'?link('Vào khu vực vận hành','work','btn footer-outline'):
          link('Đăng ký Ban tổ chức','account/application','btn footer-outline')}
      </div>
      <nav class="footer-group" aria-labelledby="footer-discovery">
        <h2 id="footer-discovery">Khám phá</h2>
        <ul>
          <li>${link('Tất cả sự kiện','events')}</li>
          <li>${link('Lịch sự kiện','calendar')}</li>
          <li>${link('Địa điểm tổ chức','venues')}</li>
          <li>${link('Sự kiện yêu thích','account/favorites')}</li>
        </ul>
      </nav>
      <nav class="footer-group" aria-labelledby="footer-booking">
        <h2 id="footer-booking">Vé và đặt chỗ</h2>
        <ul>
          <li>${link('Vé của tôi','account/tickets')}</li>
          <li>${link('Lịch sử đơn hàng','account/orders')}</li>
          <li>${link('Theo dõi hoàn tiền','account/refunds')}</li>
          <li>${link('Hồ sơ thuê địa điểm','account/rentals')}</li>
        </ul>
      </nav>
      <section class="footer-group footer-support" aria-labelledby="footer-support">
        <h2 id="footer-support">Đội hỗ trợ VÉNUE</h2>
        <button type="button" class="btn lime" data-action="support">${ic('chat',18)} Nhắn hỗ trợ</button>
        <ul>
          <li>${link('Yêu cầu hỗ trợ đã gửi','account/support')}</li>
          <li>${link('Câu hỏi thường gặp','guide/faq')}</li>
          ${supportContactLinks().map(contact=>'<li>'+contact+'</li>').join('')}
        </ul>
      </section>
    </div>
    <div class="footer-bottom">
      <nav aria-label="Thông tin VÉNUE">
        ${link('Giới thiệu','guide/about')}${link('Hướng dẫn đặt vé','guide/tickets')}${link('Quy định và chính sách','guide/rules')}
      </nav>
      <button type="button" class="footer-top" data-footer-top>${ic('chevron',17)} Về đầu trang</button>
    </div>
  </div></footer>`;
}
function eventDate(event,sessions=event.live.sessions) {
  if (!sessions.length) return 'Chưa có lịch';
  const first=[...sessions].sort((a,b)=>a.start-b.start)[0];
  return fmt(first.start)+(sessions.length>1?' · '+sessions.length+' suất':'');
}
function eventCard(event,sessions=event.live.sessions,band) {
  const info=event.live;
  const id=event.id;
  const favorite=(db.favorites[account()?.id]||[]).includes(id);
  const first=[...sessions].sort((a,b)=>a.start-b.start)[0];
  const venue=lookup(db.venues,first?.venueId);
  const prices=sessions.flatMap(s=>publicTicketPrices(s,band));
  const price=prices.length?Math.min(...prices):null;
  return `<article class="event-card">
    <a href="#event/${esc(id)}" class="event-image" aria-label="${esc(info.title)}">
      <img src="${imageFor(info.imageKey,info.cover)}" alt="" loading="lazy">
      <span class="card-tag">${esc(['POSTPONED','CANCELLED','COMPLETED'].includes(event.state)?status[event.state][0]:info.cat)}</span>
    </a>
    <button type="button" class="favorite" aria-label="${favorite?'Bỏ yêu thích':'Lưu yêu thích'}" aria-pressed="${favorite}" data-action="favorite" data-id="${esc(id)}">${ic('heart',17)}</button>
    <div class="card-copy"><h3><a href="#event/${esc(id)}">${esc(info.title)}</a></h3>
    <div class="event-meta">${ic('calendar',15)}${esc(eventDate(event,sessions))}</div>
    <div class="event-meta">${ic('pin',15)}${esc(venue?.city||'Địa điểm sẽ cập nhật')}</div>
    <div class="card-price">${price==null?'Xem sự kiện':price===0?'Miễn phí':'Từ '+money(price)}</div></div></article>`;
}
function eventListView() {
  const f=eventFilters;
  const cities=venueCities();
  const results=f.upcoming?upcomingEvents(f):filteredEvents(f);

  const maxPage=Math.max(1,Math.ceil(results.length/8));
  eventPage=Math.min(eventPage,maxPage);
  return `${pageTitle('Khám phá sự kiện','Lọc theo chủ đề, địa điểm, lịch diễn và giá vé.',link('Xem lịch theo tháng','calendar','btn secondary compact'))}
  <form class="panel compact-filters" data-form="eventFilters">
    ${field('Từ khóa',input('query',f.query,'search','placeholder="Tên, mô tả, địa điểm"'))}
    ${field('Danh mục',select('cat',opt('','Tất cả')+db.categories.map(cat=>opt(cat,cat,f.cat)).join('')))}
    ${field('Thành phố',select('city',opt('','Tất cả')+cities.map(city=>opt(city,city,f.city)).join('')))}
    ${field('Khoảng giá',priceBandSelect(f.price))}
    <button class="btn" type="submit">Lọc sự kiện</button>
    <details class="span filter-more"${f.venueId||f.date||f.session||f.organizer||f.stock||f.upcoming?' open':''}><summary>Lọc thêm</summary><div class="form-grid">
      ${field('Địa điểm',select('venueId',opt('','Tất cả')+db.venues.filter(v=>v.state==='ACTIVE').map(v=>opt(v.id,v.name,f.venueId)).join('')))}
      ${field('Ngày diễn',input('date',f.date,'date'))}
      ${field('Suất diễn',input('session',f.session,'search','placeholder="Tên suất hoặc giờ"'))}
      ${field('Ban tổ chức',input('organizer',f.organizer))}
      <div class="span">${check('stock','Chỉ xem suất còn vé',!!f.stock)}</div>
      <div class="span">${check('upcoming','Chỉ xem sự kiện sắp diễn ra',!!f.upcoming)}</div>
    </div></details>
    ${Object.values(f).some(Boolean)?'<div class="span">'+button('Xóa bộ lọc','clearEvents','','secondary compact')+'</div>':''}
  </form>
  <div class="results muted" role="status">${results.length} sự kiện</div>
  ${results.length?'<div class="event-grid">'+results.slice((eventPage-1)*8,eventPage*8).map(({event,sessions,band})=>eventCard(event,sessions,band)).join('')+'</div>':empty('Không tìm thấy sự kiện phù hợp.')}
  ${maxPage>1?`<nav class="cluster pagination" aria-label="Phân trang sự kiện">
  ${button('Trang trước','eventPage',eventPage-1,'secondary',eventPage===1?' disabled':'')}<span>Trang ${eventPage} / ${maxPage}</span>${button('Trang sau','eventPage',eventPage+1,'secondary',eventPage===maxPage?' disabled':'')}</nav>`:''}`;
}
function quickActions(event,availableSessions) {
  const id=event?.id||'';
  let ticket='';
  if (availableSessions?.size) {
    ticket=`<button type="button" class="btn compact" data-action="quickBooking"${attr('id',id)}>${ic('ticket',17)} ${availableSessions.size===1?'Chọn ghế':'Đặt vé'}</button>`;
  } else if (availableSessions&&event.live.sessions.length) {
    ticket=`<a class="btn compact" href="#${esc(eventPath(id,'tickets'))}">${ic('calendar',17)} Lịch diễn</a>`;
  }
  return `<div class="quick-actions" role="group" aria-label="Thao tác nhanh">
    <button type="button" class="btn secondary compact" data-action="support"${attr('event',id)} aria-label="Yêu cầu hỗ trợ">${ic('chat',17)} Hỗ trợ</button>
    ${ticket}
  </div>`;
}
function detailView(event,availableSessions) {
  if (!event) return pageTitle('Sự kiện không khả dụng')+link('Xem sự kiện khác','events','btn');
  const {id,live:e}=event;
  const related=db.events.filter(item=>item.id!==id&&publicEvent(item)&&item.state==='ACTIVE'&&item.live.cat===e.cat).slice(0,4);
  const ticketLink=e.sessions.length?link(availableSessions.size?'Xem suất và chọn ghế':'Xem lịch diễn',eventPath(id,'tickets'),'btn lime compact'):'';
  return `${link('Khám phá sự kiện','events','back')}
    <section class="detail-cover"><img src="${imageFor(e.imageKey,e.cover)}" alt=""><p>${esc(e.cat)} · ${esc(organizerName(event))}</p><h1>${esc(e.title)}</h1>
      <div class="split event-detail-heading"><p>${esc(eventDate(event))}</p><div class="cluster event-detail-actions">
        ${ticketLink}${button('Chia sẻ sự kiện','shareEvent',id,'ghost compact')}
      </div></div>
    </section>
    ${event.notices.length?'<div class="stack event-notices">'+event.notices.map(item=>notice(item.text)).join('')+'</div>':''}
    <div class="layout event-detail-layout">
      <section id="event/${esc(id)}/tickets" class="panel event-tickets" aria-labelledby="event-tickets-title" tabindex="-1">
        <h2 id="event-tickets-title">Lịch diễn và vé</h2>
        ${e.sessions.map(s=>{
          const venue=lookup(db.venues,s.venueId);
          const activeVenue=venue?.state==='ACTIVE';
          const saveToCalendar=canSaveSession(event,s);
          const venueLabel=activeVenue?link(venue.name,'venue/'+encodeURIComponent(venue.id),'session-venue-link'):esc(venueName(s.venueId));
          const typeList=s.types.filter(t=>t.name!=='Invitation'&&t.enabled).map(t=>`<div class="row"><div><strong>${esc(t.name)} · ${money(t.price)}</strong><small>${esc(t.benefits)}</small><small>${esc(fmt(t.from||s.from))} đến ${esc(fmt(t.to||s.to))}</small></div></div>`).join('');
          return `<section class="stack session-detail" id="event/${esc(id)}/${esc(s.id)}" tabindex="-1"><div><strong>${esc(s.name)}</strong>
          <p class="muted small">${esc(fmt(s.start))}–${esc(fmt(s.end))}<br>${venueLabel}${activeVenue&&venue.address?'<br>'+esc(venue.address):''}</p></div>${typeList}
          <p class="small muted">${esc(s.zones.map(z=>z.name+' · '+z.capacity+' chỗ').join(' / '))}${e.visibleStock?' · '+sessionFree(s)+' ghế khả dụng':''}</p>
          <div class="cluster">${button('Xem sơ đồ','map',s.id,'secondary compact',attr('event',id))}${availableSessions.has(s)?link('Chọn ghế',bookPath(id,s.id),'btn compact'):tag(event.state==='ACTIVE'?'Đóng bán / hết vé':event.state)}</div>
          ${activeVenue||saveToCalendar?`<div class="cluster">${activeVenue?venueDirections(venue):''}
            ${saveToCalendar?button('Thêm vào lịch','exportSessionCalendar',s.id,'secondary compact',attr('event',id)):''}</div>`:''}</section>`;
        }).join('')||empty('Lịch sẽ được cập nhật.')}
      </section>
      <div class="stack event-overview">
        ${panel('Về sự kiện',`<div class="stack"><p>${esc(e.description)}</p><div><strong>Chương trình</strong><p>${esc(e.program)}</p></div>${e.participants?'<div><strong>Nghệ sĩ / diễn giả</strong><p>'+esc(e.participants)+'</p></div>':''}${e.terms?'<div><strong>Điều khoản</strong><p>'+esc(e.terms)+'</p></div>':''}<div><strong>Quy định tham dự</strong><p>${esc(e.rules)}</p></div><div><strong>Điều kiện hoàn vé</strong><p>${e.refund?'Có thể yêu cầu hủy trước '+db.policy.refundHours+' giờ nếu vé chưa sử dụng. Phí dịch vụ theo điều kiện lúc mua.':'Sự kiện không áp dụng tự hủy vé.'}</p></div></div>`)}
        ${eventReviews(event)}
      </div>
    </div>${related.length?`<section class="home-section"><h2 class="section-head">Sự kiện liên quan</h2><div class="event-grid">${related.map(event=>eventCard(event)).join('')}</div></section>`:''}`;
}
function venueDetails(id) {
  const venue=lookup(db.venues,id);
  if (venue?.state!=='ACTIVE') return pageTitle('Địa điểm không khả dụng')+link('Xem địa điểm khác','venues','btn');
  const first=parseLocal(dayKey(Date.now()))+day;
  const week=Array.from({length:7},(_,i)=>first+i*day);
  const upcoming=upcomingEvents({venueId:id});
  const photo=venuePhoto(venue);
  const license=photo.license?`<a href="https://creativecommons.org/licenses/${photo.license}/">${photo.licenseLabel}</a>`:photo.licenseLabel;
  const address=matches(venue.address,venue.city)?venue.address:venue.address+' · '+venue.city;
  return `${link('Danh sách địa điểm','venues','back')}${pageTitle(venue.name,address,venueDirections(venue))}
    <div class="layout"><div class="stack">
    <figure class="venue-photo">
      <img src="${esc(photo.src)}" alt="${photo.illustration?'':esc(venue.name)}">
      ${photo.illustration?'<figcaption>Ảnh minh họa</figcaption>':photo.source?
        `<figcaption>Ảnh <a href="${photo.source}">${esc(photo.author)}</a> · ${license} · Đã thu nhỏ và chuyển WebP</figcaption>`:''}
    </figure>
    ${panel('Thông tin địa điểm',row('Sức chứa',venue.capacity+' khách')+row('Tiện ích',venue.amenities)+row('Chi phí tham khảo',money(venue.rate)+' / khung'))}
    ${panel('Khu vực và sơ đồ tổng quan',mapView(venue.zones))}
    </div><div class="stack">
    ${panel('Lịch 7 ngày tới',week.map(start=>row(dayKey(start),'',tag(rentalConflict(venue,start,start+day)?'Có lịch sử dụng':'Trống'))).join('')+
      '<div style="margin-top:14px">'+button('Kiểm tra khung giờ khác','venueCalendar',id,'secondary')+'</div>')}
    ${panel('Yêu cầu thuê',`<div class="stack"><p>${esc(faq()[2][1])}</p>${button('Gửi yêu cầu thuê','rental',id)}${button(rentalSelection.has(id)?'Bỏ chọn địa điểm':'Thêm vào danh sách thuê','toggleRental',id,'secondary')}${link('Chọn thêm địa điểm','venues','btn ghost compact')}</div>`)}
    </div></div>
    <section class="home-section" aria-labelledby="venue-upcoming-title"><div class="section-head"><h2 id="venue-upcoming-title">Sự kiện sắp diễn ra tại đây</h2></div>
    ${upcoming.length?'<div class="event-grid">'+upcoming.map(({event,sessions})=>eventCard(event,sessions)).join('')+'</div>':empty('Địa điểm này chưa có sự kiện sắp diễn ra.')}</section>`;
}
function guideView() {
  const section=route[1];
  const card=(key,title,body)=>`<div id="guide/${key}" class="guide-anchor" tabindex="-1">${panel(title,body)}</div>`;
  return `${pageTitle(section==='rules'?'Quy định và chính sách':'Hướng dẫn sử dụng VÉNUE')}
  <div class="layout"><div class="stack">
    ${card('about','Giới thiệu','<p>'+esc(db.content.about)+'</p>')}
    ${card('tickets','Cách đặt vé','<p>'+esc(db.content.guide)+'</p>')}
    ${card('rules','Quy định','<p>'+esc(db.content.rules)+'</p>')}
  </div>${card('faq','Câu hỏi thường gặp',commonQuestions(faq()))}</div>`;
}
function mapView(zones) {
  return '<div class="seat-screen">SÂN KHẤU</div>'+zones.map(z=>`<div class="stack" style="margin-bottom:14px"><div class="split"><strong>${esc(z.name)}</strong><span class="muted small">${z.capacity} chỗ</span></div>
    <div class="seat-rows"><div class="seat-row">${z.seats.slice(0,36).map(s=>'<span class="badge'+(s.blocked?' bad':'')+'">'+esc(s.code)+'</span>').join('')}</div></div>
    ${z.seats.length>36?'<p class="muted small">Hiển thị 36 ghế đầu của khu vực.</p>':''}</div>`).join('');
}
function checkoutView(id) {
  const order=lookup(db.orders,id);
  if (account()&&order?.user!==account().id) return pageTitle('Không tìm thấy đơn của bạn')+link('Lịch sử đơn','account/orders','btn');
  if (!account()) return loginGate('Đăng nhập bằng tài khoản đặt vé để mở đơn.');
  const ses=sessionOf(lookup(db.events,order.eventId),order.sessionId);
  const paymentAvailable=['PENDING_PAYMENT','EXPIRED','PAID','PAID_OUT_OF_STOCK'].includes(order.state);
  return `${link('Lịch sử đơn','account/orders','back')}${pageTitle('Đơn '+order.id,eventTitle(order.eventId)+' · '+fmt(ses?.start))}
    <ol class="stepbar"><li>Chọn ghế</li><li class="${order.state==='PAID'?'':'active'}">Thanh toán</li><li class="${order.state==='PAID'?'active':''}">Nhận vé</li></ol>
    <div class="layout">
    <div class="stack">
    ${panel('Tóm tắt đơn',tag(order.state)+order.seats.map(s=>row(s.code+' · '+s.type,s.zone,money(s.price))).join('')+
      row('Tạm tính',money(order.subtotal))+row('Giảm giá',money(order.discount))+row('Phí dịch vụ',money(order.fee))+row('Tổng thanh toán',money(order.total)))}
    ${heldOrder(order)?panel('Voucher',`<form class="cluster" data-form="voucher" data-id="${esc(id)}">
    <label class="field" style="flex:1"><span>Mã giảm giá</span>${input('code',lookup(db.vouchers,order.voucherId)?.code||'','','placeholder="VENUE15"')}</label>
    <button type="submit" class="btn secondary" style="align-self:end">Áp dụng</button>
    ${order.voucherId?button('Gỡ mã','removeVoucher',id,'ghost'):''}</form>`):''}
    </div><div class="stack">
    ${order.state==='PAID'?panel('Vé đã phát hành',link('Xem vé của tôi','account/tickets','btn')):''}
    ${paymentAvailable?panel('Thanh toán mô phỏng',`<div class="stack">
      ${heldOrder(order)?'<p>Giữ ghế còn <span class="countdown" data-expires="'+order.expires+'">'+remaining(order.expires)+'</span></p>':''}
      ${order.state==='EXPIRED'?notice('Ghế đã được giải phóng. Có thể thử kịch bản thanh toán đến muộn.'):''}
      ${heldOrder(order)?'<div class="ticket-preview">'+qrMarkup('VENUE-DEMO-'+order.id,'Mã QR thanh toán mô phỏng')+'<p class="small muted">QR mẫu để trình diễn giao diện thanh toán.</p></div>':''}
      <form class="stack" data-form="payment" data-id="${esc(id)}">
      ${field('Phương thức',select('method',opt('PayOS')+opt('QR ngân hàng')))}
      ${field('Phản hồi giao dịch',select('scenario',
        (order.state==='PENDING_PAYMENT'?opt('paid','Thành công')+opt('failed','Thất bại')+opt('review','Mất kết nối · đưa vào đối soát'):'')+
        (['PENDING_PAYMENT','EXPIRED'].includes(order.state)?opt('late-open','Đến muộn · kiểm tra lại ghế')+opt('late-conflict','Đến muộn · ghế đã có người đặt'):opt('paid','Gửi lại giao dịch đã ghi nhận'))))}
      <button class="btn" type="submit">Mô phỏng phản hồi</button></form>
      ${heldOrder(order)?button('Hủy giữ ghế','cancelHold',id,'danger'):''}
    </div>`):''}
    </div></div>`;
}
function loginGate(note) {
  return `${pageTitle('Đăng nhập',note)}${panel('',`<div class="cluster">
    ${button('Đăng nhập','login','','',attr('note',note))}${button('Tạo tài khoản','register','','secondary')}${button('Dùng tài khoản mẫu','demo','','ghost')}
  </div>`)}`;
}
function remaining(until) {
  const seconds=Math.max(0,Math.ceil((until-Date.now())/1000));
  const unit=n=>String(n).padStart(2,'0');
  return unit(Math.floor(seconds/60))+':'+unit(seconds%60);
}
function ticketShowcase(ticket) {
  const ses=sessionOf(lookup(db.events,ticket.eventId),ticket.sessionId);
  return `<div class="ticket-preview">
    <strong>${esc(eventTitle(ticket.eventId))}</strong><p>${esc(fmt(ses?.start))} · ${esc(venueName(ses?.venueId))}</p>
    <p>${esc(ticket.type)} · ${esc(ticket.zone)} · Ghế ${esc(ticket.seat)}</p>
    ${tag(ticket.state)}${validTicket(ticket)?qrMarkup(ticket.id,'QR vé '+ticket.id):''}<code>${esc(ticket.id)}</code>
  </div>`;
}
function inviteView(token) {
  const t=db.tickets.find(ticket=>ticket.token===token);
  if (!t||t.state==='REVOKED'||t.inviteUntil<Date.now()||!publicEvent(lookup(db.events,t.eventId))) return pageTitle('Liên kết vé mời hết hiệu lực')+link('Về trang chủ','home','btn');
  return `${pageTitle('Vé mời',eventTitle(t.eventId))}<div class="layout">${panel('Vé của khách mời',ticketShowcase(t))}
    ${panel('Sử dụng vé',`<div class="stack"><p>Xuất trình QR tại cổng, đúng suất diễn và khu vực trên vé.</p>
    ${t.user?tag('CLAIMED'):button('Liên kết vé với tài khoản','claim',token,'secondary')}
    ${validTicket(t)?'<div class="cluster">'+button('Tải PDF','downloadInvite',token,'secondary')+button('In vé','printInvite',token,'secondary')+'</div>':''}</div>`)}</div>`;
}
const accountTabs={
  profile:'Hồ sơ',tickets:'Vé của tôi',orders:'Lịch sử đơn hàng',favorites:'Yêu thích',
  rentals:'Thuê địa điểm',application:'Hồ sơ Ban tổ chức',refunds:'Hoàn tiền',notifications:'Thông báo',support:'Hỗ trợ'
};
function table(headers,rows,placeholder='Chưa có dữ liệu.') {
  return rows.length?'<div class="table-scroll"><table><thead><tr>'+headers.map(h=>'<th scope="col">'+esc(h)+'</th>').join('')+'</tr></thead><tbody>'+
    rows.map(cells=>'<tr>'+cells.map(value=>'<td>'+value+'</td>').join('')+'</tr>').join('')+'</tbody></table></div>':empty(placeholder);
}
function accountView(tab='profile') {
  const user=account();
  if (!user) return loginGate('Đăng nhập để xem vé, đơn hàng và thông tin cá nhân.');
  const active=accountTabs[tab]?tab:'profile';
  const primary=['tickets','orders','refunds','rentals'];
  const secondary=['profile','favorites','notifications','support',...(user.role==='user'?['application']:[])];
  return `<div class="account-heading"><div class="wrapless">
    <h1>${esc(accountTabs[active])}</h1>
    <nav class="account-nav" aria-label="Tài khoản"><div class="account-nav-main">
      ${primary.map(id=>navLink(accountTabs[id],'account/'+id,id===active)).join('')}</div>
      <details class="nav-account account-more"><summary class="btn secondary compact">${ic('menu',16)} <span>Thêm</span></summary>
        <div class="nav-popover">${secondary.map(id=>navLink(accountTabs[id],'account/'+id,id===active)).join('')}</div></details>
    </nav></div></div>${accountSection(active,user)}`;
}
function accountSection(tab,user) {
  if (tab==='profile') return `<div class="layout">${panel('Thông tin cá nhân',`<div class="stack">
    ${row(user.name,user.email+' · '+user.phone,tag(roles[user.role]))}
    ${row('Xác minh liên hệ',user.verified?'Đã xác minh':'Chưa xác minh',user.verified?'':button('Xác minh','verifyContact','','secondary compact'))}
    <div class="cluster">${button('Cập nhật hồ sơ','editProfile')}${button('Đổi mật khẩu','changePassword','','secondary')}${button('Đăng xuất','logout','','ghost')}</div></div>`)}
    ${panel('Lịch sử xem',db.policy.history?(db.history[user.id]||[]).filter(id=>publicEvent(lookup(db.events,id))).slice(0,6).map(id=>row(eventTitle(id),'',link('Xem sự kiện','event/'+id,'btn secondary compact'))).join('')||empty('Bạn chưa xem sự kiện nào.'):empty('Lịch sử xem đang tắt.'))}</div>`;
  if (tab==='favorites') {
    const list=(db.favorites[user.id]||[]).map(id=>lookup(db.events,id)).filter(publicEvent);
    return list.length?'<div class="event-grid">'+list.map(event=>eventCard(event)).join('')+'</div>':empty('Bạn chưa lưu sự kiện nào.');
  }
  if (tab==='tickets') return customerTickets(user);

  if (tab==='orders') return panel('Đơn của bạn',table(['Mã đơn','Sự kiện','Thanh toán','Trạng thái','Chi tiết'],
    db.orders.filter(o=>o.user===user.id).map(o=>[esc(o.id),esc(eventTitle(o.eventId)),money(o.total)+'<br>'+esc(o.txn||''),
      tag(o.state),link('Mở đơn','checkout/'+o.id,'btn secondary compact')]),'Bạn chưa có đơn đặt vé.'));
  if (tab==='application') {
    const app=db.applications.find(a=>a.user===user.id);
    if (user.role==='organizer') return panel('Tài khoản Ban tổ chức',`<div class="stack">
      ${row(app?.company||user.name,'Tài khoản đã có quyền tổ chức sự kiện.',tag('APPROVED'))}
      <div>${link('Quản lý sự kiện','work/events','btn')}</div></div>`);
    if (user.role!=='user') return panel('Hồ sơ Ban tổ chức',link('Vào khu vực vận hành','work','btn'));
    const enabled=!app||['DRAFT','NEEDS_INFO','REJECTED'].includes(app.state);
    return panel('Đăng ký làm Ban tổ chức',`<div class="stack">
      <p>Thời hạn xem xét tối đa 7 ngày làm việc kể từ lúc gửi hồ sơ.</p>
      ${app?row(app.company||'Hồ sơ Ban tổ chức',app.note||(app.deadline?'Hạn xem xét '+fmt(app.deadline):''),tag(app.state)):''}
      ${app?.history?.length?panel('Lịch sử cập nhật',app.history.map(item=>row(status[item.state]?.[0]||item.state,fmt(item.time)+(item.note?' · '+item.note:''))).join('')):''}
      ${enabled?button(app?'Chỉnh sửa hồ sơ':'Điền hồ sơ','application'):''}
    </div>`);
  }
  if (tab==='rentals') return customerRentals(user);

  if (tab==='refunds') return panel('Lịch sử hoàn tiền',refundList(db.refunds.filter(refund=>refund.user===user.id),false));
  if (tab==='notifications') {
    const list=db.notifications.filter(n=>n.user===user.id);
    return panel('Hộp thông báo',(list.length?button('Đánh dấu tất cả đã đọc','readAll','','secondary compact'):'')+
      (list.map(n=>row(n.title,n.detail+' · '+fmt(n.time)+' · '+n.channel,n.read?'':button('Đánh dấu đã đọc','read',n.id,'ghost compact'))).join('')||empty('Chưa có thông báo.')));
  }
  return panel('Yêu cầu hỗ trợ',button('Tạo yêu cầu','support')+
    (db.support.filter(s=>s.user===user.id).map(s=>row(s.subject,s.id+' · '+s.text+(s.answer?' · Phản hồi: '+s.answer:''),tag(s.state))).join('')||empty('Chưa có yêu cầu hỗ trợ.')));
}


// Màn hình vận hành theo vai trò

const workspaceMenus = {
  organizer:[
    ['overview','grid','Tổng quan'],['events','calendar','Sự kiện & phiên bản'],['orders','ticket','Đơn, vé & báo cáo'],
    ['scan','scan','Quét QR'],['manual','search','Check-in thủ công'],['attendees','users','Khách tham dự'],
    ['guests','users','Khách mời'],['grants','shield','Phân quyền nhân sự'],['promos','ticket','Voucher'],
    ['finance','wallet','Hợp đồng & doanh thu'],['settlements','chart','Đối soát'],['support','bell','Hỗ trợ & thông báo'],['logs','list','Nhật ký vận hành']
  ],
  staff:[
    ['overview','grid','Tổng quan'],['scan','scan','Quét QR'],['manual','search','Check-in thủ công'],
    ['attendees','users','Khách tham dự'],['orders','ticket','Đơn & vé'],['reports','chart','Báo cáo cổng'],
    ['guests','users','Vé mời'],['support','bell','Hỗ trợ sự kiện'],['content','edit','Đề xuất nội dung'],['logs','list','Nhật ký check-in']
  ],
  manager:[
    ['overview','grid','Tổng quan'],['applications','users','Duyệt hồ sơ BTC'],['reviews','calendar','Kiểm duyệt sự kiện'],
    ['rentals','building','Thuê địa điểm'],['payments','wallet','Thanh toán'],['refunds','ticket','Hoàn tiền'],
    ['support','bell','Hỗ trợ & khiếu nại'],['settlements','chart','Đối soát'],['monitoring','settings','Giám sát']
  ],
  admin:[
    ['overview','grid','Tổng quan'],['accounts','users','Tài khoản & quyền'],['venues','building','Địa điểm & ghế'],['catalog','grid','Danh mục & banner'],
    ['applications','users','Hồ sơ BTC'],['reviews','calendar','Kiểm duyệt sự kiện'],['rentals','building','Thuê địa điểm'],
    ['promos','ticket','Chiến dịch ưu đãi'],['contracts','list','Hợp đồng phí'],['payments','wallet','Thanh toán'],
    ['refunds','ticket','Hoàn tiền'],['settlements','chart','Đối soát'],['payouts','wallet','Giải ngân'],
    ['support','bell','Hỗ trợ & khiếu nại'],['policy','shield','Chính sách & lịch nghỉ'],['config','settings','Cấu hình & bảo mật'],
    ['monitoring','settings','Giám sát'],['audit','list','Nhật ký & sao lưu']
  ]
};
const staffTabs={scan:'checkin.scan',manual:'checkin.manual',attendees:'attendees.view',orders:'orders.view',reports:'reports.view',guests:'guests.manage',support:'attendees.view',content:'content.propose'};
function metrics(values) {
  return '<div class="metrics">'+values.map(([label,value,icon='chart',tab,detail])=>{
    const body=`<span class="metric-icon">${ic(icon,20)}</span><span class="metric-label">${esc(label)}</span><strong>${esc(value)}</strong>${detail?`<small class="metric-detail">${esc(detail)}</small>`:''}`;
    return tab?`<a class="metric" href="#work/${esc(tab)}">${body}</a>`:`<div class="metric">${body}</div>`;
  }).join('')+'</div>';
}
function workOverview(user) {
  if (user.role==='admin') return adminOverview();
  if (user.role==='staff') return staffDashboard('overview',user);
  const events= user.role==='organizer'?db.events.filter(e=>e.owner===user.id):db.events;
  const ids=new Set(events.map(e=>e.id));
  const orders=db.orders.filter(o=>ids.has(o.eventId)&&o.paidAt&&o.state!=='PAID_OUT_OF_STOCK'&&!db.refunds.some(r=>r.orderId===o.id&&r.priority));
  const items= user.role==='organizer'?
    [['Sự kiện',events.length,'calendar','events'],['Vé có hiệu lực',db.tickets.filter(t=>ids.has(t.eventId)&&validTicket(t)).length,'ticket','orders'],
    ['Doanh thu vé',money(orders.reduce((s,o)=>s+o.subtotal-o.discount,0)),'wallet','finance'],
    ['Đã check-in',db.tickets.filter(t=>ids.has(t.eventId)&&t.state==='CHECKED_IN').length,'scan','attendees']]:
    [['Hồ sơ BTC chờ',db.applications.filter(a=>a.state==='PENDING').length,'users','applications'],
    ['Sự kiện cần duyệt',events.filter(e=>e.state==='PENDING'||e.draftState==='PENDING_UPDATE'||e.request).length,'calendar','reviews'],
    ['Thuê chờ xử lý',db.rentals.filter(r=>['REQUESTED','DEPOSIT_REVIEW','CANCELLATION_REQUESTED'].includes(r.state)).length,'building','rentals'],
    ['Hoàn tiền mở',db.refunds.filter(r=>!['COMPLETED','REJECTED'].includes(r.state)).length,'wallet','refunds']].filter(item=>canManage(item[3]));
  const quickTabs=user.role==='organizer'?['events','orders','scan','finance','settlements']:['applications','reviews','rentals','refunds','support'];
  const actions=quickTabs.map(tab=>workspaceMenus[user.role].find(entry=>entry[0]===tab)).filter(([tab])=>user.role==='organizer'||canManage(tab));
  return `<div class="stack">${items.length?metrics(items):''}<div class="layout">
    ${panel('Truy cập nhanh',actions.length?'<div class="work-quick-links">'+actions.map(([tab,icon,name])=>
      `<a href="#work/${tab}">${ic(icon,19)} ${esc(name)}</a>`).join('')+'</div>':empty('Chưa có tác vụ được phân quyền.'))}
    ${panel('Hoạt động gần đây',db.audit.filter(a=>a.actor===user.id||user.role==='organizer'&&ids.has(a.subject)||canManage('reviews')&&ids.has(a.subject)).slice(0,6).map(a=>row(a.action,fmt(a.time)+' · '+a.subject)).join('')||empty('Thao tác sẽ được ghi tại đây.'))}
  </div></div>`;
}
function workspaceBody(user,tab) {
  if (tab==='overview') return workOverview(user);
  if (user.role==='staff') return staffDashboard(tab,user);
  if (user.role==='organizer') return ['scan','manual','attendees'].includes(tab)?staffDashboard(tab,user):organizerDashboard(tab,user);
  return platformDashboard(tab,user);
}
function eventOps(event) {
  const info=event.live||event.draft;
  const orders=db.orders.filter(o=>o.eventId===event.id);
  return `<div class="stack">
    ${metrics([['Suất diễn',info.sessions.length],['Đơn đã thu',orders.filter(o=>!!o.paidAt).length],['Vé đã check-in',db.tickets.filter(t=>t.eventId===event.id&&t.state==='CHECKED_IN').length],['Vé mời',db.tickets.filter(t=>t.eventId===event.id&&t.type==='Invitation'&&validTicket(t)).length]])}
    ${panel('Tồn kho theo suất và khu vực',table(['Suất diễn','Khu vực','Sức chứa','Bán','Giữ','Mời','Còn trống'],info.sessions.flatMap(s=>s.zones.map(z=>{
      const items=[...inventory(s.id).values()].filter(item=>item.zone===z.name);
      return [esc(s.name)+'<br>'+fmt(s.start),esc(z.name),z.capacity,items.filter(i=>i.state==='sold').length,items.filter(i=>i.state==='hold').length,
        items.filter(i=>i.state==='invite').length,Math.max(0,Math.min(z.capacity,z.seats.filter(seat=>!seat.blocked).length)-items.length)];
    }))))}
    ${panel('Tiến độ và thông báo',event.prep?esc(event.prep):empty('Chưa có cập nhật chuẩn bị.'))}
    ${panel('Đơn đặt vé',workOrders(orders))}
  </div>`;
}
function workOrders(orders) {
  return table(['Mã đơn','Sự kiện','Suất','Vé','Tổng','Trạng thái'],orders.map(order=>[
    esc(order.id),esc(eventTitle(order.eventId)),esc(fmt(sessionOf(lookup(db.events,order.eventId),order.sessionId)?.start)),
    order.seats.length,money(order.total),tag(order.state)
  ]));
}
function organizerDashboard(tab,user) {
  const events=db.events.filter(e=>e.owner===user.id);
  const ids=new Set(events.map(e=>e.id));
  if (tab==='events') {
    const edit=lookup(events,editorId);
    if (edit) return editEventView(edit);
    return `<div class="stack"><div class="cluster">${button('Tạo sự kiện','newEvent')}${link('Thuê địa điểm','venues','btn secondary')}</div>
      ${panel('Sự kiện của tôi',events.map(e=>row(eventTitle(e.id),(e.live?'Phiên bản công khai '+e.version:'Chưa công khai')+(e.decision?' · '+e.decision:''),
      tag(e.state)+(e.draftState?tag(e.draftState):'')+
      button('Quản lý','editEvent',e.id,'secondary compact')+(e.live?button('Vận hành','eventOps',e.id,'compact'):'')+
      (publicEvent(e)?link('Xem trang công khai','event/'+e.id,'btn ghost compact'):''))).join('')||empty('Chưa có sự kiện.'))}</div>`;
  }
  if (tab==='orders') return `<div class="stack">${panel('Đơn và vé',workOrders(db.orders.filter(o=>ids.has(o.eventId))))}
    ${panel('Báo cáo theo sự kiện',events.filter(e=>e.live).map(e=>row(eventTitle(e.id),eventDate(e),button('Xem tồn kho và doanh số','eventOps',e.id,'secondary compact'))).join(''))}</div>`;
  if (tab==='guests') return guestsPanel(events.filter(e=>e.live),false);
  if (tab==='grants') return `<div class="stack"><div>${button('Mời hoặc cấp quyền','grant')}</div>
    ${panel('Quyền theo sự kiện hoặc suất',db.grants.filter(g=>ids.has(g.eventId)).map(g=>row(lookup(db.accounts,g.user)?.name||g.user,
      eventTitle(g.eventId)+' · '+(g.sessionId?sessionOf(lookup(events,g.eventId),g.sessionId)?.name:'Mọi suất')+' · '+g.permissions.map(p=>permissionNames[p]).join(', '),
      tag(g.active?'ACTIVE':'REVOKED')+button('Chỉnh quyền','grant',g.id,'secondary compact')+(g.active?button('Thu hồi','revokeGrant',g.id,'danger compact'):''))).join('')||empty('Chưa phân công nhân sự.'))}</div>`;
  if (tab==='promos') return promoPanel(db.vouchers.filter(v=>v.owner===user.id));
  if (tab==='finance') {
    const orders=db.orders.filter(o=>ids.has(o.eventId)&&o.paidAt&&!db.refunds.some(r=>r.orderId===o.id&&r.priority));
    return `<div class="stack">${panel('Hợp đồng hiện có',contractTable(db.contracts.filter(c=>c.owner===user.id)))}
      ${panel('Doanh thu theo đơn và suất',financeTable(orders))}
      ${panel('Tài khoản thụ hưởng',row(user.bank||'Chưa có tài khoản',user.bankState||'',button('Cập nhật thụ hưởng','bank','','secondary')))}
      ${panel('Tiền đã giải ngân',payoutList(db.payouts.filter(p=>p.owner===user.id),false))}</div>`;
  }
  if (tab==='settlements') return settlementsPanel(db.settlements.filter(s=>s.owner===user.id),'organizer');
  if (tab==='support') return `<div class="stack">${panel('Thông báo sự kiện',events.filter(e=>e.live).map(e=>row(eventTitle(e.id),e.notices[0]?.text||'',button('Gửi thông báo','announcement',e.id,'secondary compact'))).join(''))}
    ${panel('Yêu cầu hỗ trợ',supportList(db.support.filter(s=>ids.has(s.eventId)),true))}</div>`;
  return `<div class="stack">${panel('Nhật ký check-in',scanLog(db.scans.filter(s=>ids.has(s.eventId))))}
    ${panel('Thao tác BTC và nhân sự',auditTable(db.audit.filter(a=>a.actor===user.id||ids.has(a.subject))))}</div>`;
}
function editEventView(event) {
  const pending=event.state==='PENDING'||event.draftState==='PENDING_UPDATE';
  const info=event.draft||event.live;
  const sections={general:'Hồ sơ',sessions:'Suất, khu & ghế',history:'Phiên bản'};
  return `<div class="stack">
    <div class="cluster">${button('Danh sách','endEdit','','secondary')}${tag(event.state)}${event.draftState?tag(event.draftState):''}
      ${pending?'':button(event.live?'Gửi bản cập nhật':'Gửi duyệt sự kiện','submitEvent',event.id)}
      ${!event.live&&!pending?button('Xóa bản nháp','deleteEvent',event.id,'danger'):''}
      ${event.live&&!event.request&&['ACTIVE','POSTPONED'].includes(event.state)?button('Hoãn / hủy','requestEvent',event.id,'secondary'):''}
      ${event.live?button('Tiến độ','prep',event.id,'secondary'):''}</div>
    <div><h2>${esc(eventTitle(event.id))}</h2>${event.decision?notice(event.decision):''}</div>
    <div class="categories">${Object.entries(sections).map(([id,label])=>button(label,'editSection',id,id===editorSection?'':'secondary')).join('')}</div>
    ${editorSection==='general'?panel('Nội dung sự kiện',`<div class="stack"><p>${esc(info.description||'Chưa nhập mô tả.')}</p>
      ${row('Chương trình',info.program||'Chưa nhập')}${row('Quy định',info.rules||'Chưa nhập')}
      ${row('Chính sách vé',info.refund?'Cho phép hủy vé theo chính sách lúc mua':'Không áp dụng tự hủy')}
      ${row('Xác nhận địa điểm',info.venueRequired?'Yêu cầu thuê đã xác nhận':'Địa điểm do BTC bố trí')}
      <div>${pending?'':button('Sửa hồ sơ','eventForm',event.id,'secondary')}</div></div>`):''}
    ${editorSection==='sessions'?panel('Lịch, loại vé và sơ đồ',
      info.sessions.map(s=>row(s.name,fmt(s.start)+' · '+venueName(s.venueId)+' · '+s.zones.map(z=>z.name+' '+z.capacity+' chỗ').join(', '),
        button('Sơ đồ','draftMap',s.id,'ghost compact',attr('event',event.id))+
        (pending?'':button('Cấu hình','sessionForm',s.id,'secondary compact',attr('event',event.id))+button('Loại vé','types',s.id,'secondary compact',attr('event',event.id))+button('Xóa suất','deleteSession',s.id,'danger compact',attr('event',event.id)))+
        (sessionOf(event,s.id)?button(sessionOf(event,s.id).open?'Đóng bán ngay':'Mở bán lại','saleToggle',s.id,'secondary compact',attr('event',event.id)):'')
      )).join('')+(pending?'':'<div style="margin-top:12px">'+button('Thêm suất diễn','sessionForm','')+'</div>')):''}
    ${editorSection==='history'?panel('Phiên bản và yêu cầu',`<div class="stack">
      ${event.live?row('Công khai #'+event.version,event.live.title,tag(event.state)):''}
      ${event.draft?row('Bản chỉnh sửa',event.draft.title,tag(event.draftState||event.state)):''}
      ${event.request?row('Yêu cầu '+event.request.kind,event.request.reason,tag('PENDING')):''}
      ${event.versions.map(v=>row('Phiên bản '+v.version+' · '+(status[v.state]?.[0]||v.state),v.note+' · '+fmt(v.time),v.data?button('Xem nội dung','versionDetails',v.id,'secondary compact',attr('event',event.id)):'' )).join('')||empty('Chưa có phiên bản đã lưu.')}
      </div>`):''}
  </div>`;
}
function inviteRows(events,staff=false) {
  const ids=new Set(events.map(e=>e.id));
  return db.tickets.filter(t=>t.type==='Invitation'&&ids.has(t.eventId)&&(!staff||canStaff('guests.manage',t.eventId,t.sessionId))).map(t=>row(t.id+' · '+eventTitle(t.eventId),
    t.contact+' · '+t.channel+' · Ghế '+t.seat+(t.user?' · Đã liên kết':''),
    tag(t.state)+button('Xem liên kết','inviteLink',t.id,'secondary compact')+(t.state==='ISSUED'?button('Thu hồi','revokeInvite',t.id,'danger compact')+button('Gửi lại','resendInvite',t.id,'ghost compact'):'')+
    (t.state==='REVOKED'?button('Phát lại','renewInvite',t.id,'secondary compact'):'')
  )).join('');
}
function guestsPanel(events,staff) {
  return `<div class="stack"><div>${events.length?button('Nhập khách mời','invite'):''}</div>
    ${panel('Danh sách vé mời',inviteRows(events,staff)||empty('Chưa có khách mời trong phạm vi được cấp.'))}</div>`;
}
function promoPanel(vouchers) {
  return `<div class="stack"><div>${button('Tạo voucher','promo')}</div>${panel('Voucher và chiến dịch',vouchers.map(v=>{
    const orders=db.orders.filter(o=>o.voucherId===v.id);
    return row(v.code,(v.kind==='percent'?v.value+'%':money(v.value))+' · '+fmt(v.from)+' đến '+fmt(v.to)+' · '+
      orders.filter(o=>o.voucherUsed).length+'/'+v.limit+' đã dùng · '+orders.filter(o=>heldOrder(o)).length+' đang giữ · Chi phí '+money(orders.filter(o=>o.voucherUsed).reduce((sum,o)=>sum+o.discount,0)),
      tag(v.state)+button('Cấu hình','promo',v.id,'secondary compact')+
      (account().role==='admin'&&v.state==='PENDING'?button('Duyệt chiến dịch','approvePromo',v.id,'compact')+button('Từ chối','rejectPromo',v.id,'danger compact'):'')+
      (v.state==='ACTIVE'?button('Dừng','stopPromo',v.id,'ghost compact'):''));
  }).join('')||empty('Chưa có mã khuyến mãi.'))}</div>`;
}
function staffSessions(user) {
  if (user.role==='organizer') return db.events.filter(e=>e.live&&ownerEvent(e)).flatMap(event=>event.live.sessions.map(session=>({event,session})));
  const active=db.grants.filter(g=>g.user===user.id&&g.active);
  return db.events.filter(e=>e.live&&active.some(g=>g.eventId===e.id)).flatMap(e=>e.live.sessions.filter(s=>active.some(g=>g.eventId===e.id&&(!g.sessionId||g.sessionId===s.id))).map(s=>({event:e,session:s})));
}
function staffDashboard(tab,user) {
  const permission=staffTabs[tab];
  const available=staffSessions(user).filter(({event,session})=>!permission||canOperate(permission,event.id,session.id));
  if (!available.length) return empty('Chưa được cấp quyền cho suất diễn nào. Quyền thu hồi có hiệu lực trong bản mẫu ngay khi cập nhật.');
  const scope=available.find(({event,session})=>event.id===staffScope.eventId&&session.id===staffScope.sessionId)||available[0];
  const {event,session}=scope;
  staffScope={eventId:event.id,sessionId:session.id};
  const scoped=select('scope',available.map(pair=>opt(pair.event.id+'/'+pair.session.id,eventTitle(pair.event.id)+' · '+pair.session.name+' · '+fmt(pair.session.start),event.id+'/'+session.id)).join(''),'data-change="staffScope"');
  return `<div class="stack">${panel('Sự kiện được giao',field('Suất diễn đang thao tác',scoped))}
    ${staffSection(tab,user,event,session)}</div>`;
}
function staffSection(tab,user,event,ses) {
  const tickets=db.tickets.filter(t=>t.sessionId===ses.id);
  const logs=db.scans.filter(s=>s.sessionId===ses.id&&(s.actor===user.id||canOperate('reports.view',event.id,ses.id)));
  if (tab==='overview'||tab==='reports') return `${metrics([
    ['Trạng thái',status[event.state]?.[0]||event.state],['Đã check-in',tickets.filter(t=>t.state==='CHECKED_IN').length,'scan'],
    ['Chỗ khả dụng',sessionFree(ses),'ticket'],['Quét cần xử lý',logs.filter(log=>log.result!=='CHECKED_IN').length,'bell']])}
    ${panel('Thông báo vận hành',event.notices.map(n=>row(n.text,fmt(n.time))).join('')||empty('Chưa có thông báo.'))}
    ${tab==='reports'?panel('Nhật ký theo suất',scanLog(logs)):''}`;
  if (tab==='scan') return panel('Quét mã QR',`<div class="stack">
    ${notice('Thử mã VN-DEMO-QR hoặc VN-DEMO-MOI ở “Suất chính” của Neon Wave. Có thể nhập mã hoặc tải ảnh QR.')}
    <form class="form-grid" data-form="scan">${field('Mã vé / đầu đọc QR',input('code','','text','required autocomplete="off"'))}
    ${field('Kiểm tra khu vực',select('zone',opt('','Tất cả khu vực')+ses.zones.map(z=>opt(z.name)).join('')))}
    <div class="span"><button class="btn" type="submit">Xác nhận check-in</button></div></form>
    ${field('Tải ảnh mã QR',input('qrFile','','file','accept="image/*" data-change="qrFile"'))}
    ${staffResult?notice(staffResult,staffResult.startsWith('Từ chối')):''}</div>`);
  if (tab==='manual') {
    const found=staffLookup?tickets.filter(t=>{
      const person=lookup(db.accounts,t.user);
      return [t.id,t.orderId,t.contact,person?.email,person?.phone].some(value=>value&&matches(value,staffLookup));
    }):[];
    return panel('Xác minh thông tin tại cổng',`<div class="stack"><form class="cluster" data-form="staffLookup">
      <div style="flex:1">${field('Mã đặt chỗ, email hoặc điện thoại',input('query',staffLookup,'search','required'))}</div>
      <button class="btn" style="align-self:end" type="submit">Tra cứu</button></form>
      ${found.map(t=>row(t.id,(lookup(db.accounts,t.user)?.name||'Khách mời')+' · Ghế '+t.seat,tag(t.state)+button('Xác minh và check-in','manualScan',t.id,'secondary compact'))).join('')||
      empty(staffLookup?'Không có vé phù hợp trong suất này.':'Nhập dữ liệu để tra cứu.')}
      ${staffResult?notice(staffResult,staffResult.startsWith('Từ chối')):''}</div>`);
  }
  if (tab==='attendees') return panel('Khách theo suất',table(['Vé','Khách','Khu / ghế','Trạng thái'],tickets.map(t=>[
    esc(t.id),esc(lookup(db.accounts,t.user)?.name||'Khách mời'),esc(t.zone+' · '+t.seat),tag(t.state)
  ])));
  if (tab==='orders') return panel('Đơn của suất được giao',workOrders(db.orders.filter(o=>o.sessionId===ses.id)));
  if (tab==='guests') return guestsPanel([event],true);
  if (tab==='content') return panel('Gửi đề xuất tác nghiệp',button('Đề xuất thay đổi','staffProposal',event.id));
  if (tab==='support') return panel('Hỗ trợ tại sự kiện',supportList(db.support.filter(c=>c.eventId===event.id&&c.sessionId===ses.id),false)+
    '<div style="margin-top:12px">'+button('Ghi sự cố tại cổng','incident',event.id,'secondary')+'</div>');
  return panel('Nhật ký check-in',scanLog(logs));
}
function scanLog(logs) {
  return table(['Thời gian','Vé','Nhân sự','Phương thức','Kết quả'],logs.map(log=>[
    fmt(log.time),esc(log.ticketId),esc(lookup(db.accounts,log.actor)?.name||log.actor),esc(log.method),esc(log.text)
  ]));
}
function platformDashboard(tab,user) {
  const view=user.role==='admin'?route[2]:'';
  switch(tab) {
    case 'applications': return applicationPanel();
    case 'reviews': return view==='active'?adminActiveEventsPanel():moderationPanel();
    case 'rentals': return rentalPanel();
    case 'payments': return transactionPanel();
    case 'refunds': return panel('Hàng đợi hoàn tiền',refundList(db.refunds,true));
    case 'support': return `<div class="stack">${panel('Khiếu nại và hỗ trợ',supportList(db.support,true))}
      ${panel('Đánh giá công khai',db.reviews.map(r=>row(eventTitle(r.eventId)+' · '+r.score+'/5',r.text,button(r.visible===false?'Hiện lại':'Ẩn đánh giá','moderateRating',r.id,'secondary compact'))).join('')||empty('Chưa có đánh giá.'))}</div>`;
    case 'settlements': return view==='queue'?adminFinancePanel():settlementsPanel(db.settlements,user.role);
    case 'monitoring': return view==='alerts'?adminWarningsPanel():monitoringPanel();
    case 'accounts': return accountsPanel(view==='managers');
    case 'venues': return adminVenuesPanel();
    case 'catalog': return catalogPanel();
    case 'promos': return promoPanel(db.vouchers);
    case 'contracts': return `<div class="stack"><div>${button('Tạo hợp đồng','contract')}</div>${panel('Hợp đồng phí nền tảng',contractTable(db.contracts))}</div>`;
    case 'payouts': return panel('Giải ngân theo bảng đối soát',payoutList(db.payouts,true));
    case 'policy': return policyPanel();
    case 'config': return configPanel();
    case 'audit': return backupPanel();
    default: return empty('Mục này không khả dụng.');
  }
}
function applicationPanel() {
  const items=db.applications.filter(a=>a.state!=='DRAFT');
  return panel('Hồ sơ và thời hạn xét duyệt',items.map(app=>{
    const left=app.deadline-Date.now();
    const label=app.state==='PENDING'?(left<=0?'Quá hạn':left<=2*day?'Sắp quá hạn':'Hạn '+fmt(app.deadline)):'';
    return row(app.company,(lookup(db.accounts,app.user)?.name||'')+' · '+label+(app.note?' · '+app.note:''),
      tag(app.state)+button('Xem hồ sơ','applicationDetails',app.id,'secondary compact')+
      (app.state==='PENDING'?button('Xét duyệt','applicationDecision',app.id,'compact'):'')+
      (app.state==='APPROVED'?button('Đề xuất tạm dừng BTC','flagOrg',app.id,'ghost compact'):''));
  }).join('')||empty('Chưa có hồ sơ được gửi.'));
}
function moderationPanel(events=db.events.filter(e=>e.live||e.state==='PENDING'||e.state==='REJECTED'),title='Hàng đợi và sự kiện trên nền tảng') {
  return panel(title,events.map(e=>row(eventTitle(e.id),
    organizerName(e)+(e.request?' · Yêu cầu '+e.request.kind:'')+(e.decision?' · '+e.decision:''),
    tag(e.state)+(e.draftState?tag(e.draftState):'')+button('Kiểm tra hồ sơ','inspectEvent',e.id,'secondary compact')+
    (e.state==='PENDING'||e.draftState==='PENDING_UPDATE'||e.request?button('Xét duyệt','decideEvent',e.id,'compact'):'')+
    (e.live&&['ACTIVE','POSTPONED','SUSPENDED'].includes(e.state)?button(e.state==='SUSPENDED'?'Khôi phục':'Đình chỉ','suspend',e.id,'danger compact'):'')
  )).join('')||empty('Chưa có sự kiện trong danh sách.'));
}
function rentalPanel() {
  const canProcess=r=>['REQUESTED','DEPOSIT_REVIEW','CANCELLATION_REQUESTED'].includes(r.state)||
    r.state==='ESCALATED'&&account().role==='admin';
  const rentals=[...db.rentals].sort((a,b)=>Number(canProcess(b))-Number(canProcess(a))||(b.created||0)-(a.created||0));
  return panel('Lịch giữ chỗ và chứng từ',table(['Hồ sơ / Sự kiện','Địa điểm','Khung giờ','Số khách','Tiền cọc','Trạng thái','Thao tác'],
    rentals.map(r=>[
      esc(r.eventName||r.id)+(r.batchId?'<br><small>'+esc(r.batchId)+'</small>':''),
      esc(venueName(r.venueId)),esc(fmt(r.start))+'<br>'+esc(fmt(r.end)),r.capacity,
      money(r.deposit)+(r.deadline?'<br><small>Hạn '+esc(fmt(r.deadline))+'</small>':''),
      tag(r.state),'<div class="cluster">'+button('Chi tiết','rentalDetail',r.id,'secondary compact')+
        (canProcess(r)?button('Xử lý','rentalDecision',r.id,'compact'):'')+'</div>'
    ]),'Chưa có yêu cầu thuê.'));
}

function transactionPanel() {
  return panel('Đối soát giao dịch',table(['Giao dịch','Đơn','Kênh','Số tiền','Trạng thái','Thao tác'],db.transactions.map(t=>[
    esc(t.id)+'<br>'+fmt(t.time),esc(t.orderId),esc(t.channel||'PayOS'),money(t.amount),tag(t.state)+(t.late?' · Đến muộn':''),
    t.state==='MANUAL_REVIEW'?button('Đối chiếu','transactionReview',t.id,'secondary compact'):''
  ])));
}
function refundList(refunds,manage) {
  return refunds.map(r=>row(r.id+' · '+money(r.amount),r.reason+(r.ticketIds?.length?' · '+r.ticketIds.length+' vé':'')+(r.priority?' · Ưu tiên':'')+(r.proof?' · Chứng từ '+r.proof:'')+(r.note?' · '+r.note:''),
    tag(r.state)+button('Chi tiết','refundDetail',r.id,'secondary compact')+
    (manage&&!['COMPLETED','REJECTED'].includes(r.state)?button('Xử lý','refundDecision',r.id,'compact'):'')
  )).join('')||empty('Chưa có yêu cầu hoàn.');
}
function supportList(items,manage) {
  return items.map(s=>row(s.subject,s.id+' · '+s.text+(s.answer?' · '+s.answer:''),tag(s.state)+
    (manage?button('Xử lý','supportDecision',s.id,'secondary compact'):'')+
    (manage&&s.ticketId&&s.state==='OPEN'&&!s.refundId&&canManage('support')&&canManage('refunds')?
      button('Xét hoàn ngoại lệ','supportRefund',s.id,'compact'):'')
  )).join('')||empty('Chưa có yêu cầu hỗ trợ.');
}
function contractTable(contracts) {
  return table(['Hợp đồng','BTC','Công thức phí','Hiệu lực','Thao tác'],contracts.map(c=>[
    esc(c.name),esc(lookup(db.accounts,c.owner)?.name||c.owner),esc(c.kind==='percent'?c.value+'% · '+c.basis:money(c.value)+' / vé'),
    fmt(c.from)+'<br>'+fmt(c.to),account()?.role==='admin'?button('Chi tiết / chỉnh sửa','contract',c.id,'secondary compact'):esc(c.notes||'')
  ]));
}
function financeTable(orders) {
  return table(['Đơn / suất','Tiền vé thực thu','Giảm giá','Hoàn tiền','Phí nền tảng','Mã hợp đồng'],
    orders.map(o=>[esc(o.id)+'<br>'+esc(o.sessionId),money(o.subtotal-o.discount),money(o.discount),money(settledRefundAmount(o.id)),
      money(o.platformFee),esc(o.contractSnapshot?.name||o.contractId||'—')]));
}
function settlementsPanel(settlements,role,emptyMessage='Chưa có kỳ đối soát. Sự kiện mẫu AFTERGLOW LIVE đã qua thời hạn khóa sổ.') {
  return `<div class="stack">${role==='organizer'?'':'<div>'+button('Khóa sổ sự kiện','settlement')+'</div>'}
    ${panel('Kỳ đối soát',settlements.map(s=>row(eventTitle(s.eventId)+' · '+s.id,'Thực nhận '+money(s.net)+' · Hạn BTC phản hồi '+fmt(s.deadline),
      tag(s.state)+button('Chi tiết','settlementDetail',s.id,'secondary compact')+
      (role==='organizer'&&s.state==='DRAFT'?button('Xác nhận / phản hồi','settlementResponse',s.id,'compact'):'')+
      (role!=='organizer'&&settlementAwaitingReview(s)?button('Duyệt đối soát','settlementApprove',s.id,'compact'):'')+
      (role==='admin'&&s.state==='MANAGER_APPROVED'?button('Lập lệnh chi','payoutCreate',s.id,'compact'):'')
    )).join('')||empty(emptyMessage))}</div>`;
}
function payoutList(payouts,manage) {
  return payouts.map(p=>row(p.id+' · '+money(p.amount),p.settlementId+(p.proof?' · '+p.proof:'')+(p.note?' · '+p.note:''),
    tag(p.state)+(manage&&p.state!=='COMPLETED'?button('Xử lý lệnh chi','payoutDecision',p.id,'compact'):'')
  )).join('')||empty('Chưa có lệnh giải ngân.');
}
function accountsPanel(managersOnly=false) {
  const accounts=managersOnly?adminManagers():db.accounts.filter(a=>!ui.accountSearch||matches([a.name,a.email,roles[a.role]].join(' '),ui.accountSearch));
  return `<div class="stack"><div class="cluster">${button('Tạo Quản lý hệ thống','managerAccount')}</div>
    ${managersOnly?'':`<form class="panel cluster" data-form="accountSearch"><div style="flex:1">${field('Tìm tên, email hoặc vai trò',input('query',ui.accountSearch,'search'))}</div><button class="btn" style="align-self:end" type="submit">Tìm</button></form>`}
    ${panel(managersOnly?'Tài khoản Quản lý hệ thống':'Tài khoản',table(['Tên','Liên hệ','Vai trò','Trạng thái','Thao tác'],accounts.map(a=>[
      esc(a.name),esc(a.email),esc(roles[a.role]),tag(a.locked?'DISABLED':'ACTIVE')+(a.verified?' · Đã xác minh':''),
      a.id===account().id?'Phiên hiện tại':button('Quản lý','manageAccount',a.id,'secondary compact')+
        (a.bankState==='Chờ Admin xác minh'?button('Xác minh thụ hưởng','verifyBank',a.id,'secondary compact'):'')
    ])))}
    ${managersOnly?`<div>${link('Tất cả tài khoản','work/accounts','btn secondary')}</div>`:panel('Quan hệ cấp quyền sự kiện',table(['Nhân sự','Sự kiện','Suất','Quyền','Trạng thái'],db.grants.map(g=>[
      esc(lookup(db.accounts,g.user)?.name||g.user),esc(eventTitle(g.eventId)),esc(g.sessionId||'Mọi suất'),
      esc(g.permissions.map(p=>permissionNames[p]).join(', ')),tag(g.active?'ACTIVE':'REVOKED')
    ])))}</div>`;
}
function adminVenuesPanel() {
  const v=lookup(db.venues,venueEditorId);
  if (v) return `<div class="stack"><div class="cluster">${button('Danh sách','endVenue','','secondary')}${button('Sửa thông tin','venueForm',v.id)}${button('Khu vực & ghế','venueZones',v.id,'secondary')}${button('Chặn lịch','venueBlock',v.id,'secondary')}</div>
    ${panel(v.name,row(v.address,v.city+' · '+v.capacity+' khách · '+money(v.rate))+row('Tiện ích',v.amenities)+mapView(v.zones))}
    ${panel('Lịch khóa',v.blocks.map(b=>row(fmt(b.start)+'–'+fmt(b.end),b.reason,button('Mở lịch','releaseBlock',b.id,'danger compact',attr('venue',v.id)))).join('')||empty('Chưa chặn khung giờ.'))}</div>`;
  return `<div class="stack"><div>${button('Thêm địa điểm','venueForm')}</div>${panel('Danh mục địa điểm',db.venues.map(v=>row(v.name,v.city+' · '+v.capacity+' khách · '+money(v.rate),
    tag(v.state)+button('Quản lý','editVenue',v.id,'secondary compact')+button(v.state==='ACTIVE'?'Ẩn':'Hiện lại','hideVenue',v.id,'ghost compact'))).join(''))}</div>`;
}
function catalogPanel() {
  return `<div class="stack">${panel('Danh mục',db.categories.map(c=>row(c,'',button('Đổi tên / xóa','categoryEdit',c,'secondary compact'))).join('')+
    '<div style="margin-top:10px">'+button('Thêm danh mục','categoryEdit','','secondary')+'</div>')}
    ${panel('Banner trang chủ',db.banners.map(b=>row(b.title.replace('|',' '),b.text,tag(b.state)+button('Chỉnh sửa','banner',b.id,'secondary compact'))).join('')+
    '<div style="margin-top:10px">'+button('Thêm banner','banner','','secondary')+'</div>')}
    ${panel('Nội dung và quy định',row('Giới thiệu',db.content.about)+row('Hướng dẫn',db.content.guide)+row('Quy định',db.content.rules)+button('Chỉnh nội dung','platformText','','secondary'))}</div>`;
}
function policyPanel() {
  return `<div class="stack">${panel('Chính sách giao dịch và SLA',faq().map(([q,a])=>row(q,a)).join('')+
    row('Khóa sổ và phản hồi BTC',db.policy.settlementDays+' ngày · '+db.policy.settlementHours+' giờ')+
    row('Lịch ngày nghỉ',lines(db.policy.holiday).join(', ')||'Chỉ nghỉ cuối tuần')+button('Sửa chính sách','policy'))}</div>`;
}
function configPanel() {
  const s=db.settings;
  return `<div class="stack">${panel('Cấu hình phiên bản '+s.version,
    row('Tên miền / HTTPS',s.domain+' · '+(s.https?'Bật':'Tắt'))+row('SMTP / SMS',s.smtp+' · '+s.sms)+row('Liên hệ hỗ trợ',[s.supportPhone,s.supportEmail].filter(Boolean).join(' · ')||'Nhắn trong ứng dụng')+
    row('PayOS',s.payos)+row('Data Protection',s.protection)+row('Xác thực hai bước',s.twoFactor?'Bật trong mô phỏng':'Tắt')+
    row('Phiên đăng nhập',db.policy.sessionHours+' giờ · Mật khẩu tối thiểu '+db.policy.passwordLength+' ký tự')+
    button('Chỉnh cấu hình','config'))}
    ${panel('Lịch sử cấu hình',db.configHistory.map(v=>row('Phiên bản '+v.settings.version,v.reason+' · '+fmt(v.time),button('Khôi phục','restoreConfig',v.id,'secondary compact'))).join('')||empty('Chưa có thay đổi cấu hình.'))}</div>`;
}
function monitoringPanel() {
  const pending=db.transactions.filter(t=>t.state==='MANUAL_REVIEW').length;
  const deposits=db.rentals.filter(r=>r.state==='AWAITING_DEPOSIT').length;
  return `<div class="stack">
    ${metrics([['Giữ ghế đang mở',db.orders.filter(o=>heldOrder(o)).length],['Thanh toán cần đối soát',pending],['Cọc đang chờ',deposits],['Hoàn đang chuyển',db.refunds.filter(r=>r.state==='PROCESSING').length]])}
    ${panel('Dịch vụ mẫu',row('Worker giữ ghế / voucher','Kiểm tra khi mở hoặc thao tác trên giao diện',tag('ACTIVE'))+
      row('Hàng đợi PayOS',pending+' giao dịch',tag(pending?'MANUAL_REVIEW':'ACTIVE'))+
      row('Email / SMS',db.communications.length+' bản ghi mô phỏng')+row('Lệnh chi',db.payouts.filter(p=>p.state==='PROCESSING').length+' lệnh đang chuyển'))}
    ${panel('Cảnh báo và sự cố',db.alerts.map(a=>row(a.text,fmt(a.time),a.resolved?tag('RESOLVED'):button('Đã xử lý','resolveAlert',a.id,'secondary compact'))).join('')||
      empty('Chưa có sự cố cổng được ghi nhận.'))}
    ${panel('Nhật ký gửi mô phỏng',table(['Kênh','Nội dung','Thời gian','Trạng thái'],db.communications.slice(0,40).map(m=>[
      esc(m.channel),esc(m.subject),fmt(m.time),tag(m.state)
    ])))}</div>`;
}
function auditTable(records) {
  return table(['Thời gian','Tài khoản','Thao tác','Đối tượng','Nội dung'],records.map(a=>[
    fmt(a.time),esc(lookup(db.accounts,a.actor)?.name||a.actor),esc(a.action),esc(a.subject),esc(a.detail)
  ]));
}
function backupPanel() {
  return `<div class="stack">${panel('Sao lưu dữ liệu mẫu',`<div class="stack"><div class="cluster">${button('Xuất JSON','backupExport')}${button('Kiểm tra / phục hồi JSON','backupTest','','secondary')}</div>
    ${db.backups.map(b=>row(b.action,fmt(b.time)+' · '+b.note)).join('')||empty('Chưa có bản ghi sao lưu.')}</div>`)}
    ${panel('Nhật ký thao tác',auditTable(db.audit.slice(0,120)))}</div>`;
}


// Biểu mẫu người dùng và quản lý nghiệp vụ

const ui={accountSearch:'', returnAction:null};
function openDialog(title,content,submit=null,submitLabel='Lưu',{variant='',intro='',visual='',footer='',cancel=true}={}) {
  if (modal.open) modal.close();
  currentModal={submit};
  modal.dataset.variant=variant;
  if (intro) modal.setAttribute('aria-describedby','dialog-intro');
  else modal.removeAttribute('aria-describedby');
  modal.innerHTML=`${visual}<div class="dialog-inner"><header class="split"><h2 id="dialog-title">${esc(title)}</h2>
    <button type="button" class="btn square secondary" data-action="close" aria-label="Đóng">${ic('close',19)}</button></header>
    ${intro?`<p id="dialog-intro" class="dialog-intro">${esc(intro)}</p>`:''}
    ${submit?`<form data-form="dialog"><div class="stack">${content}<p class="notice problem form-error" role="alert"></p></div>
      <div class="cluster dialog-actions">${cancel?button('Đóng','close','','secondary'):''}<button class="btn" type="submit">${esc(submitLabel)}</button></div></form>`:content}
    ${footer}</div>`;
  modal.showModal();
}
function closeDialog() {
  if (modal.open) modal.close();
  modal.replaceChildren();
  currentModal=null;
}
function dismissDialog() {
  if (currentModal?.busy) return;
  ui.returnAction=null;
  closeDialog();
  refreshSharedState();
}
function go(path) {
  if (location.hash==='#'+path) {
    route=parseRoute();
    render();
  } else location.hash=path;
}
function parseRoute() {
  try {return (location.hash.slice(1)||'home').split('/').map(decodeURIComponent);}
  catch {return ['home'];}
}
function render() {
  const [page,id,sub]=route;
  const order=page==='checkout'?lookup(db.orders,id):null;
  const eventId=page==='event'||page==='book'?id:order&&order.user===account()?.id?order.eventId:'';
  const currentEvent=eventId?lookup(db.events,eventId):null;
  const event=publicEvent(currentEvent)?currentEvent:null;
  const availableSessions=page==='event'&&event?new Set(bookableSessions(event)):null;
  const pages={
    home:homeView,events:eventListView,calendar:()=>calendarView(id,sub),event:()=>detailView(event,availableSessions),venues:venueListView,venue:()=>venueDetails(id),
    guide:guideView,book:()=>chooseView(id,sub),checkout:()=>checkoutView(id),account:()=>accountView(id),
    work:()=>workView(id),invite:()=>inviteView(id),reset:()=>resetPassword(id)
  };
  const activeElement=document.activeElement;
  const seatFocus=activeElement?.matches('[data-action="seat"],[data-action="zone"],[data-change="seatRow"],.seat-scroll')?activeElement:null;
  const seatScroll=root.querySelector('.seat-scroll')?.scrollLeft||0;
  const isHome=page==='home'||!Object.hasOwn(pages,page);
  const content=isHome?homeView():pages[page]();
  root.innerHTML=header()+quickActions(event,availableSessions)+'<main id="main" class="site-main'+(isHome?' home-main':page==='work'?' work-main':'')+'">'+
    (isHome?content:'<div class="wrap">'+content+'</div>')+'</main>'+footer();
  if (page==='work') initWorkTools();
  const seatMap=root.querySelector('.seat-scroll');
  if (seatMap&&seatFocus?.dataset.action!=='zone') seatMap.scrollLeft=seatScroll;
  if (seatMap&&seatFocus) {
    const focus=[...root.querySelectorAll('[data-action="seat"],[data-action="zone"],[data-change="seatRow"],.seat-scroll')].find(el=>
      el.dataset.action===seatFocus.dataset.action&&el.dataset.id===seatFocus.dataset.id&&el.dataset.change===seatFocus.dataset.change);
    focus?.focus({preventScroll:true});
  }
}
function restoreOrRender() {
  if (route[0]==='book'&&route[1]&&!publicEvent(lookup(db.events,route[1]))) selection={eventId:'',sessionId:'',zone:'',seats:[]};
  render();
}
function guard(action,id,extra={}) {
  if (account()) return true;
  ui.returnAction=action ? {action,id,extra} : null;
  showAuth('login');
  return false;
}
function afterAuth() {
  const next=ui.returnAction;
  ui.returnAction=null;
  closeDialog();
  if (route[0]==='reset') go('home');
  else render();
  if (next) runAction(next.action,next.id,next.extra).catch(error=>notifyUi(error.message));
}
function own(id,collection,allow=false) {
  const item=lookup(collection,id);
  assert(item&&(item.user===account()?.id||allow),'Bản ghi nằm ngoài quyền truy cập.');
  return item;
}
const required = 'required maxlength="180"';
const textValue = (form,key) => String(form.get(key)||'').trim();
const choices = (values,selected) => values.map(value=>opt(value,value,selected)).join('');
function download(filename,content,mime) {
  const url=URL.createObjectURL(new Blob([content],{type:mime}));
  const anchor=document.createElement('a');
  anchor.href=url;
  anchor.download=filename;
  anchor.click();
  setTimeout(()=>URL.revokeObjectURL(url),1000);
}
async function passwordProof(secret,salt) {
  const data=new TextEncoder().encode(salt+'|'+secret);
  if (crypto.subtle) {
    const key=await crypto.subtle.importKey('raw',new TextEncoder().encode(secret),'PBKDF2',false,['deriveBits']);
    const derived=await crypto.subtle.deriveBits({name:'PBKDF2',salt:new TextEncoder().encode(salt),iterations:100000,hash:'SHA-256'},key,256);
    return Array.from(new Uint8Array(derived),byte=>byte.toString(16).padStart(2,'0')).join('');
  }
  return 'local-'+String(data.reduce((value,byte)=>Math.imul(value^byte,16777619)>>>0,2166136261));
}
async function setDemoPassword(user,secret) {
  user.salt=uid();
  user.password=await passwordProof(secret,user.salt);
}
async function validPassword(user,secret) {
  return user.password ? user.password===await passwordProof(secret,user.salt) : demoAccounts.some(a=>a.id===user.id)&&secret==='Venue2026!';
}
function contactsUnique(form,user) {
  const email=textValue(form,'email').toLowerCase();
  const phone=textValue(form,'phone');
  assert(!db.accounts.some(a=>a.id!==user?.id&&(a.email===email||a.phone===phone)),'Email hoặc số điện thoại đã tồn tại.');
  return {email,phone};
}
const loginPrompts={
  support:'Sau khi đăng nhập, bạn có thể gửi câu hỏi và theo dõi phản hồi từ đội hỗ trợ VÉNUE.',
  rental:'Đăng nhập để gửi hồ sơ thuê địa điểm và theo dõi kết quả xét duyệt.',
  favorite:'Đăng nhập để lưu sự kiện vào danh sách yêu thích.',
  claim:'Đăng nhập để thêm vé mời vào mục Vé của tôi.',
  application:'Đăng nhập để gửi và theo dõi hồ sơ Ban tổ chức.'
};
function loginLayout(intro,footer='') {
  return {
    variant:'login',intro,footer,cancel:false,
    visual:`<div class="login-visual" aria-hidden="true">
      <img src="../assets/images/concert.webp" alt="">
      <span class="logo"><span class="logo-mark">✳</span>vénue</span>
    </div>`
  };
}
function showAuth(mode,context='') {
  if (mode==='login') {
    const intro=loginPrompts[ui.returnAction?.action]||context||'Xem vé, đơn hàng và yêu cầu của bạn trong cùng một tài khoản.';
    const footer=`<div class="login-footer"><p>Chưa có tài khoản? ${button('Tạo tài khoản','register','','login-link')}</p>
      ${button('Dùng tài khoản mẫu','demo','','login-link login-demo')}</div>`;
    openDialog('Đăng nhập',`
      ${field('Email hoặc số điện thoại',input('login','','text','required autocomplete="username" autocapitalize="none" spellcheck="false" autofocus'))}
      <div class="field">
        <label for="login-password">Mật khẩu</label>
        <div class="login-password">
          ${input('password','','password','id="login-password" required autocomplete="current-password"')}
          <button type="button" class="password-toggle" data-password-toggle aria-label="Hiện mật khẩu" aria-controls="login-password">Hiện</button>
        </div>
      </div>
      ${button('Quên mật khẩu?','forgot','','login-link login-forgot')}`,
    async form=>{
      const login=textValue(form,'login').toLowerCase();
      const user=db.accounts.find(a=>a.email===login||a.phone===login);
      assert(user&&await validPassword(user,String(form.get('password'))),'Thông tin đăng nhập không đúng.');
      assert(!user.locked,'Tài khoản đã bị khóa.');
      if (db.settings.twoFactor) {
        const code='482601';
        sendMock('Email',user.email,'Mã xác thực hai bước',code);
        openDialog('Xác thực hai bước',notice('Mã xác thực mẫu: '+code)+field('Mã xác thực',input('code','','text','required inputmode="numeric" pattern="[0-9]{6}" autocomplete="one-time-code" autofocus')),f=>{
          const current=lookup(db.accounts,user.id);
          assert(current&&!current.locked&&textValue(f,'code')===code,'Mã xác thực không đúng hoặc tài khoản đã khóa.');
          setAuth(current);
          audit('Đăng nhập',current.id);
          afterAuth();
        },'Xác minh',loginLayout('Nhập mã xác thực để hoàn tất đăng nhập.'));
        return false;
      }
      setAuth(user);
      audit('Đăng nhập',user.id);
      afterAuth();
    },'Đăng nhập',loginLayout(intro,footer));
    return;
  }
  if (mode==='register') {
    const note=notice('Dùng thông tin giả để thử biểu mẫu. Các bước xác minh và gửi thư chạy trong bản mẫu.');
    openDialog('Tạo tài khoản',`${note}<div class="form-grid">
      ${field('Họ tên',input('name','','text',required),true)}
      ${field('Email',input('email','','email',required+' autocomplete="email"'))}
      ${field('Điện thoại',input('phone','','tel','required pattern="[0-9+]{8,15}" autocomplete="tel"'))}
      ${field('Mật khẩu',input('password','','password','required autocomplete="new-password" minlength="'+db.policy.passwordLength+'"'))}
      ${field('Nhập lại mật khẩu',input('confirm','','password','required autocomplete="new-password" minlength="'+db.policy.passwordLength+'"'))}
      <div class="span">${check('terms','Tôi đồng ý với điều khoản sử dụng VÉNUE',false,'yes',true)}</div></div>
      <div>${button('Đã có tài khoản','login','','ghost compact')}</div>`,async form=>{
      const password=String(form.get('password')||'');
      assert(password.length>=db.policy.passwordLength&&password===form.get('confirm'),'Mật khẩu và xác nhận phải trùng nhau.');
      const user={id:uid('U-').slice(0,15),name:textValue(form,'name'),...contactsUnique(form),role:'user',verified:!db.policy.authVerify};
      assert(user.name&&form.get('terms'),'Cần điền tên và xác nhận điều khoản.');
      await setDemoPassword(user,password);
      db.accounts.push(user);
      setAuth(user);
      audit('Đăng ký tài khoản',user.id);
      afterAuth();
      notifyUi('Đã tạo tài khoản Người dùng.');
    },'Đăng ký');
    return;
  }
  openDialog('Quên mật khẩu',field('Email hoặc số điện thoại',input('login','','text',required)),form=>{
    const value=textValue(form,'login').toLowerCase();
    const user=db.accounts.find(a=>a.email===value||a.phone===value);
    let body=notice('Yêu cầu đã được tiếp nhận. Liên kết mẫu dùng một lần trong 30 phút.');
    if (user&&!user.locked) {
      db.resets.filter(r=>r.user===user.id).forEach(r=>{r.used=true;});
      const reset={token:uid('RESET-'),user:user.id,until:Date.now()+30*minute,used:false};
      db.resets.push(reset);
      const path='reset/'+reset.token;
      sendMock('Email',user.email,'Đặt lại mật khẩu','Mở #'+path);
      body+=`<div style="margin-top:14px">${link('Mở liên kết đặt lại mẫu',path,'btn')}</div>`;
    }
    openDialog('Hộp thư mô phỏng',body);
    return false;
  },'Gửi liên kết');
}
function resetPassword(token) {
  const reset=db.resets.find(r=>r.token===token);
  if (!reset||reset.used||reset.until<Date.now()) return pageTitle('Liên kết đã hết hiệu lực')+button('Yêu cầu liên kết mới','forgot');
  return pageTitle('Đặt lại mật khẩu','Liên kết có hiệu lực trong 30 phút và dùng một lần.')+panel('',button('Chọn mật khẩu mới','performReset',token));
}
function passwordForm(title,user,resetToken='') {
  openDialog(title,`<div class="form-grid">
    ${resetToken?'':field('Mật khẩu hiện tại',input('old','','password','required autocomplete="current-password"'),true)}
    ${field('Mật khẩu mới',input('password','','password','required minlength="'+db.policy.passwordLength+'" autocomplete="new-password"'))}
    ${field('Nhập lại mật khẩu',input('confirm','','password','required autocomplete="new-password"'))}</div>`,async form=>{
    const current=lookup(db.accounts,user.id);
    assert(current&&!current.locked,'Tài khoản không thể đổi mật khẩu.');
    const reset=resetToken&&db.resets.find(r=>r.token===resetToken);
    if (resetToken) assert(reset&&reset.user===current.id&&!reset.used&&reset.until>Date.now(),'Liên kết đã hết hiệu lực.');
    else assert(account()?.id===current.id&&await validPassword(current,String(form.get('old'))),'Mật khẩu hiện tại không đúng.');
    const password=String(form.get('password')||'');
    assert(password.length>=db.policy.passwordLength&&password===form.get('confirm'),'Mật khẩu mới hoặc xác nhận không hợp lệ.');
    await setDemoPassword(current,password);
    if (reset) reset.used=true;
    db.sessionVersions[current.id]=(db.sessionVersions[current.id]||0)+1;
    audit('Cập nhật mật khẩu',current.id);
    setAuth(resetToken?null:current);
    if (resetToken) go('home');
    notifyUi('Đã cập nhật mật khẩu.');
  },'Đổi mật khẩu');
}
function profileForm() {
  const user=account();
  openDialog('Cập nhật hồ sơ',`<div class="form-grid">${field('Họ tên',input('name',user.name,'text',required),true)}
    ${field('Email',input('email',user.email,'email',required))}
    ${field('Điện thoại',input('phone',user.phone,'tel','required pattern="[0-9+]{8,15}"'))}</div>`,form=>{
    const current=account();
    assert(current?.id===user.id,'Phiên đăng nhập đã kết thúc.');
    const data=contactsUnique(form,current);
    const name=textValue(form,'name');
    assert(name,'Cần nhập họ tên.');
    if (current.email!==data.email||current.phone!==data.phone) current.verified=false;
    Object.assign(current,data,{name});
    audit('Cập nhật hồ sơ',current.id);
  });
}
function contactForm(claim='') {
  const user=account();
  const t=claim&&db.tickets.find(t=>t.token===claim);
  const target=t?.contact||user.email;
  assert(!t||[user.email,user.phone].includes(t.contact),'Tài khoản cần có đúng email hoặc số điện thoại được mời.');
  sendMock(target.includes('@')?'Email':'SMS',target,'Xác minh liên hệ','Mã mẫu: 731652');
  openDialog(claim?'Liên kết vé mời':'Xác minh liên hệ',notice('Mã xác minh trong bản mẫu là 731652.')+
    field('Mã xác minh',input('code','','text','required pattern="[0-9]{6}" inputmode="numeric"')),form=>{
      const current=account();
      assert(current?.id===user.id&&textValue(form,'code')==='731652','Mã xác minh hoặc phiên đăng nhập không hợp lệ.');
      if (claim) {
        const ticket=db.tickets.find(x=>x.token===claim);
        assert(ticket&&validTicket(ticket)&&!ticket.user&&ticket.inviteUntil>Date.now()&&[current.email,current.phone].includes(ticket.contact),'Vé mời không thể liên kết.');
        ticket.user=current.id;
        message(current.id,'Đã liên kết vé mời',ticket.id);
        audit('Liên kết vé mời',ticket.id);
      }
      current.verified=true;
      audit('Xác minh liên hệ',current.id);
      const next=ui.verifiedAction;
      ui.verifiedAction=null;
      if (next) runAction(next.action,next.id).catch(error=>notifyUi(error.message));
    },'Xác minh');
}
function decision(title,options,saveDecision,extra='') {
  openDialog(title,`<div class="form-grid">${field('Kết quả',select('result',options.map(([value,label])=>opt(value,label)).join('')),true)}${extra}
    ${field('Lý do / nhận xét',area('note','','required maxlength="1000"'),true)}</div>`,form=>{
    const note=textValue(form,'note');
    assert(note,'Cần ghi lý do hoặc nhận xét.');
    return saveDecision(textValue(form,'result'),note,form);
  },'Xác nhận');
}
function applicationDecision(id) {
  assert(canManage('applications'),'Không có quyền xét duyệt.');
  decision('Xét duyệt hồ sơ BTC',[['APPROVED','Phê duyệt'],['NEEDS_INFO','Yêu cầu bổ sung'],['REJECTED','Từ chối']],async (state,note)=>{
    const app=lookup(db.applications,id);
    assert(canManage('applications')&&app?.state==='PENDING','Hồ sơ đã được xử lý hoặc ngoài phạm vi.');
    const profile=state==='APPROVED'&&app.profileId?await readOrganizerProfile(app):null;
    assert(canManage('applications'),'Phiên xét duyệt đã kết thúc.');
    app.state=state;app.note=note;app.history.unshift({state,note,time:Date.now()});
    if (state==='APPROVED') {
      const user=lookup(db.accounts,app.user);
      const bank=profile?[organizerBanks[profile.values.bankName],profile.values.bankAccount,profile.values.bankOwner,profile.values.bankBranch].filter(Boolean).join(' · '):app.bank;
      if (user?.role==='user'||user?.role==='organizer') Object.assign(user,{role:'organizer',bank,bankState:'Đã xác minh trong bản mẫu'});
    }
    message(app.user,'Kết quả hồ sơ BTC',status[state][0]+' · '+note);
    audit('Xét duyệt hồ sơ BTC',id,state+' · '+note);
  });
}
function venueCalendar(id) {
  const venue=lookup(db.venues,id);
  assert(venue?.state==='ACTIVE','Địa điểm không khả dụng.');
  const start=parseLocal(dayKey(Date.now()))+day+9*hour;
  openDialog('Kiểm tra khung giờ',`<div class="form-grid">${field('Bắt đầu',input('start',localInput(start),'datetime-local','required'))}
    ${field('Kết thúc',input('end',localInput(start+hour),'datetime-local','required'))}</div><p id="venue-result" class="notice" hidden></p>`,form=>{
    const start=parseLocal(textValue(form,'start')),end=parseLocal(textValue(form,'end'));
    requireFuture(start,end);
    const result=modal.querySelector('#venue-result');
    result.hidden=false;
    result.textContent=rentalConflict(lookup(db.venues,id),start,end)?'Khung giờ đã có lịch.':'Khung giờ hiện đang trống.';
    return false;
  },'Kiểm tra');
}
function depositDialog(id) {
  const r=own(id,db.rentals);
  assert(r.state==='AWAITING_DEPOSIT'&&r.deadline>Date.now(),'Hạn đặt cọc đã kết thúc.');
  openDialog('Thanh toán cọc mô phỏng',`<div class="ticket-preview">${qrMarkup('VENUE-DEPOSIT-'+r.id,'QR cọc mô phỏng')}<strong>${money(r.deposit)}</strong><p class="small muted">Hạn ${fmt(r.deadline)}</p></div>
    ${field('Mã chứng từ mẫu',input('proof','COC-'+id,'text',required))}`,form=>{
      const current=own(id,db.rentals);
      assert(current.state==='AWAITING_DEPOSIT'&&current.deadline>Date.now(),'Hạn đặt cọc đã kết thúc.');
      current.proof=textValue(form,'proof');current.state='DEPOSIT_REVIEW';
      message(current.user,'Đã ghi nhận cọc mẫu',current.id+' · Chờ đối soát');
      audit('Ghi nhận chứng từ cọc',id);
    },'Gửi chứng từ');
}
function cancellationRate(rental) {
  if (!rental.paidAt) return 0;
  const days=(rental.start-Date.now())/day;
  return days>=rental.earlyDays?rental.earlyPercent:days>=rental.midDays?rental.midPercent:0;
}
function rentalCancel(id) {
  const r=own(id,db.rentals);
  openDialog('Yêu cầu hủy thuê',notice('Tạm tính hoàn '+cancellationRate(r)+'% cọc · '+money(r.deposit*cancellationRate(r)/100))+
    field('Lý do',area('reason','','required maxlength="500"')),form=>{
    const current=own(id,db.rentals);
    assert(['REQUESTED','AWAITING_DEPOSIT','CONFIRMED'].includes(current.state)&&current.start>Date.now(),'Yêu cầu thuê không thể hủy.');
    current.cancelPercent=cancellationRate(current);
    current.cancelAmount=Math.round(current.deposit*current.cancelPercent/100);
    current.cancelFrom=current.state;
    current.cancelReason=textValue(form,'reason');
    if (!current.paidAt) current.state='CANCELLED';
    else current.state='CANCELLATION_REQUESTED';
    audit('Yêu cầu hủy thuê',id);
  },'Gửi yêu cầu');
}
function rentalDecision(id) {
  const r=lookup(db.rentals,id);
  assert(canManage('rentals')&&r,'Không có quyền xử lý yêu cầu.');
  const transitions={
    REQUESTED:[['AWAITING_DEPOSIT','Duyệt giữ lịch'],['REJECTED','Từ chối']],
    DEPOSIT_REVIEW:[['CONFIRMED','Đã đối soát cọc'],['ESCALATED','Chuyển tranh chấp lên Admin']],
    CANCELLATION_REQUESTED:[['CANCELLED','Duyệt hủy và tiếp nhận hoàn cọc'],['RESTORE','Bác yêu cầu'],['ESCALATED','Chuyển Admin']],
    ESCALATED:r.cancelFrom?[['CANCELLED','Duyệt hủy'],['RESTORE','Bác yêu cầu']]:[['CONFIRMED','Xác nhận cọc'],['REJECTED','Từ chối']]
  };
  assert(transitions[r.state]&&(r.state!=='ESCALATED'||account().role==='admin'),'Trạng thái chưa thể xử lý.');
  decision('Xử lý thuê địa điểm',transitions[r.state],(state,note)=>{
    const current=lookup(db.rentals,id);
    assert(canManage('rentals')&&current.state===r.state,'Hồ sơ đã đổi trạng thái.');
    if (state==='AWAITING_DEPOSIT') {
      const venue=lookup(db.venues,current.venueId);
      assert(venue?.state==='ACTIVE'&&current.capacity<=venue.capacity,'Địa điểm hoặc sức chứa đã thay đổi.');
      assert(current.start>Date.now()&&!rentalConflict(venue,current.start,current.end,id),'Khung giờ đã qua hoặc có lịch trùng.');
      current.deadline=Date.now()+current.depositHours*hour;
    }
    if (state==='CONFIRMED') {assert(current.proof,'Chưa có chứng từ cọc.');current.paidAt=Date.now();}
    if (state==='CANCELLED'&&current.cancelAmount>0) queueRefund({user:current.user,rentalId:id,reason:'Hoàn cọc thuê '+id,amount:current.cancelAmount,eventId:current.eventId,state:'MANUAL_REVIEW'});
    if (state==='RESTORE') {state=current.cancelFrom;current.cancelAmount=null;current.cancelPercent=null;current.cancelFrom=null;}
    current.state=state;current.note=note;
    message(current.user,'Cập nhật yêu cầu thuê',status[state][0]+' · '+note);
    audit('Xử lý yêu cầu thuê',id,state);
  });
}
function refundDecision(id) {
  const r=lookup(db.refunds,id);
  assert(canManage('refunds')&&r,'Không có quyền xử lý hoàn tiền.');
  const transitions={
    REQUESTED:[['APPROVED','Duyệt hoàn'],['REJECTED','Từ chối']],
    MANUAL_REVIEW:[['APPROVED','Duyệt hoàn'],['REJECTED','Từ chối']],
    AUTO_APPROVED:[['PROCESSING','Lập giao dịch hoàn']],APPROVED:[['PROCESSING','Lập giao dịch hoàn']],
    PROCESSING:[['COMPLETED','Xác nhận đã chuyển thành công'],['FAILED','Giao dịch thất bại']],
    FAILED:[['PROCESSING','Thử chuyển lại']]
  };
  assert(transitions[r.state],'Yêu cầu hoàn đã xử lý xong.');
  decision('Xử lý hoàn tiền '+id,transitions[r.state],(result,note,form)=>{
    const refund=lookup(db.refunds,id);
    assert(canManage('refunds')&&refund.state===r.state,'Trạng thái đã thay đổi.');
    const proof=textValue(form,'proof');
    assert(result!=='COMPLETED'||proof,'Cần mã chứng từ để xác nhận hoàn tiền thành công.');
    refund.state=result;refund.note=note;
    if (result==='PROCESSING') refund.transferId=refund.transferId||'REFUND-'+refund.id;
    if (result==='COMPLETED') {
      refund.proof=proof;
      for (const id of refund.ticketIds||[]) {const ticket=lookup(db.tickets,id); if(ticket) ticket.state='REFUNDED';}
      for (const orderId of refundOrderIds(refund)) {
        const order=lookup(db.orders,orderId);
        if (order&&!db.tickets.some(t=>t.orderId===orderId&&validTicket(t))&&
          !db.refunds.some(x=>!['COMPLETED','REJECTED'].includes(x.state)&&refundOrderIds(x).includes(orderId))) order.state='REFUNDED';
      }
    }
    message(refund.user,'Yêu cầu hoàn '+id,status[result][0]+' · '+note);
    audit('Xử lý hoàn tiền',id,result);
  },field('Mã chứng từ khi chuyển thành công',input('proof','','text','maxlength="100"'),true));
}
function supportForm(id='',contextId='') {
  const user=account();
  const ticket=id?own(id,db.tickets):null;
  const context=publicEvent(lookup(db.events,contextId))?contextId:'';
  const events=new Set(db.tickets.filter(t=>t.user===user.id).map(t=>t.eventId));
  if (context) events.add(context);
  openDialog(ticket?'Khiếu nại vé '+id:'Tạo yêu cầu hỗ trợ',`<div class="form-grid">
  ${field('Chủ đề',input('subject',ticket?'Sự cố vé '+id:'','text',required),true)}
  ${ticket?'':field('Sự kiện',select('eventId',opt('','Hỗ trợ chung',context)+[...events].map(id=>opt(id,eventTitle(id),context)).join('')),true)}
  ${field('Nội dung',area('text','','required maxlength="1200"'),true)}</div>`,form=>{
    assert(account()?.id===user.id,'Phiên đăng nhập đã kết thúc.');
    const eventId=ticket?.eventId||textValue(form,'eventId');
    assert(ticket||!eventId||eventId===context&&publicEvent(lookup(db.events,eventId))||
      db.tickets.some(t=>t.user===user.id&&t.eventId===eventId),'Sự kiện không khả dụng cho yêu cầu này.');
    const subject=textValue(form,'subject'), text=textValue(form,'text');
    assert(subject&&text,'Điền chủ đề và nội dung hỗ trợ.');
    const item={id:uid('CS-').slice(0,15),state:'OPEN',user:user.id,eventId,sessionId:ticket?.sessionId||'',ticketId:ticket?.id||'',
      subject,text,time:Date.now()};
    db.support.unshift(item);
    message(user.id,'Đã mở yêu cầu hỗ trợ',item.id);
    audit('Mở yêu cầu hỗ trợ',item.id);
    go('account/support');
  },'Gửi hỗ trợ');
}
function supportDecision(id) {
  const item=lookup(db.support,id);
  assert(item&&(canManage('support')||ownerEvent(lookup(db.events,item.eventId))),'Yêu cầu nằm ngoài phạm vi được xử lý.');
  decision('Xử lý '+item.subject,[['OPEN','Phản hồi và theo dõi'],['RESOLVED','Đóng yêu cầu']],(state,note)=>{
    const current=lookup(db.support,id);
    assert(current&&(canManage('support')||ownerEvent(lookup(db.events,current.eventId))),'Không có quyền xử lý.');
    current.state=state; current.answer=note;
    message(current.user,'Phản hồi hỗ trợ '+id,note);
    audit('Phản hồi hỗ trợ',id,state);
  },'<p class="span muted">'+esc(item.text)+'</p>');
}


// Tổ chức sự kiện và vận hành cổng

function requireOwner(id) {
  const event=lookup(db.events,id);
  assert(ownerEvent(event),'Sự kiện nằm ngoài phạm vi của Ban tổ chức.');
  return event;
}
function editable(event) {
  assert(event.state!=='PENDING'&&event.draftState!=='PENDING_UPDATE'&&event.state!=='CANCELLED','Phiên bản đang chờ duyệt hoặc sự kiện đã hủy.');
  if (!event.draft) {
    event.draft=clone(event.live);
    event.draftState='DRAFT';
  }
  return event.draft;
}
function eventForm(id='') {
  assert(account()?.role==='organizer','Chỉ Ban tổ chức được duyệt mới có quyền tạo sự kiện.');
  const event=id?requireOwner(id):null;
  const e=event?.draft||event?.live||{};
  openDialog(id?'Sửa hồ sơ sự kiện':'Tạo sự kiện',`<div class="form-grid">
    ${field('Tên sự kiện',input('title',e.title,'text',required),true)}
    ${field('Danh mục',select('cat',choices(db.categories,e.cat)))}
    ${field('Ảnh bìa mẫu',select('imageKey',Object.keys(images).map((key,i)=>opt(key,events[i]?.title||key,e.imageKey)).join('')))}
    ${field('Ảnh bìa riêng (PNG, JPEG hoặc WebP)',input('cover','','file','accept="image/png,image/jpeg,image/webp"'),true)}
    ${e.cover?'<div class="span">'+check('dropCover','Dùng lại ảnh mẫu nếu không chọn ảnh mới')+'</div>':''}
    ${field('Mô tả',area('description',e.description,'required maxlength="2000"'),true)}
    ${field('Nghệ sĩ / diễn giả',input('participants',e.participants,'text','maxlength="220"'),true)}
    ${field('Điều khoản',area('terms',e.terms,'maxlength="1000"'),true)}
    ${field('Chương trình',area('program',e.program,'required maxlength="1000"'))}
    ${field('Quy định',area('rules',e.rules,'required maxlength="1000"'))}
    ${field('Liên hệ BTC',input('contact',e.contact||account().email,'text',required),true)}
    <div class="span stack">${check('refund','Cho phép tự hủy trước hạn chính sách',e.refund!==false)}
    ${check('visibleStock','Công khai số ghế khả dụng',e.visibleStock!==false)}
    ${check('venueRequired','Yêu cầu lịch thuê đã xác nhận trước khi duyệt',e.venueRequired)}
    ${field('Giấy tờ sự kiện (mẫu)',input('docs','','file','multiple accept=".pdf,.png,.jpg,.jpeg"'))}</div></div>`,async (form,element)=>{
      const user=account();
      assert(user?.role==='organizer','Tài khoản chưa được cấp quyền Ban tổ chức.');
      const current=id?requireOwner(id):{id:uid('EV-').slice(0,15),owner:user.id,state:'DRAFT',version:0,versions:[],notices:[],request:null,live:null,draft:{sessions:[]}};
      const draft=editable(current);
      for (const key of ['title','cat','imageKey','description','program','rules','contact','participants','terms']) draft[key]=textValue(form,key);
      assert(draft.title&&draft.description&&db.categories.includes(draft.cat)&&draft.contact,'Điền tên, danh mục, mô tả và liên hệ hợp lệ.');
      Object.assign(draft,{refund:form.has('refund'),visibleStock:form.has('visibleStock'),venueRequired:form.has('venueRequired')});
      const cover=element.elements.cover.files[0];
      if (cover) draft.cover=await prepareCoverImage(cover);
      else if (form.has('dropCover')) delete draft.cover;
      const docs=material(element.elements.docs);
      assert(docs.every(name=>/\.(pdf|png|jpe?g)$/i.test(name)),'Giấy tờ phải có định dạng PDF, PNG hoặc JPEG.');
      if (docs.length) draft.docs=docs;
      if (!id) db.events.unshift(current);
      else if (current.state==='REJECTED') current.state='DRAFT';
      if (current.live) current.draftState='DRAFT';
      audit(id?'Lưu bản chỉnh sửa':'Tạo sự kiện nháp',current.id);
      editorId=current.id;editorSection='general';
    },'Lưu bản nháp');
}
async function prepareCoverImage(file) {
  assert(['image/png','image/jpeg','image/webp'].includes(file.type)&&file.size<=8*1024*1024,'Chọn ảnh PNG, JPEG hoặc WebP dưới 8 MB.');
  const bitmap=await createImageBitmap(file);
  try {
    const limit=Math.min(1,960/Math.max(bitmap.width,bitmap.height));
    const canvas=document.createElement('canvas');
    canvas.width=Math.max(1,Math.round(bitmap.width*limit));
    canvas.height=Math.max(1,Math.round(bitmap.height*limit));
    canvas.getContext('2d').drawImage(bitmap,0,0,canvas.width,canvas.height);
    const cover=canvas.toDataURL('image/webp',.72);
    assert(cover.length<=230000,'Ảnh nén vẫn quá lớn. Chọn ảnh bìa có dung lượng thấp hơn.');
    return cover;
  } finally {bitmap.close();}
}
function stringifyZones(zones) {
  return zones.map(z=>z.name+' | '+z.capacity+' | '+z.seats.map(s=>s.code+':'+s.type+(s.group?':'+s.group:'')+(s.blocked?'!':'')).join(' ')).join('\n');
}
function parseZones(text,limit) {
  const seen=new Set();
  const zoneNames=new Set();
  const zones=text.split('\n').map(line=>line.trim()).filter(Boolean).map(line=>{
    const [name,rawCapacity,rawSeats]=line.split('|').map(value=>value.trim());
    const capacity=Number(rawCapacity);
    assert(name&&Number.isInteger(capacity)&&capacity>0&&rawSeats&&!zoneNames.has(normalized(name)),'Dòng khu vực cần có tên riêng, sức chứa và danh sách ghế.');
    zoneNames.add(normalized(name));
    const seats=rawSeats.split(/[ ,]+/).filter(Boolean).map(encoded=>{
      const blocked=encoded.endsWith('!');
      const [code,type='Standard',group='']=encoded.replace(/!$/,'').split(':');
      assert(/^[A-Za-z0-9_-]{1,16}$/.test(code)&&['VIP','Standard'].includes(type)&&!seen.has(code.toUpperCase()),'Mã ghế trùng hoặc sai loại vé: '+encoded);
      seen.add(code.toUpperCase());
      return {code:code.toUpperCase(),type,group,blocked};
    });
    assert(seats.length>0&&capacity<=seats.length,'Sức chứa mỗi khu phải nằm trong số ghế đã khai báo.');
    return {name,capacity,seats};
  });
  assert(zones.length&&zones.reduce((sum,z)=>sum+z.capacity,0)<=limit,'Sức chứa các khu vượt sức chứa địa điểm hoặc thiếu khu vực.');
  return zones;
}
function zoneHelp() {
  return notice('Mỗi dòng: Tên khu | Sức chứa | A1:VIP:doi A2:VIP:doi A3:VIP! . Dùng cùng tên nhóm cho ghế đôi hoặc cụm. Dấu ! đánh dấu ghế khóa.');
}
function defaultTypes() {
  return ['Standard','VIP','Invitation'].map((name,i)=>({name,price:[300000,650000,0][i],quantity:[24,12,8][i],benefits:'',enabled:true,from:null,to:null}));
}
function sessionForm(eventId,id='') {
  const event=requireOwner(eventId);
  const ses=(event.draft||event.live).sessions.find(s=>s.id===id);
  const now=Date.now();
  const start=parseLocal(dayKey(now))+(25+(event.draft||event.live).sessions.length)*day+18*hour;
  const venues=db.venues.filter(v=>v.state==='ACTIVE'||v.id===ses?.venueId);
  const venue=lookup(venues,ses?.venueId)||venues[0];
  assert(venue,'Cần có địa điểm trong danh mục.');
  openDialog(ses?'Cấu hình suất diễn':'Thêm suất diễn',`<div class="form-grid">
    ${field('Tên suất',input('name',ses?.name,'text',required))}
    ${field('Địa điểm',select('venueId',venues.map(v=>opt(v.id,v.name,venue.id)).join(''),'data-change="sessionVenue"'))}
    ${field('Bắt đầu (giờ Việt Nam)',input('start',localInput(ses?.start||start),'datetime-local','required'))}
    ${field('Kết thúc (giờ Việt Nam)',input('end',localInput(ses?.end||start+2*hour),'datetime-local','required'))}
    ${field('Mở bán',input('from',localInput(ses?.from||now-minute),'datetime-local','required'))}
    ${field('Đóng bán',input('to',localInput(ses?.to||start-15*minute),'datetime-local','required'))}
    <div class="span">${zoneHelp()}</div>
    ${field('Khu vực, sức chứa và ghế',area('zones',stringifyZones(ses?.zones||venue.zones),'required spellcheck="false"'),true)}
    <div class="span">${check('open','Cho phép mở bán trong khung giờ đã chọn',ses?.open!==false)}</div></div>`,form=>{
      const current=requireOwner(eventId);
      const draft=editable(current);
      const place=lookup(db.venues,textValue(form,'venueId'));
      assert(place?.state==='ACTIVE','Địa điểm không hoạt động.');
      const [start,end,from,to]=['start','end','from','to'].map(key=>parseLocal(textValue(form,key)));
      requireFuture(start,end);requireFuture(from,to);
      assert(to<=start,'Giờ đóng bán phải trước hoặc bằng giờ bắt đầu.');
      const name=textValue(form,'name');
      assert(name,'Cần nhập tên suất.');
      const previous=draft.sessions.find(s=>s.id===id);
      const next={id:previous?.id||uid('S-').slice(0,15),venueId:place.id,name,start,end,from,to,
        zones:parseZones(textValue(form,'zones'),place.capacity),open:form.has('open'),types:previous?.types||defaultTypes()};
      if (previous) Object.assign(previous,next); else draft.sessions.push(next);
      audit('Lưu suất diễn nháp',eventId,next.id);
    });
}
function typeForm(eventId,id) {
  const event=requireOwner(eventId);
  const ses=(event.draft||event.live).sessions.find(s=>s.id===id);
  assert(ses,'Suất diễn không tồn tại.');
  const types=['Standard','VIP','Invitation'];
  openDialog('Loại vé · '+ses.name,types.map(name=>{
    const t=ses.types.find(t=>t.name===name)||defaultTypes().find(type=>type.name===name);
    return `<div class="panel"><h3>${name}</h3><div class="form-grid">
      ${name==='Invitation'?'<p class="small muted">Vé mời không mở bán công khai và có giá 0 đ.</p>':
        field('Giá (đ)',input(name+'price',t.price,'number','required min="0" step="1"'))}
      ${field('Số vé tối đa',input(name+'quantity',t.quantity,'number','required min="0" step="1"'))}
      ${field('Mở bán riêng',input(name+'from',t.from?localInput(t.from):'','datetime-local'))}
      ${field('Đóng bán riêng',input(name+'to',t.to?localInput(t.to):'','datetime-local'))}
      ${field('Quyền lợi',input(name+'benefits',t.benefits,'text','maxlength="200"'),true)}
      <div class="span">${check(name+'enabled','Cho phép sử dụng',t.enabled)}</div></div></div>`;
  }).join(''),form=>{
    const current=editable(requireOwner(eventId)).sessions.find(s=>s.id===id);
    assert(current,'Suất diễn không tồn tại.');
    current.types=types.map(name=>{
      const fromText=textValue(form,name+'from'),toText=textValue(form,name+'to');
      const from=fromText?parseLocal(fromText):null,to=toText?parseLocal(toText):null;
      assert((!from||from>=current.from)&&(!to||to<=current.to)&&(from||current.from)<(to||current.to),'Thời gian bán của '+name+' phải nằm trong khung bán của suất.');
      return {name,price:name==='Invitation'?0:currencyNumber(form,name+'price'),quantity:currencyNumber(form,name+'quantity'),
        benefits:textValue(form,name+'benefits'),enabled:form.has(name+'enabled'),from,to};
    });
    audit('Cấu hình loại vé',eventId,id);
  });
}
function ensureEventReady(event,info) {
  assert(info?.title&&info.description&&info.program&&info.rules&&info.sessions.length,'Cần hoàn thiện hồ sơ và ít nhất một suất diễn.');
  for (const s of info.sessions) {
    const venue=lookup(db.venues,s.venueId);
    assert(venue?.state==='ACTIVE'&&s.zones.reduce((n,z)=>n+z.capacity,0)<=venue.capacity,'Địa điểm hoặc sức chứa của '+s.name+' không hợp lệ.');
    assert(s.start<s.end&&s.from<s.to&&s.to<=s.start,'Lịch của '+s.name+' không hợp lệ.');
    assert(!info.sessions.some(other=>other.id!==s.id&&other.venueId===s.venueId&&s.start<other.end&&s.end>other.start),'Có hai suất chồng giờ ở cùng địa điểm.');
    assert(!venue.blocks.some(b=>s.start<b.end&&s.end>b.start)&&
      !eventVenueConflict(venue.id,s.start,s.end,event.id)&&
      !db.rentals.some(r=>r.eventId!==event.id&&r.venueId===venue.id&&rentalBlocking(r)&&s.start<r.end&&s.end>r.start),'Lịch của '+s.name+' xung đột với lịch địa điểm.');
    assert(!info.venueRequired||db.rentals.some(r=>r.eventId===event.id&&r.venueId===s.venueId&&r.state==='CONFIRMED'&&r.start<=s.start&&r.end>=s.end),
      'Cần xác nhận thuê địa điểm cho '+s.name+'.');
  }
  if (event.live) {
    const obligations=event.live.sessions.flatMap(s=>[...inventory(s.id)].map(([code,seat])=>({sessionId:s.id,code,...seat})));
    for (const item of obligations) {
      const next=info.sessions.find(s=>s.id===item.sessionId);
      assert(next?.zones.some(z=>z.name===item.zone&&z.seats.some(seat=>seat.code===item.code&&!seat.blocked&&(item.type==='Invitation'||seat.type===item.type))),
        'Cần giữ nguyên ghế đã phân bổ: '+item.code+'.');
    }
    for (const ses of info.sessions) {
      const existing=obligations.filter(item=>item.sessionId===ses.id);
      assert(ses.zones.every(z=>existing.filter(item=>item.zone===z.name).length<=z.capacity)&&
        ses.types.every(t=>existing.filter(item=>item.type===t.name).length<=t.quantity),'Sức chứa hoặc loại vé thấp hơn số ghế đã phân bổ của '+ses.name+'.');
    }
  }
}
function versionRecord(event,state,note,info) {
  event.versions.unshift({id:uid('VER-').slice(0,15),version:event.version+(state==='APPROVED'?0:1),state,note,time:Date.now(),data:info?clone(info):null});
}
function submitEvent(id) {
  const event=requireOwner(id);
  assert(event.draft,'Hãy lưu một bản chỉnh sửa trước khi gửi.');
  assert(!event.request,'Hãy chờ xử lý yêu cầu hoãn/hủy trước.');
  assert(!['PENDING','SUSPENDED','CANCELLED'].includes(event.state)&&event.draftState!=='PENDING_UPDATE','Hồ sơ không thể gửi ở trạng thái hiện tại.');
  ensureEventReady(event,event.draft);
  if (event.live) event.draftState='PENDING_UPDATE'; else event.state='PENDING';
  versionRecord(event,event.live?'PENDING_UPDATE':'PENDING','BTC gửi xét duyệt',event.draft);
  audit('Gửi xét duyệt sự kiện',id);
}
function versionDescription(info) {
  return !info?empty('Chưa có nội dung.'):`<div class="stack">${row(info.title,info.cat)}${row('Mô tả',info.description)}${row('Chương trình',info.program)}
    ${row('Quy định',info.rules)}${info.participants?row('Nghệ sĩ / diễn giả',info.participants):''}${info.terms?row('Điều khoản',info.terms):''}${row('Giấy tờ',info.docs?.join(', ')||'Chưa đính kèm')}
    ${panel('Suất diễn',info.sessions.map(s=>row(s.name,fmt(s.start)+' · '+venueName(s.venueId)+' · '+s.zones.map(z=>z.name+' '+z.capacity+' chỗ').join(', ')+
    ' · '+s.types.map(t=>t.name+' '+money(t.price)).join(', '))).join('')||empty('Chưa có suất diễn.'))}</div>`;
}
function lifecycleForm(id) {
  const event=requireOwner(id);
  assert(event.live&&!event.request&&!event.draftState?.startsWith('PENDING')&&['ACTIVE','POSTPONED'].includes(event.state),'Sự kiện đang có một yêu cầu cần xử lý.');
  openDialog('Đề nghị hoãn hoặc hủy',`<div class="form-grid">
    ${field('Loại yêu cầu',select('kind',opt('postpone','Hoãn và dời lịch')+opt('cancel','Hủy sự kiện')),true)}
    ${field('Số ngày dời các suất nếu hoãn',input('shift',14,'number','min="1" max="365" step="1"'),true)}
    ${field('Lý do',area('reason','','required maxlength="800"'),true)}</div>`,form=>{
      const e=requireOwner(id);
      assert(e.live&&!e.request&&!e.draftState?.startsWith('PENDING')&&['ACTIVE','POSTPONED'].includes(e.state),'Sự kiện đang có một yêu cầu cần xử lý.');
      const kind=textValue(form,'kind');
      const shift=currencyNumber(form,'shift');
      assert(kind==='cancel'||(kind==='postpone'&&shift>=1&&shift<=365),'Chọn số ngày dời lịch hợp lệ.');
      e.request={kind:kind==='postpone'?'hoãn':'hủy',shift,reason:textValue(form,'reason'),time:Date.now()};
      audit('Gửi yêu cầu '+e.request.kind,id);
    },'Gửi yêu cầu');
}
function announce(event,text,channel='Trong ứng dụng') {
  event.notices.unshift({text,time:Date.now()});
  const targets=new Set(db.tickets.filter(t=>t.eventId===event.id&&t.user).map(t=>t.user));
  for (const userId of targets) {
    message(userId,eventTitle(event.id),text,channel);
    const user=lookup(db.accounts,userId);
    if (channel==='Email'&&user?.email) sendMock(channel,user.email,eventTitle(event.id),text);
    if (channel==='SMS'&&user?.phone) sendMock(channel,user.phone,eventTitle(event.id),text);
  }
  audit('Thông báo sự kiện',event.id,text);
}
function cancelEvent(event,reason) {
  event.state='CANCELLED';
  for (const order of db.orders.filter(o=>o.eventId===event.id)) {
    if (order.state==='PENDING_PAYMENT') {order.state='CANCELLED';continue;}
    if (!order.paidAt) continue;
    const reserved=reservedRefundAmount(order.id);
    const tickets=db.tickets.filter(t=>t.orderId===order.id&&t.state==='ISSUED');
    if (order.total>reserved&&order.state!=='REFUNDED') queueRefund({user:order.user,orderId:order.id,eventId:event.id,ticketIds:tickets.map(t=>t.id),
      reason:'Sự kiện bị hủy · '+reason,amount:order.total-reserved,state:'AUTO_APPROVED'});
    if (order.state!=='REFUNDED') order.state='REFUND_PENDING';
  }
  for (const t of db.tickets.filter(t=>t.eventId===event.id&&t.state==='ISSUED')) t.state='CANCELLED';
}
function eventDecision(id) {
  const event=lookup(db.events,id);
  assert(canManage('reviews')&&event,'Không có quyền xét duyệt sự kiện.');
  const token=event.request?.time||event.draftState||event.state;
  decision('Xét duyệt '+eventTitle(id),[['approve','Chấp thuận'],['reject','Từ chối']],(result,note)=>{
    const e=lookup(db.events,id);
    assert(canManage('reviews')&&e&&(e.request?.time||e.draftState||e.state)===token,'Hồ sơ đã thay đổi.');
    if (e.request) {
      const req=e.request;
      if (result==='approve') {
        if (req.kind==='hủy') cancelEvent(e,req.reason);
        else {
          const next=clone(e.live);
          next.sessions.forEach(s=>{s.start+=req.shift*day;s.end+=req.shift*day;s.open=false;});
          ensureEventReady(e,next);
          e.live=next;e.version++;
          e.state='POSTPONED';
          e.refundUntil=Date.now()+db.policy.postponementDays*day;
          versionRecord(e,'APPROVED','Phê duyệt dời lịch · '+note,e.live);
        }
        announce(e,(req.kind==='hủy'?'Sự kiện đã hủy. ':'Sự kiện đổi lịch. Vé hiện tại dùng theo lịch mới. Hạn yêu cầu hoàn '+fmt(e.refundUntil)+'. ')+req.reason,'Email');
      }
      e.request=null;
    } else {
      assert(e.draft&&(e.state==='PENDING'||e.draftState==='PENDING_UPDATE'),'Không còn bản chờ duyệt.');
      if (result==='approve') {
        ensureEventReady(e,e.draft);
        const update=!!e.live;
        e.live=e.draft;e.draft=null;e.draftState=null;e.version++;
        if (e.state!=='SUSPENDED') e.state='ACTIVE';
        versionRecord(e,'APPROVED',note,e.live);
        if (update) announce(e,'Phiên bản mới đã được công bố. Kiểm tra lịch và nội dung sự kiện.','Email');
      } else {
        versionRecord(e,'REJECTED',note,e.draft);
        if (e.live) e.draftState='REJECTED'; else e.state='REJECTED';
      }
    }
    e.decision=note;
    message(e.owner,'Kết quả xét duyệt sự kiện',eventTitle(id)+' · '+note);
    audit('Xét duyệt sự kiện',id,result+' · '+note);
  },event.request?'<p class="span muted">'+esc('Yêu cầu '+event.request.kind+' · '+event.request.reason)+'</p>':'');
}
function inviteDialog() {
  const user=account();
  const allowed=db.events.filter(e=>e.live&&(ownerEvent(e)||canStaff('guests.manage',e.id)));
  const pairs=allowed.flatMap(e=>e.live.sessions.filter(s=>ownerEvent(e)||canStaff('guests.manage',e.id,s.id)).map(s=>[e,s]));
  assert(pairs.length,'Chưa có suất diễn được phân quyền.');
  openDialog('Phát hành vé mời',`<div class="form-grid">
    ${field('Suất diễn',select('scope',pairs.map(([e,s])=>opt(e.id+'/'+s.id,eventTitle(e.id)+' · '+s.name+' · '+fmt(s.start),staffScope.eventId+'/'+staffScope.sessionId)).join('')),true)}
    ${field('Khu vực / ghế (cách nhau bằng dấu phẩy)',input('seats','','text','required placeholder="C4, C5"'),true)}
    ${field('Kênh gửi',select('channel',opt('Email')+opt('SMS')))}${field('Ghi chú',input('note'))}
    ${field('Email hoặc điện thoại khách, mỗi dòng một người',area('contacts','','required maxlength="3000"'),true)}
    </div>`,form=>{
      assert(account()?.id===user.id,'Phiên đăng nhập đã kết thúc.');
      const [eventId,sid]=textValue(form,'scope').split('/');
      const e=lookup(db.events,eventId),s=sessionOf(e,sid);
      assert(s&&publicEvent(e)&&(ownerEvent(e)||canStaff('guests.manage',eventId,sid))&&['ACTIVE','POSTPONED'].includes(e.state)&&s.start>Date.now(),'Suất diễn nằm ngoài quyền hoặc đã đóng.');
      assert(saleType(s,'Invitation'),'Loại vé Invitation chưa mở.');
      const contacts=lines(form.get('contacts')).map(contact=>contact.toLowerCase());
      const codes=lines(form.get('seats')).flatMap(group=>group.split(/\s+/)).map(code=>code.toUpperCase());
      const channel=textValue(form,'channel');
      assert(codes.length===contacts.length&&codes.length===new Set(codes).size&&codes.length>0,'Nhập một ghế riêng cho mỗi khách mời.');
      assert(contacts.every(value=>channel==='Email'?/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value):/^[+0-9]{8,15}$/.test(value)),'Liên hệ không phù hợp kênh gửi đã chọn.');
      const seats=codes.map(code=>({code,zone:s.zones.find(z=>z.seats.some(seat=>seat.code===code))?.name,type:'Invitation'}));
      const error=inventoryError(s,seats);
      assert(!error,error);
      seats.forEach((seat,i)=>{
        const ticket={id:uid('VN-').slice(0,18),eventId,sessionId:sid,seat:seat.code,zone:seat.zone,type:'Invitation',state:'ISSUED',
          contact:contacts[i],channel,token:uid('M-'),inviteUntil:s.end,note:textValue(form,'note')};
        db.tickets.unshift(ticket);
        sendMock(channel,ticket.contact,'Vé mời '+eventTitle(eventId),'#invite/'+ticket.token);
        message(db.accounts.find(x=>x.email===ticket.contact||x.phone===ticket.contact)?.id,'Bạn nhận được vé mời',eventTitle(eventId));
        audit('Phát hành vé mời',eventId,ticket.id);
      });
    },'Phát hành vé mời');
}
function authorizedInvite(id) {
  const t=lookup(db.tickets,id);
  assert(t?.type==='Invitation'&&(ownerEvent(lookup(db.events,t.eventId))||canStaff('guests.manage',t.eventId,t.sessionId)),'Vé mời nằm ngoài quyền truy cập.');
  return t;
}
function grantDialog(id) {
  assert(account()?.role==='organizer','Không có quyền phân công.');
  const current=lookup(db.grants,id);
  if (current) requireOwner(current.eventId);
  const person=lookup(db.accounts,current?.user);
  const events=db.events.filter(e=>ownerEvent(e)&&e.live);
  const scopes=events.flatMap(e=>[opt(e.id+'/',eventTitle(e.id)+' · Mọi suất',current?.eventId+'/'+(current?.sessionId||'')),
    ...e.live.sessions.map(s=>opt(e.id+'/'+s.id,eventTitle(e.id)+' · '+s.name,current?.eventId+'/'+current?.sessionId))]).join('');
  assert(scopes,'Chưa có sự kiện được duyệt để phân công.');
  openDialog('Quyền quản lý sự kiện',`<div class="form-grid">
    ${field('Email nhân sự',input('email',person?.email,'email',required))}
    ${field('Tên nhân sự nếu mời mới',input('name',person?.name))}
    ${field('Phạm vi',select('scope',scopes),true)}
    <div class="span stack">${Object.entries(permissionNames).map(([key,label])=>check('permissions',label,current?.permissions.includes(key)||!current&&key==='checkin.scan',key)).join('')}</div>
    <div class="span">${check('verify','BTC đã xác minh danh tính nhân sự trong bản mẫu',false,'yes',true)}</div></div>`,async form=>{
      const [eventId,sessionId]=textValue(form,'scope').split('/');
      const event=requireOwner(eventId);
      assert(event.live&&(!sessionId||sessionOf(event,sessionId))&&form.has('verify'),'Phạm vi hoặc xác minh nhân sự không hợp lệ.');
      const email=textValue(form,'email').toLowerCase();
      let staff=db.accounts.find(a=>a.email===email);
      if (!staff) {
        const name=textValue(form,'name');
        assert(name,'Nhập tên nhân sự mới.');
        staff={id:uid('ST-').slice(0,15),name,email,phone:'',role:'staff',verified:true};
        await setDemoPassword(staff,'Venue2026!');
        db.accounts.push(staff);
      }
      assert(!staff.locked&&['user','staff'].includes(staff.role),'Tài khoản này không thể nhận quyền nhân sự.');
      staff.role='staff';
      const permissions=form.getAll('permissions').filter(key=>permissionNames[key]);
      assert(permissions.length,'Hãy chọn ít nhất một quyền.');
      const grant=lookup(db.grants,id)||db.grants.find(g=>g.user===staff.id&&g.eventId===eventId&&g.sessionId===sessionId);
      if (grant) requireOwner(grant.eventId);
      assert(!db.grants.some(item=>item!==grant&&item.user===staff.id&&item.eventId===eventId&&item.sessionId===sessionId),'Phạm vi này đã được phân công.');
      if (grant) Object.assign(grant,{user:staff.id,eventId,sessionId,permissions,active:true});
      else db.grants.push({id:uid('G-').slice(0,15),user:staff.id,eventId,sessionId,permissions,active:true});
      sendMock('Email',email,'Phân công quản lý sự kiện',eventTitle(eventId));
      message(staff.id,'Được cấp quyền vận hành',eventTitle(eventId));
      audit('Cấp quyền nhân sự',eventId,permissions.join(', '));
    },'Lưu phân quyền');
}
function performScan(code,zone,method) {
  const {eventId,sessionId}=staffScope;
  const event=lookup(db.events,eventId);
  const ses=sessionOf(event,sessionId);
  assert(canOperate(method==='Thủ công'?'checkin.manual':'checkin.scan',eventId,sessionId)&&ses,'Quyền check-in đã bị thu hồi hoặc không đúng suất.');
  const id=code.trim().toUpperCase();
  const ticket=lookup(db.tickets,id);
  let result='CHECKED_IN';
  let messageText='Check-in thành công · '+id;
  if (!ticket) {result='INVALID';messageText='Từ chối · Mã vé không tồn tại.';}
  else if (ticket.eventId!==eventId||ticket.sessionId!==sessionId) {result='WRONG_SESSION';messageText='Từ chối · Vé không thuộc suất đã chọn.';}
  else if (zone&&zone!==ticket.zone) {result='WRONG_ZONE';messageText='Từ chối · Vé thuộc khu '+ticket.zone+'.';}
  else if (ticket.state==='CHECKED_IN') {result='DUPLICATE';messageText='Từ chối · Vé đã check-in lúc '+fmt(ticket.checkedAt)+'.';}
  else if (!['ACTIVE','POSTPONED'].includes(event.state)||ticket.state!=='ISSUED') {result='UNAVAILABLE';messageText='Từ chối · Vé đã hủy, thu hồi hoặc sự kiện tạm ngừng.';}
  else {ticket.state='CHECKED_IN';ticket.checkedAt=Date.now();}
  db.scans.unshift({id:uid(),eventId,sessionId,ticketId:ticket?.eventId===eventId?id:'—',time:Date.now(),actor:account().id,method,result,text:messageText});
  audit('Check-in '+method,eventId,result);
  staffResult=messageText;
}
function promoDialog(id) {
  const user=account();
  const v=lookup(db.vouchers,id);
  const admin=user?.role==='admin';
  assert((admin||user?.role==='organizer')&&(!v||admin||v.owner===user.id),'Chiến dịch nằm ngoài quyền.');
  const list=db.events.filter(e=>e.live&&(admin||ownerEvent(e)));
  const start=Date.now()-minute;
  const scope=v?.eventId?v.eventId+'/'+(v.sessionId||''):'';
  openDialog('Cấu hình voucher',`<div class="form-grid">
    ${field('Mã',input('code',v?.code,'text','required pattern="[A-Za-z0-9_\\x2D]{3,24}"'))}
    ${field('Kiểu giảm',select('kind',opt('percent','Phần trăm',v?.kind)+opt('fixed','Số tiền cố định',v?.kind)))}
    ${field('Giá trị giảm (% hoặc đ)',input('value',v?.value||10,'number','required min="1" step="1"'))}
    ${field('Trần giảm phần trăm (đ, 0 là không giới hạn)',input('cap',v?.cap??150000,'number','min="0" step="1"'))}
    ${field('Giá trị đơn tối thiểu (đ)',input('min',v?.min||0,'number','min="0" step="1"'))}
    ${field('Nguồn tài trợ',select('sponsor',choices(['BTC','Nền tảng','Đồng tài trợ'],v?.sponsor)))}
    ${field('Bắt đầu',input('from',localInput(v?.from||start),'datetime-local','required'))}
    ${field('Kết thúc',input('to',localInput(v?.to||start+30*day),'datetime-local','required'))}
    ${field('Tổng lượt',input('limit',v?.limit||30,'number','required min="1" step="1"'))}
    ${field('Lượt mỗi tài khoản',input('perUser',v?.perUser||2,'number','required min="1" step="1"'))}
    ${field('Phạm vi sự kiện / suất',select('scope',opt('','Toàn phạm vi của bên tài trợ',scope)+list.flatMap(e=>[
      opt(e.id+'/',eventTitle(e.id)+' · Tất cả suất',scope),...e.live.sessions.map(s=>opt(e.id+'/'+s.id,eventTitle(e.id)+' · '+s.name,scope))]).join('')),true)}
    ${field('Tên khu áp dụng (tùy chọn)',input('zone',v?.zone),true)}</div>`,form=>{
      const user=account();
      const current=lookup(db.vouchers,id);
      assert(user?.role==='admin'||user?.role==='organizer'&&(!current||current.owner===user.id),'Không có quyền cập nhật voucher.');
      const code=textValue(form,'code').toUpperCase();
      assert(/^[A-Z0-9_-]{3,24}$/.test(code)&&!db.vouchers.some(x=>x.id!==id&&x.code===code),'Mã trùng hoặc không hợp lệ.');
      const [eventId='',sessionId='']=textValue(form,'scope').split('/');
      const event=lookup(db.events,eventId);
      assert(!eventId||event?.live&&(user.role==='admin'||ownerEvent(event))&&(!sessionId||sessionOf(event,sessionId)),'Phạm vi không hợp lệ.');
      const from=parseLocal(textValue(form,'from')),to=parseLocal(textValue(form,'to'));
      requireFuture(from,to);
      const values=Object.fromEntries(['cap','min','value','limit','perUser'].map(key=>[key,currencyNumber(form,key)]));
      const kind=textValue(form,'kind');
      assert(values.limit>=values.perUser&&values.perUser>0&&values.value>0&&(kind==='fixed'||values.value<=100),'Giá trị hoặc giới hạn lượt không hợp lệ.');
      const zone=textValue(form,'zone');
      assert(!zone||!event||event.live.sessions.filter(s=>!sessionId||s.id===sessionId).some(s=>s.zones.some(z=>z.name===zone)),'Khu vực không thuộc phạm vi.');
      const next={...values,code,eventId,sessionId,zone,kind,from,to,sponsor:textValue(form,'sponsor'),state:user.role==='admin'?'ACTIVE':'PENDING'};
      if (current) Object.assign(current,next); else db.vouchers.push({...next,id:uid('VC-').slice(0,15),owner:user.id});
      audit('Lưu voucher',code,next.state);
    },'Lưu chiến dịch');
}


// Quản trị nền tảng và tài chính

function requireAdmin() {assert(account()?.role==='admin','Thao tác chỉ dành cho Admin.');}
function adminAccountForm(id='') {
  requireAdmin();
  const person=lookup(db.accounts,id);
  assert(!person||person.id!==account().id,'Không chỉnh quyền của phiên Admin hiện tại.');
  const scope=person?.scope||Object.keys(managerPermissions);
  openDialog(person?'Quản lý '+person.name:'Tạo Quản lý hệ thống',`<div class="form-grid">
    ${field('Họ tên',input('name',person?.name,'text',required))}
    ${field('Email',input('email',person?.email,'email',required))}
    ${field('Điện thoại',input('phone',person?.phone,'tel','pattern="[0-9+]{8,15}"'))}
    ${field('Vai trò',select('role',(person?Object.entries(roles).filter(([key])=>key!=='guest'):Object.entries(roles).filter(([key])=>key==='manager')).map(([value,label])=>opt(value,label,person?.role||'manager')).join('')))}
    <div class="span stack"><strong>Quyền được giao cho Quản lý hệ thống</strong>
    ${Object.entries(managerPermissions).map(([key,label])=>check('scope',label,scope.includes(key),key)).join('')}
    ${check('locked','Khóa tài khoản',person?.locked)}
    ${check('verified','Đánh dấu liên hệ đã được xác minh trong bản mẫu',person?.verified)}
    ${check('revoke','Thu hồi các phiên đăng nhập hiện tại')}</div></div>
    ${person?'':notice('Tài khoản mẫu mới có mật khẩu khởi tạo Venue2026!.')}`,async form=>{
      requireAdmin();
      const current=lookup(db.accounts,id);
      assert(!current||current.id!==account().id,'Không chỉnh tài khoản đang dùng.');
      const email=textValue(form,'email').toLowerCase(),phone=textValue(form,'phone');
      const name=textValue(form,'name'),role=textValue(form,'role');
      assert(name&&email&&roles[role]&&role!=='guest'&&!db.accounts.some(a=>a.id!==id&&(a.email===email||(phone&&a.phone===phone))),'Thông tin thiếu hoặc liên hệ đã tồn tại.');
      const record={name,email,phone,role,scope:role==='manager'?form.getAll('scope').filter(key=>managerPermissions[key]):[],
        locked:form.has('locked'),verified:form.has('verified')};
      const roleChanged=current?.role!==role;
      const wasLocked=Boolean(current?.locked);
      const previousScope=current?.role==='manager'&&Array.isArray(current.scope)?current.scope:[];
      const added=record.scope.filter(key=>!previousScope.includes(key));
      const removed=previousScope.filter(key=>!record.scope.includes(key));
      const revokeSessions=form.has('revoke');
      const accountChanged=current&&['name','email','phone','role','verified'].some(key=>current[key]!==record[key]);
      if (current) Object.assign(current,record);
      else {
        record.id=uid('U-').slice(0,15);
        await setDemoPassword(record,'Venue2026!');
        db.accounts.push(record);
      }
      const userId=id||record.id;
      if (record.locked||roleChanged||revokeSessions) db.sessionVersions[userId]=(db.sessionVersions[userId]||0)+1;
      if (roleChanged&&role!=='staff') db.grants.filter(g=>g.user===userId).forEach(g=>{g.active=false;});
      if (!current) audit('Tạo tài khoản Quản lý hệ thống',userId);
      else if (accountChanged) audit('Cập nhật tài khoản và quyền',userId,roles[role]);
      if (current&&added.length) audit('Cấp quyền Quản lý hệ thống',userId,added.map(key=>managerPermissions[key]).join(', '));
      if (removed.length) audit('Thu hồi quyền Quản lý hệ thống',userId,removed.map(key=>managerPermissions[key]).join(', '));
      if (wasLocked!==record.locked) audit(record.locked?'Khóa tài khoản':'Mở khóa tài khoản',userId);
      if (revokeSessions) audit('Thu hồi phiên đăng nhập',userId);
    });
}
function venueForm(id) {
  requireAdmin();
  const v=lookup(db.venues,id);
  openDialog(v?'Sửa địa điểm':'Thêm địa điểm',`<div class="form-grid">
    ${field('Tên',input('name',v?.name,'text',required))}
    ${field('Thành phố / khu vực',input('city',v?.city,'text',required))}
    ${field('Địa chỉ',input('address',v?.address,'text',required),true)}
    ${field('Sức chứa',input('capacity',v?.capacity||100,'number','required min="1" step="1"'))}
    ${field('Giá thuê một khung (đ)',input('rate',v?.rate??10000000,'number','required min="0" step="1"'))}
    ${field('Tiện ích',area('amenities',v?.amenities,'maxlength="800"'),true)}
    ${field('Ảnh địa điểm (PNG, JPEG hoặc WebP)',input('cover','','file','accept="image/png,image/jpeg,image/webp"'),true)}
    ${isUploadedCover(v?.cover)?'<div class="span">'+check('dropCover','Gỡ ảnh đã tải lên nếu không chọn ảnh mới')+'</div>':''}</div>`,async (form,element)=>{
      requireAdmin();
      const current=lookup(db.venues,id);
      const capacity=currencyNumber(form,'capacity');
      assert(capacity>0&&(!current||current.zones.reduce((sum,z)=>sum+z.capacity,0)<=capacity),'Sức chứa thấp hơn khu vực đã cấu hình.');
      const record={capacity,rate:currencyNumber(form,'rate')};
      for (const key of ['name','city','address','amenities']) record[key]=textValue(form,key);
      assert(record.name&&record.city&&record.address,'Cần nhập tên, khu vực, địa chỉ.');
      const cover=element.elements.cover.files[0];
      if (cover) record.cover=await prepareCoverImage(cover);
      if (current) {
        Object.assign(current,record);
        if (!cover&&form.has('dropCover')) delete current.cover;
      }
      else {
        record.id=uid('V-').slice(0,15);
        record.state='ACTIVE';record.blocks=[];
        record.zones=[{name:'Standard',capacity:Math.min(6,capacity),seats:seatRows(['C'],Math.min(6,capacity),'Standard')}];
        db.venues.unshift(record);
      }
      venueEditorId=id||record.id;
      audit('Lưu địa điểm',venueEditorId);
    });
}
function venueZoneForm(id) {
  requireAdmin();
  const v=lookup(db.venues,id);
  assert(v,'Địa điểm không tồn tại.');
  openDialog('Sơ đồ khu vực và ghế',zoneHelp()+field('Sơ đồ',area('zones',stringifyZones(v.zones),'required spellcheck="false"')),form=>{
    requireAdmin();
    const current=lookup(db.venues,id);
    assert(current,'Địa điểm không tồn tại.');
    current.zones=parseZones(textValue(form,'zones'),current.capacity);
    audit('Cập nhật sơ đồ địa điểm',id);
  });
}
function venueBlockForm(id) {
  requireAdmin();
  const start=parseLocal(dayKey(Date.now()))+day+16*hour;
  openDialog('Chặn lịch địa điểm',`<div class="form-grid">
    ${field('Bắt đầu',input('start',localInput(start),'datetime-local','required'))}${field('Kết thúc',input('end',localInput(start+hour),'datetime-local','required'))}
    ${field('Lý do',area('reason','','required maxlength="500"'),true)}</div>`,form=>{
      requireAdmin();
      const v=lookup(db.venues,id);
      const start=parseLocal(textValue(form,'start')),end=parseLocal(textValue(form,'end'));
      requireFuture(start,end);
      assert(v&&start>Date.now()&&!rentalConflict(v,start,end),'Khung giờ đã có lịch hoặc đã qua.');
      v.blocks.push({id:uid().slice(0,18),start,end,reason:textValue(form,'reason')});
      audit('Chặn lịch địa điểm',id,fmt(start));
    });
}
function categoryDialog(id) {
  requireAdmin();
  openDialog(id?'Sửa danh mục':'Thêm danh mục',`${field('Tên danh mục',input('name',id,'text',required))}
    ${id?check('remove','Xóa danh mục này'):''}`,form=>{
    requireAdmin();
    const name=textValue(form,'name');
    const exists=info=>info?.cat===id;
    if (form.has('remove')) {
      assert(!db.events.some(e=>exists(e.draft)||exists(e.live)),'Danh mục vẫn còn sự kiện sử dụng.');
      db.categories=db.categories.filter(cat=>cat!==id);
      if (homeCategory===id) homeCategory='';
    } else {
      assert(name&&!db.categories.some(cat=>cat!==id&&matches(cat,name)&&matches(name,cat)),'Tên đã tồn tại hoặc để trống.');
      if (id) {
        db.categories=db.categories.map(cat=>cat===id?name:cat);
        for (const e of db.events) for (const info of [e.live,e.draft]) if (exists(info)) info.cat=name;
      } else db.categories.push(name);
    }
    audit('Quản lý danh mục',id||name);
  });
}
function bannerForm(id='') {
  requireAdmin();
  const b=lookup(db.banners,id);
  openDialog('Banner trang chủ',`<div class="form-grid">
    ${field('Tiêu đề (dùng | để ngắt dòng)',input('title',b?.title,'text',required),true)}
    ${field('Mô tả',input('text',b?.text,'text',required),true)}
    ${field('Sự kiện nổi bật',select('eventId',opt('','Chọn sự kiện')+db.events.filter(publicEvent).map(e=>opt(e.id,eventTitle(e.id),b?.eventId)).join('')),true)}
    <div class="span">${check('active','Hiển thị banner này',b?.state==='ACTIVE')}</div></div>`,form=>{
    requireAdmin();
    const current=lookup(db.banners,id);
    const record={title:textValue(form,'title'),text:textValue(form,'text'),eventId:textValue(form,'eventId'),state:form.has('active')?'ACTIVE':'HIDDEN'};
    assert(record.title&&record.text,'Hãy điền tiêu đề và mô tả.');
    if (record.state==='ACTIVE') db.banners.forEach(banner=>{banner.state='HIDDEN';});
    if (current) Object.assign(current,record);
    else db.banners.unshift({...record,id:uid('B-').slice(0,15)});
    audit('Cập nhật banner',id||record.title);
  });
}
function platformText() {
  requireAdmin();
  const texts={about:'Giới thiệu',guide:'Hướng dẫn đặt vé',rules:'Quy định nền tảng'};
  openDialog('Nội dung nền tảng',Object.entries(texts).map(([key,label])=>field(label,area(key,db.content[key],'required maxlength="1800"'))).join(''),form=>{
    requireAdmin();
    for (const key of Object.keys(texts)) db.content[key]=textValue(form,key);
    audit('Cập nhật nội dung trang', 'Nền tảng');
  });
}
const policyFields = {
  serviceRate:['Phí dịch vụ người mua (%)',0,100],depositPercent:['Cọc thuê (%)',1,100],depositHours:['Hạn cọc (giờ)',1,168],
  refundHours:['Thời hạn tự hủy trước sự kiện (giờ)',1,720],settlementDays:['Khóa sổ sau sự kiện (ngày)',0,60],
  settlementHours:['Hạn BTC phản hồi (giờ)',1,168],postponementDays:['Hạn yêu cầu hoàn sau hoãn (ngày)',1,60],
  rentalEarlyDays:['Mốc hoàn cọc sớm (ngày)',1,365],rentalEarlyPercent:['Hoàn cọc sớm (%)',0,100],
  rentalMidDays:['Mốc hoàn cọc giữa (ngày)',1,365],rentalMidPercent:['Hoàn cọc giữa (%)',0,100],
  payoutLimit:['Hạn mức một lần chi (đ)',1,1e12],promotionBudget:['Ngân sách mỗi chiến dịch (đ)',1,1e12]
};
function policyForm() {
  requireAdmin();
  openDialog('Chính sách cho giao dịch mới',notice('Giữ ghế 10 phút và tối đa 10 vé mỗi tài khoản trên mỗi suất. Số tiền đã chốt trên đơn và cọc mẫu được giữ theo điều kiện lúc tạo.')+
    '<div class="form-grid">'+Object.entries(policyFields).map(([key,[label,min,max]])=>field(label,input(key,db.policy[key],'number','required min="'+min+'" max="'+max+'" step="1"'))).join('')+
    field('Ngày nghỉ theo YYYY-MM-DD, mỗi dòng một ngày',area('holiday',db.policy.holiday,'maxlength="1200"'),true)+
    '<div class="span stack">'+check('history','Bật lịch sử xem sự kiện',db.policy.history)+check('authVerify','Yêu cầu xác minh liên hệ trong bản mẫu',db.policy.authVerify)+'</div></div>',form=>{
    requireAdmin();
    const next=Object.fromEntries(Object.keys(policyFields).map(key=>[key,currencyNumber(form,key)]));
    const holiday=textValue(form,'holiday');
    assert(next.rentalMidDays<next.rentalEarlyDays&&next.rentalMidPercent<=next.rentalEarlyPercent,'Thứ tự mốc ngày hoặc tỷ lệ hoàn cọc không hợp lệ.');
    assert(lines(holiday).every(value=>/^\d{4}-\d{2}-\d{2}$/.test(value)&&Number.isFinite(parseLocal(value))&&dayKey(parseLocal(value))===value),'Ngày nghỉ phải đúng định dạng YYYY-MM-DD.');
    Object.assign(db.policy,next,{holiday,history:form.has('history'),authVerify:form.has('authVerify')});
    audit('Cấu hình chính sách', 'Giao dịch mới');
  });
}
function configForm() {
  requireAdmin();
  const fields={domain:'Tên miền mẫu',supportPhone:'Hotline hỗ trợ',supportEmail:'Email hỗ trợ',smtp:'Trạng thái SMTP',sms:'Trạng thái SMS',payos:'Chế độ PayOS',protection:'Trạng thái Data Protection',
    recipient:'Bí danh tài khoản nhận tiền',bank:'Ngân hàng nhận mẫu'};
  openDialog('Cấu hình và bảo mật',notice('Dùng bí danh và trạng thái minh họa. Không nhập API key, mật khẩu SMTP hoặc dữ liệu ngân hàng thật.')+
    '<div class="form-grid">'+Object.entries(fields).map(([key,label])=>field(label,input(key,db.settings[key],'text','maxlength="150"'))).join('')+
    field('Mật khẩu tối thiểu (ký tự)',input('passwordLength',db.policy.passwordLength,'number','required min="8" max="64"'))+
    field('Thời hạn phiên (giờ)',input('sessionHours',db.policy.sessionHours,'number','required min="1" max="168"'))+
    '<div class="span stack">'+check('https','HTTPS',db.settings.https)+check('twoFactor','Bật xác thực hai bước mô phỏng',db.settings.twoFactor)+'</div>'+
    field('Lý do thay đổi',area('reason','','required maxlength="500"'),true)+'</div>',form=>{
    requireAdmin();
    const before=clone(db.settings);
    const security={passwordLength:db.policy.passwordLength,sessionHours:db.policy.sessionHours};
    db.configHistory.unshift({id:uid(),settings:before,security,time:Date.now(),reason:textValue(form,'reason')});
    const next=Object.fromEntries(Object.keys(fields).map(key=>[key,textValue(form,key)]));
    assert(!next.supportPhone||/^\+?[\d .()-]+$/.test(next.supportPhone)&&/^\d{9,15}$/.test(next.supportPhone.replace(/\D/g,'')),'Hotline hỗ trợ không hợp lệ.');
    assert(!next.supportEmail||/^[^\s@?&]+@[^\s@?&]+\.[^\s@?&]+$/.test(next.supportEmail),'Email hỗ trợ không hợp lệ.');
    Object.assign(db.settings,next,{https:form.has('https'),twoFactor:form.has('twoFactor'),version:before.version+1});
    db.policy.passwordLength=currencyNumber(form,'passwordLength');
    db.policy.sessionHours=currencyNumber(form,'sessionHours');
    audit('Thay đổi cấu hình', 'Phiên bản '+db.settings.version);
  });
}
function contractForm(id='') {
  requireAdmin();
  const c=lookup(db.contracts,id);
  const people=db.accounts.filter(a=>a.role==='organizer');
  assert(people.length,'Cần có tài khoản Ban tổ chức.');
  const now=Date.now();
  openDialog(id?'Hợp đồng đã lưu':'Tạo hợp đồng phí',`<div class="form-grid">
    ${field('Tên hợp đồng',input('name',c?.name,'text',required),true)}
    ${field('Ban tổ chức',select('owner',people.map(a=>opt(a.id,a.name,c?.owner)).join('')))}
    ${field('Cách tính',select('kind',opt('percent','Tỷ lệ trên doanh thu vé sau giảm',c?.kind)+opt('fixed','Số tiền cố định trên vé',c?.kind)))}
    ${field('Mức phí (% hoặc đ/vé)',input('value',c?.value??8,'number','required min="0" step="0.01"'))}
    ${field('Hiệu lực từ',input('from',localInput(c?.from||now),'datetime-local','required'))}
    ${field('Hiệu lực đến',input('to',localInput(c?.to||now+365*day),'datetime-local','required'))}
    ${field('Ghi chú, thuế và phí ngân hàng',area('notes',c?.notes,'maxlength="1500"'),true)}</div>
    ${c?notice('Các đơn đã chốt tiếp tục sử dụng bản phí tại thời điểm mua.') :''}`,form=>{
      requireAdmin();
      const owner=textValue(form,'owner'),kind=textValue(form,'kind');
      const from=parseLocal(textValue(form,'from')),to=parseLocal(textValue(form,'to'));
      const value=currencyNumber(form,'value'),name=textValue(form,'name');
      requireFuture(from,to);
      assert(name&&lookup(db.accounts,owner)?.role==='organizer'&&(kind==='fixed'?Number.isInteger(value):value<=100),'Tên, chủ hợp đồng hoặc mức phí không hợp lệ.');
      assert(!db.contracts.some(item=>item.id!==id&&item.owner===owner&&from<item.to&&to>item.from),'Trùng thời hạn với hợp đồng đã có.');
      const next={name,owner,kind,from,to,value,basis:kind==='percent'?'Doanh thu vé sau giảm':'Theo số vé mua',notes:textValue(form,'notes')};
      const existing=lookup(db.contracts,id);
      if (existing) Object.assign(existing,next);
      else db.contracts.unshift({...next,id:uid('CT-').slice(0,15)});
      audit('Lưu hợp đồng phí',id||name);
    });
}
function bankForm() {
  const user=account();
  assert(user?.role==='organizer','Chỉ Ban tổ chức có mục thụ hưởng.');
  openDialog('Cập nhật thông tin thụ hưởng',notice('Dùng bí danh ngân hàng và tài khoản mẫu.')+
    field('Thông tin thụ hưởng mẫu',input('bank',user.bank,'text',required)),form=>{
    const current=account();
    assert(current?.id===user.id&&current.role==='organizer','Phiên BTC không còn hiệu lực.');
    const bank=textValue(form,'bank');
    assert(bank,'Hãy nhập thông tin mẫu.');
    current.bank=bank;current.bankState='Chờ Admin xác minh';
    audit('Yêu cầu xác minh thụ hưởng',current.id);
  });
}
function eligibleSettlement(event) {
  return event.live&&event.live.sessions.length&&
    Math.max(...event.live.sessions.map(s=>s.end))+db.policy.settlementDays*day<=Date.now()&&
    !db.settlements.some(s=>s.eventId===event.id);
}
function settlementForm() {
  assert(canManage('settlements'),'Không được khóa sổ.');
  const ready=db.events.filter(eligibleSettlement);
  assert(ready.length,'Chưa có sự kiện hết thời gian khóa sổ và chưa lập đối soát.');
  openDialog('Khóa sổ sự kiện',`<div class="form-grid">
    ${field('Sự kiện',select('eventId',ready.map(e=>opt(e.id,eventTitle(e.id))).join('')),true)}
    ${field('Điều chỉnh đã duyệt (đ)',input('adjustments',0,'number','required min="0" step="1"'))}
    ${field('Ghi chú thuế, phí ngân hàng hoặc điều chỉnh',area('note','','maxlength="1000"'),true)}</div>`,form=>{
    assert(canManage('settlements'),'Không được khóa sổ.');
    const e=lookup(db.events,textValue(form,'eventId'));
    assert(e&&eligibleSettlement(e),'Sự kiện chưa đủ điều kiện hoặc đã khóa sổ.');
    assert(!db.refunds.some(r=>r.eventId===e.id&&!['COMPLETED','REJECTED'].includes(r.state))&&
      !db.support.some(s=>s.eventId===e.id&&s.state==='OPEN'),'Cần xử lý hết yêu cầu hoàn và khiếu nại trước khi khóa sổ.');
    const orders=db.orders.filter(o=>o.eventId===e.id&&o.paidAt&&!db.refunds.some(r=>r.orderId===o.id&&r.priority));
    const entries=orders.map(o=>({orderId:o.id,sessionId:o.sessionId,gross:o.subtotal-o.discount,discount:o.discount,refunds:settledRefundAmount(o.id),
      platformFee:o.platformFee,contractId:o.contractId}));
    const sum=key=>entries.reduce((total,entry)=>total+entry[key],0);
    const adjustments=currencyNumber(form,'adjustments');
    const gross=sum('gross'),refunds=sum('refunds'),fee=sum('platformFee');
    const record={id:uid('STL-').slice(0,15),eventId:e.id,owner:e.owner,entries,gross,refunds,fee,adjustments,
      net:gross-refunds-fee-adjustments,deadline:Date.now()+db.policy.settlementHours*hour,time:Date.now(),state:'DRAFT',note:textValue(form,'note')};
    db.settlements.unshift(record);
    message(e.owner,'Đối soát chờ xác nhận',eventTitle(e.id)+' · Hạn '+fmt(record.deadline));
    audit('Khóa sổ kỳ đối soát',record.id);
  },'Khóa sổ');
}
function settlementDetail(id) {
  const s=lookup(db.settlements,id);
  assert(s&&(canManage('settlements')||account()?.role==='organizer'&&s.owner===account().id),'Bảng đối soát nằm ngoài phạm vi.');
  openDialog('Đối soát '+id,`<div class="stack">${tag(s.state)}
    ${row('Doanh thu vé thực thu',money(s.gross))}${row('Tổng hoàn',money(s.refunds))}
    ${row('Phí theo hợp đồng tại từng đơn',money(s.fee))}${row('Điều chỉnh',money(s.adjustments))}${row('Thực nhận',money(s.net))}
    ${row('Hạn phản hồi',fmt(s.deadline))}${s.note?row('Ghi chú',s.note):''}${s.response?row('Phản hồi BTC',s.response):''}
    ${table(['Đơn','Suất','Thực thu','Hoàn','Phí','Hợp đồng'],s.entries.map(e=>[esc(e.orderId),esc(e.sessionId),money(e.gross),money(e.refunds),money(e.platformFee),esc(e.contractId||'—')]))}
    <div>${button('Xuất báo cáo CSV','settlementCsv',id,'secondary')}</div></div>`);
}
function settlementResponse(id) {
  const s=lookup(db.settlements,id);
  assert(s?.owner===account()?.id,'Chỉ BTC của sự kiện có thể phản hồi.');
  decision('Phản hồi đối soát',[['ORGANIZER_CONFIRMED','Xác nhận báo cáo'],['DISPUTED','Yêu cầu đối chiếu']],(state,note)=>{
    const s=lookup(db.settlements,id);
    assert(account()?.role==='organizer'&&s?.owner===account().id&&s.state==='DRAFT'&&s.deadline>=Date.now(),'Kỳ đối soát đã hết hạn phản hồi hoặc đổi trạng thái.');
    s.state=state;s.response=note;
    audit('BTC phản hồi đối soát',id,state);
  });
}
function settlementApprove(id) {
  assert(canManage('settlements'),'Không được duyệt đối soát.');
  decision('Duyệt bảng đối soát',[['MANAGER_APPROVED','Duyệt số liệu'],['DISPUTED','Yêu cầu đối chiếu thêm']],(state,note)=>{
    const s=lookup(db.settlements,id);
    assert(canManage('settlements')&&s&&s.owner!==account().id&&settlementAwaitingReview(s),
      'Chờ xác nhận BTC hoặc hết hạn phản hồi.');
    s.state=state;s.note=note;
    message(s.owner,'Kết quả duyệt đối soát',status[state][0]+' · '+note);
    audit('Xét duyệt đối soát',id,state);
  });
}
function payoutCreate(id) {
  requireAdmin();
  const s=lookup(db.settlements,id);
  const owner=lookup(db.accounts,s?.owner);
  assert(s?.state==='MANAGER_APPROVED'&&s.net>0&&s.net<=db.policy.payoutLimit&&!db.payouts.some(p=>p.settlementId===s.id),'Bảng đã lập lệnh, không đủ điều kiện hoặc vượt hạn mức.');
  assert(owner?.id==='studio'&&!owner.bankState||owner?.bankState==='Đã xác minh trong bản mẫu','Tài khoản thụ hưởng cần được xác minh.');
  const p={id:'PAY-'+id,settlementId:id,owner:s.owner,amount:s.net,time:Date.now(),state:'PAYOUT_PENDING'};
  db.payouts.unshift(p);s.state='PAYOUT_PENDING';
  audit('Lập lệnh giải ngân',p.id);
  message(s.owner,'Đã lập lệnh giải ngân',p.id);
}
function payoutDecision(id) {
  requireAdmin();
  const p=lookup(db.payouts,id);
  const transitions={PAYOUT_PENDING:[['PROCESSING','Gửi lệnh chi']],PROCESSING:[['COMPLETED','Xác nhận đã chuyển tiền'],['FAILED','Chuyển khoản thất bại']],FAILED:[['PROCESSING','Gửi lại cùng mã lệnh']]};
  assert(p&&transitions[p.state],'Lệnh đã hoàn tất.');
  decision('Xử lý lệnh '+id,transitions[p.state],(state,note,form)=>{
    requireAdmin();
    const current=lookup(db.payouts,id);
    assert(current?.state===p.state,'Lệnh đã đổi trạng thái.');
    const proof=textValue(form,'proof');
    assert(state!=='COMPLETED'||proof&&!db.payouts.some(other=>other.id!==id&&other.proof===proof),'Cần mã chứng từ duy nhất để hoàn tất lệnh.');
    current.state=state;current.note=note;
    if (state==='COMPLETED') {
      current.proof=proof;current.paidAt=Date.now();
      lookup(db.settlements,current.settlementId).state='PAID';
    }
    message(current.owner,'Cập nhật giải ngân',status[state][0]+' · '+note);
    audit('Cập nhật lệnh chi',id,state);
  },field('Mã chứng từ khi thành công',input('proof','','text','maxlength="150"'),true));
}
function backupDialog() {
  requireAdmin();
  openDialog('Kiểm tra hoặc phục hồi',notice('Có thể kiểm tra tệp mẫu trước khi nạp. Khi phục hồi, trình duyệt tải một bản sao dữ liệu hiện tại để có thể quay lại.')+
    field('Tệp sao lưu mẫu',input('file','','file','accept="application/json,.json" required'))+
    field('Thao tác',select('mode',opt('check','Kiểm tra tệp')+opt('restore','Phục hồi dữ liệu từ tệp')))+
    check('confirm','Tôi xác nhận thay dữ liệu mẫu hiện tại khi chọn phục hồi'),async (form,element)=>{
      requireAdmin();
      const file=element.elements.file.files[0];
      assert(file&&file.size<12*1024*1024,'Tệp không tồn tại hoặc lớn hơn 12 MB.');
      let data;
      try {data=JSON.parse(await file.text());} catch {throw new Error('Tệp không phải JSON hợp lệ.');}
      const snapshot=data?.data;
      const schema=startingData();
      assert(data?.app===storeKey&&snapshot&&Object.keys(schema).every(key=>{
        const value=snapshot[key];
        return Array.isArray(schema[key])?Array.isArray(value):value&&typeof value==='object'&&!Array.isArray(value);
      })&&snapshot.accounts.some(user=>user.role==='admin'&&!user.locked)&&
        snapshot.events.every(e=>e?.id&&(e.live||e.draft)&&[e.live,e.draft].filter(Boolean).every(info=>Array.isArray(info.sessions)))&&
        snapshot.venues.every(v=>v?.id&&Array.isArray(v.zones)&&Array.isArray(v.blocks))&&
        snapshot.orders.every(o=>o?.id&&Array.isArray(o.seats)),
        'Tệp không phải bản sao hợp lệ từ giao diện mẫu QLSK.');
      const restore=textValue(form,'mode')==='restore';
      if (restore) {
        assert(form.has('confirm'),'Cần xác nhận trước khi phục hồi dữ liệu.');
        download('VENUE-truoc-phuc-hoi.json',JSON.stringify({app:storeKey,time:Date.now(),data:db},null,2),'application/json');
        db=snapshot;
      }
      db.backups.unshift({action:restore?'Phục hồi dữ liệu':'Kiểm tra phục hồi',time:Date.now(),
        note:snapshot.events.length+' sự kiện · '+snapshot.accounts.length+' tài khoản'});
      audit(restore?'Phục hồi dữ liệu':'Kiểm tra khôi phục dữ liệu',file.name);
      if (restore) {setAuth(null);go('home');}
      notifyUi(restore?'Đã phục hồi dữ liệu mẫu. Hãy đăng nhập lại.':'Tệp sao lưu vượt qua kiểm tra cấu trúc.');
    },'Thực hiện');
}


// Điều hướng và xử lý tương tác

function confirmAction(title,summary,action) {
  openDialog(title,notice(summary),action,'Xác nhận');
}
function demoSwitcher() {
  const labels={member:'Người dùng',buyer2:'Người dùng thứ hai',studio:'Ban tổ chức',gate:'Quản lý sự kiện',ops:'Quản lý hệ thống',admin:'Admin'};
  openDialog('Vai trò mẫu',`<div class="stack">
    ${notice('Đổi tài khoản để tiếp tục cùng một hồ sơ, sự kiện, yêu cầu thuê hoặc đơn mẫu. Các tài khoản có sẵn dùng mật khẩu Venue2026!.')}
    ${button('Xem như khách','demoRole','guest','secondary')}
    ${demoAccounts.map(a=>row(labels[a.id],a.email,button('Sử dụng','demoRole',a.id,'secondary compact'))).join('')}
    ${row('Vé mời chưa liên kết','Mở QR khi chưa đăng nhập',link('Mở liên kết mẫu','invite/venue-demo-moi','btn ghost compact'))}
  </div>`);
}
function selectSeat(code) {
  const e=lookup(db.events,selection.eventId);
  const ses=sessionOf(e,selection.sessionId);
  const zone=ses?.zones.find(z=>z.name===selection.zone);
  const s=zone?.seats.find(seat=>seat.code===code);
  assert(account()&&s&&sellable(e,ses),'Không thể chọn ghế trong suất này.');
  const group=s.group?zone.seats.filter(seat=>seat.group===s.group):[s];
  const codes=group.map(seat=>seat.code);
  if (selection.seats.includes(code)) {
    selection.seats=selection.seats.filter(id=>!codes.includes(id));
    return;
  }
  const all=[...chosenSeats(ses),...group.filter(seat=>!selection.seats.includes(seat.code)).map(seat=>({...seat,zone:zone.name}))];
  const badType=all.find(seat=>!saleType(ses,seat.type));
  assert(!badType,'Vé cho ghế '+badType?.code+' đã đóng bán.');
  const error=inventoryError(ses,all);
  assert(!error,error);
  assert(all.length+quantityUsed(account().id,ses.id)<=db.policy.maxTickets,'Đã đạt giới hạn '+db.policy.maxTickets+' vé cho suất diễn này.');
  selection.seats=all.map(seat=>seat.code);
}
function holdSeats() {
  const user=account();
  const e=lookup(db.events,selection.eventId);
  const ses=sessionOf(e,selection.sessionId);
  assert(user&&sellable(e,ses)&&!needsVerified(user),'Tài khoản hoặc suất diễn chưa đủ điều kiện giữ ghế.');
  const seats=chosenSeats(ses).map(({code,zone,type})=>({code,zone,type,price:saleType(ses,type)?.price}));
  const error=inventoryError(ses,seats);
  assert(!error,error);
  assert(seats.every(s=>s.price!=null),'Loại vé đã đóng bán.');
  assert(seats.length+quantityUsed(user.id,ses.id)<=db.policy.maxTickets,'Đã đạt giới hạn '+db.policy.maxTickets+' vé cho suất diễn này.');
  const created=Date.now();
  const subtotal=seats.reduce((sum,seat)=>sum+seat.price,0);
  const fee=Math.round(subtotal*db.policy.serviceRate/100);
  const order={id:uid('DH-').slice(0,15),user:user.id,eventId:e.id,sessionId:ses.id,state:'PENDING_PAYMENT',
    created,expires:created+db.policy.holdMinutes*minute,seats,subtotal,fee,discount:0,total:subtotal+fee,
    refund:e.live.refund,refundHours:db.policy.refundHours,serviceRate:db.policy.serviceRate};
  financeSnapshot(order,e);
  db.orders.unshift(order);
  selection.seats=[];
  audit('Giữ ghế',order.id,seats.map(s=>s.code).join(', '));
  go('checkout/'+order.id);
}
const formHandlers={
  discover(form) {
    homeFilters=Object.fromEntries(form);
  },
  eventFilters(form) {
    const next=Object.fromEntries(form);
    eventFilters=next;eventPage=1;
  },
  calendarFilters(form) {calendarFilters=Object.fromEntries(form);},
  venueFilters(form) {venueFilters=Object.fromEntries(form);},
  voucher(form,element) {
    const order=own(element.dataset.id,db.orders);
    assert(heldOrder(order),'Đơn đã hết thời hạn giữ ghế.');
    const voucher=db.vouchers.find(v=>v.code===textValue(form,'code').toUpperCase());
    const error=voucherError(voucher,order);
    assert(!error,error);
    updateTotals(order,voucher);financeSnapshot(order,lookup(db.events,order.eventId));
    audit('Giữ lượt voucher',order.id,voucher.code);
    notifyUi('Đã áp dụng '+voucher.code+'.');
  },
  payment(form,element) {
    const order=own(element.dataset.id,db.orders);
    notifyUi(confirmPayment(order,textValue(form,'scenario'),textValue(form,'method')));
  },
  staffLookup(form) {staffLookup=textValue(form,'query');staffResult='';},
  scan(form) {performScan(textValue(form,'code'),textValue(form,'zone'),'QR');},
  accountSearch(form) {ui.accountSearch=textValue(form,'query');}
};
const actions={
  workExport:openWorkExport,
  adminAssignments:openAdminAssignments,adminAssign:adminAssignmentForm,
  close:dismissDialog,demo:demoSwitcher,login:(id,element)=>showAuth('login',element.note),register:()=>showAuth('register'),forgot:()=>showAuth('forgot'),
  demoRole(id) {
    const original=demoAccounts.find(a=>a.id===id);
    const user=lookup(db.accounts,id);
    assert(id==='guest'||original&&user&&!user.locked,'Tài khoản mẫu đã bị khóa.');
    const keepRoute=['book','event','venue','invite','checkout'].includes(route[0])||route[0]==='account'&&route[1]==='application';
    setAuth(id==='guest'?null:user);
    closeDialog();
    if (ui.returnAction&&account()) {afterAuth();return;}
    ui.returnAction=null;ui.verifiedAction=null;
    editorId='';venueEditorId='';staffResult='';staffLookup='';
    if (keepRoute&&(id==='guest'||user?.role==='user')) render();
    else go(id==='guest'||user.role==='user'?'home':'work');
  },
  logout() {if(account()) audit('Đăng xuất',account().id);setAuth(null);go('home');},
  editProfile:()=>{if(guard()) profileForm();},
  verifyContact:()=>{if(guard()) contactForm();},
  changePassword:()=>{if(guard()) passwordForm('Đổi mật khẩu',account());},
  performReset(token) {
    const reset=db.resets.find(r=>r.token===token);
    const user=lookup(db.accounts,reset?.user);
    assert(reset&&!reset.used&&reset.until>Date.now()&&user,'Liên kết đã hết hiệu lực.');
    passwordForm('Đặt mật khẩu mới',user,token);
  },
  homeCity(id) {
    const form=root.querySelector('[data-form="discover"]');
    homeFilters=form?Object.fromEntries(new FormData(form)):homeFilters;
    homeFilters.city=homeFilters.city===id?'':id;
  },
  clearHome() {homeFilters={};},
  discoverMore() {eventFilters={...homeFilters,upcoming:'on'};eventPage=1;go('events');},
  exploreCategory(id) {eventFilters={cat:id};eventPage=1;go('events');},
  shareEvent,exportSessionCalendar,quickBooking,
  help:helpDialog,
  toggleRental(id) {
    assert(lookup(db.venues,id)?.state==='ACTIVE','Địa điểm không còn khả dụng.');
    if (rentalSelection.has(id)) rentalSelection.delete(id); else rentalSelection.add(id);
  },
  clearRentalSelection() {rentalSelection.clear();},
  clearEvents() {eventFilters={};eventPage=1;},
  clearVenues() {venueFilters={};},
  eventPage(id) {eventPage=Number(id);},
  favorite(id) {
    if (!guard('favorite',id)) return;
    assert(publicEvent(lookup(db.events,id)),'Sự kiện không còn khả dụng.');
    const user=account();
    const ids=new Set(db.favorites[user.id]||[]);
    const saved=ids.has(id);
    if (saved) ids.delete(id); else ids.add(id);
    db.favorites[user.id]=[...ids];
    notifyUi(saved?'Đã bỏ yêu thích.':'Đã lưu vào mục Yêu thích.');
  },
  map(id,element) {
    const e=lookup(db.events,element.event),s=sessionOf(e,id);
    assert(publicEvent(e)&&s,'Suất diễn không còn công khai.');
    openDialog('Sơ đồ · '+s.name,mapView(s.zones));
  },
  zone(id) {selection.zone=id;selection.row='';},
  clearSeats() {selection.seats=[];},
  removeSeat(id) {
    const ses=sessionOf(lookup(db.events,selection.eventId),selection.sessionId);
    if (!ses) return;
    const selected=chosenSeats(ses);
    const seat=selected.find(s=>s.code===id);
    if (!seat) return;
    selection.seats=selected.filter(s=>s.code!==id&&!(seat.group&&s.zone===seat.zone&&s.group===seat.group)).map(s=>s.code);
  },
  seat:selectSeat,
  hold:holdSeats,
  removeVoucher(id) {
    const order=own(id,db.orders);
    assert(heldOrder(order),'Giữ ghế đã hết hạn.');
    updateTotals(order,null);financeSnapshot(order,lookup(db.events,order.eventId));
  },
  cancelHold(id) {
    confirmAction('Hủy giữ ghế','Ghế và lượt voucher được giải phóng ngay.',()=>{
      const order=own(id,db.orders);
      assert(heldOrder(order),'Giữ ghế đã hết hạn.');
      order.state='CANCELLED';
      audit('Hủy giữ ghế',order.id);
    });
  },
  ticket(id) {
    const t=own(id,db.tickets);
    openDialog('Vé '+id,`<div class="stack">${ticketShowcase(t)}
      ${validTicket(t)?'<div class="cluster">'+button('Tải PDF','downloadTicket',id)+button('In vé','printTicket',id,'secondary')+'</div>':''}</div>`);
  },
  printTicket(id) {printTicket(own(id,db.tickets));},
  downloadTicket(id) {return downloadTicketPdf(own(id,db.tickets));},
  printInvite(token) {printTicket(printableInvite(token));},
  downloadInvite(token) {return downloadTicketPdf(printableInvite(token));},
  claim:token=>{if(guard('claim',token)) contactForm(token);},
  application:()=>{if(guard('application','')) applicationForm();},
  applicationDetails,applicationDecision,
  flagOrg(id) {
    assert(canManage('applications'),'Không có quyền xử lý hồ sơ.');
    openDialog('Đề xuất tạm dừng Ban tổ chức',field('Lý do',area('reason','','required maxlength="700"')),f=>{
      assert(canManage('applications'),'Không có quyền xử lý.');
      const app=lookup(db.applications,id);
      assert(app?.state==='APPROVED','Hồ sơ không còn hợp lệ.');
      db.alerts.unshift({id:uid(),text:'Đề xuất tạm dừng '+app.company+' · '+textValue(f,'reason'),time:Date.now()});
      audit('Đề xuất tạm dừng BTC',app.id);
    },'Gửi đề xuất');
  },
  venueCalendar,
  rental:id=>{if(guard('rental',id)) rentalForm(id);},
  rentalDetail:rentalDetails,payDeposit:depositDialog,rentalCancel,rentalDecision,
  refund:refundForm,refundDecision,
  refundDetail(id) {
    const r=own(id,db.refunds,canManage('refunds'));
    openDialog('Hoàn tiền '+id,`<div class="stack">${row(r.reason,fmt(r.created),tag(r.state))}
      ${row('Số tiền',money(r.amount))}${r.orderAllocations?.length?r.orderAllocations.map(a=>row('Đơn '+a.orderId,money(a.amount)+' · '+a.ticketIds.join(', '))).join(''):
        r.orderId?row('Đơn liên quan',r.orderId):''}
      ${r.rentalId?row('Đặt cọc',r.rentalId):''}${r.transferId?row('Mã giao dịch',r.transferId):''}
      ${r.proof?row('Chứng từ',r.proof):''}${r.note?row('Ghi chú xử lý',r.note):''}</div>`);
  },
  support(id,{event:context=''}) {
    if (guard('support',id,{event:context})) supportForm(id,context);
  },
  supportDecision,
  supportRefund(id) {
    const item=lookup(db.support,id);
    const ticket=lookup(db.tickets,item?.ticketId);
    const order=lookup(db.orders,ticket?.orderId);
    assert(canManage('support')&&canManage('refunds')&&order?.paidAt&&ticket.user===item.user&&item.state==='OPEN'&&!item.refundId,
      'Khiếu nại chưa đủ điều kiện hoàn ngoại lệ.');
    const available=()=>{
      const reserved=reservedRefundAmount(order.id);
      return Math.max(0,Math.min(amountForTicket(order,ticket,true),order.total-reserved));
    };
    const maximum=available();
    assert(maximum>0,'Đơn không còn số tiền đủ điều kiện hoàn.');
    openDialog('Hoàn ngoại lệ · '+id,notice('Yêu cầu sẽ qua hàng đợi xét duyệt và đối chiếu chứng từ.')+
      field('Mức đề nghị (đ)',input('amount',maximum,'number','required min="1" max="'+maximum+'" step="1"'))+
      field('Lý do',area('reason','','required maxlength="700"')),form=>{
      const current=lookup(db.support,id);
      const amount=currencyNumber(form,'amount');
      assert(canManage('support')&&canManage('refunds')&&current?.state==='OPEN'&&!current.refundId&&amount>0&&amount<=available(),
        'Yêu cầu đã đổi trạng thái hoặc số tiền vượt giới hạn.');
      const refund=queueRefund({user:current.user,eventId:ticket.eventId,orderId:order.id,reason:'Ngoại lệ '+id+' · '+textValue(form,'reason'),
        amount,state:'MANUAL_REVIEW'});
      current.refundId=refund.id;
      current.answer='Đã chuyển xét duyệt hoàn ngoại lệ '+refund.id;
      audit('Chuyển xét hoàn ngoại lệ',id,refund.id);
    },'Chuyển xét duyệt');
  },
  review(id) {
    const t=own(id,db.tickets);
    const problem=reviewProblem(t);
    assert(!problem,problem);
    openDialog('Đánh giá '+eventTitle(t.eventId),field('Điểm',select('score',[5,4,3,2,1].map(n=>opt(n,n+'/5')).join('')))+
      field('Nhận xét',area('text','','required maxlength="600"')),f=>{
        const t=own(id,db.tickets);
        const now=Date.now();
        const problem=reviewProblem(t,now);
        assert(!problem,problem);
        const score=Number(f.get('score')),text=textValue(f,'text');
        assert(Number.isInteger(score)&&score>=1&&score<=5&&text&&text.length<=600,'Chọn điểm từ 1 đến 5 và nhập nhận xét tối đa 600 ký tự.');
        db.reviews.unshift({id:uid(),user:t.user,eventId:t.eventId,score,text,visible:true,time:now});
        audit('Đánh giá sự kiện',t.eventId);
      },'Gửi đánh giá');
  },
  moderateRating(id) {
    const review=lookup(db.reviews,id);
    assert(canManage('support')&&review,'Không có quyền kiểm duyệt đánh giá.');
    review.visible=review.visible===false;
    audit('Kiểm duyệt đánh giá',review.eventId,review.visible?'Hiện':'Ẩn');
  },
  read(id) {own(id,db.notifications).read=true;},
  readAll() {assert(account(),'Cần đăng nhập.');db.notifications.filter(n=>n.user===account().id).forEach(n=>{n.read=true;});},
  newEvent:()=>eventForm(),
  editEvent(id) {requireOwner(id);editorId=id;editorSection='general';},
  editSection(id) {editorSection=id;},
  endEdit() {editorId='';},
  eventForm,
  submitEvent,
  deleteEvent(id) {
    confirmAction('Xóa bản nháp', 'Bản nháp sự kiện này sẽ được xóa khỏi danh sách mẫu.',()=>{
      const e=requireOwner(id);
      assert(!e.live&&e.state!=='PENDING'&&!db.rentals.some(r=>r.eventId===id&&rentalBlocking(r)),'Không thể xóa sự kiện đang duyệt, công khai hoặc có lịch thuê.');
      db.events=db.events.filter(event=>event.id!==id);editorId='';
      audit('Xóa sự kiện nháp',id);
    });
  },
  sessionForm:(id,element)=>sessionForm(element.event||editorId,id),
  types:(id,element)=>typeForm(element.event,id),
  deleteSession(id,element) {
    const eventId=element.event;
    confirmAction('Xóa suất khỏi bản nháp','Suất công khai được cập nhật qua xét duyệt.',()=>{
      const e=requireOwner(eventId);
      const draft=editable(e);
      assert(!inventory(id).size,'Suất diễn đã có vé hoặc đang giữ ghế.');
      draft.sessions=draft.sessions.filter(s=>s.id!==id);audit('Xóa suất khỏi bản nháp',eventId,id);
    });
  },
  draftMap(id,element) {
    const e=requireOwner(element.event),s=(e.draft||e.live).sessions.find(s=>s.id===id);
    assert(s,'Suất không tồn tại.');openDialog('Sơ đồ suất '+s.name,mapView(s.zones));
  },
  saleToggle(id,element) {
    const e=requireOwner(element.event),s=sessionOf(e,id);
    assert(s&&e.state==='ACTIVE','Suất không ở trạng thái công khai.');
    s.open=!s.open;
    const draft=sessionOf(e,id,'draft');if (draft) draft.open=s.open;
    audit(s.open?'Mở bán suất':'Đóng bán suất',e.id,id);
  },
  versionDetails(id,element) {
    const e=requireOwner(element.event);
    const v=lookup(e.versions,id);
    assert(v?.data,'Phiên bản không tồn tại.');
    openDialog('Phiên bản '+v.version,versionDescription(v.data));
  },
  requestEvent:lifecycleForm,
  prep(id) {
    const event=requireOwner(id);
    openDialog('Tiến độ chuẩn bị',field('Tiến độ',area('text',event.prep,'required maxlength="1000"')),f=>{
      requireOwner(id).prep=textValue(f,'text');audit('Cập nhật tiến độ',id);
    });
  },
  eventOps(id) {
    const e=requireOwner(id);
    assert(e.live,'Chưa có sự kiện công khai.');
    openDialog('Vận hành · '+eventTitle(id),eventOps(e));
  },
  inspectEvent(id) {
    const e=lookup(db.events,id);
    assert(canManage('reviews')&&e,'Không có quyền xem hồ sơ.');
    openDialog('Kiểm tra '+eventTitle(id),`<div class="stack">
      ${e.live?panel('Phiên bản công khai '+e.version,versionDescription(e.live)):''}
      ${e.draft?panel('Phiên bản đề xuất',versionDescription(e.draft)):''}
      ${e.request?row('Yêu cầu '+e.request.kind,e.request.reason):''}
      ${e.live?eventOps(e):''}</div>`);
  },
  decideEvent:eventDecision,
  suspend(id) {
    const event=lookup(db.events,id);
    assert(canManage('reviews')&&event?.live,'Không có quyền đình chỉ.');
    const expected=event.state;
    const restoring=expected==='SUSPENDED';
    openDialog(restoring?'Khôi phục sự kiện':'Đình chỉ sự kiện',`${field('Lý do',area('note','','required maxlength="800"'))}
      ${field('Bằng chứng / mã biên bản mẫu',input('proof','','text',required))}`,f=>{
      const e=lookup(db.events,id);
      assert(canManage('reviews')&&e?.state===expected,'Sự kiện đã đổi trạng thái.');
      if (restoring) {
        ensureEventReady(e,e.live);e.state=e.previousState||'ACTIVE';
      } else {
        e.previousState=e.state;e.state='SUSPENDED';
        db.orders.filter(o=>o.eventId===id&&heldOrder(o)).forEach(o=>{o.state='CANCELLED';});
      }
      const note=textValue(f,'note');
      e.decision=note;
      announce(e,(restoring?'Sự kiện đã được khôi phục. ':'Sự kiện tạm ngừng. ')+note);
      audit(restoring?'Khôi phục sự kiện':'Đình chỉ sự kiện',id,textValue(f,'proof')+' · '+note);
    },'Xác nhận');
  },
  announcement(id) {
    requireOwner(id);
    openDialog('Gửi thông báo sự kiện',field('Kênh',select('channel',choices(['Trong ứng dụng','Email','SMS'])))+
      field('Thông báo',area('text','','required maxlength="1000"')),f=>{
      announce(requireOwner(id),textValue(f,'text'),textValue(f,'channel'));
    },'Công bố');
  },
  invite:inviteDialog,
  inviteLink(id) {
    const t=authorizedInvite(id);
    openDialog('Liên kết vé mời',`<div class="stack">${row(t.contact,'Quá hạn lúc '+fmt(t.inviteUntil),tag(t.state))}
      ${link('Mở vé như khách','invite/'+t.token,'btn secondary')}</div>`);
  },
  revokeInvite(id) {
    confirmAction('Thu hồi vé mời','Ghế được trả về tồn kho nếu vé chưa sử dụng.',()=>{
      const t=authorizedInvite(id);
      assert(t.state==='ISSUED','Vé đã được xử lý hoặc check-in.');
      t.state='REVOKED';audit('Thu hồi vé mời',t.eventId,id);
      message(t.user,'Vé mời đã bị thu hồi',eventTitle(t.eventId));
    });
  },
  resendInvite(id) {
    const t=authorizedInvite(id);
    assert(t.state==='ISSUED'&&t.inviteUntil>Date.now(),'Vé mời đã hết hiệu lực.');
    sendMock(t.channel,t.contact,'Gửi lại vé mời','#invite/'+t.token);audit('Gửi lại vé mời',t.eventId,id);
    notifyUi('Đã thêm bản ghi gửi mô phỏng.');
  },
  renewInvite(id) {
    const t=authorizedInvite(id);
    const e=lookup(db.events,t.eventId),s=sessionOf(e,t.sessionId);
    assert(t.state==='REVOKED'&&['ACTIVE','POSTPONED'].includes(e.state)&&s?.start>Date.now()&&saleType(s,'Invitation'),'Vé mời không thể phát lại.');
    const error=inventoryError(s,[{code:t.seat,zone:t.zone,type:'Invitation'}]);
    assert(!error,error);
    t.state='ISSUED';t.token=uid('M-');t.inviteUntil=s.end;
    sendMock(t.channel,t.contact,'Vé mời mới','#invite/'+t.token);audit('Phát lại vé mời',t.eventId,id);
    notifyUi('Đã tạo liên kết mới.');
  },
  grant:grantDialog,
  revokeGrant(id) {
    const g=lookup(db.grants,id);
    requireOwner(g?.eventId);
    confirmAction('Thu hồi quyền nhân sự','Quyền được gỡ ngay ở sự kiện hoặc suất được giao.',()=>{
      const grant=lookup(db.grants,id);
      requireOwner(grant?.eventId);grant.active=false;
      audit('Thu hồi quyền nhân sự',grant.eventId,id);
    });
  },
  manualScan(id) {
    assert(canOperate('checkin.manual',staffScope.eventId,staffScope.sessionId),'Không có quyền check-in thủ công.');
    const t=lookup(db.tickets,id);
    assert(t?.sessionId===staffScope.sessionId,'Vé nằm ngoài suất được giao.');
    openDialog('Check-in thủ công',notice('Đối chiếu tên, liên hệ và mã vé trước khi tiếp tục.')+
      check('verified','Tôi đã xác minh người xuất trình vé',false,'yes',true),f=>{
        assert(f.has('verified'),'Cần xác minh khách trước.');
        performScan(id,'','Thủ công');
      },'Xác nhận check-in');
  },
  staffProposal(id) {
    assert(canStaff('content.propose',id,staffScope.sessionId),'Không có quyền gửi đề xuất.');
    openDialog('Đề xuất nội dung',field('Nội dung',area('proposal','','required maxlength="1000"')),f=>{
      assert(canStaff('content.propose',id,staffScope.sessionId),'Quyền đề xuất đã bị thu hồi.');
      const event=lookup(db.events,id);
      const proposal=textValue(f,'proposal');
      db.support.unshift({id:uid('CS-').slice(0,15),state:'OPEN',user:account().id,eventId:id,sessionId:staffScope.sessionId,
        subject:'Đề xuất nội dung của nhân sự',text:proposal,time:Date.now()});
      message(event.owner,'Đề xuất cập nhật nội dung',eventTitle(id)+' · '+proposal);
      audit('Đề xuất nội dung',id);
    },'Gửi BTC');
  },
  incident(id) {
    assert(canStaff('attendees.view',id,staffScope.sessionId),'Không có quyền ghi nhận sự cố.');
    openDialog('Sự cố tại cổng',field('Chi tiết sự cố',area('text','','required maxlength="1000"')),f=>{
      assert(canStaff('attendees.view',id,staffScope.sessionId),'Quyền đã bị thu hồi.');
      const text=textValue(f,'text');
      db.alerts.unshift({id:uid(),text:eventTitle(id)+' · '+text,time:Date.now()});
      audit('Sự cố tại cổng',id,text);
    },'Ghi sự cố');
  },
  promo:promoDialog,
  approvePromo(id) {
    requireAdmin();const v=lookup(db.vouchers,id);
    assert(v?.state==='PENDING','Chiến dịch đã được xử lý.');
    v.state='ACTIVE';message(v.owner,'Voucher được phê duyệt',v.code);audit('Duyệt voucher',id);
  },
  rejectPromo(id) {
    requireAdmin();
    openDialog('Từ chối chiến dịch',field('Lý do',area('note','','required maxlength="600"')),f=>{
      requireAdmin();const v=lookup(db.vouchers,id);
      assert(v?.state==='PENDING','Chiến dịch đã được xử lý.');
      v.state='REJECTED';message(v.owner,'Voucher bị từ chối',v.code+' · '+textValue(f,'note'));
      audit('Từ chối voucher',id);
    },'Từ chối');
  },
  stopPromo(id) {
    const user=account(),v=lookup(db.vouchers,id);
    assert(v?.state==='ACTIVE'&&(user?.role==='admin'||user?.role==='organizer'&&v.owner===user.id),'Không có quyền dừng voucher.');
    v.state='DISABLED';audit('Dừng voucher',id);
  },
  transactionReview(id) {
    assert(canManage('payments'),'Không có quyền đối soát giao dịch.');
    decision('Đối chiếu phản hồi PayOS',[['paid','Xác nhận đã nhận tiền'],['failed','Xác nhận giao dịch thất bại']],(result,note)=>{
      assert(canManage('payments'),'Không có quyền đối soát giao dịch.');
      const transaction=lookup(db.transactions,id);
      assert(transaction?.state==='MANUAL_REVIEW','Giao dịch đã xử lý.');
      const order=lookup(db.orders,transaction.orderId);
      const valid=order&&['PENDING_PAYMENT','EXPIRED','PAID','PAID_OUT_OF_STOCK'].includes(order.state);
      assert(valid,'Đơn không còn ở trạng thái đối soát.');
      notifyUi(confirmPayment(order,result,transaction.channel));
      transaction.state='RESOLVED';transaction.note=note;
      audit('Đối chiếu giao dịch',id,result+' · '+note);
    });
  },
  bank:bankForm,contract:contractForm,settlement:settlementForm,settlementDetail,settlementResponse,settlementApprove,payoutCreate,payoutDecision,
  settlementCsv(id) {
    const s=lookup(db.settlements,id);
    assert(s&&(canManage('settlements')||account()?.role==='organizer'&&s.owner===account().id),'Bảng đối soát nằm ngoài phạm vi.');
    const rows=[['Đơn','Suất','Thực thu','Hoàn','Phí','Hợp đồng'],...s.entries.map(e=>[e.orderId,e.sessionId,e.gross,e.refunds,e.platformFee,e.contractId])];
    download(id+'.csv','\uFEFF'+rows.map(cells=>cells.map(value=>'"'+String(value??'').replaceAll('"','""')+'"').join(',')).join('\r\n'),'text/csv;charset=utf-8');
  },
  managerAccount:()=>adminAccountForm(),manageAccount:adminAccountForm,
  verifyBank(id) {
    requireAdmin();
    const person=lookup(db.accounts,id);
    assert(person?.role==='organizer'&&person.bankState==='Chờ Admin xác minh','Thông tin thụ hưởng chưa được gửi.');
    confirmAction('Xác minh thông tin thụ hưởng','Xác nhận đã kiểm tra dữ liệu thụ hưởng mẫu của '+person.name+'.',()=>{
      requireAdmin();
      const current=lookup(db.accounts,id);
      assert(current?.role==='organizer'&&current.bankState==='Chờ Admin xác minh','Thông tin thụ hưởng đã đổi.');
      current.bankState='Đã xác minh trong bản mẫu';
      message(id,'Đã xác minh thông tin thụ hưởng');
      audit('Xác minh thụ hưởng',id);
    });
  },
  venueForm,
  editVenue(id) {requireAdmin();venueEditorId=id;},
  endVenue() {venueEditorId='';},
  hideVenue(id) {
    requireAdmin();const v=lookup(db.venues,id);assert(v,'Địa điểm không tồn tại.');
    v.state=v.state==='ACTIVE'?'HIDDEN':'ACTIVE';audit('Thay đổi hiển thị địa điểm',id,v.state);
  },
  venueZones:venueZoneForm,venueBlock:venueBlockForm,
  releaseBlock(id,element) {
    requireAdmin();
    const v=lookup(db.venues,element.venue);
    assert(v?.blocks.some(block=>block.id===id),'Khung chặn đã được mở.');
    v.blocks=v.blocks.filter(block=>block.id!==id);
    audit('Mở lịch địa điểm',v.id);
  },
  categoryEdit:categoryDialog,banner:bannerForm,platformText,policy:policyForm,config:configForm,
  restoreConfig(id) {
    requireAdmin();
    const record=lookup(db.configHistory,id);
    assert(record,'Phiên bản cấu hình không tồn tại.');
    confirmAction('Khôi phục cấu hình','Cấu hình được áp dụng dưới một phiên bản mới trong bản mẫu.',()=>{
      requireAdmin();
      db.configHistory.unshift({id:uid(),settings:clone(db.settings),security:{
        passwordLength:db.policy.passwordLength,sessionHours:db.policy.sessionHours},time:Date.now(),reason:'Trước khi khôi phục'});
      db.settings={...record.settings,version:db.settings.version+1};
      Object.assign(db.policy,record.security);
      audit('Khôi phục cấu hình',id);
    });
  },
  resolveAlert(id) {
    assert(canManage('monitoring'),'Không có quyền cập nhật cảnh báo.');
    const item=lookup(db.alerts,id);
    assert(item,'Cảnh báo đã thay đổi.');item.resolved=true;audit('Đóng cảnh báo',id);
  },
  backupTest:backupDialog,
  backupExport() {
    requireAdmin();
    db.backups.unshift({action:'Xuất sao lưu',time:Date.now(),note:'Bản chụp JSON cục bộ'});
    audit('Xuất sao lưu', 'JSON');
    download('VENUE-sao-luu-'+dayKey(Date.now())+'.json',JSON.stringify({app:storeKey,time:Date.now(),data:db},null,2),'application/json');
  }
};
async function transaction(fn) {
  const perform=async ()=>{
    db=load();
    if (expire()) save();
    try {
      const result=await fn();
      save();
      return result;
    } catch(error) {
      db=load();
      throw error;
    }
  };
  return navigator.locks?.request ? navigator.locks.request(storeKey,perform) : perform();
}
async function runAction(name,id='',element={}) {
  if (!Object.hasOwn(actions,name)) return;
  const handler=actions[name];
  await transaction(()=>handler(id,element));
  if (!modal.open) restoreOrRender();
}
let refreshOnClose=false;
function refreshSharedState() {
  if (modal.open||actionRunning||!refreshOnClose) return;
  refreshOnClose=false;
  db=load();
  restoreOrRender();
}
function showFormError(form,message) {
  const target=form.querySelector('.form-error');
  if (target) target.textContent=message;
  else notifyUi(message);
}
document.addEventListener('click',async event=>{
  const toggle=event.target.closest('[data-password-toggle]');
  if (toggle) {
    const password=modal.querySelector('#login-password');
    const visible=password.type==='password';
    password.type=visible?'text':'password';
    toggle.textContent=visible?'Ẩn':'Hiện';
    toggle.setAttribute('aria-label',visible?'Ẩn mật khẩu':'Hiện mật khẩu');
    return;
  }
  if (event.target.closest('[data-footer-top]')) {
    window.scrollTo(0,0);
    root.querySelector('.site-header .logo')?.focus({preventScroll:true});
    return;
  }
  const button=event.target.closest('[data-action]');
  if (!button||button.disabled) return;
  if (button.dataset.action==='close') {dismissDialog();return;}
  if (actionRunning) return;
  event.preventDefault();
  actionRunning=true;
  try {await runAction(button.dataset.action,button.dataset.id,button.dataset);}
  catch(error) {notifyUi(error.message);if (!modal.open) restoreOrRender();}
  finally {actionRunning=false;refreshSharedState();}
});
document.addEventListener('submit',async event=>{
  const form=event.target;
  const key=form.dataset.form;
  if (!key) return;
  event.preventDefault();
  if (actionRunning) return;
  const current=currentModal;
  const handler=key==='dialog'?current?.submit:Object.hasOwn(formHandlers,key)?formHandlers[key]:null;
  if (!handler) return;
  const data=new FormData(form);
  actionRunning=true;
  const setBusy=key==='dialog'?current?.setBusy:null;
  try {
    setBusy?.(data);
    const result=await transaction(()=>handler(data,form));
    if (key==='dialog'&&currentModal===current&&result!==false) closeDialog();
    if (!modal.open) restoreOrRender();
  } catch(error) {
    showFormError(form,error.message);
  } finally {setBusy?.(null);actionRunning=false;refreshSharedState();}
});
async function readQr(file) {
  assert(file&&file.size<16*1024*1024,'Chọn ảnh QR nhỏ hơn 16 MB.');
  const bitmap=await createImageBitmap(file);
  try {
    if ('BarcodeDetector' in window) {
      try {
        const result=await new BarcodeDetector({formats:['qr_code']}).detect(bitmap);
        if (result.length) return result[0].rawValue;
      } catch {}
    }
    assert(typeof jsQR==='function','Trình duyệt chưa hỗ trợ đọc ảnh QR. Có thể nhập mã vé.');
    const scale=Math.min(1,1100/Math.max(bitmap.width,bitmap.height));
    const canvas=document.createElement('canvas');
    canvas.width=Math.max(1,Math.round(bitmap.width*scale));
    canvas.height=Math.max(1,Math.round(bitmap.height*scale));
    const ctx=canvas.getContext('2d',{willReadFrequently:true});
    ctx.drawImage(bitmap,0,0,canvas.width,canvas.height);
    const data=ctx.getImageData(0,0,canvas.width,canvas.height);
    const result=jsQR(data.data,data.width,data.height);
    assert(result?.data,'Không đọc được QR. Hãy chọn ảnh rõ hơn hoặc nhập mã.');
    return result.data;
  } finally {bitmap.close();}
}
document.addEventListener('input',event=>{
  if (event.target.matches('[data-work-search]')) {workFilter.query=event.target.value;updateWorkFilter();}
});
document.addEventListener('change',async event=>{
  const target=event.target;
  if (target.dataset.change==='workTab') go('work/'+target.value);
  if (target.matches('[data-work-status]')) {workFilter.status=target.value;updateWorkFilter();}
  if (target.dataset.change==='seatRow') {selection.row=target.value;render();}
  if (target.dataset.change==='staffScope') {
    const [eventId,sessionId]=target.value.split('/');
    staffScope={eventId,sessionId};staffResult='';staffLookup='';render();
  }
  if (target.dataset.change==='sessionVenue') {
    const venue=lookup(db.venues,target.value);
    if (venue) target.form.elements.zones.value=stringifyZones(venue.zones);
  }
  if (target.dataset.change==='qrFile'&&target.files[0]&&!actionRunning) {
    actionRunning=true;
    const scope=staffScope;
    try {
      const code=await readQr(target.files[0]);
      assert(scope===staffScope&&route[0]==='work'&&route[1]==='scan','Suất đang thao tác đã thay đổi.');
      const form=root.querySelector('[data-form="scan"]');
      if (form) {form.elements.code.value=code;form.elements.code.focus();}
      notifyUi('Đã đọc QR. Kiểm tra mã trước khi xác nhận.');
    } catch(error) {notifyUi(error.message);}
    finally {target.value='';actionRunning=false;refreshSharedState();}
  }
});
modal.addEventListener('click',event=>{if (event.target===modal) dismissDialog();});
modal.addEventListener('cancel',event=>{
  event.preventDefault();
  dismissDialog();
});
modal.addEventListener('close',()=>{
  if (!modal.open) {currentModal=null;modal.replaceChildren();}
  refreshSharedState();
});
function routeChanged() {
  const from=route;
  const calendarNav=document.activeElement?.dataset.calendarNav;
  closeDialog();
  route=parseRoute();
  if (from[0]==='work'&&route[0]!=='work') {editorId='';venueEditorId='';}
  if (from[1]!==route[1]&&route[0]==='work') {staffResult='';staffLookup='';}
  if (route[0]==='event'&&db.policy.history&&account()&&publicEvent(lookup(db.events,route[1]))) {
    const id=route[1];
    transaction(()=>{
      const user=account();
      if (user&&publicEvent(lookup(db.events,id))) db.history[user.id]=[id,...(db.history[user.id]||[]).filter(x=>x!==id)].slice(0,30);
    }).catch(error=>notifyUi(error.message));
  }
  restoreOrRender();
  if (from[0]==='calendar'&&route[0]==='calendar'&&calendarNav) {
    if ((calendarNav==='day'||calendarNav==='reset')&&matchMedia('(max-width: 780px)').matches) {
      const heading=root.querySelector(calendarNav==='day'?'#calendar-agenda-title':'#calendar-month-title');
      heading?.scrollIntoView();
      heading?.focus({preventScroll:true});
      return;
    }
    const focus=[...root.querySelectorAll('[data-calendar-nav]')].find(el=>el.dataset.calendarNav===calendarNav&&
      (calendarNav!=='day'||el.classList.contains('is-selected')));
    (focus||root.querySelector('#calendar-agenda-title'))?.focus({preventScroll:true});
    return;
  }
  const anchor=route.length>1?document.getElementById(route.join('/')):null;
  if (anchor) {
    anchor.scrollIntoView();
    if (anchor.hasAttribute('tabindex')) anchor.focus({preventScroll:true});
  } else window.scrollTo(0,0);
}
window.addEventListener('hashchange',routeChanged);
window.addEventListener('storage',event=>{
  if (event.key!==storeKey) return;
  refreshOnClose=true;
  refreshSharedState();
});
if (!memory) save();
routeChanged();
let expiryPending=false;
setInterval(()=>{
  if (actionRunning||expiryPending) return;
  root.querySelectorAll('[data-expires]').forEach(el=>{el.textContent=remaining(Number(el.dataset.expires));});
  const now=Date.now();
  const upcoming=db.orders.some(o=>o.state==='PENDING_PAYMENT'&&o.expires<=now)||
    db.rentals.some(r=>r.state==='AWAITING_DEPOSIT'&&r.deadline<=now);
  const expiredAuth=auth&&!account();
  if (expiredAuth) {setAuth(null);closeDialog();restoreOrRender();}
  if (upcoming) {
    expiryPending=true;
    transaction(()=>{}).then(()=>{if(!modal.open&&!actionRunning) restoreOrRender();})
      .catch(error=>notifyUi(error.message)).finally(()=>{expiryPending=false;});
  }
},1000);

  
