import clsx from 'clsx'
import { ArrowDownLeft, ArrowUpRight } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { DateText, Table, Text, type TableColumn } from '@/shared/ui'
import { transactionLabelKey } from '../lib/labels'
import type { AccountTransaction } from '../types'
import { TransactionAmount } from './TransactionAmount'
import { TransactionStatusBadge } from './TransactionStatusBadge'

export type TransactionHistoryTableProps = {
  transactions: AccountTransaction[]
  onSelect: (transaction: AccountTransaction) => void
}

function DirectionIcon({ direction }: Pick<AccountTransaction, 'direction'>) {
  const Icon = direction === 'In' ? ArrowDownLeft : ArrowUpRight
  return (
    <span
      className={clsx(
        'flex size-8 shrink-0 items-center justify-center rounded-full',
        direction === 'In' ? 'bg-success-soft text-success' : 'bg-neutral-100 text-neutral-600',
      )}
      aria-hidden="true"
    >
      <Icon className="size-4" />
    </span>
  )
}

export function TransactionHistoryTable({ transactions, onSelect }: TransactionHistoryTableProps) {
  const { t } = useTranslation()

  const columns: TableColumn<AccountTransaction>[] = [
    {
      key: 'createdAt',
      header: t('history.date'),
      render: (tx) => <DateText value={tx.createdAt} />,
    },
    {
      key: 'type',
      header: t('history.typeLabel'),
      render: (tx) => (
        <span className="flex items-center gap-2">
          <DirectionIcon direction={tx.direction} />
          {t(transactionLabelKey(tx))}
        </span>
      ),
    },
    {
      key: 'counterparty',
      header: t('history.counterparty'),
      render: (tx) =>
        tx.counterpartyAccountNo ? (
          <span className="font-mono text-xs">{tx.counterpartyAccountNo}</span>
        ) : (
          <span className="text-neutral-400">—</span>
        ),
    },
    {
      key: 'description',
      header: t('history.description'),
      render: (tx) => (
        <span className="line-clamp-1 max-w-56 text-neutral-600">{tx.description ?? '—'}</span>
      ),
    },
    {
      key: 'status',
      header: t('history.statusLabel'),
      render: (tx) => <TransactionStatusBadge status={tx.status} />,
    },
    {
      key: 'amount',
      header: t('history.amount'),
      align: 'right',
      render: (tx) => <TransactionAmount transaction={tx} />,
    },
  ]

  return (
    <Table
      columns={columns}
      rows={transactions}
      rowKey={(tx) => tx.id}
      onRowClick={onSelect}
      renderMobileRow={(tx) => (
        <div className="flex items-center gap-3">
          <DirectionIcon direction={tx.direction} />
          <div className="flex min-w-0 flex-1 flex-col">
            <span className="truncate text-sm font-medium text-neutral-900">
              {t(transactionLabelKey(tx))}
            </span>
            <Text variant="small" as="span">
              <DateText value={tx.createdAt} />
            </Text>
          </div>
          <div className="flex shrink-0 flex-col items-end gap-1">
            <TransactionAmount transaction={tx} />
            {tx.status === 'Success' ? null : <TransactionStatusBadge status={tx.status} />}
          </div>
        </div>
      )}
    />
  )
}
