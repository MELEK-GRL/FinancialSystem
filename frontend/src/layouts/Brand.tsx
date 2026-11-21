import clsx from 'clsx'
import { Landmark } from 'lucide-react'
import { useTranslation } from 'react-i18next'

export type BrandProps = {
  compact?: boolean
  tone?: 'light' | 'dark'
}

export function Brand({ compact = false, tone = 'dark' }: BrandProps) {
  const { t } = useTranslation()

  return (
    <span
      className={clsx(
        'flex items-center gap-2.5 font-semibold tracking-tight',
        tone === 'light' ? 'text-white' : 'text-neutral-900',
      )}
    >
      <span
        className={clsx(
          'flex size-9 items-center justify-center rounded-lg',
          tone === 'light'
            ? 'bg-white/10 text-white ring-1 ring-white/20'
            : 'bg-primary text-white',
        )}
      >
        <Landmark className="size-5" aria-hidden="true" />
      </span>
      <span className={compact ? 'sr-only sm:not-sr-only' : undefined}>{t('common.appName')}</span>
    </span>
  )
}
