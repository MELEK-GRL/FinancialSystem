import { Suspense, type ReactNode } from 'react'
import { createBrowserRouter } from 'react-router'
import { ProtectedRoute, RequireRole } from '@/features/auth'
import { AppLayout, AuthLayout, PageFallback } from '@/layouts'
import {
  CustomerDetailPage,
  CustomerListPage,
  DesignSystemPage,
  HomePage,
  LoginPage,
  NotFoundPage,
  ProfilePage,
  RegisterPage,
} from './lazyPages'
import { RouteErrorPage } from './pages/RouteErrorPage'

const standalone = (page: ReactNode) => <Suspense fallback={<PageFallback />}>{page}</Suspense>

export const router = createBrowserRouter(
  [
    {
      errorElement: <RouteErrorPage />,
      children: [
        {
          element: <AuthLayout />,
          children: [
            { path: '/login', element: <LoginPage /> },
            { path: '/register', element: <RegisterPage /> },
          ],
        },
        {
          element: <ProtectedRoute />,
          children: [
            {
              element: <AppLayout />,
              children: [
                { index: true, path: '/', element: <HomePage /> },
                {
                  element: <RequireRole role="CUSTOMER" />,
                  children: [{ path: '/profile', element: <ProfilePage /> }],
                },
                {
                  element: <RequireRole role="ADMIN" />,
                  children: [
                    { path: '/customers', element: <CustomerListPage /> },
                    { path: '/customers/:id', element: <CustomerDetailPage /> },
                  ],
                },
              ],
            },
          ],
        },
        { path: '/ui', element: standalone(<DesignSystemPage />) },
        { path: '*', element: standalone(<NotFoundPage />) },
      ],
    },
  ],
  { basename: import.meta.env.BASE_URL.replace(/\/$/, '') || '/' },
)
