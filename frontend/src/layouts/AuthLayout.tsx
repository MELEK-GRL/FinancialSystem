import { Suspense } from 'react'
import { BarChart3, Clock, ShieldCheck } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Navigate, Outlet } from 'react-router'
import { useAppSelector } from '@/app/hooks'
import { selectIsAuthenticated } from '@/features/auth'
import { LanguageSelector } from '@/shared/i18n'
import { Brand } from './Brand'
import { PageFallback } from './PageFallback'

const trustItems = [
  { icon: ShieldCheck, title: 'auth.trustSecureTitle', text: 'auth.trustSecureText' },
  { icon: Clock, title: 'auth.trustAlwaysTitle', text: 'auth.trustAlwaysText' },
  { icon: BarChart3, title: 'auth.trustReportsTitle', text: 'auth.trustReportsText' },
] as const

const currentYear = new Date().getFullYear()

export function AuthLayout() {
  const { t } = useTranslation()
  const isAuthenticated = useAppSelector(selectIsAuthenticated)

  if (isAuthenticated) return <Navigate to="/" replace />

  return (
    <div className="min-h-screen lg:grid lg:grid-cols-[minmax(0,5fr)_minmax(0,6fr)]">
      <aside className="relative hidden overflow-hidden bg-primary-dark px-12 py-10 text-white lg:flex lg:flex-col lg:justify-between">
        <div className="pointer-events-none absolute -right-32 -top-32 size-96 rounded-full bg-white/5" />
        <div className="pointer-events-none absolute -bottom-40 -left-20 size-[28rem] rounded-full bg-white/[0.03]" />
        <Brand tone="light" />
        <div className="relative flex max-w-md flex-col gap-8">
          <div className="flex flex-col gap-3">
            <h1 className="text-3xl font-semibold leading-tight tracking-tight">
              {t('auth.heroTitle')}
            </h1>
            <p className="text-primary-muted">{t('auth.heroText')}</p>
          </div>
          <ul className="flex flex-col gap-5">
            {trustItems.map(({ icon: Icon, title, text }) => (
              <li key={title} className="flex gap-4">
                <span className="flex size-10 shrink-0 items-center justify-center rounded-lg bg-white/10 ring-1 ring-white/15">
                  <Icon className="size-5" aria-hidden="true" />
                </span>
                <div>
                  <p className="font-medium">{t(title)}</p>
                  <p className="text-sm text-primary-muted">{t(text)}</p>
                </div>
              </li>
            ))}
          </ul>
        </div>
        <p className="relative text-xs text-primary-muted">
          {t('auth.copyright', { year: currentYear })}
        </p>
      </aside>

      <main className="flex min-h-screen flex-col px-4 py-6 sm:px-8">
        <div className="flex items-center justify-between lg:justify-end">
          <span className="lg:hidden">
            <Brand />
          </span>
          <LanguageSelector />
        </div>
        <div className="flex flex-1 items-center justify-center py-10">
          <div className="w-full max-w-md">
            <Suspense fallback={<PageFallback />}>
              <Outlet />
            </Suspense>
          </div>
        </div>
        <p className="text-center text-xs text-neutral-400 lg:hidden">
          {t('auth.copyright', { year: currentYear })}
        </p>
      </main>
    </div>
  )
}
