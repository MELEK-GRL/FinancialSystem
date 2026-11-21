import { useCallback, useMemo, useRef } from 'react'

export function useIdempotencyKey() {
  const last = useRef<{ fingerprint: string; key: string } | null>(null)

  const keyFor = useCallback((values: unknown) => {
    const fingerprint = JSON.stringify(values)
    if (last.current?.fingerprint !== fingerprint) {
      last.current = { fingerprint, key: crypto.randomUUID() }
    }
    return last.current.key
  }, [])

  const reset = useCallback(() => {
    last.current = null
  }, [])

  return useMemo(() => ({ keyFor, reset }), [keyFor, reset])
}
