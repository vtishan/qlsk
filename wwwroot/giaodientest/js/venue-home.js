const discoveryThemes = [
  ['Âm nhạc','music','Lễ hội, concert và nhạc sống'],
  ['Nghệ thuật','art','Triển lãm và nghệ thuật đương đại'],
  ['Sân khấu','theatre','Kịch và biểu diễn trực tiếp'],
  ['Hội thảo','conference','Gặp gỡ và học hỏi từ diễn giả'],
  ['Workshop','workshop','Trải nghiệm thực hành']
];

const venuePhotos = new Map([
  ['cung van hoa huu nghi|ha noi',{
    src:'../assets/images/cung-van-hoa-huu-nghi.webp',author:'thalling55',license:'by/2.0',licenseLabel:'CC BY 2.0',
    source:'https://commons.wikimedia.org/wiki/File:Cung_v%C4%83n_h%C3%B3a_H%E1%BB%AFu_ngh%E1%BB%8B_Vi%E1%BB%87t-X%C3%B4.jpg'
  }],
  ['cong vien bien dong|da nang',{
    src:'../assets/images/cong-vien-bien-dong.webp',author:'Leanh2001',license:'by-sa/4.0',licenseLabel:'CC BY-SA 4.0',
    source:'https://commons.wikimedia.org/wiki/File:C%C3%B4ng_vi%C3%AAn_Bi%E1%BB%83n_%C4%90%C3%B4ng_%C4%90%C3%A0_N%E1%BA%B5ng.jpg'
  }],
  ['trung tam hoi nghi quoc gia|ha noi',{
    src:'../assets/images/trung-tam-hoi-nghi-quoc-gia.webp',author:'Nguyen QuocTrung',license:'by-sa/4.0',licenseLabel:'CC BY-SA 4.0',
    source:'https://commons.wikimedia.org/wiki/File:Outside_Vietnam_National_Convention_Center.jpg'
  }],
  ['nha thi dau quan khu 7|tp. ho chi minh',{
    src:'../assets/images/nha-thi-dau-quan-khu-7.webp',author:'Magicknight94',licenseLabel:'Phạm vi công cộng',
    source:'https://commons.wikimedia.org/wiki/File:Khan_gia_xep_hang_cho_Nick_Vujicic_tai_QK7.jpg'
  }]
]);

function venueCities() {
  return [...new Set(['Hà Nội','TP. Hồ Chí Minh','Đà Nẵng',...db.venues.filter(v=>v.state==='ACTIVE').map(v=>v.city).sort()])];
}

function priceBands() {
  const prices=[...new Set(db.events.filter(publicEvent).flatMap(event=>event.live.sessions.flatMap(s=>s.types
    .filter(t=>t.enabled&&t.name!=='Invitation'&&Number.isFinite(t.price)&&t.price>=0).map(t=>t.price))))].sort((a,b)=>a-b);
  const size=Math.ceil(prices.length/4);
  const bands=[];
  for (let i=0;i<prices.length;i+=size) bands.push([prices[i],prices[Math.min(i+size-1,prices.length-1)]]);
  return bands;
}

function priceBandSelect(selected='') {
  return select('price',opt('','Mọi mức giá')+priceBands().map(([lo,hi])=>opt(lo+'-'+hi,lo===hi?money(lo):money(lo)+' – '+money(hi),selected)).join(''),'aria-label="Khoảng giá vé"');
}

