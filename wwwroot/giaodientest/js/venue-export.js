function printDocument(title,html,style) {
  const frame=document.createElement('iframe');
  frame.title=title;
  frame.style.cssText='position:fixed;width:1px;height:1px;border:0;bottom:0;right:0';
  frame.addEventListener('load',()=>{
    const view=frame.contentWindow;
    view.addEventListener('afterprint',()=>frame.remove(),{once:true});
    view.focus();
    view.print();
    setTimeout(()=>frame.remove(),60000);
  },{once:true});
  frame.srcdoc=`<!doctype html><html lang="vi"><head><meta charset="utf-8"><title>${esc(title)}</title><style>
    body{font:14px system-ui,sans-serif;color:#151129;margin:24px} h1{font-size:1.6rem} ${style}
    </style></head><body>${html}</body></html>`;
  document.body.append(frame);
}

function printableInvite(token) {
  const ticket=db.tickets.find(t=>t.token===token&&validTicket(t)&&t.inviteUntil>Date.now());
  assert(ticket&&publicEvent(lookup(db.events,ticket.eventId)),'Vé mời không còn hiệu lực.');
  return ticket;
}

function printTicket(ticket) {
  assert(validTicket(ticket),'Vé không còn hiệu lực để in.');
  printDocument('VÉNUE '+ticket.id,'<main><h1>VÉNUE · Vé điện tử</h1>'+ticketShowcase(ticket)+'</main>',
    'main{max-width:460px;border:1px solid #dedbea;border-radius:12px;padding:22px}img{display:block;width:160px;height:160px;margin:12px 0}code{display:block;overflow-wrap:anywhere}.badge{display:block}');
}

async function downloadTicketPdf(ticket) {
  assert(validTicket(ticket),'Vé không còn hiệu lực để tải.');
  const canvas=document.createElement('canvas');
  const scale=2;
  canvas.width=595*scale;canvas.height=842*scale;
  const ctx=canvas.getContext('2d');
  ctx.scale(scale,scale);
  ctx.fillStyle='#fff';ctx.fillRect(0,0,595,842);
  ctx.fillStyle='#19122f';ctx.fillRect(28,28,539,100);
  ctx.fillStyle='#e5fb8e';ctx.font='bold 36px "Segoe UI", sans-serif';ctx.fillText('VÉNUE',52,73);
  ctx.fillStyle='#fff';ctx.font='17px "Segoe UI", sans-serif';ctx.fillText('Vé điện tử',53,103);
  ctx.fillStyle='#19122f';
  let y=173;
  function line(value,size=17,bold=false) {
    ctx.font=(bold?'bold ':'')+size+'px "Segoe UI", sans-serif';
    const words=String(value).split(/\s+/);
    let text='';
    for (const word of words) {
      const next=text?text+' '+word:word;
      if (text&&ctx.measureText(next).width>485) {ctx.fillText(text,52,y,485);y+=size+8;text=word;}
      else text=next;
    }
    ctx.fillText(text,52,y,485);
    y+=size+12;
  }
  const event=lookup(db.events,ticket.eventId);
  const ses=sessionOf(event,ticket.sessionId);
  line(eventTitle(ticket.eventId),25,true);
  line(fmt(ses?.start)+' · '+venueName(ses?.venueId));
  line(ticket.type+' · '+ticket.zone+' · Ghế '+ticket.seat,17,true);
  line(status[ticket.state]?.[0]||ticket.state,15);
  const qr=qrcode(0,'M');qr.addData(ticket.id);qr.make();
  const size=6,count=qr.getModuleCount(),left=52+4*size,top=y+4*size;
  for (let row=0;row<count;row++) for (let col=0;col<count;col++) {
    if (qr.isDark(row,col)) ctx.fillRect(left+col*size,top+row*size,size,size);
  }
  y=top+(count+4)*size+19;
  line(ticket.id,16,true);
  line('Xuất trình mã QR tại cổng và vào đúng khu vực ghi trên vé.',15);
  const image=Uint8Array.from(atob(canvas.toDataURL('image/jpeg',.96).split(',')[1]),char=>char.charCodeAt(0));
  download('VENUE-'+ticket.id+'.pdf',singleImagePdf(image,canvas.width,canvas.height),'application/pdf');
}

