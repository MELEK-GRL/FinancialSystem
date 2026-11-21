import { useTranslation } from 'react-i18next'
import { Spinner } from '@/shared/ui'

export function PageFallback() {
  const { t } = useTranslation()
  return (
    <div className="flex justify-center py-16">
      <Spinner label={t('common.loading')} />
    </div>
  )
}
