import type { ReactNode } from 'react'
import { Heading } from './Heading'
import { Text } from './Text'

export type PageHeaderProps = {
  title: ReactNode
  subtitle?: string
  actions?: ReactNode
}

export function PageHeader({ title, subtitle, actions }: PageHeaderProps) {
  return (
    <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
      <div className="flex flex-col gap-1">
        <Heading>{title}</Heading>
        {subtitle ? <Text variant="muted">{subtitle}</Text> : null}
      </div>
      {actions ? <div className="flex shrink-0 gap-2">{actions}</div> : null}
    </div>
  )
}