function eventPriceBand(value) {
  return priceBands().find(([lo,hi])=>value===lo+'-'+hi);
}
function publicTicketPrices(session,band) {
  return session.types.filter(t=>t.name!=='Invitation'&&t.enabled&&(!band||t.price>=band[0]&&t.price<=band[1])).map(t=>t.price);
}
function filteredEvents(f,upcoming=false) {
  upcoming=upcoming||Boolean(f.upcoming);
  const band=eventPriceBand(f.price);
  const now=Date.now();
  const results=[];
  for (const event of db.events) {
    if (!publicEvent(event)||upcoming&&event.state!=='ACTIVE') continue;
    const info=event.live;
    if (f.cat&&info.cat!==f.cat||f.organizer&&!matches(organizerName(event),f.organizer)) continue;
    const content=[info.title,info.description,info.cat,info.participants,organizerName(event),...info.sessions.map(s=>venueName(s.venueId))].join(' ');
    if (f.query&&!matches(content,f.query)) continue;
    const sessions=info.sessions.filter(s=>{
      const venue=lookup(db.venues,s.venueId);
      return (!upcoming||s.start>now)&&(!f.city||venue?.city===f.city)&&(!f.venueId||s.venueId===f.venueId)&&
        (!f.date||dayKey(s.start)===f.date)&&(!f.session||matches(s.name+' '+fmt(s.start),f.session))&&
        (!f.stock||(sellable(event,s)&&sessionFree(s)>0))&&(!band||publicTicketPrices(s,band).length>0);
    });
    if (sessions.length) results.push({event,sessions,band});
  }
  return results;
}

function commonQuestions(items) {
  return `<div class="questions">${items.map(([question,answer])=>`<details><summary>${esc(question)}</summary><p>${esc(answer)}</p></details>`).join('')}</div>`;
}

function venueReasons(reasons) {
  return `<div class="reason-grid">${reasons.map(([icon,title,detail])=>`<article><div class="reason-icon">${ic(icon,22)}</div><h3>${esc(title)}</h3><p>${esc(detail)}</p></article>`).join('')}</div>`;
}

