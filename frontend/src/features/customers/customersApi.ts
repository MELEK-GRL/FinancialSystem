import { baseApi } from '@/shared/api'
import type { PagedResult } from '@/shared/types'
import type { Customer, CustomerListParams, CustomerStatus, CustomerSummary } from './types'

export const customersApi = baseApi.injectEndpoints({
  endpoints: (build) => ({
    getMyProfile: build.query<Customer, void>({
      query: () => '/customers/me',
      providesTags: [{ type: 'Customers', id: 'ME' }],
    }),
    updateMyProfile: build.mutation<Customer, { email: string; phone: string }>({
      query: (body) => ({ url: '/customers/me', method: 'PATCH', body }),
      invalidatesTags: [{ type: 'Customers', id: 'ME' }, 'Me'],
    }),
    listCustomers: build.query<PagedResult<CustomerSummary>, CustomerListParams>({
      query: (params) => ({ url: '/customers', params }),
      providesTags: [{ type: 'Customers', id: 'LIST' }],
    }),
    getCustomer: build.query<Customer, string>({
      query: (id) => `/customers/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'Customers', id }],
    }),
    changeCustomerStatus: build.mutation<Customer, { id: string; status: CustomerStatus }>({
      query: ({ id, status }) => ({
        url: `/customers/${id}/status`,
        method: 'PATCH',
        body: { status },
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'Customers', id },
        { type: 'Customers', id: 'LIST' },
      ],
    }),
  }),
})

export const {
  useGetMyProfileQuery,
  useUpdateMyProfileMutation,
  useListCustomersQuery,
  useGetCustomerQuery,
  useChangeCustomerStatusMutation,
} = customersApi
