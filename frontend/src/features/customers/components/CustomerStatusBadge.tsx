import { useTranslation } from 'react-i18next'
import { Badge } from '@/shared/ui'
import type { CustomerStatus } from '../types'

export function CustomerStatusBadge({ status }: { status: CustomerStatus }) {
  const { t } = useTranslation()
  return <Badge tone={status === 'Active' ? 'success' : 'neutral'}>{t(`status.${status}`)}</Badge>
}
