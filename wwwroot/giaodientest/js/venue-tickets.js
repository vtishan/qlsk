function reviewProblem(ticket,now=Date.now()) {
  if (ticket.state!=='CHECKED_IN') return 'Chỉ khách đã check-in mới có thể đánh giá.';
  const event=lookup(db.events,ticket.eventId);
  if (event?.state==='CANCELLED') return 'Sự kiện đã hủy.';
  const start=sessionOf(event,ticket.sessionId)?.start;
  if (!Number.isFinite(start)||start>now) return 'Bạn có thể đánh giá từ khi suất diễn bắt đầu.';
  if (db.reviews.some(r=>r.user===ticket.user&&r.eventId===ticket.eventId)) return 'Bạn đã đánh giá sự kiện này.';
  return '';
}

function eventReviews(event) {
  const user=account();
  const now=Date.now();
  const ticket=user&&db.tickets.find(t=>t.user===user.id&&t.eventId===event.id&&!reviewProblem(t,now));
  const scores=db.reviews.filter(r=>r.eventId===event.id&&r.visible!==false);
  const upcoming=event.state!=='CANCELLED'&&event.live.sessions.length&&event.live.sessions.every(s=>s.start>now);
  const reviews=scores.length?scores.map(r=>row('★ '+r.score+'/5 · '+(lookup(db.accounts,r.user)?.name||'Khách tham dự'),r.text)).join(''):
    empty(upcoming?'Khách đã check-in có thể đánh giá khi suất diễn bắt đầu.':'Chưa có đánh giá.');
  return panel('Đánh giá từ khách đã tham dự',`<div class="stack">${ticket?'<div>'+button('Viết đánh giá','review',ticket.id,'secondary compact')+'</div>':''}${reviews}</div>`);
}

function customerTickets(user) {
  const tickets=db.tickets.filter(t=>t.user===user.id);
  const groups=Object.groupBy(tickets,t=>t.state==='ISSUED'?'current':'history');
  const refund=tickets.some(refundable)?button('Hoàn nhiều vé','refund','','secondary compact'):'';
  function ticketCard(t) {
    const event=lookup(db.events,t.eventId);
    const ses=sessionOf(event,t.sessionId);
    return `<article class="customer-ticket">
      <div class="ticket-card-image"><img src="${imageFor(event?.live?.imageKey,event?.live?.cover)}" alt="" loading="lazy"></div>
      <div class="ticket-card-body"><div class="split"><h3>${esc(eventTitle(t.eventId))}</h3>${tag(t.state)}</div>
        <p class="event-meta">${ic('calendar',15)} ${esc(fmt(ses?.start))} · ${esc(venueName(ses?.venueId))}</p>
        <p class="small">${esc(t.type)} · ${esc(t.zone)} · Ghế <strong>${esc(t.seat)}</strong></p>
        <code>${esc(t.id)}</code>
        <div class="cluster">
          ${button(validTicket(t)?'Xem vé & QR':'Chi tiết','ticket',t.id,'compact')}
          ${validTicket(t)?button('Tải PDF','downloadTicket',t.id,'secondary compact')+button('In vé','printTicket',t.id,'secondary compact'):''}
          ${refundable(t)?button('Yêu cầu hoàn','refund',t.id,'ghost compact'):''}
          ${!reviewProblem(t)?button('Đánh giá','review',t.id,'ghost compact'):''}
          ${t.state==='CHECKED_IN'?button('Khiếu nại','support',t.id,'ghost compact'):''}
      </div></div></article>`;
  }
  if (!tickets.length) return empty('Bạn chưa có vé.')+'<div class="results">'+link('Khám phá sự kiện','events','btn')+'</div>';
  return `<div class="stack">
    <section aria-labelledby="current-tickets"><div class="split ticket-group-head"><h2 id="current-tickets">Vé hiện tại</h2>${refund}</div>
    ${groups.current?.length?'<div class="customer-tickets">'+groups.current.map(ticketCard).join('')+'</div>':empty('Bạn chưa có vé đang hiệu lực.')}</section>
    ${groups.history?.length?`<details class="ticket-history"${groups.current?.length?'':' open'}><summary>Lịch sử vé <span>${groups.history.length}</span></summary>
      <div class="customer-tickets">${groups.history.map(ticketCard).join('')}</div></details>`:''}
  </div>`;
}

function bookPath(eventId,sessionId) {
  return 'book/'+encodeURIComponent(eventId)+'/'+encodeURIComponent(sessionId);
}

function bookableSessions(event) {
  return event.live.sessions.filter(session=>sellable(event,session)&&sessionFree(session)>0);
}

function quickBooking(id) {
  const event=lookup(db.events,id);
  assert(publicEvent(event),'Sự kiện không còn khả dụng.');
  const sessions=bookableSessions(event);
  if (sessions.length===1) {
    go(bookPath(id,sessions[0].id));
    return;
  }
  const options=sessions.length?sessions.map(s=>row(s.name,fmt(s.start)+' · '+venueName(s.venueId),
    link('Chọn ghế',bookPath(id,s.id),'btn compact'))).join(''):
    empty('Hiện chưa có suất mở bán còn ghế.');
  openDialog('Chọn suất để đặt vé',options);
}

function chosenSeats(ses) {
  return ses.zones.flatMap(zone=>zone.seats.filter(s=>selection.seats.includes(s.code)).map(seat=>({...seat,zone:zone.name})));
}

