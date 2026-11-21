import { useTranslation } from 'react-i18next'
import { Badge } from '@/shared/ui'
import type { TransactionStatus } from '../types'

const tones = { Success: 'success', Failed: 'danger', Pending: 'warning' } as const

export function TransactionStatusBadge({ status }: { status: TransactionStatus }) {
  const { t } = useTranslation()
  return <Badge tone={tones[status]}>{t(`history.status.${status}`)}</Badge>
}
