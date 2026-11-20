import { Toaster } from 'sonner'
import { DesignSystemPage } from '@/features/design-system'

export function App() {
  return (
    <>
      <DesignSystemPage />
      <Toaster position="top-right" closeButton />
    </>
  )
}
