import { useState } from 'react'
import { ArrowLeft } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useLocation, useParams } from 'react-router'
import { AccountStatusBadge, useGetAccountQuery } from '@/features/accounts'
import { getErrorMessage } from '@/shared/api'
import { useLocale } from '@/shared/hooks'
import { formatMoney } from '@/shared/lib'
import {
  Card,
  EmptyState,
  ErrorState,
  Link,
  PageHeader,
  Pagination,
  Select,
  Spinner,
} from '@/shared/ui'
import { TransactionDetailModal } from '../components/TransactionDetailModal'
import { TransactionFilters } from '../components/TransactionFilters'
import { TransactionHistoryTable } from '../components/TransactionHistoryTable'
import { historySorts, useHistoryParams } from '../hooks/useHistoryParams'
import { useGetAccountHistoryQuery } from '../transactionsApi'
import type { AccountTransaction } from '../types'

const PAGE_SIZE = 20

export function TransactionHistoryPage() {
  const { t } = useTranslation()
  const locale = useLocale()
  const { id = '' } = useParams()
  const backTo = (useLocation().state as { from?: string } | null)?.from ?? '/accounts'
  const { filters, sort, page, hasFilters, update, applyFilters, clearFilters } = useHistoryParams()
  const [selected, setSelected] = useState<AccountTransaction | null>(null)

  const account = useGetAccountQuery(id)
  const history = useGetAccountHistoryQuery({
    accountId: id,
    ...filters,
    sort,
    page,
    size: PAGE_SIZE,
  })

  const back = (
    <Link to={backTo} className="inline-flex items-center gap-1 text-sm">
      <ArrowLeft className="size-4" aria-hidden="true" />
      {t('history.back')}
    </Link>
  )

  if (account.isLoading) return <Spinner label={t('common.loading')} />
  if (account.error || !account.data) {
    return (
      <div className="flex flex-col gap-4">
        {back}
        <ErrorState
          title={t('history.accountError')}
          description={getErrorMessage(account.error)}
          retryLabel={t('common.retry')}
          onRetry={account.refetch}
        />
      </div>
    )
  }

  const { data, isFetching, error, refetch } = history

  return (
    <div className="flex flex-col gap-6">
      {back}
      <PageHeader
        title={
          <span className="flex flex-wrap items-center gap-3">
            {t('history.title')}
            <AccountStatusBadge status={account.data.status} />
          </span>
        }
        subtitle={t('history.subtitle', {
          accountNo: account.data.accountNo,
          balance: formatMoney(account.data.balance, locale),
        })}
      />
      <Card>
        <TransactionFilters
          key={JSON.stringify(filters)}
          filters={filters}
          onApply={applyFilters}
          onClear={clearFilters}
        />
      </Card>
      <Card
        title={data ? t('history.count', { count: data.totalCount }) : t('history.listTitle')}
        actions={
          <div className="w-44">
            <Select
              aria-label={t('history.sortLabel')}
              value={sort}
              options={historySorts.map((value) => ({ value, label: t(`history.sort.${value}`) }))}
              onChange={(event) => update({ sort: event.target.value, page: undefined })}
            />
          </div>
        }
        padded={false}
      >
        {error ? (
          <ErrorState
            title={t('history.loadError')}
            description={getErrorMessage(error)}
            retryLabel={t('common.retry')}
            onRetry={refetch}
          />
        ) : !data ? (
          <div className="flex justify-center py-10">
            <Spinner label={t('common.loading')} />
          </div>
        ) : data.items.length === 0 ? (
          <EmptyState
            title={t(hasFilters ? 'history.emptyFiltered' : 'history.emptyTitle')}
            description={hasFilters ? t('history.emptyFilteredHint') : undefined}
          />
        ) : (
          <div className={isFetching ? 'opacity-60 transition-opacity' : undefined}>
            <TransactionHistoryTable transactions={data.items} onSelect={setSelected} />
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
      <TransactionDetailModal transaction={selected} onClose={() => setSelected(null)} />
    </div>
  )
}
