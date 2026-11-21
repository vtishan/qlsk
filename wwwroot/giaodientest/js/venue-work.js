const workGroups={
  organizer:[
    ['Sự kiện',['events','orders','promos','guests','grants']],
    ['Vận hành tại cổng',['scan','manual','attendees','logs']],
    ['Tài chính và hỗ trợ',['finance','settlements','support']]
  ],
  staff:[
    ['Tại cổng',['scan','manual','attendees','guests']],
    ['Theo dõi',['orders','reports','support','content','logs']]
  ],
  manager:[
    ['Xét duyệt',['applications','reviews','rentals']],
    ['Tài chính',['payments','refunds','settlements']],
    ['Hỗ trợ và giám sát',['support','monitoring']]
  ],
  admin:[
    ['Nền tảng',['accounts','venues','catalog','promos']],
    ['Xét duyệt',['applications','reviews','rentals','support']],
    ['Tài chính',['contracts','payments','refunds','settlements','payouts']],
    ['Hệ thống',['policy','config','monitoring','audit']]
  ]
};
const workSearchTabs=new Set([
  'events','orders','attendees','guests','grants','promos','finance','settlements','support','logs','reports',
  'accounts','venues','applications','reviews','rentals','payments','refunds','payouts','contracts','audit'
]);
let workFilterScope='';
let workFilter={query:'',status:''};

function workView(active='overview') {
  const user=account();
  if (!user) return loginGate('Đăng nhập bằng tài khoản được phân quyền để mở khu vực vận hành.');
  const menu=workspaceMenus[user.role]?.filter(([tab])=>{
    if (user.role==='manager') return tab==='overview'||canManage(tab);
    if (user.role==='staff') return !staffTabs[tab]||db.grants.some(g=>g.user===user.id&&g.active&&g.permissions.includes(staffTabs[tab]));
    return true;
  });
  if (!menu) return pageTitle('Khu vực vận hành')+notice('Tài khoản chưa có quyền vận hành. Có thể gửi hồ sơ Ban tổ chức từ tài khoản.')+link('Hồ sơ Ban tổ chức','account/application','btn secondary');
  const current=menu.find(([tab])=>tab===active);
  const scope=[user.id,active,route[2],editorId,venueEditorId,staffScope.eventId,staffScope.sessionId].join('/');
  if (scope!==workFilterScope) {workFilterScope=scope;workFilter={query:'',status:''};}
  function item([tab,icon,name]) {
    return `<a href="#work/${tab}" class="side-link"${tab===active?' aria-current="page"':''}>${ic(icon,17)} <span>${esc(name)}</span></a>`;
  }
  const groups=workGroups[user.role].map(([label,tabs])=>[label,tabs.map(tab=>menu.find(entry=>entry[0]===tab)).filter(Boolean)]).filter(([,links])=>links.length);
  const overview=item(menu[0]);
  const actions=active==='overview'&&user.role==='organizer'?button('Tạo sự kiện','newEvent'):
    active==='overview'&&user.role==='staff'&&menu.some(([tab])=>tab==='scan')?link('Mở quét QR','work/scan','btn'):'';
  const canFilter=workSearchTabs.has(active)&&!(active==='events'&&editorId)&&!(active==='venues'&&venueEditorId);
  return `<div class="workspace">
    <nav class="side work-sidebar" aria-label="Menu vận hành"><div class="side-title">${ic('grid',18)} <span>${esc(roles[user.role])}</span></div>
      ${overview}${groups.map(([label,links])=>'<div class="work-menu-group"><p>'+esc(label)+'</p>'+links.map(item).join('')+'</div>').join('')}
    </nav>
    <div class="work-mobile-nav">${field('Khu vực làm việc',select('workTab',opt('overview','Tổng quan',active)+groups.map(([label,links])=>
      `<optgroup label="${esc(label)}">${links.map(([id,,name])=>opt(id,name,active)).join('')}</optgroup>`).join(''),'data-change="workTab"'))}</div>
    <div class="work-content">
      ${current?`<header class="work-heading split"><div><p class="small">Vận hành / ${esc(roles[user.role])}</p><h1>${esc(current[2])}</h1></div>
        <div class="cluster">${actions}<span data-work-export hidden>${button('Xuất báo cáo','workExport','','secondary')}</span></div></header>
        ${canFilter?`<div class="work-tools" hidden>
        <label class="input-icon">${ic('search',18)}${input('workSearch',workFilter.query,'search','aria-label="Tìm trong danh sách" placeholder="Tìm trong danh sách" data-work-search')}</label>
        ${select('workStatus',opt('','Mọi trạng thái'),'aria-label="Trạng thái" data-work-status hidden')}
        <span role="status" data-work-count></span></div>`:''}
        <div data-work-body>${workspaceBody(user,active)}</div>`:pageTitle('Không có quyền truy cập')+notice('Mục này nằm ngoài quyền hiện tại của tài khoản.',true)}
    </div></div>`;
}

