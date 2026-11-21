import { useTranslation } from 'react-i18next'
import { useAppSelector } from '@/app/hooks'
import { selectCurrentUser, selectRole } from '@/features/auth'
import { Badge, Card, Heading, Text } from '@/shared/ui'

export function HomePage() {
  const { t } = useTranslation()
  const user = useAppSelector(selectCurrentUser)
  const role = useAppSelector(selectRole)
  const name = [user?.firstName, user?.lastName].filter(Boolean).join(' ')

  return (
    <Card>
      <div className="flex flex-col gap-3">
        <div className="flex flex-wrap items-center gap-3">
          <Heading>{name ? t('home.greeting', { name }) : t('home.greetingFallback')}</Heading>
          {role ? <Badge tone="primary">{t(`roles.${role}`)}</Badge> : null}
        </div>
        {user?.customerNo ? (
          <Text variant="muted">{t('home.customerNo', { value: user.customerNo })}</Text>
        ) : null}
        <Text>{role === 'ADMIN' ? t('home.adminIntro') : t('home.customerIntro')}</Text>
      </div>
    </Card>
  )
}