function homeView() {
  const banner=db.banners.find(b=>b.state==='ACTIVE');
  const feature=lookup(db.events,banner?.eventId);
  const f=homeFilters;
  const now=Date.now();
  const listings=upcomingEvents(f);
  const defaultBanner=banner?.id==='hero'&&banner.title==='Khám phá sự kiện|đáng nhớ.';
  const title=(!banner||defaultBanner?'Tìm sự kiện.|Chọn chỗ của bạn.':banner.title).split('|');
  const featured=publicEvent(feature)&&feature.state==='ACTIVE'&&feature.live.sessions.some(s=>s.start>now);
  const phone=db.settings.supportPhone;
  const reasons=[
    ['calendar','Biết trước lịch và chi phí','Xem giờ diễn, giá vé từng hạng và sức chứa địa điểm trước khi quyết định.'],
    ['ticket','Vé luôn trong tài khoản','Mở mã QR khi đến cổng, tải vé PDF hoặc in vé từ mục Vé của tôi.'],
    ['chat','Có chỗ để hỏi và theo dõi','Nhắn đội hỗ trợ VÉNUE và theo dõi phản hồi, yêu cầu hoàn tiền hoặc thuê địa điểm.']
  ];
  return `<section class="hero" aria-label="Khám phá VÉNUE">
    <img class="hero-image" src="../assets/images/hero.webp" alt="" fetchpriority="high">
    <div class="wrap hero-shell">
      <div class="hero-content">
        <h1>${title.map((part,i)=>i?'<span>'+esc(part)+'</span>':esc(part)).join('<br>')}</h1>
        <p>${esc(!banner||defaultBanner?'Từ đêm nhạc đến triển lãm, chọn trải nghiệm hợp gu và đặt vé ngay.':banner.text)}</p>
        <div class="cluster"><a href="#home/discover" class="btn lime">Khám phá sự kiện</a>${link('Thuê địa điểm','venues','btn ghost')}</div>
      </div>
      <div class="hero-dock">
        <div class="hero-shortcuts" aria-label="Tiện ích">
          <a href="#account/tickets">${ic('ticket',19)} Vé của tôi</a>
          <button type="button" data-action="support">${ic('chat',19)} Nhắn hỗ trợ</button>
          ${phone?`<a href="tel:${esc(phone.replace(/[^\d+]/g,''))}" aria-label="Gọi đội hỗ trợ VÉNUE ${esc(phone)}">${ic('phone',19)} Gọi hỗ trợ</a>`:
            `<button type="button" data-action="help">${ic('phone',19)} Liên hệ hỗ trợ</button>`}
        </div>
        ${featured?`<a class="hero-event" href="#event/${esc(feature.id)}"><span>${ic('calendar',21)}</span><span><small>Sự kiện nổi bật</small><strong>${esc(feature.live.title)}</strong></span></a>`:''}
      </div>
    </div>
  </section>
  <div class="wrap home-content">
    <section class="home-section home-discovery" id="home/discover" aria-labelledby="home-discovery-title" tabindex="-1">
      <div class="section-head split"><h2 id="home-discovery-title">Khám phá sự kiện</h2>${link('Xem tất cả','events','btn secondary compact')}</div>
      <form class="discover-bar" data-form="discover">
        <label class="input-icon">${ic('search',19)}${input('query',f.query,'search','aria-label="Tên sự kiện, nghệ sĩ hoặc địa điểm" placeholder="Tìm sự kiện, nghệ sĩ, địa điểm"')}</label>
        ${select('city',opt('','Mọi thành phố')+choices(venueCities(),f.city),'aria-label="Thành phố"')}
        ${priceBandSelect(f.price)}
        <button class="btn" type="submit">Tìm sự kiện</button>
      </form>
      <div class="cluster quick-cities" aria-label="Chọn nhanh thành phố">
        ${venueCities().slice(0,3).map(city=>button(city,'homeCity',city,'chip'+(f.city===city?' active':''),' aria-pressed="'+(f.city===city)+'"')).join('')}
        ${Object.values(f).some(Boolean)?button('Xóa lọc','clearHome','','ghost compact'):''}
      </div>
    </section>
    <section class="home-section upcoming-section" id="home/upcoming" aria-labelledby="upcoming-title" tabindex="-1">
      <div class="section-head split"><h2 id="upcoming-title">Sự kiện sắp diễn ra</h2><div class="cluster">
        ${link('Xem lịch','calendar','btn ghost compact')}${button('Xem thêm','discoverMore','','secondary compact')}</div></div>
      <div role="status" class="listing-count">${listings.length} sự kiện phù hợp</div>
      ${listings.length?'<div class="event-grid">'+listings.slice(0,4).map(({event,sessions,band})=>eventCard(event,sessions,band)).join('')+'</div>':empty('Chưa có sự kiện sắp diễn ra phù hợp. Hãy thử đổi thành phố hoặc khoảng giá.')}
    </section>
    <section class="home-section" aria-labelledby="themes-title">
      <div class="section-head"><h2 id="themes-title">Khám phá theo sở thích</h2></div>
      <div class="theme-grid">${discoveryThemes.filter(([cat])=>db.categories.includes(cat)).map(([cat,photo,detail])=>`<button type="button" class="theme-card" data-action="exploreCategory" data-id="${esc(cat)}">
        <img src="../assets/images/${photo}.webp" alt="" loading="lazy"><span><strong>${esc(cat)}</strong><small>${esc(detail)}</small></span></button>`).join('')}</div>
    </section>
    <section class="home-section trust-section"><div class="section-head"><h2>Vì sao chọn VÉNUE?</h2></div>${venueReasons(reasons)}</section>
    <section class="home-section host-callout">
      <img src="../assets/images/venue.webp" alt="" loading="lazy">
      <div><h2>Bạn đang lên kế hoạch tổ chức?</h2><p>Xem lịch trống, sức chứa và tiện ích. Gửi một hồ sơ thuê cho nhiều địa điểm.</p>
      <div class="cluster">${link('Khám phá địa điểm','venues','btn lime')}${account()?.role==='organizer'?link('Vào khu vực vận hành','work','btn ghost'):(!account()||account().role==='user'?link('Đăng ký Ban tổ chức','account/application','btn ghost'):'')}</div></div>
    </section>
    <section class="home-section home-faq"><div><h2>Trước khi đặt vé</h2><p class="muted">Cần hỏi thêm? ${button('Nhắn đội hỗ trợ','support','','ghost compact')}</p></div>${commonQuestions(faq())}</section>
  </div>`;
}

