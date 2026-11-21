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
    <div className="flex min-h-screen bg-neutral-50">
      <aside className="sticky top-0 hidden h-screen w-64 shrink-0 bg-primary-dark md:block">
        <div className="flex h-16 items-center border-b border-white/10 px-5">
          <Brand tone="light" />
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
          <aside className="relative h-full w-72 bg-primary-dark shadow-xl">
            <div className="flex h-16 items-center justify-between border-b border-white/10 px-4">
              <Brand tone="light" />
              <IconButton
                aria-label={t('nav.closeMenu')}
                icon={<X className="size-5" />}
                className="text-white/80 hover:bg-white/10 hover:text-white"
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
