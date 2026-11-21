import { action, element, layout, notice } from './view.js';

let fieldCount = 0;

export function field(label, options = {}) {
  const { type = 'text', choices, placeholder, hint, wide, validate, validateOnBlur = true, ...attributes } = options;
  const wrapper = element('div', '', wide ? 'field full' : 'field');
  const control = element(choices ? 'select' : type === 'textarea' ? 'textarea' : 'input');
  const id = 'field-' + ++fieldCount;
  control.id = id;
  if (control.tagName === 'INPUT') control.type = type;
  if (choices) {
    if (placeholder) control.add(new Option(placeholder, ''));
    for (const choice of choices) {
      const [value, name] = Array.isArray(choice) ? choice : [choice, choice];
      control.add(new Option(name, value));
    }
  }
  Object.assign(control, attributes);
  if (placeholder && !choices) control.placeholder = placeholder;
  const name = element('label', label);
  name.htmlFor = id;
  if (control.required) {
    const required = element('span', ' *', 'required-mark');
    required.setAttribute('aria-hidden', 'true');
    name.append(required);
  }
  wrapper.append(type === 'checkbox' || type === 'radio' ? layout('check', control, name) : name);
  if (type !== 'checkbox' && type !== 'radio') wrapper.append(control);
  const error = element('p', '', 'field-error');
  error.id = id + '-error';
  error.setAttribute('aria-live', 'polite');
  const descriptions = [error.id];
  if (hint) {
    const note = element('p', hint, 'field-hint');
    note.id = id + '-hint';
    wrapper.append(note);
    descriptions.unshift(note.id);
  }
  wrapper.append(error);
  control.setAttribute('aria-describedby', descriptions.join(' '));

  function showError(message) {
    error.textContent = message;
    if (message) control.setAttribute('aria-invalid', 'true');
    else control.removeAttribute('aria-invalid');
  }

  function check() {
    if (control.matches(':disabled')) return true;
    const { validity } = control;
    const blank = control.required && ['text', 'search', 'tel', 'textarea'].includes(control.type) && !control.value.trim();
    const message = validity.badInput ? 'Nhập giá trị hợp lệ.'
      : (validity.valueMissing || blank) ? (type === 'checkbox' ? 'Vui lòng xác nhận.' : 'Vui lòng điền hoặc chọn thông tin này.')
      : validity.typeMismatch ? (type === 'email' ? 'Nhập địa chỉ email hợp lệ.' : 'Nhập địa chỉ hợp lệ.')
      : validity.tooShort ? 'Nhập ít nhất ' + control.minLength + ' ký tự.'
      : validity.patternMismatch ? (hint || 'Kiểm tra định dạng đã nhập.')
      : !validity.valid ? 'Giá trị nằm ngoài phạm vi cho phép.'
      : (validate?.(control.value, control) || '');
    showError(message);
    return !message;
  }

  if (type !== 'file') {
    control.addEventListener('blur', () => {
      if (validateOnBlur && (control.value || control.hasAttribute('aria-invalid'))) check();
    });
    control.addEventListener('input', () => {
      if (control.hasAttribute('aria-invalid')) check();
    });
  }
  return { wrapper, control, check, showError };
}

export function fieldGrid(fields) {
  return layout('form-grid', ...fields.map(item => item.wrapper));
}

export function fieldset(title, ...children) {
  const group = element('fieldset', '', 'field-group');
  group.append(element('legend', title), ...children);
  return group;
}

export function staticForm() {
  const form = element('form', '', 'stack');
  form.noValidate = true;
  form.autocomplete = 'off';
  form.addEventListener('submit', event => event.preventDefault());
  return form;
}

export function finishForm(form, fields, message, submitLabel, submit) {
  const result = element('p', '', 'field-hint');
  result.setAttribute('role', 'status');
  function checkFields() {
    const invalid = fields.filter(item => !item.check());
    result.textContent = invalid.length ? 'Kiểm tra các trường được đánh dấu.' : 'Định dạng các trường hợp lệ.';
    invalid[0]?.control.focus();
    return !invalid.length;
  }
  const send = action(submitLabel, null, { type: 'submit', style: '', disabled: !submit });
  form.append(layout('stack', ...(message ? [notice(message)] : []),
    layout('actions', send), result));

  if (submit) form.addEventListener('submit', async event => {
    event.preventDefault();
    if (send.disabled || !checkFields()) return;
    send.disabled = true;
    form.setAttribute('aria-busy', 'true');
    result.textContent = 'Đang gửi…';
    try {
      await submit();
      result.textContent = 'Đã cập nhật.';
    } catch (error) {
      result.textContent = error instanceof TypeError ? 'Không kết nối được dịch vụ. Kiểm tra mạng rồi gửi lại.' : error.message || 'Không hoàn tất được thao tác.';
      let firstInvalid;
      for (const [key, messages] of Object.entries(error.fields ?? {})) {
        const name = key.split('.').pop().toLowerCase();
        const item = fields.find(f => f.control.name.toLowerCase() === name);
        if (!item) continue;
        item.showError(Array.isArray(messages) ? messages.join(' ') : String(messages));
        firstInvalid ??= item;
      }
      firstInvalid?.control.focus();
      if (error.status === 401 && !form.closest('.auth-dialog')) {
        const link = element('a', 'Đăng nhập lại');
        link.href = '#account/login?' + new URLSearchParams({ next: location.hash.slice(1) });
        result.append(' ', link);
      }
    } finally {
      form.removeAttribute('aria-busy');
      send.disabled = false;
    }
  });
  return form;
}

export function formValues(form) {
  return Object.fromEntries(new FormData(form));
}

export function queryForm(fields, fragment, label, clearFragment = fragment) {
  const form = element('form', '', 'panel filter-form');
  form.setAttribute('role', 'search');
  form.append(fieldGrid(fields), layout('actions', action(label, null, { type: 'submit', style: '' }),
    action('Xóa bộ lọc', () => {
      if (location.hash === '#' + clearFragment) {
        form.reset();
        fields[0].control.focus();
      } else location.hash = clearFragment;
    })));
  form.addEventListener('submit', event => {
    event.preventDefault();
    const query = new URLSearchParams();
    for (const [key, value] of new FormData(form)) {
      if (value.trim()) query.set(key, value.trim());
    }
    const hash = '#' + fragment + (query.size ? '?' + query : '');
    if (location.hash !== hash) location.hash = hash;
    else document.querySelector('[data-filter-results]')?.focus();
  });
  return form;
}

export function queryField(query, key, label, options = {}) {
  const item = field(label, { name: key, ...options });
  const value = query.get(key);
  if (value !== null) {
    if (item.control.type === 'checkbox') item.control.checked = value === 'on';
    else item.control.value = value;
  }
  return item;
}
