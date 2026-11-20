import type { ComponentProps } from 'react'
import { FieldError } from './FieldError'
import { DateInput } from './DateInput'

export type DateRangeInputProps = {
  label: string
  fromLabel: string
  toLabel: string
  fromProps: ComponentProps<'input'>
  toProps: ComponentProps<'input'>
  error?: string
}

export function DateRangeInput({
  label,
  fromLabel,
  toLabel,
  fromProps,
  toProps,
  error,
}: DateRangeInputProps) {
  const errorId = `${fromProps.name ?? 'date-range'}-range-error`

  return (
    <fieldset className="flex flex-col gap-1.5" aria-describedby={error ? errorId : undefined}>
      <legend className="sr-only">{label}</legend>
      <div className="grid grid-cols-2 gap-3">
        <DateInput label={fromLabel} {...fromProps} />
        <DateInput label={toLabel} invalid={Boolean(error)} {...toProps} />
      </div>
      <FieldError id={errorId} message={error} />
    </fieldset>
  )
}
