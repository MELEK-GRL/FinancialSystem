import type { FocusEvent, KeyboardEvent } from 'react'

const fieldSelector =
  'input:not([type=hidden]):not([disabled]), select:not([disabled]), textarea:not([disabled])'

export function useFormKeyboard() {
  const onKeyDown = (event: KeyboardEvent<HTMLFormElement>) => {
    const target = event.target as HTMLElement
    if (event.key !== 'Enter' || !(target instanceof HTMLInputElement)) return

    const fields = [...event.currentTarget.querySelectorAll<HTMLElement>(fieldSelector)]
    const next = fields[fields.indexOf(target) + 1]
    if (!next) return

    event.preventDefault()
    next.focus()
  }

  const onFocus = (event: FocusEvent<HTMLFormElement>) => {
    if (!window.matchMedia('(pointer: coarse)').matches) return
    const target = event.target
    window.setTimeout(() => target.scrollIntoView({ block: 'center', behavior: 'smooth' }), 300)
  }

  return { onKeyDown, onFocus }
}
