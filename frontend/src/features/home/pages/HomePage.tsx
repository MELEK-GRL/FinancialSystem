import { Send } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import { useAppSelector } from '@/app/hooks'
import { AccountsSection } from '@/features/accounts'
import { selectCurrentUser, selectRole } from '@/features/auth'
import { Button, Card, Text } from '@/shared/ui'

export function HomePage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const user = useAppSelector(selectCurrentUser)
  const role = useAppSelector(selectRole)
  const name = [user?.firstName, user?.lastName].filter(Boolean).join(' ')

  return (
    <div className="flex flex-col gap-6">
      <section className="relative overflow-hidden rounded-2xl bg-primary-dark px-6 py-8 text-white sm:px-8">
        <div className="pointer-events-none absolute -right-16 -top-24 size-72 rounded-full bg-white/5" />
        <div className="relative flex flex-col gap-5 sm:flex-row sm:items-end sm:justify-between">
          <div className="flex flex-col gap-2">
            <p className="text-sm text-primary-muted">{t('home.welcomeLabel')}</p>
            <h1 className="text-2xl font-semibold tracking-tight sm:text-3xl">
              {name ? t('home.greeting', { name }) : t('home.greetingFallback')}
            </h1>
            <div className="flex flex-wrap items-center gap-3 text-sm text-primary-muted">
              {role ? (
                <span className="rounded-full bg-white/10 px-3 py-0.5 font-medium text-white ring-1 ring-white/15">
                  {t(`roles.${role}`)}
                </span>
              ) : null}
              {user?.customerNo ? (
                <span>{t('home.customerNo', { value: user.customerNo })}</span>
              ) : null}
            </div>
          </div>
          {role === 'CUSTOMER' ? (
            <Button
              variant="inverse"
              icon={<Send className="size-4" />}
              className="self-start sm:self-auto"
              onClick={() => navigate('/transfer')}
            >
              {t('nav.transfer')}
            </Button>
          ) : null}
        </div>
      </section>
      {role === 'CUSTOMER' ? (
        <AccountsSection />
      ) : (
        <Card>
          <Text>{t('home.adminIntro')}</Text>
        </Card>
      )}
    </div>
  )
}
