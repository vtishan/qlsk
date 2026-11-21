const rentalEventTypes=['Hội thảo','Tiệc','Triển lãm','Workshop','Giải trí','Khác'];

function rentalForm(id) {
  const user=account();
  const venueIds=[...new Set([...rentalSelection,...(id?[id]:[])])];
  const venues=venueIds.map(venueId=>lookup(db.venues,venueId));
  assert(user&&venues.length&&venues.every(v=>v?.state==='ACTIVE'),'Hãy chọn địa điểm còn hoạt động.');
  if (needsVerified(user)) {
    ui.verifiedAction={action:'rental',id};
    contactForm();
    return;
  }
  const start=parseLocal(dayKey(Date.now()))+15*day+9*hour;
  const eventOptions=user.role==='organizer'?db.events.filter(e=>e.owner===user.id).map(e=>opt(e.id,eventTitle(e.id))).join(''):'';
  const places=venues.map(v=>`<fieldset class="request-fields"><legend>${esc(v.name)}</legend>
    <p class="muted small">${esc(v.city)} · Tối đa ${esc(v.capacity)} khách · ${money(v.rate)} / khung</p>
    <div class="form-grid slot-fields">
      ${field('Bắt đầu',input('start-'+v.id,localInput(start),'datetime-local','required'))}
      ${field('Kết thúc',input('end-'+v.id,localInput(start+4*hour),'datetime-local','required'))}
      ${field('Số người dự kiến',input('capacity-'+v.id,'','number','required min="1" step="1" max="'+v.capacity+'"'))}
    </div></fieldset>`).join('');
  openDialog(venues.length===1?'Yêu cầu thuê địa điểm':'Yêu cầu thuê '+venues.length+' địa điểm',`
    ${notice('Tạm tính '+money(venues.reduce((sum,v)=>sum+v.rate,0))+' · Cọc '+db.policy.depositPercent+'% sau khi từng địa điểm được duyệt.')}
    <fieldset class="request-fields"><legend>Thông tin sự kiện</legend><div class="form-grid">
      ${field('Tên sự kiện',input('eventName','','text',required))}
      ${field('Loại hình sự kiện',select('eventType',opt('','Chọn loại hình')+choices(rentalEventTypes),'required'))}
      ${eventOptions?field('Liên kết sự kiện của Ban tổ chức',select('eventId',opt('','Thuê độc lập')+eventOptions),true):''}
      ${field('Mục đích tổ chức',area('reason','','required maxlength="500"'),true)}
      ${field('Kịch bản và hoạt động chính',area('program','','required maxlength="1500"'),true)}
      ${field('Thiết bị cần thuê thêm',area('equipment','','required maxlength="700" placeholder="Ví dụ: 2 micro, máy chiếu. Ghi Không có nếu không cần."'))}
      ${field('Lưu ý đặc biệt',area('specialNotes','','required maxlength="700" placeholder="Ví dụ: Ăn uống, an ninh, công suất điện. Ghi Không có nếu không cần."'))}
      ${field('Họ tên người phụ trách',input('representative',user.name,'text',required+' autocomplete="name"'))}
      ${field('Số điện thoại trực tiếp',input('phone',user.phone,'tel','required maxlength="25" autocomplete="tel"'))}
    </div></fieldset>
    <div class="stack"><div><h3>Lịch thuê</h3><p class="muted small">Ngày giờ theo giờ Việt Nam. Điền một khung giờ và số khách cho từng địa điểm.</p></div>${places}</div>`,form=>{
      const current=account();
      assert(current?.id===user.id&&!needsVerified(current),'Cần đăng nhập và xác minh tài khoản.');
      const detail=Object.fromEntries(['eventName','eventType','reason','program','equipment','specialNotes','representative','phone'].map(key=>[key,textValue(form,key)]));
      assert(Object.values(detail).every(Boolean)&&rentalEventTypes.includes(detail.eventType),'Điền đầy đủ thông tin sự kiện và chọn loại hình.');
      const digits=detail.phone.replace(/\D/g,'');
      assert(/^\+?[\d .()-]+$/.test(detail.phone)&&digits.length>=9&&digits.length<=15,'Số điện thoại liên hệ không hợp lệ.');
      const eventId=textValue(form,'eventId');
      assert(!eventId||ownerEvent(lookup(db.events,eventId)),'Sự kiện không thuộc tài khoản.');
      const p=db.policy;
      const now=Date.now();
      const batchId=uid('HS-').slice(0,15);
      const records=venueIds.map(venueId=>{
        const place=lookup(db.venues,venueId);
        assert(place?.state==='ACTIVE','Một địa điểm không còn hoạt động. Hãy mở lại danh sách.');
        const from=textValue(form,'start-'+venueId), to=textValue(form,'end-'+venueId);
        const start=parseLocal(from),end=parseLocal(to);
        assert(Number.isFinite(start)&&Number.isFinite(end)&&localInput(start)===from&&localInput(end)===to&&start>now&&end>start,
          place.name+' · Chọn khung giờ trong tương lai, giờ kết thúc phải sau giờ bắt đầu.');
        assert(!rentalConflict(place,start,end),place.name+' · Khung giờ đã có lịch sử dụng.');
        const capacity=Number(textValue(form,'capacity-'+venueId));
        assert(Number.isInteger(capacity)&&capacity>0&&capacity<=place.capacity,place.name+' · Số khách phải từ 1 đến '+place.capacity+'.');
        return {id:uid('RT-').slice(0,15),batchId,venueId,user:user.id,eventId,start,end,capacity,created:now,
          ...detail,contact:detail.phone,price:place.rate,deposit:Math.round(place.rate*p.depositPercent/100),depositHours:p.depositHours,
          earlyDays:p.rentalEarlyDays,earlyPercent:p.rentalEarlyPercent,midDays:p.rentalMidDays,midPercent:p.rentalMidPercent,state:'REQUESTED'};
      });
      db.rentals.unshift(...records);
      for (const venueId of venueIds) rentalSelection.delete(venueId);
      message(user.id,'Đã gửi hồ sơ thuê '+batchId,records.length+' địa điểm · '+detail.eventName);
      audit('Gửi hồ sơ thuê địa điểm',batchId,records.map(r=>r.id).join(', '));
      go('account/rentals');
    },'Gửi yêu cầu');
  modal.querySelector('.dialog-inner').classList.add('request-dialog');
}

