export type TransactionType = 'Deposit' | 'Withdrawal' | 'Transfer'
export type TransactionStatus = 'Success' | 'Failed' | 'Pending'

export type Transaction = {
  id: string
  referenceNo: string
  type: TransactionType
  status: TransactionStatus
  amount: number
  currency: string
  sourceAccountId: string | null
  targetAccountId: string | null
  description: string | null
  failureReason: string | null
  createdAt: string
  completedAt: string | null
}

export type MoneyRequest = {
  accountId: string
  amount: number
  description?: string
  idempotencyKey: string
}
