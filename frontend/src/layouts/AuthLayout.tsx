import { Suspense } from 'react'
import { useTranslation } from 'react-i18next'
import { Navigate, Outlet } from 'react-router'
import { useAppSelector } from '@/app/hooks'
import { selectIsAuthenticated } from '@/features/auth'
import { LanguageSelector } from '@/shared/i18n'
import { Text } from '@/shared/ui'
import { Brand } from './Brand'
import { PageFallback } from './PageFallback'

export function AuthLayout() {
  const { t } = useTranslation()
  const isAuthenticated = useAppSelector(selectIsAuthenticated)

  if (isAuthenticated) return <Navigate to="/" replace />

  return (
    <div className="flex min-h-screen flex-col items-center justify-center gap-6 px-4 py-10">
      <div className="flex w-full max-w-md items-center justify-between">
        <Brand />
        <LanguageSelector />
      </div>
      <div className="w-full max-w-md rounded-xl border border-neutral-200 bg-white p-6 shadow-sm md:p-8">
        <Suspense fallback={<PageFallback />}>
          <Outlet />
        </Suspense>
      </div>
      <Text variant="muted" className="text-center">
        {t('auth.brandTagline')}
      </Text>
    </div>
  )
}
