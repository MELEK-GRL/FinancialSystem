import { useId, type ReactNode } from 'react'
import clsx from 'clsx'
import { CircleAlert, CircleCheck, Info } from 'lucide-react'
import { Button } from '../actions/Button'
import { DialogShell } from './DialogShell'

export type PopupVariant = 'success' | 'info' | 'danger'

export type PopupModalProps = {
  open: boolean
  variant: PopupVariant
  title: string
  description?: ReactNode
  actionLabel: string
  onClose: () => void
}

const variants = {
  success: { icon: CircleCheck, circle: 'bg-success-soft text-success', button: 'primary' },
  info: { icon: Info, circle: 'bg-primary-soft text-primary', button: 'primary' },
  danger: { icon: CircleAlert, circle: 'bg-danger-soft text-danger', button: 'danger' },
} as const

export function PopupModal({
  open,
  variant,
  title,
  description,
  actionLabel,
  onClose,
}: PopupModalProps) {
  const titleId = useId()
  const descriptionId = useId()
  const { icon: Icon, circle, button } = variants[variant]

  return (
    <DialogShell
      open={open}
      labelledBy={titleId}
      describedBy={description ? descriptionId : undefined}
      size="sm"
      onClose={onClose}
    >
      <div
        role={variant === 'danger' ? 'alertdialog' : undefined}
        className="flex flex-col items-center gap-4 px-6 pb-6 pt-8 text-center"
      >
        <span className={clsx('flex size-14 items-center justify-center rounded-full', circle)}>
          <Icon className="size-7" aria-hidden="true" />
        </span>
        <div className="flex flex-col gap-1.5">
          <h2 id={titleId} className="text-lg font-semibold text-neutral-900">
            {title}
          </h2>
          {description ? (
            <div id={descriptionId} className="text-sm text-neutral-600">
              {description}
            </div>
          ) : null}
        </div>
        <Button variant={button} className="mt-2 w-full" onClick={onClose} autoFocus>
          {actionLabel}
        </Button>
      </div>
    </DialogShell>
  )
}
