import { AlertTriangle, RotateCw } from 'lucide-react'
import { Button } from '../actions/Button'
import { Heading } from '../typography/Heading'
import { Text } from '../typography/Text'

export type ErrorStateProps = {
  title: string
  description?: string
  retryLabel?: string
  onRetry?: () => void
}

export function ErrorState({ title, description, retryLabel, onRetry }: ErrorStateProps) {
  return (
    <div role="alert" className="flex flex-col items-center gap-3 px-6 py-12 text-center">
      <AlertTriangle className="size-8 text-danger" aria-hidden="true" />
      <Heading level={3}>{title}</Heading>
      {description ? <Text variant="muted">{description}</Text> : null}
      {onRetry && retryLabel ? (
        <Button
          variant="secondary"
          size="sm"
          icon={<RotateCw className="size-4" />}
          onClick={onRetry}
        >
          {retryLabel}
        </Button>
      ) : null}
    </div>
  )
}
