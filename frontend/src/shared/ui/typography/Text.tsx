import clsx from 'clsx'
import type { ReactNode } from 'react'

export type TextProps = {
  variant?: 'body' | 'muted' | 'small'
  as?: 'p' | 'span'
  children: ReactNode
  className?: string
}

const styles = {
  body: 'text-sm text-neutral-800',
  muted: 'text-sm text-neutral-500',
  small: 'text-xs text-neutral-500',
} as const

export function Text({ variant = 'body', as: Tag = 'p', children, className }: TextProps) {
  return <Tag className={clsx(styles[variant], className)}>{children}</Tag>
}
