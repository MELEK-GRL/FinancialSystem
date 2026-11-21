import { ArrowRight } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useLocation } from 'react-router'
import { AccountStatusBadge } from '@/features/accounts'
import { DateText, Link, Table, Text, type TableColumn } from '@/shared/ui'
import type { AuditLog } from '../types'

export type AccountStatusHistoryTableProps = { logs: AuditLog[] }

function StatusChange({ log }: { log: AuditLog }) {
  return (
    <span className="inline-flex items-center gap-1.5">
      <AccountStatusBadge status={log.oldStatus} />
      <ArrowRight className="size-3.5 text-neutral-400" aria-hidden="true" />
      <AccountStatusBadge status={log.newStatus} />
    </span>
  )
}

export function AccountStatusHistoryTable({ logs }: AccountStatusHistoryTableProps) {
  const { t } = useTranslation()
  const location = useLocation()

  const accountLink = (log: AuditLog) => (
    <Link
      to={`/accounts/${log.accountId}/transactions`}
      state={{ from: location.pathname + location.search }}
      className="font-mono text-xs"
    >
      {log.accountNo}
    </Link>
  )

  const columns: TableColumn<AuditLog>[] = [
    {
      key: 'createdAt',
      header: t('audit.date'),
      render: (log) => <DateText value={log.createdAt} />,
    },
    { key: 'account', header: t('audit.account'), render: accountLink },
    { key: 'change', header: t('audit.change'), render: (log) => <StatusChange log={log} /> },
    {
      key: 'reason',
      header: t('audit.reason'),
      render: (log) => <span className="line-clamp-2 max-w-72 text-neutral-700">{log.reason}</span>,
    },
    {
      key: 'performedBy',
      header: t('audit.performedBy'),
      render: (log) => <span className="text-neutral-600">{log.performedByEmail}</span>,
    },
  ]

  return (
    <Table
      columns={columns}
      rows={logs}
      rowKey={(log) => log.id}
      renderMobileRow={(log) => (
        <div className="flex flex-col gap-2">
          <div className="flex items-center justify-between gap-3">
            {accountLink(log)}
            <StatusChange log={log} />
          </div>
          <Text variant="body">{log.reason}</Text>
          <Text variant="small">
            <DateText value={log.createdAt} /> · {log.performedByEmail}
          </Text>
        </div>
      )}
    />
  )
}
