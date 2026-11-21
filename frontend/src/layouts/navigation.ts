import { LayoutDashboard, Send, UserRound, Users, Wallet, type LucideIcon } from 'lucide-react'
import type { Role } from '@/features/auth'

export type NavigationItem = {
  to: string
  labelKey: 'nav.home' | 'nav.accounts' | 'nav.transfer' | 'nav.profile' | 'nav.customers'
  icon: LucideIcon
  roles: Role[]
}

export const navigationItems: NavigationItem[] = [
  { to: '/', labelKey: 'nav.home', icon: LayoutDashboard, roles: ['ADMIN', 'CUSTOMER'] },
  { to: '/customers', labelKey: 'nav.customers', icon: Users, roles: ['ADMIN'] },
  { to: '/accounts', labelKey: 'nav.accounts', icon: Wallet, roles: ['CUSTOMER'] },
  { to: '/transfer', labelKey: 'nav.transfer', icon: Send, roles: ['CUSTOMER'] },
  { to: '/profile', labelKey: 'nav.profile', icon: UserRound, roles: ['CUSTOMER'] },
]
