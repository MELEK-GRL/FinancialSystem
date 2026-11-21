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

export type TransferRequest = {
  sourceAccountId: string
  targetAccountNo: string
  amount: number
  description?: string
  idempotencyKey: string
}

export type TransactionDirection = 'In' | 'Out'

export type AccountTransaction = {
  id: string
  referenceNo: string
  type: TransactionType
  status: TransactionStatus
  direction: TransactionDirection
  amount: number
  currency: string
  counterpartyAccountNo: string | null
  description: string | null
  failureReason: string | null
  createdAt: string
  completedAt: string | null
}

export type HistorySort = 'createdAt_desc' | 'createdAt_asc' | 'amount_desc' | 'amount_asc'

export type HistoryFilters = {
  from?: string
  to?: string
  type?: TransactionType
  status?: TransactionStatus
  minAmount?: string
  maxAmount?: string
}

export type HistoryParams = HistoryFilters & {
  accountId: string
  sort: HistorySort
  page: number
  size: number
}
