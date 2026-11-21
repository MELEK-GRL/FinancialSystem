import { useCallback, useMemo } from 'react'
import { useUrlParams } from '@/shared/hooks'
import type { AuditAction, AuditFilters } from '../types'

export const auditActions: AuditAction[] = ['AccountDeactivated', 'AccountActivated']

const date = (value: string | null) =>
  value && /^\d{4}-\d{2}-\d{2}$/.test(value) ? value : undefined
const guid = (value: string | null) =>
  value && /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(value)
    ? value
    : undefined

const filterKeys = ['from', 'to', 'action', 'accountId'] as const

export function useAuditParams() {
  const { params, update } = useUrlParams()

  const filters = useMemo<AuditFilters>(
    () => ({
      from: date(params.get('from')),
      to: date(params.get('to')),
      action: auditActions.find((action) => action === params.get('action')),
      accountId: guid(params.get('accountId')),
    }),
    [params],
  )
  const page = Math.max(1, Number(params.get('page')) || 1)
  const hasFilters = filterKeys.some((key) => filters[key] !== undefined)

  const applyFilters = useCallback(
    (values: Omit<AuditFilters, 'accountId'>) => update({ ...values, page: undefined }),
    [update],
  )
  const clearFilters = useCallback(
    () => update(Object.fromEntries([...filterKeys, 'page'].map((key) => [key, undefined]))),
    [update],
  )

  return { filters, page, hasFilters, update, applyFilters, clearFilters }
}
