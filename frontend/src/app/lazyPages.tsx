import { lazy } from 'react'

export const LoginPage = lazy(() =>
  import('@/features/auth/pages/LoginPage').then((m) => ({ default: m.LoginPage })),
)
export const RegisterPage = lazy(() =>
  import('@/features/auth/pages/RegisterPage').then((m) => ({ default: m.RegisterPage })),
)
export const HomePage = lazy(() =>
  import('@/features/home/pages/HomePage').then((m) => ({ default: m.HomePage })),
)
export const DesignSystemPage = lazy(() =>
  import('@/features/design-system').then((m) => ({ default: m.DesignSystemPage })),
)
export const NotFoundPage = lazy(() =>
  import('./pages/NotFoundPage').then((m) => ({ default: m.NotFoundPage })),
)
export const ProfilePage = lazy(() =>
  import('@/features/customers/pages/ProfilePage').then((m) => ({ default: m.ProfilePage })),
)
export const CustomerListPage = lazy(() =>
  import('@/features/customers/pages/CustomerListPage').then((m) => ({
    default: m.CustomerListPage,
  })),
)
export const CustomerDetailPage = lazy(() =>
  import('@/features/customers/pages/CustomerDetailPage').then((m) => ({
    default: m.CustomerDetailPage,
  })),
)
