import { useCallback } from 'react'
import type { FieldValues, Path, UseFormSetError } from 'react-hook-form'
import type { ProblemDetails } from '@/shared/types'

export function useApiFormErrors<T extends FieldValues>(
  setError: UseFormSetError<T>,
  fields: readonly Path<T>[],
) {
  return useCallback(
    (problem: ProblemDetails) => {
      let matched = false

      for (const [field, messages] of Object.entries(problem.errors ?? {})) {
        const name = fields.find((candidate) => candidate === field)
        if (name && messages[0]) {
          setError(name, { type: 'server', message: messages[0] })
          matched = true
        }
      }

      return matched
    },
    [setError, fields],
  )
}
