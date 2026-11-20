import { useId, type ComponentProps } from 'react'
import { FormField } from './FormField'
import { describedBy, inputClassName } from './styles'

export type SelectOption = { value: string; label: string }

export type SelectProps = Omit<ComponentProps<'select'>, 'children'> & {
  label?: string
  error?: string
  hint?: string
  options: SelectOption[]
  placeholder?: string
}

export function Select({
  id,
  label,
  error,
  hint,
  required,
  options,
  placeholder,
  className,
  ...props
}: SelectProps) {
  const generatedId = useId()
  const inputId = id ?? generatedId

  return (
    <FormField id={inputId} label={label} required={required} hint={hint} error={error}>
      <select
        id={inputId}
        required={required}
        aria-invalid={error ? true : undefined}
        aria-describedby={describedBy(inputId, error, hint)}
        className={inputClassName(Boolean(error), className)}
        {...props}
      >
        {placeholder !== undefined ? <option value="">{placeholder}</option> : null}
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    </FormField>
  )
}
