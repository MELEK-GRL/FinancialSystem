import { useId, type ComponentProps, type ReactNode } from 'react'
import { FieldError } from './FieldError'

export type CheckboxProps = Omit<ComponentProps<'input'>, 'type'> & {
  label: ReactNode
  error?: string
}

export function Checkbox({ id, label, error, ...props }: CheckboxProps) {
  const generatedId = useId()
  const inputId = id ?? generatedId

  return (
    <div className="flex flex-col gap-1.5">
      <label htmlFor={inputId} className="flex items-start gap-2 text-sm text-neutral-800">
        <input
          id={inputId}
          type="checkbox"
          aria-invalid={error ? true : undefined}
          aria-describedby={error ? `${inputId}-error` : undefined}
          className="mt-0.5 size-4 rounded border-neutral-300 accent-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary"
          {...props}
        />
        <span>{label}</span>
      </label>
      <FieldError id={`${inputId}-error`} message={error} />
    </div>
  )
}
