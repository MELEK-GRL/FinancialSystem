import { RouterProvider } from 'react-router/dom'
import { PopupHost } from './PopupHost'
import { router } from './router'

export function App() {
  return (
    <>
      <RouterProvider router={router} />
      <PopupHost />
    </>
  )
}
