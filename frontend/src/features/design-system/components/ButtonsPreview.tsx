import { useTranslation } from 'react-i18next'
import { Menu, Send } from 'lucide-react'
import { Badge, Button, IconButton } from '@/shared/ui'

export function ButtonsPreview() {
  const { t } = useTranslation()

  return (
    <div className="flex flex-wrap items-center gap-3">
      <Button icon={<Send className="size-4" />}>{t('designSystem.buttons.primary')}</Button>
      <Button variant="secondary">{t('designSystem.buttons.secondary')}</Button>
      <Button variant="danger">{t('designSystem.buttons.danger')}</Button>
      <Button variant="ghost">{t('designSystem.buttons.ghost')}</Button>
      <Button loading>{t('designSystem.buttons.loading')}</Button>
      <Button disabled>{t('designSystem.buttons.disabled')}</Button>
      <Button size="sm" variant="secondary">
        {t('designSystem.buttons.ghost')}
      </Button>
      <IconButton aria-label={t('designSystem.buttons.menu')} icon={<Menu className="size-5" />} />
    </div>
  )
}

export function BadgesPreview() {
  const { t } = useTranslation()
  const tones = ['neutral', 'primary', 'success', 'danger', 'warning'] as const

  return (
    <div className="flex flex-wrap gap-2">
      {tones.map((tone) => (
        <Badge key={tone} tone={tone}>
          {t(`designSystem.badges.${tone}`)}
        </Badge>
      ))}
    </div>
  )
}
