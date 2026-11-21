import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { reportApiError } from '@/shared/api'
import { useApiFormErrors, useFieldError, useFormKeyboard } from '@/shared/hooks'
import { Button, PasswordInput, TextInput } from '@/shared/ui'
import { useLoginMutation } from '../authApi'
import { loginSchema, type LoginForm as LoginFormValues } from '../schemas'
import type { Session } from '../types'

export type LoginFormProps = {
  defaultEmail?: string
  onLoggedIn: (session: Session) => void
}

const fields = ['email', 'password'] as const

export function LoginForm({ defaultEmail = '', onLoggedIn }: LoginFormProps) {
  const { t } = useTranslation()
  const fieldError = useFieldError()
  const keyboard = useFormKeyboard()
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
    defaultValues: { email: defaultEmail, password: '' },
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
    <form noValidate onSubmit={onSubmit} {...keyboard} className="flex flex-col gap-4">
      <TextInput
        type="email"
        inputMode="email"
        autoComplete="email"
        autoFocus={!defaultEmail}
        label={t('auth.email')}
        required
        error={fieldError(errors.email)}
        {...register('email')}
      />
      <PasswordInput
        autoComplete="current-password"
        autoFocus={Boolean(defaultEmail)}
        enterKeyHint="go"
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
