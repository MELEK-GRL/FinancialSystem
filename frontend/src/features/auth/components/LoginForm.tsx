import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { useApiFormErrors, useFieldError } from '@/shared/hooks'
import { reportApiError } from '@/shared/api'
import { Button, PasswordInput, TextInput } from '@/shared/ui'
import { useLoginMutation } from '../authApi'
import { loginSchema, type LoginForm as LoginFormValues } from '../schemas'
import type { Session } from '../types'

export type LoginFormProps = { onLoggedIn: (session: Session) => void }

const fields = ['email', 'password'] as const

export function LoginForm({ onLoggedIn }: LoginFormProps) {
  const { t } = useTranslation()
  const fieldError = useFieldError()
  const [login, { isLoading }] = useLoginMutation()
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    mode: 'onBlur',
    reValidateMode: 'onChange',
  })
  const applyApiErrors = useApiFormErrors(setError, fields)

  const onSubmit = handleSubmit(async (values) => {
    try {
      onLoggedIn(await login(values).unwrap())
    } catch (error) {
      reportApiError(error, applyApiErrors)
    }
  })

  return (
    <form noValidate onSubmit={onSubmit} className="flex flex-col gap-4">
      <TextInput
        type="email"
        autoComplete="email"
        label={t('auth.email')}
        required
        error={fieldError(errors.email)}
        {...register('email')}
      />
      <PasswordInput
        autoComplete="current-password"
        label={t('auth.password')}
        required
        showLabel={t('form.showPassword')}
        hideLabel={t('form.hidePassword')}
        error={fieldError(errors.password)}
        {...register('password')}
      />
      <Button type="submit" loading={isLoading} className="w-full">
        {t('auth.loginButton')}
      </Button>
    </form>
  )
}
