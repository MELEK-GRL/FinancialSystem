import type { ReactNode } from 'react'
import { Label } from '../typography/Label'
import { FieldError } from './FieldError'

export type FormFieldProps = {
  id: string
  label?: string
  required?: boolean
  hint?: string
  error?: string
  children: ReactNode
}

export function FormField({ id, label, required, hint, error, children }: FormFieldProps) {
  return (
    <div className="flex flex-col gap-1.5">
      {label ? (
        <Label htmlFor={id} required={required}>
          {label}
        </Label>
      ) : null}
      {children}
      {hint && !error ? (
        <p id={`${id}-hint`} className="text-xs text-neutral-500">
          {hint}
        </p>
      ) : null}
      <FieldError id={`${id}-error`} message={error} />
    </div>
  )
}
