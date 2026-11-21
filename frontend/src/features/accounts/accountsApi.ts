import { baseApi } from '@/shared/api'
import type { Account } from './types'

export const accountsApi = baseApi.injectEndpoints({
  endpoints: (build) => ({
    listAccounts: build.query<Account[], string | void>({
      query: (customerId) => ({
        url: '/accounts',
        params: customerId ? { customerId } : undefined,
      }),
      providesTags: [{ type: 'Accounts', id: 'LIST' }],
    }),
  }),
})

export const { useListAccountsQuery } = accountsApi
