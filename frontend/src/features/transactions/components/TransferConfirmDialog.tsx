import { ArrowDown } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Button, CenterModal, MoneyText, Text } from '@/shared/ui'
import type { PendingTransfer } from '../hooks/useTransferForm'

export type TransferConfirmDialogProps = {
  transfer: PendingTransfer | null
  sending: boolean
  onConfirm: () => void
  onCancel: () => void
}

export function TransferConfirmDialog({
  transfer,
  sending,
  onConfirm,
  onCancel,
}: TransferConfirmDialogProps) {
  const { t } = useTranslation()

  return (
    <CenterModal
      open={transfer !== null}
      title={t('transfer.confirmTitle')}
      closeLabel={t('common.close')}
      onClose={onCancel}
      footer={
        <>
          <Button variant="secondary" onClick={onCancel} disabled={sending}>
            {t('transfer.edit')}
          </Button>
          <Button onClick={onConfirm} loading={sending}>
            {t('transfer.send')}
          </Button>
        </>
      }
    >
      {transfer ? (
        <div className="flex flex-col gap-4">
          <div className="flex flex-col items-center gap-1 rounded-xl bg-primary-soft py-5">
            <MoneyText amount={transfer.amount} size="lg" className="text-primary-dark" />
            {transfer.description ? <Text variant="muted">{transfer.description}</Text> : null}
          </div>
          <div className="flex flex-col gap-2">
            <div className="rounded-lg border border-neutral-200 px-4 py-3">
              <Text variant="small">{t('transfer.from')}</Text>
              <p className="font-mono text-sm text-neutral-900">{transfer.source.accountNo}</p>
            </div>
            <ArrowDown className="mx-auto size-4 text-neutral-400" aria-hidden="true" />
            <div className="rounded-lg border border-neutral-200 px-4 py-3">
              <Text variant="small">{t('transfer.to')}</Text>
              <p className="font-mono text-sm text-neutral-900">{transfer.recipient.accountNo}</p>
              <p className="text-sm font-medium text-neutral-900">{transfer.recipient.ownerName}</p>
            </div>
          </div>
        </div>
      ) : null}
    </CenterModal>
  )
}
