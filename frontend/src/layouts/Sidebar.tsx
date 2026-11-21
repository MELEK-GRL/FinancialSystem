import clsx from 'clsx'
import { useTranslation } from 'react-i18next'
import { NavLink } from 'react-router'
import { useAppSelector } from '@/app/hooks'
import { selectRole } from '@/features/auth'
import { navigationItems } from './navigation'

export type SidebarProps = { onNavigate?: () => void }

export function Sidebar({ onNavigate }: SidebarProps) {
  const { t } = useTranslation()
  const role = useAppSelector(selectRole)
  const items = navigationItems.filter((item) => role && item.roles.includes(role))

  return (
    <nav aria-label={t('nav.main')} className="flex flex-col gap-1 p-3">
      {items.map(({ to, labelKey, icon: Icon }) => (
        <NavLink
          key={to}
          to={to}
          end={to === '/'}
          onClick={onNavigate}
          className={({ isActive }) =>
            clsx(
              'flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors duration-150',
              'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary',
              isActive
                ? 'bg-primary-soft text-primary'
                : 'text-neutral-600 hover:bg-neutral-100 hover:text-neutral-900',
            )
          }
        >
          <Icon className="size-4" aria-hidden="true" />
          {t(labelKey)}
        </NavLink>
      ))}
    </nav>
  )
}
