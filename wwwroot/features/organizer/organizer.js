import { action, dataTable, element, facts, heading, layout, notice, openDialog, panel, routeLink, tabLinks } from '../../shared/ui/view.js';
import { field, fieldGrid, fieldset, finishForm, staticForm } from '../../shared/ui/forms.js';
import { recordPath, timeLabel } from '../../shared/ui/catalog.js';
import { slot } from '../../shared/ui/templates.js';
import { workspace } from '../../shared/ui/workspace.js';
import { enabled, endpoint, items, read, write } from '../../app/api.js';
import { todayInVietnam } from '../../shared/date.js';

const needAccount = 'Cần kết nối tài khoản và dịch vụ hồ sơ Ban tổ chức.';
const states = { DRAFT: 'Bản nháp', PENDING: 'Chờ duyệt', NEEDS_INFO: 'Yêu cầu bổ sung', APPROVED: 'Đã duyệt', REJECTED: 'Từ chối' };
const types = {
  pdf: ['application/pdf', [37, 80, 68, 70, 45]],
  png: ['image/png', [137, 80, 78, 71, 13, 10, 26, 10]],
  jpg: ['image/jpeg', [255, 216, 255]],
  jpeg: ['image/jpeg', [255, 216, 255]]
};

async function fileProblem(file) {
  const extension = file.name.split('.').pop().toLowerCase();
  if (!Object.hasOwn(types, extension)) return 'Chọn PDF, PNG, JPG hoặc JPEG.';
  if (!file.size || file.size > 5 * 1024 * 1024) return 'Tệp phải có nội dung và tối đa 5 MB.';
  const [, signature] = types[extension];
  const header = new Uint8Array(await file.slice(0, signature.length).arrayBuffer());
  return signature.every((byte, index) => byte === header[index]) ? '' : 'Nội dung tệp không khớp phần mở rộng.';
}

function attachments(dialogHost, previews, alive, label, name, options = {}) {
  const item = field(label, { name, type: 'file', accept: '.pdf,.png,.jpg,.jpeg', required: true, ...options });
  const input = item.control;
  let validFile;
  let checkedFile;
  let revision = 0;
  const show = action('Xem trước', () => {
    if (!validFile || !alive()) return;
    const [mime] = types[validFile.name.split('.').pop().toLowerCase()];
    const fileName = validFile.name;
    const url = URL.createObjectURL(new Blob([validFile], { type: mime }));
    previews.add(url);
    const preview = element(mime === 'application/pdf' ? 'iframe' : 'img', '', 'file-preview');
    preview.src = url;
    if (preview.tagName === 'IMG') preview.alt = fileName;
    else {
      preview.title = 'Xem trước ' + fileName;
      preview.tabIndex = -1;
    }
    const dialog = openDialog(dialogHost, show, 'Xem trước giấy tờ',
      layout('stack', element('p', fileName), preview), () => {
        URL.revokeObjectURL(url);
        previews.delete(url);
      });
    dialog.classList.add('preview-dialog');
  }, { disabled: true });
  show.setAttribute('aria-label', 'Xem trước ' + label);
  const remove = action('Xóa tệp', () => {
    item.clear();
    input.focus();
  }, { disabled: true });
  remove.setAttribute('aria-label', 'Xóa ' + label);
  item.wrapper.append(layout('actions', show, remove));

  item.clear = () => {
    revision++;
    input.value = '';
    validFile = undefined;
    checkedFile = undefined;
    show.disabled = true;
    remove.disabled = true;
    item.showError('');
  };
  input.addEventListener('change', async () => {
    const upload = input.files[0];
    const checkRevision = ++revision;
    validFile = undefined;
    checkedFile = undefined;
    show.disabled = true;
    remove.disabled = !upload;
    item.showError('');
    if (!upload) return;
    const error = await fileProblem(upload).catch(() => 'Không đọc được nội dung tệp.');
    if (!alive() || revision !== checkRevision) return;
    checkedFile = upload;
    validFile = error ? undefined : upload;
    item.showError(error);
    show.disabled = !validFile;
  });
  const requiredCheck = item.check;
  item.check = () => {
    if (input.matches(':disabled') || !input.files.length) return requiredCheck();
    if (validFile === input.files[0]) return true;
    if (checkedFile !== input.files[0]) item.showError('Đang kiểm tra tệp. Vui lòng thử lại.');
    return false;
  };
  return item;
}

