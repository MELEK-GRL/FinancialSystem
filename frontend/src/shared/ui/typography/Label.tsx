import type { ReactNode } from 'react'

export type LabelProps = {
  htmlFor: string
  required?: boolean
  children: ReactNode
}

export function Label({ htmlFor, required, children }: LabelProps) {
  return (
    <label htmlFor={htmlFor} className="block text-sm font-medium text-neutral-700">
      {children}
      {required && (
        <span className="ml-0.5 text-danger" aria-hidden="true">
          *
        </span>
      )}
    </label>
  )
}
