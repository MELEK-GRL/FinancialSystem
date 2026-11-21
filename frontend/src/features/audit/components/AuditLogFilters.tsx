import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { useFieldError, useFormKeyboard } from '@/shared/hooks'
import { Button, DateRangeInput, Select } from '@/shared/ui'
import { auditActions } from '../hooks/useAuditParams'
import { auditFilterSchema, type AuditFilterInput, type AuditFilterOutput } from '../schemas'
import type { AuditAction, AuditFilters } from '../types'

export type AuditLogFiltersProps = {
  filters: AuditFilters
  onApply: (filters: Omit<AuditFilters, 'accountId'>) => void
  onClear: () => void
}

export function AuditLogFilters({ filters, onApply, onClear }: AuditLogFiltersProps) {
  const { t } = useTranslation()
  const fieldError = useFieldError()
  const keyboard = useFormKeyboard()
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<AuditFilterInput, unknown, AuditFilterOutput>({
    resolver: zodResolver(auditFilterSchema),
    defaultValues: {
      from: filters.from ?? '',
      to: filters.to ?? '',
      action: filters.action ?? '',
    },
  })

  const onSubmit = handleSubmit((values) =>
    onApply({
      from: values.from,
      to: values.to,
      action: values.action as AuditAction | undefined,
    }),
  )

  return (
    <form
      noValidate
      {...keyboard}
      onSubmit={onSubmit}
      className="grid gap-4 lg:grid-cols-[2fr_1fr_auto] lg:items-start"
    >
      <DateRangeInput
        label={t('audit.dateRange')}
        fromLabel={t('audit.from')}
        toLabel={t('audit.to')}
        fromProps={register('from')}
        toProps={register('to')}
        error={fieldError(errors.to)}
      />
      <Select
        label={t('audit.action')}
        placeholder={t('audit.allActions')}
        options={auditActions.map((value) => ({ value, label: t(`audit.actions.${value}`) }))}
        {...register('action')}
      />
      <div className="flex justify-end gap-2 lg:pt-7">
        <Button variant="secondary" onClick={onClear}>
          {t('audit.clear')}
        </Button>
        <Button type="submit">{t('audit.apply')}</Button>
      </div>
    </form>
  )
}