function singleImagePdf(image,width,height) {
  const encoder=new TextEncoder();
  const parts=[];
  const offsets=[0];
  let length=0;
  function add(part) {
    const bytes=typeof part==='string'?encoder.encode(part):part;
    parts.push(bytes);
    length+=bytes.length;
  }
  function object(...content) {
    const number=offsets.length;
    offsets.push(length);
    add(number+' 0 obj\n');
    content.forEach(add);
    add('\nendobj\n');
  }
  add('%PDF-1.4\n');
  object('<< /Type /Catalog /Pages 2 0 R >>');
  object('<< /Type /Pages /Kids [3 0 R] /Count 1 >>');
  object('<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /XObject << /Ticket 4 0 R >> >> /Contents 5 0 R >>');
  object(`<< /Type /XObject /Subtype /Image /Width ${width} /Height ${height} /BitsPerComponent 8 /ColorSpace /DeviceRGB /Filter /DCTDecode /Length ${image.length} >>\nstream\n`,image,'\nendstream');
  const contents='q 595 0 0 842 0 0 cm /Ticket Do Q\n';
  object('<< /Length '+contents.length+' >>\nstream\n'+contents+'endstream');
  const xref=length;
  add('xref\n0 '+offsets.length+'\n0000000000 65535 f \n'+offsets.slice(1).map(offset=>String(offset).padStart(10,'0')+' 00000 n \n').join(''));
  add('trailer\n<< /Size '+offsets.length+' /Root 1 0 R >>\nstartxref\n'+xref+'\n%%EOF\n');
  return new Blob(parts);
}

const xmlHead='<?xml version="1.0" encoding="UTF-8" standalone="yes"?>';
const relNs='http://schemas.openxmlformats.org/package/2006/relationships';
const officeRel='http://schemas.openxmlformats.org/officeDocument/2006/relationships';
const cleanXml=value=>esc(String(value??'').replace(/[\u0000-\u0008\u000B\u000C\u000E-\u001F\uFFFE\uFFFF]/g,''));

function officeFile(entries) {
  const encoder=new TextEncoder();
  const files=[],central=[];
  let offset=0;
  for (const [name,content] of Object.entries(entries)) {
    const filename=encoder.encode(name),bytes=encoder.encode(content);
    let crc=0xffffffff;
    for (const byte of bytes) {
      crc^=byte;
      for (let bit=0;bit<8;bit++) crc=(crc>>>1)^((crc&1)?0xedb88320:0);
    }
    crc=(~crc)>>>0;
    const local=new Uint8Array(30+filename.length);
    const l=new DataView(local.buffer);
    l.setUint32(0,0x04034b50,true);l.setUint16(4,20,true);l.setUint16(12,33,true);l.setUint32(14,crc,true);
    l.setUint32(18,bytes.length,true);l.setUint32(22,bytes.length,true);l.setUint16(26,filename.length,true);
    local.set(filename,30);
    const entry=new Uint8Array(46+filename.length);
    const c=new DataView(entry.buffer);
    c.setUint32(0,0x02014b50,true);c.setUint16(4,20,true);c.setUint16(6,20,true);c.setUint16(14,33,true);
    c.setUint32(16,crc,true);c.setUint32(20,bytes.length,true);c.setUint32(24,bytes.length,true);c.setUint16(28,filename.length,true);
    c.setUint32(42,offset,true);entry.set(filename,46);
    files.push(local,bytes);central.push(entry);
    offset+=local.length+bytes.length;
  }
  const end=new Uint8Array(22);
  const e=new DataView(end.buffer);
  e.setUint32(0,0x06054b50,true);e.setUint16(8,central.length,true);e.setUint16(10,central.length,true);
  e.setUint32(12,central.reduce((sum,x)=>sum+x.length,0),true);e.setUint32(16,offset,true);
  return new Blob([...files,...central,end]);
}

function relationships(items) {
  return xmlHead+`<Relationships xmlns="${relNs}">${items.map(([type,target],i)=>
    `<Relationship Id="rId${i+1}" Type="${officeRel}/${type}" Target="${target}"/>`).join('')}</Relationships>`;
}
function contentTypes(types) {
  return xmlHead+`<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
    <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
    <Default Extension="xml" ContentType="application/xml"/>${types.map(([part,kind])=>
      `<Override PartName="/${part}" ContentType="application/vnd.openxmlformats-officedocument.${kind}+xml"/>`).join('')}</Types>`;
}

