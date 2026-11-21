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
export const MyAccountsPage = lazy(() =>
  import('@/features/accounts/pages/MyAccountsPage').then((m) => ({ default: m.MyAccountsPage })),
)
export const TransferPage = lazy(() =>
  import('@/features/transactions/pages/TransferPage').then((m) => ({ default: m.TransferPage })),
)
export const TransactionHistoryPage = lazy(() =>
  import('@/features/transactions/pages/TransactionHistoryPage').then((m) => ({
    default: m.TransactionHistoryPage,
  })),
)
