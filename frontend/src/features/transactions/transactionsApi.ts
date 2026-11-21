import { baseApi } from '@/shared/api'
import type { MoneyRequest, Transaction } from './types'

const moneyRequest =
  (url: string) =>
  ({ idempotencyKey, ...body }: MoneyRequest) => ({
    url,
    method: 'POST',
    body,
    headers: { 'Idempotency-Key': idempotencyKey },
  })

export const transactionsApi = baseApi.injectEndpoints({
  endpoints: (build) => ({
    deposit: build.mutation<Transaction, MoneyRequest>({
      query: moneyRequest('/transactions/deposit'),
      invalidatesTags: [{ type: 'Accounts', id: 'LIST' }, 'Transactions'],
    }),
    withdraw: build.mutation<Transaction, MoneyRequest>({
      query: moneyRequest('/transactions/withdraw'),
      invalidatesTags: [{ type: 'Accounts', id: 'LIST' }, 'Transactions'],
    }),
  }),
})

export const { useDepositMutation, useWithdrawMutation } = transactionsApi
