import { useCallback } from 'react'
import { useSearchParams } from 'react-router'

export type UrlParamChanges = Record<string, string | undefined>

export function useUrlParams() {
  const [params, setParams] = useSearchParams()

  const update = useCallback(
    (changes: UrlParamChanges) =>
      setParams((current) => {
        const next = new URLSearchParams(current)
        for (const [key, value] of Object.entries(changes)) {
          if (value) next.set(key, value)
          else next.delete(key)
        }
        return next
      }),
    [setParams],
  )

  return { params, update }
}
