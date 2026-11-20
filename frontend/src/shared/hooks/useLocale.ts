import { useTranslation } from 'react-i18next'

const locales = { tr: 'tr-TR', en: 'en-US' } as const

export function useLocale() {
  const { i18n } = useTranslation()
  return i18n.language === 'en' ? locales.en : locales.tr
}
