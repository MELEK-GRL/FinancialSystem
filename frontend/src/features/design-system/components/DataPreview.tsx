import { useCallback, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { statusColors } from '@/shared/theme/colors'
import {
  Badge,
  DateText,
  MoneyText,
  Pagination,
  SearchInput,
  Table,
  Text,
  type TableColumn,
} from '@/shared/ui'
import { sampleTransactions, type SampleTransaction } from '../sampleData'

const statusTones = {
  Success: 'success',
  Failed: 'danger',
  Pending: 'warning',
} as const satisfies Record<keyof typeof statusColors, string>

export function SearchPreview() {
  const { t } = useTranslation()
  const [search, setSearch] = useState('')
  const handleSearch = useCallback((value: string) => setSearch(value), [])

  return (
    <div className="flex max-w-md flex-col gap-2">
      <SearchInput
        label={t('designSystem.search.label')}
        placeholder={t('designSystem.search.placeholder')}
        clearLabel={t('form.clear')}
        onSearch={handleSearch}
      />
      <Text variant="muted">
        {search
          ? t('designSystem.search.result', { value: search })
          : t('designSystem.search.empty')}
      </Text>
    </div>
  )
}

export function MoneyPreview() {
  const { t } = useTranslation()

  return (
    <div className="flex flex-wrap items-end gap-8">
      <div className="flex flex-col gap-1">
        <Text variant="muted">{t('designSystem.money.balance')}</Text>
        <MoneyText amount={125430.75} size="lg" />
      </div>
      <div className="flex flex-col gap-1">
        <Text variant="muted">{t('designSystem.money.incoming')}</Text>
        <MoneyText amount={500} direction="in" />
      </div>
      <div className="flex flex-col gap-1">
        <Text variant="muted">{t('designSystem.money.outgoing')}</Text>
        <MoneyText amount={1250.5} direction="out" />
      </div>
      <div className="flex flex-col gap-1">
        <Text variant="muted">{t('designSystem.money.date')}</Text>
        <DateText value="2026-11-20T21:30:00Z" />
      </div>
    </div>
  )
}

export function TablePreview() {
  const { t } = useTranslation()
  const [page, setPage] = useState(1)

  const columns: TableColumn<SampleTransaction>[] = [
    {
      key: 'reference',
      header: t('designSystem.table.reference'),
      render: (row) => <span className="font-mono text-xs">{row.referenceNo}</span>,
    },
    {
      key: 'type',
      header: t('designSystem.table.type'),
      render: (row) => t(`designSystem.types.${row.type}`),
    },
    {
      key: 'status',
      header: t('designSystem.table.status'),
      render: (row) => (
        <Badge tone={statusTones[row.status]}>{t(`designSystem.status.${row.status}`)}</Badge>
      ),
    },
    {
      key: 'date',
      header: t('designSystem.table.date'),
      render: (row) => <DateText value={row.createdAt} />,
    },
    {
      key: 'amount',
      header: t('designSystem.table.amount'),
      align: 'right',
      render: (row) => <MoneyText amount={row.amount} direction={row.direction} />,
    },
  ]

  return (
    <div>
      <Table columns={columns} rows={sampleTransactions} rowKey={(row) => row.id} />
      <Pagination
        page={page}
        totalPages={3}
        summary={t('common.pageSummary', { page, total: 3 })}
        previousLabel={t('common.previous')}
        nextLabel={t('common.next')}
        onPageChange={setPage}
      />
    </div>
  )
}
