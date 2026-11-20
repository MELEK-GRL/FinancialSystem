import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Plus } from 'lucide-react'
import { notify } from '@/shared/lib'
import { Button, ConfirmDialog, EmptyState, ErrorState, Modal, Spinner, Text } from '@/shared/ui'

export function StatesPreview() {
  const { t } = useTranslation()

  return (
    <div className="grid gap-4 md:grid-cols-3">
      <div className="rounded-lg border border-dashed border-neutral-200">
        <EmptyState
          title={t('designSystem.states.emptyTitle')}
          description={t('designSystem.states.emptyDescription')}
          action={
            <Button icon={<Plus className="size-4" />}>
              {t('designSystem.states.emptyAction')}
            </Button>
          }
        />
      </div>
      <div className="rounded-lg border border-dashed border-neutral-200">
        <ErrorState
          title={t('designSystem.states.errorTitle')}
          description={t('designSystem.states.errorDescription')}
          retryLabel={t('common.retry')}
          onRetry={() => undefined}
        />
      </div>
      <div className="flex items-center justify-center rounded-lg border border-dashed border-neutral-200 py-12">
        <Spinner label={t('common.loading')} />
      </div>
    </div>
  )
}

export function OverlaysPreview() {
  const { t } = useTranslation()
  const [modalOpen, setModalOpen] = useState(false)
  const [confirmOpen, setConfirmOpen] = useState(false)

  return (
    <div className="flex flex-wrap gap-3">
      <Button variant="secondary" onClick={() => setModalOpen(true)}>
        {t('designSystem.overlays.openModal')}
      </Button>
      <Button variant="secondary" onClick={() => setConfirmOpen(true)}>
        {t('designSystem.overlays.openConfirm')}
      </Button>
      <Button
        variant="secondary"
        onClick={() => notify.success(t('designSystem.overlays.toastSuccessText'))}
      >
        {t('designSystem.overlays.toastSuccess')}
      </Button>
      <Button
        variant="secondary"
        onClick={() => notify.error(t('designSystem.overlays.toastErrorText'))}
      >
        {t('designSystem.overlays.toastError')}
      </Button>

      <Modal
        open={modalOpen}
        title={t('designSystem.overlays.modalTitle')}
        closeLabel={t('common.close')}
        onClose={() => setModalOpen(false)}
      >
        <Text>{t('designSystem.overlays.modalBody')}</Text>
      </Modal>
      <ConfirmDialog
        open={confirmOpen}
        tone="danger"
        title={t('designSystem.overlays.confirmTitle')}
        description={t('designSystem.overlays.confirmBody')}
        confirmLabel={t('designSystem.overlays.confirmAction')}
        cancelLabel={t('common.cancel')}
        onCancel={() => setConfirmOpen(false)}
        onConfirm={() => {
          setConfirmOpen(false)
          notify.success(t('designSystem.overlays.confirmed'))
        }}
      />
    </div>
  )
}
