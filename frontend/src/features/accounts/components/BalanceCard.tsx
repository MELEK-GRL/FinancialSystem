import { Wallet } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { MoneyText, Text } from '@/shared/ui'
import type { Account } from '../types'

export function BalanceCard({ accounts }: { accounts: Account[] }) {
  const { t } = useTranslation()
  const total = accounts.reduce((sum, account) => sum + account.balance, 0)

  return (
    <section className="flex items-center justify-between gap-4 rounded-xl border border-neutral-200 bg-white p-6 shadow-sm">
      <div className="flex flex-col gap-1">
        <Text variant="muted">{t('accounts.totalBalance')}</Text>
        <MoneyText amount={total} size="lg" />
        <Text variant="small">{t('accounts.accountCount', { count: accounts.length })}</Text>
      </div>
      <span className="hidden size-12 items-center justify-center rounded-full bg-primary-soft text-primary sm:flex">
        <Wallet className="size-6" aria-hidden="true" />
      </span>
    </section>
  )
}
