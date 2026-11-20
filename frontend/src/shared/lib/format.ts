export const DEFAULT_LOCALE = 'tr-TR'
export const CURRENCY = 'TRY'

export function formatMoney(amount: number, locale: string = DEFAULT_LOCALE) {
  return new Intl.NumberFormat(locale, { style: 'currency', currency: CURRENCY }).format(amount)
}

export function formatDate(value: string | Date, locale: string = DEFAULT_LOCALE, withTime = true) {
  return new Intl.DateTimeFormat(locale, {
    dateStyle: 'medium',
    timeStyle: withTime ? 'short' : undefined,
    timeZone: 'Europe/Istanbul',
  }).format(typeof value === 'string' ? new Date(value) : value)
}