function excelReport(sections) {
  const sheetNs='http://schemas.openxmlformats.org/spreadsheetml/2006/main';
  const files={
    '[Content_Types].xml':contentTypes([['xl/workbook.xml','spreadsheetml.sheet.main'],...sections.map((_,i)=>['xl/worksheets/sheet'+(i+1)+'.xml','spreadsheetml.worksheet'])]),
    '_rels/.rels':relationships([['officeDocument','xl/workbook.xml']]),
    'xl/workbook.xml':xmlHead+`<workbook xmlns="${sheetNs}" xmlns:r="${officeRel}"><sheets>${sections.map((section,i)=>
      `<sheet name="${i+1} ${cleanXml(section.title.replace(/[\[\]:*?\/\\]/g,' ').slice(0,25).replace(/'+$/,''))}" sheetId="${i+1}" r:id="rId${i+1}"/>`).join('')}</sheets></workbook>`,
    'xl/_rels/workbook.xml.rels':relationships(sections.map((_,i)=>['worksheet','worksheets/sheet'+(i+1)+'.xml']))
  };
  sections.forEach((section,index)=>{
    const rows=[section.headers,...section.rows];
    files['xl/worksheets/sheet'+(index+1)+'.xml']=xmlHead+`<worksheet xmlns="${sheetNs}"><sheetViews><sheetView workbookViewId="0"><pane ySplit="1" topLeftCell="A2" activePane="bottomLeft" state="frozen"/></sheetView></sheetViews>
    <cols><col min="1" max="${section.headers.length}" width="23" customWidth="1"/></cols><sheetData>${rows.map((row,i)=>
      `<row r="${i+1}">${row.map(value=>`<c t="inlineStr"><is><t xml:space="preserve">${cleanXml(value)}</t></is></c>`).join('')}</row>`).join('')}</sheetData></worksheet>`;
  });
  return officeFile(files);
}

function wordReport(title,sections) {
  const paragraph=(value,bold=false)=>`<w:p><w:r>${bold?'<w:rPr><w:b/></w:rPr>':''}<w:t xml:space="preserve">${cleanXml(value)}</w:t></w:r></w:p>`;
  return officeFile({
    '[Content_Types].xml':contentTypes([['word/document.xml','wordprocessingml.document.main']]),
    '_rels/.rels':relationships([['officeDocument','word/document.xml']]),
    'word/document.xml':xmlHead+`<w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"><w:body>
      ${paragraph(title,true)}${paragraph(fmt(Date.now()))}${sections.map(section=>paragraph(section.title,true)+`<w:tbl>
      <w:tblPr><w:tblW w:w="0" w:type="auto"/><w:tblBorders><w:bottom w:val="single" w:sz="6"/><w:insideH w:val="single" w:sz="4" w:color="DDDDDD"/></w:tblBorders></w:tblPr>
      <w:tblGrid>${section.headers.map(()=>'<w:gridCol/>').join('')}</w:tblGrid>
      ${[section.headers,...section.rows].map((row,i)=>`<w:tr>${i===0?'<w:trPr><w:tblHeader/></w:trPr>':''}${row.map(cell=>'<w:tc>'+paragraph(cell,i===0)+'</w:tc>').join('')}</w:tr>`).join('')}</w:tbl>`).join('')}
      </w:body></w:document>`
  });
}

function printReport(title,sections) {
  const body=`<h1>${esc(title)}</h1><p>${esc(fmt(Date.now()))}</p>${sections.map(section=>`<h2>${esc(section.title)}</h2>
    <table><thead><tr>${section.headers.map(h=>'<th>'+esc(h)+'</th>').join('')}</tr></thead><tbody>
    ${section.rows.map(row=>'<tr>'+row.map(value=>'<td>'+esc(value)+'</td>').join('')+'</tr>').join('')}</tbody></table>`).join('')}`;
  printDocument(title,body,'@page{size:A4 landscape;margin:14mm}body{margin:0}h2{font-size:1.1rem;break-after:avoid}table{width:100%;border-collapse:collapse;font-size:.78rem}th,td{border:1px solid #ccc;padding:6px;text-align:left;overflow-wrap:anywhere}tr{break-inside:avoid}thead{display:table-header-group}');
}

