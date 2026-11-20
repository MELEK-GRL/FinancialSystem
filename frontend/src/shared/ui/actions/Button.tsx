import clsx from 'clsx'
import type { ButtonHTMLAttributes, ReactNode } from 'react'
import { Spinner } from '../feedback/Spinner'

export type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: 'primary' | 'secondary' | 'danger' | 'ghost'
  size?: 'sm' | 'md'
  loading?: boolean
  icon?: ReactNode
}

const variants = {
  primary: 'bg-primary text-white hover:bg-primary-hover',
  secondary: 'border border-neutral-300 bg-white text-neutral-800 hover:bg-neutral-100',
  danger: 'bg-danger text-white hover:bg-danger-hover',
  ghost: 'text-neutral-700 hover:bg-neutral-100',
} as const

const sizes = {
  sm: 'h-8 px-3 text-sm',
  md: 'h-10 px-4 text-sm',
} as const

export function Button({
  variant = 'primary',
  size = 'md',
  loading = false,
  icon,
  type = 'button',
  disabled,
  className,
  children,
  ...props
}: ButtonProps) {
  return (
    <button
      type={type}
      disabled={disabled || loading}
      aria-busy={loading || undefined}
      className={clsx(
        'inline-flex items-center justify-center gap-2 rounded-lg font-medium transition-colors duration-150',
        'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2',
        'disabled:cursor-not-allowed disabled:opacity-60',
        variants[variant],
        sizes[size],
        className,
      )}
      {...props}
    >
      {loading ? <Spinner size="sm" /> : icon}
      {children}
    </button>
  )
}
