import { useCallback, useMemo } from 'react'
import { useUrlParams } from '@/shared/hooks'
import type { HistoryFilters, HistorySort, TransactionStatus, TransactionType } from '../types'

export const transactionTypes: TransactionType[] = ['Deposit', 'Withdrawal', 'Transfer']
export const transactionStatuses: TransactionStatus[] = ['Success', 'Failed', 'Pending']
export const historySorts: HistorySort[] = [
  'createdAt_desc',
  'createdAt_asc',
  'amount_desc',
  'amount_asc',
]

const pick = <T extends string>(values: readonly T[], value: string | null) =>
  values.find((candidate) => candidate === value)
const date = (value: string | null) =>
  value && /^\d{4}-\d{2}-\d{2}$/.test(value) ? value : undefined
const amount = (value: string | null) =>
  value && /^\d+(\.\d{1,2})?$/.test(value) ? value : undefined

const filterKeys = ['from', 'to', 'type', 'status', 'minAmount', 'maxAmount'] as const

export function useHistoryParams() {
  const { params, update } = useUrlParams()

  const filters = useMemo<HistoryFilters>(
    () => ({
      from: date(params.get('from')),
      to: date(params.get('to')),
      type: pick(transactionTypes, params.get('type')),
      status: pick(transactionStatuses, params.get('status')),
      minAmount: amount(params.get('minAmount')),
      maxAmount: amount(params.get('maxAmount')),
    }),
    [params],
  )
  const sort = pick(historySorts, params.get('sort')) ?? 'createdAt_desc'
  const page = Math.max(1, Number(params.get('page')) || 1)
  const hasFilters = filterKeys.some((key) => filters[key] !== undefined)

  const applyFilters = useCallback(
    (values: HistoryFilters) => update({ ...values, page: undefined }),
    [update],
  )
  const clearFilters = useCallback(
    () => update(Object.fromEntries([...filterKeys, 'page'].map((key) => [key, undefined]))),
    [update],
  )

  return { filters, sort, page, hasFilters, update, applyFilters, clearFilters }
}
