import clsx from 'clsx'
import type { ReactNode } from 'react'

export type TableColumn<T> = {
  key: string
  header: string
  render: (row: T) => ReactNode
  align?: 'left' | 'right'
}

export type TableProps<T> = {
  columns: TableColumn<T>[]
  rows: T[]
  rowKey: (row: T) => string
  onRowClick?: (row: T) => void
  renderMobileRow?: (row: T) => ReactNode
}

export function Table<T>({ columns, rows, rowKey, onRowClick, renderMobileRow }: TableProps<T>) {
  return (
    <>
      {renderMobileRow ? (
        <ul className="divide-y divide-neutral-100 md:hidden">
          {rows.map((row) => (
            <li key={rowKey(row)}>
              {onRowClick ? (
                <button
                  type="button"
                  onClick={() => onRowClick(row)}
                  className="w-full px-4 py-3 text-left transition-colors duration-150 hover:bg-neutral-50 focus-visible:bg-neutral-50 focus-visible:outline-none"
                >
                  {renderMobileRow(row)}
                </button>
              ) : (
                <div className="px-4 py-3">{renderMobileRow(row)}</div>
              )}
            </li>
          ))}
        </ul>
      ) : null}
      <div className={clsx('overflow-x-auto', renderMobileRow && 'hidden md:block')}>
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-neutral-200">
              {columns.map((column) => (
                <th
                  key={column.key}
                  scope="col"
                  className={clsx(
                    'px-4 py-3 text-xs font-medium uppercase tracking-wide text-neutral-500',
                    column.align === 'right' ? 'text-right' : 'text-left',
                  )}
                >
                  {column.header}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {rows.map((row) => (
              <tr
                key={rowKey(row)}
                onClick={onRowClick ? () => onRowClick(row) : undefined}
                className={clsx(
                  'border-b border-neutral-100 last:border-0 transition-colors duration-150 hover:bg-neutral-50',
                  onRowClick && 'cursor-pointer',
                )}
              >
                {columns.map((column) => (
                  <td
                    key={column.key}
                    className={clsx(
                      'px-4 py-3 text-neutral-800',
                      column.align === 'right' && 'text-right',
                    )}
                  >
                    {column.render(row)}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </>
  )
}
