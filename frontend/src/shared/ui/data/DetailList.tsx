import type { ReactNode } from 'react'

export type DetailItem = { label: string; value: ReactNode }

export type DetailListProps = { items: DetailItem[] }

export function DetailList({ items }: DetailListProps) {
  return (
    <dl className="grid gap-x-6 gap-y-4 sm:grid-cols-2">
      {items.map((item) => (
        <div key={item.label} className="flex flex-col gap-1">
          <dt className="text-xs font-medium uppercase tracking-wide text-neutral-500">
            {item.label}
          </dt>
          <dd className="text-sm text-neutral-900">{item.value}</dd>
        </div>
      ))}
    </dl>
  )
}
