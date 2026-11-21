import { skipToken } from '@reduxjs/toolkit/query'
import { useTranslation } from 'react-i18next'
import { CenterModal, DateText, DetailList, type DetailItem } from '@/shared/ui'
import { transactionLabelKey } from '../lib/labels'
import { useGetTransactionQuery } from '../transactionsApi'
import type { AccountTransaction } from '../types'
import { TransactionAmount } from './TransactionAmount'
import { TransactionStatusBadge } from './TransactionStatusBadge'

export type TransactionDetailModalProps = {
  transaction: AccountTransaction | null
  onClose: () => void
}

export function TransactionDetailModal({ transaction, onClose }: TransactionDetailModalProps) {
  const { t, i18n } = useTranslation()
  const { data } = useGetTransactionQuery(transaction?.referenceNo ?? skipToken)
  const current =
    transaction && data?.referenceNo === transaction.referenceNo
      ? {
          ...transaction,
          status: data.status,
          failureReason: data.failureReason,
          completedAt: data.completedAt,
        }
      : transaction

  const items: DetailItem[] = current
    ? [
        {
          label: t('history.referenceNo'),
          value: <span className="font-mono">{current.referenceNo}</span>,
        },
        { label: t('history.typeLabel'), value: t(transactionLabelKey(current)) },
        {
          label: t('history.statusLabel'),
          value: <TransactionStatusBadge status={current.status} />,
        },
        { label: t('history.createdAt'), value: <DateText value={current.createdAt} /> },
      ]
    : []

  if (current?.completedAt) {
    items.push({
      label: t('history.completedAt'),
      value: <DateText value={current.completedAt} />,
    })
  }
  if (current?.counterpartyAccountNo) {
    items.push({
      label: t(current.direction === 'In' ? 'history.sender' : 'history.recipient'),
      value: <span className="font-mono">{current.counterpartyAccountNo}</span>,
    })
  }
  if (current?.description) {
    items.push({ label: t('history.description'), value: current.description })
  }
  if (current?.failureReason) {
    const key = `errors.${current.failureReason}`
    items.push({
      label: t('history.failureReason'),
      value: (
        <span className="text-danger">
          {i18n.exists(key) ? t(key as never) : current.failureReason}
        </span>
      ),
    })
  }

  return (
    <CenterModal
      open={transaction !== null}
      title={t('history.detailTitle')}
      closeLabel={t('common.close')}
      onClose={onClose}
    >
      {current ? (
        <div className="flex flex-col gap-5">
          <div className="flex flex-col items-center gap-1 rounded-lg bg-neutral-50 p-5">
            <TransactionAmount transaction={current} size="lg" />
          </div>
          <DetailList items={items} />
        </div>
      ) : null}
    </CenterModal>
  )
}
