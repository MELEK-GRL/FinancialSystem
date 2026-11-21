import { Suspense, type ReactNode } from 'react'
import { createBrowserRouter } from 'react-router'
import { ProtectedRoute } from '@/features/auth'
import { AppLayout, AuthLayout, PageFallback } from '@/layouts'
import { DesignSystemPage, HomePage, LoginPage, NotFoundPage, RegisterPage } from './lazyPages'
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
              children: [{ index: true, path: '/', element: <HomePage /> }],
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
