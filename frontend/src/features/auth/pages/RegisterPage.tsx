import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import { popup } from '@/shared/lib'
import { Heading, Link, Text } from '@/shared/ui'
import { RegisterForm } from '../components/RegisterForm'

export function RegisterPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-col gap-1">
        <Heading>{t('auth.registerTitle')}</Heading>
        <Text variant="muted">{t('auth.registerSubtitle')}</Text>
      </div>
      <RegisterForm
        onRegistered={(customerNo, email) =>
          popup.success({
            title: t('auth.registeredTitle'),
            description: t('auth.registeredBody', { customerNo }),
            actionLabel: t('auth.registeredAction'),
            onClose: () => navigate('/login', { replace: true, state: { email } }),
          })
        }
      />
      <Text variant="muted" className="text-center">
        {t('auth.haveAccount')} <Link to="/login">{t('auth.goLogin')}</Link>
      </Text>
    </div>
  )
}
