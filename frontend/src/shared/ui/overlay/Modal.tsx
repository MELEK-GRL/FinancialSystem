import { useEffect, useId, useRef, type ReactNode } from 'react'
import { X } from 'lucide-react'
import { IconButton } from '../actions/IconButton'
import { Heading } from '../typography/Heading'

export type ModalProps = {
  open: boolean
  title: string
  closeLabel: string
  onClose: () => void
  children: ReactNode
  footer?: ReactNode
}

export function Modal({ open, title, closeLabel, onClose, children, footer }: ModalProps) {
  const dialogRef = useRef<HTMLDialogElement>(null)
  const titleId = useId()

  useEffect(() => {
    const dialog = dialogRef.current
    if (!dialog) return
    if (open && !dialog.open) dialog.showModal()
    if (!open && dialog.open) dialog.close()
  }, [open])

  return (
    <dialog
      ref={dialogRef}
      aria-labelledby={titleId}
      onCancel={(event) => {
        event.preventDefault()
        onClose()
      }}
      className="m-auto w-full max-w-md rounded-xl border border-neutral-200 bg-white p-0 shadow-lg backdrop:bg-neutral-900/40"
    >
      <div className="flex items-center justify-between border-b border-neutral-200 px-6 py-4">
        <span id={titleId}>
          <Heading level={2}>{title}</Heading>
        </span>
        <IconButton aria-label={closeLabel} icon={<X className="size-4" />} onClick={onClose} />
      </div>
      <div className="px-6 py-5">{children}</div>
      {footer ? (
        <div className="flex justify-end gap-2 border-t border-neutral-200 px-6 py-4">{footer}</div>
      ) : null}
    </dialog>
  )
}
