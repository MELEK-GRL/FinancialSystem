import { useId, type ComponentProps, type ReactNode } from 'react'
import clsx from 'clsx'
import { FormField } from './FormField'
import { describedBy, inputClassName } from './styles'

export type TextInputProps = Omit<ComponentProps<'input'>, 'size'> & {
  label?: string
  error?: string
  hint?: string
  invalid?: boolean
  suffix?: ReactNode
}

export function TextInput({
  id,
  label,
  error,
  hint,
  invalid,
  required,
  suffix,
  className,
  ...props
}: TextInputProps) {
  const generatedId = useId()
  const inputId = id ?? generatedId
  const hasError = Boolean(error) || Boolean(invalid)

  return (
    <FormField id={inputId} label={label} required={required} hint={hint} error={error}>
      <div className="relative">
        <input
          id={inputId}
          enterKeyHint="next"
          required={required}
          aria-invalid={hasError || undefined}
          aria-describedby={describedBy(inputId, error, hint)}
          className={inputClassName(hasError, clsx(suffix ? 'pr-10' : undefined, className))}
          {...props}
        />
        {suffix ? (
          <span className="absolute inset-y-0 right-0 flex items-center pr-3 text-sm text-neutral-500">
            {suffix}
          </span>
        ) : null}
      </div>
    </FormField>
  )
}
