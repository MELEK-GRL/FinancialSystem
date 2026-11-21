import { LayoutDashboard, UserRound, Users, type LucideIcon } from 'lucide-react'
import type { Role } from '@/features/auth'

export type NavigationItem = {
  to: string
  labelKey: 'nav.home' | 'nav.profile' | 'nav.customers'
  icon: LucideIcon
  roles: Role[]
}

export const navigationItems: NavigationItem[] = [
  { to: '/', labelKey: 'nav.home', icon: LayoutDashboard, roles: ['ADMIN', 'CUSTOMER'] },
  { to: '/customers', labelKey: 'nav.customers', icon: Users, roles: ['ADMIN'] },
  { to: '/profile', labelKey: 'nav.profile', icon: UserRound, roles: ['CUSTOMER'] },
]
