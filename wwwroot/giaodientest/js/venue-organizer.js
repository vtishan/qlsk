const organizerTypes={individual:'Cá nhân',organization:'Doanh nghiệp / Tổ chức'};
const organizerBanks={
  VCB:'Vietcombank (VCB)',TCB:'Techcombank (TCB)',MB:'MB Bank',ACB:'ACB',BIDV:'BIDV',
  CTG:'VietinBank',AGR:'Agribank',VPB:'VPBank',TPB:'TPBank',STB:'Sacombank',
  VIB:'VIB',HDB:'HDBank',SHB:'SHB',OCB:'OCB',MSB:'MSB',LPB:'LPBank',SSB:'SeABank',EIB:'Eximbank'
};
const identityFields={
  individual:[
    {name:'fullName',label:'Họ và tên đầy đủ (theo CCCD / Hộ chiếu)',wide:true,autocomplete:'name'},
    {name:'identityType',label:'Loại giấy tờ',options:{card:'CCCD',passport:'Hộ chiếu'}},
    {name:'identityNumber',label:'Số CCCD / Hộ chiếu',max:12},
    {name:'issuedDate',label:'Ngày cấp',type:'date'},
    {name:'issuedPlace',label:'Nơi cấp'},
    {name:'personalTax',label:'Mã số thuế cá nhân',optional:true,max:14}
  ],
  organization:[
    {name:'organizationName',label:'Tên doanh nghiệp / tổ chức (theo GPKD)',wide:true,autocomplete:'organization'},
    {name:'organizationTax',label:'Mã số thuế (MST)',max:14},
    {name:'representative',label:'Người đại diện theo pháp luật'},
    {name:'position',label:'Chức vụ',autocomplete:'organization-title'},
    {name:'headOffice',label:'Địa chỉ trụ sở đăng ký',wide:true}
  ]
};
const organizerContact=[
  {name:'email',label:'Email liên hệ chính',type:'email',autocomplete:'email',hint:'Dùng để nhận thông báo và đối soát.'},
  {name:'phone',label:'Số điện thoại',type:'tel',autocomplete:'tel',hint:'Zalo hoặc hotline hỗ trợ khẩn cấp.'},
  {name:'contactAddress',label:'Địa chỉ liên hệ thực tế',wide:true,autocomplete:'street-address',hint:'Địa chỉ nhận hợp đồng và hóa đơn.'},
  {name:'website',label:'Website / Fanpage sự kiện',wide:true,optional:true,mode:'url'}
];
const organizerPayment=[
  {name:'bankName',label:'Tên ngân hàng',options:organizerBanks},
  {name:'bankBranch',label:'Chi nhánh ngân hàng',optional:true},
  {name:'bankAccount',label:'Số tài khoản ngân hàng',mode:'numeric',max:34},
  {name:'bankOwner',label:'Tên chủ tài khoản',wide:true,
    hint:'Tên chủ tài khoản phải trùng khớp với tên Cá nhân hoặc Doanh nghiệp đã đăng ký ở Phần 1.'}
];
const organizerDocuments={
  individual:[
    {name:'identityFront',label:'Ảnh CCCD mặt trước'},
    {name:'identityBack',label:'Ảnh CCCD mặt sau'}
  ],
  organization:[
    {name:'businessLicense',label:'Giấy chứng nhận đăng ký doanh nghiệp'},
    {name:'authorization',label:'Giấy ủy quyền',optional:true,hint:'Đính kèm nếu người đăng ký không phải đại diện pháp luật.'}
  ]
};
const organizerTerms={
  duties:{
    title:'Trách nhiệm Ban tổ chức',
    text:['Cung cấp thông tin pháp lý, giấy tờ và thông tin liên hệ chính xác. Cập nhật hồ sơ khi thông tin thay đổi.',
      'Chịu trách nhiệm về nội dung, điều kiện tổ chức, lịch diễn, địa điểm và quyền lợi vé đã công bố. Gửi sự kiện và các thay đổi ảnh hưởng đến người mua để được xét duyệt.',
      'Phối hợp xử lý hỗ trợ, khiếu nại, hoàn vé và thông báo đến người mua khi sự kiện thay đổi.']
  },
  finance:{
    title:'Điều khoản tài chính',
    text:['Doanh thu được đối soát theo giao dịch thanh toán thành công, các khoản hoàn, phí theo hợp đồng và khoản điều chỉnh áp dụng.',
      'Ban tổ chức kiểm tra báo cáo đối soát và gửi xác nhận hoặc phản hồi trong thời hạn quy định.',
      'Khoản chi trả được chuyển đến tài khoản thụ hưởng đã xác minh sau khi hoàn tất các bước đối soát và phê duyệt.']
  }
};

