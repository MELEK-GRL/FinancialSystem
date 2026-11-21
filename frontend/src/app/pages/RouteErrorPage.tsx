import { useTranslation } from 'react-i18next'
import { ErrorState } from '@/shared/ui'

export function RouteErrorPage() {
  const { t } = useTranslation()

  return (
    <div className="flex min-h-screen items-center justify-center px-4">
      <ErrorState
        title={t('errors.INTERNAL_ERROR')}
        retryLabel={t('common.retry')}
        onRetry={() => window.location.reload()}
      />
    </div>
  )
}
