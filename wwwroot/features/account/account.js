import { action, dataTable, element, facts, heading, layout, notice, openDialog, panel, routeLink } from '../../shared/ui/view.js';
import { field, fieldGrid, finishForm, formValues, staticForm } from '../../shared/ui/forms.js';
import { recordPath, timeLabel } from '../../shared/ui/catalog.js';
import { slot } from '../../shared/ui/templates.js';
import { accountNav } from '../../shared/ui/workspace.js';
import { enabled, endpoint, items, read, refreshView, resetCapabilities, write } from '../../app/api.js';

const routes = [['Đăng nhập', 'login'], ['Đăng ký', 'register'], ['Khôi phục mật khẩu', 'forgot'],
  ['Hồ sơ', 'profile'], ['Thông báo', 'notifications'], ['Sự kiện yêu thích', 'favorites']];
const authUnavailable = 'Dịch vụ xác thực hiện chưa khả dụng. Vui lòng thử lại sau.';
const personalUnavailable = 'Cần phiên đăng nhập và quyền truy cập dữ liệu tài khoản.';
const identifier = value => /^([\w.!#$%&'*+/=?^`{|}~-]+@[^\s@]+\.[^\s@]+|\+?\d[\d ().-]{7,19})$/u.test(value)
  ? '' : 'Nhập email hoặc số điện thoại hợp lệ.';
const phone = value => /^\+?[\d ().-]+$/.test(value) && value.replace(/\D/g, '').length >= 8 && value.replace(/\D/g, '').length <= 15
  ? '' : 'Nhập số điện thoại hợp lệ.';

function password(label, name, options = {}) {
  const item = field(label, { name, type: 'password', required: true, maxLength: 128, ...options });
  const toggle = action('Hiện', () => {
    const reveal = item.control.type === 'password';
    item.control.type = reveal ? 'text' : 'password';
    toggle.textContent = reveal ? 'Ẩn' : 'Hiện';
    toggle.setAttribute('aria-pressed', String(reveal));
    toggle.setAttribute('aria-label', reveal ? 'Ẩn mật khẩu' : 'Hiện mật khẩu');
  });
  toggle.className = 'password-toggle';
  toggle.setAttribute('aria-label', 'Hiện mật khẩu');
  toggle.setAttribute('aria-controls', item.control.id);
  toggle.setAttribute('aria-pressed', 'false');
  const controls = layout('password-control');
  item.control.before(controls);
  controls.append(item.control, toggle);
  return item;
}

async function authContent(mode, api, signal, query) {
  const form = staticForm();
  form.autocomplete = 'on';
  form.classList.add('auth-form');
  const operation = { login: 'auth.login', register: 'auth.register', forgot: 'auth.reset' }[mode];
  const path = endpoint('auth', { login: 'session', register: 'register', forgot: 'password-resets' }[mode]);
  let fields;
  let policy;
  const links = layout('actions');
  const next = query.get('next');
  const authLink = (label, route) => routeLink(label, 'account/' + route + (next ? '?' + new URLSearchParams({ next }) : ''));
  if (mode === 'register') {
    const policies = await read(api, 'catalog.policies', endpoint('policies'), {
      signal, query: new URLSearchParams({ type: 'registration' })
    });
    policy = items(policies)?.[0];
    const value = password('Mật khẩu', 'password', { minLength: 8, autocomplete: 'new-password', hint: 'Tối thiểu 8 ký tự. Máy chủ áp dụng chính sách tại thời điểm đăng ký.' });
    const confirm = password('Nhập lại mật khẩu', 'confirm', { autocomplete: 'new-password',
      validate: input => input === value.control.value ? '' : 'Hai mật khẩu phải trùng khớp.'
    });
    fields = [
      field('Họ và tên', { name: 'name', required: true, maxLength: 100, autocomplete: 'name' }),
      field('Tên đăng nhập', { name: 'userName', required: true, maxLength: 50, autocomplete: 'username', spellcheck: false, validate: value => /^[\p{L}\p{N}_.-]+$/u.test(value) ? '' : 'Chỉ dùng chữ, số, dấu chấm, gạch ngang hoặc gạch dưới.' }),
      field('Email', { name: 'email', type: 'email', maxLength: 254, autocomplete: 'email' }),
      field('Số điện thoại', { name: 'phone', type: 'tel', maxLength: 20, autocomplete: 'tel', validate: input => input ? phone(input) : '' }),
      value, confirm,
      field('Tôi đồng ý với điều khoản sử dụng QLSK', { name: 'terms', type: 'checkbox', wide: true, required: true })
    ];
    links.append(routeLink('Xem điều khoản sử dụng', 'guide/rules'), authLink('Đã có tài khoản? Đăng nhập', 'login'));
  } else {
    fields = [field('Email hoặc số điện thoại', { name: 'identifier', required: true,
      maxLength: 254, spellcheck: false, autocomplete: 'username', validate: value => identifier(value.trim()) })];
    if (mode === 'login') {
      fields.push(password('Mật khẩu', 'password', { autocomplete: 'current-password' }));
      links.append(authLink('Quên mật khẩu?', 'forgot'));
    } else links.append(authLink('Nhập mã khôi phục', 'reset'), authLink('Quay lại đăng nhập', 'login'));
  }
  const buttonLabel = { login: 'Đăng nhập', register: 'Đăng ký', forgot: 'Gửi mã khôi phục' }[mode];
  const ready = enabled(api, operation, true) && (mode !== 'register' || Boolean(policy?.id));
  const unavailable = mode === 'register' && enabled(api, operation, true) && !policy?.id
    ? 'Chưa tải được điều khoản đăng ký đã công bố. Vui lòng thử lại sau.'
    : mode === 'forgot' && api && !enabled(api, operation, true)
      ? 'Chức năng gửi email khôi phục chưa được cấu hình. Vui lòng liên hệ bộ phận hỗ trợ.' : authUnavailable;
  const confirmation = element('p', '', 'field-hint');
  confirmation.setAttribute('role', 'status');
  const grid = fieldGrid(fields);
  if (mode === 'login') grid.classList.add('login-fields');
  form.append(grid, links, confirmation);
  const body = panel(routes.find(([, key]) => key === mode)[0],
    element('p', mode === 'forgot' ? 'Mã dùng một lần được gửi đến email đã xác minh và có hiệu lực trong 30 phút.'
      : mode === 'register' ? 'Nhập ít nhất một kênh liên hệ. Tài khoản mới có vai trò Người dùng.'
      : 'Đăng nhập để xem vé, đơn hàng và thông tin cá nhân.', 'muted'), form);
  finishForm(form, fields, ready ? '' : unavailable, buttonLabel, ready ? async () => {
    const { confirm, terms, ...data } = formValues(form);
    for (const name of ['name', 'userName', 'email', 'phone', 'identifier']) {
      if (typeof data[name] === 'string') data[name] = data[name].trim();
    }
    if (mode === 'register') {
      if (!data.email && !data.phone) {
        fields[2].showError('Nhập email hoặc số điện thoại.');
        fields[2].control.focus();
        throw new Error('Thiếu kênh liên hệ.');
      }
      data.acceptedPolicyId = policy.id;
    }
    await write(api, operation, path, data);
    if (signal.aborted) return;
    form.reset();
    if (mode === 'login') {
      resetCapabilities();
      const permitted = next && /^(?:home|events?|calendar|venues?|guide|account|tickets|requests|organizer|work|export)(?:[\/?]|$)/.test(next);
      location.hash = permitted ? next : 'account/profile';
    }
    if (mode === 'forgot') confirmation.textContent = 'Nếu liên hệ phù hợp với tài khoản đã xác minh, mã khôi phục sẽ được gửi đến email của bạn.';
    if (mode === 'register') confirmation.textContent = 'Đã tạo tài khoản. Đăng nhập để mở hồ sơ.';
  } : undefined);
  body.classList.add('auth-panel');
  const privacy = element('p', 'Thông tin được dùng để bảo vệ tài khoản và xử lý yêu cầu của bạn. ', 'auth-privacy');
  privacy.append(routeLink('Chính sách bảo mật', 'guide/privacy'));
  if (mode === 'login') {
    body.append(layout('login-footer', 'Chưa có tài khoản? ', authLink('Tạo tài khoản', 'register'),
      routeLink('Chính sách bảo mật', 'guide/privacy', 'auth-policy-link')));
  } else body.append(privacy);
  return body;
}

async function personalContent(mode, api, signal) {
  if (mode === 'profile') {
    const [profile, history] = await Promise.all([
      read(api, 'account.profile', endpoint('me'), { signal }),
      read(api, 'account.viewHistory', endpoint('me', 'view-history'), { signal })
    ]);
    if (!profile) return panel('Thông tin cá nhân', notice(personalUnavailable), routeLink('Đăng nhập lại', 'account/login', 'button'));
    const content = layout('account-layout');
    let editDialog;
    let passwordDialog;
    const form = staticForm();
    form.autocomplete = 'on';
    const fields = [
      field('Họ và tên', { name: 'name', value: profile?.name ?? '', required: true, maxLength: 100, autocomplete: 'name' }),
      field('Email', { name: 'email', type: 'email', value: profile?.email ?? '', autocomplete: 'email', maxLength: 254 }),
      field('Số điện thoại', { name: 'phone', type: 'tel', value: profile?.phone ?? '', autocomplete: 'tel', maxLength: 20, validate: value => value ? phone(value) : '' })
    ];
    form.append(fieldGrid(fields));
    const ready = profile && enabled(api, 'account.updateProfile', true);
    finishForm(form, fields, ready ? '' : personalUnavailable, 'Lưu hồ sơ', ready
      ? async () => {
        await write(api, 'account.updateProfile', endpoint('me'), formValues(form), 'PUT');
        editDialog.close();
        if (!signal.aborted) refreshView();
      } : undefined);
    const change = staticForm();
    const current = password('Mật khẩu hiện tại', 'currentPassword', { autocomplete: 'current-password' });
    const next = password('Mật khẩu mới', 'newPassword', { minLength: 8, autocomplete: 'new-password' });
    const confirm = password('Nhập lại mật khẩu mới', 'confirm', { autocomplete: 'new-password',
      validate: value => value === next.control.value ? '' : 'Hai mật khẩu phải trùng khớp.' });
    change.append(fieldGrid([current, next, confirm]));
    const canChange = profile && enabled(api, 'account.changePassword', true);
    finishForm(change, [current, next, confirm], canChange ? '' : authUnavailable, 'Đổi mật khẩu', canChange ? async () => {
      await write(api, 'account.changePassword', endpoint('me', 'password'), { currentPassword: current.control.value, newPassword: next.control.value }, 'PUT');
      change.reset();
      passwordDialog.close();
      if (!signal.aborted) refreshView();
    } : undefined);
    const logout = action('Đăng xuất', async () => {
      logout.disabled = true;
      try {
        await write(api, 'auth.logout', endpoint('auth', 'session'), undefined, 'DELETE');
        resetCapabilities();
        location.hash = 'account/login';
      } catch (error) {
        logout.insertAdjacentElement('afterend', notice(error.message));
        logout.disabled = false;
      }
    }, { disabled: !profile || !enabled(api, 'auth.logout', true) });
    const edit = action('Cập nhật hồ sơ', () => {
      fields.forEach(item => { item.control.value = profile[item.control.name] ?? ''; });
      fields.forEach(item => item.showError(''));
      editDialog = openDialog(content, edit, 'Cập nhật hồ sơ', form);
      fields[0].control.focus();
    }, { style: '', disabled: !ready });
    const changePassword = action('Đổi mật khẩu', () => {
      change.reset();
      [current, next, confirm].forEach(item => item.showError(''));
      passwordDialog = openDialog(content, changePassword, 'Đổi mật khẩu', change);
      current.control.focus();
    }, { disabled: !canChange });
    const identity = layout('account-identity', layout('stack', element('h3', profile.name),
      element('p', [profile.email, profile.phone].filter(Boolean).join(' · ') || 'Chưa có liên hệ', 'muted')),
      element('span', profile.roleLabel, 'account-role'));
    const summary = panel('Thông tin cá nhân', identity, facts([['Tên đăng nhập', profile.userName],
      ['Xác minh liên hệ', profile.verificationLabel]]),
      layout('actions', edit, changePassword, logout), routeLink('Xác minh liên hệ', 'account/verify'));
    const recentEvents = items(history);
    let historyContent;
    if (recentEvents?.length) {
      historyContent = element('ol', '', 'view-history');
      for (const event of recentEvents) {
        const entry = element('li');
        entry.append(routeLink(event.title, recordPath('event', event.id)));
        historyContent.append(entry);
      }
    } else {
      historyContent = notice(recentEvents ? 'Bạn chưa xem sự kiện nào.' : 'Chưa tải được lịch sử xem.');
    }
    content.append(summary, layout('stack', panel('Lịch sử xem', historyContent,
      routeLink('Khám phá sự kiện', 'events', 'button secondary')),
      panel('Truy cập nhanh', layout('stack', routeLink('Vé của tôi', 'tickets/mine'), routeLink('Đơn hàng', 'tickets/orders'),
        routeLink('Yêu cầu hỗ trợ', 'requests/support'), routeLink('Chính sách bảo mật', 'guide/privacy')))));
    return content;
  }
  if (mode === 'notifications') {
    const records = items(await read(api, 'account.notifications', endpoint('me', 'notifications'), { signal }));
    const feedback = element('p', '', 'field-hint');
    feedback.setAttribute('role', 'status');
    const rows = records?.map(item => {
      const mark = action('Đánh dấu đã đọc', async () => {
        mark.disabled = true;
        try {
          await write(api, 'account.readNotification', endpoint('me', 'notifications', item.id, 'read'), undefined, 'PUT');
          refreshView();
        } catch (error) {
          feedback.textContent = error.message;
          mark.disabled = false;
        }
      }, { disabled: item.read || !enabled(api, 'account.readNotification', true) });
      return [item.title, item.detail, timeLabel(item.createdAt), item.read ? 'Đã đọc' : mark];
    });
    return panel('Hộp thông báo', dataTable('Thông báo của tôi', ['Tiêu đề', 'Nội dung', 'Thời điểm', 'Trạng thái'],
      rows, personalUnavailable, 'Bạn chưa có thông báo.'), feedback);
  }
  const favorites = items(await read(api, 'account.favorites', endpoint('me', 'favorites'), { signal }));
  const feedback = element('p', '', 'field-hint');
  feedback.setAttribute('role', 'status');
  const rows = favorites?.map(event => {
    const remove = action('Bỏ yêu thích', async () => {
      remove.disabled = true;
      try {
        await write(api, 'account.removeFavorite', endpoint('me', 'favorites', event.id), undefined, 'DELETE');
        refreshView();
      } catch (error) {
        feedback.textContent = error.message;
        remove.disabled = false;
      }
    }, { disabled: !enabled(api, 'account.removeFavorite', true) });
    return [routeLink(event.title, recordPath('event', event.id)), event.category, timeLabel(event.nextStart), remove];
  });
  return panel('Sự kiện yêu thích', dataTable('Sự kiện đã lưu', ['Sự kiện', 'Danh mục', 'Suất tiếp theo', 'Thao tác'],
    rows, personalUnavailable, 'Bạn chưa lưu sự kiện yêu thích.'), feedback, routeLink('Khám phá sự kiện', 'events', 'button secondary'));
}

function recovery(api, signal) {
  const form = staticForm();
  const token = field('Mã khôi phục', { name: 'token', type: 'password', required: true, maxLength: 128, autocomplete: 'one-time-code' });
  const pass = password('Mật khẩu mới', 'newPassword', { minLength: 8, autocomplete: 'new-password' });
  const confirm = password('Nhập lại mật khẩu', 'confirm', {
    validate: value => value === pass.control.value ? '' : 'Hai mật khẩu phải trùng khớp.'
  });
  const fields = [token, pass, confirm];
  form.append(fieldGrid(fields));
  const ready = enabled(api, 'auth.confirmReset', true);
  finishForm(form, fields, ready ? '' : authUnavailable, 'Xác nhận đổi mật khẩu', ready ? async () => {
    await write(api, 'auth.confirmReset', endpoint('auth', 'password-resets', 'confirm'), {
      token: token.control.value, newPassword: pass.control.value
    });
    form.reset();
    if (!signal.aborted) location.hash = 'account/login';
  } : undefined);
  return panel('Đặt mật khẩu mới', element('p', 'Nhập mã trong thông báo khôi phục. Mã dùng một lần trong 30 phút.'),
    form, routeLink('Yêu cầu mã mới', 'account/forgot', 'button secondary'));
}

function verification(api, signal) {
  const request = staticForm();
  const channel = field('Kênh xác minh', { name: 'channel', choices: [['email', 'Email'], ['sms', 'SMS']]
    .filter(([name]) => !api || api.verificationChannels?.includes(name)) });
  request.append(channel.wrapper);
  const canRequest = enabled(api, 'account.requestVerification', true);
  finishForm(request, [channel], canRequest ? '' : personalUnavailable, 'Gửi mã xác minh', canRequest
    ? () => write(api, 'account.requestVerification', endpoint('me', 'contact-verifications'), formValues(request)) : undefined);
  const confirm = staticForm();
  const code = field('Mã xác minh', { name: 'code', required: true, maxLength: 16 });
  confirm.append(code.wrapper);
  const canConfirm = enabled(api, 'account.confirmVerification', true);
  finishForm(confirm, [code], canConfirm ? '' : personalUnavailable, 'Xác minh liên hệ', canConfirm ? async () => {
    await write(api, 'account.confirmVerification', endpoint('me', 'contact-verifications', 'confirm'), formValues(confirm));
    confirm.reset();
    if (!signal.aborted) location.hash = 'account/profile';
  } : undefined);
  return panel('Xác minh liên hệ', request, confirm);
}

export async function renderAccount(target, segments, query, { view, api, signal }) {
  const [mode = 'profile'] = segments;
  if (segments.length > 1 || (!['reset', 'verify'].includes(mode) && !routes.some(([, key]) => mode === key))) return;
  const authMode = ['login', 'register', 'forgot', 'reset'].includes(mode);
  const dialogTemplate = view.querySelector('[data-auth-dialog]');
  dialogTemplate.remove();
  if (authMode || !api?.authenticated) {
    const next = query.get('next');
    const suffix = next ? '?' + new URLSearchParams({ next }) : '';
    const gate = layout('panel actions', routeLink('Đăng nhập', 'account/login' + suffix, 'button'),
      routeLink('Tạo tài khoản', 'account/register' + suffix, 'button secondary'));
    target.append(heading('Đăng nhập', 'Đăng nhập để xem vé, đơn hàng và thông tin cá nhân.'), gate);
    if (!authMode) return;

    const dialog = dialogTemplate.content.firstElementChild;
    const content = mode === 'reset' ? recovery(api, signal) : await authContent(mode, api, signal, query);
    content.classList.add('auth-panel');
    content.querySelector('form')?.classList.add('auth-form');
    const title = content.querySelector('h2');
    title.id = 'auth-title';
    dialog.dataset.authMode = mode;
    const close = action('×', () => dialog.close(), { style: 'secondary compact' });
    close.setAttribute('aria-label', 'Đóng');
    const authHeading = layout('auth-heading');
    title.before(authHeading);
    authHeading.append(title, close);
    content.querySelector('form input')?.setAttribute('autofocus', '');
    dialog.querySelector('[data-auth-body]').append(content);
    dialog.addEventListener('close', () => {
      if (!signal.aborted) location.hash = next || 'account';
    });
    target.append(dialog);
    return;
  }

  const title = mode === 'verify' ? 'Xác minh liên hệ' : routes.find(([, key]) => key === mode)[0];
  slot(view, 'content', mode === 'verify' ? verification(api, signal) : await personalContent(mode, api, signal));
  target.append(heading(title), accountNav('account/' + (mode === 'verify' ? 'profile' : mode)), view);
}
