import type { AccountTransaction } from '../types'

export const transactionLabelKey = ({
  type,
  direction,
}: Pick<AccountTransaction, 'type' | 'direction'>) =>
  type === 'Transfer'
    ? direction === 'In'
      ? 'history.type.TransferIn'
      : 'history.type.TransferOut'
    : (`history.type.${type}` as const)
