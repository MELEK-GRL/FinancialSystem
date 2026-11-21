import type { FetchBaseQueryError } from '@reduxjs/toolkit/query'
import type { SerializedError } from '@reduxjs/toolkit'
import i18n from 'i18next'
import { isProblemDetails, type ProblemDetails } from '@/shared/types'

export type ApiError = FetchBaseQueryError | SerializedError | undefined

export function getProblemDetails(error: ApiError): ProblemDetails | undefined {
  return error && 'status' in error && isProblemDetails(error.data) ? error.data : undefined
}

export function getErrorMessage(error: ApiError) {
  const problem = getProblemDetails(error)
  if (problem) return problem.title || i18n.t(`errors.${problem.errorCode}` as never)

  if (
    error &&
    'status' in error &&
    (error.status === 'FETCH_ERROR' || error.status === 'TIMEOUT_ERROR')
  ) {
    return i18n.t('errors.NETWORK_ERROR')
  }

  return i18n.t('errors.INTERNAL_ERROR')
}
