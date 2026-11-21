import { baseApi } from '@/shared/api'
import type { MoneyRequest, Transaction, TransferRequest } from './types'

const withIdempotency =
  (url: string) =>
  ({ idempotencyKey, ...body }: MoneyRequest | TransferRequest) => ({
    url,
    method: 'POST',
    body,
    headers: { 'Idempotency-Key': idempotencyKey },
  })

const affected = [{ type: 'Accounts' as const, id: 'LIST' }, 'Transactions' as const]

export const transactionsApi = baseApi.injectEndpoints({
  endpoints: (build) => ({
    deposit: build.mutation<Transaction, MoneyRequest>({
      query: withIdempotency('/transactions/deposit'),
      invalidatesTags: affected,
    }),
    withdraw: build.mutation<Transaction, MoneyRequest>({
      query: withIdempotency('/transactions/withdraw'),
      invalidatesTags: affected,
    }),
    transfer: build.mutation<Transaction, TransferRequest>({
      query: withIdempotency('/transactions/transfer'),
      invalidatesTags: affected,
    }),
  }),
})

export const { useDepositMutation, useWithdrawMutation, useTransferMutation } = transactionsApi
