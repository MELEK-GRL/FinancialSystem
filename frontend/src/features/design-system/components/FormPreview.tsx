import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { useFieldError } from '@/shared/hooks'
import { notify } from '@/shared/lib'
import {
  AmountInput,
  Button,
  Checkbox,
  DateRangeInput,
  PasswordInput,
  PhoneInput,
  Select,
  TcknInput,
  Text,
  Textarea,
  TextInput,
} from '@/shared/ui'
import {
  previewFormSchema,
  transactionTypes,
  type PreviewFormInput,
  type PreviewFormOutput,
} from '../schemas'

export function FormPreview() {
  const { t } = useTranslation()
  const fieldError = useFieldError()
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<PreviewFormInput, unknown, PreviewFormOutput>({
    resolver: zodResolver(previewFormSchema),
    mode: 'onBlur',
    reValidateMode: 'onChange',
    defaultValues: { type: undefined, approve: false, from: '', to: '' },
  })

  const label = (key: keyof PreviewFormInput) => t(`designSystem.form.${key}`)

  return (
    <form
      noValidate
      onSubmit={handleSubmit(() => notify.success(t('designSystem.form.valid')))}
      className="grid gap-4 md:grid-cols-2"
    >
      <TextInput
        label={label('firstName')}
        required
        error={fieldError(errors.firstName)}
        {...register('firstName')}
      />
      <TextInput
        type="email"
        label={label('email')}
        required
        error={fieldError(errors.email)}
        {...register('email')}
      />
      <TcknInput
        label={label('nationalId')}
        required
        error={fieldError(errors.nationalId)}
        {...register('nationalId')}
      />
      <PhoneInput
        label={label('phone')}
        required
        error={fieldError(errors.phone)}
        {...register('phone')}
      />
      <PasswordInput
        label={label('password')}
        required
        showLabel={t('form.showPassword')}
        hideLabel={t('form.hidePassword')}
        error={fieldError(errors.password)}
        {...register('password')}
      />
      <AmountInput
        label={label('amount')}
        required
        error={fieldError(errors.amount)}
        {...register('amount')}
      />
      <TextInput
        label={label('accountNo')}
        required
        placeholder="TR1000000001"
        error={fieldError(errors.accountNo)}
        {...register('accountNo')}
      />
      <Select
        label={label('type')}
        required
        placeholder={t('designSystem.form.typePlaceholder')}
        options={transactionTypes.map((type) => ({
          value: type,
          label: t(`designSystem.types.${type}`),
        }))}
        error={fieldError(errors.type)}
        {...register('type')}
      />
      <div className="md:col-span-2">
        <DateRangeInput
          label={t('designSystem.form.dateRange')}
          fromLabel={t('designSystem.form.from')}
          toLabel={t('designSystem.form.to')}
          fromProps={register('from')}
          toProps={register('to')}
          error={fieldError(errors.to)}
        />
      </div>
      <div className="md:col-span-2">
        <Textarea
          label={label('description')}
          maxLength={250}
          error={fieldError(errors.description)}
          {...register('description')}
        />
      </div>
      <div className="md:col-span-2">
        <Checkbox
          label={label('approve')}
          error={fieldError(errors.approve)}
          {...register('approve')}
        />
      </div>
      <div className="flex items-center gap-3 md:col-span-2">
        <Button type="submit" loading={isSubmitting}>
          {t('designSystem.form.submit')}
        </Button>
        <Text variant="muted">{t('designSystem.form.hint')}</Text>
      </div>
    </form>
  )
}
