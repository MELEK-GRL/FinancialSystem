import clsx from 'clsx'
import { DEFAULT_LOCALE, formatMoney } from '@/shared/lib/format'

export type MoneyTextProps = {
  amount: number
  direction?: 'in' | 'out'
  locale?: string
  size?: 'md' | 'lg'
  className?: string
}

export function MoneyText({
  amount,
  direction,
  locale = DEFAULT_LOCALE,
  size = 'md',
  className,
}: MoneyTextProps) {
  const sign = direction === 'in' ? '+ ' : direction === 'out' ? '− ' : ''
  return (
    <span
      className={clsx(
        'tabular-nums',
        size === 'lg' ? 'text-3xl font-semibold tracking-tight' : 'font-medium',
        direction === 'in' && 'text-success',
        direction === 'out' && 'text-danger',
        className,
      )}
    >
      {sign}
      {formatMoney(Math.abs(amount), locale)}
    </span>
  )
}
