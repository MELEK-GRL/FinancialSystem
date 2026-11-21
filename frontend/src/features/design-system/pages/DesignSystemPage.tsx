import { useTranslation } from 'react-i18next'
import { LanguageSelector } from '@/shared/i18n'
import { Card, Heading, Text } from '@/shared/ui'
import { BadgesPreview, ButtonsPreview } from '../components/ButtonsPreview'
import { ColorPalette } from '../components/ColorPalette'
import { MoneyPreview, SearchPreview, TablePreview } from '../components/DataPreview'
import { FormPreview } from '../components/FormPreview'
import { OverlaysPreview, StatesPreview } from '../components/StatesPreview'
import { TypographyPreview } from '../components/TypographyPreview'

export function DesignSystemPage() {
  const { t } = useTranslation()

  return (
    <main className="mx-auto flex max-w-6xl flex-col gap-6 px-4 py-8 md:px-8">
      <header className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
        <div className="flex flex-col gap-1">
          <Text variant="small">{t('common.appName')}</Text>
          <Heading>{t('designSystem.title')}</Heading>
          <Text variant="muted">{t('designSystem.subtitle')}</Text>
        </div>
        <LanguageSelector />
      </header>

      <Card title={t('designSystem.sections.colors')}>
        <ColorPalette />
      </Card>
      <div className="grid gap-6 lg:grid-cols-2">
        <Card title={t('designSystem.sections.typography')}>
          <TypographyPreview />
        </Card>
        <div className="flex flex-col gap-6">
          <Card title={t('designSystem.sections.buttons')}>
            <ButtonsPreview />
          </Card>
          <Card title={t('designSystem.sections.badges')}>
            <BadgesPreview />
          </Card>
        </div>
      </div>
      <Card title={t('designSystem.sections.form')}>
        <FormPreview />
      </Card>
      <div className="grid gap-6 lg:grid-cols-2">
        <Card title={t('designSystem.sections.search')}>
          <SearchPreview />
        </Card>
        <Card title={t('designSystem.sections.money')}>
          <MoneyPreview />
        </Card>
      </div>
      <Card title={t('designSystem.sections.table')} padded={false}>
        <TablePreview />
      </Card>
      <Card title={t('designSystem.sections.states')}>
        <StatesPreview />
      </Card>
      <Card title={t('designSystem.sections.overlays')}>
        <OverlaysPreview />
      </Card>
    </main>
  )
}
