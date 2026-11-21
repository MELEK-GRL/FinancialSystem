import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import { useListAccountsQuery } from '@/features/accounts'
import { Button, Card, EmptyState, ErrorState, PageHeader, Spinner } from '@/shared/ui'
import { TransferForm } from '../components/TransferForm'

export function TransferPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { data, isLoading, isError, refetch } = useListAccountsQuery()
  const activeAccounts = (data ?? []).filter((account) => account.status === 'Active')

  return (
    <div className="flex flex-col gap-6">
      <PageHeader title={t('transfer.title')} subtitle={t('transfer.subtitle')} />
      <Card className="max-w-2xl">
        {isLoading ? (
          <Spinner label={t('common.loading')} />
        ) : isError ? (
          <ErrorState
            title={t('accounts.loadError')}
            retryLabel={t('common.retry')}
            onRetry={refetch}
          />
        ) : activeAccounts.length === 0 ? (
          <EmptyState
            title={t('transfer.noActiveAccounts')}
            action={
              <Button onClick={() => navigate('/accounts')}>{t('transfer.goAccounts')}</Button>
            }
          />
        ) : (
          <TransferForm accounts={activeAccounts} />
        )}
      </Card>
    </div>
  )
}
