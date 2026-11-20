import { useLocale } from '@/shared/hooks/useLocale'
import { formatDate } from '@/shared/lib/format'

export type DateTextProps = {
  value: string
  withTime?: boolean
}

export function DateText({ value, withTime = true }: DateTextProps) {
  const locale = useLocale()
  return (
    <time dateTime={value} className="tabular-nums">
      {formatDate(value, locale, withTime)}
    </time>
  )
}
