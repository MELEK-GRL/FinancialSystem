import { X } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useGetAccountQuery } from '@/features/accounts'
import { getErrorMessage } from '@/shared/api'
import {
  Badge,
  Card,
  EmptyState,
  ErrorState,
  IconButton,
  PageHeader,
  Pagination,
  Spinner,
} from '@/shared/ui'
import { useListAuditLogsQuery } from '../auditApi'
import { AccountStatusHistoryTable } from '../components/AccountStatusHistoryTable'
import { AuditLogFilters } from '../components/AuditLogFilters'
import { useAuditParams } from '../hooks/useAuditParams'

const PAGE_SIZE = 20

export function AuditLogPage() {
  const { t } = useTranslation()
  const { filters, page, hasFilters, update, applyFilters, clearFilters } = useAuditParams()
  const { data, isFetching, error, refetch } = useListAuditLogsQuery({
    ...filters,
    page,
    size: PAGE_SIZE,
  })
  const account = useGetAccountQuery(filters.accountId ?? '', { skip: !filters.accountId })

  return (
    <div className="flex flex-col gap-6">
      <PageHeader title={t('audit.title')} subtitle={t('audit.subtitle')} />
      <Card>
        <AuditLogFilters
          key={JSON.stringify(filters)}
          filters={filters}
          onApply={applyFilters}
          onClear={clearFilters}
        />
        {filters.accountId ? (
          <div className="mt-4 flex items-center gap-1">
            <Badge tone="primary">
              {t('audit.accountFilter', { accountNo: account.data?.accountNo ?? '…' })}
            </Badge>
            <IconButton
              aria-label={t('audit.removeAccountFilter')}
              icon={<X className="size-4" />}
              onClick={() => update({ accountId: undefined, page: undefined })}
            />
          </div>
        ) : null}
      </Card>
      <Card
        title={data ? t('audit.count', { count: data.totalCount }) : t('audit.title')}
        padded={false}
      >
        {error ? (
          <ErrorState
            title={t('audit.loadError')}
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
            title={t(hasFilters ? 'audit.emptyFiltered' : 'audit.emptyTitle')}
            description={hasFilters ? t('audit.emptyFilteredHint') : undefined}
          />
        ) : (
          <div className={isFetching ? 'opacity-60 transition-opacity' : undefined}>
            <AccountStatusHistoryTable logs={data.items} />
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
