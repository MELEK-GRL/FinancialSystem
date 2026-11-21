import { Suspense, useState } from 'react'
import { X } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Outlet } from 'react-router'
import { useMeQuery } from '@/features/auth'
import { IconButton } from '@/shared/ui'
import { Brand } from './Brand'
import { Header } from './Header'
import { PageFallback } from './PageFallback'
import { Sidebar } from './Sidebar'

export function AppLayout() {
  const { t } = useTranslation()
  const [menuOpen, setMenuOpen] = useState(false)
  useMeQuery()

  return (
    <div className="flex min-h-screen">
      <aside className="hidden w-64 shrink-0 border-r border-neutral-200 bg-white md:block">
        <div className="flex h-16 items-center border-b border-neutral-200 px-5">
          <Brand />
        </div>
        <Sidebar />
      </aside>

      {menuOpen ? (
        <div className="fixed inset-0 z-40 md:hidden">
          <button
            type="button"
            aria-label={t('nav.closeMenu')}
            className="absolute inset-0 bg-neutral-900/40"
            onClick={() => setMenuOpen(false)}
          />
          <aside className="relative h-full w-72 bg-white shadow-lg">
            <div className="flex h-16 items-center justify-between border-b border-neutral-200 px-4">
              <Brand />
              <IconButton
                aria-label={t('nav.closeMenu')}
                icon={<X className="size-5" />}
                onClick={() => setMenuOpen(false)}
              />
            </div>
            <Sidebar onNavigate={() => setMenuOpen(false)} />
          </aside>
        </div>
      ) : null}

      <div className="flex min-w-0 flex-1 flex-col">
        <Header onOpenMenu={() => setMenuOpen(true)} />
        <main className="flex-1 px-4 py-6 md:px-8 md:py-8">
          <Suspense fallback={<PageFallback />}>
            <Outlet />
          </Suspense>
        </main>
      </div>
    </div>
  )
}