function adminManagers() {
  return db.accounts.filter(person=>person.role==='manager');
}

function adminWorkWarnings(now=Date.now()) {
  const accounts=new Map(db.accounts.map(person=>[person.id,person]));
  const assignments=['applications','reviews'].flatMap(kind=>adminAssignmentItems(kind)
    .filter(record=>record.managerId&&!canTakeAdminAssignment(accounts.get(record.managerId),kind))
    .map(record=>({kind,record})));
  return {
    applications:db.applications.filter(app=>app.state==='PENDING'&&Number.isFinite(app.deadline)&&app.deadline<=now),
    events:db.events.filter(event=>event.state==='SUSPENDED'),
    alerts:db.alerts.filter(alert=>!alert.resolved),assignments
  };
}

function settlementAwaitingReview(record,now=Date.now()) {
  return ['ORGANIZER_CONFIRMED','DISPUTED'].includes(record.state)||record.state==='DRAFT'&&record.deadline<=now;
}

function adminFinanceQueue(now=Date.now()) {
  const payoutSettlements=new Set(db.payouts.map(payout=>payout.settlementId));
  return {
    settlements:db.settlements.filter(record=>settlementAwaitingReview(record,now)||
      record.state==='MANAGER_APPROVED'&&!payoutSettlements.has(record.id)),
    payouts:db.payouts.filter(payout=>['PAYOUT_PENDING','PROCESSING','FAILED'].includes(payout.state))
  };
}

function adminActiveEvents(now=Date.now()) {
  return db.events.filter(event=>event.state==='ACTIVE'&&event.live?.sessions.some(session=>session.end>now));
}

function adminOverview() {
  const now=Date.now();
  const managers=adminManagers();
  const warnings=adminWorkWarnings(now);
  const otherWarnings=warnings.alerts.length+warnings.assignments.length;
  const finance=adminFinanceQueue(now);
  const items=[
    ['Quản lý hệ thống hoạt động',`${managers.filter(person=>!person.locked).length} / ${managers.length}`,'users','accounts/managers','Tài khoản đang hoạt động / tổng số'],
    ['Cảnh báo quá hạn & vi phạm',warnings.applications.length+warnings.events.length+otherWarnings,'shield','monitoring/alerts',
      `${warnings.applications.length} hồ sơ quá hạn · ${warnings.events.length} sự kiện đình chỉ · ${otherWarnings} cảnh báo khác`],
    ['Đối soát & giải ngân',finance.settlements.length+finance.payouts.length,'wallet','settlements/queue',`${finance.settlements.length} kỳ đối soát · ${finance.payouts.length} lệnh chi cần theo dõi`],
    ['Sự kiện & địa điểm',adminActiveEvents(now).length,'calendar','reviews/active','Sự kiện đang vận hành']
  ];
  const quickAccess=`<section class="panel" aria-labelledby="admin-quick-title">
    <h2 id="admin-quick-title">Truy cập nhanh</h2>
    <div class="admin-shortcuts">
      <h3>Ưu tiên Admin</h3>
      ${adminShortcut('Tạo & phân quyền Quản lý hệ thống','users',
        [['Tài khoản & quyền','accounts/managers']],button('Tạo tài khoản','managerAccount','','ghost compact'))}
      <button type="button" class="admin-shortcut admin-shortcut-action" data-action="adminAssignments">
        ${ic('list',19)} <strong>Phân công hồ sơ / sự kiện</strong>${ic('chevron',17)}
      </button>
      ${adminShortcut('Đối soát & giải ngân','wallet',[['Đối soát','settlements'],['Giải ngân','payouts']])}
      ${adminShortcut('Nhật ký & cấu hình hệ thống','settings',[['Nhật ký','audit'],['Cấu hình','config']])}
    </div>
    <div class="admin-shortcuts">
      <h3>Lối tắt nghiệp vụ khác</h3>
      ${adminShortcut('Hồ sơ BTC & Sự kiện','calendar',[['Hồ sơ BTC','applications'],['Sự kiện','reviews']])}
      ${adminShortcut('Địa điểm & Thuê','building',[['Địa điểm','venues'],['Yêu cầu thuê','rentals']])}
      ${adminShortcut('Hoàn tiền & Hỗ trợ','chat',[['Hoàn tiền','refunds'],['Hỗ trợ','support']])}
    </div>
  </section>`;
  return `<div class="stack admin-overview">
    ${metrics(items)}<div class="layout">${quickAccess}${adminRecentActivity()}</div>
  </div>`;
}