function rentalDetails(id) {
  const rent=own(id,db.rentals,canManage('rentals'));
  const related=rent.batchId?db.rentals.filter(r=>r.batchId===rent.batchId&&r.user===rent.user):[];
  openDialog(rent.eventName||'Yêu cầu thuê '+id,`<div class="stack">
    ${row(venueName(rent.venueId),rent.id+(rent.batchId?' · Hồ sơ '+rent.batchId:''),tag(rent.state))}
    ${row('Thời gian',fmt(rent.start)+' – '+fmt(rent.end))}${row('Số người dự kiến',rent.capacity+' khách')}
    ${row('Chi phí',money(rent.price)+' · Cọc '+money(rent.deposit))}
    ${rent.eventType?row('Loại hình',rent.eventType):''}${row('Mục đích',rent.reason)}
    ${rent.program?row('Kịch bản',rent.program):''}${rent.equipment?row('Thiết bị thuê thêm',rent.equipment):''}
    ${rent.specialNotes?row('Lưu ý đặc biệt',rent.specialNotes):''}
    ${row('Người phụ trách',rent.representative?rent.representative+' · '+rent.phone:rent.contact)}
    ${rent.deadline?row('Hạn thanh toán cọc',fmt(rent.deadline)):''}${rent.proof?row('Chứng từ cọc',rent.proof):''}
    ${rent.cancelAmount!=null?row('Hoàn cọc',money(rent.cancelAmount)+' · '+rent.cancelPercent+'%'):''}
    ${rent.note?row('Phản hồi',rent.note):''}
    ${related.length>1?panel('Địa điểm cùng hồ sơ',related.filter(item=>item.id!==id).map(item=>
      row(venueName(item.venueId),fmt(item.start),tag(item.state)+button('Chi tiết','rentalDetail',item.id,'secondary compact'))).join('')):''}
  </div>`);
}

function customerRentals(user) {
  const groups=new Map();
  for (const rent of db.rentals.filter(r=>r.user===user.id)) {
    const key=rent.batchId||rent.id;
    if (!groups.has(key)) groups.set(key,[]);
    groups.get(key).push(rent);
  }
  return `<div class="stack">${[...groups].map(([id,rentals])=>panel(rentals[0].eventName||'Yêu cầu thuê',
    `<p class="small muted rental-batch-meta">${esc(id)} · ${rentals.length} địa điểm</p>`+rentals.map(r=>row(venueName(r.venueId),
    fmt(r.start)+' · Cọc '+money(r.deposit)+(r.state==='AWAITING_DEPOSIT'?' · Hạn '+fmt(r.deadline):''),
    tag(r.state)+button('Chi tiết','rentalDetail',r.id,'secondary compact')+
    (r.state==='AWAITING_DEPOSIT'?button('Thanh toán cọc','payDeposit',r.id,'compact'):'')+
    (['REQUESTED','AWAITING_DEPOSIT','CONFIRMED'].includes(r.state)&&r.start>Date.now()?button('Yêu cầu hủy','rentalCancel',r.id,'ghost compact'):''))).join(''))).join('')||empty('Chưa có yêu cầu thuê địa điểm.')}
    <div>${link('Tìm địa điểm','venues','btn secondary')}</div></div>`;
}

const refundReasons=['Thay đổi kế hoạch','Không thể tham dự','Đặt nhầm suất hoặc ghế','Sự kiện thay đổi lịch','Khác'];

