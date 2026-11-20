import type { ChangeEvent } from 'react'

export function sanitizeAmount(value: string, decimalSeparator: ',' | '.') {
  const thousandsSeparator = decimalSeparator === ',' ? '.' : ','
  const cleaned = value.replace(/[^0-9.,]/g, '').replaceAll(thousandsSeparator, '')
  const [whole, ...rest] = cleaned.split(decimalSeparator)
  return rest.length === 0 ? whole : `${whole}${decimalSeparator}${rest.join('').slice(0, 2)}`
}

export function sanitizeDigits(value: string, maxLength: number) {
  return value.replace(/\D/g, '').slice(0, maxLength)
}

export function formatPhone(value: string) {
  const digits = sanitizeDigits(value, 10)
  return [digits.slice(0, 3), digits.slice(3, 6), digits.slice(6, 8), digits.slice(8, 10)]
    .filter(Boolean)
    .join(' ')
}

export function withSanitizer<T extends HTMLInputElement | HTMLTextAreaElement>(
  sanitize: (value: string) => string,
  onChange?: (event: ChangeEvent<T>) => void,
) {
  return (event: ChangeEvent<T>) => {
    const next = sanitize(event.target.value)
    if (next !== event.target.value) event.target.value = next
    onChange?.(event)
  }
}