function upcomingEvents(filters) {
  const firstStart=item=>Math.min(...item.sessions.map(s=>s.start));
  return filteredEvents(filters,true).sort((a,b)=>firstStart(a)-firstStart(b));
}

function venueLocation(venue) {
  const city=venue.city&&!matches(venue.address,venue.city)?venue.city:'';
  return [venue.name,venue.address,city].filter(Boolean).join(', ');
}

function venueDirections(venue) {
  const params=new URLSearchParams({api:'1',destination:venueLocation(venue)});
  return `<a class="btn secondary compact" href="https://www.google.com/maps/dir/?${esc(params)}" target="_blank" rel="noopener">${ic('pin',16)} Chỉ đường</a>`;
}

function venuePhoto(venue) {
  if (isUploadedCover(venue.cover)) return {src:venue.cover};

  const name=normalized(venue.name);
  const photo=venuePhotos.get(name+'|'+normalized(venue.city));
  if (photo) return photo;
  const illustration=[
    [/nha hat/,'theatre'],[/hoi nghi/,'conference'],[/factory/,'art'],[/indika/,'dj'],
    [/studio/,'dance'],[/cong vien/,'music'],[/nha thi dau/,'concert']
  ].find(([pattern])=>pattern.test(name))?.[1]||'venue';
  return {src:`../assets/images/${illustration}.webp`,illustration:true};
}

