import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import { useAppDispatch } from '@/app/hooks'
import { notify } from '@/shared/lib'
import { Heading, Link, Text } from '@/shared/ui'
import { loggedIn } from '../authSlice'
import { RegisterForm } from '../components/RegisterForm'

export function RegisterPage() {
  const { t } = useTranslation()
  const dispatch = useAppDispatch()
  const navigate = useNavigate()

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-col gap-1">
        <Heading>{t('auth.registerTitle')}</Heading>
        <Text variant="muted">{t('auth.registerSubtitle')}</Text>
      </div>
      <RegisterForm
        onRegistered={(session) => {
          dispatch(loggedIn(session))
          notify.success(t('auth.registered'))
          navigate('/', { replace: true })
        }}
      />
      <Text variant="muted" className="text-center">
        {t('auth.haveAccount')} <Link to="/login">{t('auth.goLogin')}</Link>
      </Text>
    </div>
  )
}
