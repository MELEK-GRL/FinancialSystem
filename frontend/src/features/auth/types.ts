export type Role = 'ADMIN' | 'CUSTOMER'

export type Session = {
  accessToken: string
  expiresAt: string
  role: Role
}

export type AuthUser = {
  userId: string
  email: string
  role: Role
  customerId: string | null
  customerNo: string | null
  firstName: string | null
  lastName: string | null
}
