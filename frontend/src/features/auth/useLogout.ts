import { useCallback } from 'react'
import { useNavigate } from 'react-router'
import { useAppDispatch } from '@/app/hooks'
import { baseApi } from '@/shared/api'
import { loggedOut } from './authSlice'

export function useLogout() {
  const dispatch = useAppDispatch()
  const navigate = useNavigate()

  return useCallback(() => {
    dispatch(loggedOut())
    dispatch(baseApi.util.resetApiState())
    navigate('/login', { replace: true })
  }, [dispatch, navigate])
}