function organizerFieldLabel({label,optional}) {
  return esc(label)+(optional?' <span class="organizer-optional">Không bắt buộc</span>':' <span class="organizer-required" aria-hidden="true">*</span>');
}

function organizerFieldMarkup(field,values) {
  const {name,type='text',options,optional,wide,mode,autocomplete,hint,max=180}=field;
  const id='organizer-'+name;
  const value=values[name]??(name==='identityType'?'card':'');
  const attributes=`id="${id}" data-organizer-field aria-describedby="${id}-error${hint?' '+id+'-hint':''}"${optional?'':' required'}`;
  const control=options?select(name,(name==='bankName'?opt('','Chọn ngân hàng',value):'')+
    Object.entries(options).map(([key,label])=>opt(key,label,value)).join(''),attributes):
    input(name,value,type,`${attributes} ${type==='date'?'max="'+dayKey(Date.now())+'"':'maxlength="'+max+'"'}${mode?' inputmode="'+mode+'"':''}${autocomplete?' autocomplete="'+autocomplete+'"':''}`);
  return `<div class="organizer-field${wide?' span':''}"><label for="${id}">${organizerFieldLabel(field)}</label>${control}
    ${hint?`<p id="${id}-hint" class="organizer-hint">${esc(hint)}</p>`:''}
    <p class="organizer-error" id="${id}-error" data-organizer-error aria-live="polite"></p></div>`;
}

function organizerUploadMarkup(doc) {
  const {name,hint,optional}=doc;
  const id='organizer-'+name;
  return `<div class="organizer-upload" data-organizer-upload="${name}">
    <label id="${id}-label" for="${id}">${organizerFieldLabel(doc)}</label>
    <label class="organizer-drop">
      <input type="file" id="${id}" name="${name}" accept=".pdf,.png,.jpg,.jpeg,application/pdf,image/png,image/jpeg"
        aria-labelledby="${id}-label" aria-describedby="organizer-file-rules ${id}-error${hint?' '+id+'-hint':''}"${optional?'':' required'}>
      ${ic('download',20)} <span>Kéo thả hoặc <strong>chọn tệp</strong></span>
    </label>
    <div class="organizer-file" hidden><span data-organizer-filename></span><div class="cluster">
      <button type="button" class="btn ghost compact" data-organizer-preview aria-label="Xem trước ${esc(doc.label)}">Xem trước</button>
      <button type="button" class="btn ghost compact organizer-file-remove" data-organizer-remove aria-label="Xóa ${esc(doc.label)}">Xóa</button>
    </div></div>
    ${hint?`<p id="${id}-hint" class="organizer-hint">${esc(hint)}</p>`:''}
    <p class="organizer-error" id="${id}-error" data-organizer-error aria-live="polite"></p>
  </div>`;
}

