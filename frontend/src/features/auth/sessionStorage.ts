import type { Session } from './types'

const STORAGE_KEY = 'session'

export function isSessionActive(session: Session | null, now = Date.now()) {
  return session !== null && new Date(session.expiresAt).getTime() > now
}

export function loadSession(): Session | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    const session = raw ? (JSON.parse(raw) as Session) : null
    return isSessionActive(session) ? session : null
  } catch {
    return null
  }
}

export function saveSession(session: Session | null) {
  try {
    if (session) localStorage.setItem(STORAGE_KEY, JSON.stringify(session))
    else localStorage.removeItem(STORAGE_KEY)
  } catch {
    return
  }
}
