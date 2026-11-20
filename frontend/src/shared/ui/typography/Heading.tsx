import clsx from 'clsx'
import type { ReactNode } from 'react'

export type HeadingProps = {
  level?: 1 | 2 | 3
  children: ReactNode
  className?: string
}

const styles = {
  1: 'text-2xl font-semibold tracking-tight',
  2: 'text-lg font-semibold',
  3: 'text-base font-semibold',
} as const

export function Heading({ level = 1, children, className }: HeadingProps) {
  const Tag = `h${level}` as const
  return <Tag className={clsx(styles[level], 'text-neutral-900', className)}>{children}</Tag>
}
