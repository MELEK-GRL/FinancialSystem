import clsx from 'clsx'
import { useLocale } from '@/shared/hooks/useLocale'
import { formatMoney } from '@/shared/lib/format'

export type MoneyTextProps = {
  amount: number
  direction?: 'in' | 'out'
  size?: 'md' | 'lg'
  className?: string
}

export function MoneyText({ amount, direction, size = 'md', className }: MoneyTextProps) {
  const locale = useLocale()
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
