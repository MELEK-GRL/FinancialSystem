import { LogOut, Menu } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useAppSelector } from '@/app/hooks'
import { selectCurrentUser, selectRole, useLogout } from '@/features/auth'
import { LanguageSelector } from '@/shared/i18n'
import { Button, IconButton, Text } from '@/shared/ui'
import { Brand } from './Brand'

export type HeaderProps = { onOpenMenu: () => void }

export function Header({ onOpenMenu }: HeaderProps) {
  const { t } = useTranslation()
  const user = useAppSelector(selectCurrentUser)
  const role = useAppSelector(selectRole)
  const logout = useLogout()
  const name = [user?.firstName, user?.lastName].filter(Boolean).join(' ') || user?.email

  return (
    <header className="flex h-16 items-center justify-between gap-4 border-b border-neutral-200 bg-white px-4 md:px-8">
      <div className="flex items-center gap-2 md:hidden">
        <IconButton
          aria-label={t('nav.menu')}
          icon={<Menu className="size-5" />}
          onClick={onOpenMenu}
        />
        <Brand compact />
      </div>
      <div className="ml-auto flex items-center gap-3">
        <LanguageSelector />
        {name ? (
          <div className="hidden text-right sm:block">
            <Text className="font-medium">{name}</Text>
            {role ? <Text variant="small">{t(`roles.${role}`)}</Text> : null}
          </div>
        ) : null}
        <Button
          variant="ghost"
          size="sm"
          aria-label={t('auth.logout')}
          icon={<LogOut className="size-4" />}
          onClick={logout}
        >
          <span className="hidden sm:inline">{t('auth.logout')}</span>
        </Button>
      </div>
    </header>
  )
}