function organizerFormMarkup(type,values,documentNotice) {
  const branch=(category,content)=>`<fieldset class="form-grid organizer-branch" data-organizer-type="${category}"${type===category?'':' hidden disabled'}>${content}</fieldset>`;
  const fields=list=>list.map(field=>organizerFieldMarkup(field,values)).join('');
  return `<form class="organizer-form" data-form="dialog" novalidate>
    <input type="hidden" name="mode" value="PENDING">
    <div class="organizer-body">
      <fieldset class="organizer-type">
        <legend>Loại hình đăng ký</legend>
        <div class="organizer-type-options">
          ${Object.entries(organizerTypes).map(([key,label])=>`<label><input type="radio" name="registrationType" value="${key}"${type===key?' checked':''} required><span>${esc(label)}</span></label>`).join('')}
        </div>
      </fieldset>
      <section class="organizer-section" aria-labelledby="organizer-identity-title">
        <h3 id="organizer-identity-title">1. Thông tin định danh pháp lý</h3>
        ${Object.entries(identityFields).map(([key,list])=>branch(key,fields(list))).join('')}
      </section>
      <section class="organizer-section" aria-labelledby="organizer-contact-title">
        <h3 id="organizer-contact-title">2. Thông tin liên hệ &amp; vận hành</h3>
        <div class="form-grid">${fields(organizerContact)}</div>
      </section>
      <section class="organizer-section" aria-labelledby="organizer-payment-title">
        <h3 id="organizer-payment-title">3. Tài khoản nhận thanh toán</h3>
        <p class="organizer-section-note">Dùng để đối soát và nhận doanh thu sự kiện.</p>
        <div class="form-grid">${fields(organizerPayment)}</div>
      </section>
      <section class="organizer-section" aria-labelledby="organizer-documents-title">
        <h3 id="organizer-documents-title">4. Hồ sơ đính kèm</h3>
        <p class="organizer-section-note" id="organizer-file-rules">PDF, PNG, JPG hoặc JPEG, tối đa 5 MB mỗi tệp.</p>
        ${documentNotice?'<p class="organizer-section-note">'+esc(documentNotice)+'</p>':''}
        ${Object.entries(organizerDocuments).map(([key,list])=>branch(key,list.map(organizerUploadMarkup).join(''))).join('')}
      </section>
    </div>
    <footer class="organizer-footer">
      <div class="organizer-consent">
        <input id="organizer-agreement" type="checkbox" name="agreement" required value="yes" aria-describedby="organizer-agreement-error">
        <div><label for="organizer-agreement">Tôi cam kết mọi thông tin khai báo là chính xác và đồng ý với</label>
          <button type="button" class="organizer-term" data-organizer-term="duties">Trách nhiệm Ban tổ chức</button> cùng
          <button type="button" class="organizer-term" data-organizer-term="finance">Điều khoản tài chính</button>.
          <p id="organizer-agreement-error" class="organizer-error" aria-live="polite"></p>
        </div>
      </div>
      <p class="organizer-error form-error" role="alert"></p>
      <div class="organizer-actions">
        <button type="button" class="btn secondary" data-organizer-draft>Lưu nháp</button>
        <button type="submit" class="btn" disabled>Gửi hồ sơ xét duyệt</button>
      </div>
    </footer>
  </form>`;
}

function organizerPhone(value) {
  return value.replace(/[\s().-]/g,'').replace(/^\+?84/,'0');
}

function organizerWebsite(value) {
  if (!value) return '';
  try {
    const url=new URL(/^[a-z][a-z\d+.-]*:/i.test(value)?value:'https://'+value);
    return ['http:','https:'].includes(url.protocol)&&url.hostname.includes('.')&&!url.username&&!url.password?url.href:'';
  } catch {return '';}
}

function organizerBankOwner(value) {
  return value.normalize('NFD').replace(/\p{Diacritic}/gu,'').replace(/[đĐ]/g,'D').toUpperCase();
}

