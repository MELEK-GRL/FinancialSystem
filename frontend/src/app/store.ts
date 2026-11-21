import { configureStore } from '@reduxjs/toolkit'
import { authReducer, saveSession } from '@/features/auth'

export const store = configureStore({
  reducer: {
    auth: authReducer,
  },
})

let savedSession = store.getState().auth.session
store.subscribe(() => {
  const session = store.getState().auth.session
  if (session !== savedSession) {
    savedSession = session
    saveSession(session)
  }
})

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch
