const calendarMonthLabel = new Intl.DateTimeFormat('vi-VN',{month:'long',year:'numeric',timeZone:'Asia/Ho_Chi_Minh'});
const calendarDayLabel = new Intl.DateTimeFormat('vi-VN',{dateStyle:'full',timeZone:'Asia/Ho_Chi_Minh'});
const calendarTimeLabel = new Intl.DateTimeFormat('vi-VN',{timeStyle:'short',timeZone:'Asia/Ho_Chi_Minh'});
const calendarWeekdays = ['T2','T3','T4','T5','T6','T7','CN'];
let calendarFilters = {};

function eventPath(eventId,sessionId) {
  return 'event/'+encodeURIComponent(eventId)+(sessionId?'/'+encodeURIComponent(sessionId):'');
}

function eventUrl(eventId,sessionId) {
  const url=new URL(location.href);
  url.hash=eventPath(eventId,sessionId);
  return url.href;
}

function shareEvent(id) {
  assert(publicEvent(lookup(db.events,id)),'Sự kiện không còn khả dụng.');
  const url=eventUrl(id);
  openDialog('Chia sẻ sự kiện',field('Liên kết sự kiện',input('eventUrl',url,'url','readonly autofocus')),
    async (form,element)=>{
      try {await navigator.clipboard.writeText(url);}
      catch {
        element.elements.eventUrl.select();
        throw new Error('Hãy dùng lệnh sao chép của thiết bị để sao chép liên kết đã chọn.');
      }
      notifyUi('Đã sao chép liên kết sự kiện.');
    },'Sao chép liên kết');
}

function shiftCalendarMonth(month,offset) {
  const date=new Date(month+'-01T00:00:00Z');
  date.setUTCMonth(date.getUTCMonth()+offset);
  return date.toISOString().slice(0,7);
}

function calendarSessionState(event,session,now) {
  if (event.state!=='ACTIVE') return tag(event.state);
  if (session.end<=now) return '<span class="badge">Đã qua</span>';
  if (session.start<=now) return '<span class="badge green">Đang diễn ra</span>';
  return '<span class="badge">Sắp diễn ra</span>';
}

function calendarView(requestedMonth,requestedDay) {
  const now=Date.now();
  const today=dayKey(now);
  const month=/^[1-9]\d{3}-(0[1-9]|1[0-2])$/.test(requestedMonth)?requestedMonth:today.slice(0,7);
  const first=parseLocal(month+'-01');
  const daysInMonth=new Date(Date.UTC(Number(month.slice(0,4)),Number(month.slice(5)),0)).getUTCDate();
  const days=Array.from({length:daysInMonth},(_,i)=>dayKey(first+i*day));
  const selected=days.includes(requestedDay)?requestedDay:'';
  const end=first+daysInMonth*day;
  const startOffset=(new Date(month+'-01T00:00:00Z').getUTCDay()+6)%7;
  const {city,cat}=calendarFilters;
  const schedule=filteredEvents({city,cat}).flatMap(({event,sessions})=>sessions
    .filter(s=>s.start<end&&s.end>first)
    .map(session=>({event,session}))).sort((a,b)=>a.session.start-b.session.start);
  const daySchedule=new Map(days.map((date,i)=>{
    const start=first+i*day;
    return [date,schedule.filter(({session})=>session.start<start+day&&session.end>start)];
  }));
  const shown=selected?daySchedule.get(selected):schedule;
  const heading=selected?calendarDayLabel.format(parseLocal(selected)):'Lịch trong tháng';
  const monthPath='calendar/'+month;
  const monthLabel=calendarMonthLabel.format(first);
  return `${pageTitle('Lịch sự kiện','Chọn ngày để xem từng suất theo giờ Việt Nam.',link('Xem danh sách sự kiện','events','btn secondary compact'))}
    <form class="panel calendar-filters" data-form="calendarFilters">
      ${field('Thành phố',select('city',opt('','Mọi thành phố')+choices(venueCities(),city)))}
      ${field('Danh mục',select('cat',opt('','Mọi danh mục')+choices(db.categories,cat)))}
      <button class="btn" type="submit">Áp dụng bộ lọc</button>
    </form>
    <div class="calendar-layout">
      <section class="panel calendar-month" aria-labelledby="calendar-month-title">
        <div class="split calendar-month-heading"><h2 id="calendar-month-title" tabindex="-1">${esc(monthLabel)}</h2>
          <a class="btn ghost compact" href="#calendar" data-calendar-nav="today">Tháng này</a>
        </div>
        <nav class="split calendar-month-nav" aria-label="Chuyển tháng">
          <a class="btn secondary compact" href="#calendar/${shiftCalendarMonth(month,-1)}" data-calendar-nav="previous">Tháng trước</a>
          <a class="btn secondary compact" href="#calendar/${shiftCalendarMonth(month,1)}" data-calendar-nav="next">Tháng sau</a>
        </nav>
        <div class="calendar-grid" aria-label="Các ngày trong tháng">
          ${calendarWeekdays.map(label=>`<span class="calendar-weekday" aria-hidden="true">${label}</span>`).join('')}
          ${'<span aria-hidden="true"></span>'.repeat(startOffset)}
          ${days.map((date,i)=>{
            const count=daySchedule.get(date).length;
            const label=calendarDayLabel.format(first+i*day)+', '+count+' suất'+(date===selected?', đang chọn':'');
            return `<a class="calendar-day${date===selected?' is-selected':''}" href="#${monthPath}/${date}" data-calendar-nav="day" aria-controls="calendar-agenda"
              aria-label="${esc(label)}"${date===today?' aria-current="date"':''}><span>${i+1}</span>${count?'<small aria-hidden="true">'+count+'</small>':''}</a>`;
          }).join('')}
        </div>
      </section>
      <section class="panel calendar-agenda" id="calendar-agenda" aria-labelledby="calendar-agenda-title">
        <div class="split"><h2 id="calendar-agenda-title" tabindex="-1">${esc(heading)}</h2>
          ${selected?`<a class="btn ghost compact" href="#${monthPath}" data-calendar-nav="reset">Xem cả tháng</a>`:''}
        </div>
        <p class="muted small" role="status">${shown.length} suất</p>
        ${shown.length?`<ol class="calendar-agenda-list">${shown.map(({event,session})=>{
          const sameDay=dayKey(session.start)===dayKey(session.end);
          const time=sameDay?fmt(session.start)+' – '+calendarTimeLabel.format(session.end):fmt(session.start)+' – '+fmt(session.end);
          return `<li><div class="calendar-session-status">${calendarSessionState(event,session,now)}</div>
            <h3>${link(event.live.title,eventPath(event.id,session.id))}</h3>
            <p>${esc(session.name)} · ${esc(venueName(session.venueId))}</p><p>${esc(time)}</p></li>`;
        }).join('')}</ol>`:empty(selected?'Không có suất nào phù hợp trong ngày này.':'Không có suất nào phù hợp trong tháng này.')}
      </section>
    </div>`;
}

function canSaveSession(event,session) {
  return publicEvent(event)&&event.state==='ACTIVE'&&session?.end>Date.now();
}

function exportSessionCalendar(sessionId,element) {
  const event=lookup(db.events,element.event);
  const session=sessionOf(event,sessionId);
  assert(canSaveSession(event,session),'Suất này không còn khả dụng để thêm vào lịch.');
  const text=value=>String(value).replaceAll('\\','\\\\').replace(/\r\n|\r|\n/g,'\\n').replaceAll(';','\\;').replaceAll(',','\\,');
  const stamp=value=>new Date(value).toISOString().replace(/[-:]/g,'').replace(/\.\d{3}Z$/,'Z');
  const url=eventUrl(event.id,session.id);
  const venue=lookup(db.venues,session.venueId);
  const place=venue?.state==='ACTIVE'?venueLocation(venue):venueName(session.venueId);
  const lines=[
    'BEGIN:VCALENDAR','VERSION:2.0','PRODID:-//VENUE//Lich su kien//VI','BEGIN:VEVENT',
    'UID:'+encodeURIComponent(event.id)+'/'+encodeURIComponent(session.id)+'@'+(location.host||'venue.local'),
    'DTSTAMP:'+stamp(Date.now()),'DTSTART:'+stamp(session.start),'DTEND:'+stamp(session.end),
    'SUMMARY:'+text(event.live.title+' – '+session.name),'LOCATION:'+text(place),
    'DESCRIPTION:'+text(event.live.description+'\n'+url),'URL:'+url,'END:VEVENT','END:VCALENDAR'
  ];
  const encoder=new TextEncoder();
  const folded=lines.map(line=>{
    let output='',bytes=0;
    for (const char of line) {
      const length=encoder.encode(char).length;
      if (bytes+length>75) {output+='\r\n ';bytes=1;}
      output+=char;
      bytes+=length;
    }
    return output;
  });
  download('VENUE-'+encodeURIComponent(session.id)+'.ics',folded.join('\r\n')+'\r\n','text/calendar;charset=utf-8');
}
