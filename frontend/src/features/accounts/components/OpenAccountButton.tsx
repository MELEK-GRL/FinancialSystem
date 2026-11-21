import { Plus } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '@/shared/api'
import { popup } from '@/shared/lib'
import { Button } from '@/shared/ui'
import { useOpenAccountMutation } from '../accountsApi'

export function OpenAccountButton() {
  const { t } = useTranslation()
  const [openAccount, { isLoading }] = useOpenAccountMutation()

  const open = async () => {
    try {
      const account = await openAccount().unwrap()
      popup.success({
        title: t('accounts.opened'),
        description: t('accounts.openedBody', { accountNo: account.accountNo }),
      })
    } catch (error) {
      popup.danger({ title: t('accounts.openFailed'), description: getErrorMessage(error) })
    }
  }

  return (
    <Button icon={<Plus className="size-4" />} loading={isLoading} onClick={open}>
      {t('accounts.open')}
    </Button>
  )
}