function chooseView(eventId,sessionId) {
  const event=lookup(db.events,eventId);
  const ses=sessionOf(event,sessionId);
  if (!publicEvent(event)||!sellable(event,ses)) return pageTitle('Suất diễn đã đóng bán')+link('Khám phá sự kiện','events','btn');
  if (!account()) return loginGate('Đăng nhập để chọn và giữ ghế cho tài khoản của bạn.');
  if (needsVerified(account())) return pageTitle('Xác minh liên hệ','Xác minh liên hệ để tiếp tục giữ ghế.')+button('Xác minh','verifyContact');
  if (selection.sessionId!==ses.id) selection={eventId,sessionId,zone:ses.zones[0]?.name,row:'',seats:[]};
  const zone=ses.zones.find(z=>z.name===selection.zone)||ses.zones[0];
  if (!zone) return pageTitle('Chọn ghế')+empty('Suất diễn chưa có sơ đồ ghế.');
  const rows=new Map();
  for (const seat of zone.seats) {
    const label=seat.code.match(/^[^\d]+/)?.[0]||'Ghế';
    if (!rows.has(label)) rows.set(label,[]);
    rows.get(label).push(seat);
  }
  const used=inventory(ses.id);
  const blocked=seat=>seat.blocked||used.has(seat.code)||!saleType(ses,seat.type);
  const blockedGroups=new Set(zone.seats.filter(seat=>seat.group&&blocked(seat)).map(seat=>seat.group));
  const selected=chosenSeats(ses);
  const price=selected.reduce((total,seat)=>total+(saleType(ses,seat.type)?.price||0),0);
  const limit=Math.max(0,db.policy.maxTickets-quantityUsed(account().id,ses.id));
  const held=holdButton(selected.length);
  function seatButton(s) {
    const occupy=used.get(s.code);
    const chosen=selection.seats.includes(s.code);
    const ticketType=saleType(ses,s.type);
    const description=occupy?{hold:'Đang giữ',sold:'Đã bán',invite:'Vé mời'}[occupy.state]:s.blocked?'Bị khóa':
      !ticketType?'Đóng bán':blockedGroups.has(s.group)?'Nhóm có ghế không khả dụng':money(ticketType.price);
    const label=`Ghế ${s.code} · ${s.type} · ${description}${s.group?' · Nhóm '+s.group:''}`;
    return `<button type="button" data-action="seat" data-id="${esc(s.code)}" class="seat ${chosen?'selected':''} ${occupy?.state||''}${s.group?' grouped':''}"
      aria-pressed="${chosen}" title="${esc(label)}" aria-label="${esc(label)}"${blocked(s)||blockedGroups.has(s.group)?' disabled':''}>${esc(s.code)}</button>`;
  }
  return `${link(event.live.title,'event/'+eventId,'back')}${pageTitle('Chọn ghế',ses.name+' · '+fmt(ses.start)+' · '+venueName(ses.venueId))}
    <ol class="stepbar"><li class="active" aria-current="step">Chọn ghế</li><li>Thanh toán</li><li>Nhận vé</li></ol>
    <div class="seat-layout">
      <div class="panel seat-panel">
        <div class="seat-toolbar">
          <div class="cluster" aria-label="Khu vực ghế">${ses.zones.map(z=>button(z.name,'zone',z.name,z===zone?'compact':'secondary compact',' aria-pressed="'+(z===zone)+'"')).join('')}</div>
          ${rows.size>1?select('seatRow',opt('','Tất cả hàng')+[...rows.keys()].map(r=>opt(r,r==='Ghế'?r:'Hàng '+r,selection.row)).join(''),'aria-label="Lọc hàng ghế" data-change="seatRow"'):''}
        </div>
        <div class="seat-legend"><span><i></i>Còn trống</span><span><i class="legend-selected"></i>Đang chọn</span><span><i class="legend-hold"></i>Đang giữ</span>
          <span><i class="legend-invite"></i>Vé mời</span><span><i class="legend-closed"></i>Không khả dụng</span></div>
        <div class="seat-screen">SÂN KHẤU</div>
        <div class="seat-scroll" tabindex="0" role="region" aria-label="Sơ đồ ghế khu ${esc(zone.name)}">
          <div class="seat-map-rows">${[...rows].filter(([r])=>!selection.row||r===selection.row).map(([name,seats])=>
            `<div class="seat-map-row"><span class="row-name" aria-label="Hàng ${esc(name)}">${esc(name)}</span><div class="seat-line">${seats.map(seatButton).join('')}</div></div>`).join('')}
          </div>
        </div>
        <p class="small muted">Ghế có chấm thuộc ghế đôi hoặc cụm và được chọn cùng nhóm.</p>
      </div>
      <section class="panel seat-summary">
        <div class="split"><h2>Ghế đã chọn</h2><span class="badge info" role="status">${selected.length} / ${limit} vé</span></div>
        ${selected.length?`<ul class="picked-seats">${selected.map(s=>`<li><strong>${esc(s.code)}</strong> <span>${esc(s.zone)} · ${money(saleType(ses,s.type)?.price)}</span>
          ${button('×','removeSeat',s.code,'ghost compact remove-seat',' aria-label="Bỏ chọn ghế '+esc(s.code)+'"')}</li>`).join('')}</ul>
          ${button('Bỏ chọn tất cả','clearSeats','','ghost compact')}`:'<p class="muted">Chọn vị trí trên sơ đồ ghế.</p>'}
        <div class="split seat-total"><span>Tạm tính</span><strong>${money(price)}</strong></div>
        <p class="small muted">Giữ ghế ${db.policy.holdMinutes} phút sau khi xác nhận.</p>
        ${held}
      </section>
    </div>
    <div class="seat-mobile-action"><div><strong>${money(price)}</strong><small>${selected.length} vé đã chọn</small></div>${held}</div>`;
}
function holdButton(count) {
  return button('Giữ ghế và thanh toán','hold','','full',!count?' disabled':'');
}