function adminShortcut(label,icon,links,action='') {
  return `<div class="admin-shortcut">${ic(icon,19)}<div><strong>${esc(label)}</strong>
    <div class="admin-shortcut-links">${action}${links.map(([text,tab])=>link(text,'work/'+tab)).join('')}</div>
  </div></div>`;
}

const adminActivityGroups={
  accounts:['Tạo tài khoản Quản lý hệ thống','Cập nhật tài khoản và quyền','Khóa tài khoản','Mở khóa tài khoản',
    'Cấp quyền Quản lý hệ thống','Thu hồi quyền Quản lý hệ thống','Thu hồi phiên đăng nhập','Xác minh thụ hưởng'],
  applications:['Gửi hồ sơ BTC','Xét duyệt hồ sơ BTC','Đề xuất tạm dừng BTC','Phân công hồ sơ BTC','Bỏ phân công hồ sơ BTC'],
  events:['Xét duyệt sự kiện','Đình chỉ sự kiện','Khôi phục sự kiện','Cấp quyền nhân sự','Thu hồi quyền nhân sự',
    'Phân công sự kiện','Bỏ phân công sự kiện'],
  settlements:['Khóa sổ kỳ đối soát','BTC phản hồi đối soát','Xét duyệt đối soát'],
  payouts:['Lập lệnh giải ngân','Cập nhật lệnh chi'],
  orders:['Xác nhận thanh toán','Đối soát thanh toán'],
  transactions:['Đối chiếu giao dịch'],
  rentals:['Gửi hồ sơ thuê địa điểm','Xử lý yêu cầu thuê','Ghi nhận chứng từ cọc','Yêu cầu hủy thuê','Giải phóng lịch thuê'],
  refunds:['Tạo yêu cầu hoàn','Xử lý hoàn tiền'],
  system:['Thay đổi cấu hình','Khôi phục cấu hình','Cấu hình chính sách','Đóng cảnh báo','Phục hồi dữ liệu']
};
const adminActivityLabels={
  'Xét duyệt hồ sơ BTC':{APPROVED:'Duyệt hồ sơ BTC',NEEDS_INFO:'Yêu cầu bổ sung hồ sơ BTC',REJECTED:'Từ chối hồ sơ BTC'},
  'Xét duyệt sự kiện':{approve:'Duyệt sự kiện',reject:'Từ chối sự kiện'},
  'BTC phản hồi đối soát':{ORGANIZER_CONFIRMED:'BTC xác nhận đối soát',DISPUTED:'BTC yêu cầu đối chiếu'},
  'Xét duyệt đối soát':{MANAGER_APPROVED:'Duyệt đối soát',DISPUTED:'Yêu cầu đối chiếu thêm'},
  'Cập nhật lệnh chi':{PROCESSING:'Gửi lệnh giải ngân',COMPLETED:'Xác nhận đã chuyển tiền',FAILED:'Chuyển khoản thất bại'},
  'Xử lý yêu cầu thuê':{AWAITING_DEPOSIT:'Duyệt giữ lịch thuê',CONFIRMED:'Xác nhận cọc thuê',CANCELLED:'Duyệt hủy thuê',
    REJECTED:'Từ chối yêu cầu thuê',ESCALATED:'Chuyển tranh chấp thuê lên Admin'},
  'Xử lý hoàn tiền':{APPROVED:'Duyệt hoàn tiền',PROCESSING:'Gửi giao dịch hoàn tiền',COMPLETED:'Xác nhận đã hoàn tiền',
    REJECTED:'Từ chối hoàn tiền',FAILED:'Giao dịch hoàn tiền thất bại'}
};

