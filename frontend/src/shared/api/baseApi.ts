import {
  createApi,
  fetchBaseQuery,
  type BaseQueryFn,
  type FetchArgs,
  type FetchBaseQueryError,
} from '@reduxjs/toolkit/query/react'
import i18n from 'i18next'
import { notify } from '@/shared/lib/notify'
import { sessionExpired } from './sessionExpired'

type StateWithSession = { auth: { session: { accessToken: string } | null } }

const rawBaseQuery = fetchBaseQuery({
  baseUrl: import.meta.env.VITE_API_URL ?? '/api/v1',
  prepareHeaders: (headers, { getState }) => {
    const token = (getState() as StateWithSession).auth.session?.accessToken
    if (token) headers.set('Authorization', `Bearer ${token}`)
    headers.set('Accept-Language', i18n.language)
    return headers
  },
})

const baseQuery: BaseQueryFn<string | FetchArgs, unknown, FetchBaseQueryError> = async (
  args,
  api,
  extraOptions,
) => {
  const hadSession = (api.getState() as StateWithSession).auth.session !== null
  const result = await rawBaseQuery(args, api, extraOptions)

  if (result.error?.status === 401 && hadSession) {
    api.dispatch(sessionExpired())
    notify.error(i18n.t('errors.UNAUTHORIZED'))
  }

  return result
}

export const baseApi = createApi({
  reducerPath: 'api',
  baseQuery,
  tagTypes: ['Me', 'Accounts', 'Transactions', 'Customers', 'Reports', 'AuditLogs'],
  endpoints: () => ({}),
})
