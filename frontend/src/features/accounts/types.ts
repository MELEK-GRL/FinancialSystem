export type AccountStatus = 'Active' | 'Passive'

export type Account = {
  id: string
  accountNo: string
  customerId: string
  currency: string
  balance: number
  status: AccountStatus
  createdAt: string
}
