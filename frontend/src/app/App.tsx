import { useTranslation } from 'react-i18next'
import { Toaster } from 'sonner'
import { languages } from '@/app/i18n'
import { Heading, LanguageSwitcher } from '@/shared/ui'

export function App() {
  const { t, i18n } = useTranslation()

  return (
    <>
      <main className="flex min-h-screen flex-col items-center justify-center gap-4">
        <Heading>{t('common.appName')}</Heading>
        <LanguageSwitcher
          label={t('language.label')}
          value={i18n.language}
          options={languages.map((language) => ({
            value: language,
            label: t(`language.${language}`),
          }))}
          onChange={(language) => void i18n.changeLanguage(language)}
        />
      </main>
      <Toaster position="top-right" closeButton />
    </>
  )
}