function adminRecentActivity() {
  const types=new Map(Object.entries(adminActivityGroups).flatMap(([type,actions])=>actions.map(action=>[action,type])));
  const entries=db.audit.filter(entry=>types.has(entry.action)).sort((a,b)=>b.time-a.time).slice(0,6);
  return `<section class="panel" aria-labelledby="admin-activity-title"><div class="split admin-activity-heading">
    <h2 id="admin-activity-title">Hoạt động gần đây</h2>${link('Xem nhật ký','work/audit','small')}
    </div>${entries.length?`<ul class="admin-activity">${entries.map(entry=>{
      const [result]=String(entry.detail??'').split(' · ',1);
      const label=adminActivityLabels[entry.action]?.[result]||entry.action;
      return `<li><strong>${esc(label)}</strong><span>${esc(adminActivityReference(entry,types.get(entry.action)))}</span>
        <time datetime="${new Date(entry.time).toISOString()}">${esc(fmt(entry.time))}</time></li>`;
    }).join('')}</ul>`:empty('Chưa có hoạt động quản trị hoặc giao dịch cần theo dõi.')}</section>`;
}

function adminActivityReference({action,subject},type) {
  const records=db[type]||[];
  const record=action==='Gửi hồ sơ thuê địa điểm'?records.find(item=>item.batchId===subject):lookup(records,subject);
  const name={
    accounts:()=>record?.name||'Tài khoản',
    applications:()=>record?.company||'Hồ sơ BTC',
    events:()=>eventTitle(subject),
    settlements:()=>record?eventTitle(record.eventId):'Bảng đối soát',
    payouts:()=>lookup(db.accounts,record?.owner)?.name||'Lệnh giải ngân',
    orders:()=>record?eventTitle(record.eventId):'Đơn vé',
    transactions:()=>record?.orderId?'Đơn '+record.orderId:'Giao dịch',
    rentals:()=>lookup(db.venues,record?.venueId)?.name||'Yêu cầu thuê',
    refunds:()=>record?.orderId?'Đơn '+record.orderId:record?.rentalId?'Yêu cầu thuê '+record.rentalId:'Yêu cầu hoàn tiền'
  }[type]?.();
  return [name,subject].filter(Boolean).join(' · ');
}

function adminWarningsPanel() {
  const {applications,events,alerts,assignments}=adminWorkWarnings();
  const alertRows=alerts.map(alert=>row(alert.text,fmt(alert.time),
    button('Đã xử lý','resolveAlert',alert.id,'secondary compact')));
  const assignmentRows=assignments.map(({kind,record})=>row(adminAssignmentTitle(kind,record)+' · '+record.id,
    'Tài khoản phụ trách đã bị khóa hoặc mất quyền xử lý.',
    button('Phân công lại','adminAssign',record.id,'secondary compact',attr('kind',kind))));
  return `<div class="stack">
    ${panel('Hồ sơ BTC quá hạn SLA 7 ngày làm việc',applications.map(app=>row(app.company+' · '+app.id,
      'Hạn xét duyệt '+fmt(app.deadline),button('Xem hồ sơ','applicationDetails',app.id,'secondary compact')+
      button('Xét duyệt','applicationDecision',app.id,'compact'))).join('')||empty('Không có hồ sơ BTC quá hạn.'))}
    ${panel('Sự kiện bị đình chỉ',events.map(event=>row(eventTitle(event.id)+' · '+event.id,event.decision,
      tag(event.state)+button('Kiểm tra hồ sơ','inspectEvent',event.id,'secondary compact')+
      (event.live?button('Khôi phục','suspend',event.id,'compact'):''))).join('')||empty('Không có sự kiện bị đình chỉ.'))}
    ${panel('Cảnh báo cần xử lý',[...alertRows,...assignmentRows].join('')||empty('Không có cảnh báo đang mở.'))}
    <div>${link('Xem giám sát dịch vụ','work/monitoring','btn secondary')}</div>
  </div>`;
}

function adminFinancePanel() {
  const {settlements,payouts}=adminFinanceQueue();
  return `<div class="stack">${settlementsPanel(settlements,'admin','Không có kỳ đối soát chờ duyệt hoặc lập lệnh chi.')}
    ${panel('Lệnh giải ngân cần theo dõi',payoutList(payouts,true))}
    <div class="cluster">${link('Tất cả kỳ đối soát','work/settlements','btn secondary')}
      ${link('Tất cả lệnh giải ngân','work/payouts','btn secondary')}</div></div>`;
}

