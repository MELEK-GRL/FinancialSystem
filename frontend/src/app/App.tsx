import { Toaster } from 'sonner'
import { Heading } from '@/shared/ui'

export function App() {
  return (
    <>
      <main className="flex min-h-screen items-center justify-center">
        <Heading>Finansal İşlem Sistemi</Heading>
      </main>
      <Toaster position="top-right" closeButton />
    </>
  )
}
