import { useTranslation } from 'react-i18next'
import { Heading, Text } from '@/shared/ui'

export function TypographyPreview() {
  const { t } = useTranslation()

  return (
    <div className="flex flex-col gap-2">
      <Heading level={1}>{t('designSystem.sample.heading1')}</Heading>
      <Heading level={2}>{t('designSystem.sample.heading2')}</Heading>
      <Heading level={3}>{t('designSystem.sample.heading3')}</Heading>
      <Text>{t('designSystem.sample.body')}</Text>
      <Text variant="muted">{t('designSystem.sample.muted')}</Text>
      <Text variant="small">{t('designSystem.sample.small')}</Text>
    </div>
  )
}
