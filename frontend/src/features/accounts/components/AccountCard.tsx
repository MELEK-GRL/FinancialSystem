import { ArrowDownToLine, ArrowUpFromLine } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useLocale } from '@/shared/hooks'
import { formatDate } from '@/shared/lib'
import { Button, MoneyText, Text } from '@/shared/ui'
import type { Account } from '../types'
import { AccountStatusBadge } from './AccountStatusBadge'

export type AccountCardProps = {
  account: Account
  onDeposit: (account: Account) => void
  onWithdraw: (account: Account) => void
}

export function AccountCard({ account, onDeposit, onWithdraw }: AccountCardProps) {
  const { t } = useTranslation()
  const locale = useLocale()
  const active = account.status === 'Active'

  return (
    <article className="flex flex-col gap-5 rounded-xl border border-neutral-200 bg-white p-6 shadow-sm">
      <div className="flex items-start justify-between gap-3">
        <div className="flex flex-col gap-0.5">
          <span className="font-mono text-sm font-medium tracking-wide text-neutral-900">
            {account.accountNo}
          </span>
          <Text variant="small">
            {t('accounts.openedAt', { date: formatDate(account.createdAt, locale, false) })}
          </Text>
        </div>
        <AccountStatusBadge status={account.status} />
      </div>
      <div className="flex flex-col gap-0.5">
        <Text variant="muted">{t('accounts.balance')}</Text>
        <MoneyText amount={account.balance} size="lg" />
      </div>
      {active ? (
        <div className="grid grid-cols-2 gap-2">
          <Button
            variant="secondary"
            icon={<ArrowDownToLine className="size-4" />}
            onClick={() => onDeposit(account)}
          >
            {t('accounts.deposit')}
          </Button>
          <Button
            variant="secondary"
            icon={<ArrowUpFromLine className="size-4" />}
            onClick={() => onWithdraw(account)}
          >
            {t('accounts.withdraw')}
          </Button>
        </div>
      ) : (
        <Text variant="muted">{t('accounts.passiveNote')}</Text>
      )}
    </article>
  )
}
