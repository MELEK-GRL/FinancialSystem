import { useTranslation } from 'react-i18next'
import { Badge } from '@/shared/ui'
import type { AccountStatus } from '../types'

export function AccountStatusBadge({ status }: { status: AccountStatus }) {
  const { t } = useTranslation()
  return <Badge tone={status === 'Active' ? 'success' : 'neutral'}>{t(`status.${status}`)}</Badge>
}