function refundOrderIds(refund) {
  return refund.orderAllocations?.map(item=>item.orderId)||(refund.orderId?[refund.orderId]:[]);
}
function refundForOrder(refund,orderId) {
  return refund.orderAllocations?.find(item=>item.orderId===orderId)?.amount ?? (refund.orderId===orderId?refund.amount:0);
}
function reservedRefundAmount(orderId) {
  return db.refunds.filter(r=>r.state!=='REJECTED').reduce((total,r)=>total+refundForOrder(r,orderId),0);
}
function refundAllocations(tickets) {
  const allocations=new Map();
  for (const ticket of tickets) {
    const order=lookup(db.orders,ticket.orderId);
    const entry=allocations.get(order.id)||{orderId:order.id,amount:0,ticketIds:[]};
    const available=Math.max(0,order.total-reservedRefundAmount(order.id)-entry.amount);
    entry.amount+=Math.min(available,amountForTicket(order,ticket,refundable(ticket)==='postponed'));
    entry.ticketIds.push(ticket.id);
    allocations.set(order.id,entry);
  }
  return [...allocations.values()];
}

function refreshRefundDialog(form) {
  const choice=form.elements.reason.value;
  const feedback=form.elements.feedback;
  feedback.required=choice==='Khác';
  feedback.closest('label').hidden=!feedback.required;
  const ids=new Set(new FormData(form).getAll('tickets'));
  const tickets=db.tickets.filter(ticket=>ids.has(ticket.id)&&ticket.user===account()?.id&&refundable(ticket));
  const amount=refundAllocations(tickets).reduce((sum,item)=>sum+item.amount,0);
  form.querySelector('[data-refund-total]').textContent=tickets.length+' vé · Tạm tính '+money(amount);
}

function refundForm(id='') {
  const user=account();
  assert(user,'Đăng nhập để yêu cầu hoàn tiền.');
  if (id) assert(refundable(own(id,db.tickets)),'Vé không đủ điều kiện hoàn.');
  const tickets=db.tickets.filter(t=>t.user===user.id&&refundable(t));
  assert(tickets.length,'Bạn chưa có vé đủ điều kiện hoàn tiền.');
  openDialog('Yêu cầu hoàn vé',`<p class="muted">Chọn các vé muốn hoàn trong cùng một yêu cầu. Số tiền tạm tính theo chính sách từng đơn.</p>
    <fieldset class="request-fields"><legend>Vé đủ điều kiện</legend>
    <label class="check refund-all"><input type="checkbox" data-change="refund-all"> Chọn tất cả</label>
    <div class="refund-options">${tickets.map(t=>{
      const order=lookup(db.orders,t.orderId);
      return `<label class="refund-option"><input type="checkbox" name="tickets" value="${esc(t.id)}"${t.id===id?' checked':''}>
      <span><strong>${esc(eventTitle(t.eventId))}</strong><small>Ghế ${esc(t.seat)} · ${esc(t.type)} · ${esc(order.id)} · ${esc(t.id)}</small></span></label>`;
    }).join('')}</div></fieldset>
    <p class="notice" role="status" data-refund-total></p>
    ${field('Lý do hoàn',select('reason',opt('','Chọn lý do')+choices(refundReasons),'required'))}
    ${field('Mô tả lý do khác',area('feedback','','maxlength="500"'))}`,form=>{
      assert(account()?.id===user.id,'Phiên đăng nhập đã thay đổi.');
      const ids=[...new Set(form.getAll('tickets'))];
      assert(ids.length,'Hãy chọn ít nhất một vé.');
      const reason=textValue(form,'reason'), feedback=textValue(form,'feedback');
      assert(refundReasons.includes(reason)&&(reason!=='Khác'||feedback&&feedback.length<=500),'Chọn lý do hoàn và nhập nội dung nếu chọn Khác.');
      const selected=ids.map(ticketId=>own(ticketId,db.tickets));
      assert(selected.every(t=>refundable(t)),'Điều kiện hoàn của một hoặc nhiều vé đã thay đổi. Hãy chọn lại.');
      const orderAllocations=refundAllocations(selected);
      const amount=orderAllocations.reduce((sum,item)=>sum+item.amount,0);
      const eventIds=[...new Set(selected.map(t=>t.eventId))];
      queueRefund({user:user.id,eventId:eventIds.length===1?eventIds[0]:'',orderId:orderAllocations.length===1?orderAllocations[0].orderId:'',
        orderAllocations,ticketIds:ids,reason:reason==='Khác'?'Khác · '+feedback:reason,amount,state:'AUTO_APPROVED'});
      for (const ticket of selected) ticket.state='CANCELLED';
      for (const {orderId} of orderAllocations)
        if (!db.tickets.some(t=>t.orderId===orderId&&validTicket(t))) lookup(db.orders,orderId).state='REFUND_PENDING';
      go('account/refunds');
    },'Gửi yêu cầu');
  const form=modal.querySelector('form');
  refreshRefundDialog(form);
  form.addEventListener('change',event=>{
    const all=form.querySelector('[data-change="refund-all"]');
    const options=[...form.querySelectorAll('[name="tickets"]')];
    if (event.target===all) options.forEach(el=>{el.checked=all.checked;});
    const checked=options.filter(el=>el.checked).length;
    all.checked=checked===options.length;
    all.indeterminate=checked>0&&checked<options.length;
    refreshRefundDialog(form);
  });
}

