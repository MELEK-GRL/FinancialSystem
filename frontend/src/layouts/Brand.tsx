import { Landmark } from 'lucide-react'
import { useTranslation } from 'react-i18next'

export type BrandProps = { compact?: boolean }

export function Brand({ compact = false }: BrandProps) {
  const { t } = useTranslation()

  return (
    <span className="flex items-center gap-2 font-semibold text-neutral-900">
      <span className="flex size-8 items-center justify-center rounded-lg bg-primary text-white">
        <Landmark className="size-4" aria-hidden="true" />
      </span>
      <span className={compact ? 'sr-only sm:not-sr-only' : undefined}>{t('common.appName')}</span>
    </span>
  )
}
