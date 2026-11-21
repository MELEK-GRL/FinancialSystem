import { useCallback } from 'react'
import { useTranslation } from 'react-i18next'
import { useLocation, useNavigate, useSearchParams } from 'react-router'
import {
  Card,
  EmptyState,
  ErrorState,
  PageHeader,
  Pagination,
  SearchInput,
  Select,
  Spinner,
} from '@/shared/ui'
import { CustomerTable } from '../components/CustomerTable'
import { useListCustomersQuery } from '../customersApi'
import type { CustomerStatus } from '../types'

const PAGE_SIZE = 20
const statuses: CustomerStatus[] = ['Active', 'Passive']

export function CustomerListPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const location = useLocation()
  const [params, setParams] = useSearchParams()
  const search = params.get('search') ?? ''
  const status = statuses.find((value) => value === params.get('status'))
  const page = Math.max(1, Number(params.get('page')) || 1)

  const { data, isFetching, isError, refetch } = useListCustomersQuery({
    search: search || undefined,
    status,
    page,
    size: PAGE_SIZE,
  })

  const update = useCallback(
    (changes: Record<string, string | undefined>) =>
      setParams((current) => {
        const next = new URLSearchParams(current)
        for (const [key, value] of Object.entries(changes)) {
          if (value) next.set(key, value)
          else next.delete(key)
        }
        return next
      }),
    [setParams],
  )
  const handleSearch = useCallback(
    (value: string) => update({ search: value, page: undefined }),
    [update],
  )

  return (
    <div className="flex flex-col gap-6">
      <PageHeader title={t('customers.listTitle')} subtitle={t('customers.listSubtitle')} />
      <Card padded={false}>
        <div className="flex flex-col gap-3 border-b border-neutral-200 p-4 sm:flex-row sm:items-end">
          <div className="flex-1">
            <SearchInput
              label={t('customers.searchLabel')}
              placeholder={t('customers.searchPlaceholder')}
              clearLabel={t('form.clear')}
              defaultValue={search}
              onSearch={handleSearch}
            />
          </div>
          <div className="sm:w-48">
            <Select
              aria-label={t('customers.statusFilter')}
              value={status ?? ''}
              placeholder={t('customers.allStatuses')}
              options={statuses.map((value) => ({ value, label: t(`status.${value}`) }))}
              onChange={(event) =>
                update({ status: event.target.value || undefined, page: undefined })
              }
            />
          </div>
        </div>
        {isError ? (
          <ErrorState
            title={t('customers.loadError')}
            retryLabel={t('common.retry')}
            onRetry={refetch}
          />
        ) : !data ? (
          <div className="flex justify-center py-10">
            <Spinner label={t('common.loading')} />
          </div>
        ) : data.items.length === 0 ? (
          <EmptyState
            title={t('customers.emptyTitle')}
            description={t('customers.emptyDescription')}
          />
        ) : (
          <div className={isFetching ? 'opacity-60 transition-opacity' : undefined}>
            <CustomerTable
              customers={data.items}
              onSelect={(customer) =>
                navigate(`/customers/${customer.id}`, {
                  state: { from: location.pathname + location.search },
                })
              }
            />
            <Pagination
              page={data.page}
              totalPages={data.totalPages}
              summary={t('common.pageSummary', { page: data.page, total: data.totalPages })}
              previousLabel={t('common.previous')}
              nextLabel={t('common.next')}
              onPageChange={(next) => update({ page: String(next) })}
            />
          </div>
        )}
      </Card>
    </div>
  )
}
