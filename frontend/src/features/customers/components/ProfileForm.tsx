import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { getProblemDetails, reportApiError } from '@/shared/api'
import { useApiFormErrors, useFieldError, useFormKeyboard } from '@/shared/hooks'
import { notify } from '@/shared/lib'
import { Button, formatPhone, PhoneInput, TextInput } from '@/shared/ui'
import { useUpdateMyProfileMutation } from '../customersApi'
import { profileSchema, type ProfileFormInput, type ProfileFormOutput } from '../schemas'
import type { Customer } from '../types'

const fields = ['email', 'phone'] as const

export function ProfileForm({ customer }: { customer: Customer }) {
  const { t } = useTranslation()
  const fieldError = useFieldError()
  const keyboard = useFormKeyboard()
  const [updateProfile, { isLoading }] = useUpdateMyProfileMutation()
  const {
    register,
    handleSubmit,
    reset,
    setError,
    formState: { errors, isDirty },
  } = useForm<ProfileFormInput, unknown, ProfileFormOutput>({
    resolver: zodResolver(profileSchema),
    mode: 'onBlur',
    reValidateMode: 'onChange',
    defaultValues: { email: customer.email, phone: formatPhone(customer.phone) },
  })
  const applyApiErrors = useApiFormErrors(setError, fields)

  const onSubmit = handleSubmit(async (values) => {
    try {
      const updated = await updateProfile(values).unwrap()
      reset({ email: updated.email, phone: formatPhone(updated.phone) })
      notify.success(t('customers.saved'))
    } catch (error) {
      const problem = getProblemDetails(error)
      if (problem?.errorCode === 'DUPLICATE_CUSTOMER') {
        setError('email', { type: 'server', message: problem.title })
        return
      }
      reportApiError(error, applyApiErrors)
    }
  })

  return (
    <form noValidate {...keyboard} onSubmit={onSubmit} className="flex max-w-md flex-col gap-4">
      <TextInput
        type="email"
        autoComplete="email"
        label={t('customers.email')}
        hint={t('customers.contactHint')}
        required
        error={fieldError(errors.email)}
        {...register('email')}
      />
      <PhoneInput
        label={t('customers.phone')}
        required
        error={fieldError(errors.phone)}
        {...register('phone')}
      />
      <div>
        <Button type="submit" loading={isLoading} disabled={!isDirty}>
          {t('customers.save')}
        </Button>
      </div>
    </form>
  )
}
