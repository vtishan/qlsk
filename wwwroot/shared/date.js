const todayFormat = new Intl.DateTimeFormat('en-CA', {
  timeZone: 'Asia/Ho_Chi_Minh', year: 'numeric', month: 'numeric', day: 'numeric'
});

export function todayInVietnam(now = new Date()) {
  const parts = todayFormat.formatToParts(now);
  const number = type => Number(parts.find(part => part.type === type).value);
  return new Date(Date.UTC(number('year'), number('month') - 1, number('day')));
}

export function monthStart(date, offset = 0) {
  return new Date(Date.UTC(date.getUTCFullYear(), date.getUTCMonth() + offset, 1));
}

export function dayOfMonth(month, day) {
  return new Date(Date.UTC(month.getUTCFullYear(), month.getUTCMonth(), day));
}
