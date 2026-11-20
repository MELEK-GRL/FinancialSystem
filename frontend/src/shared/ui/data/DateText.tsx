import { DEFAULT_LOCALE, formatDate } from '@/shared/lib/format'

export type DateTextProps = {
  value: string
  locale?: string
  withTime?: boolean
}

export function DateText({ value, locale = DEFAULT_LOCALE, withTime = true }: DateTextProps) {
  return (
    <time dateTime={value} className="tabular-nums">
      {formatDate(value, locale, withTime)}
    </time>
  )
}
