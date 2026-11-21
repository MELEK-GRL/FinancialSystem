import { useTranslation } from 'react-i18next'
import { useLocation } from 'react-router'
import {
  AccountStatusBadge,
  AccountStatusToggle,
  useListAccountsQuery,
  type Account,
} from '@/features/accounts'
import {
  Card,
  EmptyState,
  ErrorState,
  Link,
  MoneyText,
  Spinner,
  Table,
  type TableColumn,
} from '@/shared/ui'

export function CustomerAccounts({ customerId }: { customerId: string }) {
  const { t } = useTranslation()
  const location = useLocation()
  const { data, isLoading, isError, refetch } = useListAccountsQuery(customerId)

  const columns: TableColumn<Account>[] = [
    {
      key: 'accountNo',
      header: t('customers.accountNo'),
      render: (a) => <span className="font-mono text-xs">{a.accountNo}</span>,
    },
    {
      key: 'status',
      header: t('customers.accountStatus'),
      render: (a) => <AccountStatusBadge status={a.status} />,
    },
    {
      key: 'balance',
      header: t('customers.balance'),
      align: 'right',
      render: (a) => <MoneyText amount={a.balance} />,
    },
    {
      key: 'history',
      header: t('customers.records'),
      render: (a) => (
        <span className="flex flex-wrap gap-x-3 gap-y-1 text-sm">
          <Link
            to={`/accounts/${a.id}/transactions`}
            state={{ from: location.pathname + location.search }}
          >
            {t('customers.transactionsLink')}
          </Link>
          <Link to={`/audit-logs?accountId=${a.id}`}>{t('customers.statusHistoryLink')}</Link>
        </span>
      ),
    },
    {
      key: 'action',
      header: t('accounts.statusAction'),
      align: 'right',
      render: (a) => <AccountStatusToggle account={a} />,
    },
  ]

  return (
    <Card title={t('customers.accounts')} padded={false}>
      {isLoading ? (
        <div className="flex justify-center py-10">
          <Spinner label={t('common.loading')} />
        </div>
      ) : isError ? (
        <ErrorState
          title={t('errors.INTERNAL_ERROR')}
          retryLabel={t('common.retry')}
          onRetry={refetch}
        />
      ) : data && data.length > 0 ? (
        <Table columns={columns} rows={data} rowKey={(a) => a.id} />
      ) : (
        <EmptyState title={t('customers.noAccounts')} />
      )}
    </Card>
  )
}
