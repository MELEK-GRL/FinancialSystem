import { LayoutDashboard, type LucideIcon } from 'lucide-react'
import type { Role } from '@/features/auth'

export type NavigationItem = {
  to: string
  labelKey: 'nav.home'
  icon: LucideIcon
  roles: Role[]
}

export const navigationItems: NavigationItem[] = [
  { to: '/', labelKey: 'nav.home', icon: LayoutDashboard, roles: ['ADMIN', 'CUSTOMER'] },
]
