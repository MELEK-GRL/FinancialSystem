import { useTranslation } from 'react-i18next'
import { PageHeader } from '@/shared/ui'
import { AccountsSection } from '../components/AccountsSection'
import { OpenAccountButton } from '../components/OpenAccountButton'

export function MyAccountsPage() {
  const { t } = useTranslation()

  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title={t('accounts.title')}
        subtitle={t('accounts.subtitle')}
        actions={<OpenAccountButton />}
      />
      <AccountsSection />
    </div>
  )
}
