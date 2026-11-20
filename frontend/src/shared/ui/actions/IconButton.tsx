import clsx from 'clsx'
import type { ButtonHTMLAttributes, ReactNode } from 'react'

export type IconButtonProps = Omit<
  ButtonHTMLAttributes<HTMLButtonElement>,
  'aria-label' | 'children'
> & {
  'aria-label': string
  icon: ReactNode
}

export function IconButton({ icon, type = 'button', className, ...props }: IconButtonProps) {
  return (
    <button
      type={type}
      className={clsx(
        'inline-flex size-9 items-center justify-center rounded-lg text-neutral-600 transition-colors duration-150',
        'hover:bg-neutral-100 hover:text-neutral-900',
        'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary',
        'disabled:cursor-not-allowed disabled:opacity-60',
        className,
      )}
      {...props}
    >
      {icon}
    </button>
  )
}
