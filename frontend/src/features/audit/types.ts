import type { AccountStatus } from '@/features/accounts'

export type AuditAction = 'AccountActivated' | 'AccountDeactivated'

export type AuditLog = {
  id: string
  accountId: string
  accountNo: string
  action: AuditAction
  oldStatus: AccountStatus
  newStatus: AccountStatus
  reason: string
  performedBy: string
  performedByEmail: string
  correlationId: string
  createdAt: string
}

export type AuditFilters = {
  from?: string
  to?: string
  action?: AuditAction
  accountId?: string
}

export type AuditLogParams = AuditFilters & { page: number; size: number }
