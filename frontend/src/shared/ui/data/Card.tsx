import clsx from 'clsx'
import type { ReactNode } from 'react'
import { Heading } from '../typography/Heading'

export type CardProps = {
  title?: string
  actions?: ReactNode
  padded?: boolean
  children: ReactNode
  className?: string
}

export function Card({ title, actions, padded = true, children, className }: CardProps) {
  return (
    <section className={clsx('rounded-xl border border-neutral-200 bg-white shadow-sm', className)}>
      {title || actions ? (
        <header className="flex items-center justify-between gap-4 border-b border-neutral-200 px-6 py-4">
          {title ? <Heading level={2}>{title}</Heading> : <span />}
          {actions}
        </header>
      ) : null}
      <div className={clsx(padded && 'p-6')}>{children}</div>
    </section>
  )
}