function registration(target, context) {
  const { api, profile } = context;
  let active = true;
  const previews = new Set();
  const form = staticForm();
  const documentField = (...args) => attachments(target, previews, () => active, ...args);
  const required = (label, name, options = {}) => field(label, { name, required: true, maxLength: 180, ...options });
  const individual = field('Cá nhân', { type: 'radio', name: 'registrationType', value: 'individual', checked: true });
  const organization = field('Doanh nghiệp / Tổ chức', { type: 'radio', name: 'registrationType', value: 'organization' });
  const idType = required('Loại giấy tờ', 'identityType', { choices: [['card', 'CCCD'], ['passport', 'Hộ chiếu']] });
  const idNumber = required('Số CCCD / Hộ chiếu', 'identityNumber', { maxLength: 12,
    validate: value => idType.control.value === 'card' ? (/^\d{12}$/.test(value) ? '' : 'Số CCCD phải có 12 chữ số.')
      : (/^(?=.*\d)[A-Z\d]{6,12}$/i.test(value) ? '' : 'Nhập số hộ chiếu gồm 6–12 chữ hoặc số, có ít nhất một chữ số.') });
  const tax = value => /^\d{10}(?:-?\d{3})?$/.test(value) ? '' : 'Nhập mã số thuế 10 hoặc 13 chữ số.';
  const individualFields = [
    required('Họ và tên theo CCCD / Hộ chiếu', 'fullName', { wide: true, value: profile?.name ?? '', autocomplete: 'name' }), idType, idNumber,
    required('Ngày cấp', 'issuedDate', { type: 'date', max: todayInVietnam().toISOString().slice(0, 10) }),
    required('Nơi cấp', 'issuedPlace'),
    field('Mã số thuế cá nhân (không bắt buộc)', { name: 'personalTax', maxLength: 14,
      validate: value => value && !/^\d{12}$/.test(value) ? tax(value) : '' })
  ];
  const organizationFields = [required('Tên doanh nghiệp / tổ chức theo GPKD', 'organizationName', { wide: true }),
    required('Mã số thuế', 'organizationTax', { maxLength: 14, validate: tax }), required('Người đại diện theo pháp luật', 'representative'),
    required('Chức vụ', 'position'), required('Địa chỉ trụ sở đăng ký', 'headOffice', { wide: true })];
  const individualId = fieldset('1. Định danh cá nhân', fieldGrid(individualFields));
  const organizationId = fieldset('1. Định danh doanh nghiệp / tổ chức', fieldGrid(organizationFields));
  const contact = [required('Email liên hệ chính', 'email', { type: 'email', value: profile?.email ?? '', autocomplete: 'email' }),
    required('Số điện thoại', 'phone', { type: 'tel', value: profile?.phone ?? '', autocomplete: 'tel', validate: input =>
      /^\+?[\d ().-]+$/.test(input) && input.replace(/\D/g, '').length >= 9 ? '' : 'Nhập số điện thoại hợp lệ.' }),
    required('Địa chỉ liên hệ thực tế', 'contactAddress', { wide: true }),
    field('Website / Fanpage sự kiện (không bắt buộc)', { name: 'website', type: 'url', wide: true,
      hint: 'Nhập địa chỉ đầy đủ, bắt đầu bằng https:// hoặc http://.',
      validate: value => value && !/^https?:\/\//i.test(value) ? 'Dùng địa chỉ HTTP hoặc HTTPS.' : '' })];
  const canonical = value => value.normalize('NFD').replace(/\p{Diacritic}/gu, '').replace(/[đĐ]/g, 'd')
    .toLowerCase().trim().replace(/\s+/g, ' ');
  const payment = [required('Tên ngân hàng', 'bankName'), field('Chi nhánh (không bắt buộc)', { name: 'bankBranch' }),
    required('Số tài khoản ngân hàng', 'bankAccount', { inputMode: 'numeric', maxLength: 34,
      validate: value => /^\d+$/.test(value) ? '' : 'Số tài khoản chỉ gồm chữ số.' }),
    required('Tên chủ tài khoản', 'bankOwner', { hint: 'Nhập tên tương ứng với tên cá nhân hoặc tổ chức đã khai.',
      validate: value => {
        const legalName = (individual.control.checked ? individualFields[0] : organizationFields[0]).control.value;
        return !legalName || canonical(value) === canonical(legalName) ? '' : 'Tên chủ tài khoản phải khớp tên pháp lý.';
      } })];
  const front = documentField('Ảnh CCCD mặt trước', 'identityFront');
  const back = documentField('Ảnh CCCD mặt sau', 'identityBack');
  const passport = documentField('Trang thông tin hộ chiếu', 'passport');
  const business = documentField('Giấy chứng nhận đăng ký doanh nghiệp', 'businessLicense');
  const authorization = documentField('Giấy ủy quyền', 'authorization', { required: false,
    hint: 'Đính kèm nếu người nộp khác người đại diện theo pháp luật.' });
  const personalFiles = fieldset('4. Giấy tờ cá nhân', fieldGrid([front, back, passport]));
  const companyFiles = fieldset('4. Giấy tờ tổ chức', fieldGrid([business, authorization]));
  const consent = required('Tôi cam kết thông tin chính xác và đồng ý với trách nhiệm, điều khoản tài chính', 'agreement', { type: 'checkbox' });
  const terms = (label, text) => {
    const link = action(label, () => openDialog(target, link, label, element('p', text)));
    return link;
  };
  function updateBranch() {
    const isIndividual = individual.control.checked;
    const card = idType.control.value === 'card';
    for (const [fieldset, show] of [[individualId, isIndividual], [personalFiles, isIndividual],
      [organizationId, !isIndividual], [companyFiles, !isIndividual]]) {
      fieldset.hidden = !show;
      fieldset.disabled = !show;
    }
    for (const [item, show] of [[front, isIndividual && card], [back, isIndividual && card],
      [passport, isIndividual && !card], [business, !isIndividual], [authorization, !isIndividual]]) {
      if (!show) item.clear();
      item.wrapper.hidden = !show;
      item.control.disabled = !show;
    }
    if (idNumber.control.hasAttribute('aria-invalid')) idNumber.check();
  }
  form.addEventListener('change', event => {
    if (['registrationType', 'identityType'].includes(event.target.name)) updateBranch();
  });
  updateBranch();

  form.append(fieldset('Loại hình đăng ký', layout('actions', individual.wrapper, organization.wrapper)),
    individualId, organizationId, fieldset('2. Liên hệ và vận hành', fieldGrid(contact)),
    fieldset('3. Tài khoản nhận thanh toán', element('p', 'Dùng để đối soát và nhận doanh thu sự kiện.', 'field-hint'), fieldGrid(payment)),
    element('p', 'Giấy tờ đính kèm ở định dạng PDF, PNG, JPG hoặc JPEG, tối đa 5 MB mỗi tệp.', 'field-hint'),
    personalFiles, companyFiles, consent.wrapper, layout('actions',
      terms('Trách nhiệm Ban tổ chức', 'Cung cấp giấy tờ chính xác, bảo đảm điều kiện tổ chức và quyền lợi vé. Sự kiện và thay đổi ảnh hưởng người mua cần được duyệt.'),
      terms('Điều khoản tài chính', 'Doanh thu được đối soát theo giao dịch thành công, khoản hoàn, phí hợp đồng và khoản điều chỉnh đã được duyệt.')));
  const ready = enabled(api, 'organizer.submit', true);
  finishForm(form, [...individualFields, ...organizationFields, ...contact, ...payment, front, back, passport, business, authorization, consent],
    ready ? '' : needAccount + ' Tệp chỉ mở trong trình duyệt khi bạn chọn xem trước.', 'Gửi hồ sơ xét duyệt', ready ? async () => {
      const data = new FormData(form);
      for (const key of ['identityFront', 'identityBack', 'passport', 'businessLicense', 'authorization']) {
        if (!data.get(key)?.size) data.delete(key);
      }
      const result = await write(api, 'organizer.submit', endpoint('me', 'organizer-applications'), data);
      if (active) location.hash = result?.id ? recordPath('organizer/status', result.id) : 'organizer/status';
    } : undefined);
  return { form, dispose() {
    active = false;
    [front, back, passport, business, authorization].forEach(item => item.clear());
    for (const url of previews) URL.revokeObjectURL(url);
    previews.clear();
    form.reset();
  } };
}

export async function renderOrganizer(target, segments, query, context) {
  const { view, api, signal } = context;
  const [mode = 'apply', id] = segments;
  if (!['apply', 'status'].includes(mode) || segments.length > (mode === 'status' ? 2 : 1)) return;
  const tabs = [['Khai hồ sơ', 'organizer/apply'], ['Trạng thái và lịch sử', 'organizer/status']];
  slot(view, 'tabs', tabLinks('Hồ sơ Ban tổ chức', tabs, 'organizer/' + mode));
  let dispose;
  if (mode === 'apply') {
    if (api && !api.authenticated) {
      target.append(heading('Đăng ký Ban tổ chức', 'Chuẩn bị thông tin pháp lý, liên hệ, tài khoản ngân hàng và giấy tờ đính kèm.'),
        panel('Đăng nhập để nộp hồ sơ', element('p', 'Hồ sơ được gắn với tài khoản để bạn theo dõi xét duyệt và nhận quyền tổ chức sự kiện.'),
          routeLink('Đăng nhập', 'account/login?next=organizer%2Fapply', 'button')));
      return;
    }
    const profile = await read(api, 'account.profile', endpoint('me'), { signal });
    const application = registration(target, { ...context, profile });
    dispose = application.dispose;
    slot(view, 'application', panel('Đăng ký Ban tổ chức',
      element('p', 'Quản lý hệ thống xét duyệt trong tối đa 7 ngày làm việc. Kết quả và yêu cầu bổ sung xuất hiện trong lịch sử.'),
      application.form));
  } else {
    const response = await read(api, 'organizer.applications', endpoint('me', 'organizer-applications'), { signal });
    const records = items(response);
    const current = id ? await read(api, 'organizer.application', endpoint('organizer-applications', id),
      { signal, missingRecord: true }) : undefined;
    const status = (state, label) => states[state] ?? label;
    const history = current?.history;
    slot(view, 'status', layout('stack',
      panel('Hồ sơ đã gửi',
        dataTable('Danh sách hồ sơ', ['Hồ sơ', 'Ngày gửi', 'Trạng thái'], records?.map(item => [
          routeLink(item.name || 'Xem hồ sơ', recordPath('organizer/status', item.id)),
          timeLabel(item.submittedAt), status(item.status, item.statusLabel)
        ]), needAccount, 'Bạn chưa gửi hồ sơ Ban tổ chức.')),
      panel('Trạng thái hồ sơ',
        ...(current ? [facts([['Trạng thái', status(current.status, current.statusLabel)],
          ['Ngày gửi', timeLabel(current.submittedAt)], ['Lý do / yêu cầu bổ sung', current.reason]])]
          : [notice(current === null ? 'Hồ sơ không còn khả dụng.' :
            records?.length ? 'Chọn một hồ sơ để xem trạng thái và lý do xét duyệt.' : needAccount)]),
        ...(['NEEDS_INFO', 'REJECTED'].includes(current?.status) ? [routeLink('Chuẩn bị hồ sơ nộp lại', 'organizer/apply', 'button')] : [])),
      panel('Lịch sử xét duyệt', dataTable('Các lần cập nhật', ['Thời điểm', 'Trạng thái', 'Lý do'],
        history?.map(item => [timeLabel(item.at), status(item.status, item.statusLabel), item.note]),
        needAccount, 'Hồ sơ chưa có cập nhật xét duyệt.'))));
  }
  target.append(heading('Ban tổ chức', 'Đăng ký, bổ sung giấy tờ và theo dõi kết quả xét duyệt.'), workspace('organizer', layout('stack', view)));
  return dispose;
}
