import clsx from 'clsx'

export function inputClassName(hasError: boolean, className?: string) {
  return clsx(
    'block h-10 w-full rounded-lg border bg-white px-3 text-sm text-neutral-900 placeholder:text-neutral-400',
    'transition-colors duration-150 focus:outline-none focus-visible:ring-2',
    'disabled:cursor-not-allowed disabled:bg-neutral-100 disabled:text-neutral-500',
    hasError
      ? 'border-danger focus-visible:ring-danger/30'
      : 'border-neutral-300 focus-visible:border-primary focus-visible:ring-primary/20',
    className,
  )
}

export function describedBy(id: string, error?: string, hint?: string) {
  return clsx(error && `${id}-error`, hint && `${id}-hint`) || undefined
}
