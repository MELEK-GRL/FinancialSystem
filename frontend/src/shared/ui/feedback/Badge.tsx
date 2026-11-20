import clsx from 'clsx'
import type { ReactNode } from 'react'

export type BadgeProps = {
  tone?: 'neutral' | 'primary' | 'success' | 'danger' | 'warning'
  children: ReactNode
}

const tones = {
  neutral: 'bg-neutral-100 text-neutral-700',
  primary: 'bg-primary-soft text-primary',
  success: 'bg-success-soft text-success',
  danger: 'bg-danger-soft text-danger',
  warning: 'bg-warning-soft text-warning',
} as const

export function Badge({ tone = 'neutral', children }: BadgeProps) {
  return (
    <span
      className={clsx(
        'inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium',
        tones[tone],
      )}
    >
      {children}
    </span>
  )
}
