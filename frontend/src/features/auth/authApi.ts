import { baseApi } from '@/shared/api'
import { userLoaded } from './authSlice'
import type { AuthUser, Session } from './types'

export type LoginRequest = { email: string; password: string }

export type RegisterRequest = {
  firstName: string
  lastName: string
  nationalId: string
  email: string
  phone: string
  password: string
}

export const authApi = baseApi.injectEndpoints({
  endpoints: (build) => ({
    login: build.mutation<Session, LoginRequest>({
      query: (body) => ({ url: '/auth/login', method: 'POST', body }),
    }),
    register: build.mutation<{ customerId: string; customerNo: string }, RegisterRequest>({
      query: (body) => ({ url: '/auth/register', method: 'POST', body }),
    }),
    me: build.query<AuthUser, void>({
      query: () => '/auth/me',
      providesTags: ['Me'],
      async onQueryStarted(_, { dispatch, queryFulfilled }) {
        const { data } = await queryFulfilled.catch(() => ({ data: null }))
        if (data) dispatch(userLoaded(data))
      },
    }),
  }),
})

export const { useLoginMutation, useRegisterMutation, useMeQuery } = authApi
