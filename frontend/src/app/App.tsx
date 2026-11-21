import { RouterProvider } from 'react-router/dom'
import { Toaster } from 'sonner'
import { PopupHost } from './PopupHost'
import { router } from './router'

export function App() {
  return (
    <>
      <RouterProvider router={router} />
      <PopupHost />
      <Toaster position="top-right" closeButton />
    </>
  )
}