function organizerFieldError(control,values,type) {
  const {name}=control;
  const value=values[name];
  if (!value) return control.required?'Vui lòng điền thông tin này.':'';
  if (control.maxLength>0&&value.length>control.maxLength) return 'Thông tin vượt quá độ dài cho phép.';
  const tax=/^\d{10}(?:-?\d{3})?$/;
  switch (name) {
    case 'identityType':
      return ['card','passport'].includes(value)?'':'Chọn loại giấy tờ hợp lệ.';
    case 'identityNumber':
      if (values.identityType==='card') return /^\d{12}$/.test(value)?'':'Số CCCD phải gồm 12 chữ số.';
      return /^(?=.*\d)[A-Z\d]{6,12}$/i.test(value)?'':'Nhập số hộ chiếu gồm 6–12 ký tự chữ hoặc số, có ít nhất một chữ số.';
    case 'issuedDate':
      return Number.isFinite(parseLocal(value))&&value<=dayKey(Date.now())?'':'Ngày cấp không được sau ngày hiện tại.';
    case 'personalTax':
      return tax.test(value)||/^\d{12}$/.test(value)?'':'Nhập mã số thuế hoặc số định danh cá nhân hợp lệ.';
    case 'organizationTax':
      return tax.test(value)?'':'Nhập mã số thuế gồm 10 hoặc 13 chữ số.';
    case 'email':
      return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)&&!control.validity.typeMismatch?'':'Nhập địa chỉ email hợp lệ.';
    case 'phone':
      return /^0(?:[35789]\d{8}|2\d{9})$/.test(organizerPhone(value))?'':'Nhập số di động hoặc số cố định Việt Nam hợp lệ.';
    case 'website':
      return organizerWebsite(value)?'':'Nhập địa chỉ website hoặc fanpage hợp lệ (HTTP/HTTPS).';
    case 'bankName':
      return Object.hasOwn(organizerBanks,value)?'':'Chọn ngân hàng trong danh sách.';
    case 'bankAccount':
      return /^\d+$/.test(value)?'':'Số tài khoản chỉ được chứa chữ số.';
    case 'bankOwner': {
      const expected=values[type==='individual'?'fullName':'organizationName'];
      const canonical=name=>normalized(name).replace(/\s+/g,' ');
      return expected&&canonical(expected)!==canonical(value)?'Tên chủ tài khoản phải trùng với tên đăng ký ở Phần 1.':'';
    }
    default: return '';
  }
}

async function organizerFile(file) {
  const extension=file.name.match(/\.([^.]+)$/)?.[1].toLowerCase();
  const types={pdf:'application/pdf',png:'image/png',jpg:'image/jpeg',jpeg:'image/jpeg'};
  assert(Object.hasOwn(types,extension),'Chọn tệp PDF, PNG, JPG hoặc JPEG.');
  assert(file.size>0&&file.size<=5*1024*1024,'Tệp phải có nội dung và không vượt quá 5 MB.');
  const header=new Uint8Array(await file.slice(0,8).arrayBuffer());
  const signature=extension==='pdf'?[37,80,68,70,45]:extension==='png'?[137,80,78,71,13,10,26,10]:[255,216,255];
  assert(signature.every((byte,i)=>header[i]===byte),'Nội dung tệp không khớp với định dạng đã chọn.');
  return new File([file],file.name,{type:types[extension],lastModified:file.lastModified});
}

function organizerOverlay(title,body,objectUrl='') {
  const overlay=document.createElement('dialog');
  overlay.className='organizer-overlay';
  overlay.setAttribute('aria-labelledby','organizer-overlay-title');
  overlay.innerHTML=`<div class="dialog-inner"><header class="split">
    <h2 id="organizer-overlay-title">${esc(title)}</h2>
    <form method="dialog"><button type="submit" class="btn square secondary" aria-label="Đóng">${ic('close',19)}</button></form>
    </header>${body}</div>`;
  const cleanup=()=>{
    modal.removeEventListener('close',cleanup);
    if (objectUrl) URL.revokeObjectURL(objectUrl);
    overlay.remove();
  };
  overlay.addEventListener('close',cleanup,{once:true});
  modal.addEventListener('close',cleanup,{once:true});
  overlay.addEventListener('click',event=>{if (event.target===overlay) overlay.close();});
  document.body.append(overlay);
  overlay.showModal();
}

function previewOrganizerFile(file) {
  const url=URL.createObjectURL(file);
  const preview=file.type==='application/pdf'?`<iframe class="organizer-pdf" src="${esc(url)}" title="Nội dung PDF"></iframe>`:
    `<img class="organizer-preview-image" src="${esc(url)}" alt="${esc(file.name)}">`;
  organizerOverlay(file.name,`${preview}<p class="organizer-preview-link"><a href="${esc(url)}" target="_blank" rel="noopener">Mở tệp trong tab mới</a></p>`,url);
}

async function organizerStorage(mode,operation) {
  const dbConnection=await new Promise((resolve,reject)=>{
    const request=indexedDB.open(storeKey+'-organizer',1);
    request.onupgradeneeded=()=>request.result.createObjectStore('profiles');
    request.onsuccess=()=>resolve(request.result);
    request.onerror=()=>reject(request.error);
  });
  try {
    return await new Promise((resolve,reject)=>{
      const transaction=dbConnection.transaction('profiles',mode);
      const request=operation(transaction.objectStore('profiles'));
      transaction.oncomplete=()=>resolve(request.result);
      transaction.onabort=()=>reject(transaction.error||request.error);
    });
  } finally {
    dbConnection.close();
  }
}

async function readOrganizerProfile(application) {
  const profile=await organizerStorage('readonly',store=>store.get(application.profileId));
  assert(profile?.owner===application.user,'Không tìm thấy thông tin và giấy tờ của hồ sơ trong trình duyệt này.');
  return profile;
}

function legacyOrganizerValues(application) {
  if (!application) return {};
  const {company,representative,legal,contact}=application;
  return {fullName:company,organizationName:company,representative,identityNumber:legal,organizationTax:legal,
    identityType:legal&&!/^\d+$/.test(legal)?'passport':'card',
    email:/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(contact)&&!contact.endsWith('.test')?contact:'',
    phone:/^0(?:[35789]\d{8}|2\d{9})$/.test(organizerPhone(contact||''))?contact:''};
}

async function applicationForm() {
  const user=account();
  if (user?.role==='organizer') {go('work/events');return;}
  assert(user?.role==='user','Hồ sơ chỉ dành cho khách đăng ký làm Ban tổ chức.');
  if (needsVerified(user)) {
    ui.verifiedAction={action:'application',id:''};
    contactForm();
    return;
  }
  const previous=db.applications.find(a=>a.user===user.id);
  assert(previous?.state!=='PENDING','Hồ sơ đang chờ xét duyệt.');
  const startRoute=route;
  let profile=null;
  let documentNotice=previous?.docs?.length?'Hồ sơ cũ chỉ có tên tệp. Vui lòng đính kèm lại giấy tờ để gửi xét duyệt.':'';
  if (previous?.profileId) {
    try {profile=await readOrganizerProfile(previous);}
    catch {documentNotice='Không tìm thấy dữ liệu hồ sơ trong trình duyệt này. Vui lòng bổ sung lại thông tin và giấy tờ.';}
  }
  if (route!==startRoute||account()?.id!==user.id) return;
  const defaultType=previous?.representative&&normalized(previous.representative)!==normalized(previous.company)?'organization':'individual';
  const values=profile?.values||legacyOrganizerValues(previous);
  openDialog('Hồ sơ đăng ký Ban tổ chức',organizerFormMarkup(profile?.type||defaultType,values,documentNotice),
    null,undefined,{variant:'organizer'});
  bindOrganizerForm(modal.querySelector('.organizer-form'),user,previous,profile);
}

function bindOrganizerForm(form,user,previous,profile) {
  const dialog=currentModal;
  const fields=[...form.querySelectorAll('[data-organizer-field]')];
  const uploads=[...form.querySelectorAll('[data-organizer-upload]')].map(element=>{
    const name=element.dataset.organizerUpload;
    return {name,element,control:form.elements[name],file:profile?.files[name],problem:'',token:null,touched:false};
  });
  const primary=form.querySelector('[type="submit"]');
  const draft=form.querySelector('[data-organizer-draft]');
  const agreement=form.elements.agreement;
  const touched=new Set();
  let supersededProfile='';

  function showError(control,error,text) {
    if (error.textContent!==text) error.textContent=text;
    if (text) control.setAttribute('aria-invalid','true');
    else control.removeAttribute('aria-invalid');
  }

  function synchronize(showAll=false) {
    const type=form.elements.registrationType.value;
    const activeFields=fields.filter(control=>!control.matches(':disabled'));
    const values=Object.fromEntries(activeFields.map(control=>[control.name,control.value.trim()]));
    const activeUploads=uploads.filter(upload=>!upload.control.matches(':disabled'));
    const files=Object.fromEntries(activeUploads.filter(upload=>upload.file).map(({name,file})=>[name,file]));
    let valid=Object.hasOwn(organizerTypes,type)&&agreement.checked;
    let reading=false;
    for (const control of activeFields) {
      const problem=organizerFieldError(control,values,type);
      if (problem) valid=false;
      showError(control,control.parentElement.querySelector('[data-organizer-error]'),showAll||touched.has(control)?problem:'');
    }
    for (const upload of activeUploads) {
      const {file,token,problem,control,element}=upload;
      if (token) reading=true;
      const missing=control.required&&!file;
      if (missing||problem) valid=false;
      const fileRow=element.querySelector('.organizer-file');
      fileRow.hidden=!file&&!problem&&!token;
      fileRow.querySelector('[data-organizer-filename]').textContent=token?'Đang kiểm tra tệp…':file?.name||'';
      fileRow.querySelector('[data-organizer-preview]').hidden=!file||!!token;
      fileRow.querySelector('[data-organizer-remove]').hidden=!!token;
      showError(control,element.querySelector('[data-organizer-error]'),problem||
        ((showAll||upload.touched)&&missing&&!token?'Vui lòng đính kèm tệp này.':''));
    }
    showError(agreement,form.querySelector('#organizer-agreement-error'),
      (showAll||touched.has(agreement))&&!agreement.checked?'Bạn cần xác nhận trước khi gửi hồ sơ.':'');
    primary.disabled=dialog.busy||reading||!valid;
    draft.disabled=dialog.busy||reading;
    return {type,values,files,valid:valid&&!reading};
  }

  function normalizeInput(control) {
    if (control.name==='bankAccount') control.value=control.value.replace(/\D/g,'');
    if (control.name==='bankOwner') {
      const value=organizerBankOwner(control.value);
      if (value!==control.value) {
        const caret=control.selectionStart;
        control.value=value;
        if (caret!==null) control.setSelectionRange(caret,caret);
      }
    }
  }

  form.addEventListener('input',event=>{
    if (!event.isComposing) {
      normalizeInput(event.target);
      synchronize();
    }
  });
  form.addEventListener('compositionend',event=>{normalizeInput(event.target);synchronize();});
  form.addEventListener('focusout',event=>{
    if (fields.includes(event.target)) {touched.add(event.target);synchronize();}
  });
  form.addEventListener('change',event=>{
    const control=event.target;
    if (control.name==='registrationType') {
      for (const group of form.querySelectorAll('[data-organizer-type]')) {
        group.disabled=group.hidden=group.dataset.organizerType!==control.value;
      }
    }
    if (control===agreement||fields.includes(control)) touched.add(control);
    form.elements.identityNumber.inputMode=form.elements.identityType.value==='card'?'numeric':'text';
    synchronize();
  });
  form.elements.identityNumber.inputMode=form.elements.identityType.value==='card'?'numeric':'text';

  for (const upload of uploads) {
    const {control,element}=upload;
    const zone=element.querySelector('.organizer-drop');
    async function choose(files) {
      if (!files.length||dialog.busy||control.matches(':disabled')) return;
      const token={};
      upload.token=token;
      upload.touched=true;
      upload.problem='';
      synchronize();
      try {
        assert(files.length===1,'Chỉ chọn một tệp cho mục này.');
        const file=await organizerFile(files[0]);
        if (upload.token===token) upload.file=file;
      } catch(error) {
        if (upload.token===token) upload.problem=error.message;
      } finally {
        if (upload.token===token) {
          upload.token=null;
          synchronize();
        }
      }
    }
    control.addEventListener('change',()=>{
      const files=[...control.files];
      control.value='';
      void choose(files);
    });
    zone.addEventListener('dragover',event=>{event.preventDefault();zone.classList.add('is-dragging');});
    zone.addEventListener('dragleave',event=>{if (!zone.contains(event.relatedTarget)) zone.classList.remove('is-dragging');});
    zone.addEventListener('drop',event=>{
      event.preventDefault();
      zone.classList.remove('is-dragging');
      void choose([...event.dataTransfer.files]);
    });
    element.querySelector('[data-organizer-preview]').addEventListener('click',()=>{if (upload.file) previewOrganizerFile(upload.file);});
    element.querySelector('[data-organizer-remove]').addEventListener('click',()=>{
      Object.assign(upload,{file:null,problem:'',token:null,touched:true});
      synchronize();
      control.focus();
    });
  }

  for (const control of form.querySelectorAll('[data-organizer-term]')) {
    control.addEventListener('click',()=>{
      const term=organizerTerms[control.dataset.organizerTerm];
      organizerOverlay(term.title,term.text.map(text=>'<p>'+esc(text)+'</p>').join(''));
    });
  }
  draft.addEventListener('click',()=>{
    form.elements.mode.value='DRAFT';
    try {form.requestSubmit();} finally {form.elements.mode.value='PENDING';}
  });
  dialog.setBusy=data=>{
    const submitting=!!data;
    const savingDraft=data?.get('mode')==='DRAFT';
    if (submitting) form.querySelector('.form-error').textContent='';
    dialog.busy=submitting;
    form.setAttribute('aria-busy',String(submitting));
    form.querySelector('.organizer-body').inert=submitting;
    form.querySelector('.organizer-consent').inert=submitting;
    const close=modal.querySelector('.dialog-inner > header [data-action="close"]');
    if (modal.contains(form)&&close) close.disabled=submitting;
    primary.textContent=submitting&&!savingDraft?'Đang gửi hồ sơ…':'Gửi hồ sơ xét duyệt';
    draft.textContent=submitting&&savingDraft?'Đang lưu…':'Lưu nháp';
    (savingDraft?draft:primary).classList.toggle('is-loading',submitting);
    synchronize();
    if (!submitting) {
      draft.classList.remove('is-loading');
      if (supersededProfile&&!memoryOnly) {
        void organizerStorage('readwrite',store=>store.delete(supersededProfile)).catch(()=>{});
      }
    }
  };
  dialog.submit=async data=>{
    const state=textValue(data,'mode')==='DRAFT'?'DRAFT':'PENDING';
    const {type,values,files,valid}=synchronize(state==='PENDING');
    if (state==='PENDING'&&!valid) {
      requestAnimationFrame(()=>form.querySelector('[aria-invalid="true"]:not(:disabled)')?.focus());
      return false;
    }
    assert(Object.hasOwn(organizerTypes,type),'Chọn loại hình đăng ký.');
    const owner=()=>account()?.id===user.id&&account()?.role==='user'&&!needsVerified(account());
    assert(owner()&&modal.contains(form),'Tài khoản cần xác minh và đủ điều kiện gửi hồ sơ.');
    const current=db.applications.find(a=>a.user===user.id);
    assert(current?.id===previous?.id&&current?.profileId===previous?.profileId&&current?.state===previous?.state&&
      current?.history?.[0]?.time===previous?.history?.[0]?.time,'Hồ sơ đã thay đổi. Hãy mở lại để xem bản mới nhất.');
    assert(current?.state!=='PENDING','Hồ sơ đang chờ xét duyệt.');
    const profileId=uid('KYC-');
    const formattedWebsite=organizerWebsite(values.website);
    const savedValues={...values,bankOwner:organizerBankOwner(values.bankOwner),
      phone:state==='PENDING'?organizerPhone(values.phone):values.phone,website:formattedWebsite||values.website};
    try {
      await organizerStorage('readwrite',store=>store.put({owner:user.id,type,values:savedValues,files},profileId));
    } catch {
      throw new Error('Không lưu được hồ sơ trong trình duyệt. Hãy kiểm tra dung lượng lưu trữ rồi thử lại.');
    }
    if (!owner()||!modal.contains(form)) {
      await organizerStorage('readwrite',store=>store.delete(profileId)).catch(()=>{});
      throw new Error('Phiên đăng nhập hoặc biểu mẫu đã đóng. Hãy mở lại hồ sơ để tiếp tục.');
    }
    const now=Date.now();
    const company=type==='individual'?values.fullName:values.organizationName;
    const next={id:current?.id||uid('APP-').slice(0,15),user:user.id,company,profileId,state,
      submitted:now,deadline:state==='PENDING'?workDeadline(now,7):null,note:'',managerId:current?.managerId||'',
      history:[{state,time:now},...(current?.history||[])]};
    if (current) db.applications[db.applications.indexOf(current)]=next;
    else db.applications.unshift(next);
    audit(state==='DRAFT'?'Lưu nháp hồ sơ BTC':'Gửi hồ sơ BTC',next.id);
    if (state==='PENDING') message(user.id,'Đã gửi hồ sơ Ban tổ chức','Hạn xử lý '+fmt(next.deadline));
    supersededProfile=current?.profileId||'';
    notifyUi(state==='DRAFT'?'Đã lưu bản nháp.':'Đã gửi hồ sơ xét duyệt.');
  };
  synchronize();
}

async function applicationDetails(id) {
  const app=lookup(db.applications,id);
  assert(canManage('applications')&&app,'Hồ sơ nằm ngoài quyền truy cập.');
  const startRoute=route;
  const profile=app.profileId?await readOrganizerProfile(app):null;
  if (route!==startRoute||!canManage('applications')) return;
  const history=(app.history||[]).map(item=>row(status[item.state]?.[0]||item.state,fmt(item.time)+(item.note?' · '+item.note:''))).join('');
  const summary=row('Thời hạn xét duyệt',fmt(app.deadline))+panel('Lịch sử hồ sơ',history);
  if (!profile) {
    openDialog('Hồ sơ '+app.company,`<div class="stack">
      ${row('Đại diện',app.representative)}${row('Liên hệ',app.contact)}${row('Thông tin pháp lý',app.legal)}
      ${row('Tài khoản thanh toán',app.bank)}${row('Giấy tờ đã đính kèm',app.docs?.join(', ')||'Chưa có')}
      ${summary}</div>`);
    return;
  }
  const {type,values,files}=profile;
  const details=fields=>fields.filter(({name})=>values[name]).map(({label,name,options})=>
    row(label,options?.[values[name]]||values[name])).join('');
  const documents=organizerDocuments[type].map(({label,name})=>files[name]?row(label,files[name].name,
    `<button type="button" class="btn secondary compact" data-organizer-review-file="${name}">Xem tệp</button>`):'').join('');
  openDialog('Hồ sơ '+app.company,`<div class="stack">
    ${row('Loại hình',organizerTypes[type])}${panel('Thông tin định danh pháp lý',details(identityFields[type]))}
    ${panel('Thông tin liên hệ & vận hành',details(organizerContact))}
    ${panel('Tài khoản nhận thanh toán',details(organizerPayment))}
    ${panel('Hồ sơ đính kèm',documents||empty('Chưa có giấy tờ.'))}${summary}</div>`);
  for (const control of modal.querySelectorAll('[data-organizer-review-file]')) {
    control.addEventListener('click',()=>{
      if (canManage('applications')) previewOrganizerFile(files[control.dataset.organizerReviewFile]);
    });
  }
}
