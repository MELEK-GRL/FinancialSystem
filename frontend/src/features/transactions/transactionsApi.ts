import type { FetchBaseQueryError } from '@reduxjs/toolkit/query'
import { baseApi } from '@/shared/api'
import type { PagedResult } from '@/shared/types'
import type {
  AccountTransaction,
  HistoryParams,
  MoneyRequest,
  Transaction,
  TransferRequest,
} from './types'

const withIdempotency =
  (url: string) =>
  ({ idempotencyKey, ...body }: MoneyRequest | TransferRequest) => ({
    url,
    method: 'POST',
    body,
    headers: { 'Idempotency-Key': idempotencyKey },
  })

const affected = [{ type: 'Accounts' as const, id: 'LIST' }, 'Transactions' as const]
const afterAnswer = (_result: unknown, error?: FetchBaseQueryError) =>
  error && typeof error.status !== 'number' ? [] : affected

export const transactionsApi = baseApi.injectEndpoints({
  endpoints: (build) => ({
    getAccountHistory: build.query<PagedResult<AccountTransaction>, HistoryParams>({
      query: ({ accountId, ...params }) => ({ url: `/accounts/${accountId}/transactions`, params }),
      providesTags: ['Transactions'],
    }),
    getTransaction: build.query<Transaction, string>({
      query: (referenceNo) => `/transactions/${referenceNo}`,
      providesTags: ['Transactions'],
    }),
    deposit: build.mutation<Transaction, MoneyRequest>({
      query: withIdempotency('/transactions/deposit'),
      invalidatesTags: afterAnswer,
    }),
    withdraw: build.mutation<Transaction, MoneyRequest>({
      query: withIdempotency('/transactions/withdraw'),
      invalidatesTags: afterAnswer,
    }),
    transfer: build.mutation<Transaction, TransferRequest>({
      query: withIdempotency('/transactions/transfer'),
      invalidatesTags: afterAnswer,
    }),
  }),
})

export const {
  useDepositMutation,
  useWithdrawMutation,
  useTransferMutation,
  useGetAccountHistoryQuery,
  useGetTransactionQuery,
} = transactionsApi
