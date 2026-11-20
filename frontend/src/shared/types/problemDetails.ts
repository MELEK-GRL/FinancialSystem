export type ProblemDetails = {
  status: number
  title: string
  errorCode: string
  traceId?: string
  errors?: Record<string, string[]>
}

export function isProblemDetails(value: unknown): value is ProblemDetails {
  return typeof value === 'object' && value !== null && 'errorCode' in value && 'title' in value
}
