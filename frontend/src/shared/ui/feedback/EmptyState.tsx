import type { ReactNode } from 'react'
import { Inbox } from 'lucide-react'
import { Heading } from '../typography/Heading'
import { Text } from '../typography/Text'

export type EmptyStateProps = {
  title: string
  description?: string
  icon?: ReactNode
  action?: ReactNode
}

export function EmptyState({ title, description, icon, action }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center gap-3 px-6 py-12 text-center">
      <span className="text-neutral-400">
        {icon ?? <Inbox className="size-8" aria-hidden="true" />}
      </span>
      <Heading level={3}>{title}</Heading>
      {description ? <Text variant="muted">{description}</Text> : null}
      {action}
    </div>
  )
}
