import { useTranslation } from 'react-i18next'
import { useLocation, useNavigate } from 'react-router'
import { useAppDispatch } from '@/app/hooks'
import { Heading, Link, Text } from '@/shared/ui'
import { loggedIn } from '../authSlice'
import { LoginForm } from '../components/LoginForm'

export function LoginPage() {
  const { t } = useTranslation()
  const dispatch = useAppDispatch()
  const navigate = useNavigate()
  const from = (useLocation().state as { from?: string } | null)?.from ?? '/'

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-col gap-1">
        <Heading>{t('auth.loginTitle')}</Heading>
        <Text variant="muted">{t('auth.loginSubtitle')}</Text>
      </div>
      <LoginForm
        onLoggedIn={(session) => {
          dispatch(loggedIn(session))
          navigate(from, { replace: true })
        }}
      />
      <Text variant="muted" className="text-center">
        {t('auth.noAccount')} <Link to="/register">{t('auth.goRegister')}</Link>
      </Text>
    </div>
  )
}
