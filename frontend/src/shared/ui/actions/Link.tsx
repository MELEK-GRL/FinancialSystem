import clsx from 'clsx'
import { Link as RouterLink, type LinkProps as RouterLinkProps } from 'react-router'

export type LinkProps = RouterLinkProps

export function Link({ className, ...props }: LinkProps) {
  return (
    <RouterLink
      className={clsx(
        'font-medium text-primary underline-offset-4 hover:underline',
        'focus-visible:rounded focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary',
        className,
      )}
      {...props}
    />
  )
}