function venueListView() {
  const f=venueFilters;
  const all=db.venues.filter(v=>v.state==='ACTIVE');
  const rates=[...new Set(all.map(v=>v.rate))].sort((a,b)=>a-b);
  const results=all.filter(v=>(!f.query||matches(v.name+' '+v.address,f.query))&&(!f.city||f.city===v.city)&&
    (!f.capacity||v.capacity>=Number(f.capacity))&&(!f.rate||v.rate<=Number(f.rate))&&(!f.amenities||matches(v.amenities,f.amenities))&&
    (!f.date||!rentalConflict(v,parseLocal(f.date),parseLocal(f.date)+day)));
  const picked=all.filter(v=>rentalSelection.has(v.id));
  return `${pageTitle('Địa điểm tổ chức','Chọn các không gian phù hợp rồi gửi chung một hồ sơ thuê.')}
  <form class="panel compact-filters" data-form="venueFilters">
    ${field('Địa điểm',input('query',f.query,'search','placeholder="Tên địa điểm"'))}
    ${field('Thành phố',select('city',opt('','Tất cả')+choices(venueCities(),f.city)))}
    ${field('Sức chứa tối thiểu',input('capacity',f.capacity,'number','min="1"'))}
    ${field('Giá thuê tối đa',select('rate',opt('','Mọi mức giá')+rates.map(rate=>opt(rate,money(rate),f.rate)).join('')))}
    <button class="btn" type="submit">Lọc địa điểm</button>
    <details class="span filter-more"${f.amenities||f.date?' open':''}><summary>Lọc thêm</summary><div class="form-grid">${field('Tiện ích',input('amenities',f.amenities))}${field('Ngày trống',input('date',f.date,'date'))}</div></details>
    ${Object.values(f).some(Boolean)?'<div class="span">'+button('Xóa bộ lọc','clearVenues','','secondary compact')+'</div>':''}
  </form>
  <div class="venue-toolbar split">
    <div role="status"><strong>${results.length} địa điểm</strong>${picked.length?`<p class="small muted">Đã chọn ${picked.length}. ${esc(picked.map(v=>v.name).join(', '))}</p>`:''}</div>
    ${picked.length?'<div class="cluster">'+button('Bỏ chọn tất cả','clearRentalSelection','','ghost compact')+button('Gửi yêu cầu thuê ('+picked.length+')','rental','','compact')+'</div>':''}
  </div>
  ${results.length?'<div class="event-grid venue-grid">'+results.map(v=>{
    const photo=venuePhoto(v);
    return `<article class="event-card venue-card">
    <a href="#venue/${esc(v.id)}" class="event-image" aria-label="${esc(v.name)}">
      <img src="${esc(photo.src)}" alt="" loading="lazy"><span class="card-tag">${esc(v.city)}</span>
      ${photo.illustration?'<span class="venue-photo-label">Ảnh minh họa</span>':''}
    </a>
    <div class="card-copy"><h3>${link(v.name,'venue/'+v.id)}</h3><p class="event-meta">${ic('users',16)} ${esc(v.capacity)} khách</p>
    <p class="event-meta">${esc(v.amenities)}</p><div class="card-price">${money(v.rate)} / khung</div>
    ${button(rentalSelection.has(v.id)?'Đã chọn':'Chọn địa điểm','toggleRental',v.id,(rentalSelection.has(v.id)?'':'secondary ')+'compact',' aria-pressed="'+rentalSelection.has(v.id)+'"')}</div></article>`;
  }).join('')+'</div>':empty('Không có địa điểm phù hợp.')}
  <section class="home-section trust-section"><div class="section-head"><h2>Vì sao thuê địa điểm qua VÉNUE?</h2></div>
    ${venueReasons([
      ['calendar','Kiểm tra lịch trước khi gửi','Xem khung giờ trống và đối chiếu sức chứa của từng địa điểm với số người tham dự.'],
      ['building','Một hồ sơ cho nhiều nơi','Khai thông tin sự kiện một lần. Chọn ngày, giờ và số khách riêng cho từng địa điểm.'],
      ['wallet','Theo dõi tiền cọc từng nơi','Xem giá và tiền cọc trước khi gửi. Theo dõi kết quả duyệt, hạn cọc và chứng từ trong tài khoản.']
    ])}</section>
  <section class="home-section home-faq"><h2>Câu hỏi khi thuê</h2>${commonQuestions([
    ['Tôi thuê nhiều địa điểm cùng lúc như thế nào?','Bấm Chọn địa điểm trên từng thẻ rồi Gửi yêu cầu thuê. Sau khi gửi, các địa điểm cùng mã hồ sơ được theo dõi và xử lý riêng.'],
    faq()[2],
    ['Tôi cần chuẩn bị thông tin gì?','Tên và loại sự kiện, mục đích, kịch bản, thiết bị, lưu ý, thông tin người phụ trách và lịch dự kiến cho từng nơi.']
  ])}</section>`;
}

function supportContactLinks(kind='') {
  const {supportPhone,supportEmail}=db.settings;
  const contacts=[];
  if (supportPhone) contacts.push(`<a class="${kind}" href="tel:${esc(supportPhone.replace(/[^\d+]/g,''))}">${ic('phone',16)} ${esc(supportPhone)}</a>`);
  if (supportEmail) contacts.push(`<a class="${kind}" href="mailto:${esc(supportEmail)}">${esc(supportEmail)}</a>`);
  return contacts;
}

function helpDialog() {
  const contacts=supportContactLinks('btn secondary');
  openDialog('Đội hỗ trợ VÉNUE',`<div class="stack"><p>Hỏi về vé, thanh toán hoặc hồ sơ thuê địa điểm.</p>
    <div class="cluster">${button('Nhắn đội hỗ trợ','support')}
    ${contacts.join('')}</div>
    ${!contacts.length?notice('Kênh đang mở: nhắn đội hỗ trợ trong VÉNUE. Hotline và email chưa được cấu hình.'):''}</div>`);
}

