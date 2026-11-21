import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { reportApiError } from '@/shared/api'
import { useApiFormErrors, useFieldError } from '@/shared/hooks'
import { Button, PasswordInput, PhoneInput, TcknInput, TextInput } from '@/shared/ui'
import { useLoginMutation, useRegisterMutation } from '../authApi'
import { registerSchema, type RegisterFormInput, type RegisterFormOutput } from '../schemas'
import type { Session } from '../types'

export type RegisterFormProps = { onRegistered: (session: Session, customerNo: string) => void }

const fields = ['firstName', 'lastName', 'nationalId', 'email', 'phone', 'password'] as const

export function RegisterForm({ onRegistered }: RegisterFormProps) {
  const { t } = useTranslation()
  const fieldError = useFieldError()
  const [registerCustomer, registerState] = useRegisterMutation()
  const [login, loginState] = useLoginMutation()
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<RegisterFormInput, unknown, RegisterFormOutput>({
    resolver: zodResolver(registerSchema),
    mode: 'onBlur',
    reValidateMode: 'onChange',
  })
  const applyApiErrors = useApiFormErrors(setError, fields)

  const onSubmit = handleSubmit(
    async ({ firstName, lastName, nationalId, email, phone, password }) => {
      try {
        const { customerNo } = await registerCustomer({
          firstName,
          lastName,
          nationalId,
          email,
          phone,
          password,
        }).unwrap()
        onRegistered(await login({ email, password }).unwrap(), customerNo)
      } catch (error) {
        reportApiError(error, applyApiErrors)
      }
    },
  )

  const passwordLabels = { showLabel: t('form.showPassword'), hideLabel: t('form.hidePassword') }

  return (
    <form noValidate onSubmit={onSubmit} className="flex flex-col gap-4">
      <div className="grid gap-4 sm:grid-cols-2">
        <TextInput
          autoComplete="given-name"
          label={t('auth.firstName')}
          required
          error={fieldError(errors.firstName)}
          {...register('firstName')}
        />
        <TextInput
          autoComplete="family-name"
          label={t('auth.lastName')}
          required
          error={fieldError(errors.lastName)}
          {...register('lastName')}
        />
      </div>
      <TcknInput
        label={t('auth.nationalId')}
        required
        error={fieldError(errors.nationalId)}
        {...register('nationalId')}
      />
      <TextInput
        type="email"
        autoComplete="email"
        label={t('auth.email')}
        required
        error={fieldError(errors.email)}
        {...register('email')}
      />
      <PhoneInput
        label={t('auth.phone')}
        required
        error={fieldError(errors.phone)}
        {...register('phone')}
      />
      <PasswordInput
        autoComplete="new-password"
        label={t('auth.password')}
        hint={t('auth.passwordHint')}
        required
        error={fieldError(errors.password)}
        {...passwordLabels}
        {...register('password')}
      />
      <PasswordInput
        autoComplete="new-password"
        label={t('auth.passwordConfirm')}
        required
        error={fieldError(errors.passwordConfirm)}
        {...passwordLabels}
        {...register('passwordConfirm')}
      />
      <Button
        type="submit"
        loading={registerState.isLoading || loginState.isLoading}
        className="w-full"
      >
        {t('auth.registerButton')}
      </Button>
    </form>
  )
}
