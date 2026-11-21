export type PagedResult<T> = {
  items: T[]
  page: number
  size: number
  totalCount: number
  totalPages: number
}
