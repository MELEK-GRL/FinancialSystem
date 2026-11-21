import { useTranslation } from 'react-i18next'
import { DateText, Table, type TableColumn } from '@/shared/ui'
import type { CustomerSummary } from '../types'
import { CustomerStatusBadge } from './CustomerStatusBadge'

export type CustomerTableProps = {
  customers: CustomerSummary[]
  onSelect: (customer: CustomerSummary) => void
}

export function CustomerTable({ customers, onSelect }: CustomerTableProps) {
  const { t } = useTranslation()

  const columns: TableColumn<CustomerSummary>[] = [
    {
      key: 'customerNo',
      header: t('customers.customerNo'),
      render: (c) => <span className="font-mono text-xs">{c.customerNo}</span>,
    },
    { key: 'name', header: t('customers.fullName'), render: (c) => `${c.firstName} ${c.lastName}` },
    { key: 'email', header: t('customers.email'), render: (c) => c.email },
    {
      key: 'status',
      header: t('customers.status'),
      render: (c) => <CustomerStatusBadge status={c.status} />,
    },
    {
      key: 'createdAt',
      header: t('customers.createdAt'),
      render: (c) => <DateText value={c.createdAt} withTime={false} />,
    },
  ]

  return (
    <Table
      columns={columns}
      rows={customers}
      rowKey={(c) => c.id}
      onRowClick={onSelect}
      renderMobileRow={(c) => (
        <div className="flex items-start justify-between gap-3">
          <div className="flex min-w-0 flex-col gap-0.5">
            <span className="font-medium text-neutral-900">
              {c.firstName} {c.lastName}
            </span>
            <span className="truncate text-sm text-neutral-500">{c.email}</span>
            <span className="font-mono text-xs text-neutral-500">{c.customerNo}</span>
          </div>
          <CustomerStatusBadge status={c.status} />
        </div>
      )}
    />
  )
}
