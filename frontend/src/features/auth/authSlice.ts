import { createSlice, type PayloadAction } from '@reduxjs/toolkit'
import { sessionExpired } from '@/shared/api/sessionExpired'
import { isSessionActive, loadSession } from './sessionStorage'
import type { AuthUser, Session } from './types'

export type AuthState = {
  session: Session | null
  user: AuthUser | null
}

const initialState: AuthState = {
  session: loadSession(),
  user: null,
}

export const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    loggedIn(state, action: PayloadAction<Session>) {
      state.session = action.payload
      state.user = null
    },
    userLoaded(state, action: PayloadAction<AuthUser>) {
      state.user = action.payload
    },
    loggedOut(state) {
      state.session = null
      state.user = null
    },
  },
  extraReducers: (builder) => {
    builder.addCase(sessionExpired, (state) => {
      state.session = null
      state.user = null
    })
  },
  selectors: {
    selectSession: (state) => state.session,
    selectCurrentUser: (state) => state.user,
    selectIsAuthenticated: (state) => isSessionActive(state.session),
    selectRole: (state) => (isSessionActive(state.session) ? (state.session?.role ?? null) : null),
  },
})

export const { loggedIn, userLoaded, loggedOut } = authSlice.actions
export const { selectSession, selectCurrentUser, selectIsAuthenticated, selectRole } =
  authSlice.selectors
export default authSlice.reducer
