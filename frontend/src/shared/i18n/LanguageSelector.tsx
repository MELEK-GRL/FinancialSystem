import { useTranslation } from 'react-i18next'
import { LanguageSwitcher } from '@/shared/ui'
import { languages } from './languages'

export function LanguageSelector() {
  const { t, i18n } = useTranslation()

  return (
    <LanguageSwitcher
      label={t('language.label')}
      value={i18n.language}
      options={languages.map((language) => ({ value: language, label: t(`language.${language}`) }))}
      onChange={(language) => void i18n.changeLanguage(language)}
    />
  )
}
