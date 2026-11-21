import type { FetchBaseQueryError } from '@reduxjs/toolkit/query'
import i18n from 'i18next'
import { popup } from '@/shared/lib/popup'
import { isProblemDetails, type ProblemDetails } from '@/shared/types'

function isFetchError(error: unknown): error is FetchBaseQueryError {
  return typeof error === 'object' && error !== null && 'status' in error
}

export function getProblemDetails(error: unknown): ProblemDetails | undefined {
  return isFetchError(error) && isProblemDetails(error.data) ? error.data : undefined
}

export function getErrorMessage(error: unknown) {
  const problem = getProblemDetails(error)
  if (problem) return problem.title || i18n.t(`errors.${problem.errorCode}` as never)

  if (isFetchError(error) && (error.status === 'FETCH_ERROR' || error.status === 'TIMEOUT_ERROR')) {
    return i18n.t('errors.NETWORK_ERROR')
  }

  return i18n.t('errors.INTERNAL_ERROR')
}

export function reportApiError(
  error: unknown,
  applyFieldErrors?: (problem: ProblemDetails) => boolean,
) {
  const problem = getProblemDetails(error)
  if (problem && applyFieldErrors?.(problem)) return
  popup.danger({ title: i18n.t('common.errorTitle'), description: getErrorMessage(error) })
}
