export type CustomerStatus = 'Active' | 'Passive'

export type Customer = {
  id: string
  customerNo: string
  firstName: string
  lastName: string
  nationalId: string
  email: string
  phone: string
  status: CustomerStatus
  createdAt: string
}

export type CustomerSummary = Pick<
  Customer,
  'id' | 'customerNo' | 'firstName' | 'lastName' | 'email' | 'status' | 'createdAt'
>

export type CustomerListParams = {
  search?: string
  status?: CustomerStatus
  page: number
  size: number
}
