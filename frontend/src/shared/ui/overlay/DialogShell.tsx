import { useEffect, useRef, type ReactNode } from 'react'
import clsx from 'clsx'

export type DialogShellProps = {
  open: boolean
  labelledBy: string
  describedBy?: string
  size?: 'sm' | 'md' | 'lg'
  onClose: () => void
  children: ReactNode
}

const sizes = { sm: 'max-w-sm', md: 'max-w-md', lg: 'max-w-2xl' } as const

export function DialogShell({
  open,
  labelledBy,
  describedBy,
  size = 'md',
  onClose,
  children,
}: DialogShellProps) {
  const dialogRef = useRef<HTMLDialogElement>(null)

  useEffect(() => {
    const dialog = dialogRef.current
    if (!dialog) return
    if (open && !dialog.open) dialog.showModal()
    if (!open && dialog.open) dialog.close()
  }, [open])

  return (
    <dialog
      ref={dialogRef}
      aria-labelledby={labelledBy}
      aria-describedby={describedBy}
      onCancel={(event) => {
        event.preventDefault()
        onClose()
      }}
      className={clsx(
        'm-auto w-[calc(100%-2rem)] rounded-2xl border border-neutral-200 bg-white p-0 shadow-xl',
        'backdrop:bg-neutral-900/50 backdrop:backdrop-blur-[2px]',
        sizes[size],
      )}
    >
      {open ? children : null}
    </dialog>
  )
}
