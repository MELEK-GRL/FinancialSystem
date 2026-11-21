import { baseApi } from '@/shared/api'
import type { PagedResult } from '@/shared/types'
import type { AuditLog, AuditLogParams } from './types'

const ISTANBUL_OFFSET = '+03:00'

export const auditApi = baseApi.injectEndpoints({
  endpoints: (build) => ({
    listAuditLogs: build.query<PagedResult<AuditLog>, AuditLogParams>({
      query: ({ from, to, ...params }) => ({
        url: '/audit-logs',
        params: {
          ...params,
          from: from ? `${from}T00:00:00${ISTANBUL_OFFSET}` : undefined,
          to: to ? `${to}T23:59:59.999${ISTANBUL_OFFSET}` : undefined,
        },
      }),
      providesTags: ['AuditLogs'],
    }),
  }),
})

export const { useListAuditLogsQuery } = auditApi
