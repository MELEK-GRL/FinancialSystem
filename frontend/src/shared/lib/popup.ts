import type { ReactNode } from 'react'

export type PopupVariant = 'success' | 'info' | 'danger'

export type PopupContent = {
  title: string
  description?: ReactNode
  actionLabel?: string
}

export type PopupState = (PopupContent & { variant: PopupVariant }) | null

let state: PopupState = null
const listeners = new Set<() => void>()

function show(next: PopupState) {
  state = next
  listeners.forEach((listener) => listener())
}

export const popup = {
  success: (content: PopupContent) => show({ ...content, variant: 'success' }),
  info: (content: PopupContent) => show({ ...content, variant: 'info' }),
  danger: (content: PopupContent) => show({ ...content, variant: 'danger' }),
  close: () => show(null),
  subscribe: (listener: () => void) => {
    listeners.add(listener)
    return () => {
      listeners.delete(listener)
    }
  },
  getSnapshot: () => state,
}
