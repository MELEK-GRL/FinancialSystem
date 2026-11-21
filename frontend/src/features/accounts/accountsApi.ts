import { baseApi } from '@/shared/api'
import type { Account, AccountStatus } from './types'

export const accountsApi = baseApi.injectEndpoints({
  endpoints: (build) => ({
    listAccounts: build.query<Account[], string | void>({
      query: (customerId) => ({
        url: '/accounts',
        params: customerId ? { customerId } : undefined,
      }),
      providesTags: [{ type: 'Accounts', id: 'LIST' }],
    }),
    openAccount: build.mutation<Account, void>({
      query: () => ({ url: '/accounts', method: 'POST' }),
      invalidatesTags: [{ type: 'Accounts', id: 'LIST' }],
    }),
    changeAccountStatus: build.mutation<
      Account,
      { id: string; status: AccountStatus; reason: string }
    >({
      query: ({ id, ...body }) => ({ url: `/accounts/${id}/status`, method: 'PATCH', body }),
      invalidatesTags: [{ type: 'Accounts', id: 'LIST' }],
    }),
  }),
})

export const { useListAccountsQuery, useOpenAccountMutation, useChangeAccountStatusMutation } =
  accountsApi
