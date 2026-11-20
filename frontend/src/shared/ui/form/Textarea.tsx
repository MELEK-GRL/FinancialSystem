import { useId, useState, type ComponentProps } from 'react'
import clsx from 'clsx'
import { FormField } from './FormField'
import { describedBy, inputClassName } from './styles'

export type TextareaProps = ComponentProps<'textarea'> & {
  label?: string
  error?: string
  hint?: string
}

export function Textarea({
  id,
  label,
  error,
  hint,
  required,
  maxLength,
  onChange,
  defaultValue,
  className,
  ...props
}: TextareaProps) {
  const generatedId = useId()
  const inputId = id ?? generatedId
  const [length, setLength] = useState(String(defaultValue ?? '').length)

  return (
    <FormField id={inputId} label={label} required={required} hint={hint} error={error}>
      <textarea
        id={inputId}
        rows={3}
        required={required}
        maxLength={maxLength}
        defaultValue={defaultValue}
        aria-invalid={error ? true : undefined}
        aria-describedby={describedBy(inputId, error, hint)}
        onChange={(event) => {
          setLength(event.target.value.length)
          onChange?.(event)
        }}
        className={inputClassName(Boolean(error), clsx('h-auto py-2', className))}
        {...props}
      />
      {maxLength ? (
        <span className="self-end text-xs tabular-nums text-neutral-500">
          {length} / {maxLength}
        </span>
      ) : null}
    </FormField>
  )
}
