import { Compass } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import { Button, EmptyState } from '@/shared/ui'

export function NotFoundPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()

  return (
    <div className="flex min-h-screen items-center justify-center px-4">
      <EmptyState
        icon={<Compass className="size-8" aria-hidden="true" />}
        title={t('notFound.title')}
        description={t('notFound.description')}
        action={<Button onClick={() => navigate('/')}>{t('notFound.action')}</Button>}
      />
    </div>
  )
}
