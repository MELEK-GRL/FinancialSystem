import { useSyncExternalStore } from 'react'
import { useTranslation } from 'react-i18next'
import { popup } from '@/shared/lib'
import { PopupModal } from '@/shared/ui'

export function PopupHost() {
  const { t } = useTranslation()
  const current = useSyncExternalStore(popup.subscribe, popup.getSnapshot)

  return (
    <PopupModal
      open={current !== null}
      variant={current?.variant ?? 'info'}
      title={current?.title ?? ''}
      description={current?.description}
      actionLabel={current?.actionLabel ?? t('common.ok')}
      onClose={popup.close}
    />
  )
}
