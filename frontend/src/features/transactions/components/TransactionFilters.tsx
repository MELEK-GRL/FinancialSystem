import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { useFieldError, useFormKeyboard, useLocale } from '@/shared/hooks'
import { AmountInput, Button, DateRangeInput, Select } from '@/shared/ui'
import { transactionStatuses, transactionTypes } from '../hooks/useHistoryParams'
import { historyFilterSchema, type HistoryFilterInput, type HistoryFilterOutput } from '../schemas'
import type { HistoryFilters, TransactionStatus, TransactionType } from '../types'

export type TransactionFiltersProps = {
  filters: HistoryFilters
  onApply: (filters: HistoryFilters) => void
  onClear: () => void
}

export function TransactionFilters({ filters, onApply, onClear }: TransactionFiltersProps) {
  const { t } = useTranslation()
  const fieldError = useFieldError()
  const keyboard = useFormKeyboard()
  const decimalSeparator = useLocale() === 'tr-TR' ? ',' : '.'
  const toInput = (value?: string) => value?.replace('.', decimalSeparator) ?? ''

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<HistoryFilterInput, unknown, HistoryFilterOutput>({
    resolver: zodResolver(historyFilterSchema),
    defaultValues: {
      from: filters.from ?? '',
      to: filters.to ?? '',
      type: filters.type ?? '',
      status: filters.status ?? '',
      minAmount: toInput(filters.minAmount),
      maxAmount: toInput(filters.maxAmount),
    },
  })

  const onSubmit = handleSubmit((values) =>
    onApply({
      from: values.from,
      to: values.to,
      type: values.type as TransactionType | undefined,
      status: values.status as TransactionStatus | undefined,
      minAmount: values.minAmount?.toString(),
      maxAmount: values.maxAmount?.toString(),
    }),
  )

  return (
    <form noValidate {...keyboard} onSubmit={onSubmit} className="grid gap-4 lg:grid-cols-2">
      <DateRangeInput
        label={t('history.dateRange')}
        fromLabel={t('history.from')}
        toLabel={t('history.to')}
        fromProps={register('from')}
        toProps={register('to')}
        error={fieldError(errors.to)}
      />
      <div className="grid grid-cols-2 gap-3">
        <Select
          label={t('history.typeLabel')}
          placeholder={t('history.all')}
          options={transactionTypes.map((value) => ({
            value,
            label: t(`history.type.${value}`),
          }))}
          {...register('type')}
        />
        <Select
          label={t('history.statusLabel')}
          placeholder={t('history.all')}
          options={transactionStatuses.map((value) => ({
            value,
            label: t(`history.status.${value}`),
          }))}
          {...register('status')}
        />
      </div>
      <div className="grid grid-cols-2 gap-3">
        <AmountInput
          label={t('history.minAmount')}
          error={fieldError(errors.minAmount)}
          {...register('minAmount')}
        />
        <AmountInput
          label={t('history.maxAmount')}
          error={fieldError(errors.maxAmount)}
          {...register('maxAmount')}
        />
      </div>
      <div className="flex items-end justify-end gap-2">
        <Button variant="secondary" onClick={onClear}>
          {t('history.clear')}
        </Button>
        <Button type="submit">{t('history.apply')}</Button>
      </div>
    </form>
  )
}
