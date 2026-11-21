import { useCallback, useRef } from 'react'

export function useIdempotencyKey() {
  const last = useRef<{ fingerprint: string; key: string } | null>(null)

  return useCallback((values: unknown) => {
    const fingerprint = JSON.stringify(values)
    if (last.current?.fingerprint !== fingerprint) {
      last.current = { fingerprint, key: crypto.randomUUID() }
    }
    return last.current.key
  }, [])
}
