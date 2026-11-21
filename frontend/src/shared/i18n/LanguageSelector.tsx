import clsx from 'clsx'
import { useTranslation } from 'react-i18next'
import { languages } from './languages'

export function LanguageSelector() {
  const { t, i18n } = useTranslation()

  return (
    <div
      role="group"
      aria-label={t('language.label')}
      className="inline-flex rounded-lg border border-neutral-200 bg-white p-0.5"
    >
      {languages.map((language) => (
        <button
          key={language}
          type="button"
          aria-pressed={i18n.language === language}
          onClick={() => void i18n.changeLanguage(language)}
          className={clsx(
            'rounded-md px-2.5 py-1 text-xs font-medium transition-colors duration-150',
            'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary',
            i18n.language === language
              ? 'bg-primary text-white'
              : 'text-neutral-600 hover:bg-neutral-100',
          )}
        >
          {t(`language.${language}`)}
        </button>
      ))}
    </div>
  )
}
