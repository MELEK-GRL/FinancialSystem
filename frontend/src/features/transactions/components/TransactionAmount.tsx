import { MoneyText } from '@/shared/ui'
import type { AccountTransaction } from '../types'

export type TransactionAmountProps = {
  transaction: Pick<AccountTransaction, 'amount' | 'direction' | 'status'>
  size?: 'md' | 'lg'
}

export function TransactionAmount({ transaction, size }: TransactionAmountProps) {
  const { amount, direction, status } = transaction
  if (status !== 'Success') {
    return (
      <MoneyText
        amount={amount}
        size={size}
        className={status === 'Failed' ? 'text-neutral-400 line-through' : 'text-neutral-500'}
      />
    )
  }
  return <MoneyText amount={amount} size={size} direction={direction === 'In' ? 'in' : 'out'} />
}
