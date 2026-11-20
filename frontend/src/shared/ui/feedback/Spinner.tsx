import clsx from 'clsx'
import { LoaderCircle } from 'lucide-react'

export type SpinnerProps = {
  size?: 'sm' | 'md'
  label?: string
  className?: string
}

export function Spinner({ size = 'md', label, className }: SpinnerProps) {
  return (
    <span role="status" className={clsx('inline-flex items-center gap-2', className)}>
      <LoaderCircle
        className={clsx('animate-spin', size === 'sm' ? 'size-4' : 'size-5')}
        aria-hidden="true"
      />
      {label ? <span className="text-sm text-neutral-500">{label}</span> : null}
    </span>
  )
}
