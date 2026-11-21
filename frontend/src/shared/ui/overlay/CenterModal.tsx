import { useId, type ReactNode } from 'react'
import { X } from 'lucide-react'
import { IconButton } from '../actions/IconButton'
import { Heading } from '../typography/Heading'
import { DialogShell } from './DialogShell'

export type CenterModalProps = {
  open: boolean
  title: string
  closeLabel: string
  onClose: () => void
  children: ReactNode
  footer?: ReactNode
  size?: 'sm' | 'md' | 'lg'
}

export function CenterModal({
  open,
  title,
  closeLabel,
  onClose,
  children,
  footer,
  size = 'md',
}: CenterModalProps) {
  const titleId = useId()

  return (
    <DialogShell open={open} labelledBy={titleId} size={size} onClose={onClose}>
      <div className="flex items-center justify-between gap-4 border-b border-neutral-200 px-6 py-4">
        <span id={titleId}>
          <Heading level={2}>{title}</Heading>
        </span>
        <IconButton aria-label={closeLabel} icon={<X className="size-4" />} onClick={onClose} />
      </div>
      <div className="px-6 py-5">{children}</div>
      {footer ? (
        <div className="flex justify-end gap-2 border-t border-neutral-200 px-6 py-4">{footer}</div>
      ) : null}
    </DialogShell>
  )
}
