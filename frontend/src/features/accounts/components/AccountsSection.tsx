import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { CenterModal, EmptyState, ErrorState, Spinner } from '@/shared/ui'
import { useListAccountsQuery } from '../accountsApi'
import type { Account } from '../types'
import { AccountCard } from './AccountCard'
import { BalanceCard } from './BalanceCard'
import { DepositForm, WithdrawForm } from './MoneyOperationForm'
import { OpenAccountButton } from './OpenAccountButton'

type Operation = { account: Account; mode: 'deposit' | 'withdraw' } | null

export function AccountsSection() {
  const { t } = useTranslation()
  const { data: accounts, isLoading, isError, refetch } = useListAccountsQuery()
  const [operation, setOperation] = useState<Operation>(null)
  const close = () => setOperation(null)

  if (isLoading) return <Spinner label={t('common.loading')} />
  if (isError || !accounts) {
    return (
      <ErrorState
        title={t('accounts.loadError')}
        retryLabel={t('common.retry')}
        onRetry={refetch}
      />
    )
  }
  if (accounts.length === 0) {
    return (
      <div className="rounded-xl border border-dashed border-neutral-300 bg-white">
        <EmptyState
          title={t('accounts.emptyTitle')}
          description={t('accounts.emptyDescription')}
          action={<OpenAccountButton />}
        />
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-6">
      <BalanceCard accounts={accounts} />
      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
        {accounts.map((account) => (
          <AccountCard
            key={account.id}
            account={account}
            onDeposit={() => setOperation({ account, mode: 'deposit' })}
            onWithdraw={() => setOperation({ account, mode: 'withdraw' })}
          />
        ))}
      </div>
      <CenterModal
        open={operation !== null}
        title={t(
          operation?.mode === 'withdraw' ? 'accounts.withdrawTitle' : 'accounts.depositTitle',
        )}
        closeLabel={t('common.close')}
        onClose={close}
      >
        {operation?.mode === 'deposit' ? (
          <DepositForm account={operation.account} onDone={close} />
        ) : null}
        {operation?.mode === 'withdraw' ? (
          <WithdrawForm account={operation.account} onDone={close} />
        ) : null}
      </CenterModal>
    </div>
  )
}
