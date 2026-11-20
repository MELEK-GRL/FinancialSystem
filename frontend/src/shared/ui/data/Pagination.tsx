import { ChevronLeft, ChevronRight } from 'lucide-react'
import { Button } from '../actions/Button'
import { Text } from '../typography/Text'

export type PaginationProps = {
  page: number
  totalPages: number
  summary: string
  previousLabel: string
  nextLabel: string
  onPageChange: (page: number) => void
}

export function Pagination({
  page,
  totalPages,
  summary,
  previousLabel,
  nextLabel,
  onPageChange,
}: PaginationProps) {
  if (totalPages <= 1) return null

  return (
    <nav className="flex items-center justify-between gap-4 px-4 py-3" aria-label={summary}>
      <Text variant="muted">{summary}</Text>
      <div className="flex gap-2">
        <Button
          variant="secondary"
          size="sm"
          icon={<ChevronLeft className="size-4" />}
          disabled={page <= 1}
          onClick={() => onPageChange(page - 1)}
        >
          {previousLabel}
        </Button>
        <Button
          variant="secondary"
          size="sm"
          disabled={page >= totalPages}
          onClick={() => onPageChange(page + 1)}
        >
          {nextLabel}
          <ChevronRight className="size-4" aria-hidden="true" />
        </Button>
      </div>
    </nav>
  )
}
