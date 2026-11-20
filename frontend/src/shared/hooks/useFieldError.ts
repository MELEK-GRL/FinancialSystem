import { useCallback } from 'react'
import { useTranslation } from 'react-i18next'

export function useFieldError() {
  const { t, i18n } = useTranslation()

  return useCallback(
    (error?: { message?: string }) => {
      const message = error?.message
      if (!message) return undefined
      return i18n.exists(message) ? t(message as never) : message
    },
    [t, i18n],
  )
}
