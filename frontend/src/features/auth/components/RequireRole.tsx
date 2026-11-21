import { Navigate, Outlet } from 'react-router'
import { useAppSelector } from '@/app/hooks'
import { selectRole } from '../authSlice'
import type { Role } from '../types'

export type RequireRoleProps = { role: Role }

export function RequireRole({ role }: RequireRoleProps) {
  const currentRole = useAppSelector(selectRole)

  if (currentRole !== role) return <Navigate to="/" replace />
  return <Outlet />
}