function adminActiveEventsPanel() {
  return `<div class="stack">${moderationPanel(adminActiveEvents(),'Sự kiện đang vận hành')}
    <div class="cluster">${link('Tất cả sự kiện','work/reviews','btn secondary')}
      ${link('Địa điểm','work/venues','btn secondary')}${link('Yêu cầu thuê','work/rentals','btn secondary')}</div></div>`;
}

function adminAssignmentItems(kind) {
  if (kind==='applications') return db.applications.filter(app=>app.state!=='DRAFT');
  if (kind==='reviews') return db.events.filter(event=>event.state==='PENDING'||event.live&&!['CANCELLED','COMPLETED'].includes(event.state));
  return [];
}

function adminAssignmentTitle(kind,record) {
  return kind==='applications'?record.company:eventTitle(record.id);
}

function canTakeAdminAssignment(manager,kind) {
  return manager?.role==='manager'&&!manager.locked&&Array.isArray(manager.scope)&&manager.scope.includes(kind);
}

function openAdminAssignments() {
  requireAdmin();
  const list=(kind,title)=>panel(title,adminAssignmentItems(kind).map(record=>{
    const manager=lookup(db.accounts,record.managerId);
    const assignee=manager?.name||record.managerId;
    const detail=assignee?'Phụ trách: '+assignee:'Chưa phân công';
    const unavailable=assignee&&!canTakeAdminAssignment(manager,kind);
    return row(adminAssignmentTitle(kind,record)+' · '+record.id,detail+(unavailable?' · Cần phân công lại':''),
      button('Phân công','adminAssign',record.id,'secondary compact',attr('kind',kind)));
  }).join('')||empty('Chưa có mục để phân công.'));
  openDialog('Phân công hồ sơ / sự kiện',`<div class="stack">${list('applications','Hồ sơ BTC')}${list('reviews','Sự kiện')}</div>`);
}

function adminAssignmentForm(id,{kind}) {
  requireAdmin();
  const record=lookup(adminAssignmentItems(kind),id);
  assert(record,'Hồ sơ hoặc sự kiện không còn khả dụng.');
  const eligible=()=>adminManagers().filter(manager=>canTakeAdminAssignment(manager,kind));
  const previous=record.managerId||'';
  const managers=eligible();
  const options=opt('','Chưa phân công',previous)+managers.map(manager=>opt(manager.id,manager.name,previous)).join('');
  openDialog('Phân công '+(kind==='applications'?'hồ sơ BTC':'sự kiện'),`
    <p>${esc(adminAssignmentTitle(kind,record))} · ${esc(id)}</p>
    ${field('Quản lý hệ thống phụ trách',select('managerId',options))}
    ${managers.length?'':notice('Chưa có Quản lý hệ thống đang hoạt động được cấp quyền cho nghiệp vụ này.')}`,form=>{
    requireAdmin();
    const current=lookup(adminAssignmentItems(kind),id);
    const managerId=textValue(form,'managerId');
    const manager=lookup(eligible(),managerId);
    assert(current&&(current.managerId||'')===previous,'Phân công đã thay đổi. Hãy mở lại hồ sơ hoặc sự kiện.');
    assert(!managerId||manager,'Tài khoản được chọn chưa có quyền phù hợp hoặc đã bị khóa.');
    if (managerId===previous) return;
    current.managerId=managerId;
    const subject=kind==='applications'?'hồ sơ BTC':'sự kiện';
    audit((manager?'Phân công ':'Bỏ phân công ')+subject,id,managerId||previous);
    if (manager) message(manager.id,'Phân công '+subject,adminAssignmentTitle(kind,current)+' · '+id);
    notifyUi(manager?'Đã lưu phân công.':'Đã bỏ phân công.');
  },'Lưu phân công');
}

function workRecords() {
  const body=root.querySelector('[data-work-body]');
  return body?[...body.querySelectorAll('.panel .row,.table-scroll tbody tr')].filter(el=>!el.parentElement.closest('.row')):[];
}

function workStatusOf(row) {
  return [...row.querySelectorAll('[data-status]')].map(el=>el.dataset.status);
}

function updateWorkFilter() {
  const toolbar=root.querySelector('.work-tools');
  if (!toolbar||toolbar.hidden) return;
  const rows=workRecords();
  let count=0;
  for (const row of rows) {
    const included=(!workFilter.query||matches(reportCell(row),workFilter.query))&&
      (!workFilter.status||workStatusOf(row).includes(workFilter.status));
    row.hidden=!included;
    if (included) count++;
  }
  toolbar.querySelector('[data-work-count]').textContent=count+' / '+rows.length+' mục';
}

function initWorkTools() {
  const records=workRecords();
  const toolbar=root.querySelector('.work-tools');
  const exportAction=root.querySelector('[data-work-export]');
  if (exportAction) exportAction.hidden=!records.length||!workSearchTabs.has(route[1]);
  if (toolbar&&records.length) {
    toolbar.hidden=false;
    const states=[...new Set(records.flatMap(workStatusOf))];
    const filter=toolbar.querySelector('[data-work-status]');
    if (!states.includes(workFilter.status)) workFilter.status='';
    filter.hidden=!states.length;
    filter.innerHTML=opt('','Mọi trạng thái')+states.map(state=>opt(state,status[state]?.[0]||state,workFilter.status)).join('');
    updateWorkFilter();
  }
}

function reportCell(element) {
  const clone=element.cloneNode(true);
  clone.querySelectorAll('button,select,input,textarea,a.btn,svg').forEach(el=>el.remove());
  clone.querySelectorAll('br,small,.badge').forEach(el=>el.before(' '));
  return clone.textContent.replace(/\s+/g,' ').trim();
}

function workReportSections() {
  const body=root.querySelector('[data-work-body]');
  if (!body) return [];
  const sections=[];
  const allRows=workRecords().filter(row=>!row.hidden);
  for (const panel of body.querySelectorAll('.panel')) {
    const title=panel.querySelector(':scope > h3,:scope > h2,:scope > .split h3')?.textContent.trim()||'Danh sách';
    for (const table of panel.querySelectorAll('table')) {
      if (table.closest('.panel')!==panel) continue;
      const columns=[...table.querySelectorAll('th')].map((el,i)=>[reportCell(el),i]).filter(([label])=>!['Thao tác','Chi tiết'].includes(label));
      const rows=[...table.querySelectorAll('tbody tr')].filter(tr=>!tr.hidden).map(tr=>columns.map(([,i])=>reportCell(tr.cells[i])));
      if (rows.length&&columns.length) sections.push({title,headers:columns.map(([label])=>label),rows});
    }
    const list=allRows.filter(row=>row.matches('.row')&&row.closest('.panel')===panel);
    if (list.length) sections.push({title,headers:['Tên / Mã','Thông tin','Trạng thái'],rows:list.map(el=>[
      el.querySelector('strong')?.textContent.trim()||'',
      el.querySelector('small')?.textContent.trim()||'',
      workStatusOf(el).map(state=>status[state]?.[0]||state).join(' · ')
    ])});
  }
  return sections;
}

function openWorkExport() {
  const user=account();
  const tab=route[1];
  assert(route[0]==='work'&&workspaceMenus[user?.role]&&workSearchTabs.has(tab),'Mục này không có báo cáo để xuất.');
  render();
  const title=root.querySelector('.work-heading h1')?.textContent||'Báo cáo';
  const sections=workReportSections();
  assert(sections.length,'Danh sách đang lọc không có dữ liệu.');
  openDialog('Xuất báo cáo',`<p class="muted">${esc(title)} · ${sections.reduce((sum,s)=>sum+s.rows.length,0)} dòng đang hiển thị</p>
    <fieldset class="request-fields"><legend>Định dạng</legend><div class="stack">
      ${[['xlsx','Excel (.xlsx)'],['docx','Word (.docx)'],['pdf','In hoặc lưu PDF']].map(([kind,label],i)=>
        `<label class="check"><input type="radio" name="format" value="${kind}"${i===0?' checked':''}> ${label}</label>`).join('')}</div></fieldset>`,
    form=>{
      assert(account()?.id===user.id&&route[0]==='work'&&route[1]===tab,'Phiên làm việc đã thay đổi.');
      render();
      const current=workReportSections();
      assert(current.length,'Danh sách này không còn dữ liệu trong phạm vi của bạn.');
      const type=textValue(form,'format');
      const name='VENUE-'+normalized(title).replace(/[^a-z0-9]+/g,'-')+'-'+dayKey(Date.now());
      if (type==='xlsx') download(name+'.xlsx',excelReport(current),'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet');
      else if (type==='docx') download(name+'.docx',wordReport(title,current),'application/vnd.openxmlformats-officedocument.wordprocessingml.document');
      else if (type==='pdf') printReport(title,current);
      else throw new Error('Định dạng báo cáo không hợp lệ.');
    },'Xuất báo cáo');
}

